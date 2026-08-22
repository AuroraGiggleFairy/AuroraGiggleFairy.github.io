using HarmonyLib;

namespace DisintegrationPatch;

[HarmonyPatch(typeof(EntityAlive), "Disintegrate")]
public static class KeepBody
{
	[HarmonyPriority(800)]
	public static bool Prefix(EntityAlive __instance)
	{
		__instance.canDisintegrate = false;
		return false;
	}
}
