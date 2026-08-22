using HarmonyLib;

namespace CombatGlitchMitigations
{
	static class PlayerTargetRewrite
	{
		public static void AfterTarget(ItemAction __instance, ItemActionData _actionData, WorldRayHitInfo __result)
		{
			try
			{
				if (!CombatGlitchMitigationsLogic.IsServerContext() || _actionData?.invData == null || __result == null)
				{
					return;
				}

				EntityAlive holder = _actionData.invData.holdingEntity;
				bool melee = __instance is ItemActionMelee || __instance is ItemActionDynamic;
				CombatGlitchMitigationsLogic.TryRewritePlayerTarget(
					holder,
					__instance,
					__result,
					allowPlantSkip: melee,
					allowGetUp: true);
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(ItemActionMelee), nameof(ItemActionMelee.GetExecuteActionTarget))]
	public static class Patch_ItemActionMelee_GetExecuteActionTarget
	{
		public static void Postfix(ItemActionMelee __instance, ItemActionData _actionData, WorldRayHitInfo __result)
		{
			PlayerTargetRewrite.AfterTarget(__instance, _actionData, __result);
		}
	}

	[HarmonyPatch(typeof(ItemActionDynamic), nameof(ItemActionDynamic.GetExecuteActionTarget))]
	public static class Patch_ItemActionDynamic_GetExecuteActionTarget
	{
		public static void Postfix(ItemActionDynamic __instance, ItemActionData _actionData, WorldRayHitInfo __result)
		{
			PlayerTargetRewrite.AfterTarget(__instance, _actionData, __result);
		}
	}

	[HarmonyPatch(typeof(ItemActionRanged), nameof(ItemActionRanged.GetExecuteActionTarget))]
	public static class Patch_ItemActionRanged_GetExecuteActionTarget
	{
		public static void Postfix(ItemActionRanged __instance, ItemActionData _actionData, WorldRayHitInfo __result)
		{
			PlayerTargetRewrite.AfterTarget(__instance, _actionData, __result);
		}
	}
}
