using System.Collections;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class BlockRanged : BlockPowered
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string playSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public float soundDelay = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float maxDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] allowedAmmoTypes;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityDamage = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public int blockDamage = 10;

	public string AmmoItemName;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("options", "tool", _enabled: true),
		new BlockActivationCommand("take", "hand", _enabled: false)
	};

	public BlockRanged()
	{
		HasTileEntity = true;
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool enabled = _world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		cmds[0].enabled = enabled;
		cmds[1].enabled = flag && TakeDelay > 0f;
		return cmds;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!_world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer()))
		{
			return "";
		}
		TileEntityPoweredRangedTrap obj = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityPoweredRangedTrap;
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		if (obj != null)
		{
			string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
			string arg2 = _blockValue.Block.GetLocalizedBlockName();
			return string.Format(Localization.Get("vendingMachineActivate"), arg, arg2);
		}
		return "";
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return OnBlockActivated(_commandName, _world, _cIdx, parentPos, block, _player);
		}
		_player.CancelInventoryActions();
		if (!(_world.GetTileEntity(_cIdx, _blockPos) is TileEntityPoweredRangedTrap tileEntityPoweredRangedTrap))
		{
			return false;
		}
		if (!(_commandName == "options"))
		{
			if (_commandName == "take")
			{
				TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
				return true;
			}
			return false;
		}
		_player.AimingGun = false;
		Vector3i worldPos = tileEntityPoweredRangedTrap.ToWorldPos();
		GameManager.Instance.StartCoroutine(lockLater(_world, _cIdx, worldPos, tileEntityPoweredRangedTrap.entityId, _player));
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator lockLater(WorldBase _world, int _cIdx, Vector3i _worldPos, int _tileId, EntityPlayerLocal _player)
	{
		while (_player.IsReloading())
		{
			yield return null;
		}
		_world.GetGameManager().TELockServer(_cIdx, _worldPos, _tileId, _player.entityId);
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		if (_world.GetTileEntity(_result.clrIdx, _result.blockPos) is TileEntityPoweredRangedTrap tileEntityPoweredRangedTrap && _ea != null && _ea.entityType == EntityType.Player)
		{
			tileEntityPoweredRangedTrap.SetOwner(PlatformManager.InternalLocalUserIdentifier);
		}
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		AutoTurretController component = _ebcd.transform.gameObject.GetComponent<AutoTurretController>();
		if (component != null)
		{
			component.FireController.BlockPosition = _ebcd.pos.ToVector3();
			component.Init(base.Properties);
		}
		TileEntityPowered tileEntityPowered = (TileEntityPowered)_world.GetTileEntity(_cIdx, _blockPos);
		if (tileEntityPowered == null)
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
			tileEntityPowered = CreateTileEntity(chunk);
			tileEntityPowered.localChunkPos = World.toBlock(_blockPos);
			tileEntityPowered.InitializePowerData();
			chunk.AddTileEntity(tileEntityPowered);
		}
		tileEntityPowered.BlockTransform = _ebcd.transform;
		tileEntityPowered.MarkWireDirty();
		if (tileEntityPowered.GetParent().y != -9999 && _world.GetTileEntity(_cIdx, tileEntityPowered.GetParent()) is IPowered powered)
		{
			powered.DrawWires();
		}
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("AmmoItem"))
		{
			AmmoItemName = base.Properties.Values["AmmoItem"];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateState(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue)
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
		TileEntityPoweredRangedTrap tileEntityPoweredRangedTrap = (TileEntityPoweredRangedTrap)_world.GetTileEntity(_cIdx, _blockPos);
		if (flag)
		{
			if (tileEntityPoweredRangedTrap != null)
			{
				PowerManager.Instance.ClientUpdateList.Add(tileEntityPoweredRangedTrap);
			}
		}
		else if (tileEntityPoweredRangedTrap != null)
		{
			PowerManager.Instance.ClientUpdateList.Remove(tileEntityPoweredRangedTrap);
		}
		AutoTurretController component = blockEntity.transform.gameObject.GetComponent<AutoTurretController>();
		if (component == null)
		{
			return false;
		}
		component.TileEntity = tileEntityPoweredRangedTrap;
		component.IsOn = flag;
		return true;
	}

	public override bool ActivateBlock(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool isOn, bool isPowered)
	{
		byte b = (byte)((_blockValue.meta & -3) | (isOn ? 2 : 0));
		if (_blockValue.meta != b)
		{
			_blockValue.meta = b;
			_world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
		}
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
		AutoTurretController component = blockEntity.transform.gameObject.GetComponent<AutoTurretController>();
		if (component == null)
		{
			return false;
		}
		TileEntityPoweredRangedTrap tileEntity = (TileEntityPoweredRangedTrap)_world.GetTileEntity(_cIdx, _blockPos);
		component.TileEntity = tileEntity;
		component.IsOn = isOn;
		return isOn;
	}

	public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		bool num = (_blockValue.meta & 2) != 0;
		TileEntityPowered tileEntityPowered = (TileEntityPowered)_world.GetTileEntity(_clrIdx, _blockPos);
		if (num)
		{
			if (tileEntityPowered != null && tileEntityPowered is TileEntityPoweredBlock)
			{
				PowerManager.Instance.ClientUpdateList.Add(tileEntityPowered as TileEntityPoweredBlock);
			}
		}
		else if (tileEntityPowered != null && tileEntityPowered is TileEntityPoweredBlock)
		{
			PowerManager.Instance.ClientUpdateList.Remove(tileEntityPowered as TileEntityPoweredBlock);
		}
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		updateState(_world, _clrIdx, _blockPos, _newBlockValue);
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
		if (!_blockValue.ischild)
		{
			if (!(world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityPoweredRangedTrap))
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

	public override TileEntityPowered CreateTileEntity(Chunk chunk)
	{
		return new TileEntityPoweredRangedTrap(chunk)
		{
			PowerItemType = PowerItem.PowerItemTypes.RangedTrap
		};
	}
}
