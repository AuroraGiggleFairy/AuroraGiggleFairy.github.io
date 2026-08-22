using HarmonyLib;

namespace DoomInfighting;

[HarmonyPatch(typeof(EAISetAsTargetIfHurt), "Start")]
public static class ThresholdStamp
{
	public static void Postfix(EAISetAsTargetIfHurt __instance)
	{
		Threshold.Set(__instance.theEntity);
	}
}
