using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Audio;
using InControl;
using Unity.Profiling;
using UnityEngine;

public abstract class XUiView : IXUiElement
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<Type, GameObject> componentTemplates = new Dictionary<Type, GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform templatesParent;

	public readonly XUi xui;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool destroyed;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController controller;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform uiTransform;

	[PublicizedFrom(EAccessModifier.Protected)]
	public BoxCollider collider;

	public readonly List<XUiTweenAbs> Tweeners = new List<XUiTweenAbs>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string id;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isVisible = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool enabled = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2i size;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2i position;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool positionDirty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool ignoreParentPadding;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiSideSizes padding;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float rotation;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool rotationDirty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int depth;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip xuiSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip xuiHoverSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool eventOnHover;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool eventOnPress;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool eventOnDoubleClick;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool eventOnHeld;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool eventOnScroll;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool eventOnDrag;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool eventOnSelect;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isNavigatable = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool gamepadSelectableSetFromAttributes;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool gamepadSnappableSetFromAttributes;

	[PublicizedFrom(EAccessModifier.Private)]
	public string navUpTargetString;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView navUpTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public string navDownTargetString;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView navDownTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public string navLeftTargetString;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView navLeftTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public string navRightTargetString;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView navRightTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public string toolTip;

	[PublicizedFrom(EAccessModifier.Private)]
	public string disabledToolTip;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerMarker pmUpdateHover = new ProfilerMarker("XV.U-Hover");

	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerMarker pmUpdatePress = new ProfilerMarker("XV.U-Press");

	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerMarker pmUpdateData = new ProfilerMarker("XV.U-UpdateData");

	[PublicizedFrom(EAccessModifier.Private)]
	public string anchorLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public string anchorRight;

	[PublicizedFrom(EAccessModifier.Private)]
	public string anchorBottom;

	[PublicizedFrom(EAccessModifier.Private)]
	public string anchorTop;

	[PublicizedFrom(EAccessModifier.Private)]
	public string anchorLeftParsed;

	[PublicizedFrom(EAccessModifier.Private)]
	public string anchorRightParsed;

	[PublicizedFrom(EAccessModifier.Private)]
	public string anchorBottomParsed;

	[PublicizedFrom(EAccessModifier.Private)]
	public string anchorTopParsed;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool anchoredLeftAndRight;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool anchoredTopAndBottom;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasDragging;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isHold;

	[PublicizedFrom(EAccessModifier.Private)]
	public float pressStartTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdStartTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdEventIntervalCurrent;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdEventIntervalChangeSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdEventNextTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdEventLastTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isOver;

	public XUiController Controller
	{
		get
		{
			return controller;
		}
		set
		{
			controller = value;
			if (controller.ViewComponent != this)
			{
				controller.ViewComponent = this;
			}
		}
	}

	public abstract UIRect UiRect
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
	}

	public Transform UiTransform => uiTransform;

	public bool ColliderEnabled
	{
		get
		{
			if (!destroyed)
			{
				return collider.enabled;
			}
			return false;
		}
	}

	public Vector3 ColliderCenter
	{
		get
		{
			if (!destroyed)
			{
				return collider.bounds.center;
			}
			return default(Vector3);
		}
	}

	public float ColliderHeightExtent
	{
		get
		{
			if (!destroyed)
			{
				return collider.bounds.size.y / 2f;
			}
			return 0f;
		}
	}

	public float ColliderWidthExtent
	{
		get
		{
			if (!destroyed)
			{
				return collider.bounds.size.x / 2f;
			}
			return 0f;
		}
	}

	public Bounds ColliderBounds
	{
		get
		{
			if (!destroyed)
			{
				return collider.bounds;
			}
			return default(Bounds);
		}
	}

	public virtual Vector3 LocalCenter => (position + size / 2).AsVector2();

	public abstract Vector3[] WorldCorners { get; }

	[XuiXmlAttribute("repeat_content", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool RepeatContent { get; set; }

	[XuiXmlAttribute("repeat_count", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public virtual int RepeatCount { get; set; }

	public bool UiTransformIsHovered => UICamera.hoveredObject == uiTransform.gameObject;

	public bool IsActiveInHierarchy
	{
		get
		{
			if (!(uiTransform != null))
			{
				return isVisible;
			}
			return uiTransform.gameObject.activeInHierarchy;
		}
	}

	public string ID => id;

	[XuiXmlAttribute("visible", false)]
	public bool IsVisible
	{
		get
		{
			return isVisible;
		}
		set
		{
			if (isVisible == value)
			{
				return;
			}
			isVisible = value;
			if (uiTransform != null && isVisible != uiTransform.gameObject.activeSelf)
			{
				if (ID == "newContent")
				{
					Log.Warning($"XUiView.IsVisible - SetActive({isVisible}), frame={Time.frameCount}");
				}
				uiTransform.gameObject.SetActive(isVisible);
			}
			Controller.OnVisibilityChanged(IsActiveInHierarchy);
			if (xui.playerUI.CursorController.navigationTarget == this)
			{
				xui.playerUI.RefreshNavigationTarget();
			}
			SetDirty();
		}
	}

	[XuiXmlAttribute("enabled", false)]
	public virtual bool Enabled
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
				if (!enabled && xui.playerUI.CursorController.navigationTarget == this)
				{
					xui.playerUI.RefreshNavigationTarget();
				}
				if (!value && isOver)
				{
					OnHover(_isOver: true);
				}
				Controller.OnEnabledChanged(enabled);
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("collider_scale", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public float ColliderScale
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
		[PublicizedFrom(EAccessModifier.Protected)]
		set;
	} = 1f;

	[XuiXmlAttribute("collider_padding", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Vector2i ColliderPadding
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
		[PublicizedFrom(EAccessModifier.Protected)]
		set;
	} = Vector2i.zero;

	[XuiXmlAttribute("size", false)]
	public Vector2i Size
	{
		get
		{
			return size;
		}
		set
		{
			if (!(size == value))
			{
				size = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("width", false)]
	public int Width
	{
		get
		{
			return Size.x;
		}
		set
		{
			if (Width != value)
			{
				Vector2i vector2i = Size;
				vector2i.x = value;
				Size = vector2i;
			}
		}
	}

	[XuiXmlAttribute("height", false)]
	public int Height
	{
		get
		{
			return Size.y;
		}
		set
		{
			if (Height != value)
			{
				Vector2i vector2i = Size;
				vector2i.y = value;
				Size = vector2i;
			}
		}
	}

	[XuiXmlAttribute("pos", false)]
	public Vector2i Position
	{
		get
		{
			return position;
		}
		set
		{
			if (!(position == value))
			{
				position = value;
				SetDirty();
				positionDirty = true;
			}
		}
	}

	[XuiXmlAttribute("ignoreparentpadding", false)]
	public bool IgnoreParentPadding
	{
		get
		{
			return ignoreParentPadding;
		}
		set
		{
			if (ignoreParentPadding != value)
			{
				ignoreParentPadding = value;
				SetDirty();
			}
		}
	}

	public Vector2i PaddedPosition => position + (ignoreParentPadding ? Vector2i.zero : (Controller.Parent?.ViewComponent?.InnerPosition ?? Vector2i.zero));

	public virtual Vector2i InnerSize => new Vector2i(size.x - padding.SumLeftRight, size.y - padding.SumTopBottom);

	public Vector2i InnerPosition => new Vector2i(padding.Left, -padding.Top);

	[XuiXmlAttribute("rotation", false)]
	public float Rotation
	{
		get
		{
			return rotation;
		}
		set
		{
			if (!Mathf.Approximately(rotation, value))
			{
				rotation = value;
				SetDirty();
				rotationDirty = true;
			}
		}
	}

	public int Depth
	{
		get
		{
			return depth;
		}
		set
		{
			if (depth != value)
			{
				depth = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("sound_play_on_press", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool SoundPlayOnClick { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool SoundPlayOnHover { get; set; }

	[XuiXmlAttribute("sound_play_on_open", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool SoundPlayOnOpen { get; set; }

	[XuiXmlAttribute("sound_volume", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public float SoundVolume { get; set; }

	public bool HasHoverSound => xuiHoverSound != null;

	[XuiXmlAttribute("hold_delay", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public float HoldDelay
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = 0.5f;

	[XuiXmlAttribute("hold_timed_initial_interval", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public float HoldEventIntervalInitial
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = 0.5f;

	[XuiXmlAttribute("hold_timed_final_interval", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public float HoldEventIntervalFinal
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = 0.06f;

	[XuiXmlAttribute("hold_timed_step_acceleration", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public float HoldEventIntervalAcceleration
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = 0.015f;

	[XuiXmlAttribute("on_hover", false)]
	public bool EventOnHover
	{
		get
		{
			return eventOnHover;
		}
		set
		{
			if (eventOnHover != value)
			{
				eventOnHover = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("on_press", false)]
	public bool EventOnPress
	{
		get
		{
			return eventOnPress;
		}
		set
		{
			if (eventOnPress != value)
			{
				eventOnPress = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("on_doubleclick", false)]
	public bool EventOnDoubleClick
	{
		get
		{
			return eventOnDoubleClick;
		}
		set
		{
			if (eventOnDoubleClick != value)
			{
				eventOnDoubleClick = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("on_held", false)]
	public bool EventOnHeld
	{
		get
		{
			return eventOnHeld;
		}
		set
		{
			if (eventOnHeld != value)
			{
				eventOnHeld = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("on_scroll", false)]
	public bool EventOnScroll
	{
		get
		{
			return eventOnScroll;
		}
		set
		{
			if (eventOnScroll != value)
			{
				eventOnScroll = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("on_drag", false)]
	public bool EventOnDrag
	{
		get
		{
			return eventOnDrag;
		}
		set
		{
			if (eventOnDrag != value)
			{
				eventOnDrag = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("on_select", false)]
	public bool EventOnSelect
	{
		get
		{
			return eventOnSelect;
		}
		set
		{
			if (eventOnSelect != value)
			{
				eventOnSelect = value;
				SetDirty();
			}
		}
	}

	public virtual bool HasAnyEvent
	{
		get
		{
			if (!EventOnPress && !EventOnDoubleClick && !EventOnHover && !EventOnHeld && !EventOnDrag && !EventOnScroll && !EventOnSelect)
			{
				return !string.IsNullOrEmpty(ToolTip);
			}
			return true;
		}
	}

	public bool IsHovered => isOver;

	public bool IsNavigatable
	{
		get
		{
			if (isNavigatable && Enabled && IsVisible)
			{
				return IsActiveInHierarchy;
			}
			return false;
		}
		set
		{
			isNavigatable = value;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsSnappable { get; set; } = true;

	[XuiXmlAttribute("use_selection_box", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool UseSelectionBox { get; set; } = true;

	[XuiXmlAttribute("nav_up", false)]
	public string NavUpTargetString
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return navUpTargetString;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (!(navUpTargetString == value))
			{
				navUpTargetString = value;
				SetDirty();
			}
		}
	}

	public XUiView NavUpTarget
	{
		get
		{
			return navUpTarget;
		}
		set
		{
			navUpTarget = value;
			foreach (XUiController child in Controller.Children)
			{
				child.ViewComponent.NavUpTarget = value;
			}
		}
	}

	[XuiXmlAttribute("nav_down", false)]
	public string NavDownTargetString
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return navDownTargetString;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (!(navDownTargetString == value))
			{
				navDownTargetString = value;
				SetDirty();
			}
		}
	}

	public XUiView NavDownTarget
	{
		get
		{
			return navDownTarget;
		}
		set
		{
			navDownTarget = value;
			foreach (XUiController child in Controller.Children)
			{
				child.ViewComponent.navDownTarget = value;
			}
		}
	}

	[XuiXmlAttribute("nav_left", false)]
	public string NavLeftTargetString
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return navLeftTargetString;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (!(navLeftTargetString == value))
			{
				navLeftTargetString = value;
				SetDirty();
			}
		}
	}

	public XUiView NavLeftTarget
	{
		get
		{
			return navLeftTarget;
		}
		set
		{
			navLeftTarget = value;
			foreach (XUiController child in Controller.Children)
			{
				child.ViewComponent.navLeftTarget = value;
			}
		}
	}

	[XuiXmlAttribute("nav_right", false)]
	public string NavRightTargetString
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return navRightTargetString;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (!(navRightTargetString == value))
			{
				navRightTargetString = value;
				SetDirty();
			}
		}
	}

	public XUiView NavRightTarget
	{
		get
		{
			return navRightTarget;
		}
		set
		{
			navRightTarget = value;
			foreach (XUiController child in Controller.Children)
			{
				child.ViewComponent.navRightTarget = value;
			}
		}
	}

	[XuiXmlAttribute("tooltip", false)]
	public string ToolTip
	{
		get
		{
			return toolTip;
		}
		set
		{
			if (!(toolTip == value))
			{
				if (GameManager.Instance.GameIsFocused && enabled && isOver && xui.ToolTipWindow != null && xui.ToolTipWindow.ToolTip == toolTip)
				{
					xui.ToolTipWindow.ToolTip = value;
				}
				toolTip = value;
			}
		}
	}

	[XuiXmlAttribute("disabled_tooltip", false)]
	public string DisabledToolTip
	{
		get
		{
			return disabledToolTip;
		}
		set
		{
			if (!(disabledToolTip == value))
			{
				if (GameManager.Instance.GameIsFocused && enabled && isOver && xui.ToolTipWindow != null && xui.ToolTipWindow.ToolTip == disabledToolTip)
				{
					xui.ToolTipWindow.ToolTip = value;
				}
				disabledToolTip = value;
			}
		}
	}

	[XuiXmlAttribute("anchor_left", false)]
	public string AnchorLeft
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return anchorLeft;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (!(anchorLeft == value))
			{
				anchorLeft = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("anchor_right", false)]
	public string AnchorRight
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return anchorRight;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (!(anchorRight == value))
			{
				anchorRight = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("anchor_bottom", false)]
	public string AnchorBottom
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return anchorBottom;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (!(anchorBottom == value))
			{
				anchorBottom = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("anchor_top", false)]
	public string AnchorTop
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return anchorTop;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (!(anchorTop == value))
			{
				anchorTop = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("name", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeName(string _value)
	{
		id = _value;
	}

	[XuiXmlAttribute("depth", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeDepth(int _value)
	{
		Depth = _value + (controller.Parent.ViewComponent?.Depth ?? 0);
	}

	[XuiXmlAttribute("padding_left", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributePaddingLeft(int _value)
	{
		padding = padding.SetLeft(_value);
	}

	[XuiXmlAttribute("padding_right", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributePaddingRight(int _value)
	{
		padding = padding.SetRight(_value);
	}

	[XuiXmlAttribute("padding_top", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributePaddingTop(int _value)
	{
		padding = padding.SetTop(_value);
	}

	[XuiXmlAttribute("padding_bottom", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributePaddingBottom(int _value)
	{
		padding = padding.SetBottom(_value);
	}

	[XuiXmlAttribute("padding", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributePadding(string _value)
	{
		XUiSideSizes.TryParse(_value, out padding, "padding");
	}

	[XuiXmlAttribute("sound", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeSound(string _value)
	{
		xui.LoadData(_value, [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
		{
			xuiSound = _o;
		});
	}

	[XuiXmlAttribute("sound_play_on_hover", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeSoundOnHover(string _value)
	{
		xui.LoadData(_value, [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
		{
			xuiHoverSound = _o;
		});
	}

	[XuiXmlAttribute("gamepad_selectable", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeGamepadSelectable(bool _value)
	{
		IsNavigatable = _value;
		gamepadSelectableSetFromAttributes = true;
	}

	[XuiXmlAttribute("snap", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeGamepadSnappable(bool _value)
	{
		IsSnappable = _value;
		gamepadSnappableSetFromAttributes = true;
	}

	[XuiXmlAttribute("tooltip_key", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeTooltipKey(string _value)
	{
		ToolTip = Localization.Get(_value);
	}

	[XuiXmlAttribute("disabled_tooltip_key", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeDisabledTooltipKey(string _value)
	{
		DisabledToolTip = Localization.Get(_value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiView(XUi _xui, string _id)
	{
		xui = _xui;
		id = _id;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void createComponents(GameObject _go)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void buildView()
	{
		Type type = GetType();
		if (componentTemplates.TryGetValue(type, out var value))
		{
			uiTransform = UnityEngine.Object.Instantiate(value).transform;
			return;
		}
		if (templatesParent == null)
		{
			Transform transform = xui.playerUI.uiCamera.transform;
			GameObject gameObject = new GameObject("_ViewTemplates");
			gameObject.SetActive(value: false);
			templatesParent = gameObject.transform;
			templatesParent.parent = transform;
		}
		GameObject gameObject2 = new GameObject(type.Name);
		gameObject2.layer = 12;
		gameObject2.transform.parent = templatesParent;
		GameObject gameObject3 = gameObject2;
		gameObject3.AddComponent<BoxCollider>().enabled = false;
		UIEventListener.Get(gameObject3);
		createComponents(gameObject3);
		componentTemplates[type] = gameObject3;
		uiTransform = UnityEngine.Object.Instantiate(gameObject3).transform;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void captureComponents()
	{
		collider = uiTransform.gameObject.GetComponent<BoxCollider>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void addTweeners()
	{
		for (int i = 0; i < Tweeners.Count; i++)
		{
			Tweeners[i].CreateTween(uiTransform.gameObject);
		}
	}

	public virtual void InitView()
	{
		buildView();
		uiTransform.name = id;
		captureComponents();
		addTweeners();
		if (controller.Parent?.ViewComponent != null)
		{
			XUiView viewComponent = controller.Parent.ViewComponent;
			uiTransform.parent = viewComponent.uiTransform;
			uiTransform.localScale = Vector3.one;
			uiTransform.localPosition = new Vector3(PaddedPosition.x, PaddedPosition.y, 0f);
			uiTransform.localEulerAngles = new Vector3(0f, 0f, rotation);
		}
		if (HasAnyEvent)
		{
			collider.enabled = true;
			refreshBoxCollider();
			UIEventListener uIEventListener = UIEventListener.Get(uiTransform.gameObject);
			uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, (UIEventListener.BoolDelegate)([PublicizedFrom(EAccessModifier.Private)] (GameObject _, bool _state) =>
			{
				OnHover(_state);
			}));
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClick));
			uIEventListener.onDoubleClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onDoubleClick, new UIEventListener.VoidDelegate(OnDoubleClick));
			uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDrag));
			uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnPress));
			uIEventListener.onScroll = (UIEventListener.FloatDelegate)Delegate.Combine(uIEventListener.onScroll, new UIEventListener.FloatDelegate(OnScroll));
			uIEventListener.onSelect = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onSelect, new UIEventListener.BoolDelegate(OnSelect));
			uIEventListener.onDragOut = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onDragOut, new UIEventListener.VoidDelegate(OnHeldDragOut));
		}
		if (uiTransform.gameObject.activeSelf != isVisible)
		{
			uiTransform.gameObject.SetActive(isVisible);
		}
		if (!gamepadSelectableSetFromAttributes)
		{
			IsNavigatable = EventOnPress;
		}
		if (!gamepadSnappableSetFromAttributes)
		{
			IsSnappable = EventOnPress;
		}
	}

	public virtual void SetRepeatContentTemplateParams(Dictionary<string, object> _templateParams, int _curRepeatNum)
	{
	}

	public virtual void Cleanup()
	{
		destroyed = true;
	}

	public virtual void Update(float _dt)
	{
		if (isOver && !UiTransformIsHovered)
		{
			using (pmUpdateHover.Auto())
			{
				OnHover(_isOver: false);
			}
		}
		if (isPressed && enabled)
		{
			using (pmUpdatePress.Auto())
			{
				float unscaledTime = Time.unscaledTime;
				if (!isHold)
				{
					isHold = unscaledTime - pressStartTime >= HoldDelay;
					if (isHold)
					{
						holdStartTime = unscaledTime;
						controller.Held(EHoldType.HoldStart, 0f);
						holdEventNextTime = 0f;
						holdEventLastTime = unscaledTime;
						holdEventIntervalChangeSpeed = 0f;
						holdEventIntervalCurrent = HoldEventIntervalInitial;
					}
				}
				else
				{
					controller.Held(EHoldType.Hold, unscaledTime - holdStartTime);
				}
				if (isHold && unscaledTime >= holdEventNextTime)
				{
					holdEventIntervalCurrent = Mathf.SmoothDamp(holdEventIntervalCurrent, HoldEventIntervalFinal, ref holdEventIntervalChangeSpeed, HoldEventIntervalAcceleration, float.PositiveInfinity, _dt);
					holdEventNextTime = unscaledTime + holdEventIntervalCurrent;
					controller.Held(EHoldType.HoldTimed, unscaledTime - holdStartTime, unscaledTime - holdEventLastTime);
					holdEventLastTime = unscaledTime;
				}
			}
		}
		if (isDirty)
		{
			using (pmUpdateData.Auto())
			{
				updateData();
				isDirty = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateData()
	{
		if (positionDirty)
		{
			uiTransform.localPosition = new Vector3(PaddedPosition.x, PaddedPosition.y, uiTransform.localPosition.z);
			positionDirty = false;
		}
		if (rotationDirty)
		{
			uiTransform.localEulerAngles = new Vector3(0f, 0f, rotation);
			rotationDirty = false;
		}
		parseAnchors();
		parseNavigationTargets();
	}

	public void TryUpdatePosition()
	{
		if (positionDirty)
		{
			uiTransform.localPosition = new Vector3(PaddedPosition.x, PaddedPosition.y, uiTransform.localPosition.z);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void refreshBoxCollider()
	{
		collider.center = new Vector3((float)size.x * 0.5f, 0f - (float)size.y * 0.5f, 0f);
		collider.size = new Vector3((float)size.x * ColliderScale + (float)(2 * ColliderPadding.x), (float)size.y * ColliderScale + (float)(2 * ColliderPadding.y), 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetDirty()
	{
		isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void parseAnchors()
	{
		UIRect uiRect = UiRect;
		if (!(uiRect == null))
		{
			if (ParseAnchorString(anchorLeft, ref anchorLeftParsed, uiRect, uiRect.leftAnchor) | ParseAnchorString(anchorRight, ref anchorRightParsed, uiRect, uiRect.rightAnchor) | ParseAnchorString(anchorBottom, ref anchorBottomParsed, uiRect, uiRect.bottomAnchor) | ParseAnchorString(anchorTop, ref anchorTopParsed, uiRect, uiRect.topAnchor))
			{
				anchoredLeftAndRight = (bool)uiRect.leftAnchor.target && (bool)uiRect.rightAnchor.target;
				anchoredTopAndBottom = (bool)uiRect.bottomAnchor.target && (bool)uiRect.topAnchor.target;
				SetDirty();
				uiRect.ResetAnchors();
			}
			anchorsParsed();
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool ParseAnchorString(string _anchorString, ref string _parsedString, UIRect _uiRect, UIRect.AnchorPoint _anchor)
		{
			if (string.IsNullOrEmpty(_anchorString))
			{
				return false;
			}
			if (_anchorString == _parsedString && _anchor.target != null)
			{
				return false;
			}
			_parsedString = _anchorString;
			int num = _anchorString.IndexOf(',');
			if (num < 0)
			{
				throw new ArgumentException("Invalid anchor string '" + _anchorString + "', expected '<target>,<relative>,<absolute>'");
			}
			string text = _anchorString.Substring(0, num);
			int num2 = _anchorString.IndexOf(',', num + 1);
			if (num2 < 0)
			{
				throw new ArgumentException("Invalid anchor string '" + _anchorString + "', expected '<target>,<relative>,<absolute>'");
			}
			float relative = StringParsers.ParseFloat(_anchorString, num + 1, num2 - 1);
			int absolute = StringParsers.ParseSInt32(_anchorString, num2 + 1);
			if (text.Length == 0)
			{
				throw new ArgumentException("Invalid anchor string '" + _anchorString + "', expected '<target>,<relative>,<absolute>'");
			}
			if (text.EqualsCaseInsensitive("#parent"))
			{
				_anchor.target = uiTransform.parent;
			}
			else if (text.EqualsCaseInsensitive("#cam"))
			{
				UICamera componentInParent = uiTransform.gameObject.GetComponentInParent<UICamera>();
				if (componentInParent == null)
				{
					throw new Exception("UICamera not found");
				}
				_anchor.target = componentInParent.transform;
			}
			else if (text[0] == '#')
			{
				string text2 = text.Substring(1);
				if (!EnumUtils.TryParse<UIAnchor.Side>(text2, out var _result, _ignoreCase: true))
				{
					throw new ArgumentException("Invalid anchor side name '" + text2 + "', expected any of '\tBottomLeft,Left,TopLeft,Top,TopRight,Right,BottomRight,Bottom,Center'");
				}
				_anchor.target = xui.GetAnchor(_result).transform;
			}
			else
			{
				XUiView xUiView = XUiUtils.FindHierarchyClosestView(Controller, text);
				if (xUiView == null)
				{
					throw new ArgumentException("Invalid anchor string '" + _anchorString + "', view component with name '" + text + "' not found.\nOn: " + GetXuiHierarchy());
				}
				_anchor.target = xUiView.UiTransform;
				if (xUiView is XUiV_Grid xUiV_Grid)
				{
					xUiV_Grid.OnSizeChanged += [PublicizedFrom(EAccessModifier.Internal)] (Vector2Int _, Vector2 _) =>
					{
						_uiRect.UpdateAnchors();
					};
				}
			}
			_anchor.relative = relative;
			_anchor.absolute = absolute;
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void anchorsParsed()
	{
	}

	public virtual void OnOpen()
	{
		if (xuiSound != null && SoundPlayOnOpen)
		{
			Manager.PlayXUiSound(xuiSound, SoundVolume);
		}
		isPressed = false;
		isHold = false;
	}

	public virtual void OnClose()
	{
		isPressed = false;
		isHold = false;
		if (!GameManager.Instance.IsQuitting)
		{
			CursorControllerAbs cursorController = xui.playerUI.CursorController;
			if (cursorController.HoverTarget == this)
			{
				cursorController.HoverTarget = null;
			}
			if (cursorController.navigationTarget == this)
			{
				controller.Hovered(_isOver: false);
				cursorController.SetNavigationTarget(null);
			}
			if (cursorController.lockNavigationToView == this)
			{
				cursorController.SetNavigationLockView(null);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnScroll(GameObject _go, float _delta)
	{
		if (EventOnScroll)
		{
			controller.Scrolled(_delta);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnSelect(GameObject _go, bool _selected)
	{
		if (EventOnSelect && enabled)
		{
			controller.Selected(_selected);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDrag(GameObject _go, Vector2 _delta)
	{
		if (EventOnDrag && enabled)
		{
			EDragType dragType = (wasDragging ? EDragType.Dragging : EDragType.DragStart);
			wasDragging = true;
			controller.Dragged(_delta, dragType);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPress(GameObject _go, bool _pressed)
	{
		if (EventOnPress && enabled)
		{
			controller.MouseUpDown(_pressed);
		}
		if (EventOnDrag && enabled && !_pressed && wasDragging)
		{
			wasDragging = false;
			controller.Dragged(default(Vector2), EDragType.DragEnd);
		}
		if (!EventOnHeld)
		{
			return;
		}
		if (_pressed && !isPressed && enabled)
		{
			isPressed = true;
			isHold = false;
			pressStartTime = Time.unscaledTime;
			holdStartTime = -1f;
		}
		else if (!_pressed)
		{
			isPressed = false;
			bool num = isHold;
			isHold = false;
			if (num)
			{
				controller.Held(EHoldType.HoldEnd, Time.unscaledTime - holdStartTime);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnHeldDragOut(GameObject _go)
	{
		if (EventOnHeld && isPressed)
		{
			isPressed = false;
			bool num = isHold;
			isHold = false;
			if (num)
			{
				controller.Held(EHoldType.HoldEnd, Time.unscaledTime - holdStartTime);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnClick(GameObject _go)
	{
		if (EventOnPress && enabled)
		{
			if (xuiSound != null && SoundPlayOnClick && UICamera.currentTouchID == -1)
			{
				Manager.PlayXUiSound(xuiSound, SoundVolume);
			}
			controller.Pressed(UICamera.currentTouchID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDoubleClick(GameObject _go)
	{
		if (EventOnDoubleClick && enabled)
		{
			if (xuiSound != null && SoundPlayOnClick && UICamera.currentTouchID == -1)
			{
				Manager.PlayXUiSound(xuiSound, SoundVolume);
			}
			controller.DoubleClicked(UICamera.currentTouchID);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnHover(bool _isOver)
	{
		PlayerActionsLocal playerInput = xui.playerUI.playerInput;
		if (playerInput != null && playerInput.LastDeviceClass == InputDeviceClass.Keyboard && !Cursor.visible)
		{
			_isOver = false;
		}
		bool flag = _isOver && !enabled && !string.IsNullOrEmpty(DisabledToolTip);
		_isOver &= enabled;
		bool flag2 = _isOver && !string.IsNullOrEmpty(ToolTip);
		if (_isOver != isOver && _isOver)
		{
			PlayHoverSound();
		}
		isOver = _isOver;
		if (EventOnHover)
		{
			controller.Hovered(_isOver);
		}
		if (xui.ToolTipWindow != null)
		{
			if (flag2)
			{
				xui.ToolTipWindow.ToolTip = ToolTip;
			}
			else if (flag)
			{
				xui.ToolTipWindow.ToolTip = DisabledToolTip;
			}
			else
			{
				xui.ToolTipWindow.ToolTip = "";
			}
		}
		xui.playerUI.CursorController.HoverTarget = (_isOver ? this : null);
	}

	public void PlayHoverSound()
	{
		if (xuiHoverSound != null && SoundPlayOnHover && enabled && GameManager.Instance.GameIsFocused)
		{
			Manager.PlayXUiSound(xuiHoverSound, SoundVolume);
		}
	}

	public void PlayClickSound()
	{
		if (xuiSound != null)
		{
			Manager.PlayXUiSound(xuiSound, SoundVolume);
		}
	}

	public Rect GetXUiRect()
	{
		return GetXUiRect(Vector2.zero);
	}

	public Rect GetXUiRect(Vector2 _padding)
	{
		XUiController parent = controller;
		Vector3 localPosition = uiTransform.localPosition;
		localPosition.x -= _padding.x;
		localPosition.y += _padding.y;
		while (parent.Parent != null && parent.Parent.ViewComponent != null)
		{
			parent = parent.Parent;
			localPosition += parent.ViewComponent.uiTransform.localPosition;
		}
		localPosition += parent.ViewComponent.uiTransform.parent.localPosition;
		Vector2 vector = Size.AsVector2() + 2f * _padding;
		if (parent.ViewComponent is XUiV_Window { IsInStackPanel: not false } xUiV_Window)
		{
			Transform parent2 = xUiV_Window.uiTransform.parent.parent;
			localPosition *= parent2.localScale.x;
			vector *= parent2.localScale.x;
			localPosition += parent2.localPosition;
		}
		return new Rect(new Vector2(localPosition.x, localPosition.y - vector.y), vector);
	}

	public string GetXuiHierarchy()
	{
		return XUiUtils.GetXuiHierarchy(Controller);
	}

	public Vector2 GetClosestPoint(Vector3 _point)
	{
		return collider.ClosestPointOnBounds(_point);
	}

	public void ClearNavigationTargets()
	{
		XUiView xUiView = (NavRightTarget = null);
		XUiView xUiView3 = (NavLeftTarget = xUiView);
		XUiView xUiView5 = (NavDownTarget = xUiView3);
		NavUpTarget = xUiView5;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void parseNavigationTargets()
	{
		if (!string.IsNullOrEmpty(navUpTargetString))
		{
			NavUpTarget = FindView(navUpTargetString);
		}
		if (!string.IsNullOrEmpty(navDownTargetString))
		{
			NavDownTarget = FindView(navDownTargetString);
		}
		if (!string.IsNullOrEmpty(navLeftTargetString))
		{
			NavLeftTarget = FindView(navLeftTargetString);
		}
		if (!string.IsNullOrEmpty(navRightTargetString))
		{
			NavRightTarget = FindView(navRightTargetString);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		XUiView FindView(string _name)
		{
			if (string.IsNullOrEmpty(_name))
			{
				return null;
			}
			XUiView xUiView = XUiUtils.FindHierarchyClosestView(Controller, _name);
			if (xUiView != null)
			{
				return xUiView;
			}
			xUiView = controller.WindowGroup.Controller.GetChildById(_name)?.ViewComponent;
			if (xUiView != null)
			{
				return xUiView;
			}
			Log.Error("Invalid navigation target, view component with name '" + _name + "' not found.\nOn: " + GetXuiHierarchy());
			return null;
		}
	}

	public virtual void SetDefaults(XUiController _parent)
	{
		RepeatContent = false;
		RepeatCount = 1;
		Size = Vector2i.min;
		Depth = (_parent?.ViewComponent?.Depth).GetValueOrDefault();
		ToolTip = "";
		SoundPlayOnClick = true;
		SoundPlayOnOpen = false;
		SoundPlayOnHover = true;
		SoundVolume = 1f;
	}

	public virtual void SetPostParsingDefaults(XUiController _parent)
	{
		XUiView xUiView = _parent?.ViewComponent;
		Vector2i vector2i = Size;
		if (vector2i.x == int.MinValue)
		{
			vector2i.x = ((!ignoreParentPadding) ? (xUiView?.InnerSize.x ?? 0) : (xUiView?.Size.x ?? 0));
		}
		if (vector2i.y == int.MinValue)
		{
			vector2i.y = ((!ignoreParentPadding) ? (xUiView?.InnerSize.y ?? 0) : (xUiView?.Size.y ?? 0));
		}
		Size = vector2i;
	}

	public void ParseInitialAttributeValue(string _attribute, string _value)
	{
		if (_value.Contains("{"))
		{
			BindingsManager.CreateBinding(this, _attribute, _value);
		}
		else if (!ParsingMethodCache.Instance.TryParseDirect(this, _attribute, _value) && (Controller == null || !ParsingMethodCache.Instance.TryParseDirect(Controller, _attribute, _value)))
		{
			ParseAttributeViewAndController(_attribute, _value);
		}
	}

	public bool ParseAttributeViewAndController(string _attribute, string _value)
	{
		if (_value.Contains("{") && XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
		{
			Log.Warning("[XUi] Refreshed binding contained '{': " + _attribute + "='" + _value + "' on " + id);
		}
		if (Controller != null)
		{
			if (!Controller.ParseAttribute(_attribute, _value))
			{
				Controller.CustomAttributes[_attribute] = _value;
			}
			return true;
		}
		return false;
	}

	public virtual void OnVisibilityChanged(bool _visibleInScene)
	{
		if (isOver && !_visibleInScene)
		{
			OnHover(_isOver: false);
		}
	}
}
