using System;
using System.Collections.Generic;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public abstract class XUiC_ComboBox<TValue> : XUiController
{
	public delegate void XUiEvent_ValueChanged(XUiController _sender, TValue _oldValue, TValue _newValue);

	public delegate void XUiEvent_GenericValueChanged(XUiController _sender);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled = true;

	public TValue Min;

	public TValue Max;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool Wrap = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool ScrollByIncrement;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorDisabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorBg;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorFillEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorFillDisabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorIndexMarkerActive = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorIndexMarkerInactive = Color.grey;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction gamepadDecreaseShortcut;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction gamepadIncreaseShortcut;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool indexMarkers;

	[PublicizedFrom(EAccessModifier.Private)]
	public string valueText = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite sprFill;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly IList<XUiV_Sprite> IndexMarkerSprites = new List<XUiV_Sprite>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public TValue currentValue;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool UsesSegmentedFill;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int SegmentedFillSpacing;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int SegmentedFillCount = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController clickable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDragging;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button forwardButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button backButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string bindingPrefixSegmentFillValue = "segment_fill_";

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

	public Color TextColor
	{
		get
		{
			return colorEnabled;
		}
		set
		{
			if (colorEnabled != value)
			{
				colorEnabled = value;
				IsDirty = true;
			}
		}
	}

	public string ValueText
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return valueText;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (valueText != value)
			{
				valueText = value;
				IsDirty = true;
			}
		}
	}

	public abstract TValue Value { get; set; }

	public abstract int IndexElementCount
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
	}

	public abstract int IndexMarkerIndex
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
	}

	public virtual bool UsesIndexMarkers
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (indexMarkers && sprFill != null && IndexElementCount > 0)
			{
				return IndexElementCount < IndexMarkerSprites.Count;
			}
			return false;
		}
	}

	public event XUiEvent_ValueChanged OnValueChanged;

	public event XUiEvent_GenericValueChanged OnValueChangedGeneric;

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("forward");
		if (childById != null)
		{
			childById.OnPress += ForwardButton_OnPress;
			forwardButton = childById.ViewComponent as XUiV_Button;
		}
		XUiController childById2 = GetChildById("back");
		if (childById2 != null)
		{
			childById2.OnPress += BackButton_OnPress;
			backButton = childById2.ViewComponent as XUiV_Button;
		}
		XUiController childById3 = GetChildById("fill");
		if (childById3 != null && childById3.ViewComponent is XUiV_Sprite xUiV_Sprite)
		{
			sprFill = xUiV_Sprite;
		}
		XUiController childById4 = GetChildById("indexMarkers");
		if (childById4 != null)
		{
			if (childById4.CustomAttributes.TryGetValue("active_color", out var value))
			{
				colorIndexMarkerActive = StringParsers.ParseColor32(value);
			}
			if (childById4.CustomAttributes.TryGetValue("inactive_color", out var value2))
			{
				colorIndexMarkerInactive = StringParsers.ParseColor32(value2);
			}
			for (int i = 0; i < childById4.Children.Count; i++)
			{
				if (childById4.Children[i].ViewComponent is XUiV_Sprite item)
				{
					IndexMarkerSprites.Add(item);
				}
			}
		}
		InitSegmentedFillPositions();
		clickable = GetChildById("directvalue");
		if (clickable != null)
		{
			clickable.OnPress += PressEvent;
			clickable.OnScroll += ScrollEvent;
			clickable.OnDrag += DragEvent;
			clickable.OnHover += HoverEvent;
		}
		if (!string.IsNullOrEmpty(viewComponent.ToolTip) || base.Parent == null)
		{
			return;
		}
		foreach (XUiController child in base.Parent.Children)
		{
			if (child.ViewComponent is XUiV_Label xUiV_Label)
			{
				viewComponent.ToolTip = xUiV_Label.ToolTip;
				break;
			}
		}
	}

	public override void OnOpen()
	{
		IsDirty = true;
		base.OnOpen();
		UpdateLabel();
		UpdateIndexMarkerPositions();
		UpdateIndexMarkerStates();
		RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		if (isOver)
		{
			base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuComboBox);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (enabled && clickable != null && base.xui.playerUI.CursorController.CurrentTarget == clickable.ViewComponent && !base.xui.playerUI.CursorController.Locked)
		{
			XUi.HandlePaging(base.xui, PageUpAction, PageDownAction);
		}
		if (enabled && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			forwardButton.CurrentColor = (isOver ? forwardButton.HoverSpriteColor : forwardButton.DefaultSpriteColor);
			backButton.CurrentColor = (isOver ? backButton.HoverSpriteColor : backButton.DefaultSpriteColor);
		}
		if (gamepadDecreaseShortcut != null && gamepadDecreaseShortcut.WasPressed)
		{
			PageDownAction();
		}
		if (gamepadIncreaseShortcut != null && gamepadIncreaseShortcut.WasPressed)
		{
			PageUpAction();
		}
		if (IsDirty)
		{
			IsDirty = false;
			RefreshBindings();
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "value_wrap":
			if (!_value.EqualsCaseInsensitive("@def"))
			{
				Wrap = StringParsers.ParseBool(_value);
			}
			return true;
		case "enabled_color":
			colorEnabled = StringParsers.ParseColor32(_value);
			return true;
		case "disabled_color":
			colorDisabled = StringParsers.ParseColor32(_value);
			return true;
		case "bg_color":
			colorBg = StringParsers.ParseColor32(_value);
			return true;
		case "enabled_fill_color":
			colorFillEnabled = StringParsers.ParseColor32(_value);
			return true;
		case "disabled_fill_color":
			colorFillDisabled = StringParsers.ParseColor32(_value);
			return true;
		case "segmented_fill":
			UsesSegmentedFill = StringParsers.ParseBool(_value);
			return true;
		case "segment_spacing":
			SegmentedFillSpacing = StringParsers.ParseSInt32(_value);
			return true;
		case "index_markers":
			indexMarkers = StringParsers.ParseBool(_value);
			return true;
		case "gamepad_decrease":
			gamepadDecreaseShortcut = base.xui.playerUI.playerInput.GUIActions.GetPlayerActionByName(_value);
			return true;
		case "gamepad_increase":
			gamepadIncreaseShortcut = base.xui.playerUI.playerInput.GUIActions.GetPlayerActionByName(_value);
			return true;
		case "scroll_by_increment":
			ScrollByIncrement = StringParsers.ParseBool(_value);
			return true;
		default:
			return base.ParseAttribute(_name, _value, _parent);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void UpdateLabel();

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateIndexMarkerStates()
	{
		if (UsesIndexMarkers)
		{
			for (int i = 0; i < IndexMarkerSprites.Count; i++)
			{
				IndexMarkerSprites[i].Color = colorIndexMarkerInactive;
			}
			if (IndexMarkerIndex >= 0 && IndexMarkerIndex < IndexMarkerSprites.Count)
			{
				IndexMarkerSprites[IndexMarkerIndex].Color = colorIndexMarkerActive;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateIndexMarkerPositions()
	{
		if (!UsesIndexMarkers)
		{
			return;
		}
		float num = (float)(sprFill.Size.x + 5) / (float)IndexElementCount;
		int x = Mathf.RoundToInt(num - 5f);
		for (int i = 0; i < IndexMarkerSprites.Count; i++)
		{
			if (i >= IndexElementCount)
			{
				IndexMarkerSprites[i].IsVisible = false;
				continue;
			}
			Vector2i position = IndexMarkerSprites[i].Position;
			position.x = Mathf.RoundToInt((float)i * num);
			IndexMarkerSprites[i].Position = position;
			IndexMarkerSprites[i].IsDirty = true;
			Vector2i size = IndexMarkerSprites[i].Size;
			size.x = x;
			if (i == IndexElementCount - 1)
			{
				size.x = sprFill.Size.x - position.x;
			}
			IndexMarkerSprites[i].Size = size;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void InitSegmentedFillPositions()
	{
		if (!UsesSegmentedFill || sprFill == null)
		{
			return;
		}
		XUiController childById = GetChildById("segmentedFill");
		if (childById == null || sprFill == null)
		{
			return;
		}
		SegmentedFillCount = childById.Children.Count;
		float num = (float)(sprFill.Size.x + SegmentedFillSpacing) / (float)SegmentedFillCount;
		int x = Mathf.RoundToInt(num - (float)SegmentedFillSpacing);
		for (int i = 0; i < childById.Children.Count; i++)
		{
			XUiView xUiView = childById.Children[i].ViewComponent;
			Vector2i position = xUiView.Position;
			position.x = Mathf.RoundToInt((float)i * num);
			xUiView.Position = position;
			xUiView.IsDirty = true;
			foreach (XUiController child in xUiView.Controller.Children)
			{
				XUiView xUiView2 = child.ViewComponent;
				Vector2i size = xUiView2.Size;
				size.x = x;
				if (i == SegmentedFillCount - 1)
				{
					size.x = sprFill.Size.x - position.x;
				}
				xUiView2.Size = size;
				xUiView2.IsDirty = true;
			}
		}
	}

	public void TriggerValueChangedEvent(TValue _oldVal)
	{
		UpdateIndexMarkerStates();
		if (this.OnValueChangedGeneric != null)
		{
			this.OnValueChangedGeneric(this);
		}
		if (this.OnValueChanged != null)
		{
			this.OnValueChanged(this, _oldVal, currentValue);
		}
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void BackButton_OnPress(XUiController _sender, int _mouseButton)
	{
		if (enabled)
		{
			TValue oldVal = currentValue;
			BackPressed();
			UpdateLabel();
			if (isDifferentValue(oldVal, currentValue))
			{
				TriggerValueChangedEvent(oldVal);
			}
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ForwardButton_OnPress(XUiController _sender, int _mouseButton)
	{
		if (enabled)
		{
			TValue oldVal = currentValue;
			ForwardPressed();
			UpdateLabel();
			if (isDifferentValue(oldVal, currentValue))
			{
				TriggerValueChangedEvent(oldVal);
			}
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void PageUpAction()
	{
		ForwardButton_OnPress(null, 0);
		if (base.xui.playerUI.CursorController.CursorModeActive)
		{
			SelectCursorElement(_withDelay: false, _overrideCursorMode: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void PageDownAction()
	{
		BackButton_OnPress(null, 0);
		if (base.xui.playerUI.CursorController.CursorModeActive)
		{
			SelectCursorElement(_withDelay: false, _overrideCursorMode: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract bool isDifferentValue(TValue _oldVal, TValue _currentValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void BackPressed();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void ForwardPressed();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract bool isMax();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract bool isMin();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract bool isEmpty();

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName.StartsWith("segment_fill_", StringComparison.Ordinal))
		{
			if (int.TryParse(_bindingName.AsSpan("segment_fill_".Length), out var result))
			{
				return handleSegmentedFillValueBinding(ref _value, result);
			}
			return false;
		}
		switch (_bindingName)
		{
		case "valuetext":
			_value = valueText;
			return true;
		case "valuecolor":
			_value = XUiUtils.ToXuiColorString(enabled ? colorEnabled : colorDisabled);
			return true;
		case "isnumber":
			_value = false.ToString();
			return true;
		case "usesmarkers":
			_value = UsesIndexMarkers.ToString();
			return true;
		case "fillvalue":
			_value = "0";
			return true;
		case "fillcolor":
			_value = XUiUtils.ToXuiColorString(enabled ? colorFillEnabled : colorFillDisabled);
			return true;
		case "bgcolor":
			_value = XUiUtils.ToXuiColorString(colorBg);
			return true;
		case "hascontrollershortcuts":
			_value = (gamepadIncreaseShortcut != null && gamepadDecreaseShortcut != null).ToString();
			return true;
		case "can_backward":
			_value = (enabled && !isEmpty() && (Wrap || !isMin())).ToString();
			return true;
		case "can_forward":
			_value = (enabled && !isEmpty() && (Wrap || !isMax())).ToString();
			return true;
		default:
			return base.GetBindingValue(ref _value, _bindingName);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool handleSegmentedFillValueBinding(ref string _value, int _index)
	{
		_value = "0";
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PressEvent(XUiController _sender, int _mouseButton)
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && enabled)
		{
			Vector2i mouseXUIPosition = base.xui.GetMouseXUIPosition();
			XUiController xUiController = clickable;
			Vector3 localPosition = xUiController.ViewComponent.UiTransform.localPosition;
			while (xUiController.Parent != null && xUiController.Parent.ViewComponent != null)
			{
				xUiController = xUiController.Parent;
				localPosition += xUiController.ViewComponent.UiTransform.localPosition;
			}
			localPosition += xUiController.ViewComponent.UiTransform.parent.localPosition;
			if (xUiController.ViewComponent is XUiV_Window { IsInStackpanel: not false } xUiV_Window)
			{
				Transform transform = xUiV_Window.UiTransform.parent.parent;
				localPosition *= transform.localScale.x;
				localPosition += transform.localPosition;
			}
			Vector2i vector2i = new Vector2i((int)localPosition.x, (int)localPosition.y);
			int num = (vector2i + clickable.ViewComponent.Size).x - vector2i.x;
			float num2 = (float)(mouseXUIPosition.x - vector2i.x) / (float)num;
			setRelativeValue(num2);
		}
	}

	public void ScrollEvent(XUiController _sender, float _delta)
	{
		if (!enabled || isEmpty())
		{
			return;
		}
		if (ScrollByIncrement)
		{
			if (_delta > 0f)
			{
				PageUpAction();
			}
			else
			{
				PageDownAction();
			}
		}
		else
		{
			incrementalChangeValue(_delta);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HoverEvent(XUiController _sender, bool _isOver)
	{
		isOver = _isOver;
		LocalPlayerUI.IsAnyComboBoxFocused = isOver;
		if (isOver)
		{
			base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuComboBox);
		}
		else
		{
			base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuComboBox);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DragEvent(XUiController _sender, EDragType _dragType, Vector2 _mousePositionDelta)
	{
		if (isOver && enabled && !isEmpty())
		{
			PressEvent(_sender, -1);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void setRelativeValue(double _value);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void incrementalChangeValue(double _value);

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_ComboBox()
	{
	}
}
