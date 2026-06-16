using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LootWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_LootWindow lootWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public ITileEntityLootable te;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lootingHeader;

	public static string ID = "looting";

	public override void Init()
	{
		base.Init();
		lootWindow = GetChildByType<XUiC_LootWindow>();
		nonPagingHeaderWindow = GetChildByType<XUiC_WindowNonPagingHeader>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openContainer(string _lootContainerName, ITileEntityLootable _te, bool _playOpenSound)
	{
		te = _te;
		te.SetUserAccessing(_bUserAccessing: true);
		lootWindow.SetTileEntityChest(_lootContainerName, _te, _playOpenSound);
		xui.playerUI.windowManager.Open(windowGroup, _bModal: true);
		xui.playerUI.windowManager.Open("backpack", _bModal: false);
		nonPagingHeaderWindow?.SetHeader(Localization.Get("xuiLooting"));
		xui.RecenterWindowGroup(windowGroup);
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.playerUI.windowManager.Close("backpack");
		Vector3i blockPos = te.ToWorldPos();
		ITileEntityLootable selfOrFeature = GameManager.Instance.World.GetTileEntity(blockPos).GetSelfOrFeature<ITileEntityLootable>();
		if ((selfOrFeature == null || !selfOrFeature.IsRemoving) && selfOrFeature == te)
		{
			te.SetModified();
		}
		te.SetUserAccessing(_bUserAccessing: false);
		LockManager.Instance.UnlockRequestLocal();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openTimerFinished(TimerEventData _data)
	{
		var (lootContainerName, tileEntityLootable) = ((string, ITileEntityLootable))_data.Data;
		if (!tileEntityLootable.bPlayerStorage)
		{
			xui.playerUI.entityPlayer.Progression.AddLevelExp(xui.playerUI.entityPlayer.gameStage, "_xpFromLoot", Progression.XPTypes.Looting);
		}
		openContainer(lootContainerName, tileEntityLootable, _playOpenSound: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void openTimerClosedManually(TimerEventData _data)
	{
		ITileEntityLootable item = (((string, ITileEntityLootable))_data.Data).Item2;
		if (item.bWasTouched)
		{
			LockManager.Instance.UnlockRequestLocal();
			return;
		}
		Vector3i blockPos = item.ToWorldPos();
		ITileEntityLootable selfOrFeature = GameManager.Instance.World.GetTileEntity(blockPos).GetSelfOrFeature<ITileEntityLootable>();
		if ((selfOrFeature == null || !selfOrFeature.IsRemoving) && selfOrFeature == item)
		{
			item.bTouched = false;
			item.SetModified();
		}
		LockManager.Instance.UnlockRequestLocal();
	}

	public void OpenLooting(string _lootContainerName, ITileEntityLootable _te)
	{
		if (EffectManager.GetValue(PassiveEffects.DisableLoot, null, 0f, xui.playerUI.entityPlayer, null, _te.blockValue.Block.Tags) > 0f)
		{
			Manager.PlayInsidePlayerHead("twitch_no_attack");
			LockManager.Instance.UnlockRequestLocal();
			return;
		}
		if (_te.bWasTouched)
		{
			openContainer(_lootContainerName, _te, _playOpenSound: true);
			return;
		}
		EntityPlayer entityPlayer = xui.playerUI.entityPlayer;
		float openTime = LootContainer.GetLootContainer(_te.lootListName).openTime;
		float num = EffectManager.GetValue(PassiveEffects.ScavengingTime, null, entityPlayer.IsCrouching ? (openTime * 1.5f) : openTime, entityPlayer) * LootContainer.LootTimerModifier;
		if (num == 0f)
		{
			num = 0.01f;
		}
		TimerEventData timerEventData = new TimerEventData();
		timerEventData.CloseEvent += openTimerClosedManually;
		timerEventData.FullTimeFinishEvent += openTimerFinished;
		timerEventData.Data = (_lootContainerName, _te);
		timerEventData.CloseOnHit = true;
		timerEventData.CancelWithActivateButton = true;
		XUiC_Timer.OpenTimer(xui, num, timerEventData, -1f, Localization.Get("xuiOpeningLoot"));
		string text = LootContainer.GetLootContainer(_te.lootListName)?.soundOpen;
		if (!string.IsNullOrEmpty(text))
		{
			Manager.BroadcastPlayByLocalPlayer(_te.ToWorldPos().ToVector3() + Vector3.one * 0.5f, text);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_LootWindowGroup GetInstance(XUi _xuiInstance = null)
	{
		if ((object)_xuiInstance == null)
		{
			_xuiInstance = LocalPlayerUI.GetUIForPrimaryPlayer().xui;
		}
		return (XUiC_LootWindowGroup)_xuiInstance.FindWindowGroupByName(ID);
	}

	public static Vector3i GetTeBlockPos(XUi _xuiInstance = null)
	{
		return GetInstance(_xuiInstance).te?.ToWorldPos() ?? Vector3i.zero;
	}

	public static void CloseIfOpenAtPos(Vector3i _blockPos, XUi _xuiInstance = null)
	{
		GUIWindowManager windowManager = GetInstance(_xuiInstance).xui.playerUI.windowManager;
		if (windowManager.IsWindowOpen(ID) && GetTeBlockPos() == _blockPos)
		{
			windowManager.Close(ID);
		}
	}
}
