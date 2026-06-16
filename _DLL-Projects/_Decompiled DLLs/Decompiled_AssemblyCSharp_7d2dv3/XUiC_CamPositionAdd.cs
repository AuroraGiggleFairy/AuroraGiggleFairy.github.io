using UnityEngine.Scripting;

[Preserve]
public class XUiC_CamPositionAdd : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CamPositionsList parentListWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtComment;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool? modalWindowGroupEscCloseableBefore;

	[PublicizedFrom(EAccessModifier.Private)]
	public string name = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string comment = string.Empty;

	public override void Init()
	{
		base.Init();
		parentListWindow = GetParentByType<XUiC_CamPositionsList>();
		txtName = GetChildById("txtName")?.GetChildByType<XUiC_TextInput>();
		txtComment = GetChildById("txtComment")?.GetChildByType<XUiC_TextInput>();
		if (txtName != null)
		{
			txtName.OnChangeHandler += Name_OnChangeHandler;
			txtName.OnSubmitHandler += Inputs_OnSubmitHandler;
			txtName.SelectOnTab = txtComment;
		}
		if (txtComment != null)
		{
			txtComment.OnChangeHandler += Comment_OnChangeHandler;
			txtComment.OnSubmitHandler += Inputs_OnSubmitHandler;
			txtComment.SelectOnTab = txtName;
		}
		if (GetChildById("btnAdd") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				add();
			};
		}
		base.OnVisiblity += OnVisibilityChanged;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		base.OnVisiblity -= OnVisibilityChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Name_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		name = _text;
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Comment_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		comment = _text;
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Inputs_OnSubmitHandler(XUiController _sender, string _text)
	{
		add();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clear()
	{
		if (txtName != null)
		{
			txtName.Text = string.Empty;
		}
		if (txtComment != null)
		{
			txtComment.Text = string.Empty;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void add()
	{
		if (validateInput())
		{
			parentListWindow.Add(name, comment);
			parentListWindow.ShowAddCamPositionWindow = false;
			clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validateInput()
	{
		return name.Trim().Length > 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "valid_input")
		{
			_value = validateInput().ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public void OnVisibilityChanged(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
	{
		GUIWindow modalWindow = xui.playerUI.windowManager.GetModalWindow();
		if (modalWindow != null)
		{
			if (_visibleInScene)
			{
				modalWindowGroupEscCloseableBefore = modalWindow.isEscClosable;
				modalWindow.isEscClosable = false;
				txtName?.SelectOrVirtualKeyboard();
			}
			else if (modalWindowGroupEscCloseableBefore.HasValue)
			{
				modalWindow.isEscClosable = modalWindowGroupEscCloseableBefore.Value;
			}
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		modalWindowGroupEscCloseableBefore = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		UpdateInput();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateInput()
	{
		if (viewComponent.IsVisible && (xui.playerUI.playerInput.GUIActions.Cancel.WasPressed || xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed))
		{
			parentListWindow.ShowAddCamPositionWindow = false;
		}
	}
}
