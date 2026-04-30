using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockSpawnEntity : Block
{
	public string[] spawnClasses;

	public override void Init()
	{
		base.Init();
		if (!base.Properties.Values.ContainsKey("SpawnClass"))
		{
			throw new Exception($"Need 'SpawnClass' in block {GetBlockName()}");
		}
		spawnClasses = base.Properties.Values["SpawnClass"].Split(',');
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!GameManager.Instance.IsEditMode() && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && spawnClasses.Length != 0)
		{
			EntityCreationData entityCreationData = new EntityCreationData();
			entityCreationData.id = -1;
			string text = spawnClasses[_blockValue.meta % spawnClasses.Length];
			entityCreationData.entityClass = text.GetHashCode();
			entityCreationData.pos = _blockPos.ToVector3() + new Vector3(0.5f, 0.25f, 0.5f);
			entityCreationData.rot = new Vector3(0f, 90 * (_blockValue.rotation & 3), 0f);
			_chunk.AddEntityStub(entityCreationData);
			_world.GetWBT().AddScheduledBlockUpdate(0, _blockPos, blockID, 80uL);
		}
	}

	public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
		if (chunkCluster == null)
		{
			return;
		}
		Chunk chunk = (Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos);
		if (chunk != null)
		{
			BlockEntityData blockEntityData = new BlockEntityData(_blockValue, _blockPos);
			blockEntityData.bNeedsTemperature = true;
			chunk.AddEntityBlockStub(blockEntityData);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				_world.GetWBT().AddScheduledBlockUpdate(0, _blockPos, blockID, 300uL);
			}
		}
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		if (!_blockValue.ischild)
		{
			if (!_world.IsEditor())
			{
				_ebcd.transform.gameObject.SetActive(value: false);
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				_world.GetWBT().AddScheduledBlockUpdate(0, _blockPos, blockID, 300uL);
			}
		}
	}

	public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (GameManager.Instance.World.GetEntitiesInBounds(null, new Bounds(_blockPos.ToVector3(), Vector3.one * 2f)).Count == 0 && !GameManager.Instance.IsEditMode())
			{
				ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
				if (chunkCluster == null)
				{
					return false;
				}
				if ((Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos) == null)
				{
					return false;
				}
				if (spawnClasses.Length != 0)
				{
					string text = spawnClasses[_blockValue.meta % spawnClasses.Length];
					int et = -1;
					foreach (KeyValuePair<int, EntityClass> item in EntityClass.list.Dict)
					{
						if (item.Value.entityClassName == text)
						{
							et = item.Key;
						}
					}
					Vector3 transformPos = _blockPos.ToVector3() + new Vector3(0.5f, 0.25f, 0.5f);
					Vector3 rotation = new Vector3(0f, 90 * (_blockValue.rotation & 3), 0f);
					Entity entity = EntityFactory.CreateEntity(et, transformPos, rotation);
					entity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
					GameManager.Instance.World.SpawnEntityInWorld(entity);
					Log.Out("BlockSpawnEntity:: Spawn New Trader.");
				}
			}
			_world.GetWBT().AddScheduledBlockUpdate(0, _blockPos, blockID, 320uL);
		}
		return base.UpdateTick(_world, _clrIdx, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);
	}
}
