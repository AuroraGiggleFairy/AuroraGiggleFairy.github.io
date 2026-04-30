using System;
using HarmonyLib;

namespace NoEACVisualEntityTracker
{
    public class ModAPI : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            try
            {
                var harmony = new Harmony("agf.noeac.visualentitytracker");
                harmony.PatchAll();
                Console.WriteLine("[NoEACVisualEntityTracker] Initialized. Harmony patches applied.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[NoEACVisualEntityTracker] Failed to initialize: " + ex);
            }
        }
    }
}
