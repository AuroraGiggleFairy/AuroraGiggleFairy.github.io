using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DialogWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DialogResponseList responseWindow;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Dialog CurrentDialog
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static void Open(XUi _xui, Action _onDialogClosed = null)
	{
		_xui.playerUI.windowManager.Open("dialog", _bModal: true);
		if (_onDialogClosed != null)
		{
			GUIWindow window = _xui.playerUI.windowManager.GetWindow("dialog");
			window.OnWindowClose = (Action)Delegate.Combine(window.OnWindowClose, _onDialogClosed);
		}
	}

	public override void Init()
	{
		base.Init();
		responseWindow = GetChildByType<XUiC_DialogResponseList>();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		xui.Dialog.DialogWindowGroup = this;
		xui.playerUI.entityPlayer.OverrideFOV = 30f;
		xui.playerUI.entityPlayer.OverrideLookAt = xui.Dialog.Respondent.getHeadPosition();
		xui.playerUI.windowManager.Close("windowpaging");
		xui.playerUI.windowManager.Close("toolbelt");
		CurrentDialog = Dialog.DialogList[xui.Dialog.Respondent.NPCInfo.DialogID];
		CurrentDialog.CurrentOwner = xui.Dialog.Respondent;
		if (xui.Dialog.ReturnStatement == "" || CurrentDialog.CurrentStatement == null)
		{
			CurrentDialog.RestartDialog(xui.playerUI.entityPlayer);
		}
		else if (xui.Dialog.ReturnStatement != "")
		{
			CurrentDialog.CurrentStatement = CurrentDialog.GetStatement(xui.Dialog.ReturnStatement);
			CurrentDialog.ChildDialog = null;
		}
		xui.Dialog.ReturnStatement = "";
		responseWindow.CurrentDialog = CurrentDialog;
		GameManager.Instance.SetToolTipPause(xui.playerUI.nguiWindowManager, _isPaused: true);
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.playerUI.windowManager.Close("questOffer");
		xui.playerUI.windowManager.Open("toolbelt", _bModal: false);
		xui.Dialog.Respondent = null;
		GameManager.Instance.SetToolTipPause(xui.playerUI.nguiWindowManager, _isPaused: false);
		if (!xui.Dialog.KeepZoomOnClose)
		{
			xui.playerUI.entityPlayer.OverrideFOV = -1f;
		}
	}

	public void RefreshDialog()
	{
		if (CurrentDialog.CurrentStatement != null)
		{
			responseWindow.Refresh();
		}
		else
		{
			xui.playerUI.windowManager.Close("dialog");
		}
	}

	public void ShowResponseWindow(bool isVisible)
	{
		responseWindow.ViewComponent.IsVisible = isVisible;
	}
}
