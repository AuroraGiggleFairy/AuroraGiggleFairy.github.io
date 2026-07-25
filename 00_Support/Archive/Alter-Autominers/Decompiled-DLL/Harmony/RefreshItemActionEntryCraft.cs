using HarmonyLib;

namespace Harmony;

[HarmonyPatch(typeof(ItemActionEntryCraft))]
[HarmonyPatch("RefreshEnabled")]
public class RefreshItemActionEntryCraft
{
	public static void Postfix(ItemActionEntryCraft __instance)
	{
		XUiC_RecipeEntry xuiC_RecipeEntry = (XUiC_RecipeEntry)__instance.ItemController;
		Recipe recipe = xuiC_RecipeEntry.Recipe;
		if (recipe != null && (recipe.craftingArea == "oilPumpjack" || recipe.craftingArea == "miningMachine"))
		{
			__instance.ActionName = Localization.Get("lblContextActionMHarvest", false);
			__instance.IconName = "ui_game_symbol_miningmachine";
		}
	}
}
