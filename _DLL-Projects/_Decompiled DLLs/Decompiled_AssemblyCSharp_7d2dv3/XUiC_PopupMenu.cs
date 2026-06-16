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

	[XuiBindComponent("list", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Grid gridView;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_PopupMenuItem[] popupMenuItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public string title;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i gridSize = Vector2i.one;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool justOpened;

	[XuiXmlAttribute("menu_min_width", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int MenuMinWidth
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = 100;

	[XuiXmlAttribute("element_padding_over_label", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int MaxWidthPadding
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = 60;

	[XuiXmlAttribute("slider_min_width", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int SliderMinWidth { get; set; } = 250;

	[XuiXmlBinding("grid_width")]
	public int GridSizeX
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return gridSize.x;
		}
	}

	[XuiXmlBinding("grid_height")]
	public int GridSizeY
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return gridSize.y;
		}
	}

	[XuiXmlBinding("title")]
	public string Title
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return title ?? "";
		}
	}

	public override void Init()
	{
		base.Init();
		xui.PopupMenuWindow = this;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		xui.PopupMenuWindow = null;
	}

	public XUiC_PopupMenu Setup(Vector2i _offsetPosition, string _title = "")
	{
		clearItems();
		IsOver = false;
		xuiPosition = xui.GetMouseXUiPosition();
		offset = _offsetPosition;
		title = _title;
		IsDirty = true;
		return this;
	}

	public XUiC_PopupMenu AddItem(XUiC_PopupMenuItem.Entry _newMenuItems)
	{
		menuItems.Add(_newMenuItems);
		IsDirty = true;
		return this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearItems()
	{
		menuItems.Clear();
		XUiC_PopupMenuItem[] array = popupMenuItems;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetEntry(null);
		}
		IsDirty = true;
	}

	public XUiC_PopupMenu Show()
	{
		xui.playerUI.windowManager.Open(windowGroup, _bModal: false);
		justOpened = true;
		return this;
	}

	public void Close()
	{
		xui.playerUI.windowManager.Close(windowGroup);
		clearItems();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (menuItems.Count > 0 && !justOpened)
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
			{
				if ((xui.playerUI.CursorController.GetMouseButtonUp(UICamera.MouseButton.LeftButton) | xui.playerUI.playerInput.GUIActions.RightStick.WasReleased) && !IsOver)
				{
					Close();
				}
			}
			else if (xui.playerUI.playerInput.GUIActions.Cancel.WasReleased)
			{
				Close();
			}
		}
		if (justOpened)
		{
			justOpened = false;
			if (menuItems.Count == 0)
			{
				Close();
				return;
			}
			int num = MenuMinWidth - MaxWidthPadding;
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
			num += MaxWidthPadding;
			gridSize = new Vector2i(num, menuItems.Count * gridView.CellHeight);
			limitPositionToScreenBounds();
			xui.playerUI.CursorController.SetNavigationTargetLater(popupMenuItems[num2].ViewComponent);
			IsDirty = true;
		}
		if (IsDirty)
		{
			IsDirty = false;
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void limitPositionToScreenBounds()
	{
		Vector2i vector2i = gridSize;
		vector2i.y = -vector2i.y;
		Vector2i vector2i2 = xui.GetXUiScreenSize() / 2;
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
		base.ViewComponent.TryUpdatePosition();
	}
}
