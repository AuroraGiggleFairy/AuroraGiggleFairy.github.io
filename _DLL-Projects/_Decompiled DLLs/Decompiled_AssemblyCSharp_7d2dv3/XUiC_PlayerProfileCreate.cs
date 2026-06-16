using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayerProfileCreate : XUiController
{
	[XuiBindComponent("createProfileName", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput createProfileName;

	[XuiBindComponent("btnConfirm", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button createProfileConfirm;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button createProfileCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool createNameValid;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<string> onClosed;

	[XuiXmlBinding("create_name_valid")]
	public bool CreateNameValid
	{
		get
		{
			return createNameValid;
		}
		set
		{
			if (createNameValid != value)
			{
				createNameValid = value;
				IsDirty = true;
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		createProfileName.Text = "";
		createProfileName.SelectOrVirtualKeyboard();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
		if (xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
		{
			BtnCancelCreate_OnPressed(this, -1);
		}
		if (xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
		{
			BtnConfirmCreate_OnPressed(this, -1);
		}
	}

	[XuiBindEvent("OnPress", "createProfileCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancelCreate_OnPressed(XUiController _sender, int _mouseButton)
	{
		xui.playerUI.windowManager.Close(windowGroup);
		onClosed?.Invoke(null);
	}

	[XuiBindEvent("OnPress", "createProfileConfirm")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirmCreate_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (CreateNameValid)
		{
			xui.playerUI.windowManager.Close(windowGroup);
			onClosed?.Invoke(createProfileName.Text.Trim());
		}
	}

	[XuiBindEvent("OnSubmitHandler", "createProfileName")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateProfileName_OnSubmitHandler(XUiController _sender, string _text)
	{
		if (CreateNameValid)
		{
			BtnConfirmCreate_OnPressed(this, -1);
		}
	}

	[XuiBindEvent("OnChangeHandler", "createProfileName")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateProfileName_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		string text = _text.Trim();
		CreateNameValid = text.Length > 0 && text.IndexOf('.') < 0 && !ProfileSDF.ProfileExists(text);
	}

	public static void Open(XUi _xui, Action<string> _onClosed)
	{
		XUiC_PlayerProfileCreate childByType = _xui.GetChildByType<XUiC_PlayerProfileCreate>();
		childByType.onClosed = _onClosed;
		_xui.playerUI.windowManager.Open(childByType.windowGroup, _bModal: false);
	}
}
