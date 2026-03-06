using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockPoweredTrap : BlockPowered
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public new static string PropDamage = "Damage";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDamageReceived = "Damage_received";

	[PublicizedFrom(EAccessModifier.Protected)]
	public int damage;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int damageReceived;

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey(PropDamage))
		{
			int.TryParse(base.Properties.Values[PropDamage], out damage);
		}
		else
		{
			damage = 0;
		}
		if (base.Properties.Values.ContainsKey(PropDamageReceived))
		{
			int.TryParse(base.Properties.Values[PropDamageReceived], out damageReceived);
		}
		else
		{
			damageReceived = 0;
		}
	}

	public override void GetCollisionAABB(BlockValue _blockValue, int _x, int _y, int _z, float _distortedY, List<Bounds> _result)
	{
		base.GetCollisionAABB(_blockValue, _x, _y, _z, _distortedY, _result);
		Vector3 vector = new Vector3(0.05f, 0.05f, 0.05f);
		for (int i = 0; i < _result.Count; i++)
		{
			Bounds value = _result[i];
			value.SetMinMax(value.min - vector, value.max + vector);
			_result[i] = value;
		}
	}

	public override IList<Bounds> GetClipBoundsList(BlockValue _blockValue, Vector3 _blockPos)
	{
		Block.staticList_IntersectRayWithBlockList.Clear();
		GetCollisionAABB(_blockValue, (int)_blockPos.x, (int)_blockPos.y, (int)_blockPos.z, 0f, Block.staticList_IntersectRayWithBlockList);
		return Block.staticList_IntersectRayWithBlockList;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateTrapState(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bSwitchTrap = false)
	{
		ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
		if (chunkCluster == null)
		{
			return false;
		}
		IChunk chunkSync = chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
		if (chunkSync == null)
		{
			return false;
		}
		BlockEntityData blockEntity = chunkSync.GetBlockEntity(_blockPos);
		if (blockEntity == null || !blockEntity.bHasTransform)
		{
			return false;
		}
		bool flag = (_blockValue.meta & 2) != 0;
		if (_bSwitchTrap)
		{
			flag = !flag;
			_blockValue.meta = (byte)((_blockValue.meta & -3) | (flag ? 2 : 0));
			_world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
		}
		ActivateTrap(blockEntity, flag);
		TileEntityPoweredMeleeTrap tileEntityPoweredMeleeTrap = (TileEntityPoweredMeleeTrap)_world.GetTileEntity(_cIdx, _blockPos);
		if (tileEntityPoweredMeleeTrap != null)
		{
			SpinningBladeTrapController component = blockEntity.transform.gameObject.GetComponent<SpinningBladeTrapController>();
			if (component != null)
			{
				component.BladeController.OwnerTE = tileEntityPoweredMeleeTrap;
			}
		}
		return true;
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		if (!_newBlockValue.ischild)
		{
			updateTrapState(_world, _clrIdx, _blockPos, _newBlockValue);
			((World)_world).ChunkClusters[_clrIdx].GetBlockEntity(_blockPos);
		}
	}

	public static bool IsOn(byte _metadata)
	{
		return (_metadata & 2) != 0;
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		if (_world.GetTileEntity(_result.clrIdx, _result.blockPos) is TileEntityPoweredMeleeTrap tileEntityPoweredMeleeTrap && _ea != null && _ea.entityType == EntityType.Player)
		{
			tileEntityPoweredMeleeTrap.SetOwner(PlatformManager.InternalLocalUserIdentifier);
		}
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		SpinningBladeTrapController component = _ebcd.transform.gameObject.GetComponent<SpinningBladeTrapController>();
		if (component != null)
		{
			component.Cleanup();
		}
		ActivateTrap(_ebcd, isOn: false);
		TileEntityPoweredMeleeTrap tileEntityPoweredMeleeTrap = (TileEntityPoweredMeleeTrap)_world.GetTileEntity(_cIdx, _blockPos);
		if (tileEntityPoweredMeleeTrap == null)
		{
			ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
			if (chunkCluster == null)
			{
				return;
			}
			Chunk chunk = (Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos);
			if (chunk == null)
			{
				return;
			}
			tileEntityPoweredMeleeTrap = (TileEntityPoweredMeleeTrap)CreateTileEntity(chunk);
			tileEntityPoweredMeleeTrap.localChunkPos = World.toBlock(_blockPos);
			tileEntityPoweredMeleeTrap.InitializePowerData();
			chunk.AddTileEntity(tileEntityPoweredMeleeTrap);
		}
		if (tileEntityPoweredMeleeTrap != null)
		{
			bool flag = (_blockValue.meta & 2) != 0;
			_blockValue.meta = (byte)((_blockValue.meta & -3) | (flag ? 2 : 0));
			updateTrapState(_world, _cIdx, _blockPos, _blockValue);
			if (component != null)
			{
				component.BladeController.OwnerTE = tileEntityPoweredMeleeTrap;
			}
		}
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!_blockValue.ischild)
		{
			if (!(world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityPoweredMeleeTrap))
			{
				TileEntityPowered tileEntityPowered = CreateTileEntity(_chunk);
				tileEntityPowered.localChunkPos = World.toBlock(_blockPos);
				tileEntityPowered.InitializePowerData();
				_chunk.AddTileEntity(tileEntityPowered);
			}
			BlockEntityData blockEntityData = new BlockEntityData(_blockValue, _blockPos);
			blockEntityData.bNeedsTemperature = true;
			_chunk.AddEntityBlockStub(blockEntityData);
		}
	}

	public override bool ActivateBlock(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool isOn, bool isPowered)
	{
		if (_blockValue.ischild)
		{
			return false;
		}
		_blockValue.meta = (byte)((_blockValue.meta & -3) | (isOn ? 2 : 0));
		_world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
		updateTrapState(_world, _cIdx, _blockPos, _blockValue);
		return true;
	}

	public virtual bool ActivateTrap(BlockEntityData blockEntity, bool isOn)
	{
		return false;
	}

	public override TileEntityPowered CreateTileEntity(Chunk chunk)
	{
		return new TileEntityPoweredMeleeTrap(chunk);
	}
}
