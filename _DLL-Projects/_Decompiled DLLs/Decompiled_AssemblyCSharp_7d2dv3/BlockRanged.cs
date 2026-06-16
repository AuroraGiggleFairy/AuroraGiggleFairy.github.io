using UnityEngine.Scripting;

[Preserve]
public class BlockRanged : BlockPowered
{
	public string AmmoItemName;

	[PublicizedFrom(EAccessModifier.Private)]
	public new readonly BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("options", "tool", _enabled: true),
		new BlockActivationCommand("take", "hand", _enabled: false)
	};

	public BlockRanged()
	{
		HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("AmmoItem"))
		{
			AmmoItemName = base.Properties.Values["AmmoItem"];
		}
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool enabled = _world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		cmds[0].enabled = enabled;
		cmds[1].enabled = flag && TakeDelay > 0f;
		return cmds;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!_world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer()))
		{
			return "";
		}
		TileEntityPoweredRangedTrap obj = _world.GetTileEntity(_blockPos) as TileEntityPoweredRangedTrap;
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		if (obj == null)
		{
			return "";
		}
		string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		string arg2 = _blockValue.Block.GetLocalizedBlockName();
		return string.Format(Localization.Get("vendingMachineActivate"), arg, arg2);
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return OnBlockActivated(_commandName, _world, parentPos, block, _player);
		}
		if (!(_world.GetTileEntity(_blockPos) is TileEntityPoweredRangedTrap target))
		{
			return false;
		}
		if (!(_commandName == "options"))
		{
			if (_commandName == "take")
			{
				takeItemWithTimer(_blockPos, _blockValue, _player, TakeDelay);
				return true;
			}
			return false;
		}
		_player.AimingGun = false;
		LockManager.Instance.LockRequestLocal(target, null, 0);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateState(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		ChunkCluster chunkCache = _world.ChunkCache;
		if (chunkCache == null)
		{
			return false;
		}
		IChunk chunkSync = chunkCache.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
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
		TileEntityPoweredRangedTrap tileEntityPoweredRangedTrap = (TileEntityPoweredRangedTrap)_world.GetTileEntity(_blockPos);
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

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _blockPos, _oldBlockValue, _newBlockValue);
		updateState(_world, _blockPos, _newBlockValue);
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
		AutoTurretController component = _ebcd.transform.gameObject.GetComponent<AutoTurretController>();
		if (component != null)
		{
			component.FireController.BlockPosition = _ebcd.pos.ToVector3();
			component.Init(base.Properties);
		}
		TileEntityPowered tileEntityPowered = _world.GetTileEntity(_blockPos) as TileEntityPowered;
		if (tileEntityPowered == null)
		{
			ChunkCluster chunkCache = _world.ChunkCache;
			if (chunkCache == null)
			{
				return;
			}
			Chunk chunk = (Chunk)chunkCache.GetChunkFromWorldPos(_blockPos);
			if (chunk == null)
			{
				return;
			}
			tileEntityPowered = CreateTileEntity(chunk);
			tileEntityPowered.SetDisableModifiedCheck(_b: true);
			tileEntityPowered.localChunkPos = World.toBlock(_blockPos);
			tileEntityPowered.InitializePowerData();
			chunk.AddTileEntity(tileEntityPowered);
			tileEntityPowered.SetDisableModifiedCheck(_b: false);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				tileEntityPowered.SetModified();
			}
		}
		tileEntityPowered.BlockTransform = _ebcd.transform;
		tileEntityPowered.MarkWireDirty();
		if (tileEntityPowered.GetParent().y != -9999 && _world.GetTileEntity(tileEntityPowered.GetParent()) is IPowered powered)
		{
			powered.DrawWires();
		}
	}

	public override bool ActivateBlock(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _isOn, bool _isPowered)
	{
		byte b = (byte)((_blockValue.meta & -3) | (_isOn ? 2 : 0));
		if (_blockValue.meta != b)
		{
			_blockValue.meta = b;
			_world.SetBlockRPC(_blockPos, _blockValue);
		}
		ChunkCluster chunkCache = _world.ChunkCache;
		if (chunkCache == null)
		{
			return false;
		}
		IChunk chunkSync = chunkCache.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
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
		TileEntityPoweredRangedTrap tileEntity = (TileEntityPoweredRangedTrap)_world.GetTileEntity(_blockPos);
		component.TileEntity = tileEntity;
		component.IsOn = _isOn;
		return _isOn;
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (_blockValue.ischild)
		{
			return;
		}
		if (!(_world.GetTileEntity(_blockPos) is TileEntityPoweredRangedTrap))
		{
			TileEntityPoweredRangedTrap tileEntityPoweredRangedTrap = (TileEntityPoweredRangedTrap)CreateTileEntity(_chunk);
			tileEntityPoweredRangedTrap.SetDisableModifiedCheck(_b: true);
			tileEntityPoweredRangedTrap.localChunkPos = World.toBlock(_blockPos);
			if (_addedByPlayer != null)
			{
				tileEntityPoweredRangedTrap.SetOwner(_addedByPlayer);
			}
			tileEntityPoweredRangedTrap.InitializePowerData();
			_chunk.AddTileEntity(tileEntityPoweredRangedTrap);
			tileEntityPoweredRangedTrap.SetDisableModifiedCheck(_b: false);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				tileEntityPoweredRangedTrap.SetModified();
			}
		}
		BlockEntityData ecd = new BlockEntityData(_blockValue, _blockPos)
		{
			bNeedsTemperature = true
		};
		_chunk.AddEntityBlockStub(ecd);
	}

	public override void OnBlockLoaded(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _blockPos, _blockValue);
		bool flag = (_blockValue.meta & 2) != 0;
		if (_world.GetTileEntity(_blockPos) is TileEntityPoweredBlock item)
		{
			if (flag)
			{
				PowerManager.Instance.ClientUpdateList.Add(item);
			}
			else
			{
				PowerManager.Instance.ClientUpdateList.Remove(item);
			}
		}
	}

	public override TileEntityPowered CreateTileEntity(Chunk _chunk)
	{
		return new TileEntityPoweredRangedTrap(_chunk)
		{
			PowerItemType = PowerItem.PowerItemTypes.RangedTrap
		};
	}
}
