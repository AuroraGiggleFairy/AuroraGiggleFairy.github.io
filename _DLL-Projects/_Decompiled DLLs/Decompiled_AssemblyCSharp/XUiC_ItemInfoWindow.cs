using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemInfoWindow : XUiC_InfoWindow
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack itemStack = ItemStack.Empty.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass itemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemStack selectedItemStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_EquipmentStack selectedEquipmentStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_BasePartStack selectedPartStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TraderItemEntry selectedTraderItemStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestTurnInEntry selectedTurnInItemStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController itemPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemActionList mainActionItemList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemActionList traderActionItemList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PartList partList;

	public XUiC_Counter BuySellCounter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController statButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController descriptionButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_InfoWindow emptyInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBuying;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useCustomMarkup;

	public bool SetMaxCountOnDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemDisplayEntry itemDisplayEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SelectableEntry hoverEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack compareStack = ItemStack.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showStats = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt itemcostFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> markupFormatter = new CachedStringFormatter<int>([PublicizedFrom(EAccessModifier.Internal)] (int _i) => (_i <= 0) ? ((_i >= 0) ? "" : $" ({_i}%)") : $" (+{_i}%)");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor durabilitycolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat durabilityfillFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt durabilitytextFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor altitemtypeiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, string> itemGroupToIcon = new CaseInsensitiveStringDictionary<string>
	{
		{ "basics", "ui_game_symbol_campfire" },
		{ "building", "ui_game_symbol_map_house" },
		{ "resources", "ui_game_symbol_resource" },
		{ "ammo/weapons", "ui_game_symbol_knife" },
		{ "tools/traps", "ui_game_symbol_tool" },
		{ "food/cooking", "ui_game_symbol_fork" },
		{ "medicine", "ui_game_symbol_medical" },
		{ "clothing", "ui_game_symbol_shirt" },
		{ "decor/miscellaneous", "ui_game_symbol_chair" },
		{ "books", "ui_game_symbol_book" },
		{ "chemicals", "ui_game_symbol_water" },
		{ "mods", "ui_game_symbol_assemble" }
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string defaultItemGroupIcon = "ui_game_symbol_campfire";

	public bool isOpenAsTrader
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (base.xui.Trader != null)
			{
				return base.xui.Trader.Trader != null;
			}
			return false;
		}
	}

	public XUiC_SelectableEntry HoverEntry
	{
		get
		{
			return hoverEntry;
		}
		set
		{
			if (hoverEntry == value)
			{
				return;
			}
			hoverEntry = value;
			if (hoverEntry != null && !hoverEntry.Selected && !itemStack.IsEmpty())
			{
				ItemStack hoverControllerItemStack = GetHoverControllerItemStack();
				if (!hoverControllerItemStack.IsEmpty() && XUiM_ItemStack.CanCompare(hoverControllerItemStack.itemValue.ItemClass, itemClass))
				{
					CompareStack = hoverControllerItemStack;
				}
				else
				{
					CompareStack = ItemStack.Empty;
				}
			}
			else
			{
				CompareStack = ItemStack.Empty;
			}
		}
	}

	public ItemStack CompareStack
	{
		get
		{
			return compareStack;
		}
		set
		{
			if (compareStack != value)
			{
				compareStack = value;
				RefreshBindings();
			}
		}
	}

	public ItemStack EquippedStack
	{
		get
		{
			if (compareStack.IsEmpty() && itemClass is ItemClassArmor itemClassArmor)
			{
				return base.xui.PlayerEquipment.GetStackFromSlot(itemClassArmor.EquipSlot);
			}
			return compareStack;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack GetHoverControllerItemStack()
	{
		if (hoverEntry is XUiC_ItemStack xUiC_ItemStack)
		{
			return xUiC_ItemStack.ItemStack;
		}
		if (hoverEntry is XUiC_EquipmentStack xUiC_EquipmentStack)
		{
			return xUiC_EquipmentStack.ItemStack;
		}
		if (hoverEntry is XUiC_BasePartStack xUiC_BasePartStack)
		{
			return xUiC_BasePartStack.ItemStack;
		}
		if (hoverEntry is XUiC_TraderItemEntry xUiC_TraderItemEntry)
		{
			return xUiC_TraderItemEntry.Item;
		}
		if (hoverEntry is XUiC_QuestTurnInEntry xUiC_QuestTurnInEntry)
		{
			return xUiC_QuestTurnInEntry.Item;
		}
		return null;
	}

	public override void Init()
	{
		base.Init();
		itemPreview = GetChildById("itemPreview");
		mainActionItemList = (XUiC_ItemActionList)GetChildById("itemActions");
		traderActionItemList = (XUiC_ItemActionList)GetChildById("vendorItemActions");
		partList = (XUiC_PartList)GetChildById("parts");
		BuySellCounter = GetChildByType<XUiC_Counter>();
		if (BuySellCounter != null)
		{
			BuySellCounter.OnCountChanged += Counter_OnCountChanged;
			BuySellCounter.Count = 1;
		}
		statButton = GetChildById("statButton");
		statButton.OnPress += StatButton_OnPress;
		descriptionButton = GetChildById("descriptionButton");
		descriptionButton.OnPress += DescriptionButton_OnPress;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DescriptionButton_OnPress(XUiController _sender, int _mouseButton)
	{
		((XUiV_Button)statButton.ViewComponent).Selected = false;
		((XUiV_Button)descriptionButton.ViewComponent).Selected = true;
		showStats = false;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StatButton_OnPress(XUiController _sender, int _mouseButton)
	{
		((XUiV_Button)statButton.ViewComponent).Selected = true;
		((XUiV_Button)descriptionButton.ViewComponent).Selected = false;
		showStats = true;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Counter_OnCountChanged(XUiController _sender, OnCountChangedEventArgs _e)
	{
		RefreshBindings();
		traderActionItemList.RefreshActionList();
	}

	public override void Deselect()
	{
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty && base.ViewComponent.IsVisible)
		{
			if (emptyInfoWindow == null)
			{
				emptyInfoWindow = (XUiC_InfoWindow)base.xui.FindWindowGroupByName("backpack").GetChildById("emptyInfoPanel");
			}
			if (selectedItemStack != null)
			{
				SetItemStack(selectedItemStack);
			}
			else if (selectedEquipmentStack != null)
			{
				SetItemStack(selectedEquipmentStack);
			}
			else if (selectedPartStack != null)
			{
				SetItemStack(selectedPartStack);
			}
			else if (selectedTraderItemStack != null)
			{
				SetItemStack(selectedTraderItemStack);
			}
			else if (selectedTurnInItemStack != null)
			{
				SetItemStack(selectedTurnInItemStack);
			}
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "itemname":
			value = ((this.itemClass != null) ? this.itemClass.GetLocalizedItemName() : "");
			return true;
		case "itemammoname":
			value = "";
			if (this.itemClass != null)
			{
				if (this.itemClass.Actions[0] is ItemActionRanged itemActionRanged)
				{
					if (itemActionRanged.MagazineItemNames.Length > 1)
					{
						ItemClass itemClass = ItemClass.GetItemClass(itemActionRanged.MagazineItemNames[itemStack.itemValue.SelectedAmmoTypeIndex]);
						value = itemClass.GetLocalizedItemName();
					}
				}
				else if (this.itemClass.Actions[0] is ItemActionLauncher itemActionLauncher && itemActionLauncher.MagazineItemNames.Length > 1)
				{
					ItemClass itemClass2 = ItemClass.GetItemClass(itemActionLauncher.MagazineItemNames[itemStack.itemValue.SelectedAmmoTypeIndex]);
					value = itemClass2.GetLocalizedItemName();
				}
			}
			return true;
		case "itemicon":
			if (itemStack != null)
			{
				Block block = itemStack.itemValue?.ToBlockValue().Block;
				if (block != null && block.SelectAlternates)
				{
					value = block.GetAltBlockValue(itemStack.itemValue.Meta).Block.GetIconName();
				}
				else
				{
					value = itemStack.itemValue.GetPropertyOverride("CustomIcon", (this.itemClass != null) ? this.itemClass.GetIconName() : "");
				}
			}
			else
			{
				value = "";
			}
			return true;
		case "itemcost":
			value = "";
			if (this.itemClass != null)
			{
				bool flag = ((!this.itemClass.IsBlock()) ? this.itemClass.SellableToTrader : Block.list[itemStack.itemValue.type].SellableToTrader);
				if (!flag && !isBuying)
				{
					value = Localization.Get("xuiNoSellPrice");
					return true;
				}
				int count = itemStack.count;
				if (isOpenAsTrader)
				{
					count = BuySellCounter.Count;
				}
				if (isBuying)
				{
					if (useCustomMarkup)
					{
						value = itemcostFormatter.Format(XUiM_Trader.GetBuyPrice(base.xui, itemStack.itemValue, count, this.itemClass, selectedTraderItemStack.SlotIndex));
						return true;
					}
					value = itemcostFormatter.Format(XUiM_Trader.GetBuyPrice(base.xui, itemStack.itemValue, count, this.itemClass));
				}
				else
				{
					int sellPrice = XUiM_Trader.GetSellPrice(base.xui, itemStack.itemValue, count, this.itemClass);
					value = ((sellPrice > 0) ? itemcostFormatter.Format(sellPrice) : Localization.Get("xuiNoSellPrice"));
				}
			}
			return true;
		case "pricelabel":
			value = "";
			if (this.itemClass != null)
			{
				if (!((!this.itemClass.IsBlock()) ? this.itemClass.SellableToTrader : Block.list[itemStack.itemValue.type].SellableToTrader))
				{
					return true;
				}
				int count2 = itemStack.count;
				if (isOpenAsTrader)
				{
					count2 = BuySellCounter.Count;
				}
				if (isBuying)
				{
					value = ((XUiM_Trader.GetBuyPrice(base.xui, itemStack.itemValue, count2, this.itemClass) > 0) ? Localization.Get("xuiBuyPrice") : "");
				}
				else
				{
					value = ((XUiM_Trader.GetSellPrice(base.xui, itemStack.itemValue, count2, this.itemClass) > 0) ? Localization.Get("xuiSellPrice") : "");
				}
			}
			return true;
		case "markup":
			value = "";
			if (useCustomMarkup)
			{
				int v3 = base.xui.Trader.Trader.GetMarkupByIndex(selectedTraderItemStack.SlotIndex) * 20;
				value = markupFormatter.Format(v3);
			}
			return true;
		case "itemicontint":
		{
			Color32 v = Color.white;
			if (this.itemClass != null)
			{
				v = this.itemClass.GetIconTint(itemStack.itemValue);
			}
			value = itemicontintcolorFormatter.Format(v);
			return true;
		}
		case "itemdescription":
			value = "";
			if (this.itemClass != null)
			{
				if (this.itemClass.IsBlock())
				{
					string descriptionKey = Block.list[this.itemClass.Id].DescriptionKey;
					if (Localization.Exists(descriptionKey))
					{
						value = Localization.Get(descriptionKey);
					}
				}
				else
				{
					string descriptionKey2 = this.itemClass.DescriptionKey;
					if (Localization.Exists(descriptionKey2))
					{
						value = Localization.Get(descriptionKey2);
					}
					if (this.itemClass.Unlocks != "")
					{
						ItemClass itemClass3 = ItemClass.GetItemClass(this.itemClass.Unlocks);
						if (itemClass3 != null)
						{
							value = value + "\n\n" + Localization.Get(itemClass3.DescriptionKey);
						}
					}
				}
			}
			return true;
		case "itemgroupicon":
			value = "";
			if (this.itemClass != null && this.itemClass.Groups.Length != 0)
			{
				string key = this.itemClass.Groups[0];
				if (!itemGroupToIcon.TryGetValue(key, out value))
				{
					value = defaultItemGroupIcon;
				}
			}
			return true;
		case "hasdurability":
			value = (!itemStack.IsEmpty() && this.itemClass != null && this.itemClass.ShowQualityBar).ToString();
			return true;
		case "durabilitycolor":
		{
			Color32 v2 = Color.white;
			if (!itemStack.IsEmpty())
			{
				v2 = QualityInfo.GetTierColor(itemStack.itemValue.Quality);
			}
			value = durabilitycolorFormatter.Format(v2);
			return true;
		}
		case "durabilityfill":
			value = (itemStack.IsEmpty() ? "0" : ((itemStack.itemValue.MaxUseTimes == 0) ? "1" : durabilityfillFormatter.Format(((float)itemStack.itemValue.MaxUseTimes - itemStack.itemValue.UseTimes) / (float)itemStack.itemValue.MaxUseTimes)));
			return true;
		case "durabilityjustify":
			value = "center";
			if (!itemStack.IsEmpty() && this.itemClass != null && !this.itemClass.ShowQualityBar)
			{
				value = "right";
			}
			return true;
		case "durabilitytext":
			value = "";
			if (!itemStack.IsEmpty() && this.itemClass != null)
			{
				if (this.itemClass.ShowQualityBar)
				{
					value = ((itemStack.itemValue.Quality > 0) ? durabilitytextFormatter.Format(itemStack.itemValue.Quality) : "-");
				}
				else
				{
					value = ((this.itemClass.Stacknumber == 1) ? "" : durabilitytextFormatter.Format(itemStack.count));
				}
			}
			return true;
		case "itemtypeicon":
			value = "";
			if (!itemStack.IsEmpty() && this.itemClass != null)
			{
				if (this.itemClass.IsBlock())
				{
					value = Block.list[itemStack.itemValue.type].ItemTypeIcon;
				}
				else
				{
					if (this.itemClass.AltItemTypeIcon != null && this.itemClass.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, this.itemClass, itemStack.itemValue))
					{
						value = this.itemClass.AltItemTypeIcon;
						return true;
					}
					value = this.itemClass.ItemTypeIcon;
				}
			}
			return true;
		case "hasitemtypeicon":
			value = "false";
			if (!itemStack.IsEmpty() && this.itemClass != null)
			{
				if (this.itemClass.IsBlock())
				{
					value = (Block.list[itemStack.itemValue.type].ItemTypeIcon != "").ToString();
				}
				else
				{
					value = (this.itemClass.ItemTypeIcon != "").ToString();
				}
			}
			return true;
		case "itemtypeicontint":
			value = "255,255,255,255";
			if (!itemStack.IsEmpty() && this.itemClass != null && this.itemClass.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, this.itemClass, itemStack.itemValue))
			{
				value = altitemtypeiconcolorFormatter.Format(this.itemClass.AltItemTypeIconColor);
			}
			return true;
		case "shownormaloptions":
			value = (!isOpenAsTrader).ToString();
			return true;
		case "showtraderoptions":
			value = isOpenAsTrader.ToString();
			return true;
		case "showstats":
			value = showStats.ToString();
			return true;
		case "showdescription":
			value = (!showStats).ToString();
			return true;
		case "iscomparing":
			value = (!CompareStack.IsEmpty()).ToString();
			return true;
		case "isnotcomparing":
			value = CompareStack.IsEmpty().ToString();
			return true;
		case "showstatoptions":
			value = "false";
			return true;
		case "showonlydescription":
			value = (!XUiM_ItemStack.HasItemStats(itemStack)).ToString();
			return true;
		case "showstatanddescription":
			value = XUiM_ItemStack.HasItemStats(itemStack).ToString();
			return true;
		case "itemstattitle1":
			value = ((this.itemClass != null) ? GetStatTitle(0) : "");
			return true;
		case "itemstat1":
			value = ((this.itemClass != null) ? GetStatValue(0) : "");
			return true;
		case "itemstattitle2":
			value = ((this.itemClass != null) ? GetStatTitle(1) : "");
			return true;
		case "itemstat2":
			value = ((this.itemClass != null) ? GetStatValue(1) : "");
			return true;
		case "itemstattitle3":
			value = ((this.itemClass != null) ? GetStatTitle(2) : "");
			return true;
		case "itemstat3":
			value = ((this.itemClass != null) ? GetStatValue(2) : "");
			return true;
		case "itemstattitle4":
			value = ((this.itemClass != null) ? GetStatTitle(3) : "");
			return true;
		case "itemstat4":
			value = ((this.itemClass != null) ? GetStatValue(3) : "");
			return true;
		case "itemstattitle5":
			value = ((this.itemClass != null) ? GetStatTitle(4) : "");
			return true;
		case "itemstat5":
			value = ((this.itemClass != null) ? GetStatValue(4) : "");
			return true;
		case "itemstattitle6":
			value = ((this.itemClass != null) ? GetStatTitle(5) : "");
			return true;
		case "itemstat6":
			value = ((this.itemClass != null) ? GetStatValue(5) : "");
			return true;
		case "itemstattitle7":
			value = ((this.itemClass != null) ? GetStatTitle(6) : "");
			return true;
		case "itemstat7":
			value = ((this.itemClass != null) ? GetStatValue(6) : "");
			return true;
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetStatTitle(int index)
	{
		if (itemDisplayEntry == null || itemDisplayEntry.DisplayStats.Count <= index)
		{
			return "";
		}
		if (itemDisplayEntry.DisplayStats[index].TitleOverride != null)
		{
			return itemDisplayEntry.DisplayStats[index].TitleOverride;
		}
		return UIDisplayInfoManager.Current.GetLocalizedName(itemDisplayEntry.DisplayStats[index].StatType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetStatValue(int index)
	{
		if (itemDisplayEntry == null || itemDisplayEntry.DisplayStats.Count <= index)
		{
			return "";
		}
		DisplayInfoEntry infoEntry = itemDisplayEntry.DisplayStats[index];
		if (!CompareStack.IsEmpty())
		{
			return XUiM_ItemStack.GetStatItemValueTextWithCompareInfo(itemStack.itemValue, CompareStack.itemValue, base.xui.playerUI.entityPlayer, infoEntry, flipCompare: false, useMods: false);
		}
		if (!EquippedStack.IsEmpty())
		{
			return XUiM_ItemStack.GetStatItemValueTextWithCompareInfo(itemStack.itemValue, EquippedStack.itemValue, base.xui.playerUI.entityPlayer, infoEntry, flipCompare: true, useMods: false);
		}
		return XUiM_ItemStack.GetStatItemValueTextWithCompareInfo(itemStack.itemValue, CompareStack.itemValue, base.xui.playerUI.entityPlayer, infoEntry);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void makeVisible(bool _makeVisible)
	{
		if (_makeVisible && windowGroup.isShowing)
		{
			base.ViewComponent.IsVisible = true;
		}
	}

	public void SetItemStack(XUiC_ItemStack stack, bool _makeVisible = false)
	{
		if (stack == null || stack.ItemStack.IsEmpty())
		{
			ShowEmptyInfo();
			return;
		}
		makeVisible(_makeVisible);
		selectedEquipmentStack = null;
		selectedItemStack = stack;
		selectedPartStack = null;
		selectedTraderItemStack = null;
		selectedTurnInItemStack = null;
		SetInfo(stack.ItemStack, stack, XUiC_ItemActionList.ItemActionListTypes.Item);
	}

	public void SetItemStack(XUiC_EquipmentStack stack, bool _makeVisible = false)
	{
		if (stack == null || stack.ItemStack.IsEmpty())
		{
			ShowEmptyInfo();
			return;
		}
		makeVisible(_makeVisible);
		selectedItemStack = null;
		selectedEquipmentStack = stack;
		selectedPartStack = null;
		selectedTraderItemStack = null;
		selectedTurnInItemStack = null;
		SetInfo(stack.ItemStack, stack, XUiC_ItemActionList.ItemActionListTypes.Equipment);
	}

	public void SetItemStack(XUiC_BasePartStack stack, bool _makeVisible = false)
	{
		if (stack == null || stack.ItemStack.IsEmpty())
		{
			ShowEmptyInfo();
			return;
		}
		makeVisible(_makeVisible);
		selectedItemStack = null;
		selectedEquipmentStack = null;
		selectedPartStack = stack;
		selectedTraderItemStack = null;
		selectedTurnInItemStack = null;
		SetInfo(stack.ItemStack, stack, XUiC_ItemActionList.ItemActionListTypes.Part);
	}

	public void SetItemStack(XUiC_TraderItemEntry stack, bool _makeVisible = false)
	{
		if (stack == null || stack.Item == null || stack.Item.IsEmpty())
		{
			ShowEmptyInfo();
			return;
		}
		makeVisible(_makeVisible);
		selectedItemStack = null;
		selectedEquipmentStack = null;
		selectedPartStack = null;
		selectedTraderItemStack = stack;
		selectedTurnInItemStack = null;
		SetInfo(stack.Item, stack, XUiC_ItemActionList.ItemActionListTypes.Trader);
	}

	public void SetItemStack(XUiC_QuestTurnInEntry stack, bool _makeVisible = false)
	{
		if (stack == null || stack.Item == null || stack.Item.IsEmpty())
		{
			ShowEmptyInfo();
			return;
		}
		makeVisible(_makeVisible);
		selectedItemStack = null;
		selectedEquipmentStack = null;
		selectedPartStack = null;
		selectedTraderItemStack = null;
		selectedTurnInItemStack = stack;
		SetInfo(stack.Item, stack, XUiC_ItemActionList.ItemActionListTypes.QuestReward);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowEmptyInfo()
	{
		if (emptyInfoWindow == null)
		{
			emptyInfoWindow = (XUiC_InfoWindow)base.xui.FindWindowGroupByName("backpack").GetChildById("emptyInfoPanel");
		}
		emptyInfoWindow.ViewComponent.IsVisible = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetInfo(ItemStack stack, XUiController controller, XUiC_ItemActionList.ItemActionListTypes actionListType)
	{
		bool flag = stack.itemValue.type == itemStack.itemValue.type && stack.count == itemStack.count;
		itemStack = stack.Clone();
		bool flag2 = itemStack != null && !itemStack.IsEmpty();
		if (itemPreview == null)
		{
			return;
		}
		if (!flag || !stack.itemValue.Equals(itemStack.itemValue))
		{
			compareStack = ItemStack.Empty.Clone();
		}
		itemClass = null;
		int num = 1;
		if (flag2)
		{
			itemClass = itemStack.itemValue.ItemClassOrMissing;
			if (itemClass != null)
			{
				if (itemClass is ItemClassQuest)
				{
					itemClass = ItemClassQuest.GetItemQuestById(itemStack.itemValue.Seed);
				}
				num = (itemClass.IsBlock() ? Block.list[itemStack.itemValue.type].EconomicBundleSize : itemClass.EconomicBundleSize);
			}
		}
		if (itemClass != null)
		{
			itemDisplayEntry = UIDisplayInfoManager.Current.GetDisplayStatsForTag(itemClass.IsBlock() ? Block.list[itemStack.itemValue.type].DisplayType : itemClass.DisplayType);
		}
		if (isOpenAsTrader)
		{
			isBuying = actionListType == XUiC_ItemActionList.ItemActionListTypes.Trader;
			useCustomMarkup = selectedTraderItemStack != null && base.xui.Trader.TraderTileEntity is TileEntityVendingMachine && (base.xui.Trader.Trader.TraderInfo.PlayerOwned || base.xui.Trader.Trader.TraderInfo.Rentable);
			traderActionItemList.SetCraftingActionList(actionListType, controller);
			int count = BuySellCounter.Count;
			if (!flag)
			{
				BuySellCounter.Count = ((itemStack.count >= num) ? num : 0);
			}
			else if (count > itemStack.count)
			{
				BuySellCounter.Count = ((itemStack.count >= num) ? itemStack.count : 0);
			}
			int num2 = (isBuying ? Math.Min(itemStack.count, base.xui.PlayerInventory.CountAvailableSpaceForItem(itemStack.itemValue)) : itemStack.count);
			BuySellCounter.MaxCount = num2 / num * num;
			BuySellCounter.Step = num;
			if (BuySellCounter.Count == 0 && itemStack.count >= num)
			{
				BuySellCounter.Count = num;
			}
			if (SetMaxCountOnDirty)
			{
				BuySellCounter.Count = BuySellCounter.MaxCount;
				SetMaxCountOnDirty = false;
			}
			BuySellCounter.ForceTextRefresh();
		}
		else
		{
			mainActionItemList.SetCraftingActionList(actionListType, controller);
			isBuying = false;
			useCustomMarkup = false;
		}
		if (flag2 && itemStack.itemValue.Modifications != null)
		{
			partList.SetMainItem(itemStack);
			if (itemStack.itemValue.CosmeticMods != null && itemStack.itemValue.CosmeticMods.Length != 0 && itemStack.itemValue.CosmeticMods[0] != null && !itemStack.itemValue.CosmeticMods[0].IsEmpty())
			{
				partList.SetSlot(itemStack.itemValue.CosmeticMods[0], 0);
				partList.SetSlots(itemStack.itemValue.Modifications, 1);
			}
			else
			{
				partList.SetSlots(itemStack.itemValue.Modifications);
			}
			partList.ViewComponent.IsVisible = true;
		}
		else
		{
			partList.ViewComponent.IsVisible = false;
		}
		RefreshBindings();
	}
}
