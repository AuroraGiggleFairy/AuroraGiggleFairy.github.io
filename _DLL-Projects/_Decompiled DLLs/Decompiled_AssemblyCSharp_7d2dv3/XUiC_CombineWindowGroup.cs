using UnityEngine.Scripting;

[Preserve]
public class XUiC_CombineWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CombineGrid combineGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureCombine te;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;

	public override void Init()
	{
		base.Init();
		if (GetChildByType<XUiC_WindowNonPagingHeader>() != null)
		{
			nonPagingHeaderWindow = GetChildByType<XUiC_WindowNonPagingHeader>();
		}
		combineGrid = GetChildByType<XUiC_CombineGrid>();
		AlwaysUpdate = true;
	}

	public void SetTileEntity(TEFeatureCombine _teCombine)
	{
		te = _teCombine;
		if (combineGrid != null)
		{
			combineGrid.SetTileEntity(_teCombine);
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
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		if (nonPagingHeaderWindow != null)
		{
			nonPagingHeaderWindow.SetHeader(Localization.Get("xuiCombineStation"));
		}
		_ = xui.playerUI.windowManager;
		xui.RecenterWindowGroup(windowGroup);
		for (int i = 0; i < children.Count; i++)
		{
			children[i].OnOpen();
		}
		IsDirty = true;
		xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
	}

	public override void OnClose()
	{
		wasReleased = false;
		activeKeyDown = false;
		base.OnClose();
		te.SetUserAccessing(_bUserAccessing: false);
		LockManager.Instance.UnlockRequestLocal();
		te = null;
		xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
	}
}
