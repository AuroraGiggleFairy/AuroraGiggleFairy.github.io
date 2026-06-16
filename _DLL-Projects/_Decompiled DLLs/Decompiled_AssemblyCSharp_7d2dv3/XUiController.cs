using System;
using System.Collections.Generic;
using Platform;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiController : IXUiElement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiView viewComponent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController parent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly List<XUiController> children = new List<XUiController>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiWindowGroup windowGroup;

	public bool AlwaysUpdate;

	public bool IsDirty;

	public object CustomData;

	public readonly ObservableDictionary<string, object> CustomAttributes = new ObservableDictionary<string, object>();

	public XUi xui;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerMarker pmControllerUpdate = new ProfilerMarker("XC.Update");

	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerMarker pmVcUpdate = new ProfilerMarker("VC");

	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerMarker pmVcNamedUpdate;

	public readonly BindingsManager Bindings = new BindingsManager();

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerInputManager.InputStyle lastInputStyle = PlayerInputManager.InputStyle.Count;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerInputManager.InputStyle curInputStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool registeredForInputStyleChanges;

	public XUiView ViewComponent
	{
		get
		{
			return viewComponent;
		}
		set
		{
			viewComponent = value;
		}
	}

	public XUiController Parent
	{
		get
		{
			return parent;
		}
		set
		{
			if (parent != value)
			{
				parent?.children.Remove(this);
				parent = value;
				parent?.children.Add(this);
			}
		}
	}

	public List<XUiController> Children => children;

	public XUiWindowGroup WindowGroup
	{
		get
		{
			return windowGroup;
		}
		set
		{
			windowGroup = value;
		}
	}

	public bool IsOpen => windowGroup.isShowing;

	[XuiXmlBinding("is_online")]
	public bool IsOnline => PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.LoggedIn;

	public PlayerInputManager.InputStyle CurrentInputStyle
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return lastInputStyle;
		}
	}

	public XUiController Controller => this;

	public event XUiEvent_OnMouseUpDownEventHandler OnMouseUpDown;

	public event XUiEvent_OnPressEventHandler OnPress;

	public event XUiEvent_OnPressEventHandler OnDoubleClick;

	public event XUiEvent_OnPressEventHandler OnRightPress;

	public event XUiEvent_OnHoverEventHandler OnHover;

	public event XUiEvent_OnDragEventHandler OnDrag;

	public event XUiEvent_OnHeldHandler OnHold;

	public event XUiEvent_OnScrollEventHandler OnScroll;

	public event XUiEvent_OnSelectEventHandler OnSelect;

	public event XUiEvent_OnVisibilityChanged OnVisiblity;

	public event XUiEvent_OnEnabledChanged OnEnabled;

	public event XUiEvent_OnInteraction OnInteraction;

	public void AutoBindComponents()
	{
		AutoBindCache.Instance.BindComponents(this);
	}

	public void AutoBindEvents()
	{
		AutoBindCache.Instance.BindEvents(this);
	}

	public virtual void Init()
	{
		if (viewComponent != null)
		{
			viewComponent.InitView();
		}
		for (int i = 0; i < children.Count; i++)
		{
			children[i].AutoBindComponents();
			children[i].Init();
			children[i].AutoBindEvents();
		}
		curInputStyle = PlatformManager.NativePlatform.Input.CurrentInputStyle;
	}

	public virtual void Update(float _dt)
	{
		XUiView xUiView = viewComponent;
		bool isShowing = windowGroup.isShowing;
		if (curInputStyle != lastInputStyle)
		{
			PlayerInputManager.InputStyle oldStyle = lastInputStyle;
			lastInputStyle = curInputStyle;
			RefreshBindings();
			inputStyleChanged(oldStyle, lastInputStyle);
		}
		if (isShowing && xUiView != null && xUiView.IsVisible)
		{
			using (pmVcUpdate.Auto())
			{
				using (pmVcNamedUpdate.Auto())
				{
					xUiView.Update(_dt);
				}
			}
		}
		List<XUiController> list = children;
		for (int i = 0; i < list.Count; i++)
		{
			XUiController xUiController = list[i];
			if (isShowing || xUiController.AlwaysUpdate)
			{
				xUiController.Update(_dt);
			}
		}
	}

	public bool TryGetChildController(Type _controllerType, ReadOnlyMemory<char> _id, out XUiController _child)
	{
		if ((_id.IsEmpty || (viewComponent != null && MemoryExtensions.Equals(_id.Span, viewComponent.ID, StringComparison.OrdinalIgnoreCase))) && _controllerType.IsInstanceOfType(this))
		{
			_child = this;
			return true;
		}
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i].TryGetChildController(_controllerType, _id, out _child))
			{
				return true;
			}
		}
		_child = null;
		return false;
	}

	public bool TryGetChildController<T>(ReadOnlyMemory<char> _id, out T _child) where T : XUiController
	{
		if ((_id.IsEmpty || (viewComponent != null && MemoryExtensions.Equals(_id.Span, viewComponent.ID, StringComparison.OrdinalIgnoreCase))) && this is T val)
		{
			_child = val;
			return true;
		}
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i].TryGetChildController<T>(_id, out _child))
			{
				return true;
			}
		}
		_child = null;
		return false;
	}

	public bool TryGetChildController<T>(string _id, out T _child) where T : XUiController
	{
		return TryGetChildController<T>(_id.AsMemory(), out _child);
	}

	public XUiController[] GetChildControllers(Type _controllerType, ReadOnlyMemory<char> _id, List<XUiController> _list = null)
	{
		List<XUiController> list = _list ?? new List<XUiController>();
		if ((_id.IsEmpty || (viewComponent != null && MemoryExtensions.Equals(_id.Span, viewComponent.ID, StringComparison.OrdinalIgnoreCase))) && _controllerType.IsInstanceOfType(this))
		{
			list.Add(this);
		}
		else
		{
			for (int i = 0; i < children.Count; i++)
			{
				children[i].GetChildControllers(_controllerType, _id, list);
			}
		}
		if (_list != null)
		{
			return null;
		}
		return list.ToArray();
	}

	public T[] GetChildControllers<T>(ReadOnlyMemory<char> _id, List<T> _list = null) where T : XUiController
	{
		List<T> list = _list ?? new List<T>();
		if ((_id.IsEmpty || (viewComponent != null && MemoryExtensions.Equals(_id.Span, viewComponent.ID, StringComparison.OrdinalIgnoreCase))) && this is T item)
		{
			list.Add(item);
		}
		else
		{
			for (int i = 0; i < children.Count; i++)
			{
				children[i].GetChildControllers(_id, list);
			}
		}
		if (_list != null)
		{
			return null;
		}
		return list.ToArray();
	}

	public T[] GetChildControllers<T>(string _id, List<T> _list = null) where T : XUiController
	{
		return GetChildControllers(_id.AsMemory(), _list);
	}

	public bool TryGetChildView(Type _viewType, ReadOnlyMemory<char> _id, out XUiView _child)
	{
		if (viewComponent != null && (_id.IsEmpty || MemoryExtensions.Equals(_id.Span, viewComponent.ID, StringComparison.OrdinalIgnoreCase)) && _viewType.IsInstanceOfType(viewComponent))
		{
			_child = viewComponent;
			return true;
		}
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i].TryGetChildView(_viewType, _id, out _child))
			{
				return true;
			}
		}
		_child = null;
		return false;
	}

	public bool TryGetChildView<T>(ReadOnlyMemory<char> _id, out T _child) where T : XUiView
	{
		if (viewComponent is T val && (_id.IsEmpty || MemoryExtensions.Equals(_id.Span, val.ID, StringComparison.OrdinalIgnoreCase)))
		{
			_child = val;
			return true;
		}
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i].TryGetChildView<T>(_id, out _child))
			{
				return true;
			}
		}
		_child = null;
		return false;
	}

	public bool TryGetChildView<T>(string _id, out T _child) where T : XUiView
	{
		return TryGetChildView<T>(_id.AsMemory(), out _child);
	}

	public XUiView[] GetChildViews(Type _viewType, ReadOnlyMemory<char> _id, List<XUiView> _list = null)
	{
		List<XUiView> list = _list ?? new List<XUiView>();
		if (viewComponent != null && (_id.IsEmpty || MemoryExtensions.Equals(_id.Span, viewComponent.ID, StringComparison.OrdinalIgnoreCase)) && _viewType.IsInstanceOfType(viewComponent))
		{
			list.Add(viewComponent);
		}
		else
		{
			for (int i = 0; i < children.Count; i++)
			{
				children[i].GetChildViews(_viewType, _id, list);
			}
		}
		if (_list != null)
		{
			return null;
		}
		return list.ToArray();
	}

	public T[] GetChildViews<T>(ReadOnlyMemory<char> _id, List<T> _list = null) where T : XUiView
	{
		List<T> list = _list ?? new List<T>();
		if (viewComponent is T val && (_id.IsEmpty || MemoryExtensions.Equals(_id.Span, val.ID, StringComparison.OrdinalIgnoreCase)))
		{
			list.Add(val);
		}
		else
		{
			for (int i = 0; i < children.Count; i++)
			{
				children[i].GetChildViews(_id, list);
			}
		}
		if (_list != null)
		{
			return null;
		}
		return list.ToArray();
	}

	public T[] GetChildViews<T>(string _id, List<T> _list = null) where T : XUiView
	{
		return GetChildViews(_id.AsMemory(), _list);
	}

	public bool TryGetChildByIdAndType<T>(string _id, out T _child) where T : XUiController
	{
		if (viewComponent != null && string.Equals(viewComponent.ID, _id, StringComparison.OrdinalIgnoreCase) && this is T val)
		{
			_child = val;
			return true;
		}
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i].TryGetChildByIdAndType<T>(_id, out _child))
			{
				return true;
			}
		}
		_child = null;
		return false;
	}

	public XUiController GetChildById(string _id)
	{
		if (viewComponent != null && string.Equals(viewComponent.ID, _id, StringComparison.OrdinalIgnoreCase))
		{
			return this;
		}
		for (int i = 0; i < children.Count; i++)
		{
			XUiController childById = children[i].GetChildById(_id);
			if (childById != null)
			{
				return childById;
			}
		}
		return null;
	}

	public XUiController[] GetChildrenById(string _id, List<XUiController> _list = null)
	{
		List<XUiController> list = _list ?? new List<XUiController>();
		if (viewComponent != null && string.Equals(viewComponent.ID, _id, StringComparison.OrdinalIgnoreCase))
		{
			list.Add(this);
		}
		else
		{
			for (int i = 0; i < children.Count; i++)
			{
				children[i].GetChildrenById(_id, list);
			}
		}
		if (_list != null)
		{
			return null;
		}
		return list.ToArray();
	}

	public T GetChildByType<T>() where T : XUiController
	{
		if (this is T result)
		{
			return result;
		}
		foreach (XUiController child in children)
		{
			T childByType = child.GetChildByType<T>();
			if (childByType != null)
			{
				return childByType;
			}
		}
		return null;
	}

	public T[] GetChildrenByType<T>(List<T> _list = null) where T : XUiController
	{
		List<T> list = _list ?? new List<T>();
		if (this is T item)
		{
			list.Add(item);
		}
		else
		{
			foreach (XUiController child in children)
			{
				child.GetChildrenByType(list);
			}
		}
		if (_list != null)
		{
			return null;
		}
		return list.ToArray();
	}

	public T[] GetChildrenByViewType<T>(List<T> _list = null) where T : XUiView
	{
		List<T> list = _list ?? new List<T>();
		if (viewComponent is T item)
		{
			list.Add(item);
		}
		else
		{
			foreach (XUiController child in children)
			{
				child.GetChildrenByViewType(list);
			}
		}
		if (_list != null)
		{
			return null;
		}
		return list.ToArray();
	}

	public T GetParentByType<T>() where T : XUiController
	{
		if (this is T result)
		{
			return result;
		}
		XUiController xUiController = Parent;
		if (xUiController == null)
		{
			return null;
		}
		return xUiController.GetParentByType<T>();
	}

	public bool TryGetParentController(Type _controllerType, out XUiController _parent)
	{
		if (_controllerType.IsInstanceOfType(this))
		{
			_parent = this;
			return true;
		}
		if (Parent != null)
		{
			return Parent.TryGetParentController(_controllerType, out _parent);
		}
		_parent = null;
		return false;
	}

	public bool TryGetParentController<T>(out T _parent) where T : XUiController
	{
		if (this is T val)
		{
			_parent = val;
			return true;
		}
		if (Parent != null)
		{
			return Parent.TryGetParentController<T>(out _parent);
		}
		_parent = null;
		return false;
	}

	public bool TryGetParentView(Type _viewType, out XUiView _parent)
	{
		if (_viewType.IsInstanceOfType(viewComponent))
		{
			_parent = viewComponent;
			return true;
		}
		if (Parent != null)
		{
			return Parent.TryGetParentView(_viewType, out _parent);
		}
		_parent = null;
		return false;
	}

	public bool TryGetParentView<T>(out T _parent) where T : XUiView
	{
		if (viewComponent is T val)
		{
			_parent = val;
			return true;
		}
		if (Parent != null)
		{
			return Parent.TryGetParentView<T>(out _parent);
		}
		_parent = null;
		return false;
	}

	public bool IsChildOf(XUiController _controller)
	{
		if (Parent == null)
		{
			return false;
		}
		if (Parent == _controller)
		{
			return true;
		}
		return Parent.IsChildOf(_controller);
	}

	public bool IsSelfOrChildOf(XUiController _controller)
	{
		if (this == _controller)
		{
			return true;
		}
		return IsChildOf(_controller);
	}

	public XUiV_Window GetParentWindow()
	{
		if (ViewComponent is XUiV_Window result)
		{
			return result;
		}
		return Parent?.GetParentWindow();
	}

	public void MouseUpDown(bool _pressed)
	{
		this.OnMouseUpDown?.Invoke(this, _pressed);
		this.OnInteraction?.Invoke(this, EXUiControllerInteractionType.MouseUpDown);
	}

	public virtual void Pressed(int _mouseButton)
	{
		switch (_mouseButton)
		{
		case -1:
			this.OnPress?.Invoke(this, _mouseButton);
			this.OnInteraction?.Invoke(this, EXUiControllerInteractionType.LeftClick);
			break;
		case -2:
			this.OnRightPress?.Invoke(this, _mouseButton);
			this.OnInteraction?.Invoke(this, EXUiControllerInteractionType.RightClick);
			break;
		}
	}

	public void DoubleClicked(int _mouseButton)
	{
		this.OnDoubleClick?.Invoke(this, _mouseButton);
		this.OnInteraction?.Invoke(this, EXUiControllerInteractionType.DoubleClick);
	}

	public void Hovered(bool _isOver)
	{
		this.OnHover?.Invoke(this, _isOver);
		this.OnInteraction?.Invoke(this, EXUiControllerInteractionType.Hover);
		OnHovered(_isOver);
	}

	public void Scrolled(float _delta)
	{
		this.OnScroll?.Invoke(this, _delta);
		this.OnInteraction?.Invoke(this, EXUiControllerInteractionType.Scroll);
	}

	public void Selected(bool _selected)
	{
		this.OnSelect?.Invoke(this, _selected);
		this.OnInteraction?.Invoke(this, EXUiControllerInteractionType.Select);
	}

	public void Dragged(Vector2 _mouseDelta, EDragType _dragType)
	{
		if (viewComponent.ID == "btn2")
		{
			Log.Warning($"[XUi] F={Time.frameCount} ButtonC received Dragged={_dragType} event");
		}
		this.OnDrag?.Invoke(this, _dragType, _mouseDelta);
		this.OnInteraction?.Invoke(this, EXUiControllerInteractionType.Drag);
	}

	public void Held(EHoldType _event, float _holdDuration, float _deltaSinceLastTimedEvent = -1f)
	{
		this.OnHold?.Invoke(this, _event, _holdDuration, _deltaSinceLastTimedEvent);
		this.OnInteraction?.Invoke(this, EXUiControllerInteractionType.Held);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnHovered(bool _isOver)
	{
	}

	public virtual void OnOpen()
	{
		for (int i = 0; i < children.Count; i++)
		{
			children[i].OnOpen();
		}
		ViewComponent?.OnOpen();
		this.OnInteraction?.Invoke(this, EXUiControllerInteractionType.Open);
		RefreshBindings();
	}

	public virtual void OnClose()
	{
		for (int i = 0; i < children.Count; i++)
		{
			children[i].OnClose();
		}
		ViewComponent?.OnClose();
		this.OnInteraction?.Invoke(this, EXUiControllerInteractionType.Close);
	}

	public void OnVisibilityChanged(bool _parentVisibleInScene)
	{
		bool isVisible = viewComponent.IsVisible;
		bool flag = isVisible && _parentVisibleInScene;
		this.OnVisiblity?.Invoke(this, isVisible, flag);
		this.OnInteraction?.Invoke(this, EXUiControllerInteractionType.VisibilityChanged);
		for (int i = 0; i < children.Count; i++)
		{
			children[i].OnVisibilityChanged(flag);
			children[i].viewComponent.OnVisibilityChanged(flag);
		}
	}

	public void OnEnabledChanged(bool _enabled)
	{
		this.OnEnabled?.Invoke(this, _enabled);
		this.OnInteraction?.Invoke(this, EXUiControllerInteractionType.EnabledChanged);
	}

	public virtual void OnCursorSelected(bool _isActualElement)
	{
		if (_isActualElement && TryGetParentView<XUiV_ScrollView>(out var _parent))
		{
			_parent.MakeVisible(ViewComponent);
		}
		parent?.OnCursorSelected(_isActualElement: false);
	}

	public virtual void OnCursorUnSelected()
	{
		parent?.OnCursorUnSelected();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool handleDirtyUpdateDefault()
	{
		if (!IsDirty)
		{
			return false;
		}
		IsDirty = false;
		RefreshBindings();
		return true;
	}

	public virtual bool ParseAttribute(string _name, string _value)
	{
		if (_name == "always_update")
		{
			StringParsers.TryParseBool(_value, out AlwaysUpdate);
			return true;
		}
		return false;
	}

	public bool GetBindingValue(ref string _value, string _bindingName)
	{
		try
		{
			return GetBindingValueInternal(ref _value, _bindingName);
		}
		catch
		{
			Log.Error("Unhandled exception in GetBindingValue. Binding name: " + _bindingName + ", hierarchy: " + GetXuiHierarchy());
			throw;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "is_world_loaded":
			_value = (GameManager.Instance.World != null).ToString();
			return true;
		case "is_unityeditor":
			_value = "false";
			return true;
		case "is_release":
			_value = "true";
			return true;
		case "is_creative":
			_value = (GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled)).ToString();
			return true;
		case "is_map_enabled":
			_value = World.MapEnabled.ToString();
			return true;
		case "is_challenge_enabled":
			_value = (ChallengeJournal.AllowChallenges || World.BiomeProgressionEnabled).ToString();
			return true;
		case "gamelanguage":
			_value = Localization.ActiveLanguage;
			return true;
		case "is_editmode":
			_value = GameManager.Instance.IsEditMode().ToString();
			return true;
		case "is_server":
			_value = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer.ToString();
			return true;
		case "is_prefab_editor":
			_value = PrefabEditModeManager.Instance.IsActive().ToString();
			return true;
		case "is_playtesting":
			_value = GameUtils.IsPlaytesting().ToString();
			return true;
		case "is_modal":
			_value = WindowGroup.isModal.ToString();
			return true;
		case "inputstyle":
			registerForInputStyleChanges();
			_value = lastInputStyle.ToStringCached();
			return true;
		case "is_controller_input":
			registerForInputStyleChanges();
			_value = (lastInputStyle != PlayerInputManager.InputStyle.Keyboard).ToString();
			return true;
		case "is_console":
			_value = (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent().ToString();
			return true;
		case "is_ps5":
			_value = DeviceFlag.PS5.IsCurrent().ToString();
			return true;
		case "version_long":
			_value = Constants.cVersionInformation.LongString;
			return true;
		default:
			return false;
		}
	}

	public void RefreshBindingsSelfAndChildren()
	{
		for (int i = 0; i < children.Count; i++)
		{
			children[i].RefreshBindingsSelfAndChildren();
		}
		Bindings.RefreshBindings();
	}

	public void RefreshBindings()
	{
		Bindings.RefreshBindings();
	}

	public virtual void Cleanup()
	{
		foreach (XUiController child in children)
		{
			child.Cleanup();
		}
		if (registeredForInputStyleChanges && PlatformManager.NativePlatform?.Input != null)
		{
			PlatformManager.NativePlatform.Input.OnLastInputStyleChanged -= OnLastInputStyleChanged;
		}
		ViewComponent?.Cleanup();
	}

	public void FindNavigatableChildren(List<XUiView> _views)
	{
		foreach (XUiController child in children)
		{
			if (child.viewComponent.IsNavigatable)
			{
				_views.Add(child.viewComponent);
			}
			child.FindNavigatableChildren(_views);
		}
	}

	public bool TryFindFirstNavigableView(out XUiView _foundView)
	{
		_foundView = null;
		bool isActiveInHierarchy = ViewComponent.IsActiveInHierarchy;
		if (ViewComponent.IsNavigatable && ViewComponent.IsVisible && isActiveInHierarchy)
		{
			_foundView = ViewComponent;
			return true;
		}
		if (!isActiveInHierarchy)
		{
			return false;
		}
		foreach (XUiController child in children)
		{
			if (child.TryFindFirstNavigableView(out _foundView))
			{
				return true;
			}
		}
		return false;
	}

	public bool SelectCursorElement(bool _withDelay = false, bool _overrideCursorMode = false)
	{
		if (ViewComponent == null)
		{
			return false;
		}
		CursorControllerAbs cursorController = xui.playerUI.CursorController;
		if (cursorController.CursorModeActive && !_overrideCursorMode)
		{
			return false;
		}
		TryFindFirstNavigableView(out var _foundView);
		if (_foundView == null)
		{
			return false;
		}
		if (_withDelay)
		{
			cursorController.SetNavigationTargetLater(_foundView);
		}
		else
		{
			cursorController.SetNavigationTarget(_foundView);
		}
		return true;
	}

	public void SetAllChildrenDirty()
	{
		for (int i = 0; i < children.Count; i++)
		{
			children[i].SetAllChildrenDirty();
		}
		viewComponent?.SetDirty();
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void registerForInputStyleChanges()
	{
		if (!registeredForInputStyleChanges)
		{
			registeredForInputStyleChanges = true;
			PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += OnLastInputStyleChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLastInputStyleChanged(PlayerInputManager.InputStyle _style)
	{
		curInputStyle = _style;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void inputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
	}

	public void ForceInputStyleChange(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		if (registeredForInputStyleChanges)
		{
			inputStyleChanged(_oldStyle, _newStyle);
		}
		foreach (XUiController child in children)
		{
			child.ForceInputStyleChange(_oldStyle, _newStyle);
		}
	}

	public string GetXuiHierarchy()
	{
		return XUiUtils.GetXuiHierarchy(this);
	}
}
