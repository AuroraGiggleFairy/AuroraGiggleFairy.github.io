using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TraderItemEntry : XUiC_SelectableEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack item;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass itemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasIngredients;

	public int SlotIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, int> itemNameFormatter = new CachedStringFormatter<string, int>([PublicizedFrom(EAccessModifier.Internal)] (string _s, int _i) => $"{_s} ({_i})");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor durabilityColorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt durabilityValueFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat durabilityFillFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat removedDurabilityFillFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor stateColorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt itemPriceFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor altitemtypeiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TraderWindow TraderWindow { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemInfoWindow InfoWindow { get; set; }

	public ItemStack Item
	{
		get
		{
			return item;
		}
		set
		{
			item = value;
			IsDirty = true;
			itemClass = ((item == null) ? null : item.itemValue.ItemClass);
			XUiView xUiView = base.ViewComponent;
			bool enabled = (base.ViewComponent.IsNavigatable = item != null);
			xUiView.Enabled = enabled;
			RefreshBindings();
			if (item == null)
			{
				background.Color = new Color32(64, 64, 64, byte.MaxValue);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SelectedChanged(bool isSelected)
	{
		if (background != null)
		{
			background.Color = (isSelected ? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue) : new Color32(64, 64, 64, byte.MaxValue));
			background.SpriteName = (isSelected ? "ui_game_select_row" : "menu_empty");
		}
	}

	public override void Init()
	{
		base.Init();
		for (int i = 0; i < children.Count; i++)
		{
			XUiView xUiView = children[i].ViewComponent;
			if (xUiView.ID.EqualsCaseInsensitive("background"))
			{
				background = xUiView as XUiV_Sprite;
			}
		}
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		isHovered = _isOver;
		if (background != null && item != null && !base.IsSelected)
		{
			if (_isOver)
			{
				background.Color = new Color32(96, 96, 96, byte.MaxValue);
			}
			else
			{
				background.Color = new Color32(64, 64, 64, byte.MaxValue);
			}
		}
		base.OnHovered(_isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "hasitem":
			value = (item != null).ToString();
			return true;
		case "itemname":
			value = "";
			if (item != null)
			{
				if (item.count == 1)
				{
					value = itemClass.GetLocalizedItemName();
				}
				else
				{
					value = itemNameFormatter.Format(itemClass.GetLocalizedItemName(), item.count);
				}
			}
			return true;
		case "itemicontint":
		{
			Color32 v = Color.white;
			if (item != null)
			{
				v = item.itemValue.ItemClass.GetIconTint(item.itemValue);
			}
			value = itemicontintcolorFormatter.Format(v);
			return true;
		}
		case "itemicon":
			if (item != null)
			{
				value = item.itemValue.GetPropertyOverride("CustomIcon", itemClass.GetIconName());
			}
			else
			{
				value = "";
			}
			return true;
		case "hasdurability":
			value = (item != null && !item.IsEmpty() && item.itemValue.ItemClass.ShowQualityBar).ToString();
			return true;
		case "durabilitycolor":
			if (item != null && !item.IsEmpty() && item.itemValue.ItemClass.ShowQualityBar)
			{
				Color32 v3 = QualityInfo.GetTierColor(item.itemValue.Quality);
				value = durabilityColorFormatter.Format(v3);
			}
			else
			{
				value = "0,0,0,0";
			}
			return true;
		case "durabilityvalue":
			value = "";
			if (item != null)
			{
				if (item.itemValue.HasQuality || itemClass.HasSubItems)
				{
					value = ((item.itemValue.Quality > 0) ? durabilityValueFormatter.Format(item.itemValue.Quality) : "-");
				}
				else
				{
					value = "-";
				}
			}
			return true;
		case "durabilityfill":
			value = ((item == null || item.IsEmpty()) ? "1" : ((item.itemValue.MaxUseTimes == 0) ? "1" : durabilityFillFormatter.Format(((float)item.itemValue.MaxUseTimes - item.itemValue.UseTimes) / (float)item.itemValue.MaxUseTimesUI)));
			return true;
		case "removeddurabilityfill":
			if (item?.itemValue == null)
			{
				value = "1";
				return true;
			}
			value = removedDurabilityFillFormatter.Format(item.itemValue.MaxDurabilityModifier);
			return true;
		case "statecolor":
			if (item != null)
			{
				Color32 v2 = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
				value = stateColorFormatter.Format(v2);
			}
			else
			{
				value = "255,255,255,255";
			}
			return true;
		case "itemprice":
			if (item != null)
			{
				int count = (itemClass.IsBlock() ? Block.list[item.itemValue.type].EconomicBundleSize : itemClass.EconomicBundleSize);
				value = itemPriceFormatter.Format(XUiM_Trader.GetBuyPrice(xui, item.itemValue, count, null, SlotIndex));
			}
			else
			{
				value = "";
			}
			return true;
		case "pricecolor":
			value = "255,255,255,255";
			if (item != null)
			{
				TraderData traderData = xui.Trader.TraderData;
				TraderInfo traderInfo = traderData.TraderInfo;
				if (xui.Trader.Trader is TileEntityVendingMachine && (traderInfo.PlayerOwned || traderInfo.Rentable))
				{
					int markup = traderData.PrimaryInventory[SlotIndex].Markup;
					if (markup > 0)
					{
						value = "255,0,0,255";
					}
					else if (markup < 0)
					{
						value = "0,255,0,255";
					}
				}
			}
			return true;
		case "currencyicon":
			if (item != null)
			{
				value = TraderInfo.CurrencyItem;
			}
			else
			{
				value = "";
			}
			return true;
		case "itemtypeicon":
			if (item == null)
			{
				value = "";
			}
			else if (item.itemValue.ItemClass.IsBlock())
			{
				value = Block.list[item.itemValue.type].ItemTypeIcon;
			}
			else
			{
				if (item.itemValue.ItemClass.AltItemTypeIcon != null && item.itemValue.ItemClass.Unlocks != "" && XUiM_ItemStack.CheckKnown(xui.playerUI.entityPlayer, item.itemValue.ItemClass, item.itemValue))
				{
					value = item.itemValue.ItemClass.AltItemTypeIcon;
					return true;
				}
				value = item.itemValue.ItemClass.ItemTypeIcon;
			}
			return true;
		case "hasitemtypeicon":
			if (item == null)
			{
				value = "false";
			}
			else if (item.itemValue.ItemClass.IsBlock())
			{
				value = (Block.list[item.itemValue.type].ItemTypeIcon != "").ToString();
			}
			else
			{
				value = (item.itemValue.ItemClass.ItemTypeIcon != "").ToString();
			}
			return true;
		case "itemtypeicontint":
			value = "255,255,255,255";
			if (item != null && item.itemValue.ItemClass.Unlocks != "" && XUiM_ItemStack.CheckKnown(xui.playerUI.entityPlayer, item.itemValue.ItemClass, item.itemValue))
			{
				value = altitemtypeiconcolorFormatter.Format(item.itemValue.ItemClass.AltItemTypeIconColor);
			}
			return true;
		case "hasBoostedIcon":
			value = item?.itemValue.HasAnyBoostedStats().ToString() ?? "false";
			return true;
		default:
			return base.GetBindingValueInternal(ref value, bindingName);
		}
	}

	public void Refresh()
	{
		if (base.IsSelected)
		{
			InfoWindow.SetItemStack(this);
		}
		IsDirty = true;
		RefreshBindings();
	}
}
