using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(XUiC_WorkstationMaterialInputGrid), nameof(XUiC_WorkstationMaterialInputGrid.Update))]
public static class Patch_WorkstationMaterialInputGrid_Update_SmeltTimerTotal
{
	public static void Postfix(XUiC_WorkstationMaterialInputGrid __instance)
	{
		if (!SmeltTimerModeSettings.IsTotal)
		{
			return;
		}

		if (__instance?.WorkstationData?.TileEntity == null || __instance.itemControllers == null)
		{
			return;
		}

		TileEntityWorkstation te = __instance.WorkstationData.TileEntity;
		ItemStack[] input = te.Input;
		if (input == null)
		{
			return;
		}

		ItemStack[] tools = te.Tools;
		// isModuleUsed[0] = tools module (bellows / CraftingSmeltTime).
		bool toolsModuleUsed = te.isModuleUsed != null && te.isModuleUsed.Length > 0 && te.isModuleUsed[0];

		int limit = Mathf.Min(input.Length, __instance.itemControllers.Length);
		// Primary smelting input slots are before the internal unit material bins.
		int materialBinCount = te.MaterialNames?.Length ?? 0;
		int primarySlots = Mathf.Max(0, input.Length - materialBinCount);
		limit = Mathf.Min(limit, primarySlots);

		for (int i = 0; i < limit; i++)
		{
			XUiC_ItemStack stackUi = __instance.itemControllers[i];
			if (stackUi?.timer == null)
			{
				continue;
			}

			ItemStack stack = input[i];
			if (stack == null || stack.IsEmpty() || stack.count <= 0)
			{
				stackUi.timer.IsVisible = false;
				continue;
			}

			ItemClass itemClass = stack.itemValue?.ItemClass;
			float perItem = SmeltTimerCalculator.GetPerItemMeltSeconds(itemClass, tools, toolsModuleUsed);
			float remaining = te.GetTimerForSlot(i);
			float total = SmeltTimerCalculator.GetTotalMeltSeconds(remaining, stack.count, perItem);
			if (total <= 0f)
			{
				stackUi.timer.IsVisible = false;
				continue;
			}

			stackUi.timer.IsVisible = true;
			stackUi.timer.Text = SmeltTimerCalculator.FormatTimer(total);
		}
	}
}
