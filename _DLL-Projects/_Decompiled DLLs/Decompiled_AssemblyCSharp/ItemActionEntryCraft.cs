using System.Collections.Generic;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryCraft : BaseItemActionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum StateTypes
	{
		Normal,
		RecipeLocked,
		NotEnoughMaterials,
		WrongWorkStation,
		Other
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public StateTypes state;

	[PublicizedFrom(EAccessModifier.Private)]
	public string otherMessage = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int craftingTier;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_RecipeCraftCount craftCountControl;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ItemStack> tempIngredientList = new List<ItemStack>();

	public ItemActionEntryCraft(XUiController _controller, XUiC_RecipeCraftCount _recipeCraftCount, int _craftingTier)
		: base(_controller, "lblContextActionCraft", "ui_game_symbol_hammer", GamepadShortCut.DPadUp)
	{
		craftCountControl = _recipeCraftCount;
		craftingTier = _craftingTier;
		WorkstationData workstationData = CraftingManager.GetWorkstationData(((XUiC_RecipeEntry)base.ItemController).Recipe.craftingArea);
		if (workstationData != null)
		{
			base.ActionName = workstationData.CraftActionName;
			base.IconName = workstationData.CraftIcon;
			base.SoundName = workstationData.CraftSound;
		}
	}

	public override void RefreshEnabled()
	{
		base.Enabled = true;
		XUiC_RecipeEntry xUiC_RecipeEntry = (XUiC_RecipeEntry)base.ItemController;
		Recipe recipe = xUiC_RecipeEntry.Recipe;
		if (xUiC_RecipeEntry.Recipe == null)
		{
			state = StateTypes.Other;
			base.Enabled = false;
			return;
		}
		if (!xUiC_RecipeEntry.IsCurrentWorkstation)
		{
			state = StateTypes.WrongWorkStation;
			base.Enabled = false;
			return;
		}
		if (!XUiM_Recipes.GetRecipeIsUnlocked(base.ItemController.xui, ((XUiC_RecipeEntry)base.ItemController).Recipe))
		{
			state = StateTypes.RecipeLocked;
			base.Enabled = false;
			return;
		}
		List<XUiC_CraftingWindowGroup> childrenByType = base.ItemController.xui.GetChildrenByType<XUiC_CraftingWindowGroup>();
		for (int i = 0; i < childrenByType.Count; i++)
		{
			if (childrenByType[i].WindowGroup != null && childrenByType[i].WindowGroup.isShowing)
			{
				XUiC_CraftingWindowGroup xUiC_CraftingWindowGroup = childrenByType[i];
				if (xUiC_CraftingWindowGroup.CraftingRequirementsValid(((XUiC_RecipeEntry)base.ItemController).Recipe))
				{
					break;
				}
				state = StateTypes.Other;
				otherMessage = xUiC_CraftingWindowGroup.CraftingRequirementsInvalidMessage(((XUiC_RecipeEntry)base.ItemController).Recipe);
				base.Enabled = false;
				return;
			}
		}
		if (!hasItems(xUiC_RecipeEntry.xui, recipe))
		{
			state = StateTypes.NotEnoughMaterials;
			base.Enabled = false;
			return;
		}
		if (craftingTier > (int)EffectManager.GetValue(PassiveEffects.CraftingTier, null, 1f, base.ItemController.xui.playerUI.entityPlayer, recipe, recipe.tags))
		{
			state = StateTypes.RecipeLocked;
			base.Enabled = false;
			return;
		}
		ItemAction holdingPrimary = base.ItemController.xui.playerUI.entityPlayer.inventory.GetHoldingPrimary();
		if (holdingPrimary != null && holdingPrimary.IsActionRunning(base.ItemController.xui.playerUI.entityPlayer.inventory.holdingItemData.actionData[0]))
		{
			state = StateTypes.Other;
			base.Enabled = false;
		}
		else if (base.ItemController.xui.isUsingItemActionEntryUse)
		{
			state = StateTypes.Other;
			base.Enabled = false;
		}
	}

	public override void OnActivated()
	{
		Recipe recipe = ((XUiC_RecipeEntry)base.ItemController).Recipe;
		XUi xui = base.ItemController.xui;
		List<XUiC_CraftingWindowGroup> childrenByType = xui.GetChildrenByType<XUiC_CraftingWindowGroup>();
		XUiC_CraftingWindowGroup xUiC_CraftingWindowGroup = null;
		for (int i = 0; i < childrenByType.Count; i++)
		{
			if (childrenByType[i].WindowGroup != null && childrenByType[i].WindowGroup.isShowing)
			{
				xUiC_CraftingWindowGroup = childrenByType[i];
				break;
			}
		}
		if (xUiC_CraftingWindowGroup == null)
		{
			return;
		}
		if (!XUiM_Recipes.GetRecipeIsUnlocked(base.ItemController.xui, recipe))
		{
			craftCountControl.IsDirty = true;
			xUiC_CraftingWindowGroup.WindowGroup.Controller.SetAllChildrenDirty();
			return;
		}
		if (!xUiC_CraftingWindowGroup.CraftingRequirementsValid(recipe))
		{
			craftCountControl.IsDirty = true;
			xUiC_CraftingWindowGroup.WindowGroup.Controller.SetAllChildrenDirty();
			return;
		}
		Recipe recipe2 = new Recipe
		{
			itemValueType = recipe.itemValueType,
			count = XUiM_Recipes.GetRecipeCraftOutputCount(xui, recipe),
			craftingArea = recipe.craftingArea,
			craftExpGain = recipe.craftExpGain,
			craftingTime = XUiM_Recipes.GetRecipeCraftTime(xui, recipe),
			craftingToolType = recipe.craftingToolType,
			craftingTier = craftingTier,
			tags = recipe.tags
		};
		if (!hasItems(xui, recipe))
		{
			return;
		}
		bool flag = false;
		for (int j = 0; j < recipe.ingredients.Count; j++)
		{
			flag |= recipe.ingredients[j].itemValue.HasQuality;
			if (flag || tempIngredientList[j].count != recipe.ingredients[j].count)
			{
				recipe2.scrapable = true;
			}
		}
		recipe2.AddIngredients(tempIngredientList);
		XUiC_WorkstationInputGrid childByType = craftCountControl.WindowGroup.Controller.GetChildByType<XUiC_WorkstationInputGrid>();
		if (xUiC_CraftingWindowGroup.AddItemToQueue(recipe2))
		{
			if (flag)
			{
				tempIngredientList.Clear();
			}
			if (childByType != null)
			{
				childByType.RemoveItems(recipe2.ingredients, craftCountControl.Count, tempIngredientList);
			}
			else
			{
				xui.PlayerInventory.RemoveItems(recipe2.ingredients, craftCountControl.Count, tempIngredientList);
			}
			if (flag)
			{
				recipe2.ingredients.Clear();
				recipe2.AddIngredients(tempIngredientList);
			}
			if (recipe == xui.Recipes.TrackedRecipe)
			{
				xui.Recipes.TrackedRecipe = null;
				xui.Recipes.ResetToPreviousTracked(xui.playerUI.entityPlayer);
			}
		}
		else
		{
			warnQueueFull();
		}
		craftCountControl.IsDirty = true;
		xUiC_CraftingWindowGroup.WindowGroup.Controller.SetAllChildrenDirty();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasItems(XUi _xui, Recipe _recipe)
	{
		bool flag = false;
		List<ItemStack> allItemStacks = _xui.PlayerInventory.GetAllItemStacks();
		tempIngredientList.Clear();
		for (int i = 0; i < _recipe.ingredients.Count; i++)
		{
			int num = _recipe.ingredients[i].count;
			if (_recipe.UseIngredientModifier)
			{
				num = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, _recipe.ingredients[i].count, _xui.playerUI.entityPlayer, _recipe, FastTags<TagGroup.Global>.Parse(_recipe.ingredients[i].itemValue.ItemClass.GetItemName()), calcEquipment: true, calcHoldingItem: true, calcProgression: true, calcBuffs: true, calcChallenges: true, craftingTier);
			}
			if (!_recipe.ingredients[i].itemValue.HasQuality)
			{
				tempIngredientList.Add(new ItemStack(_recipe.ingredients[i].itemValue, num));
				continue;
			}
			int num2 = ((num == 0) ? 1 : num);
			tempIngredientList.Add(new ItemStack(_recipe.ingredients[i].itemValue.Clone(), num2));
			for (int j = 0; j < allItemStacks.Count; j++)
			{
				ItemStack itemStack = allItemStacks[j];
				if ((!itemStack.itemValue.HasModSlots || !itemStack.itemValue.HasMods()) && itemStack.itemValue.type == _recipe.ingredients[i].itemValue.type)
				{
					num2--;
					if (num2 == 0)
					{
						break;
					}
				}
			}
			if (num2 > 0)
			{
				return false;
			}
		}
		flag |= _xui.PlayerInventory.HasItems(tempIngredientList, craftCountControl.Count);
		XUiC_WorkstationInputGrid childByType = craftCountControl.WindowGroup.Controller.GetChildByType<XUiC_WorkstationInputGrid>();
		if (childByType != null)
		{
			flag |= childByType.HasItems(tempIngredientList, craftCountControl.Count);
		}
		return flag;
	}

	public override void OnDisabledActivate()
	{
		EntityPlayerLocal entityPlayer = base.ItemController.xui.playerUI.entityPlayer;
		switch (state)
		{
		case StateTypes.RecipeLocked:
			GameManager.ShowTooltip(entityPlayer, Localization.Get("ttMissingCraftingRecipe"));
			break;
		case StateTypes.NotEnoughMaterials:
			GameManager.ShowTooltip(entityPlayer, Localization.Get("ttMissingCraftingResources"));
			break;
		case StateTypes.Other:
			GameManager.ShowTooltip(entityPlayer, otherMessage);
			break;
		case StateTypes.WrongWorkStation:
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void warnQueueFull()
	{
		GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, Localization.Get("xuiCraftQueueFull"));
		Manager.PlayInsidePlayerHead("ui_denied");
	}
}
