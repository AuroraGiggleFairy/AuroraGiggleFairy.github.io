using System;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureLockPickable : TEFeatureAbs, ILockPickable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ILockable lockFeature;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string LockPickItem;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float LockPickTime = 15f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float LockPickBreakChance;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string LockPickSuccessEvent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string LockPickFailedEvent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public BlockValue LockpickDowngradeBlock;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float PickTimeLeft = -1f;

	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		lockFeature = base.Parent.GetFeature<ILockable>();
		if (lockFeature == null)
		{
			Log.Error("Block with name " + base.Parent.TeData.Block.GetBlockName() + " does have a LockPickable but no Lockable feature");
		}
		DynamicProperties props = _featureData.Props;
		if (!props.Values.ContainsKey(BlockSecureLoot.PropLockPickItem))
		{
			Log.Error("Block with name " + base.Parent.TeData.Block.GetBlockName() + " does not have a " + BlockSecureLoot.PropLockPickItem + " property for LockPickable feature");
		}
		props.ParseString(BlockSecureLoot.PropLockPickItem, ref LockPickItem);
		props.ParseFloat(BlockSecureLoot.PropLockPickTime, ref LockPickTime);
		props.ParseFloat(BlockSecureLoot.PropLockPickBreakChance, ref LockPickBreakChance);
		props.ParseString(BlockSecureLoot.PropOnLockPickSuccessEvent, ref LockPickSuccessEvent);
		props.ParseString(BlockSecureLoot.PropOnLockPickFailedEvent, ref LockPickFailedEvent);
		if (props.Values.ContainsKey(Block.PropLockpickDowngradeBlock))
		{
			string text = props.Values[Block.PropLockpickDowngradeBlock];
			if (!string.IsNullOrEmpty(text))
			{
				LockpickDowngradeBlock = Block.GetBlockValue(text);
				if (LockpickDowngradeBlock.isair)
				{
					throw new Exception("Block with name '" + text + "' not found in block " + base.Parent.TeData.Block.GetBlockName());
				}
			}
		}
		else
		{
			LockpickDowngradeBlock = base.Parent.TeData.Block.DowngradeBlock;
		}
	}

	public void ShowLockpickUi(EntityPlayerLocal _player)
	{
		if (!(_player == null))
		{
			LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_player);
			uIForPlayer.windowManager.Open("timer", _bModal: true);
			XUiC_Timer childByType = uIForPlayer.xui.GetChildByType<XUiC_Timer>();
			float alternateTime = -1f;
			float num = _player.rand.RandomRange(1f);
			float value = EffectManager.GetValue(PassiveEffects.LockPickTime, _player.inventory.holdingItemItemValue, LockPickTime, _player);
			if (num < EffectManager.GetValue(PassiveEffects.LockPickBreakChance, _player.inventory.holdingItemItemValue, LockPickBreakChance, _player))
			{
				float num2 = value - ((PickTimeLeft == -1f) ? (value - 1f) : (PickTimeLeft + 1f));
				alternateTime = _player.rand.RandomRange(num2 + 1f, value - 1f);
			}
			TimerEventData timerEventData = new TimerEventData();
			timerEventData.CloseEvent += EventData_CloseEvent;
			timerEventData.Data = _player;
			timerEventData.Event += EventData_Event;
			timerEventData.alternateTime = alternateTime;
			timerEventData.AlternateEvent += EventData_CloseEvent;
			childByType.SetTimer(value, timerEventData, PickTimeLeft);
			Manager.BroadcastPlayByLocalPlayer(base.Parent.ToWorldPos().ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_CloseEvent(TimerEventData _timerData)
	{
		EntityPlayerLocal entityPlayerLocal = (EntityPlayerLocal)_timerData.Data;
		Vector3i vector3i = base.Parent.ToWorldPos();
		ItemValue item = ItemClass.GetItem(LockPickItem);
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		Manager.BroadcastPlayByLocalPlayer(vector3i.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
		ItemStack itemStack = new ItemStack(item, 1);
		uIForPlayer.xui.PlayerInventory.RemoveItem(itemStack);
		GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttLockpickBroken"));
		uIForPlayer.xui.CollectedItemList.RemoveItemStack(itemStack);
		PickTimeLeft = Mathf.Max(LockPickTime * 0.25f, _timerData.timeLeft);
		if (LockPickFailedEvent != null)
		{
			GameEventManager.Current.HandleAction(LockPickFailedEvent, null, entityPlayerLocal, twitchActivated: false, vector3i);
		}
		ResetEventData(_timerData);
		GameManager.Instance.TEUnlockServer(0, vector3i, base.Parent.EntityId, _allowContainerDestroy: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_Event(TimerEventData _timerData)
	{
		World world = GameManager.Instance.World;
		EntityPlayerLocal entity = (EntityPlayerLocal)_timerData.Data;
		Vector3i vector3i = base.Parent.ToWorldPos();
		BlockValue block = world.GetBlock(vector3i);
		lockFeature.SetLocked(_isLocked: false);
		if (!LockpickDowngradeBlock.isair)
		{
			BlockValue lockpickDowngradeBlock = base.Parent.TeData.Block.LockpickDowngradeBlock;
			lockpickDowngradeBlock = BlockPlaceholderMap.Instance.Replace(lockpickDowngradeBlock, world.GetGameRandom(), vector3i.x, vector3i.z);
			lockpickDowngradeBlock.rotation = block.rotation;
			lockpickDowngradeBlock.meta = block.meta;
			world.SetBlockRPC(0, vector3i, lockpickDowngradeBlock, lockpickDowngradeBlock.Block.Density);
		}
		Manager.BroadcastPlayByLocalPlayer(vector3i.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
		if (LockPickSuccessEvent != null)
		{
			GameEventManager.Current.HandleAction(LockPickSuccessEvent, null, entity, twitchActivated: false, vector3i);
		}
		ResetEventData(_timerData);
		GameManager.Instance.TEUnlockServer(0, vector3i, base.Parent.EntityId, _allowContainerDestroy: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetEventData(TimerEventData _timerData)
	{
		_timerData.AlternateEvent -= EventData_CloseEvent;
		_timerData.CloseEvent -= EventData_CloseEvent;
		_timerData.Event -= EventData_Event;
	}

	public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		base.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
		if (!lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			return string.Format(Localization.Get("tooltipLocked"), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		return null;
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("pick", "unlock", _enabled: false), TileEntityComposite.EBlockCommandOrder.First, base.FeatureData);
	}

	public override void UpdateBlockActivationCommands(ref BlockActivationCommand _command, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		base.UpdateBlockActivationCommands(ref _command, _commandName, _world, _blockPos, _blockValue, _entityFocusing);
		if (CommandIs(_commandName, "pick"))
		{
			_command.enabled = lockFeature.IsLocked() && !lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier);
		}
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (CommandIs(_commandName, "pick"))
		{
			LocalPlayerUI playerUI = _player.PlayerUI;
			ItemValue item = ItemClass.GetItem(LockPickItem);
			if (playerUI.xui.PlayerInventory.GetItemCount(item) == 0)
			{
				playerUI.xui.CollectedItemList.AddItemStack(new ItemStack(item, 0), _bAddOnlyIfNotExisting: true);
				GameManager.ShowTooltip(_player, Localization.Get("ttLockpickMissing"));
				return true;
			}
			_player.AimingGun = false;
			Vector3i blockPos = base.Parent.ToWorldPos();
			_world.GetGameManager().TELockServer(0, blockPos, base.Parent.EntityId, _player.entityId, "lockpick");
			return true;
		}
		return false;
	}
}
