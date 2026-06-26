using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryTrackRecipe : BaseItemActionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RecipeCraftCount craftCountControl;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selectedCraftingTier = 1;

	public ItemActionEntryTrackRecipe(XUiController controller, XUiC_RecipeCraftCount recipeCraftCount, int craftingTier)
		: base(controller, "lblContextActionTrack", "ui_game_symbol_compass", GamepadShortCut.DPadLeft)
	{
		craftCountControl = recipeCraftCount;
		Recipe recipe = ((XUiC_RecipeEntry)base.ItemController).Recipe;
		selectedCraftingTier = (recipe.craftingTier = craftingTier);
	}

	public override void OnActivated()
	{
		Recipe recipe = ((XUiC_RecipeEntry)base.ItemController).Recipe;
		recipe.craftingTier = selectedCraftingTier;
		XUi xui = base.ItemController.xui;
		EntityPlayerLocal entityPlayer = xui.playerUI.entityPlayer;
		if (xui.Recipes.TrackedRecipe != recipe || xui.Recipes.TrackedRecipeQuality != selectedCraftingTier)
		{
			if (xui.Recipes.TrackedRecipe == null)
			{
				xui.Recipes.SetPreviousTracked(entityPlayer);
			}
			xui.Recipes.TrackedRecipeQuality = selectedCraftingTier;
			xui.Recipes.TrackedRecipeCount = 1;
			xui.Recipes.TrackedRecipe = recipe;
		}
		else
		{
			xui.Recipes.TrackedRecipe = null;
			xui.Recipes.ResetToPreviousTracked(entityPlayer);
		}
	}
}
