using HarmonyLib;

namespace DoomInfighting;

[HarmonyPatch(typeof(EntityAlive), "DamageEntity")]
public static class SpeciesRules
{
	[HarmonyPriority(800)]
	public static bool Prefix(EntityAlive __instance, DamageSource _damageSource, ref int __result)
	{
		if (_damageSource == null || !Demons.Is(__instance))
		{
			return true;
		}
		if (SplashScope.Active && Demons.SplashImmune(__instance))
		{
			__result = -1;
			return false;
		}
		World world = ((GameManager.Instance == null) ? null : GameManager.Instance.World);
		if (world == null)
		{
			return true;
		}
		EntityAlive attacker = world.GetEntity(_damageSource.getEntityId()) as EntityAlive;
		if (attacker == null || attacker == __instance || !Demons.Is(attacker))
		{
			return true;
		}
		if (Demons.SameSpecies(attacker, __instance) && !Demons.NotMissile(attacker))
		{
			__result = -1;
			return false;
		}
		return true;
	}
}
