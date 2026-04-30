using HarmonyLib;

namespace NoEACVisualEntityTracker
{
    [HarmonyPatch(typeof(XUiC_CompassWindow), nameof(XUiC_CompassWindow.Update))]
    public static class CompassWindowUpdatePatch
    {
        public static void Postfix()
        {
            VisualEntityTrackerService.Tick();
        }
    }
}
