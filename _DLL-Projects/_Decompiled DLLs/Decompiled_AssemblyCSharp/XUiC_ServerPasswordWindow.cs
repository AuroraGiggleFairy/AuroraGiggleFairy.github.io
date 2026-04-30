using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerPasswordWindow : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtPassword;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblPassNormal;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblPassIncorrect;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<string> onSubmit;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onCancel;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "msgText")
		{
			return true;
		}
		return false;
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		lblPassNormal = (XUiV_Label)GetChildById("lblPassNormal").ViewComponent;
		lblPassIncorrect = (XUiV_Label)GetChildById("lblPassIncorrect").ViewComponent;
		txtPassword = (XUiC_TextInput)GetChildById("txtPassword");
		txtPassword.OnSubmitHandler += TxtPassword_OnSubmitHandler;
		((XUiC_SimpleButton)GetChildById("btnCancel")).OnPressed += BtnCancel_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnSubmit")).OnPressed += BtnSubmit_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtPassword_OnSubmitHandler(XUiController _sender, string _text)
	{
		BtnSubmit_OnPressed(_sender, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSubmit_OnPressed(XUiController _sender, int _mouseButton)
	{
		onCancel = null;
		onSubmit(txtPassword.Text);
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		Action action = onCancel;
		onCancel = null;
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
		action?.Invoke();
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

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.playerUI.CursorController.SetNavigationLockView(viewComponent);
	}

	public override void OnClose()
	{
		base.OnClose();
		if (onCancel != null)
		{
			BtnCancel_OnPressed(this, -1);
		}
		base.xui.playerUI.CursorController.SetNavigationLockView(null);
		txtPassword.Text = "";
	}

	public override void UpdateInput()
	{
		base.UpdateInput();
		if (base.xui.playerUI.playerInput != null && base.xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
		{
			base.xui.playerUI.windowManager.Close(windowGroup.ID);
		}
	}

	public static void OpenPasswordWindow(XUi _xuiInstance, bool _badPassword, string _currentPwd, bool _modal, Action<string> _onSubmitDelegate, Action _onCancelDelegate)
	{
		XUiC_ServerPasswordWindow childByType = _xuiInstance.FindWindowGroupByName(ID).GetChildByType<XUiC_ServerPasswordWindow>();
		childByType.txtPassword.Text = _currentPwd;
		_xuiInstance.playerUI.windowManager.Open(ID, _modal);
		childByType.onSubmit = _onSubmitDelegate;
		childByType.onCancel = _onCancelDelegate;
		if (_badPassword)
		{
			childByType.lblPassNormal.IsVisible = false;
			childByType.lblPassIncorrect.IsVisible = true;
		}
		else
		{
			childByType.lblPassNormal.IsVisible = true;
			childByType.lblPassIncorrect.IsVisible = false;
		}
	}
}
