
using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

namespace StormTracker
{
	[Preserve]
	public class XUiController
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public XUiView viewComponent;

		[PublicizedFrom(EAccessModifier.Protected)]
		public XUiController parent;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly List<XUiController> children = new List<XUiController>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public XUiWindowGroup windowGroup;

		[PublicizedFrom(EAccessModifier.Private)]
		public PlayerInputManager.InputStyle lastInputStyle = PlayerInputManager.InputStyle.Count;

		[PublicizedFrom(EAccessModifier.Private)]
		public PlayerInputManager.InputStyle curInputStyle;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool registeredForInputStyleChanges;

		public bool IsDirty;

		public bool IsDormant;

		public object CustomData;

		public readonly Dictionary<string, string> CustomAttributes = new Dictionary<string, string>();

		public readonly List<BindingInfo> BindingList = new List<BindingInfo>();

	public PlayerInputManager.InputStyle CurrentInputStyle
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return lastInputStyle;
		}
	}

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
			parent = value;
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

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUi xui { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsOpen
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public event XUiEvent_OnPressEventHandler OnPress;

	public event XUiEvent_OnPressEventHandler OnDoubleClick;

	public event XUiEvent_OnPressEventHandler OnRightPress;

	public event XUiEvent_OnHoverEventHandler OnHover;

	public event XUiEvent_OnDragEventHandler OnDrag;

	public event XUiEvent_OnHeldHandler OnHold;

	public event XUiEvent_OnScrollEventHandler OnScroll;

	public event XUiEvent_OnSelectEventHandler OnSelect;

	public event XUiEvent_OnVisibilityChanged OnVisiblity;

	public XUiController()
	{
		parent = null;
	}

	public XUiController(XUiController _parent)
	{
		parent = _parent;
		parent?.AddChild(this);
	}

	public virtual void Init()
	{
		if (viewComponent != null)
		{
			viewComponent.InitView();
		}
		for (int i = 0; i < children.Count; i++)
		{
			children[i].Init();
		}
		curInputStyle = PlatformManager.NativePlatform.Input.CurrentInputStyle;
	}

	public virtual void UpdateInput()
	{
		for (int i = 0; i < children.Count; i++)
		{
			if (!children[i].IsDormant)
			{
				children[i].UpdateInput();
			}
		}
	}

	public virtual void Update(float _dt)
	{
		if (viewComponent != null && windowGroup != null && windowGroup.isShowing && viewComponent.IsVisible)
		{
			viewComponent.Update(_dt);
		}
		if (curInputStyle != lastInputStyle)
		{
			PlayerInputManager.InputStyle oldStyle = lastInputStyle;
			lastInputStyle = curInputStyle;
			RefreshBindings();
			InputStyleChanged(oldStyle, lastInputStyle);
		}
		for (int i = 0; i < children.Count; i++)
		{
			XUiController xUiController = children[i];
			if (!xUiController.IsDormant)
			{
				xUiController.Update(_dt);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
	}

	public void ForceInputStyleChange(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		if (registeredForInputStyleChanges)
		{
			InputStyleChanged(_oldStyle, _newStyle);
		}
		foreach (XUiController child in children)
		{
			_ = child;
			ForceInputStyleChange(_oldStyle, _newStyle);
		}
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
		XUiController xUiController = null;
		if (viewComponent != null && string.Equals(viewComponent.ID, _id, StringComparison.OrdinalIgnoreCase))
		{
			xUiController = this;
		}
		else
		{
			for (int i = 0; i < children.Count; i++)
			{
				xUiController = children[i].GetChildById(_id);
				if (xUiController != null)
				{
					break;
				}
			}
		}
		return xUiController;
	}

	public XUiController[] GetChildrenById(string _id, List<XUiController> _list = null)
	{
		List<XUiController> list = ((_list != null) ? _list : new List<XUiController>());
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
		if (_list == null)
		{
			return list.ToArray();
		}
		return null;
	}

	public T GetChildByType<T>() where T : XUiController
	{
		T val = this as T;
		if (val == null)
		{
			foreach (XUiController child in children)
			{
				val = child.GetChildByType<T>();
				if (val != null)
				{
					break;
				}
			}
		}
		return val;
	}

	public T[] GetChildrenByType<T>(List<T> _list = null) where T : XUiController
	{
		List<T> list = ((_list != null) ? _list : new List<T>());
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
		if (_list == null)
		{
			return list.ToArray();
		}
		return null;
	}

	public T[] GetChildrenByViewType<T>(List<T> _list = null) where T : XUiView
	{
		List<T> list = ((_list != null) ? _list : new List<T>());
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
		if (_list == null)
		{
			return list.ToArray();
		}
		return null;
	}

	public T GetParentByType<T>() where T : XUiController
	{
		if (this is T)
		{
			return this as T;
		}
		if (Parent != null)
		{
			return Parent.GetParentByType<T>();
		}
		return null;
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

	public XUiV_Window GetParentWindow()
	{
		if (ViewComponent is XUiV_Window result)
		{
			return result;
		}
		return Parent?.GetParentWindow();
	}

	public void AddChild(XUiController _child)
	{
		children.Add(_child);
	}

	public void Pressed(int _mouseButton)
	{
		OnPressed(_mouseButton);
	}

	public void DoubleClicked(int _mouseButton)
	{
		OnDoubleClicked(_mouseButton);
	}

	public void Hovered(bool _isOver)
	{
		OnHovered(_isOver);
	}

	public void Scrolled(float _delta)
	{
		OnScrolled(_delta);
	}

	public void Selected(bool _selected)
	{
		OnSelected(_selected);
	}

	public void Dragged(Vector2 _mouseDelta, EDragType _dragType)
	{
		OnDragged(_dragType, _mouseDelta);
	}

	public void Held(EHoldType _event, float _holdDuration, float _deltaSinceLastTimedEvent = -1f)
	{
		OnHeld(_event, _holdDuration, _deltaSinceLastTimedEvent);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnPressed(int _mouseButton)
	{
		switch (_mouseButton)
		{
		case -1:
			this.OnPress?.Invoke(this, _mouseButton);
			break;
		case -2:
			this.OnRightPress?.Invoke(this, _mouseButton);
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDoubleClicked(int _mouseButton)
	{
		this.OnDoubleClick?.Invoke(this, _mouseButton);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnHovered(bool _isOver)
	{
		this.OnHover?.Invoke(this, _isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDragged(EDragType _dragType, Vector2 _mousePositionDelta)
	{
		this.OnDrag?.Invoke(this, _dragType, _mousePositionDelta);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnHeld(EHoldType _event, float _holdDuration, float _deltaSinceLastTimedEvent)
	{
		this.OnHold?.Invoke(this, _event, _holdDuration, _deltaSinceLastTimedEvent);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnScrolled(float _delta)
	{
		this.OnScroll?.Invoke(this, _delta);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnSelected(bool _selected)
	{
		this.OnSelect?.Invoke(this, _selected);
	}

	public virtual void OnOpen()
	{
		IsOpen = true;
		for (int i = 0; i < children.Count; i++)
		{
			children[i].OnOpen();
		}
		if (ViewComponent != null)
		{
			if (ViewComponent.ForceHide)
			{
				ViewComponent.IsVisible = false;
			}
			else if (!ViewComponent.IsVisible)
			{
				ViewComponent.OnOpen();
				ViewComponent.IsVisible = true;
			}
		}
	}

	public virtual void OnClose()
	{
		IsOpen = false;
		for (int i = 0; i < children.Count; i++)
		{
			children[i].OnClose();
		}
		if (ViewComponent != null && ViewComponent.IsVisible)
		{
			ViewComponent.OnClose();
			ViewComponent.IsVisible = false;
		}
	}

	public virtual void OnVisibilityChanged(bool _isVisible)
	{
		this.OnVisiblity?.Invoke(this, _isVisible);
	}

	public virtual bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
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
			Log.Error("Unhandled exception in GetBindingValue. Binding name: " + _bindingName);
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
		case "is_creative":
			_value = (GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled)).ToString();
			return true;
		case "gamelanguage":
			_value = Localization.language;
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
			RegisterForInputStyleChanges();
			_value = lastInputStyle.ToStringCached();
			return true;
		case "is_controller_input":
			RegisterForInputStyleChanges();
			_value = (lastInputStyle != PlayerInputManager.InputStyle.Keyboard).ToString();
			return true;
		case "is_console":
			_value = (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent().ToString();
			return true;
		default:
			return false;
		}
	}

	public virtual void SetAllChildrenDirty(bool _includeViewComponents = false)
	{
		for (int i = 0; i < children.Count; i++)
		{
			children[i].SetAllChildrenDirty();
		}
		if (viewComponent != null)
		{
			viewComponent.IsDirty = true;
		}
		IsDirty = true;
	}

	public virtual void RefreshBindingsSelfAndChildren()
	{
		for (int i = 0; i < children.Count; i++)
		{
			children[i].RefreshBindingsSelfAndChildren();
		}
		RefreshBindings(_forceAll: true);
	}

	public void RefreshBindings(bool _forceAll = false)
	{
		for (int i = 0; i < BindingList.Count; i++)
		{
			BindingList[i].RefreshValue(_forceAll);
		}
	}

	public void AddBinding(BindingInfo _info)
	{
		if (!BindingList.Contains(_info))
		{
			BindingList.Add(_info);
		}
	}

	public virtual bool AlwaysUpdate()
	{
		return false;
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

	public void FindNavigatableChildren(List<XUiView> views)
	{
		foreach (XUiController child in children)
		{
			if (child.viewComponent.IsNavigatable)
			{
				views.Add(child.viewComponent);
			}
			child.FindNavigatableChildren(views);
		}
	}

	public bool TryFindFirstNavigableChild(out XUiView foundView)
	{
		foundView = null;
		foreach (XUiController child in children)
		{
			if (child.ViewComponent.IsNavigatable && child.viewComponent.IsVisible && child.viewComponent.UiTransform.gameObject.activeInHierarchy)
			{
				foundView = child.viewComponent;
				return true;
			}
			if (child.TryFindFirstNavigableChild(out foundView))
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
		if (xui.playerUI.CursorController.CursorModeActive && !_overrideCursorMode)
		{
			return false;
		}
		XUiView foundView = ViewComponent;
		if (foundView.IsNavigatable && foundView.IsVisible)
		{
			if (_withDelay)
			{
				xui.playerUI.CursorController.SetNavigationTargetLater(foundView);
			}
			else
			{
				xui.playerUI.CursorController.SetNavigationTarget(foundView);
			}
			return true;
		}
		TryFindFirstNavigableChild(out foundView);
		if (foundView != null)
		{
			if (_withDelay)
			{
				xui.playerUI.CursorController.SetNavigationTargetLater(foundView);
			}
			else
			{
				xui.playerUI.CursorController.SetNavigationTarget(foundView);
			}
			return true;
		}
		return false;
	}

	public virtual void OnCursorSelected()
	{
		if (parent != null)
		{
			parent.OnCursorSelected();
		}
	}

	public virtual void OnCursorUnSelected()
	{
		if (parent != null)
		{
			parent.OnCursorUnSelected();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void RegisterForInputStyleChanges()
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
	}
}
