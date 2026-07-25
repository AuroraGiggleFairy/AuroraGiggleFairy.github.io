using System.Collections.Generic;
using HarmonyLib;

namespace Harmony;

[HarmonyPatch(typeof(XUiM_Recipes))]
[HarmonyPatch("FilterRecipesByName")]
public class FilterRecipesByName
{
	public static void Postfix(ref List<Recipe> __result)
	{
		List<Recipe> recipesToRemove = new List<Recipe>();
		foreach (Recipe recipe in __result)
		{
			if (recipe.craftingArea == "oilPumpjack" || recipe.craftingArea == "miningMachine")
			{
				recipesToRemove.Add(recipe);
			}
		}
		foreach (Recipe recipe2 in recipesToRemove)
		{
			__result.Remove(recipe2);
		}
	}
}
