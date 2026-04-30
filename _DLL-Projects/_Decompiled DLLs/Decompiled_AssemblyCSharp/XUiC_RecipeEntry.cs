using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_RecipeEntry : XUiC_SelectableEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe recipe;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasIngredients;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite icoRecipe;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite icoFavorite;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite icoBook;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	public XUiC_RecipeList RecipeList;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor recipeicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor hasingredientsstatecolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor unlockstatecolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor altitemtypeiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	public bool HasIngredients
	{
		get
		{
			return hasIngredients;
		}
		set
		{
			hasIngredients = value;
			RefreshBindings();
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsCurrentWorkstation
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public Recipe Recipe
	{
		get
		{
			return recipe;
		}
		set
		{
			recipe = value;
			IsCurrentWorkstation = false;
			if (recipe != null)
			{
				for (int i = 0; i < RecipeList.craftingArea.Length; i++)
				{
					if (RecipeList.craftingArea[i] == recipe.craftingArea)
					{
						IsCurrentWorkstation = true;
						break;
					}
				}
			}
			if (!base.Selected)
			{
				background.Color = new Color32(64, 64, 64, byte.MaxValue);
			}
			base.ViewComponent.IsNavigatable = (base.ViewComponent.IsSnappable = value != null);
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

	public void SetRecipeAndHasIngredients(Recipe recipe, bool hasIngredients)
	{
		Recipe = recipe;
		this.hasIngredients = hasIngredients;
		isDirty = true;
		RefreshBindings();
		if (recipe == null)
		{
			background.Color = new Color32(64, 64, 64, byte.MaxValue);
		}
	}

	public override void Init()
	{
		base.Init();
		for (int i = 0; i < children.Count; i++)
		{
			XUiView xUiView = children[i].ViewComponent;
			if (xUiView.ID.EqualsCaseInsensitive("name"))
			{
				lblName = xUiView as XUiV_Label;
			}
			else if (xUiView.ID.EqualsCaseInsensitive("icon"))
			{
				icoRecipe = xUiView as XUiV_Sprite;
			}
			else if (xUiView.ID.EqualsCaseInsensitive("favorite"))
			{
				icoFavorite = xUiView as XUiV_Sprite;
			}
			else if (xUiView.ID.EqualsCaseInsensitive("unlocked"))
			{
				icoBook = xUiView as XUiV_Sprite;
			}
			else if (xUiView.ID.EqualsCaseInsensitive("background"))
			{
				background = xUiView as XUiV_Sprite;
			}
		}
		isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		isHovered = _isOver;
		if (background != null && recipe != null && !base.Selected)
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
		case "hasrecipe":
			value = (recipe != null).ToString();
			return true;
		case "recipename":
			value = ((recipe != null) ? Localization.Get(recipe.GetName()) : "");
			return true;
		case "recipeicon":
			value = ((recipe != null) ? recipe.GetIcon() : "");
			return true;
		case "recipeicontint":
		{
			Color32 v3 = Color.white;
			if (recipe != null)
			{
				ItemClass forId3 = ItemClass.GetForId(recipe.itemValueType);
				if (forId3 != null)
				{
					v3 = forId3.GetIconTint();
				}
			}
			value = recipeicontintcolorFormatter.Format(v3);
			return true;
		}
		case "isfavorite":
			value = ((recipe != null) ? XUiM_Recipes.GetRecipeIsFavorite(base.xui, recipe).ToString() : "false");
			return true;
		case "isunlockable":
			value = ((recipe != null) ? (!IsCurrentWorkstation || XUiM_Recipes.GetRecipeIsUnlockable(base.xui, recipe) || recipe.isQuest || recipe.isChallenge || recipe.IsTracked).ToString() : "false");
			return true;
		case "hasingredients":
			value = ((recipe != null) ? HasIngredients.ToString() : "false");
			return true;
		case "hasingredientsstatecolor":
			if (recipe != null)
			{
				Color32 v = new Color32(148, 148, 148, byte.MaxValue);
				if (HasIngredients)
				{
					v = ((!CustomAttributes.ContainsKey("enabled_font_color")) ? ((Color32)Color.white) : ((Color32)StringParsers.ParseColor32(CustomAttributes["enabled_font_color"])));
				}
				else if (CustomAttributes.ContainsKey("disabled_font_color"))
				{
					v = StringParsers.ParseColor32(CustomAttributes["disabled_font_color"]);
				}
				value = hasingredientsstatecolorFormatter.Format(v);
			}
			else
			{
				value = "255,255,255,255";
			}
			return true;
		case "itemtypeicon":
			value = "";
			if (recipe != null)
			{
				ItemClass forId = ItemClass.GetForId(recipe.itemValueType);
				if (forId != null)
				{
					if (forId.IsBlock())
					{
						value = forId.GetBlock().ItemTypeIcon;
					}
					else
					{
						if (forId.AltItemTypeIcon != null && forId.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, forId))
						{
							value = forId.AltItemTypeIcon;
							return true;
						}
						value = forId.ItemTypeIcon;
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
				ItemClass forId4 = ItemClass.GetForId(recipe.itemValueType);
				if (forId4 != null && forId4.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, forId4))
				{
					value = altitemtypeiconcolorFormatter.Format(forId4.AltItemTypeIconColor);
				}
			}
			return true;
		case "unlockstatecolor":
			if (recipe != null)
			{
				if (recipe.isQuest || recipe.isChallenge)
				{
					value = unlockstatecolorFormatter.Format(Color.yellow);
				}
				else
				{
					Color32 v2 = (XUiM_Recipes.GetRecipeIsUnlocked(base.xui, recipe) ? Color.white : Color.gray);
					value = unlockstatecolorFormatter.Format(v2);
				}
			}
			else
			{
				value = "255,255,255,255";
			}
			return true;
		case "unlockicon":
			if (recipe != null)
			{
				if (recipe.isChallenge)
				{
					value = "ui_game_symbol_challenge";
					return true;
				}
				if (recipe.isQuest)
				{
					value = "ui_game_symbol_quest";
					return true;
				}
				if (recipe.IsTracked)
				{
					value = "ui_game_symbol_compass";
					return true;
				}
				if (XUiM_Recipes.GetRecipeIsUnlockable(base.xui, recipe) && !XUiM_Recipes.GetRecipeIsUnlocked(base.xui, recipe))
				{
					value = "ui_game_symbol_lock";
				}
				else if (!IsCurrentWorkstation)
				{
					WorkstationData workstationData = CraftingManager.GetWorkstationData(recipe.craftingArea);
					if (workstationData != null)
					{
						value = workstationData.WorkstationIcon;
					}
					else
					{
						value = "ui_game_symbol_hammer";
					}
				}
				else
				{
					value = "";
				}
			}
			else
			{
				value = "ui_game_symbol_book";
			}
			return true;
		default:
			return false;
		}
	}

	public void Refresh()
	{
		isDirty = true;
		RefreshBindings();
	}
}
