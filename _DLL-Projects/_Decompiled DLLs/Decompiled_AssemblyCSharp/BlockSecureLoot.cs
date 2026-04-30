using System;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockSecureLoot : Block
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropLootList = "LootList";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropLootStageMod = "LootStageMod";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropLootStageBonus = "LootStageBonus";

	public static string PropLockPickTime = "LockPickTime";

	public static string PropLockPickItem = "LockPickItem";

	public static string PropLockPickBreakChance = "LockPickBreakChance";

	public static string PropOnLockPickSuccessEvent = "LockPickSuccessEvent";

	public static string PropOnLockPickFailedEvent = "LockPickFailedEvent";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string lootList;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float lockPickTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string lockPickItem;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float lockPickBreakChance;

	public float LootStageMod;

	public float LootStageBonus;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string lockPickSuccessEvent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string lockPickFailedEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[6]
	{
		new BlockActivationCommand("pick", "unlock", _enabled: false),
		new BlockActivationCommand("Search", "search", _enabled: false),
		new BlockActivationCommand("lock", "lock", _enabled: false),
		new BlockActivationCommand("unlock", "unlock", _enabled: false),
		new BlockActivationCommand("keypad", "keypad", _enabled: false),
		new BlockActivationCommand("trigger", "wrench", _enabled: true)
	};

	public override bool AllowBlockTriggers => true;

	public BlockSecureLoot()
	{
		HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (!base.Properties.Values.ContainsKey(PropLootList))
		{
			throw new Exception("Block with name " + GetBlockName() + " doesnt have a loot list");
		}
		lootList = base.Properties.Values[PropLootList];
		if (base.Properties.Values.ContainsKey(PropLockPickTime))
		{
			lockPickTime = StringParsers.ParseFloat(base.Properties.Values[PropLockPickTime]);
		}
		else
		{
			lockPickTime = 15f;
		}
		if (base.Properties.Values.ContainsKey(PropLockPickItem))
		{
			lockPickItem = base.Properties.Values[PropLockPickItem];
		}
		if (base.Properties.Values.ContainsKey(PropLockPickBreakChance))
		{
			lockPickBreakChance = StringParsers.ParseFloat(base.Properties.Values[PropLockPickBreakChance]);
		}
		else
		{
			lockPickBreakChance = 0f;
		}
		base.Properties.ParseFloat(PropLootStageMod, ref LootStageMod);
		base.Properties.ParseFloat(PropLootStageBonus, ref LootStageBonus);
		base.Properties.ParseString(PropOnLockPickSuccessEvent, ref lockPickSuccessEvent);
		base.Properties.ParseString(PropOnLockPickFailedEvent, ref lockPickFailedEvent);
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		if (_world.GetTileEntity(_result.clrIdx, _result.blockPos) is TileEntitySecureLootContainer tileEntitySecureLootContainer)
		{
			tileEntitySecureLootContainer.SetEmpty();
			if (_ea != null && _ea.entityType == EntityType.Player)
			{
				tileEntitySecureLootContainer.bPlayerStorage = true;
				tileEntitySecureLootContainer.SetOwner(PlatformManager.InternalLocalUserIdentifier);
				tileEntitySecureLootContainer.SetLocked(_isLocked: false);
			}
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntitySecureLootContainer tileEntitySecureLootContainer = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntitySecureLootContainer;
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		if (tileEntitySecureLootContainer != null)
		{
			string arg2 = _blockValue.Block.GetLocalizedBlockName();
			if (!tileEntitySecureLootContainer.IsLocked())
			{
				return string.Format(Localization.Get("tooltipUnlocked"), arg, arg2);
			}
			if (lockPickItem == null && !tileEntitySecureLootContainer.LocalPlayerIsOwner())
			{
				return string.Format(Localization.Get("tooltipJammed"), arg, arg2);
			}
			return string.Format(Localization.Get("tooltipLocked"), arg, arg2);
		}
		return "";
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return _world.GetTileEntity(_clrIdx, _blockPos) is TileEntitySecureLootContainer;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!(_world.GetTileEntity(_clrIdx, _blockPos) is TileEntitySecureLootContainer tileEntitySecureLootContainer))
		{
			return BlockActivationCommand.Empty;
		}
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		PersistentPlayerData playerData = _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(tileEntitySecureLootContainer.GetOwner());
		bool flag = tileEntitySecureLootContainer.LocalPlayerIsOwner();
		bool flag2 = !flag && playerData != null && playerData.ACL != null && playerData.ACL.Contains(internalLocalUserIdentifier);
		cmds[1].enabled = true;
		cmds[2].enabled = !tileEntitySecureLootContainer.IsLocked() && (flag || flag2);
		cmds[3].enabled = tileEntitySecureLootContainer.IsLocked() && flag;
		cmds[4].enabled = (!tileEntitySecureLootContainer.IsUserAllowed(internalLocalUserIdentifier) && tileEntitySecureLootContainer.HasPassword() && tileEntitySecureLootContainer.IsLocked()) || flag;
		cmds[0].enabled = lockPickItem != null && tileEntitySecureLootContainer.IsLocked() && !flag;
		cmds[5].enabled = _world.IsEditor() && !GameUtils.IsWorldEditor();
		return cmds;
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!_blockValue.ischild && !(world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntitySecureLootContainer))
		{
			TileEntitySecureLootContainer tileEntitySecureLootContainer = new TileEntitySecureLootContainer(_chunk);
			tileEntitySecureLootContainer.localChunkPos = World.toBlock(_blockPos);
			tileEntitySecureLootContainer.lootListName = lootList;
			tileEntitySecureLootContainer.SetContainerSize(LootContainer.GetLootContainer(lootList).size);
			_chunk.AddTileEntity(tileEntitySecureLootContainer);
		}
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		_chunk.RemoveTileEntityAt<TileEntitySecureLootContainer>((World)world, World.toBlock(_blockPos));
	}

	public override DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		if (_world.GetTileEntity(_clrIdx, _blockPos) is TileEntitySecureLootContainer tileEntitySecureLootContainer)
		{
			tileEntitySecureLootContainer.OnDestroy();
		}
		return DestroyedResult.Downgrade;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return OnBlockActivated(_commandName, _world, _cIdx, parentPos, block, _player);
		}
		if (!(_world.GetTileEntity(_cIdx, _blockPos) is TileEntitySecureLootContainer tileEntitySecureLootContainer))
		{
			return false;
		}
		switch (_commandName)
		{
		case "Search":
			if (!tileEntitySecureLootContainer.IsLocked() || tileEntitySecureLootContainer.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
			{
				return OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
			}
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
			return false;
		case "lock":
			tileEntitySecureLootContainer.SetLocked(_isLocked: true);
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locking");
			GameManager.ShowTooltip(_player, "containerLocked");
			return true;
		case "unlock":
			tileEntitySecureLootContainer.SetLocked(_isLocked: false);
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
			GameManager.ShowTooltip(_player, "containerUnlocked");
			return true;
		case "keypad":
		{
			LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_player);
			if (uIForPlayer != null)
			{
				XUiC_KeypadWindow.Open(uIForPlayer, tileEntitySecureLootContainer);
			}
			return true;
		}
		case "pick":
		{
			LocalPlayerUI playerUI = _player.PlayerUI;
			ItemValue item = ItemClass.GetItem(lockPickItem);
			if (playerUI.xui.PlayerInventory.GetItemCount(item) == 0)
			{
				playerUI.xui.CollectedItemList.AddItemStack(new ItemStack(item, 0), _bAddOnlyIfNotExisting: true);
				GameManager.ShowTooltip(_player, Localization.Get("ttLockpickMissing"));
				return true;
			}
			_player.AimingGun = false;
			Vector3i blockPos = tileEntitySecureLootContainer.ToWorldPos();
			tileEntitySecureLootContainer.bWasTouched = tileEntitySecureLootContainer.bTouched;
			_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntitySecureLootContainer.entityId, _player.entityId, "lockpick");
			return true;
		}
		case "trigger":
			XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _cIdx, _blockPos, _showTriggers: false, _showTriggeredBy: true);
			return true;
		default:
			return false;
		}
	}

	public void ShowLockpickUi(TileEntitySecureLootContainer _te, EntityPlayerLocal _player)
	{
		Vector3i vector3i = _te.ToWorldPos();
		ItemValue item = ItemClass.GetItem(lockPickItem);
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_player);
		uIForPlayer.windowManager.Open("timer", _bModal: true);
		XUiC_Timer childByType = uIForPlayer.xui.GetChildByType<XUiC_Timer>();
		TimerEventData timerEventData = new TimerEventData();
		timerEventData.CloseEvent += EventData_CloseEvent;
		float alternateTime = -1f;
		if (_player.rand.RandomRange(1f) < EffectManager.GetValue(PassiveEffects.LockPickBreakChance, _player.inventory.holdingItemItemValue, lockPickBreakChance, _player))
		{
			float value = EffectManager.GetValue(PassiveEffects.LockPickTime, _player.inventory.holdingItemItemValue, lockPickTime, _player);
			float num = value - ((_te.PickTimeLeft == -1f) ? (value - 1f) : (_te.PickTimeLeft + 1f));
			alternateTime = _player.rand.RandomRange(num + 1f, value - 1f);
		}
		timerEventData.Data = new object[5]
		{
			_te.GetClrIdx(),
			_te.blockValue,
			vector3i,
			_player,
			item
		};
		timerEventData.Event += EventData_Event;
		timerEventData.alternateTime = alternateTime;
		timerEventData.AlternateEvent += EventData_CloseEvent;
		childByType.SetTimer(EffectManager.GetValue(PassiveEffects.LockPickTime, _player.inventory.holdingItemItemValue, lockPickTime, _player), timerEventData, _te.PickTimeLeft);
		Manager.BroadcastPlayByLocalPlayer(vector3i.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_CloseEvent(TimerEventData timerData)
	{
		object[] array = (object[])timerData.Data;
		int clrIdx = (int)array[0];
		Vector3i vector3i = (Vector3i)array[2];
		EntityPlayerLocal entityPlayerLocal = array[3] as EntityPlayerLocal;
		ItemValue itemValue = array[4] as ItemValue;
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		Manager.BroadcastPlayByLocalPlayer(vector3i.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
		ItemStack itemStack = new ItemStack(itemValue, 1);
		uIForPlayer.xui.PlayerInventory.RemoveItem(itemStack);
		GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttLockpickBroken"));
		uIForPlayer.xui.CollectedItemList.RemoveItemStack(itemStack);
		if (GameManager.Instance.World.GetTileEntity((int)array[0], vector3i) is TileEntitySecureLootContainer tileEntitySecureLootContainer)
		{
			tileEntitySecureLootContainer.PickTimeLeft = Mathf.Max(lockPickTime * 0.25f, timerData.timeLeft);
			if (lockPickFailedEvent != null)
			{
				GameEventManager.Current.HandleAction(lockPickFailedEvent, null, entityPlayerLocal, twitchActivated: false, vector3i);
			}
			ResetEventData(timerData);
			GameManager.Instance.TEUnlockServer(clrIdx, vector3i, tileEntitySecureLootContainer.EntityId, _allowContainerDestroy: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_Event(TimerEventData timerData)
	{
		World world = GameManager.Instance.World;
		object[] obj = (object[])timerData.Data;
		int clrIdx = (int)obj[0];
		_ = (BlockValue)obj[1];
		Vector3i vector3i = (Vector3i)obj[2];
		BlockValue block = world.GetBlock(vector3i);
		EntityPlayerLocal entityPlayerLocal = obj[3] as EntityPlayerLocal;
		_ = obj[4];
		if (world.GetTileEntity(clrIdx, vector3i) is TileEntitySecureLootContainer tileEntitySecureLootContainer)
		{
			LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
			if (!LockpickDowngradeBlock.isair)
			{
				BlockValue lockpickDowngradeBlock = LockpickDowngradeBlock;
				lockpickDowngradeBlock = BlockPlaceholderMap.Instance.Replace(lockpickDowngradeBlock, world.GetGameRandom(), vector3i.x, vector3i.z);
				lockpickDowngradeBlock.rotation = block.rotation;
				lockpickDowngradeBlock.meta = block.meta;
				world.SetBlockRPC(clrIdx, vector3i, lockpickDowngradeBlock, lockpickDowngradeBlock.Block.Density);
			}
			Manager.BroadcastPlayByLocalPlayer(vector3i.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
			if (lockPickSuccessEvent != null)
			{
				GameEventManager.Current.HandleAction(lockPickSuccessEvent, null, entityPlayerLocal, twitchActivated: false, vector3i);
			}
			ResetEventData(timerData);
			GameManager.Instance.TEUnlockServer(clrIdx, vector3i, tileEntitySecureLootContainer.EntityId, _allowContainerDestroy: false);
		}
	}

	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return OnBlockActivated(_world, _cIdx, parentPos, block, _player);
		}
		if (!(_world.GetTileEntity(_cIdx, _blockPos) is TileEntitySecureLootContainer tileEntitySecureLootContainer))
		{
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = tileEntitySecureLootContainer.ToWorldPos();
		tileEntitySecureLootContainer.bWasTouched = tileEntitySecureLootContainer.bTouched;
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntitySecureLootContainer.entityId, _player.entityId);
		return true;
	}

	public override bool IsWaterBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides)
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetEventData(TimerEventData timerData)
	{
		timerData.AlternateEvent -= EventData_CloseEvent;
		timerData.CloseEvent -= EventData_CloseEvent;
		timerData.Event -= EventData_Event;
	}

	public override void OnTriggered(EntityPlayer _player, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		base.OnTriggered(_player, _world, _cIdx, _blockPos, _blockValue, _blockChanges, _triggeredBy);
		if (!LockpickDowngradeBlock.isair)
		{
			BlockValue lockpickDowngradeBlock = LockpickDowngradeBlock;
			lockpickDowngradeBlock = BlockPlaceholderMap.Instance.Replace(lockpickDowngradeBlock, _world.GetGameRandom(), _blockPos.x, _blockPos.z);
			lockpickDowngradeBlock.rotation = _blockValue.rotation;
			lockpickDowngradeBlock.meta = _blockValue.meta;
			_world.SetBlockRPC(_cIdx, _blockPos, lockpickDowngradeBlock, lockpickDowngradeBlock.Block.Density);
		}
		Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
	}
}
