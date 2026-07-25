using HarmonyLib;

namespace Harmony;

[HarmonyPatch(typeof(XUiC_ItemActionList))]
[HarmonyPatch("AddActionListEntry")]
public class AddActionListEntry
{
	public static bool Prefix(BaseItemActionEntry actionEntry)
	{
		if (actionEntry is ItemActionEntryFavorite && actionEntry.ItemController is XUiC_RecipeEntry)
		{
			XUiC_RecipeEntry xuiC_RecipeEntry = (XUiC_RecipeEntry)actionEntry.ItemController;
			Recipe recipe = xuiC_RecipeEntry.Recipe;
			if (recipe != null && (recipe.craftingArea == "oilPumpjack" || recipe.craftingArea == "miningMachine"))
			{
				return false;
			}
		}
		return true;
	}
}
