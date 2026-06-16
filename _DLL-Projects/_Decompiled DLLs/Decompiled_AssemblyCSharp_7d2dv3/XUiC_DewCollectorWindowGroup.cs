using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DewCollectorWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DewCollectorWindow dewCatcherWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DewCollectorModGrid dewCollectorModGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label headerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DewCollectorStack[] collectorStacks;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityCollector te;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController fuelWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label fuelWindowTitle;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite fuelWindowSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CollectorFuelGrid collectorFuelGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController catalystWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label catalystWindowTitle;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite catalystWindowSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CollectorFuelGrid collectorCatalyst;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lootingHeader;

	public static string ID = "dewcollector";

	[PublicizedFrom(EAccessModifier.Private)]
	public float totalOpenTime;

	public override void Init()
	{
		base.Init();
		dewCatcherWindow = GetChildByType<XUiC_DewCollectorWindow>();
		dewCollectorModGrid = GetChildByType<XUiC_DewCollectorModGrid>();
		collectorStacks = GetChildrenByType<XUiC_DewCollectorStack>();
		fuelWindow = GetChildById("windowCollectorFuel");
		fuelWindowTitle = fuelWindow.GetChildById("windowCollectorFuelTitle").ViewComponent as XUiV_Label;
		fuelWindowSprite = fuelWindow.GetChildById("windowCollectorFuelSprite").ViewComponent as XUiV_Sprite;
		collectorFuelGrid = fuelWindow.GetChildByType<XUiC_CollectorFuelGrid>();
		catalystWindow = GetChildById("windowCollectorCatalyst");
		catalystWindowTitle = catalystWindow.GetChildById("windowCollectorFuelTitle").ViewComponent as XUiV_Label;
		catalystWindowSprite = catalystWindow.GetChildById("windowCollectorFuelSprite").ViewComponent as XUiV_Sprite;
		collectorCatalyst = catalystWindow.GetChildByType<XUiC_CollectorFuelGrid>();
		nonPagingHeaderWindow = GetChildByType<XUiC_WindowNonPagingHeader>();
	}

	public void SetTileEntity(TileEntityCollector _te)
	{
		te = _te;
		BlockCollector blockCollector = te.blockValue.Block as BlockCollector;
		fuelWindowTitle.Text = Localization.Get(blockCollector.FuelTitleText);
		fuelWindowSprite.UIAtlas = blockCollector.FuelTitleSpriteAtlas;
		fuelWindowSprite.SpriteName = blockCollector.FuelTitleSprite;
		catalystWindowTitle.Text = Localization.Get(blockCollector.CatalystTitleText);
		catalystWindowSprite.UIAtlas = blockCollector.CatalystTitleSpriteAtlas;
		catalystWindowSprite.SpriteName = blockCollector.CatalystTitleSprite;
		dewCatcherWindow.SetTileEntity(_te);
		dewCollectorModGrid.SetTileEntity(_te);
		collectorFuelGrid.SetTileEntity(_te);
		collectorCatalyst.SetTileEntity(_te);
		int num = collectorStacks.Length;
		for (int i = 0; i < num; i++)
		{
			XUiC_DewCollectorStack obj = collectorStacks[i];
			obj.SetStandardFillColor(blockCollector.ItemBackgroundColor);
			obj.SetSprite(blockCollector.ItemIconBackdrop[i % blockCollector.ItemIconBackdrop.Length]);
		}
		lootingHeader = Localization.Get(blockCollector.LootLabelKey);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OpenContainer()
	{
		base.OnOpen();
		xui.playerUI.windowManager.Open("backpack", _bModal: false);
		dewCatcherWindow.ViewComponent.IsVisible = true;
		dewCatcherWindow.OpenContainer();
		if (nonPagingHeaderWindow != null)
		{
			nonPagingHeaderWindow.SetHeader(lootingHeader);
		}
		dewCatcherWindow.ViewComponent.IsVisible = true;
	}

	public override void OnOpen()
	{
		_ = xui.playerUI.entityPlayer;
		if (nonPagingHeaderWindow != null)
		{
			nonPagingHeaderWindow.SetHeader("LOOTING");
		}
		OpenContainer();
		if (te.blockValue.Block is BlockCollector blockCollector)
		{
			fuelWindow.ViewComponent.IsVisible = blockCollector.UsesFuel();
			catalystWindow.ViewComponent.IsVisible = blockCollector.UsesCatalyst();
			string openSound = blockCollector.OpenSound;
			Manager.BroadcastPlayByLocalPlayer(te.ToWorldPos().ToVector3() + Vector3.one * 0.5f, openSound);
			doEvent(blockCollector.ActivationEvent);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.playerUI.windowManager.Close("backpack");
		te.ToWorldPos();
		te.SetUserAccessing(_bUserAccessing: false);
		te.SetModified();
		LockManager.Instance.UnlockRequestLocal();
		if (te.blockValue.Block != null && te.blockValue.Block is BlockCollector { CloseSound: var closeSound } blockCollector)
		{
			Manager.BroadcastPlayByLocalPlayer(te.ToWorldPos().ToVector3() + Vector3.one * 0.5f, closeSound);
			_ = blockCollector.RunningSound;
			doEvent(blockCollector.CloseEvent);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doEvent(string activationEvent)
	{
		if (string.IsNullOrEmpty(activationEvent))
		{
			return;
		}
		List<Tuple<string, string>> list = new List<Tuple<string, string>>();
		int num = 1;
		ItemStack[] modSlots = te.ModSlots;
		foreach (ItemStack itemStack in modSlots)
		{
			string item = "";
			if (!itemStack.IsEmpty())
			{
				item = itemStack.itemValue.ItemClass.Name;
			}
			list.Add(new Tuple<string, string>($"_Mod{num++}Name", item));
		}
		GameEventManager.Current.HandleAction(activationEvent, null, null, twitchActivated: false, te.ToWorldCenterPos(), "", "", crateShare: false, allowRefunds: true, "", null, list);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_DewCollectorWindowGroup GetInstance(XUi _xuiInstance = null)
	{
		if ((object)_xuiInstance == null)
		{
			_xuiInstance = LocalPlayerUI.GetUIForPrimaryPlayer().xui;
		}
		return (XUiC_DewCollectorWindowGroup)_xuiInstance.FindWindowGroupByName(ID);
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
