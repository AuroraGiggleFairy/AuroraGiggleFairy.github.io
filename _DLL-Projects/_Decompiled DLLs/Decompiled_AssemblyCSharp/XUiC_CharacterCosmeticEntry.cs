using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CharacterCosmeticEntry : XUiController
{
	public enum EntryTypes
	{
		Item,
		Empty,
		Hide
	}

	public XUiC_CharacterCosmeticList Owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass itemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntryTypes entryType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int index = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool selected;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isHovered;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite itemIconSprite;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Texture backgroundTexture;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 selectionBorderColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 pressColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 lockedBackgroundColor = new Color32(48, 48, 48, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 highlightColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 holdingColor = new Color32(byte.MaxValue, 128, 0, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDragAndDrop;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor backgroundcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor selectionbordercolorFormatter = new CachedStringFormatterXuiRgbaColor();

	public ItemClass ItemClass
	{
		get
		{
			return itemClass;
		}
		set
		{
			itemClass = value;
			IsDirty = true;
		}
	}

	public EntryTypes EntryType
	{
		get
		{
			return entryType;
		}
		set
		{
			entryType = value;
			IsDirty = true;
		}
	}

	public int Index
	{
		get
		{
			return index;
		}
		set
		{
			index = value;
			IsDirty = true;
		}
	}

	public new bool Selected
	{
		get
		{
			return selected;
		}
		set
		{
			selected = value;
		}
	}

	public Color32 SelectionBorderColor
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return selectionBorderColor;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (!selectionBorderColor.ColorEquals(value))
			{
				selectionBorderColor = value;
				IsDirty = true;
			}
		}
	}

	public bool IsDragAndDrop
	{
		get
		{
			return isDragAndDrop;
		}
		set
		{
			isDragAndDrop = value;
			if (value)
			{
				base.ViewComponent.EventOnPress = false;
				base.ViewComponent.EventOnHover = false;
			}
		}
	}

	public string Name
	{
		get
		{
			switch (EntryType)
			{
			case EntryTypes.Item:
				if (itemClass != null)
				{
					return itemClass.GetLocalizedItemName();
				}
				break;
			case EntryTypes.Empty:
				return "Empty";
			case EntryTypes.Hide:
				return "Hide";
			}
			return "COSMETICS";
		}
	}

	public override void Init()
	{
		base.Init();
		IsDirty = true;
		XUiController childById = GetChildById("itemIcon");
		if (childById != null)
		{
			itemIconSprite = childById.ViewComponent as XUiV_Sprite;
		}
		XUiController childById2 = GetChildById("backgroundTexture");
		if (childById2 != null)
		{
			backgroundTexture = childById2.ViewComponent as XUiV_Texture;
			if (backgroundTexture != null)
			{
				backgroundTexture.CreateMaterial();
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.PlayerEquipment.Equipment.CosmeticUnlocked += Equipment_CosmeticUnlocked;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.PlayerEquipment.Equipment.CosmeticUnlocked -= Equipment_CosmeticUnlocked;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Equipment_CosmeticUnlocked(string itemName)
	{
		if (entryType == EntryTypes.Item && ItemClass != null && ItemClass.GetItemName() == itemName)
		{
			if (Owner.SelectedEntry == this)
			{
				Owner.Owner.RefreshBindings();
			}
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBorderColor()
	{
		if (IsDragAndDrop)
		{
			SelectionBorderColor = Color.clear;
		}
		else if (Selected)
		{
			SelectionBorderColor = selectColor;
		}
		else if (isHovered)
		{
			SelectionBorderColor = highlightColor;
		}
		else
		{
			SelectionBorderColor = backgroundColor;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		base.OnHovered(_isOver);
		if (EntryType == EntryTypes.Item && itemClass == null)
		{
			isHovered = false;
		}
		else if (isHovered != _isOver)
		{
			isHovered = _isOver;
			RefreshBindings();
		}
	}

	public override void Update(float _dt)
	{
		if (IsDirty)
		{
			base.ViewComponent.IsNavigatable = (base.ViewComponent.IsSnappable = entryType != EntryTypes.Item || (entryType == EntryTypes.Item && itemClass != null));
			RefreshBindings(_forceAll: true);
			IsDirty = false;
		}
		updateBorderColor();
		base.Update(_dt);
	}

	public (bool isUnlocked, EntitlementSetEnum set) IsUnlocked()
	{
		if (entryType != EntryTypes.Item)
		{
			return (isUnlocked: true, set: EntitlementSetEnum.None);
		}
		if (itemClass == null)
		{
			return (isUnlocked: false, set: EntitlementSetEnum.None);
		}
		return base.xui.PlayerEquipment.Equipment.HasCosmeticUnlocked(itemClass);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		var (flag, entitlementSetEnum) = IsUnlocked();
		switch (bindingName)
		{
		case "itemicon":
			value = "";
			switch (EntryType)
			{
			case EntryTypes.Item:
				if (itemClass != null)
				{
					value = itemClass.GetIconName();
				}
				break;
			case EntryTypes.Empty:
				value = "server_refresh";
				break;
			case EntryTypes.Hide:
				value = "ui_game_symbol_x";
				break;
			}
			return true;
		case "iconatlas":
			value = "ItemIconAtlas";
			if (EntryType != EntryTypes.Item)
			{
				value = "UIAtlas";
			}
			else
			{
				if (itemClass == null)
				{
					return true;
				}
				if (!flag)
				{
					value = "ItemIconAtlasGreyscale";
				}
			}
			return true;
		case "islocked":
			value = "false";
			if (!flag && entitlementSetEnum != EntitlementSetEnum.None)
			{
				value = "true";
			}
			return true;
		case "selectionbordercolor":
			value = selectionbordercolorFormatter.Format(SelectionBorderColor);
			return true;
		case "backgroundcolor":
			if (!flag && itemClass != null)
			{
				value = backgroundcolorFormatter.Format(lockedBackgroundColor);
			}
			else
			{
				value = backgroundcolorFormatter.Format(backgroundColor);
			}
			return true;
		case "tooltip":
			value = "";
			switch (EntryType)
			{
			case EntryTypes.Item:
				if (itemClass != null)
				{
					value = itemClass.GetLocalizedItemName();
				}
				break;
			case EntryTypes.Empty:
				value = "Empty";
				break;
			case EntryTypes.Hide:
				value = "Hide";
				break;
			}
			return true;
		default:
			return false;
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "select_color":
			selectColor = StringParsers.ParseColor32(_value);
			return true;
		case "press_color":
			pressColor = StringParsers.ParseColor32(_value);
			return true;
		case "background_color":
			backgroundColor = StringParsers.ParseColor32(_value);
			return true;
		case "locked_background_color":
			backgroundColor = StringParsers.ParseColor32(_value);
			return true;
		case "highlight_color":
			highlightColor = StringParsers.ParseColor32(_value);
			return true;
		case "holding_color":
			holdingColor = StringParsers.ParseColor32(_value);
			return true;
		default:
			return base.ParseAttribute(_name, _value, _parent);
		}
	}
}
