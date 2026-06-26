using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PopupMenu : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<MenuItemEntry> menuItems = new List<MenuItemEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i xuiPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i offset;

	public bool IsOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController grid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController sprBackgroundBorder;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController sprBackground;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid gridView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView originView;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool setVisiblePending;

	[PublicizedFrom(EAccessModifier.Private)]
	public float setVisibleTime;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int MenuWidth
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public void SetupItems(List<MenuItemEntry> newMenuItems, Vector2i offsetPosition, XUiView _originView)
	{
		menuItems.Clear();
		MenuWidth = 0;
		IsOver = false;
		menuItems = newMenuItems;
		xuiPosition = base.xui.GetMouseXUIPosition();
		offset = offsetPosition;
		originView = _originView;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetPosition()
	{
		if (gridView != null)
		{
			Vector2i size = gridView.Size;
			Vector2i vector2i = base.xui.GetXUiScreenSize() / 2;
			Vector2i vector2i2 = new Vector2i((int)((double)vector2i.x * 0.97), (int)((double)vector2i.y * 0.97));
			Vector2i vector2i3 = xuiPosition + offset;
			Vector2i vector2i4 = vector2i3;
			Vector2i vector2i5 = vector2i4 + size;
			if (vector2i5.x >= vector2i2.x)
			{
				vector2i3.x = vector2i2.x - size.x;
			}
			else if (vector2i4.x <= -vector2i2.x)
			{
				vector2i3.x = -vector2i2.x;
			}
			if (vector2i5.y >= vector2i2.y)
			{
				vector2i3.y = vector2i2.y - size.y;
			}
			else if (vector2i4.y <= -vector2i2.y)
			{
				vector2i3.y = -vector2i2.y;
			}
			base.ViewComponent.Position = vector2i3;
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetWidth(int newWidth)
	{
		if (newWidth > MenuWidth)
		{
			MenuWidth = newWidth;
		}
	}

	public void ClearItems()
	{
		menuItems.Clear();
		IsDirty = true;
	}

	public override void Init()
	{
		base.Init();
		base.xui.currentPopupMenu = this;
		grid = GetChildById("list");
		sprBackgroundBorder = GetChildById("sprBackgroundBorder");
		sprBackground = GetChildById("sprBackground");
		base.ViewComponent.IsVisible = false;
		gridView = grid.ViewComponent as XUiV_Grid;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (setVisiblePending && Time.time - setVisibleTime > 0.1f)
		{
			setVisiblePending = false;
			base.ViewComponent.IsVisible = true;
			base.xui.playerUI.CursorController.SetNavigationLockView(viewComponent, children[0].ViewComponent);
		}
		if (menuItems.Count > 0 && !IsDirty && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && (base.xui.playerUI.CursorController.GetMouseButtonUp(UICamera.MouseButton.LeftButton) | base.xui.playerUI.playerInput.GUIActions.RightStick.WasReleased) && !IsOver)
		{
			ClearItems();
		}
		if (!IsDirty)
		{
			return;
		}
		if (menuItems.Count == 0)
		{
			IsDirty = false;
			base.ViewComponent.IsVisible = false;
			if (base.xui.playerUI.CursorController.navigationTarget != null && base.xui.playerUI.CursorController.navigationTarget.Controller.IsChildOf(this))
			{
				base.xui.playerUI.CursorController.SetNavigationLockView(null);
				base.xui.playerUI.CursorController.SetNavigationTarget(originView);
			}
			return;
		}
		XUiC_PopupMenuItem[] childrenByType = grid.GetChildrenByType<XUiC_PopupMenuItem>();
		for (int i = 0; i < childrenByType.Length; i++)
		{
			XUiC_PopupMenuItem xUiC_PopupMenuItem = childrenByType[i];
			if (i < menuItems.Count)
			{
				xUiC_PopupMenuItem.ItemEntry = menuItems[i];
				xUiC_PopupMenuItem.ViewComponent.IsVisible = true;
			}
			else
			{
				xUiC_PopupMenuItem.ItemEntry = null;
				xUiC_PopupMenuItem.ViewComponent.IsVisible = false;
			}
		}
		for (int j = 0; j < childrenByType.Length; j++)
		{
			childrenByType[j].SetWidth(MenuWidth + 60);
		}
		XUiView xUiView = sprBackground.ViewComponent;
		Vector2i size = (gridView.Size = new Vector2i(MenuWidth + 60, menuItems.Count * 43));
		xUiView.Size = size;
		XUiView xUiView2 = sprBackgroundBorder.ViewComponent;
		size = (base.ViewComponent.Size = new Vector2i(MenuWidth + 60 + 6, menuItems.Count * 43 + 6));
		xUiView2.Size = size;
		SetPosition();
		IsDirty = false;
		base.ViewComponent.IsVisible = true;
		base.xui.playerUI.CursorController.SetNavigationLockView(viewComponent, childrenByType[0].ViewComponent);
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
