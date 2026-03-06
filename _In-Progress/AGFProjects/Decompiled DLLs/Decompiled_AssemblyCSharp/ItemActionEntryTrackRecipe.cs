using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryTrackRecipe : BaseItemActionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int selectedCraftingTier;

	public ItemActionEntryTrackRecipe(XUiController _controller, int _craftingTier)
		: base(_controller, "lblContextActionTrack", "ui_game_symbol_compass", GamepadShortCut.DPadLeft)
	{
		Recipe recipe = ((XUiC_RecipeEntry)base.ItemController).Recipe;
		selectedCraftingTier = (recipe.craftingTier = _craftingTier);
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
