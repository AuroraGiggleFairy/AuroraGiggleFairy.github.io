using System.Collections;
using System.Collections.Generic;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_TextInput : XUiController
{
	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label label;

	[XuiBindComponent("scrollarea", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController scrollArea;

	[XuiBindComponent(false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_ScrollView scrollView;

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
	public Color activeTextColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIInput.InputType displayType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string virtKeyboardPrompt = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openCompleted;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool removeFocusOnOpen = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly KeyCombo tabCombo = new KeyCombo(Key.Tab);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool textChangeFromCode;

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_TextInput currentSearchField;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Vector3> caretWorldCorners = new List<Vector3>(4);

	[XuiXmlAttribute("value", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string InitialValue
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("caret_color", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Color CaretColor
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("selection_color", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Color SelectionColor
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("validation", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public UIInput.Validation Validation
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("hide_input", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool HideInput
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("on_return", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public UIInput.OnReturnKey OnReturnKey
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = UIInput.OnReturnKey.Submit;

	[XuiXmlAttribute("character_limit", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int CharacterLimit
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("input_type", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public UIInput.KeyboardType InputType
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("ignore_up_down_keys", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IgnoreUpDownKeys
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("search_field", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsSearchField
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("clear_button", false)]
	[XuiXmlBinding("clearbutton")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool HasClearButton
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("password_field", false)]
	[XuiXmlBinding("passwordfield")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsPasswordField
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("focus_on_open", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool FocusOnOpen
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("open_vk_on_open", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool OpenVkOnOpen
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("clear_on_open", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool ClearOnOpen
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("vk_propagates_events", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool VkPropagatesEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = true;

	[XuiXmlAttribute("close_group_on_tab", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool CloseGroupOnTab
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public UIInput UIInput => uiInput;

	public XUiController UIInputController => uiInputController;

	public string Text
	{
		get
		{
			return uiInput?.value ?? "";
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

	[XuiXmlAttribute("active_text_color", false)]
	public Color ActiveTextColor
	{
		get
		{
			return activeTextColor;
		}
		set
		{
			if (!(value == activeTextColor))
			{
				activeTextColor = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("input_enabled", false)]
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
			return label.SupportBbCode;
		}
		set
		{
			label.SupportBbCode = value;
		}
	}

	public bool IsSelected => uiInput.isSelected;

	[XuiXmlBinding("showpassword")]
	public bool ShowPassword => displayType == UIInput.InputType.Standard;

	[XuiXmlBinding("txt_empty")]
	public bool IsEmpty
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return Text.Length == 0;
		}
	}

	public event XUiEvent_InputOnSubmitEventHandler OnSubmitHandler;

	public event XUiEvent_InputOnChangedEventHandler OnChangeHandler;

	public event XUiEvent_InputOnAbortedEventHandler OnInputAbortedHandler;

	public event XUiEvent_InputOnSelectedEventHandler OnInputSelectedHandler;

	public event XUiEvent_InputOnErrorEventHandler OnInputErrorHandler;

	public event UIInput.OnClipboard OnClipboardHandler;

	public static void SelectCurrentSearchField(LocalPlayerUI _playerUi)
	{
		if (currentSearchField != null && !_playerUi.windowManager.IsInputActive() && currentSearchField.viewComponent.IsActiveInHierarchy)
		{
			currentSearchField.SetSelected();
		}
	}

	[XuiXmlAttribute("virtual_keyboard_prompt", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeVirtualKeyboardPrompt(string _value)
	{
		virtKeyboardPrompt = Localization.Get(_value);
	}

	[XuiXmlAttribute("select_on_tab", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeSelectOnTab(string _value)
	{
		if (!string.IsNullOrEmpty(_value))
		{
			selectOnTabString = _value;
			selectOnTabSetFromXML = true;
			IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		uiInputController = label.Controller;
		uiInput = uiInputController.ViewComponent.UiTransform.gameObject.AddComponent<UIInput>();
		if (IgnoreUpDownKeys)
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
		label.BindToUiInput(uiInput);
		uiInput.value = InitialValue ?? "";
		uiInput.activeTextColor = activeTextColor;
		uiInput.caretColor = CaretColor;
		uiInput.selectionColor = SelectionColor;
		uiInput.inputType = displayType;
		uiInput.validation = Validation;
		uiInput.hideInput = HideInput;
		uiInput.onReturnKey = OnReturnKey;
		uiInput.characterLimit = CharacterLimit;
		uiInput.keyboardType = InputType;
		uiInputController.OnSelect += inputFieldSelected;
	}

	[XuiBindEvent("OnPress", "scrollArea")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ScrollArea_OnPress(XUiController _sender, int _mouseButton)
	{
		SelectOrVirtualKeyboard(_delayed: true);
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void showVirtualKeyboard()
	{
		if (Enabled)
		{
			IVirtualKeyboard virtualKeyboard = PlatformManager.NativePlatform.VirtualKeyboard;
			string text;
			if (virtualKeyboard == null)
			{
				text = Localization.Get("ttPlatformHasNoVirtualKeyboard");
			}
			else
			{
				uint singleLineLength = ((CharacterLimit <= 0) ? 200u : ((uint)CharacterLimit));
				text = virtualKeyboard.Open(virtKeyboardPrompt, uiInput.value, OnTextReceived, displayType, OnReturnKey == UIInput.OnReturnKey.NewLine, singleLineLength);
			}
			if (text != null)
			{
				GameManager.ShowTooltip(GameManager.Instance?.World?.GetPrimaryPlayer(), "[BB0000]" + text);
				this.OnInputErrorHandler?.Invoke(this, text);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void inputFieldSelected(XUiController _sender, bool _selected)
	{
		if (_selected)
		{
			ThreadManager.StartCoroutine(removeSelection());
		}
		else
		{
			this.OnInputSelectedHandler?.Invoke(this, _selected: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator removeSelection()
	{
		yield return null;
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			SetSelected(_selected: false);
			showVirtualKeyboard();
		}
		this.OnInputSelectedHandler?.Invoke(this, _selected: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTextReceived(bool _success, string _text)
	{
		if (!isOpen)
		{
			return;
		}
		Text = _text;
		if (VkPropagatesEvents)
		{
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
		}
		uiInput.RemoveFocus();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnSubmit()
	{
		string value = UIInput.current.value;
		uiInput.RemoveFocus();
		if (this.OnSubmitHandler != null)
		{
			ThreadManager.StartCoroutine(delaySubmitHandler(value));
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
		if (windowGroup.isShowing)
		{
			updateScrollPositionToCaret();
		}
		IsDirty = true;
		this.OnChangeHandler?.Invoke(this, UIInput.current.value, textChangeFromCode);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateScrollPositionToCaret()
	{
		if (scrollView == null)
		{
			return;
		}
		UITexture caret = uiInput.caret;
		if (caret == null || !caret.enabled)
		{
			scrollView?.UpdatePosition();
			return;
		}
		List<Vector3> verts = caret.geometry.verts;
		for (int i = 0; i < verts.Count; i++)
		{
			if (i >= caretWorldCorners.Count)
			{
				caretWorldCorners.Add(Vector3.zero);
			}
			caretWorldCorners[i] = caret.transform.TransformPoint(verts[i]);
		}
		scrollView.MakeVisible(caretWorldCorners);
	}

	public void TriggerOnChangeHandler(bool _changeFromCode = false)
	{
		this.OnChangeHandler?.Invoke(this, uiInput.value, _changeFromCode);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnClipboard(UIInput.ClipboardAction _actionType, string _oldText, int _selStart, int _selEnd, string _actionResultText)
	{
		this.OnClipboardHandler?.Invoke(_actionType, _oldText, _selStart, _selEnd, _actionResultText);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		label.Text = uiInput.value;
		IsDirty = true;
		isOpen = true;
		openCompleted = false;
		removeFocusOnOpen = true;
		if (IsPasswordField)
		{
			displayType = UIInput.InputType.Password;
			uiInput.UpdateLabel();
		}
		if (IsSearchField)
		{
			currentSearchField = this;
		}
		if (ClearOnOpen)
		{
			Text = "";
		}
		if (FocusOnOpen)
		{
			SetSelected(_selected: true, _delayed: true);
		}
		if (OpenVkOnOpen)
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

	[XuiBindEvent("OnVisiblity", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void visibilityChanged(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
	{
		if (!_visibleInScene)
		{
			SetSelected(_selected: false);
			_ = UICamera.selectedObject;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (xui.playerUI.CursorController.GetMouseButton(UICamera.MouseButton.LeftButton) && uiInput.isSelected && !uiInputController.ViewComponent.UiTransformIsHovered)
		{
			uiInput.isSelected = false;
		}
		if (CloseGroupOnTab && uiInput.isSelected && selectOnTab == null)
		{
			PlayerAction inventory = xui.playerUI.playerInput.PermanentActions.Inventory;
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
		PlayerAction inventoryAction = xui.playerUI.playerInput.PermanentActions.Inventory;
		KeyBindingSource keyBindingSource = inventoryAction.GetBindingOfType() as KeyBindingSource;
		if (!(keyBindingSource == null) && !(keyBindingSource.Control != tabCombo))
		{
			while (inventoryAction.IsPressed)
			{
				yield return null;
			}
			if (CloseGroupOnTab && uiInput.isSelected && selectOnTab == null)
			{
				xui.playerUI.windowManager.Close(windowGroup);
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
			showVirtualKeyboard();
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
