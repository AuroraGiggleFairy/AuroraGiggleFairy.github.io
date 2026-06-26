using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockModelTree : BlockPlantGrowing
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFallOver;

	public BlockModelTree()
	{
		fertileLevel = 1;
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("FallOver"))
		{
			bFallOver = StringParsers.ParseBool(base.Properties.Values["FallOver"]);
		}
		else
		{
			bFallOver = true;
		}
		IsTerrainDecoration = true;
		CanDecorateOnSlopes = true;
		CanPlayersSpawnOn = false;
		CanMobsSpawnOn = false;
	}

	public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		if (!_blockValue.ischild)
		{
			return base.UpdateTick(_world, _clrIdx, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);
		}
		return false;
	}

	public override void OnNeighborBlockChange(WorldBase _world, int _clrIdx, Vector3i _myBlockPos, BlockValue _myBlockValue, Vector3i _blockPosThatChanged, BlockValue _newNeighborBlockValue, BlockValue _oldNeighborBlockValue)
	{
		if (!_myBlockValue.ischild)
		{
			base.OnNeighborBlockChange(_world, _clrIdx, _myBlockPos, _myBlockValue, _blockPosThatChanged, _newNeighborBlockValue, _oldNeighborBlockValue);
		}
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		if (!_newBlockValue.ischild)
		{
			base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		}
	}

	public override bool CheckPlantAlive(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (!isPlantGrowingRandom && !_world.IsRemote())
		{
			return base.CheckPlantAlive(_world, _clrIdx, _blockPos, _blockValue);
		}
		return true;
	}

	public override bool CanPlaceBlockAt(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
	{
		if (!base.CanPlaceBlockAt(_world, _clrIdx, _blockPos, _blockValue, _bOmitCollideCheck))
		{
			return false;
		}
		for (int i = _blockPos.x - 3; i <= _blockPos.x + 3; i++)
		{
			for (int j = _blockPos.z - 3; j <= _blockPos.z + 3; j++)
			{
				for (int k = _blockPos.y - 6; k <= _blockPos.y + 6; k++)
				{
					if (_world.GetBlock(_clrIdx, new Vector3i(i, k, j)).Block is BlockModelTree)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		removeAllTrunks(_world, _chunk, _blockPos, _blockValue, -1, _bDropItemsAndStartParticle: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeAllTrunks(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, int _entityid, bool _bDropItemsAndStartParticle)
	{
		if (_chunk == null)
		{
			_chunk = (Chunk)_world.GetChunkFromWorldPos(_blockPos);
			if (_chunk == null)
			{
				return;
			}
		}
		if (_bDropItemsAndStartParticle)
		{
			dropItems(_world, _blockPos, _blockValue, _entityid);
			float lightBrightness = _world.GetLightBrightness(_blockPos);
			SpawnDestroyParticleEffect(_world, _blockValue, _blockPos, lightBrightness, GetColorForSide(_blockValue, BlockFace.Top), _entityid);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void dropItems(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, int _entityId)
	{
		_blockValue.Block.DropItemsOnEvent(_world, _blockValue, EnumDropEvent.Destroy, 1f, World.blockToTransformPos(_blockPos), new Vector3(0.5f, 0f, 0.5f), 0f, _entityId, _bGetSameItemIfNoneFound: true);
	}

	public override DestroyedResult OnBlockDestroyedByExplosion(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _playerThatStartedExpl)
	{
		startToFall(_world, _clrIdx, _blockPos, _blockValue, -1);
		return DestroyedResult.Keep;
	}

	public override void OnBlockStartsToFall(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (!_blockValue.ischild)
		{
			if (OnBlockDestroyedBy(_world, 0, _blockPos, _blockValue, -1, _bUseHarvestTool: false) != DestroyedResult.Keep)
			{
				base.OnBlockStartsToFall(_world, _blockPos, _blockValue);
				return;
			}
			float lightBrightness = _world.GetLightBrightness(_blockPos);
			SpawnDestroyParticleEffect(_world, _blockValue, _blockPos, lightBrightness, GetColorForSide(_blockValue, BlockFace.Top), -1);
		}
	}

	public override void SpawnDestroyParticleEffect(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, float _lightValue, Color _color, int _entityIdThatCaused)
	{
		base.SpawnDestroyParticleEffect(_world, _blockValue, _blockPos, _lightValue, _color, _entityIdThatCaused);
		_world.GetGameManager().PlaySoundAtPositionServer(_blockPos.ToVector3(), "trunkbreak", AudioRolloffMode.Logarithmic, 100, -1);
	}

	public override bool ShowModelOnFall()
	{
		return false;
	}

	public override BlockValue OnBlockPlaced(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, GameRandom _rnd)
	{
		_blockValue.rotation = BiomeBlockDecoration.GetRandomRotation(_rnd.RandomFloat, 7);
		return _blockValue;
	}

	public override DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		if (!bFallOver)
		{
			return DestroyedResult.Downgrade;
		}
		if (!startToFall(_world, _clrIdx, _blockPos, _blockValue, _entityId))
		{
			return DestroyedResult.Downgrade;
		}
		return DestroyedResult.Keep;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool startToFall(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId)
	{
		Transform transform;
		if (!DecoManager.Instance.IsEnabled || !_blockValue.Block.IsDistantDecoration)
		{
			BlockEntityData blockEntity = ((Chunk)_world.GetChunkFromWorldPos(_blockPos)).GetBlockEntity(_blockPos);
			if (blockEntity == null || !blockEntity.bHasTransform)
			{
				return false;
			}
			transform = blockEntity.transform;
		}
		else
		{
			transform = DecoManager.Instance.GetDecorationTransform(_blockPos);
		}
		if (transform == null)
		{
			_world.SetBlockRPC(_blockPos, BlockValue.Air);
			return false;
		}
		_blockValue.damage = _blockValue.Block.MaxDamage;
		_world.SetBlocksRPC(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_blockPos, _blockValue, _updateLight: false, _bOnlyDamage: true)
		});
		Entity entity = _world.GetEntity(_entityId);
		EntityCreationData entityCreationData = new EntityCreationData();
		entityCreationData.entityClass = "fallingTree".GetHashCode();
		entityCreationData.blockPos = _blockPos;
		entityCreationData.fallTreeDir = ((entity != null) ? (transform.position + Origin.position - entity.GetPosition()) : new Vector3(_world.GetGameRandom().RandomFloat, 0f, _world.GetGameRandom().RandomFloat));
		entityCreationData.fallTreeDir.y = 0f;
		entityCreationData.fallTreeDir = entityCreationData.fallTreeDir.normalized;
		entityCreationData.pos = transform.position + Origin.position;
		entityCreationData.rot = transform.rotation.eulerAngles;
		entityCreationData.id = -1;
		_world.GetGameManager().RequestToSpawnEntityServer(entityCreationData);
		return true;
	}

	public override int OnBlockDamaged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _damagePoints, int _entityIdThatDamaged, ItemActionAttack.AttackHitInfo _attackHitInfo, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		if (!_world.IsRemote() && _blockValue.damage > _blockValue.Block.MaxDamage + 100)
		{
			_world.SetBlockRPC(_blockPos, BlockValue.Air);
		}
		return base.OnBlockDamaged(_world, _clrIdx, _blockPos, _blockValue, _damagePoints, _entityIdThatDamaged, _attackHitInfo, _bUseHarvestTool, _bBypassMaxDamage, _recDepth);
	}

	public override bool IsExplosionAffected()
	{
		return false;
	}
}
