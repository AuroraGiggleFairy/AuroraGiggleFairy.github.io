using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryRecipes : BaseItemActionEntry
{
	public ItemActionEntryRecipes(XUiController _controller)
		: base(_controller, "lblContextActionRecipes", "ui_game_symbol_hammer", GamepadShortCut.DPadLeft)
	{
	}

	public override void RefreshEnabled()
	{
		base.Enabled = true;
	}

	public override void OnActivated()
	{
		XUi xui = base.ItemController.xui;
		xui.playerUI.windowManager.CloseIfOpen("looting");
		List<XUiC_RecipeList> childrenByType = xui.GetChildrenByType<XUiC_RecipeList>();
		XUiC_RecipeList xUiC_RecipeList = null;
		for (int i = 0; i < childrenByType.Count; i++)
		{
			if (childrenByType[i].WindowGroup != null && childrenByType[i].WindowGroup.isShowing)
			{
				xUiC_RecipeList = childrenByType[i];
				break;
			}
		}
		if (xUiC_RecipeList == null)
		{
			XUiC_WindowSelector.OpenSelectorAndWindow(xui.playerUI.entityPlayer, "crafting");
			xUiC_RecipeList = xui.GetChildByType<XUiC_RecipeList>();
		}
		ItemStack recipeDataByIngredientStack = ItemStack.Empty.Clone();
		if (base.ItemController is XUiC_ItemStack xUiC_ItemStack)
		{
			recipeDataByIngredientStack = xUiC_ItemStack.ItemStack;
		}
		xUiC_RecipeList?.SetRecipeDataByIngredientStack(recipeDataByIngredientStack);
	}
}
