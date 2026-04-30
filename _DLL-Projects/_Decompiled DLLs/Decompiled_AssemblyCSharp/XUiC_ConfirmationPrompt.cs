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

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<Result> resultHandler;

	[PublicizedFrom(EAccessModifier.Private)]
	public string headerText;

	[PublicizedFrom(EAccessModifier.Private)]
	public string bodyText;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cancelText;

	[PublicizedFrom(EAccessModifier.Private)]
	public string confirmText;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnConfirm;

	public bool IsVisible => viewComponent.IsVisible;

	public override void Init()
	{
		base.Init();
		viewComponent.IsVisible = false;
		btnCancel = (XUiC_SimpleButton)GetChildById("btnPromptCancel");
		btnConfirm = (XUiC_SimpleButton)GetChildById("btnPromptConfirm");
		btnCancel.OnPressed += BtnCancel_OnPress;
		btnConfirm.OnPressed += BtnConfirm_OnPress;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		viewComponent.IsVisible = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirm_OnPress(XUiController _sender, int _mouseButton)
	{
		Confirm();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPress(XUiController _sender, int _mouseButton)
	{
		Cancel();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings(_forceAll: true);
			IsDirty = false;
		}
	}

	public void ShowPrompt(string headerText, string bodyText, string cancelText, string confirmText, Action<Result> callback)
	{
		this.headerText = headerText;
		this.bodyText = bodyText;
		this.cancelText = cancelText;
		this.confirmText = confirmText;
		resultHandler = callback;
		viewComponent.IsVisible = true;
		base.xui.playerUI.CursorController.SetNavigationLockView(viewComponent, btnCancel.ViewComponent);
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "headertext":
			_value = headerText;
			return true;
		case "bodytext":
			_value = bodyText;
			return true;
		case "canceltext":
			_value = cancelText;
			return true;
		case "confirmtext":
			_value = confirmText;
			return true;
		case "confirmvisible":
			_value = (!string.IsNullOrWhiteSpace(confirmText)).ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClosePrompt()
	{
		base.xui.playerUI.CursorController.SetNavigationLockView(null);
		viewComponent.IsVisible = false;
	}

	public void Confirm()
	{
		if (IsVisible)
		{
			ClosePrompt();
			resultHandler(Result.Confirmed);
		}
	}

	public void Cancel()
	{
		if (IsVisible)
		{
			ClosePrompt();
			resultHandler(Result.Cancelled);
		}
	}
}
