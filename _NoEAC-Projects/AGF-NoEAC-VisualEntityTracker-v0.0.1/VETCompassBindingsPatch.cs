using HarmonyLib;

namespace NoEACVisualEntityTracker
{
    [HarmonyPatch]
    public static class VETCompassBindingsPatch
    {
        public static System.Reflection.MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(XUiC_CompassWindow), "GetBindingValueInternal");
        }

        public static bool Prefix(ref string value, string bindingName, ref bool __result)
        {
            if (string.IsNullOrEmpty(bindingName))
            {
                return true;
            }

            if (bindingName == "agf_vet_edge_left_visible" || bindingName == "agf_vet_edge_left")
            {
                value = VETCompassClampPatch.EdgeLeftVisible ? "true" : "false";
                __result = true;
                return false;
            }

            if (bindingName == "agf_vet_edge_right_visible" || bindingName == "agf_vet_edge_right")
            {
                value = VETCompassClampPatch.EdgeRightVisible ? "true" : "false";
                __result = true;
                return false;
            }

            return true;
        }
    }
}
