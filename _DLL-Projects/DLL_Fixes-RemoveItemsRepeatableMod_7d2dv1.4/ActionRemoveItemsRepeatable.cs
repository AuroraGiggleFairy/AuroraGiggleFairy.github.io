using HarmonyLib;
using GameEvent.SequenceActions;

[HarmonyPatch(typeof(ActionRemoveItems), "HandleItemStackChange")]
class Patch_ActionRemoveItems_HandleItemStackChange
{
    static void Postfix(ActionRemoveItems __instance)
    {
        // Force repeatable: always set isFinished to false after method runs
        __instance.isFinished = false;
    }
}

[HarmonyPatch(typeof(ActionRemoveItems), "HandleItemValueChange")]
class Patch_ActionRemoveItems_HandleItemValueChange
{
    static void Postfix(ActionRemoveItems __instance)
    {
        // Force repeatable: always set isFinished to false after method runs
        __instance.isFinished = false;
    }
}
