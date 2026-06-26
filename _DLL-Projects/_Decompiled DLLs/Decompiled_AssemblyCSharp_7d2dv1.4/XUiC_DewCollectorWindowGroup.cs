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
	public TileEntityDewCollector te;

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
		nonPagingHeaderWindow = GetChildByType<XUiC_WindowNonPagingHeader>();
	}

	public void SetTileEntity(TileEntityDewCollector _te)
	{
		te = _te;
		dewCatcherWindow.SetTileEntity(_te);
		dewCollectorModGrid.SetTileEntity(_te);
		lootingHeader = Localization.Get("xuiDewCollector");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OpenContainer()
	{
		base.OnOpen();
		base.xui.playerUI.windowManager.OpenIfNotOpen("backpack", _bModal: false);
		dewCatcherWindow.ViewComponent.UiTransform.gameObject.SetActive(value: true);
		dewCatcherWindow.OpenContainer();
		if (nonPagingHeaderWindow != null)
		{
			nonPagingHeaderWindow.SetHeader(lootingHeader);
		}
		dewCatcherWindow.ViewComponent.IsVisible = true;
		if (windowGroup.UseStackPanelAlignment)
		{
			base.xui.RecenterWindowGroup(windowGroup);
		}
	}

	public override void OnOpen()
	{
		_ = base.xui.playerUI.entityPlayer;
		if (nonPagingHeaderWindow != null)
		{
			nonPagingHeaderWindow.SetHeader("LOOTING");
		}
		OpenContainer();
		if (te.blockValue.Block is BlockDewCollector { OpenSound: var openSound })
		{
			Manager.BroadcastPlayByLocalPlayer(te.ToWorldPos().ToVector3() + Vector3.one * 0.5f, openSound);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.CloseIfOpen("backpack");
		te.ToWorldPos();
		if (te.blockValue.Block != null && te.blockValue.Block is BlockDewCollector { CloseSound: var closeSound })
		{
			Manager.BroadcastPlayByLocalPlayer(te.ToWorldPos().ToVector3() + Vector3.one * 0.5f, closeSound);
		}
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
