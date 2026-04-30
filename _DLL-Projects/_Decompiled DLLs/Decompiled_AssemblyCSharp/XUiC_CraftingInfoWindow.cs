using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CraftingInfoWindow : XUiC_InfoWindow
{
	public enum TabTypes
	{
		Ingredients,
		Description,
		UnlockedBy
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe recipe;

	[PublicizedFrom(EAccessModifier.Private)]
	public int craftCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selectedCraftingTier = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selectedMaxCraftingTier = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe lastRecipeSelected;

	public TabTypes TabType;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RecipeEntry selectedEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController itemPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController windowName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController windowIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController description;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController craftingTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController requiredToolOverlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController requiredToolCheckmark;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController requiredToolText;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_IngredientList ingredientList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_UnlockByList unlockByList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemActionList actionItemList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RecipeList recipeList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_InfoWindow emptyInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemInfoWindow itemInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RecipeCraftCount recipeCraftCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController ingredientsButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController descriptionButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController unlockedByButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController addQualityButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController subtractQualityButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<float> craftingtimeFormatter = new CachedStringFormatter<float>([PublicizedFrom(EAccessModifier.Internal)] (float _time) => $"{(int)(_time / 60f):00}:{(int)(_time % 60f):00}");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor durabilitycolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat durabilityfillFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt durabilitytextFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor altitemtypeiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public Color validColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color invalidColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string validSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public string invalidSprite;

	public int SelectedCraftingTier => selectedCraftingTier;

	public override void Init()
	{
		base.Init();
		itemPreview = GetChildById("itemPreview");
		windowName = GetChildById("windowName");
		windowIcon = GetChildById("windowIcon");
		description = GetChildById("descriptionText");
		craftingTime = GetChildById("craftingTime");
		addQualityButton = GetChildById("addQualityButton");
		addQualityButton.OnPress += AddQualityButton_OnPress;
		subtractQualityButton = GetChildById("subtractQualityButton");
		subtractQualityButton.OnPress += SubtractQualityButton_OnPress;
		requiredToolOverlay = GetChildById("requiredToolOverlay");
		requiredToolCheckmark = GetChildById("requiredToolCheckmark");
		requiredToolText = GetChildById("requiredToolText");
		actionItemList = (XUiC_ItemActionList)GetChildById("itemActions");
		ingredientsButton = GetChildById("ingredientsButton");
		ingredientsButton.OnPress += IngredientsButton_OnPress;
		descriptionButton = GetChildById("descriptionButton");
		descriptionButton.OnPress += DescriptionButton_OnPress;
		unlockedByButton = GetChildById("showunlocksButton");
		unlockedByButton.OnPress += UnlockedByButton_OnPress;
		recipeCraftCount = GetChildByType<XUiC_RecipeCraftCount>();
		if (recipeCraftCount != null)
		{
			recipeCraftCount.OnCountChanged += HandleOnCountChanged;
		}
		recipeList = windowGroup.Controller.GetChildByType<XUiC_RecipeList>();
		if (recipeList != null)
		{
			recipeList.RecipeChanged += HandleRecipeChanged;
		}
		categoryList = windowGroup.Controller.GetChildByType<XUiC_CategoryList>();
		ingredientList = GetChildByType<XUiC_IngredientList>();
		unlockByList = GetChildByType<XUiC_UnlockByList>();
		IsDormant = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SubtractQualityButton_OnPress(XUiController _sender, int _mouseButton)
	{
		if (selectedCraftingTier > 1)
		{
			selectedCraftingTier--;
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddQualityButton_OnPress(XUiController _sender, int _mouseButton)
	{
		if (selectedCraftingTier < 6)
		{
			selectedCraftingTier++;
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectedButtonByType(TabTypes tabType)
	{
		((XUiV_Button)ingredientsButton.ViewComponent).Selected = tabType == TabTypes.Ingredients;
		((XUiV_Button)descriptionButton.ViewComponent).Selected = tabType == TabTypes.Description;
		((XUiV_Button)unlockedByButton.ViewComponent).Selected = tabType == TabTypes.UnlockedBy;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void IngredientsButton_OnPress(XUiController _sender, int _mouseButton)
	{
		TabType = TabTypes.Ingredients;
		SetSelectedButtonByType(TabType);
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DescriptionButton_OnPress(XUiController _sender, int _mouseButton)
	{
		TabType = TabTypes.Description;
		SetSelectedButtonByType(TabType);
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UnlockedByButton_OnPress(XUiController _sender, int _mouseButton)
	{
		TabType = TabTypes.UnlockedBy;
		SetSelectedButtonByType(TabType);
		IsDirty = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		IsDormant = false;
		if (base.xui.PlayerInventory != null)
		{
			base.xui.PlayerInventory.OnBackpackItemsChanged += PlayerInventory_OnItemsChanged;
			base.xui.PlayerInventory.OnToolbeltItemsChanged += PlayerInventory_OnItemsChanged;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		IsDormant = true;
		if (base.xui.PlayerInventory != null)
		{
			base.xui.PlayerInventory.OnBackpackItemsChanged -= PlayerInventory_OnItemsChanged;
			base.xui.PlayerInventory.OnToolbeltItemsChanged -= PlayerInventory_OnItemsChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnItemsChanged()
	{
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnCountChanged(XUiController _sender, OnCountChangedEventArgs _e)
	{
		craftCount = _e.Count;
		IsDirty = true;
	}

	public override void Deselect()
	{
		if (selectedEntry != null)
		{
			selectedEntry.Selected = false;
		}
	}

	public void SetCategory(string category)
	{
		if (categoryList != null && categoryList.CurrentCategory?.CategoryName != category)
		{
			categoryList.SetCategory(category);
		}
		if (recipeList != null && recipeList.GetCategory() != category)
		{
			recipeList.SetCategory(category);
		}
	}

	public override void Update(float _dt)
	{
		if (!windowGroup.isShowing)
		{
			return;
		}
		base.Update(_dt);
		if (!windowGroup.isShowing || !IsDirty)
		{
			return;
		}
		if (emptyInfoWindow == null)
		{
			emptyInfoWindow = (XUiC_InfoWindow)base.xui.FindWindowGroupByName("backpack").GetChildById("emptyInfoPanel");
		}
		if (itemInfoWindow == null)
		{
			itemInfoWindow = (XUiC_ItemInfoWindow)base.xui.FindWindowGroupByName("backpack").GetChildById("itemInfoPanel");
		}
		lastRecipeSelected = recipe;
		recipe = ((selectedEntry != null) ? selectedEntry.Recipe : null);
		bool flag = recipe != null;
		if (flag)
		{
			int craftingTier = recipe.GetCraftingTier(base.xui.playerUI.entityPlayer);
			if (recipe != lastRecipeSelected || (recipe == lastRecipeSelected && craftingTier != selectedMaxCraftingTier))
			{
				selectedCraftingTier = craftingTier;
			}
			selectedMaxCraftingTier = craftingTier;
		}
		if (emptyInfoWindow != null && !flag && !itemInfoWindow.ViewComponent.IsVisible)
		{
			emptyInfoWindow.ViewComponent.IsVisible = true;
		}
		if (itemPreview == null)
		{
			return;
		}
		_ = itemPreview.ViewComponent;
		_ = windowName.ViewComponent;
		_ = windowIcon.ViewComponent;
		_ = description.ViewComponent;
		_ = craftingTime.ViewComponent;
		if (ingredientList != null)
		{
			ingredientList.CraftingTier = selectedCraftingTier;
			ingredientList.Recipe = recipe;
		}
		if (unlockByList != null)
		{
			unlockByList.Recipe = recipe;
		}
		actionItemList.SetCraftingActionList(flag ? XUiC_ItemActionList.ItemActionListTypes.Crafting : XUiC_ItemActionList.ItemActionListTypes.None, selectedEntry);
		XUiC_WorkstationToolGrid childByType = windowGroup.Controller.GetChildByType<XUiC_WorkstationToolGrid>();
		if (childByType != null && selectedEntry != null && selectedEntry.Recipe != null && selectedEntry.Recipe.craftingToolType != 0)
		{
			requiredToolOverlay.ViewComponent.IsVisible = true;
			ItemClass forId = ItemClass.GetForId(selectedEntry.Recipe.craftingToolType);
			string text = "";
			if (forId != null)
			{
				text = ((!forId.IsBlock()) ? forId.GetLocalizedItemName() : Block.list[forId.Id].GetLocalizedBlockName());
				string format = Localization.Get("xuiToolRequired");
				((XUiV_Label)requiredToolText.ViewComponent).Text = string.Format(format, text);
				if (childByType.HasRequirement(selectedEntry.Recipe))
				{
					((XUiV_Sprite)requiredToolCheckmark.ViewComponent).Color = validColor;
					((XUiV_Sprite)requiredToolCheckmark.ViewComponent).SpriteName = validSprite;
				}
				else
				{
					((XUiV_Sprite)requiredToolCheckmark.ViewComponent).Color = invalidColor;
					((XUiV_Sprite)requiredToolCheckmark.ViewComponent).SpriteName = invalidSprite;
				}
			}
		}
		else
		{
			requiredToolOverlay.ViewComponent.IsVisible = false;
		}
		recipeCraftCount.RefreshCounts();
		RefreshBindings();
		IsDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "itemname":
			value = ((recipe != null) ? Localization.Get(recipe.GetName()) : "");
			return true;
		case "itemicon":
			if (recipe != null)
			{
				ItemValue itemValue2 = new ItemValue(recipe.itemValueType);
				value = itemValue2.GetPropertyOverride("CustomIcon", itemValue2.ItemClass.GetIconName());
			}
			return true;
		case "itemicontint":
		{
			Color32 v = Color.white;
			if (recipe != null)
			{
				ItemValue itemValue = new ItemValue(recipe.itemValueType);
				v = itemValue.ItemClass.GetIconTint(itemValue);
			}
			value = itemicontintcolorFormatter.Format(v);
			return true;
		}
		case "itemgroupicon":
			value = ((recipe == null) ? "" : categoryList.CurrentCategory?.SpriteName);
			return true;
		case "itemdescription":
		{
			string text = "";
			if (recipe != null)
			{
				ItemClass forId3 = ItemClass.GetForId(recipe.itemValueType);
				if (forId3 != null)
				{
					if (forId3.IsBlock())
					{
						string descriptionKey = Block.list[recipe.itemValueType].DescriptionKey;
						if (Localization.Exists(descriptionKey))
						{
							text = Localization.Get(descriptionKey);
						}
					}
					else
					{
						string itemDescriptionKey = forId3.GetItemDescriptionKey();
						if (Localization.Exists(itemDescriptionKey))
						{
							text = Localization.Get(itemDescriptionKey);
						}
					}
				}
			}
			value = text;
			return true;
		}
		case "craftingtime":
			value = "";
			if (recipe != null)
			{
				float recipeCraftTime = XUiM_Recipes.GetRecipeCraftTime(base.xui, recipe);
				float num = recipeCraftTime * (float)(craftCount - 1) + recipeCraftTime;
				value = craftingtimeFormatter.Format(num + 0.5f);
			}
			return true;
		case "hasdurability":
			value = (recipe != null && recipe.GetOutputItemClass().ShowQualityBar).ToString();
			return true;
		case "durabilitycolor":
		{
			Color32 v2 = Color.white;
			if (recipe != null)
			{
				v2 = QualityInfo.GetTierColor(selectedCraftingTier);
			}
			value = durabilitycolorFormatter.Format(v2);
			return true;
		}
		case "durabilityfill":
			value = ((recipe == null) ? "0" : "1");
			return true;
		case "durabilityjustify":
			value = "center";
			if (recipe != null && !recipe.GetOutputItemClass().ShowQualityBar)
			{
				value = "right";
			}
			return true;
		case "durabilitytext":
			value = "";
			if (recipe != null && recipe.GetOutputItemClass().ShowQualityBar)
			{
				value = durabilitytextFormatter.Format(selectedCraftingTier);
			}
			return true;
		case "itemtypeicon":
			value = "";
			if (recipe != null)
			{
				ItemClass forId5 = ItemClass.GetForId(recipe.itemValueType);
				if (forId5 != null)
				{
					if (forId5.IsBlock())
					{
						value = forId5.GetBlock().ItemTypeIcon;
					}
					else
					{
						if (forId5.AltItemTypeIcon != null && forId5.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, forId5))
						{
							value = forId5.AltItemTypeIcon;
							return true;
						}
						value = forId5.ItemTypeIcon;
					}
				}
			}
			return true;
		case "hasitemtypeicon":
			value = "false";
			if (recipe != null)
			{
				ItemClass forId2 = ItemClass.GetForId(recipe.itemValueType);
				if (forId2 != null)
				{
					if (forId2.IsBlock())
					{
						value = (forId2.GetBlock().ItemTypeIcon != "").ToString();
					}
					else
					{
						value = (forId2.ItemTypeIcon != "").ToString();
					}
				}
			}
			return true;
		case "itemtypeicontint":
			value = "255,255,255,255";
			if (recipe != null)
			{
				ItemClass forId = ItemClass.GetForId(recipe.itemValueType);
				if (forId != null && forId.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, forId))
				{
					value = altitemtypeiconcolorFormatter.Format(forId.AltItemTypeIconColor);
				}
			}
			return true;
		case "showingredients":
			value = (TabType == TabTypes.Ingredients).ToString();
			return true;
		case "showdescription":
			value = (TabType == TabTypes.Description).ToString();
			return true;
		case "showunlockedby":
			value = (TabType == TabTypes.UnlockedBy).ToString();
			return true;
		case "showunlockedbytab":
			value = "false";
			if (recipe != null && !XUiM_Recipes.GetRecipeIsUnlocked(base.xui, recipe))
			{
				ItemClass forId4 = ItemClass.GetForId(recipe.itemValueType);
				if (forId4 != null)
				{
					if (forId4.IsBlock())
					{
						value = (forId4.GetBlock().UnlockedBy.Length != 0).ToString();
					}
					else
					{
						value = (forId4.UnlockedBy.Length != 0).ToString();
					}
				}
			}
			return true;
		case "enableaddquality":
			if (recipe != null && recipe.GetOutputItemClass().ShowQualityBar && selectedCraftingTier < (int)EffectManager.GetValue(PassiveEffects.CraftingTier, null, 1f, base.xui.playerUI.entityPlayer, recipe, recipe.tags))
			{
				value = "true";
			}
			else
			{
				value = "false";
			}
			return true;
		case "enablesubtractquality":
			if (recipe != null && recipe.GetOutputItemClass().ShowQualityBar && selectedCraftingTier > 1)
			{
				value = "true";
			}
			else
			{
				value = "false";
			}
			return true;
		default:
			return false;
		}
	}

	public void SetRecipe(XUiC_RecipeEntry _recipeEntry)
	{
		selectedEntry = _recipeEntry;
		if (recipeCraftCount != null)
		{
			recipeCraftCount.IsDirty = true;
			craftCount = recipeCraftCount.Count;
		}
		else
		{
			craftCount = 1;
		}
		if (selectedEntry != null && selectedEntry.Recipe != null)
		{
			if (XUiM_Recipes.GetRecipeIsUnlocked(base.xui, selectedEntry.Recipe))
			{
				TabType = TabTypes.Ingredients;
				SetSelectedButtonByType(TabType);
			}
			else
			{
				TabType = TabTypes.UnlockedBy;
				SetSelectedButtonByType(TabType);
			}
		}
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleRecipeChanged(Recipe _recipe, XUiC_RecipeEntry _recipeEntry)
	{
		if (base.WindowGroup.isShowing)
		{
			base.ViewComponent.IsVisible = true;
		}
		SetRecipe(_recipeEntry);
	}

	public override bool ParseAttribute(string attribute, string value, XUiController _parent)
	{
		if (base.ParseAttribute(attribute, value, _parent))
		{
			return true;
		}
		switch (attribute)
		{
		case "valid_color":
			validColor = StringParsers.ParseColor32(value);
			return true;
		case "invalid_color":
			invalidColor = StringParsers.ParseColor32(value);
			return true;
		case "valid_sprite":
			validSprite = value;
			return true;
		case "invalid_sprite":
			invalidSprite = value;
			return true;
		default:
			return false;
		}
	}

	public void RefreshRecipe()
	{
		if (recipeCraftCount != null)
		{
			recipeCraftCount.CalculateMaxCount();
		}
	}
}
