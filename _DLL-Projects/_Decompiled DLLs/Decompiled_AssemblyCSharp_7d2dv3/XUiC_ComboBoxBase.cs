using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public abstract class XUiC_ComboBoxBase : XUiController
{
	public delegate void XUiEvent_GenericValueChanged(XUiController _sender);

	public delegate void XUiEvent_HoveredStateChanged(XUiController _sender, bool _isOverMainArea, bool _isOverAnyPart);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showButtons = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool Wrap = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorIndexMarkerActive = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorIndexMarkerInactive = Color.grey;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorIndexMarkerDisabledActive = Color.white;

	[XuiBindComponent("indexMarkers.", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite[] indexMarkerSprites;

	[PublicizedFrom(EAccessModifier.Private)]
	public int indexMarkersSetupBefore;

	[PublicizedFrom(EAccessModifier.Private)]
	public string valueText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string valueTextOverride;

	[XuiBindComponent("fill", true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiV_Sprite sprFill;

	[PublicizedFrom(EAccessModifier.Private)]
	public int segmentedFillCount = 1;

	[XuiBindComponent("directvalue", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController clickable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOverAnyPart;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDragging;

	[XuiBindComponent("forward", false)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiController forwardButton;

	[XuiBindComponent("back", false)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiController backButton;

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

	[XuiXmlAttribute("combo_enabled", false)]
	[XuiXmlBinding("combo_enabled")]
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

	[XuiXmlBinding("show_buttons")]
	public bool ShowButtons
	{
		get
		{
			return showButtons;
		}
		set
		{
			if (value != showButtons)
			{
				showButtons = value;
				IsDirty = true;
			}
		}
	}

	public Color TextColor
	{
		get
		{
			return ColorEnabled;
		}
		set
		{
			if (!(ColorEnabled == value))
			{
				ColorEnabled = value;
				IsDirty = true;
			}
		}
	}

	public abstract long ValueGeneric { get; set; }

	public abstract long ValueMinGeneric { get; set; }

	public abstract long ValueMaxGeneric { get; set; }

	[XuiXmlAttribute("scroll_by_increment", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool ScrollByIncrement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
		[PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[XuiXmlAttribute("enabled_color", false)]
	public Color ColorEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return colorEnabled;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (!(colorEnabled == value))
			{
				colorEnabled = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("disabled_color", false)]
	public Color ColorDisabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return colorDisabled;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (!(colorDisabled == value))
			{
				colorDisabled = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("bg_color", false)]
	[XuiXmlBinding("bgcolor")]
	public Color ColorBg
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return colorBg;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (!(colorBg == value))
			{
				colorBg = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("enabled_fill_color", false)]
	public Color ColorFillEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return colorFillEnabled;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (!(colorFillEnabled == value))
			{
				colorFillEnabled = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("disabled_fill_color", false)]
	public Color ColorFillDisabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return colorFillDisabled;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (!(colorFillDisabled == value))
			{
				colorFillDisabled = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("index_markers", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IndexMarkers
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

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

	[XuiXmlAttribute("index_marker_spacing", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int IndexMarkerSpacing
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
		[PublicizedFrom(EAccessModifier.Protected)]
		set;
	} = 5;

	[XuiXmlBinding("usesmarkers")]
	public virtual bool UsesIndexMarkers
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (IndexMarkers && sprFill != null && IndexElementCount > 0)
			{
				return IndexElementCount < indexMarkerSprites.Length;
			}
			return false;
		}
	}

	[XuiXmlAttribute("gamepad_default_hover_control", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool GamepadDefaultHoverControl
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("gamepad_decrease", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction GamepadDecreaseShortcut
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("gamepad_increase", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction GamepadIncreaseShortcut
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("gamepad_shortcuts_hoveronly", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool GamepadShortcutsHoverOnly
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlBinding("valuetext")]
	public string ValueText
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return valueTextOverride ?? valueText;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (!(valueText == value))
			{
				valueText = value;
				IsDirty = true;
			}
		}
	}

	public string ValueTextOverride
	{
		get
		{
			return valueTextOverride;
		}
		set
		{
			valueTextOverride = value;
			IsDirty = true;
		}
	}

	[XuiXmlAttribute("segment_count", false)]
	[XuiXmlBinding("segmentcount")]
	public int SegmentedFillCount
	{
		get
		{
			return Mathf.Max(segmentedFillCount, 1);
		}
		set
		{
			if (value != segmentedFillCount)
			{
				segmentedFillCount = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlBinding("cbx_is_hovered")]
	public bool IsOver
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return isOver;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (isOver != value)
			{
				isOver = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlBinding("fillvalue")]
	public abstract double RelativeValue { get; set; }

	[XuiXmlBinding("valuecolor")]
	public Color ValueColor
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!Enabled)
			{
				return ColorDisabled;
			}
			return ColorEnabled;
		}
	}

	[XuiXmlBinding("isnumber")]
	public virtual bool IsNumber
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return false;
		}
	}

	[XuiXmlBinding("fillcolor")]
	public Color FillColor
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!Enabled)
			{
				return ColorFillDisabled;
			}
			return ColorFillEnabled;
		}
	}

	[XuiXmlBinding("hascontrollershortcuts")]
	public bool HasControllerShortcuts
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (GamepadIncreaseShortcut != null)
			{
				return GamepadDecreaseShortcut != null;
			}
			return false;
		}
	}

	[XuiXmlBinding("can_backward")]
	public bool CanBackward
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (Enabled && !isEmpty())
			{
				if (!Wrap)
				{
					return !isMin();
				}
				return true;
			}
			return false;
		}
	}

	[XuiXmlBinding("can_forward")]
	public bool CanForward
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (Enabled && !isEmpty())
			{
				if (!Wrap)
				{
					return !isMax();
				}
				return true;
			}
			return false;
		}
	}

	public event XUiEvent_GenericValueChanged OnValueChangedGeneric;

	public event XUiEvent_HoveredStateChanged OnHoveredStateChanged;

	public override void Init()
	{
		base.Init();
		base.OnVisiblity += [PublicizedFrom(EAccessModifier.Internal)] (XUiController _, bool _, bool _visibleInScene) =>
		{
			if (!_visibleInScene)
			{
				XUiC_Paging.IsOverPagingOverrideElement = false;
			}
		};
		XUiController childById = GetChildById("indexMarkers");
		if (childById != null)
		{
			if (childById.CustomAttributes.TryGetValue("active_color", out var value))
			{
				colorIndexMarkerActive = StringParsers.ParseColor32(value.ToString());
			}
			if (childById.CustomAttributes.TryGetValue("inactive_color", out var value2))
			{
				colorIndexMarkerInactive = StringParsers.ParseColor32(value2.ToString());
			}
			if (childById.CustomAttributes.TryGetValue("disabled_active_color", out var value3))
			{
				colorIndexMarkerDisabledActive = StringParsers.ParseColor32(value3.ToString());
			}
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
			xui.CalloutWindow?.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuComboBox);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (UsesIndexMarkers && indexMarkersSetupBefore != IndexElementCount)
		{
			UpdateIndexMarkerPositions();
		}
		if (Enabled)
		{
			bool flag = clickable != null && xui.playerUI.CursorController.CurrentTarget == clickable.ViewComponent && !xui.playerUI.CursorController.Locked && XUiUtils.HotkeysAllowedFor(viewComponent);
			if (flag && GamepadDefaultHoverControl)
			{
				XUi.HandlePaging(xui, PageUpAction, PageDownAction);
			}
			if (GamepadDecreaseShortcut != null && viewComponent.IsActiveInHierarchy && GamepadDecreaseShortcut.WasPressed && (!GamepadShortcutsHoverOnly || flag))
			{
				PageDownAction();
			}
			if (GamepadIncreaseShortcut != null && viewComponent.IsActiveInHierarchy && GamepadIncreaseShortcut.WasPressed && (!GamepadShortcutsHoverOnly || flag))
			{
				PageUpAction();
			}
		}
		if (handleDirtyUpdateDefault())
		{
			UpdateIndexMarkerStates();
		}
	}

	[XuiXmlAttribute("value_wrap", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValueWrap(string _value)
	{
		if (!_value.EqualsCaseInsensitive("@def"))
		{
			Wrap = StringParsers.ParseBool(_value);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void UpdateLabel();

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateIndexMarkerStates()
	{
		if (UsesIndexMarkers)
		{
			for (int i = 0; i < indexMarkerSprites.Length; i++)
			{
				indexMarkerSprites[i].Color = colorIndexMarkerInactive;
			}
			if (IndexMarkerIndex >= 0 && IndexMarkerIndex < indexMarkerSprites.Length)
			{
				indexMarkerSprites[IndexMarkerIndex].Color = (Enabled ? colorIndexMarkerActive : colorIndexMarkerDisabledActive);
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
		indexMarkersSetupBefore = IndexElementCount;
		float num = (float)(sprFill.Size.x + IndexMarkerSpacing) / (float)IndexElementCount;
		int x = Mathf.RoundToInt(num - (float)IndexMarkerSpacing);
		for (int i = 0; i < indexMarkerSprites.Length; i++)
		{
			if (i >= IndexElementCount)
			{
				indexMarkerSprites[i].IsVisible = false;
				continue;
			}
			Vector2i position = indexMarkerSprites[i].Position;
			position.x = Mathf.RoundToInt((float)i * num);
			indexMarkerSprites[i].Position = position;
			Vector2i size = indexMarkerSprites[i].Size;
			size.x = x;
			if (i == IndexElementCount - 1)
			{
				size.x = sprFill.Size.x - position.x;
			}
			indexMarkerSprites[i].Size = size;
			indexMarkerSprites[i].IsVisible = true;
		}
	}

	[XuiBindEvent("OnHover", "forwardButton")]
	[XuiBindEvent("OnHover", "backButton")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void Button_OnHover(XUiController _sender, bool _isOver)
	{
		isOverAnyPart = _isOver;
		this.OnHoveredStateChanged?.Invoke(this, isOver, isOverAnyPart);
	}

	[XuiBindEvent("OnPress", "backButton")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BackButton_OnPress(XUiController _sender, int _mouseButton)
	{
		TryPageDown();
	}

	[XuiBindEvent("OnPress", "forwardButton")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ForwardButton_OnPress(XUiController _sender, int _mouseButton)
	{
		TryPageUp();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool PageUpAction()
	{
		bool result = TryPageUp();
		if (xui.playerUI.CursorController.CursorModeActive)
		{
			SelectCursorElement(_withDelay: false, _overrideCursorMode: true);
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool PageDownAction()
	{
		bool result = TryPageDown();
		if (xui.playerUI.CursorController.CursorModeActive)
		{
			SelectCursorElement(_withDelay: false, _overrideCursorMode: true);
		}
		return result;
	}

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

	[XuiBindEvent("OnPress", "clickable")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void pressEvent(XUiController _sender, int _mouseButton)
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && Enabled)
		{
			Vector2i mouseXUiPosition = xui.GetMouseXUiPosition();
			Rect xUiRect = clickable.ViewComponent.GetXUiRect();
			float relativeValue = ((float)mouseXUiPosition.x - xUiRect.min.x) / xUiRect.width;
			setRelativeValue(relativeValue);
		}
	}

	[XuiBindEvent("OnScroll", "clickable")]
	public void ScrollEvent(XUiController _sender, float _delta)
	{
		if (!InputUtils.ShiftKeyPressed || !Enabled || isEmpty())
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

	[XuiBindEvent("OnHover", "clickable")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void hoverEvent(XUiController _sender, bool _isOver)
	{
		IsOver = _isOver;
		isOverAnyPart = _isOver;
		this.OnHoveredStateChanged?.Invoke(this, isOver, isOverAnyPart);
		XUiC_Paging.IsOverPagingOverrideElement = isOver;
		if (isOver && GamepadDefaultHoverControl)
		{
			xui.CalloutWindow?.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuComboBox);
		}
		else
		{
			xui.CalloutWindow?.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuComboBox);
		}
	}

	[XuiBindEvent("OnDrag", "clickable")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void dragEvent(XUiController _sender, EDragType _dragType, Vector2 _mousePositionDelta)
	{
		if (isOver && Enabled && !isEmpty())
		{
			pressEvent(_sender, -1);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void invokeValueChangedGeneric()
	{
		this.OnValueChangedGeneric?.Invoke(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void incrementalChangeValue(double _value);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract bool TryPageUp();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract bool TryPageDown();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void setRelativeValue(float _value);

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_ComboBoxBase()
	{
	}
}
