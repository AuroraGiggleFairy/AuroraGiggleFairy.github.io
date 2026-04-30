using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PopupMenu : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_PopupMenuItem.Entry> menuItems = new List<XUiC_PopupMenuItem.Entry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i xuiPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i offset;

	public bool IsOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid gridView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PopupMenuItem[] popupMenuItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView originView;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxWidthPadding = 60;

	public int SliderMinWidth = 250;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i gridSize = Vector2i.one;

	public override void Init()
	{
		base.Init();
		base.xui.currentPopupMenu = this;
		gridView = (XUiV_Grid)GetChildById("list").ViewComponent;
		popupMenuItems = gridView.Controller.GetChildrenByType<XUiC_PopupMenuItem>();
		base.ViewComponent.IsVisible = false;
	}

	public void Setup(Vector2i _offsetPosition, XUiView _originView)
	{
		ClearItems();
		IsOver = false;
		xuiPosition = base.xui.GetMouseXUIPosition();
		offset = _offsetPosition;
		originView = _originView;
		IsDirty = true;
	}

	public void AddItem(XUiC_PopupMenuItem.Entry _newMenuItems)
	{
		menuItems.Add(_newMenuItems);
		IsDirty = true;
	}

	public void ClearItems()
	{
		menuItems.Clear();
		IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (menuItems.Count > 0 && !IsDirty)
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
			{
				if ((base.xui.playerUI.CursorController.GetMouseButtonUp(UICamera.MouseButton.LeftButton) | base.xui.playerUI.playerInput.GUIActions.RightStick.WasReleased) && !IsOver)
				{
					ClearItems();
				}
			}
			else if (base.xui.playerUI.playerInput.GUIActions.Cancel.WasReleased)
			{
				ClearItems();
			}
		}
		if (!IsDirty)
		{
			return;
		}
		IsDirty = false;
		if (menuItems.Count == 0)
		{
			base.ViewComponent.IsVisible = false;
			if (base.xui.playerUI.CursorController.navigationTarget != null && base.xui.playerUI.CursorController.navigationTarget.Controller.IsChildOf(this))
			{
				base.xui.playerUI.CursorController.SetNavigationLockView(null);
				base.xui.playerUI.CursorController.SetNavigationTarget(originView);
				originView = null;
			}
			return;
		}
		int num = 0;
		int num2 = menuItems.Count - 1;
		for (int i = 0; i < popupMenuItems.Length; i++)
		{
			int num3 = popupMenuItems[i].SetEntry((i < menuItems.Count) ? menuItems[i] : null);
			if (num3 > num)
			{
				num = num3;
			}
			if (i < num2 && i < menuItems.Count && menuItems[i].IsEnabled)
			{
				num2 = i;
			}
		}
		num += maxWidthPadding;
		gridSize = new Vector2i(num, menuItems.Count * gridView.CellHeight);
		limitPositionToScreenBounds();
		base.ViewComponent.IsVisible = true;
		base.xui.playerUI.CursorController.SetNavigationLockView(viewComponent, popupMenuItems[num2].ViewComponent);
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void limitPositionToScreenBounds()
	{
		Vector2i vector2i = gridSize;
		vector2i.y = -vector2i.y;
		Vector2i vector2i2 = base.xui.GetXUiScreenSize() / 2;
		Vector2i vector2i3 = new Vector2i((int)((double)vector2i2.x * 0.97), (int)((double)vector2i2.y * 0.97));
		Vector2i vector2i4 = xuiPosition + offset;
		Vector2i vector2i5 = vector2i4;
		Vector2i vector2i6 = vector2i5 + vector2i;
		if (vector2i6.x >= vector2i3.x)
		{
			vector2i4.x = vector2i3.x - vector2i.x;
		}
		else if (vector2i5.x <= -vector2i3.x)
		{
			vector2i4.x = -vector2i3.x;
		}
		if (vector2i6.y <= -vector2i3.y)
		{
			vector2i4.y = -vector2i3.y - vector2i.y;
		}
		else if (vector2i5.y >= vector2i3.y)
		{
			vector2i4.y = vector2i3.y;
		}
		base.ViewComponent.Position = vector2i4;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (!(_bindingName == "grid_width"))
		{
			if (_bindingName == "grid_height")
			{
				_value = gridSize.y.ToString();
				return true;
			}
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = gridSize.x.ToString();
		return true;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (!(_name == "element_padding_over_label"))
		{
			if (_name == "slider_min_width")
			{
				SliderMinWidth = StringParsers.ParseSInt32(_value);
				return true;
			}
			return base.ParseAttribute(_name, _value, _parent);
		}
		maxWidthPadding = StringParsers.ParseSInt32(_value);
		return true;
	}

	public override void OnVisibilityChanged(bool _isVisible)
	{
		base.OnVisibilityChanged(_isVisible);
		if (_isVisible)
		{
			base.ViewComponent.TryUpdatePosition();
		}
	}
}
