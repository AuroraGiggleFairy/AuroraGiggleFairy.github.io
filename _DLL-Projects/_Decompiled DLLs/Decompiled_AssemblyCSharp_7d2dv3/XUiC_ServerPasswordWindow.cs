using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerPasswordWindow : XUiController
{
	[XuiBindComponent("txtPassword", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtPassword;

	[XuiBindComponent("btnSubmit", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnSubmit;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<string> onSubmit;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInvalidPassword;

	[XuiXmlBinding("isinvalidpassword")]
	public bool IsInvalidPassword
	{
		get
		{
			return isInvalidPassword;
		}
		set
		{
			isInvalidPassword = value;
			IsDirty = true;
		}
	}

	[XuiBindEvent("OnSubmitHandler", "txtPassword")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtPassword_OnSubmitHandler(XUiController _sender, string _text)
	{
		BtnSubmit_OnPressed(_sender, -1);
	}

	[XuiBindEvent("OnPress", "btnSubmit")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSubmit_OnPressed(XUiController _sender, int _mouseButton)
	{
		Action<string> action = onSubmit;
		string text = txtPassword.Text;
		xui.playerUI.windowManager.Close(base.WindowGroup);
		action?.Invoke(text);
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		Action action = onCancel;
		xui.playerUI.windowManager.Close(base.WindowGroup);
		action?.Invoke();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
		if (XUiUtils.HotkeysAllowedFor(viewComponent))
		{
			if (xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
			{
				BtnCancel_OnPressed(this, -1);
			}
			if (xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
			{
				BtnSubmit_OnPressed(this, -1);
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		txtPassword.SetSelected(_selected: true, _delayed: true);
	}

	public override void OnClose()
	{
		base.OnClose();
		onCancel = null;
		onSubmit = null;
		txtPassword.Text = "";
	}

	public static void OpenPasswordWindow(XUi _xuiInstance, bool _badPassword, string _currentPwd, bool _modal, Action<string> _onSubmitDelegate, Action _onCancelDelegate)
	{
		XUiC_ServerPasswordWindow childByType = _xuiInstance.GetChildByType<XUiC_ServerPasswordWindow>();
		childByType.txtPassword.Text = _currentPwd;
		childByType.onSubmit = _onSubmitDelegate;
		childByType.onCancel = _onCancelDelegate;
		childByType.IsInvalidPassword = _badPassword;
		_xuiInstance.playerUI.windowManager.Open(childByType.windowGroup, _modal, _bIsNotEscClosable: true);
	}
}
