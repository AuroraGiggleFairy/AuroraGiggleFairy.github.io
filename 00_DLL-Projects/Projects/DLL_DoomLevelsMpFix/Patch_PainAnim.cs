using System;
using HarmonyLib;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// Doom enemies play a pain flinch when a hit crosses the pain threshold.
	/// Clear that flag for those enemies only. Animals and the player keep it.
	/// Damage is unchanged.
	/// </summary>
	internal static class Patch_PainAnim
	{
		private static bool SkipPain(EntityAlive entity)
		{
			if (entity == null)
			{
				return false;
			}

			EntityClass entityClass = EntityClass.list[entity.entityClass];
			string name = entityClass != null ? entityClass.entityClassName : null;
			return name != null && name.StartsWith("demon", StringComparison.Ordinal);
		}

		[HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.ProcessDamageResponseLocal))]
		private static class Local
		{
			private static void Prefix(EntityAlive __instance, ref DamageResponse _dmResponse)
			{
				if (SkipPain(__instance))
				{
					_dmResponse.PainHit = false;
				}
			}
		}

		[HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.damageEntityLocal))]
		private static class Returned
		{
			private static void Postfix(EntityAlive __instance, ref DamageResponse __result)
			{
				if (SkipPain(__instance))
				{
					__result.PainHit = false;
				}
			}
		}
	}
}
