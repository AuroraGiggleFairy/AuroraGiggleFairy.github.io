using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PopupMenuItem : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label label;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 disabledFontColor = Color.gray;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 defaultFontColor = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public BoxCollider collider;

	[PublicizedFrom(EAccessModifier.Private)]
	public MenuItemEntry itemEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor statuscolorFormatter = new CachedStringFormatterXuiRgbaColor();

	public MenuItemEntry ItemEntry
	{
		get
		{
			return itemEntry;
		}
		set
		{
			itemEntry = value;
			RefreshBindings();
			label.Label.text = label.Text;
			base.xui.currentPopupMenu.SetWidth((int)label.Label.printedSize.x);
			label.Size = new Vector2i((int)label.Label.printedSize.x, label.Label.height);
			label.Position = new Vector2i(50, -8);
			background.SpriteName = "menu_empty";
			background.Color = new Color32(64, 64, 64, byte.MaxValue);
		}
	}

	public override void Init()
	{
		base.Init();
		base.OnPress += onPressed;
		base.OnHover += OnHovered;
		label = (XUiV_Label)GetChildById("lblText").ViewComponent;
		label.Overflow = UILabel.Overflow.ResizeFreely;
		background = (XUiV_Sprite)GetChildById("background").ViewComponent;
		collider = viewComponent.UiTransform.GetComponent<BoxCollider>();
		viewComponent.UseSelectionBox = false;
		background.UseSelectionBox = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnHovered(XUiController _sender, bool _isOver)
	{
		if (background != null)
		{
			if (_isOver)
			{
				background.SpriteName = "ui_game_select_row";
				background.Color = Color.white;
			}
			else
			{
				background.SpriteName = "menu_empty";
				background.Color = new Color32(64, 64, 64, byte.MaxValue);
			}
		}
		base.xui.currentPopupMenu.IsOver = _isOver;
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "menuicon":
			value = ((ItemEntry != null) ? ItemEntry.IconName : "");
			return true;
		case "menutext":
			value = ((ItemEntry != null) ? ItemEntry.Text : "");
			return true;
		case "statuscolor":
			value = "255,255,255,255";
			if (ItemEntry != null)
			{
				Color32 v = (ItemEntry.IsEnabled ? defaultFontColor : disabledFontColor);
				value = statuscolorFormatter.Format(v);
			}
			return true;
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onPressed(XUiController _sender, int _mouseButton)
	{
		if (ItemEntry != null)
		{
			if (ItemEntry != null && ItemEntry.IsEnabled)
			{
				ItemEntry.HandleItemClicked();
			}
			base.xui.currentPopupMenu.ClearItems();
		}
	}

	public void SetWidth(int width)
	{
		if (background != null)
		{
			background.Size = new Vector2i(width, background.Size.y);
			if (collider != null)
			{
				collider.size = new Vector3(width, collider.size.y, collider.size.z);
				collider.center = new Vector3(width / 2, collider.center.y, collider.center.z);
			}
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			if (!(name == "default_font_color"))
			{
				if (!(name == "disabled_font_color"))
				{
					return false;
				}
				disabledFontColor = StringParsers.ParseColor32(value);
			}
			else
			{
				defaultFontColor = StringParsers.ParseColor32(value);
			}
			return true;
		}
		return flag;
	}
}
