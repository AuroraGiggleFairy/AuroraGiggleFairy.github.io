using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ConfirmationPrompt : XUiController
{
	public enum Result
	{
		Cancelled,
		Confirmed
	}

	[XuiBindComponent("btnPromptCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[XuiBindComponent("btnPromptConfirm", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnConfirm;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<Result> resultHandler;

	[XuiXmlBinding("headertext")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string HeaderText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "";

	[XuiXmlBinding("bodytext")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string BodyText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "";

	[XuiXmlBinding("canceltext")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string CancelText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "";

	[XuiXmlBinding("confirmtext")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string ConfirmText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "";

	[XuiXmlBinding("cancelvisible")]
	public bool CancelVisible
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !string.IsNullOrWhiteSpace(CancelText);
		}
	}

	[XuiXmlBinding("confirmvisible")]
	public bool ConfirmVisible
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !string.IsNullOrWhiteSpace(ConfirmText);
		}
	}

	public bool IsVisible => viewComponent.IsVisible;

	public override void Init()
	{
		base.Init();
		viewComponent.IsVisible = false;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		viewComponent.IsVisible = false;
	}

	[XuiBindEvent("OnPress", "btnConfirm")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirm_OnPress(XUiController _sender, int _mouseButton)
	{
		Confirm();
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPress(XUiController _sender, int _mouseButton)
	{
		Cancel();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (XUiUtils.HotkeysAllowedFor(viewComponent) && xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
		{
			ThreadManager.RunTaskAfterFrames(Cancel);
		}
		handleDirtyUpdateDefault();
	}

	public void ShowPrompt(string _headerText, string _bodyText, string _cancelText, string _confirmText, Action<Result> _callback)
	{
		HeaderText = _headerText;
		BodyText = _bodyText;
		CancelText = _cancelText;
		ConfirmText = _confirmText;
		resultHandler = _callback;
		viewComponent.IsVisible = true;
		XUiView viewToSelect = ((!string.IsNullOrWhiteSpace(_cancelText)) ? btnCancel.ViewComponent : ((!string.IsNullOrWhiteSpace(_confirmText)) ? btnConfirm.ViewComponent : null));
		xui.playerUI.CursorController.SetNavigationLockView(viewComponent, viewToSelect);
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closePrompt()
	{
		xui.playerUI.CursorController.SetNavigationLockView(null);
		viewComponent.IsVisible = false;
	}

	public void Confirm()
	{
		if (IsVisible)
		{
			closePrompt();
			resultHandler?.Invoke(Result.Confirmed);
		}
	}

	public void Cancel()
	{
		if (IsVisible)
		{
			closePrompt();
			resultHandler?.Invoke(Result.Cancelled);
		}
	}
}
