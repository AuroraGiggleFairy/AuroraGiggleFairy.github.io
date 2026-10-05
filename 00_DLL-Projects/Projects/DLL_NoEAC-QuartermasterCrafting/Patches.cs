using System;
using System.Collections.Generic;
using HarmonyLib;

namespace QuartermasterCrafting
{
	[HarmonyPatch(typeof(XUiC_IngredientEntry), "GetBindingValueInternal")]
	internal static class Patch_IngredientEntry
	{
		private static bool Prefix(XUiC_IngredientEntry __instance, ref bool __result, ref string value, string bindingName)
		{
			if (bindingName != "haveneedcount")
			{
				return true;
			}

			try
			{
				if (__instance.windowGroup == null || !__instance.windowGroup.isShowing)
				{
					return true;
				}

				ItemStack ingredient = __instance.Ingredient;
				Recipe recipe = CraftBridge.RecipeFor(__instance);
				if (recipe == null || ingredient == null || RecipeLines.LeaveVanilla(recipe) || __instance.xui == null)
				{
					return true;
				}

				int count = CraftBridge.ShownCraftCount(__instance.windowGroup.Controller);
				if (!CraftBridge.ReadHeld(__instance.xui, recipe, ingredient, count, out int held, out int total, out int need))
				{
					return true;
				}

				value = "[7EC8FF](" + held + ")[-] " + total + " / " + need;
				__result = true;
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("ingredient line", ex);
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(ItemActionEntryCraft), "hasItems")]
	internal static class Patch_HasItems
	{
		private static void Postfix(ref bool __result, XUi _xui, Recipe _recipe, XUiC_RecipeCraftCount ___craftCountControl)
		{
			if (__result || RecipeLines.LeaveVanilla(_recipe))
			{
				return;
			}

			try
			{
				int count = ___craftCountControl != null ? ___craftCountControl.Count : 1;
				if (CraftBridge.CoversFromPool(_xui, _recipe, count))
				{
					__result = true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("has items", ex);
			}
		}
	}

	[HarmonyPatch(typeof(ItemActionEntryCraft), "OnActivated")]
	internal static class Patch_OnActivated
	{
		private static bool Prefix(ItemActionEntryCraft __instance, XUiC_RecipeCraftCount ___craftCountControl, int ___craftingTier)
		{
			try
			{
				return CraftBridge.HandleClick(__instance, ___craftCountControl, ___craftingTier);
			}
			catch (Exception ex)
			{
				Log.Error("craft click", ex);
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(XUiM_Recipes), nameof(XUiM_Recipes.HasIngredientsForRecipe))]
	internal static class Patch_HasIngredients
	{
		private static void Postfix(ref bool __result, IList<ItemStack> allItems, Recipe _recipe, EntityAlive _ea)
		{
			if (__result || RecipeLines.LeaveVanilla(_recipe))
			{
				return;
			}

			try
			{
				EntityPlayerLocal player = _ea as EntityPlayerLocal;
				XUi xui = player?.playerUI?.xui;
				if (xui == null || !CraftBridge.TryNearby(xui, out Dictionary<int, int> nearby))
				{
					return;
				}

				if (RecipeLines.CoveredFromPool(allItems, _recipe, player, nearby))
				{
					__result = true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("recipe list", ex);
			}
		}
	}

	[HarmonyPatch(typeof(XUiC_RecipeCraftCount), "calcMaxCraftable")]
	internal static class Patch_MaxCraft
	{
		private static void Postfix(XUiC_RecipeCraftCount __instance, ref int __result, Recipe ___recipe)
		{
			if (RecipeLines.LeaveVanilla(___recipe) || __instance?.xui == null)
			{
				return;
			}

			try
			{
				CraftBridge.RefreshPool(__instance.xui);
				if (!CraftBridge.TryNearby(__instance.xui, out Dictionary<int, int> nearby))
				{
					return;
				}

				int crafts = RecipeLines.MaxCrafts(__instance.xui, ___recipe, nearby);
				if (crafts > __result)
				{
					__result = crafts;
				}
			}
			catch (Exception ex)
			{
				Log.Error("craft count", ex);
			}
		}
	}

	[HarmonyPatch(typeof(XUiC_RecipeList), "Update")]
	internal static class Patch_RecipeList
	{
		private static readonly AccessTools.FieldRef<XUiC_RecipeList, bool> ResortRecipes =
			AccessTools.FieldRefAccess<XUiC_RecipeList, bool>("resortRecipes");

		private static readonly AccessTools.FieldRef<XUiC_RecipeList, bool> PageChanged =
			AccessTools.FieldRefAccess<XUiC_RecipeList, bool>("pageChanged");

		private static void Prefix(XUiC_RecipeList __instance)
		{
			if (__instance?.xui == null || __instance.windowGroup == null || !__instance.windowGroup.isShowing)
			{
				return;
			}

			try
			{
				bool numbers = CraftBridge.RefreshPool(__instance.xui);
				if (numbers && __instance.CraftCount != null)
				{
					__instance.CraftCount.IsDirty = true;
					__instance.CraftCount.CalculateMaxCount();
				}

				if (!CraftBridge.TakeListResort())
				{
					return;
				}

				__instance.IsDirty = true;
				ResortRecipes(__instance) = true;
				PageChanged(__instance) = true;
			}
			catch (Exception ex)
			{
				Log.Error("recipe list refresh", ex);
			}
		}
	}
}
