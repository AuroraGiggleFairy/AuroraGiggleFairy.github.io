using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LootWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_LootWindow lootWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label headerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public ITileEntityLootable te;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lootContainerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpening;

	[PublicizedFrom(EAccessModifier.Private)]
	public float openTimeLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Timer timerWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public UISprite timerHourGlass;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isClosingFromDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lootingHeader;

	public static string ID = "looting";

	[PublicizedFrom(EAccessModifier.Private)]
	public float totalOpenTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ignoreCloseSound;

	public override void Init()
	{
		base.Init();
		openTimeLeft = 0f;
		lootWindow = GetChildByType<XUiC_LootWindow>();
		timerWindow = base.xui.GetChildByType<XUiC_Timer>();
		nonPagingHeaderWindow = GetChildByType<XUiC_WindowNonPagingHeader>();
	}

	public void SetTileEntityChest(string _lootContainerName, ITileEntityLootable _te)
	{
		lootContainerName = _lootContainerName;
		te = _te;
		lootWindow.SetTileEntityChest(_lootContainerName, _te);
		lootingHeader = Localization.Get("xuiLooting");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OpenContainer()
	{
		base.OnOpen();
		base.xui.playerUI.windowManager.OpenIfNotOpen("backpack", _bModal: false);
		lootWindow.ViewComponent.UiTransform.gameObject.SetActive(value: true);
		lootWindow.OpenContainer();
		if (nonPagingHeaderWindow != null)
		{
			nonPagingHeaderWindow.SetHeader(lootingHeader);
		}
		lootWindow.ViewComponent.IsVisible = true;
		base.xui.playerUI.windowManager.Close("timer");
		if (windowGroup.UseStackPanelAlignment)
		{
			base.xui.RecenterWindowGroup(windowGroup);
		}
		isOpening = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.xui.playerUI.entityPlayer != null && base.xui.playerUI.entityPlayer.hasBeenAttackedTime > 0 && isOpening)
		{
			GUIWindowManager windowManager = base.xui.playerUI.windowManager;
			windowManager.Close("timer");
			isOpening = false;
			isClosingFromDamage = true;
			windowManager.Close("looting");
		}
		else
		{
			if (!isOpening)
			{
				return;
			}
			if (te.bWasTouched || openTimeLeft <= 0f)
			{
				if (!te.bWasTouched && !te.bPlayerStorage && !te.bPlayerBackpack)
				{
					base.xui.playerUI.entityPlayer.Progression.AddLevelExp(base.xui.playerUI.entityPlayer.gameStage, "_xpFromLoot", Progression.XPTypes.Looting);
				}
				openTimeLeft = 0f;
				OpenContainer();
			}
			else
			{
				if (timerWindow != null)
				{
					float fillAmount = openTimeLeft / totalOpenTime;
					timerWindow.UpdateTimer(openTimeLeft, fillAmount);
				}
				openTimeLeft -= _dt;
			}
		}
	}

	public override void OnOpen()
	{
		isClosingFromDamage = false;
		if (te.EntityId != -1)
		{
			Entity entity = GameManager.Instance.World.GetEntity(te.EntityId);
			if (EffectManager.GetValue(PassiveEffects.DisableLoot, null, 0f, base.xui.playerUI.entityPlayer, null, entity.EntityClass.Tags) > 0f)
			{
				Manager.PlayInsidePlayerHead("twitch_no_attack");
				GUIWindowManager windowManager = base.xui.playerUI.windowManager;
				ignoreCloseSound = true;
				windowManager.Close("timer");
				isOpening = false;
				isClosingFromDamage = true;
				windowManager.Close("looting");
				return;
			}
		}
		else if (EffectManager.GetValue(PassiveEffects.DisableLoot, null, 0f, base.xui.playerUI.entityPlayer, null, te.blockValue.Block.Tags) > 0f)
		{
			Manager.PlayInsidePlayerHead("twitch_no_attack");
			GUIWindowManager windowManager2 = base.xui.playerUI.windowManager;
			ignoreCloseSound = true;
			windowManager2.Close("timer");
			isOpening = false;
			isClosingFromDamage = true;
			windowManager2.Close("looting");
			return;
		}
		ignoreCloseSound = false;
		base.xui.playerUI.windowManager.CloseIfOpen("backpack");
		lootWindow.ViewComponent.UiTransform.gameObject.SetActive(value: false);
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		float openTime = LootContainer.GetLootContainer(te.lootListName).openTime;
		totalOpenTime = (openTimeLeft = EffectManager.GetValue(PassiveEffects.ScavengingTime, null, entityPlayer.IsCrouching ? (openTime * 1.5f) : openTime, entityPlayer));
		if (nonPagingHeaderWindow != null)
		{
			nonPagingHeaderWindow.SetHeader("LOOTING");
		}
		base.xui.playerUI.windowManager.OpenIfNotOpen("CalloutGroup", _bModal: false);
		base.xui.playerUI.windowManager.Open("timer", _bModal: false);
		timerWindow = base.xui.GetChildByType<XUiC_Timer>();
		timerWindow.currentOpenEventText = Localization.Get("xuiOpeningLoot");
		isOpening = true;
		LootContainer lootContainer = LootContainer.GetLootContainer(te.lootListName);
		if (lootContainer == null || lootContainer.soundClose == null)
		{
			return;
		}
		Vector3 position = te.ToWorldPos().ToVector3() + Vector3.one * 0.5f;
		if (te.EntityId != -1 && GameManager.Instance.World != null)
		{
			Entity entity2 = GameManager.Instance.World.GetEntity(te.EntityId);
			if (entity2 != null)
			{
				position = entity2.GetPosition();
			}
		}
		Manager.BroadcastPlayByLocalPlayer(position, lootContainer.soundOpen);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.CloseIfOpen("backpack");
		Vector3i blockPos = te.ToWorldPos();
		if (isOpening)
		{
			base.xui.playerUI.windowManager.Close("timer");
		}
		if (openTimeLeft > 0f && !te.bWasTouched)
		{
			ITileEntityLootable selfOrFeature = GameManager.Instance.World.GetTileEntity(te.GetClrIdx(), blockPos).GetSelfOrFeature<ITileEntityLootable>();
			if ((selfOrFeature == null || !selfOrFeature.IsRemoving) && selfOrFeature == te)
			{
				te.bTouched = false;
				te.SetModified();
			}
		}
		lootWindow.CloseContainer(ignoreCloseSound);
		lootWindow.ViewComponent.IsVisible = false;
		isOpening = false;
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
