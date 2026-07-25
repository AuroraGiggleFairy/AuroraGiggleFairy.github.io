using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace Harmony;

[HarmonyPatch(typeof(XUiC_RecipeList))]
[HarmonyPatch("GetRecipeData")]
public class GetRecipeData
{
	public static void Postfix(XUiC_RecipeList __instance)
	{
		if (!(__instance.Workstation == "miningMachine"))
		{
			return;
		}
		XUiC_WorkstationWindowGroup childByType = __instance.windowGroup.Controller.GetChildByType<XUiC_WorkstationWindowGroup>();
		if (childByType == null)
		{
			return;
		}
		TileEntityWorkstation _te = childByType.WorkstationData.TileEntity;
		if (_te == null)
		{
			return;
		}
		List<Recipe> recipes = new List<Recipe>();
		string blockName = ((WorldBase)GameManager.Instance.World).GetBlock(((TileEntity)_te).GetClrIdx(), _te.ToWorldPos() + ModUtil.getOffset(_te.blockValue)).Block.GetBlockName();
		if (_te.blockValue.Block.Properties.Contains("MiningData." + blockName))
		{
			List<string> itemNames = _te.blockValue.Block.Properties.GetString("MiningData." + blockName).Split(',').ToList();
			foreach (Recipe recipe in __instance.recipes)
			{
				if (itemNames.Contains(recipe.GetOutputItemClass().GetItemName()))
				{
					recipes.Add(recipe);
				}
			}
		}
		__instance.recipes = recipes;
		__instance.Page = 0;
		__instance.IsDirty = true;
		__instance.resortRecipes = true;
		__instance.pageChanged = true;
	}
}
