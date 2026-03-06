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
		base.xui.Dialog.DialogWindowGroup = this;
		base.xui.playerUI.entityPlayer.OverrideFOV = 30f;
		base.xui.playerUI.entityPlayer.OverrideLookAt = base.xui.Dialog.Respondent.getHeadPosition();
		base.xui.playerUI.windowManager.CloseIfOpen("windowpaging");
		base.xui.playerUI.windowManager.CloseIfOpen("toolbelt");
		CurrentDialog = Dialog.DialogList[base.xui.Dialog.Respondent.NPCInfo.DialogID];
		CurrentDialog.CurrentOwner = base.xui.Dialog.Respondent;
		if (base.xui.Dialog.ReturnStatement == "" || CurrentDialog.CurrentStatement == null)
		{
			CurrentDialog.RestartDialog(base.xui.playerUI.entityPlayer);
		}
		else if (base.xui.Dialog.ReturnStatement != "")
		{
			CurrentDialog.CurrentStatement = CurrentDialog.GetStatement(base.xui.Dialog.ReturnStatement);
			CurrentDialog.ChildDialog = null;
		}
		base.xui.Dialog.ReturnStatement = "";
		responseWindow.CurrentDialog = CurrentDialog;
		GameManager.Instance.SetToolTipPause(base.xui.playerUI.nguiWindowManager, _isPaused: true);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.CloseIfOpen("questOffer");
		base.xui.playerUI.windowManager.OpenIfNotOpen("toolbelt", _bModal: false);
		base.xui.Dialog.Respondent = null;
		GameManager.Instance.SetToolTipPause(base.xui.playerUI.nguiWindowManager, _isPaused: false);
		if (!base.xui.Dialog.keepZoomOnClose)
		{
			base.xui.playerUI.entityPlayer.OverrideFOV = -1f;
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
			base.xui.playerUI.windowManager.Close("dialog");
		}
	}

	public void ShowResponseWindow(bool isVisible)
	{
		responseWindow.ViewComponent.IsVisible = isVisible;
	}
}
