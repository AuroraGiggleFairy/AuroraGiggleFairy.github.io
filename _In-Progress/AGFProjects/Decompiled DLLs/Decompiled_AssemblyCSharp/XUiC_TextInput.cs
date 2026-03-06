using System.Collections;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_TextInput : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController text;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController uiInputController;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIInput uiInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput selectOnTab;

	[PublicizedFrom(EAccessModifier.Private)]
	public string selectOnTabString;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool selectOnTabSetFromXML;

	[PublicizedFrom(EAccessModifier.Private)]
	public string value;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color activeTextColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color caretColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color selectionColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIInput.InputType displayType;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIInput.Validation validation;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hideInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIInput.OnReturnKey onReturnKey = UIInput.OnReturnKey.Submit;

	[PublicizedFrom(EAccessModifier.Private)]
	public int characterLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIInput.KeyboardType inputType;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ignoreUpDownKeys;

	public bool useVirtualKeyboard;

	[PublicizedFrom(EAccessModifier.Private)]
	public string virtKeyboardPrompt = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openCompleted;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool removeFocusOnOpen = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSearchField;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasClearButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPasswordField;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool focusOnOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openVKOnOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool clearOnOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly KeyCombo tabCombo = new KeyCombo(Key.Tab);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool closeGroupOnTab;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool textChangeFromCode;

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_TextInput currentSearchField;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled = true;

	public UIInput UIInput => uiInput;

	public XUiController UIInputController => uiInputController;

	public string Text
	{
		get
		{
			return uiInput.value;
		}
		set
		{
			textChangeFromCode = true;
			uiInput.value = value;
			textChangeFromCode = false;
			uiInput.UpdateLabel();
		}
	}

	public XUiC_TextInput SelectOnTab
	{
		get
		{
			return selectOnTab;
		}
		set
		{
			if (selectOnTab == value)
			{
				return;
			}
			selectOnTab = value;
			if (value != null)
			{
				UIKeyNavigation uIKeyNavigation = uiInputController.ViewComponent.UiTransform.gameObject.AddMissingComponent<UIKeyNavigation>();
				uIKeyNavigation.constraint = UIKeyNavigation.Constraint.Explicit;
				uIKeyNavigation.onTab = value.uiInput.gameObject;
				return;
			}
			UIKeyNavigation component = uiInputController.ViewComponent.UiTransform.gameObject.GetComponent<UIKeyNavigation>();
			if (component != null)
			{
				Object.Destroy(component);
			}
		}
	}

	public Color ActiveTextColor
	{
		get
		{
			return activeTextColor;
		}
		set
		{
			if (value != activeTextColor)
			{
				activeTextColor = value;
				IsDirty = true;
			}
		}
	}

	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			if (value != enabled)
			{
				enabled = value;
				IsDirty = true;
			}
		}
	}

	public bool SupportBbCode
	{
		get
		{
			return ((XUiV_Label)text.ViewComponent).SupportBbCode;
		}
		set
		{
			((XUiV_Label)text.ViewComponent).SupportBbCode = value;
		}
	}

	public bool IsSelected => uiInput.isSelected;

	public event XUiEvent_InputOnSubmitEventHandler OnSubmitHandler;

	public event XUiEvent_InputOnChangedEventHandler OnChangeHandler;

	public event XUiEvent_InputOnAbortedEventHandler OnInputAbortedHandler;

	public event XUiEvent_InputOnSelectedEventHandler OnInputSelectedHandler;

	public event XUiEvent_InputOnErrorEventHandler OnInputErrorHandler;

	public event UIInput.OnClipboard OnClipboardHandler;

	public static void SelectCurrentSearchField(LocalPlayerUI _playerUi)
	{
		if (currentSearchField != null && !_playerUi.windowManager.IsInputActive() && currentSearchField.viewComponent.UiTransform.gameObject.activeInHierarchy)
		{
			currentSearchField.SetSelected();
		}
	}

	public override void Init()
	{
		if (viewComponent is XUiV_Panel xUiV_Panel)
		{
			xUiV_Panel.createUiPanel = true;
		}
		base.Init();
		text = GetChildById("text");
		uiInputController = text;
		uiInput = uiInputController.ViewComponent.UiTransform.gameObject.AddComponent<UIInput>();
		if (ignoreUpDownKeys)
		{
			uiInput.onDownArrow = [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
			};
			uiInput.onUpArrow = [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
			};
		}
		XUiController childById = GetChildById("btnShowPassword");
		if (childById != null)
		{
			childById.OnPress += BtnShowPassword_OnPress;
		}
		XUiController childById2 = GetChildById("btnClearInput");
		if (childById2 != null)
		{
			childById2.OnPress += BtnClearInput_OnPress;
		}
		EventDelegate.Add(uiInput.onSubmit, OnSubmit);
		EventDelegate.Add(uiInput.onChange, OnChange);
		uiInput.onClipboard += OnClipboard;
		uiInput.label = ((XUiV_Label)text.ViewComponent).Label;
		uiInput.label.autoResizeBoxCollider = true;
		uiInput.value = value ?? "";
		uiInput.activeTextColor = activeTextColor;
		uiInput.caretColor = caretColor;
		uiInput.selectionColor = selectionColor;
		uiInput.inputType = displayType;
		uiInput.validation = validation;
		uiInput.hideInput = hideInput;
		uiInput.onReturnKey = onReturnKey;
		uiInput.characterLimit = characterLimit;
		uiInput.keyboardType = inputType;
		uiInputController.OnSelect += InputFieldSelected;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnShowPassword_OnPress(XUiController _sender, int _mouseButton)
	{
		displayType = ((displayType != UIInput.InputType.Password) ? UIInput.InputType.Password : UIInput.InputType.Standard);
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnClearInput_OnPress(XUiController _sender, int _mouseButton)
	{
		Text = "";
		IsDirty = true;
	}

	public void ShowVirtualKeyboard()
	{
		XUiC_TextInput_OnPress(this, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InputFieldSelected(XUiController _sender, bool _selected)
	{
		if (_selected)
		{
			ThreadManager.StartCoroutine(removeSelection());
		}
		else if (this.OnInputSelectedHandler != null)
		{
			this.OnInputSelectedHandler(this, _selected: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator removeSelection()
	{
		yield return null;
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			SetSelected(_selected: false);
			XUiC_TextInput_OnPress(this, -1);
		}
		if (this.OnInputSelectedHandler != null)
		{
			this.OnInputSelectedHandler(this, _selected: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiC_TextInput_OnPress(XUiController _sender, int _mouseButton)
	{
		if (!Enabled)
		{
			return;
		}
		string text = null;
		IVirtualKeyboard virtualKeyboard = PlatformManager.NativePlatform.VirtualKeyboard;
		if (virtualKeyboard == null)
		{
			text = Localization.Get("ttPlatformHasNoVirtualKeyboard");
		}
		else
		{
			uint singleLineLength = ((characterLimit <= 0) ? 200u : ((uint)characterLimit));
			text = virtualKeyboard.Open(virtKeyboardPrompt, uiInput.value, OnTextReceived, displayType, onReturnKey == UIInput.OnReturnKey.NewLine, singleLineLength);
		}
		if (text != null)
		{
			GameManager.ShowTooltip(GameManager.Instance?.World?.GetPrimaryPlayer(), "[BB0000]" + text);
			if (this.OnInputErrorHandler != null)
			{
				this.OnInputErrorHandler(this, text);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTextReceived(bool _success, string _text)
	{
		if (!isOpen)
		{
			return;
		}
		Text = _text;
		if (_success)
		{
			if (this.OnSubmitHandler != null)
			{
				this.OnSubmitHandler(this, _text);
			}
			else if (this.OnChangeHandler != null)
			{
				this.OnChangeHandler(this, _text, _changeFromCode: false);
			}
		}
		else if (this.OnInputAbortedHandler != null)
		{
			this.OnInputAbortedHandler(this);
		}
		uiInput.RemoveFocus();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnSubmit()
	{
		string input = UIInput.current.value;
		uiInput.RemoveFocus();
		if (this.OnSubmitHandler != null)
		{
			ThreadManager.StartCoroutine(delaySubmitHandler(input));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator delaySubmitHandler(string _input)
	{
		yield return null;
		this.OnSubmitHandler?.Invoke(this, _input);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnChange()
	{
		this.OnChangeHandler?.Invoke(this, UIInput.current.value, textChangeFromCode);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnClipboard(UIInput.ClipboardAction _actiontype, string _oldtext, int _selstart, int _selend, string _actionresulttext)
	{
		this.OnClipboardHandler?.Invoke(_actiontype, _oldtext, _selstart, _selend, _actionresulttext);
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		switch (name)
		{
		case "value":
			this.value = value;
			break;
		case "input_enabled":
			Enabled = StringParsers.ParseBool(value);
			break;
		case "active_text_color":
			ActiveTextColor = StringParsers.ParseColor32(value);
			break;
		case "caret_color":
			caretColor = StringParsers.ParseColor32(value);
			break;
		case "selection_color":
			selectionColor = StringParsers.ParseColor32(value);
			break;
		case "validation":
			validation = EnumUtils.Parse<UIInput.Validation>(value, _ignoreCase: true);
			break;
		case "hide_input":
			hideInput = StringParsers.ParseBool(value);
			break;
		case "on_return":
			onReturnKey = EnumUtils.Parse<UIInput.OnReturnKey>(value, _ignoreCase: true);
			break;
		case "character_limit":
			characterLimit = int.Parse(value);
			break;
		case "input_type":
			inputType = EnumUtils.Parse<UIInput.KeyboardType>(value, _ignoreCase: true);
			break;
		case "ignore_up_down_keys":
			ignoreUpDownKeys = StringParsers.ParseBool(value);
			break;
		case "use_virtual_keyboard":
			useVirtualKeyboard = StringParsers.ParseBool(value);
			break;
		case "virtual_keyboard_prompt":
			virtKeyboardPrompt = Localization.Get(value);
			break;
		case "search_field":
			isSearchField = StringParsers.ParseBool(value);
			break;
		case "clear_button":
			hasClearButton = StringParsers.ParseBool(value);
			break;
		case "password_field":
			isPasswordField = StringParsers.ParseBool(value);
			break;
		case "focus_on_open":
			focusOnOpen = StringParsers.ParseBool(value);
			break;
		case "open_vk_on_open":
			openVKOnOpen = StringParsers.ParseBool(value);
			break;
		case "clear_on_open":
			clearOnOpen = StringParsers.ParseBool(value);
			break;
		case "close_group_on_tab":
			closeGroupOnTab = StringParsers.ParseBool(value);
			break;
		case "select_on_tab":
			if (!string.IsNullOrEmpty(value))
			{
				selectOnTabString = value;
				selectOnTabSetFromXML = true;
				IsDirty = true;
			}
			break;
		}
		return base.ParseAttribute(name, value, _parent);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "clearbutton":
			_value = hasClearButton.ToString();
			return true;
		case "passwordfield":
			_value = isPasswordField.ToString();
			return true;
		case "showpassword":
			_value = (displayType == UIInput.InputType.Standard).ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		((XUiV_Label)text.ViewComponent).Text = uiInput.value;
		IsDirty = true;
		isOpen = true;
		openCompleted = false;
		removeFocusOnOpen = true;
		if (isPasswordField)
		{
			displayType = UIInput.InputType.Password;
			uiInput.UpdateLabel();
		}
		if (isSearchField)
		{
			currentSearchField = this;
		}
		if (clearOnOpen)
		{
			Text = "";
		}
		if (focusOnOpen)
		{
			SetSelected(_selected: true, _delayed: true);
		}
		if (openVKOnOpen)
		{
			SelectOrVirtualKeyboard(_delayed: true);
		}
	}

	public override void OnClose()
	{
		SetSelected(_selected: false);
		base.OnClose();
		isOpen = false;
		if (currentSearchField == this)
		{
			currentSearchField = null;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.xui.playerUI.CursorController.GetMouseButton(UICamera.MouseButton.LeftButton) && uiInput.isSelected && UICamera.hoveredObject != uiInputController.ViewComponent.UiTransform.gameObject)
		{
			uiInput.isSelected = false;
		}
		if (closeGroupOnTab && uiInput.isSelected && selectOnTab == null)
		{
			PlayerAction inventory = base.xui.playerUI.playerInput.PermanentActions.Inventory;
			KeyBindingSource keyBindingSource = inventory.GetBindingOfType() as KeyBindingSource;
			if (inventory.WasPressed && keyBindingSource != null && keyBindingSource.Control == tabCombo)
			{
				ThreadManager.StartCoroutine(closeOnTabLater());
			}
		}
		if (IsDirty)
		{
			if (!Enabled)
			{
				uiInput.isSelected = false;
			}
			uiInput.enabled = Enabled;
			uiInput.inputType = displayType;
			uiInput.activeTextColor = activeTextColor;
			uiInput.UpdateLabel();
			if (!openCompleted && removeFocusOnOpen)
			{
				uiInput.RemoveFocus();
			}
			parseSelectOnTabName();
			IsDirty = false;
			RefreshBindings();
			if (!openCompleted)
			{
				openCompleted = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator closeOnTabLater()
	{
		PlayerAction inventoryAction = base.xui.playerUI.playerInput.PermanentActions.Inventory;
		KeyBindingSource keyBindingSource = inventoryAction.GetBindingOfType() as KeyBindingSource;
		if (!(keyBindingSource == null) && !(keyBindingSource.Control != tabCombo))
		{
			while (inventoryAction.IsPressed)
			{
				yield return null;
			}
			if (closeGroupOnTab && uiInput.isSelected && selectOnTab == null)
			{
				base.xui.playerUI.windowManager.Close(windowGroup);
			}
		}
	}

	public void SetSelected(bool _selected = true, bool _delayed = false)
	{
		ThreadManager.StartCoroutine(setSelectedDelayed(_selected, _delayed));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator setSelectedDelayed(bool _selected, bool _delayed)
	{
		if (_delayed)
		{
			yield return null;
		}
		if (!_selected || PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			uiInput.isSelected = _selected;
			if (!openCompleted)
			{
				removeFocusOnOpen = false;
			}
		}
	}

	public void SelectOrVirtualKeyboard(bool _delayed = false)
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			SetSelected(_selected: true, _delayed);
		}
		else
		{
			ShowVirtualKeyboard();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void parseSelectOnTabName()
	{
		if (selectOnTabSetFromXML && !(selectOnTab?.ViewComponent.ID == selectOnTabString) && XUiUtils.FindHierarchyClosestView(this, selectOnTabString)?.Controller is XUiC_TextInput xUiC_TextInput)
		{
			SelectOnTab = xUiC_TextInput;
		}
	}
}
