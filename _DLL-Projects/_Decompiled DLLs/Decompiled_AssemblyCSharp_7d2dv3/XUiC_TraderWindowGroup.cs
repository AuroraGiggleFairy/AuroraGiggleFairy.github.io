using UnityEngine.Scripting;

[Preserve]
public class XUiC_TraderWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeader;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TraderWindow TraderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServiceInfoWindow ServiceInfoWindow;

	public static string ID = "trader";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;

	public override void Init()
	{
		base.Init();
		XUiController childByType = GetChildByType<XUiC_WindowNonPagingHeader>();
		if (childByType != null)
		{
			nonPagingHeader = (XUiC_WindowNonPagingHeader)childByType;
		}
		childByType = GetChildByType<XUiC_TraderWindow>();
		if (childByType != null)
		{
			TraderWindow = (XUiC_TraderWindow)childByType;
		}
		childByType = GetChildByType<XUiC_ServiceInfoWindow>();
		if (childByType != null)
		{
			ServiceInfoWindow = (XUiC_ServiceInfoWindow)childByType;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!windowGroup.isShowing)
		{
			return;
		}
		if (!xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
		{
			wasReleased = true;
		}
		if (!wasReleased)
		{
			return;
		}
		if (xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
		{
			activeKeyDown = true;
		}
		if (xui.playerUI.playerInput.PermanentActions.Activate.WasReleased && activeKeyDown)
		{
			activeKeyDown = false;
			if (!xui.playerUI.windowManager.IsInputActive())
			{
				xui.playerUI.windowManager.CloseAllOpenModalWindows();
			}
		}
	}

	public override void OnOpen()
	{
		xui.Trader.TraderWindowGroup = this;
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		if (nonPagingHeader != null)
		{
			nonPagingHeader.SetHeader((xui.Trader.Trader is TileEntityVendingMachine) ? Localization.Get("xuiVending") : Localization.Get("xuiTrader"));
		}
		xui.RecenterWindowGroup(windowGroup);
		for (int i = 0; i < children.Count; i++)
		{
			children[i].OnOpen();
		}
		if (ServiceInfoWindow != null)
		{
			ServiceInfoWindow.ViewComponent.IsVisible = false;
		}
		if (xui.Trader.Trader is EntityTrader entityTrader && xui.playerUI.entityPlayer.OverrideFOV != 30f)
		{
			xui.playerUI.entityPlayer.OverrideFOV = 30f;
			xui.playerUI.entityPlayer.OverrideLookAt = entityTrader.getHeadPosition();
		}
		xui.Dialog.KeepZoomOnClose = false;
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		wasReleased = false;
		activeKeyDown = false;
		xui.Trader.TraderWindowGroup = null;
		xui.playerUI.entityPlayer.OverrideFOV = -1f;
	}

	public void RefreshTraderItems()
	{
		TraderWindow.CompletedTransaction = true;
		TraderWindow.RefreshTraderItems();
	}

	public void RefreshTraderWindow()
	{
		TraderWindow.RefreshBindings();
	}
}
