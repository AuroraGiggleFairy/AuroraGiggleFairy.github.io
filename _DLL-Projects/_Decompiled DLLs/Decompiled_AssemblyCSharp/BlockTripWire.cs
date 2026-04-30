using UnityEngine.Scripting;

[Preserve]
public class BlockTripWire : BlockPowered
{
	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("options", "tool", _enabled: true),
		new BlockActivationCommand("take", "hand", _enabled: false)
	};

	public BlockTripWire()
	{
		HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
	}

	public override TileEntityPowered CreateTileEntity(Chunk chunk)
	{
		return new TileEntityPoweredTrigger(chunk)
		{
			PowerItemType = PowerItem.PowerItemTypes.TripWireRelay,
			TriggerType = PowerTrigger.TriggerTypes.TripWire
		};
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!(_world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityPoweredTrigger))
		{
			TileEntityPowered tileEntityPowered = CreateTileEntity(_chunk);
			tileEntityPowered.localChunkPos = World.toBlock(_blockPos);
			tileEntityPowered.InitializePowerData();
			_chunk.AddTileEntity(tileEntityPowered);
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntityPoweredTrigger tileEntityPoweredTrigger = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityPoweredTrigger;
		if (!tileEntityPoweredTrigger.ShowTriggerOptions || !_world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer()))
		{
			return "";
		}
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		if (tileEntityPoweredTrigger != null && tileEntityPoweredTrigger.ShowTriggerOptions)
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
		if (!(_world.GetTileEntity(_cIdx, _blockPos) is TileEntityPoweredTrigger tileEntityPoweredTrigger))
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
		Vector3i blockPos = tileEntityPoweredTrigger.ToWorldPos();
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityPoweredTrigger.entityId, _player.entityId);
		return true;
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntityPoweredTrigger tileEntityPoweredTrigger = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityPoweredTrigger;
		bool flag = _world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		bool flag2 = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		cmds[0].enabled = tileEntityPoweredTrigger.ShowTriggerOptions && flag;
		cmds[1].enabled = flag2 && TakeDelay > 0f;
		return cmds;
	}
}
