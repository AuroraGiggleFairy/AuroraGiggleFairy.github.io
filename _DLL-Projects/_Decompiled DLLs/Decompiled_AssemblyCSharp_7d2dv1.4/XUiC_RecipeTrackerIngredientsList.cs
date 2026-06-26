using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_RecipeTrackerIngredientsList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe recipe;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selectedCraftingTier;

	[PublicizedFrom(EAccessModifier.Private)]
	public int count = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_RecipeTrackerIngredientEntry> ingredientEntries = new List<XUiC_RecipeTrackerIngredientEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstSetup;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastComplete;

	public string completeIconName = "";

	public string incompleteIconName = "";

	public string completeHexColor = "FF00FF00";

	public string incompleteHexColor = "FFB400";

	public string warningHexColor = "FFFF00FF";

	public string inactiveHexColor = "888888FF";

	public string activeHexColor = "FFFFFFFF";

	public string completeColor = "0,255,0,255";

	public string incompleteColor = "255, 180, 0, 255";

	public string warningColor = "255,255,0,255";

	public Recipe Recipe
	{
		get
		{
			return recipe;
		}
		set
		{
			recipe = value;
			selectedCraftingTier = recipe?.craftingTier ?? (-1);
			isDirty = true;
			firstSetup = true;
		}
	}

	public int Count
	{
		get
		{
			return count;
		}
		set
		{
			count = value;
			isDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_RecipeTrackerIngredientEntry[] childrenByType = GetChildrenByType<XUiC_RecipeTrackerIngredientEntry>();
		for (int i = 0; i < childrenByType.Length; i++)
		{
			if (childrenByType[i] != null)
			{
				childrenByType[i].Owner = this;
				ingredientEntries.Add(childrenByType[i]);
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.PlayerInventory.OnBackpackItemsChanged += PlayerInventory_OnBackpackItemsChanged;
		base.xui.PlayerInventory.OnToolbeltItemsChanged += PlayerInventory_OnToolbeltItemsChanged;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.PlayerInventory.OnBackpackItemsChanged -= PlayerInventory_OnBackpackItemsChanged;
		base.xui.PlayerInventory.OnToolbeltItemsChanged -= PlayerInventory_OnToolbeltItemsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnToolbeltItemsChanged()
	{
		isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnBackpackItemsChanged()
	{
		isDirty = true;
	}

	public override void Update(float _dt)
	{
		if (isDirty)
		{
			if (recipe != null)
			{
				bool flag = true;
				int num = ingredientEntries.Count;
				int num2 = recipe.ingredients.Count;
				int craftingTier = ((selectedCraftingTier == -1) ? ((int)EffectManager.GetValue(PassiveEffects.CraftingTier, null, 1f, base.xui.playerUI.entityPlayer, recipe, recipe.tags)) : selectedCraftingTier);
				for (int i = 0; i < num; i++)
				{
					XUiC_RecipeTrackerIngredientEntry xUiC_RecipeTrackerIngredientEntry = ingredientEntries[i];
					ItemStack itemStack = ((i < num2) ? recipe.ingredients[i].Clone() : null);
					if (itemStack != null && recipe.UseIngredientModifier)
					{
						itemStack.count = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, itemStack.count, base.xui.playerUI.entityPlayer, recipe, FastTags<TagGroup.Global>.Parse(itemStack.itemValue.ItemClass.GetItemName()), calcEquipment: true, calcHoldingItem: true, calcProgression: true, calcBuffs: true, calcChallenges: true, craftingTier);
					}
					if (itemStack == null || (itemStack != null && itemStack.count > 0))
					{
						xUiC_RecipeTrackerIngredientEntry.Ingredient = itemStack;
					}
					else
					{
						xUiC_RecipeTrackerIngredientEntry.Ingredient = null;
					}
					if (xUiC_RecipeTrackerIngredientEntry.Ingredient != null && !xUiC_RecipeTrackerIngredientEntry.IsComplete)
					{
						flag = false;
					}
				}
				if (firstSetup)
				{
					lastComplete = flag;
					firstSetup = false;
				}
				if (flag && !lastComplete)
				{
					GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "ttAllIngredientsFound", Localization.Get(recipe.GetName()));
					lastComplete = flag;
					isDirty = false;
					base.Update(_dt);
					return;
				}
				lastComplete = flag;
			}
			else
			{
				int num3 = ingredientEntries.Count;
				for (int j = 0; j < num3; j++)
				{
					ingredientEntries[j].Ingredient = null;
				}
			}
			isDirty = false;
		}
		base.Update(_dt);
	}

	public int GetActiveIngredientCount()
	{
		if (recipe == null)
		{
			return 0;
		}
		int craftingTier = ((selectedCraftingTier == -1) ? ((int)EffectManager.GetValue(PassiveEffects.CraftingTier, null, 1f, base.xui.playerUI.entityPlayer, recipe, recipe.tags)) : selectedCraftingTier);
		int num = 0;
		for (int i = 0; i < recipe.ingredients.Count; i++)
		{
			ItemStack itemStack = recipe.ingredients[i];
			if (recipe.UseIngredientModifier && (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, itemStack.count, base.xui.playerUI.entityPlayer, recipe, FastTags<TagGroup.Global>.Parse(itemStack.itemValue.ItemClass.GetItemName()), calcEquipment: true, calcHoldingItem: true, calcProgression: true, calcBuffs: true, calcChallenges: true, craftingTier) > 0)
			{
				num++;
			}
		}
		return num;
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		switch (name)
		{
		case "complete_icon":
			completeIconName = value;
			return true;
		case "incomplete_icon":
			incompleteIconName = value;
			return true;
		case "complete_color":
		{
			Color32 color2 = StringParsers.ParseColor(value);
			completeColor = $"{color2.r},{color2.g},{color2.b},{color2.a}";
			completeHexColor = Utils.ColorToHex(color2);
			return true;
		}
		case "incomplete_color":
		{
			Color32 color = StringParsers.ParseColor(value);
			incompleteColor = $"{color.r},{color.g},{color.b},{color.a}";
			incompleteHexColor = Utils.ColorToHex(color);
			return true;
		}
		default:
			return base.ParseAttribute(name, value, _parent);
		}
	}
}
