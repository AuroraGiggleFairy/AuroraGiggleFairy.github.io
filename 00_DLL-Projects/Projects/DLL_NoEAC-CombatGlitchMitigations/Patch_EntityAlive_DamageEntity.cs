using HarmonyLib;

namespace CombatGlitchMitigations
{
	[HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.DamageEntity))]
	public static class Patch_EntityAlive_DamageEntity
	{
		public static bool Prefix(EntityAlive __instance, DamageSource _damageSource, ref int __result)
		{
			try
			{
				if (!CombatGlitchMitigationsLogic.IsServerContext())
				{
					return true;
				}

				if (!CombatGlitchMitigationsLogic.IsZombieMeleeAgainstPlayer(__instance, _damageSource, out EntityAlive attacker))
				{
					return true;
				}

				if (!CombatGlitchMitigationsLogic.ShouldCancelZombiePunch(__instance, attacker, out BarrierKind barrier, out string reason))
				{
					return true;
				}

				CombatGlitchMitigationsLogic.ScheduleSnap(attacker, __instance, barrier);
				CombatGlitchMitigationsLogic.Log(
					"blocked punch from " + attacker.EntityName + " #" + attacker.entityId + " (" + reason + ")");
				CombatGlitchMitigationsLogic.TryDebugLine(__instance, attacker.position, new UnityEngine.Color(1f, 0.2f, 0.2f));
				__result = -1;
				return false;
			}
			catch
			{
				return true;
			}
		}
	}
}
