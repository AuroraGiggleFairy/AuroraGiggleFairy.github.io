using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryFavorite : BaseItemActionEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Recipe recipe;

	public ItemActionEntryFavorite(XUiController _controller, Recipe _recipe)
		: base(_controller, "lblContextActionFavorite", "server_favorite", GamepadShortCut.DPadRight)
	{
		recipe = _recipe;
	}

	public override void OnActivated()
	{
		XUiC_RecipeEntry xUiC_RecipeEntry = (XUiC_RecipeEntry)base.ItemController;
		if (xUiC_RecipeEntry?.Recipe == null)
		{
			if (recipe != null)
			{
				CraftingManager.ToggleFavoriteRecipe(recipe);
			}
		}
		else
		{
			CraftingManager.ToggleFavoriteRecipe(xUiC_RecipeEntry.Recipe);
		}
		xUiC_RecipeEntry?.WindowGroup.Controller.GetChildByType<XUiC_RecipeList>()?.RefreshCurrentRecipes();
	}
}
