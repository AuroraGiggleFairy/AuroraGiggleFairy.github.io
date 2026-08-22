using HarmonyLib;

namespace CombatGlitchMitigations
{
	[HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.OnUpdateEntity))]
	public static class Patch_EntityAlive_OnUpdateEntity
	{
		public static void Postfix(EntityAlive __instance)
		{
			try
			{
				if (!CombatGlitchMitigationsLogic.IsServerContext() || __instance is EntityPlayer)
				{
					return;
				}

				CombatGlitchMitigationsLogic.TickEntity(__instance);
			}
			catch
			{
			}
		}
	}
}
