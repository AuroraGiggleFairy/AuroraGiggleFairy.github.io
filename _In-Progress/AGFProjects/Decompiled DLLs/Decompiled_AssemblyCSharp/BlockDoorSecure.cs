using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockDoorSecure : BlockDoor
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDoorIsLockedMask = 4;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string lockedSound = "Misc/locked";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string lockingSound = "Misc/locking";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string unlockingSound = "Misc/unlocking";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropLockedSound = "LockedSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropLockingSound = "LockingSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropUnLockingSound = "UnlockingSound";

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[6]
	{
		new BlockActivationCommand("close", "door", _enabled: false),
		new BlockActivationCommand("open", "door", _enabled: false),
		new BlockActivationCommand("lock", "lock", _enabled: false),
		new BlockActivationCommand("unlock", "unlock", _enabled: false),
		new BlockActivationCommand("keypad", "keypad", _enabled: false),
		new BlockActivationCommand("trigger", "wrench", _enabled: true)
	};

	public override bool AllowBlockTriggers => true;

	public BlockDoorSecure()
	{
		HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		base.Properties.ParseString(PropLockedSound, ref lockedSound);
		base.Properties.ParseString(PropLockingSound, ref lockingSound);
		base.Properties.ParseString(PropUnLockingSound, ref unlockingSound);
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!_world.IsEditor() && !_blockValue.ischild)
		{
			TileEntitySecureDoor tileEntitySecureDoor = _world.GetTileEntity(_chunk.ClrIdx, _blockPos) as TileEntitySecureDoor;
			if (tileEntitySecureDoor == null)
			{
				tileEntitySecureDoor = new TileEntitySecureDoor(_chunk);
				tileEntitySecureDoor.SetDisableModifiedCheck(_b: true);
				tileEntitySecureDoor.localChunkPos = World.toBlock(_blockPos);
				tileEntitySecureDoor.SetLocked(IsDoorLockedMeta(_blockValue.meta));
				tileEntitySecureDoor.SetDisableModifiedCheck(_b: false);
				_chunk.AddTileEntity(tileEntitySecureDoor);
			}
		}
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		if (_world.IsEditor())
		{
			SetIsLocked(IsDoorLockedMeta(_newBlockValue.meta), _world, _blockPos);
		}
	}

	public override bool FilterIndexType(BlockValue bv)
	{
		if (IndexName == "TraderOnOff")
		{
			return !IsDoorLockedMeta(bv.meta);
		}
		return true;
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		_chunk.RemoveTileEntityAt<TileEntitySecureDoor>((World)world, World.toBlock(_blockPos));
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!(_world.GetTileEntity(_clrIdx, _blockPos) is TileEntitySecureDoor))
		{
			return _world.IsEditor();
		}
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return GetBlockActivationCommands(_world, block, _clrIdx, parentPos, _entityFocusing);
		}
		TileEntitySecureDoor tileEntitySecureDoor = (TileEntitySecureDoor)_world.GetTileEntity(_clrIdx, _blockPos);
		if (tileEntitySecureDoor == null && !_world.IsEditor())
		{
			return BlockActivationCommand.Empty;
		}
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		PersistentPlayerData persistentPlayerData = ((!_world.IsEditor()) ? _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(tileEntitySecureDoor.GetOwner()) : null);
		bool flag = _world.IsEditor() || (!tileEntitySecureDoor.LocalPlayerIsOwner() && persistentPlayerData != null && persistentPlayerData.ACL != null && persistentPlayerData.ACL.Contains(internalLocalUserIdentifier));
		bool flag2 = ((!_world.IsEditor()) ? tileEntitySecureDoor.IsLocked() : IsDoorLockedMeta(_blockValue.meta));
		bool flag3 = _world.IsEditor() || tileEntitySecureDoor.LocalPlayerIsOwner();
		bool flag4 = !_world.IsEditor() && tileEntitySecureDoor.IsUserAllowed(internalLocalUserIdentifier);
		bool flag5 = !_world.IsEditor() && tileEntitySecureDoor.HasPassword();
		((Chunk)_world.ChunkClusters[_clrIdx].GetChunkSync(World.toChunkXZ(_blockPos.x), _blockPos.y, World.toChunkXZ(_blockPos.z))).GetBlockTrigger(World.toBlock(_blockPos));
		cmds[0].enabled = BlockDoor.IsDoorOpen(_blockValue.meta);
		cmds[1].enabled = !BlockDoor.IsDoorOpen(_blockValue.meta);
		cmds[2].enabled = !flag2 && (flag3 || flag || _world.IsEditor());
		cmds[3].enabled = flag2 && (flag3 || _world.IsEditor());
		cmds[4].enabled = (!flag4 && flag5 && flag2) || flag3;
		cmds[5].enabled = _world.IsEditor() && !GameUtils.IsWorldEditor();
		return cmds;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return OnBlockActivated(_commandName, _world, _cIdx, parentPos, block, _player);
		}
		TileEntitySecureDoor tileEntitySecureDoor = (TileEntitySecureDoor)_world.GetTileEntity(_cIdx, _blockPos);
		if (tileEntitySecureDoor == null && !_world.IsEditor())
		{
			return false;
		}
		bool flag = ((!_world.IsEditor()) ? tileEntitySecureDoor.IsLocked() : IsDoorLockedMeta(_blockValue.meta));
		bool flag2 = !_world.IsEditor() && tileEntitySecureDoor.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier);
		switch (_commandName)
		{
		case "close":
			if (_world.IsEditor() || !flag || flag2)
			{
				HandleTrigger(_player, (World)_world, _cIdx, _blockPos, _blockValue);
				return OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
			}
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, lockedSound);
			return false;
		case "open":
			if (_world.IsEditor() || !flag || flag2)
			{
				HandleTrigger(_player, (World)_world, _cIdx, _blockPos, _blockValue);
				return OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
			}
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, lockedSound);
			return false;
		case "lock":
			SetIsLocked(_isLocked: true, _world, _blockPos);
			return true;
		case "unlock":
			SetIsLocked(_isLocked: false, _world, _blockPos);
			return true;
		case "keypad":
		{
			LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_player);
			if (uIForPlayer != null && tileEntitySecureDoor != null)
			{
				XUiC_KeypadWindow.Open(uIForPlayer, tileEntitySecureDoor);
			}
			return true;
		}
		case "trigger":
			XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _cIdx, _blockPos, _showTriggers: true, _showTriggeredBy: true);
			break;
		}
		return false;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return GetActivationText(_world, block, _clrIdx, parentPos, _entityFocusing);
		}
		TileEntitySecureDoor tileEntitySecureDoor = (TileEntitySecureDoor)_world.GetTileEntity(_clrIdx, _blockPos);
		if (tileEntitySecureDoor == null && !_world.IsEditor())
		{
			return "";
		}
		bool num = ((!_world.IsEditor()) ? tileEntitySecureDoor.IsLocked() : IsDoorLockedMeta(_blockValue.meta));
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		string arg2 = Localization.Get("door");
		if (!num)
		{
			return string.Format(Localization.Get("tooltipUnlocked"), arg, arg2);
		}
		return string.Format(Localization.Get("tooltipLocked"), arg, arg2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool IsDoorLockedMeta(byte _metadata)
	{
		return (_metadata & 4) != 0;
	}

	public bool IsDoorLocked(WorldBase _world, Vector3i _blockPos)
	{
		BlockValue block = _world.GetBlock(_blockPos);
		if (block.isair)
		{
			return false;
		}
		if (block.ischild)
		{
			return IsDoorLocked(_world, _blockPos + block.parent);
		}
		if (_world.IsEditor())
		{
			return IsDoorLockedMeta(block.meta);
		}
		if (!(_world.GetTileEntity(_blockPos) is TileEntitySecureDoor tileEntitySecureDoor))
		{
			return false;
		}
		return tileEntitySecureDoor.IsLocked();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue SetIsLocked(bool _isLocked, WorldBase _world, Vector3i _blockPos)
	{
		BlockValue block = _world.GetBlock(_blockPos);
		if (block.isair)
		{
			return block;
		}
		if (block.ischild)
		{
			return block;
		}
		if (_world.IsEditor())
		{
			return SetIsLockedEditor(_isLocked, _world, _blockPos, block);
		}
		SetIsLockedNonEditor(_isLocked, _world, _blockPos);
		return block;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetIsLockedNonEditor(bool _isLocked, WorldBase _world, Vector3i _blockPos)
	{
		if (_world.GetTileEntity(_blockPos) is TileEntitySecureDoor tileEntitySecureDoor && tileEntitySecureDoor.IsLocked() != _isLocked)
		{
			tileEntitySecureDoor.SetLocked(_isLocked);
			PlayLockingSound(_blockPos);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue SetIsLockedEditor(bool _isLocked, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		byte meta = _blockValue.meta;
		byte b = ((!_isLocked) ? ((byte)(meta & -5)) : ((byte)(meta | 4)));
		if (meta != b)
		{
			_blockValue.meta = b;
			_world.SetBlockRPC(_blockPos, _blockValue);
			PlayLockingSound(_blockPos);
		}
		return _blockValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayLockingSound(Vector3i _blockPos)
	{
		Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, lockingSound);
	}

	public override void OnTriggered(EntityPlayer _player, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		base.OnTriggered(_player, _world, _cIdx, _blockPos, _blockValue, _blockChanges, _triggeredBy);
		if ((_blockValue.meta & 1) != 0)
		{
			Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, closeSound);
		}
		else
		{
			Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, openSound);
		}
		if (_triggeredBy != null && _triggeredBy.Unlock)
		{
			_blockValue = SetIsLocked(_isLocked: false, _world, _blockPos);
		}
		_blockValue.meta ^= 1;
		_blockChanges.Add(new BlockChangeInfo(_cIdx, _blockPos, _blockValue));
	}

	public override void OnBlockReset(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (!_blockValue.ischild && _world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntitySecureDoor tileEntitySecureDoor)
		{
			tileEntitySecureDoor.SetLocked(IsDoorLockedMeta(_blockValue.meta));
		}
	}
}
