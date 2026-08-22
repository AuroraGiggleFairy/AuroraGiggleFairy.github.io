using HarmonyLib;

namespace DoomInfighting;

[HarmonyPatch(typeof(EAISetAsTargetIfHurt), "CanExecute")]
public static class ThresholdGate
{
	[HarmonyPriority(800)]
	public static bool Prefix(EAISetAsTargetIfHurt __instance, ref bool __result)
	{
		EntityAlive entity = __instance.theEntity;
		if (!Demons.Is(entity) || Demons.AlwaysRevenge(entity) || !Threshold.Running(entity))
		{
			return true;
		}
		__result = false;
		return false;
	}
}
