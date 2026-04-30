using System;
using System.Reflection;
using HarmonyLib;

[HarmonyPatch]
internal static class VisualEntityTrackerOptionPatch
{
    private static bool wasDisabled;

    static MethodBase TargetMethod()
    {
        Type serviceType = AccessTools.TypeByName("NoEACVisualEntityTracker.VisualEntityTrackerService");
        return serviceType == null ? null : AccessTools.Method(serviceType, "Tick");
    }

    static bool Prefix()
    {
        if (!OptionsRegistry.IsVisualEntityTrackerPresent())
        {
            return true;
        }

        bool enabled = OptionsRegistry.GetVisualEntityTrackerModeCached(OptionMode.On) != OptionMode.Off;
        if (enabled)
        {
            wasDisabled = false;
            return true;
        }

        if (!wasDisabled)
        {
            TryCleanupVisualEntityTracker();
        }

        wasDisabled = true;
        return false;
    }

    private static void TryCleanupVisualEntityTracker()
    {
        try
        {
            Type serviceType = AccessTools.TypeByName("NoEACVisualEntityTracker.VisualEntityTrackerService");
            MethodInfo cleanup = AccessTools.Method(serviceType, "CleanupAllDllNavObjects");
            cleanup?.Invoke(null, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[optionsAGF] Failed to cleanup Visual Entity Tracker while disabled: " + ex.Message);
        }
    }
}
