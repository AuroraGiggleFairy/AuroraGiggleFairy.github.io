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
		if (!base.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
		{
			wasReleased = true;
		}
		if (!wasReleased)
		{
			return;
		}
		if (base.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
		{
			activeKeyDown = true;
		}
		if (base.xui.playerUI.playerInput.PermanentActions.Activate.WasReleased && activeKeyDown)
		{
			activeKeyDown = false;
			if (!base.xui.playerUI.windowManager.IsInputActive())
			{
				base.xui.playerUI.windowManager.CloseAllOpenWindows();
			}
		}
	}

	public override bool AlwaysUpdate()
	{
		return false;
	}

	public override void OnOpen()
	{
		base.xui.Trader.TraderWindowGroup = this;
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		if (nonPagingHeader != null)
		{
			nonPagingHeader.SetHeader((base.xui.Trader.TraderTileEntity.entityId == -1) ? Localization.Get("xuiVending") : Localization.Get("xuiTrader"));
		}
		base.xui.RecenterWindowGroup(windowGroup);
		for (int i = 0; i < children.Count; i++)
		{
			children[i].OnOpen();
		}
		if (ServiceInfoWindow != null)
		{
			ServiceInfoWindow.ViewComponent.IsVisible = false;
		}
		if (base.xui.Trader.TraderTileEntity.entityId != -1 && base.xui.playerUI.entityPlayer.OverrideFOV != 30f)
		{
			base.xui.playerUI.entityPlayer.OverrideFOV = 30f;
			base.xui.playerUI.entityPlayer.OverrideLookAt = base.xui.Trader.TraderEntity.getHeadPosition();
		}
		base.xui.Dialog.keepZoomOnClose = false;
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		wasReleased = false;
		activeKeyDown = false;
		base.xui.Trader.TraderWindowGroup = null;
		base.xui.playerUI.entityPlayer.OverrideFOV = -1f;
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
