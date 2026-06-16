using System;
using SandboxOptions;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SandboxSettingsSaveAsPreset : XUiController
{
	[XuiBindComponent("presetName", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtPresetName;

	[XuiBindComponent("description", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtDescription;

	[XuiBindComponent("btnConfirm", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnConfirm;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SandboxOptionManager sandboxManager = SandboxOptionManager.Current;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<string> onConfirm;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sandboxCode;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool nameValid;

	[PublicizedFrom(EAccessModifier.Private)]
	public string description;

	[XuiXmlAttribute("sandboxcode", false)]
	[XuiXmlBinding("sandboxcode")]
	public string SandboxCode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return sandboxCode ?? "";
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (!string.Equals(value, sandboxCode))
			{
				sandboxCode = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlBinding("name_valid")]
	public bool NameValid
	{
		get
		{
			return nameValid;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (nameValid != value)
			{
				nameValid = value;
				IsDirty = true;
			}
		}
	}

	public string Description
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return description ?? "";
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (!string.Equals(value, description))
			{
				description = value;
				IsDirty = true;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		txtPresetName.UIInput.onValidate = GameUtils.ValidateGameNameInput;
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancelCreate_OnPressed(XUiController _sender, int _mouseButton)
	{
		xui.playerUI.windowManager.Close(windowGroup);
		onConfirm = null;
	}

	[XuiBindEvent("OnPress", "btnConfirm")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirmCreate_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (NameValid)
		{
			xui.playerUI.windowManager.Close(windowGroup);
			SandboxOptionPreset sandboxOptionPreset = new SandboxOptionPreset
			{
				Name = txtPresetName.Text,
				Description = txtDescription.Text,
				IsUserPreset = true,
				Group = "User",
				Icon = "Data/Sandbox/icons/user_custom"
			};
			sandboxManager.LoadOptionsFromCode(SandboxCode, sandboxOptionPreset);
			sandboxManager.SavePreset(sandboxOptionPreset, addToDictionary: true, saveToFile: true);
			onConfirm?.Invoke(sandboxOptionPreset.Name);
			onConfirm = null;
		}
	}

	[XuiBindEvent("OnSubmitHandler", "txtPresetName")]
	[XuiBindEvent("OnSubmitHandler", "txtDescription")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateProfileName_OnSubmitHandler(XUiController _sender, string _text)
	{
		BtnConfirmCreate_OnPressed(this, -1);
	}

	[XuiBindEvent("OnChangeHandler", "txtPresetName")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateProfileName_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		string text = _text.Trim();
		NameValid = text.Length > 0 && text.IndexOf('.') < 0 && sandboxManager.GetPreset(text) == null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (XUiUtils.HotkeysAllowedFor(viewComponent))
		{
			if (xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
			{
				BtnCancelCreate_OnPressed(this, -1);
			}
			if (xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
			{
				BtnConfirmCreate_OnPressed(null, 0);
			}
		}
		if (IsDirty)
		{
			IsDirty = false;
			RefreshBindings();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		NameValid = false;
		txtPresetName.Text = "";
		txtDescription.Text = Description ?? "";
		txtPresetName.SelectOrVirtualKeyboard(_delayed: true);
	}

	public override void OnClose()
	{
		base.OnClose();
		Description = "";
	}

	public static void Open(XUi _xui, string _sandboxCode, string _description, Action<string> _onConfirm = null)
	{
		XUiC_SandboxSettingsSaveAsPreset childByType = _xui.GetChildByType<XUiC_SandboxSettingsSaveAsPreset>();
		childByType.SandboxCode = _sandboxCode;
		childByType.Description = _description;
		childByType.onConfirm = _onConfirm;
		_xui.playerUI.windowManager.Open(childByType.windowGroup, _bModal: false);
	}
}
