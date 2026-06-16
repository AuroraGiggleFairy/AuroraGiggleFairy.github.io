using System;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BagStorageWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_BagContainer containerWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Timer timerWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool playOnOpenSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ignoreCloseSound;

	public static string ID = "bagStorage";

	public Entity Entity;

	[PublicizedFrom(EAccessModifier.Private)]
	public LootContainer lootContainer;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onCloseCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<bool> checkInteraction;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Bag Bag
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Init()
	{
		base.Init();
		containerWindow = GetChildByType<XUiC_BagContainer>();
		nonPagingHeaderWindow = GetChildByType<XUiC_WindowNonPagingHeader>();
		timerWindow = xui.GetChildByType<XUiC_Timer>();
	}

	public static void Open(XUi _xui, Entity _entity, Bag _bag, LootContainer _lootContainer, string _containerName = null, Action _onModified = null, Action _onClose = null, Func<bool> _interactionCheck = null, bool _runOpenTimer = false)
	{
		if (_containerName == null)
		{
			_containerName = Localization.Get("xuiLoot");
		}
		XUiC_BagStorageWindowGroup ctrl = _xui.FindWindowGroupByName(ID).GetChildByType<XUiC_BagStorageWindowGroup>();
		ctrl.Entity = _entity;
		ctrl.Bag = _bag;
		ctrl.lootContainer = _lootContainer;
		ctrl.onCloseCallback = _onClose;
		ctrl.checkInteraction = _interactionCheck;
		ctrl.playOnOpenSound = !_runOpenTimer;
		ctrl.containerWindow.SetBag(_bag, _lootContainer, _containerName, _onModified);
		if (!_runOpenTimer || _lootContainer == null)
		{
			ctrl.OpenStorageWindow();
			return;
		}
		EntityPlayer entityPlayer = _xui.playerUI.entityPlayer;
		float num = EffectManager.GetValue(PassiveEffects.ScavengingTime, null, (entityPlayer != null && entityPlayer.IsCrouching) ? (_lootContainer.openTime * 1.5f) : _lootContainer.openTime, entityPlayer) * LootContainer.LootTimerModifier;
		if (num == 0f)
		{
			num = 0.01f;
		}
		if (num <= 0f)
		{
			ctrl.OpenStorageWindow();
			return;
		}
		_xui.playerUI.windowManager.Close("backpack");
		_xui.playerUI.windowManager.Open("CalloutGroup", _bModal: false);
		TimerEventData timerEventData = new TimerEventData
		{
			CloseOnHit = true
		};
		timerEventData.FullTimeFinishEvent += [PublicizedFrom(EAccessModifier.Internal)] (TimerEventData _) =>
		{
			ctrl.OpenStorageWindow();
		};
		timerEventData.CloseEvent += [PublicizedFrom(EAccessModifier.Internal)] (TimerEventData _) =>
		{
			ctrl.CancelOpen();
		};
		XUiC_Timer.OpenTimer(_xui, num, timerEventData, -1f, Localization.Get("xuiOpeningLoot"), _modal: false);
		if (!string.IsNullOrEmpty(ctrl.lootContainer.soundOpen))
		{
			Manager.BroadcastPlayByLocalPlayer(XUiM_Player.GetPlayer().position, ctrl.lootContainer.soundOpen);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenStorageWindow()
	{
		xui.playerUI.windowManager.Open(ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CancelOpen()
	{
		onCloseCallback?.Invoke();
		ClearPendingState();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (checkInteraction != null && !checkInteraction())
		{
			xui.playerUI.windowManager.Close(ID);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		ignoreCloseSound = false;
		EntityPlayer entityPlayer = xui.playerUI.entityPlayer;
		if (EffectManager.GetValue(PassiveEffects.DisableLoot, null, 0f, entityPlayer, null, Entity.EntityClass.Tags) > 0f)
		{
			Manager.PlayInsidePlayerHead("twitch_no_attack");
			GUIWindowManager windowManager = xui.playerUI.windowManager;
			ignoreCloseSound = true;
			windowManager.Close("timer");
			windowManager.Close(ID);
			return;
		}
		if (nonPagingHeaderWindow != null)
		{
			nonPagingHeaderWindow.SetHeader(Localization.Get("xuiStorage"));
		}
		if (lootContainer != null)
		{
			xui.playerUI.windowManager.Open("backpack", _bModal: false);
			if (containerWindow?.ViewComponent != null)
			{
				containerWindow.ViewComponent.UiTransform.gameObject.SetActive(value: true);
				containerWindow.ViewComponent.IsVisible = true;
			}
			xui.RecenterWindowGroup(windowGroup);
			if (playOnOpenSound && !string.IsNullOrEmpty(lootContainer.soundOpen))
			{
				Manager.BroadcastPlayByLocalPlayer(XUiM_Player.GetPlayer().position, lootContainer.soundOpen);
			}
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.playerUI.windowManager.Close("backpack");
		if (!ignoreCloseSound && lootContainer != null && !string.IsNullOrEmpty(lootContainer.soundOpen))
		{
			Manager.BroadcastPlayByLocalPlayer(XUiM_Player.GetPlayer().position, lootContainer.soundOpen);
		}
		onCloseCallback?.Invoke();
		ClearPendingState();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearPendingState()
	{
		ignoreCloseSound = false;
		playOnOpenSound = false;
		if (containerWindow?.ViewComponent != null)
		{
			containerWindow.ViewComponent.UiTransform.gameObject.SetActive(value: true);
			containerWindow.ViewComponent.IsVisible = true;
		}
		Bag = null;
		lootContainer = null;
		onCloseCallback = null;
		checkInteraction = null;
	}
}
