using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureLockPickable : TEFeatureAbs, IFeatureTriggerCapability
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropLockPickTime = "LockPickTime";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropLockPickItem = "LockPickItem";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropLockPickBreakChance = "LockPickBreakChance";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropOnLockPickSuccessEvent = "LockPickSuccessEvent";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropOnLockPickFailedEvent = "LockPickFailedEvent";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropLockpickDowngradeBlock = "LockPickDowngradeBlock";

	[PublicizedFrom(EAccessModifier.Private)]
	public string lockPickItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lockPickTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lockPickBreakChance;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lockPickSuccessEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lockPickFailedEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue lockpickDowngradeBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public float unlockCompletion = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lockPicksUsed = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public float nthPercentile = -1f;

	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		DynamicProperties props = _featureData.Props;
		if (!props.Values.ContainsKey(PropLockPickItem))
		{
			Log.Error("Block with name " + base.Parent.TeData.Block.GetBlockName() + " does not have a " + PropLockPickItem + " property for LockPickable feature");
		}
		props.ParseString(PropLockPickItem, ref lockPickItem);
		props.ParseFloat(PropLockPickTime, ref lockPickTime);
		props.ParseFloat(PropLockPickBreakChance, ref lockPickBreakChance);
		props.ParseString(PropOnLockPickSuccessEvent, ref lockPickSuccessEvent);
		props.ParseString(PropOnLockPickFailedEvent, ref lockPickFailedEvent);
		if (props.Values.ContainsKey(PropLockpickDowngradeBlock))
		{
			string text = props.Values[PropLockpickDowngradeBlock];
			if (!string.IsNullOrEmpty(text))
			{
				lockpickDowngradeBlock = Block.GetBlockValue(text);
				if (lockpickDowngradeBlock.isair)
				{
					throw new Exception("Block with name '" + text + "' not found in block " + base.Parent.TeData.Block.GetBlockName());
				}
			}
		}
		else
		{
			lockpickDowngradeBlock = base.Parent.TeData.Block.DowngradeBlock;
		}
		if (lockPickTime > 0f)
		{
			unlockCompletion = 0f;
		}
		else
		{
			unlockCompletion = 1f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyFromInternal(TileEntityComposite _other)
	{
	}

	public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.Read(_br, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeRead.Persistency)
		{
			if (base.Parent.UseLocalVersioning())
			{
				_br.ReadUInt16();
			}
			else
			{
				base.Parent.GetLegacyForkVersion();
			}
		}
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write((ushort)18);
		}
	}

	public bool NeedsLockpicking()
	{
		return unlockCompletion < 1f;
	}

	public override bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return LocalPlayerUI.GetUIForPrimaryPlayer() != null;
	}

	public override void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		ShowUI(_success);
	}

	public void ShowUI(bool _lockGranted)
	{
		LocalPlayerUI uIForPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (!_lockGranted)
		{
			GameManager.ShowTooltip(uIForPrimaryPlayer.entityPlayer, Localization.Get("ttNoInteractItem"), string.Empty, "ui_denied");
			return;
		}
		EntityPlayerLocal player = uIForPrimaryPlayer.entityPlayer;
		if (nthPercentile < 0f)
		{
			nthPercentile = player.rand.RandomRange(0f, 0.999999f);
			lockPicksUsed = 0;
			unlockCompletion = 0f;
		}
		float alternateTime = -1f;
		float value = EffectManager.GetValue(PassiveEffects.LockPickTime, player.inventory.holdingItemItemValue, lockPickTime, player);
		float num = value * (1f - unlockCompletion);
		float value2 = EffectManager.GetValue(PassiveEffects.LockPickBreakChance, player.inventory.holdingItemItemValue, lockPickBreakChance, player);
		int num2 = CalculateLockpicksNeeded(1f - value2, nthPercentile) - lockPicksUsed;
		if (num2 > 1)
		{
			alternateTime = value - num + RandomizeNextBreak(num2 - 1) * num;
		}
		TimerEventData timerEventData = new TimerEventData();
		timerEventData.CloseEvent += EventData_CloseEvent;
		timerEventData.Data = player;
		timerEventData.FullTimeFinishEvent += EventData_Event;
		timerEventData.AlternateTime = alternateTime;
		timerEventData.AlternateEvent += EventData_BreakEvent;
		XUiC_Timer.OpenTimer(uIForPrimaryPlayer.xui, value, timerEventData, num);
		Manager.BroadcastPlayByLocalPlayer(ToWorldPos().ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
		[PublicizedFrom(EAccessModifier.Private)]
		int CalculateLockpicksNeeded(float _lpSuccess, float _nthPercentile, int _maxTries = 15)
		{
			if (_lpSuccess <= 0f || _lpSuccess > 1f || _maxTries <= 0)
			{
				return 1;
			}
			float num3 = 1f - _lpSuccess;
			return Utils.FastMin(Utils.Fastfloor(Math.Log(1.0 - (double)nthPercentile) / Math.Log(num3)) + 1, _maxTries);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		float RandomizeNextBreak(int _breaksRemaining)
		{
			if (_breaksRemaining <= 0)
			{
				return -1f;
			}
			float num3 = 1f;
			for (int i = 0; i < _breaksRemaining; i++)
			{
				float num4 = player.rand.RandomRange(0f, 1f);
				if (num4 < num3)
				{
					num3 = num4;
				}
			}
			return Utils.FastClamp(num3, 0.05f, 0.95f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_CloseEvent(TimerEventData _timerData)
	{
		_ = (EntityPlayerLocal)_timerData.Data;
		Manager.BroadcastPlayByLocalPlayer(ToWorldPos().ToVector3() + Vector3.one * 0.5f, "Misc/locked");
		LockManager.Instance.UnlockRequestLocal();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_BreakEvent(TimerEventData _timerData)
	{
		EntityPlayerLocal entityPlayerLocal = (EntityPlayerLocal)_timerData.Data;
		Vector3i vector3i = ToWorldPos();
		ItemValue item = ItemClass.GetItem(lockPickItem);
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		Manager.BroadcastPlayByLocalPlayer(vector3i.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
		ItemStack itemStack = new ItemStack(item, 1);
		uIForPlayer.xui.PlayerInventory.RemoveItem(itemStack);
		GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttLockpickBroken"));
		uIForPlayer.xui.CollectedItemList.RemoveItemStack(itemStack);
		unlockCompletion = Utils.FastMax(unlockCompletion, _timerData.Completion);
		lockPicksUsed++;
		if (lockPickFailedEvent != null)
		{
			GameEventManager.Current.HandleAction(lockPickFailedEvent, null, entityPlayerLocal, twitchActivated: false, vector3i);
		}
		LockManager.Instance.UnlockRequestLocal();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_Event(TimerEventData _timerData)
	{
		EntityPlayerLocal entity = (EntityPlayerLocal)_timerData.Data;
		Vector3i vector3i = ToWorldPos();
		LockManager.Instance.UnlockRequestLocal();
		unlockCompletion = 1f;
		lockPicksUsed++;
		DowngradeToUnlockedVariant(vector3i);
		Manager.BroadcastPlayByLocalPlayer(vector3i.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
		if (lockPickSuccessEvent != null)
		{
			GameEventManager.Current.HandleAction(lockPickSuccessEvent, null, entity, twitchActivated: false, vector3i);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DowngradeToUnlockedVariant(Vector3i _blockPos)
	{
		if (!lockpickDowngradeBlock.isair)
		{
			World world = GameManager.Instance.World;
			BlockValue block = world.GetBlock(_blockPos);
			BlockValue blockValue = lockpickDowngradeBlock;
			blockValue = BlockPlaceholderMap.Instance.Replace(blockValue, world.GetGameRandom(), _blockPos.x, _blockPos.z);
			blockValue.rotation = block.rotation;
			blockValue.meta = block.meta;
			world.SetBlockRPC(_blockPos, blockValue, blockValue.Block.Density);
		}
	}

	public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		base.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
		if (unlockCompletion < 1f)
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

	public override bool AllowBlockActivationCommand(ITileEntityFeature _module, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		if (!base.AllowBlockActivationCommand(_module, _commandName, _world, _blockPos, _blockValue, _entityFocusing))
		{
			return false;
		}
		if (!Equals(_module))
		{
			if (unlockCompletion != 1f)
			{
				return base.Parent.LocalPlayerIsOwner;
			}
			return true;
		}
		if (CommandIs(_commandName, "pick"))
		{
			return !base.Parent.LocalPlayerIsOwner;
		}
		return true;
	}

	public void OnBlockTriggered(EntityPlayer _player, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		if (_triggeredBy != null && _triggeredBy.Unlock)
		{
			DowngradeToUnlockedVariant(_blockPos);
		}
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (CommandIs(_commandName, "pick"))
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
			LockManager.Instance.LockRequestLocal(this, null, 0);
			return true;
		}
		return false;
	}
}
