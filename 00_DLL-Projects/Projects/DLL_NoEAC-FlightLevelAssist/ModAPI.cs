using System;
using HarmonyLib;

namespace FlightLevelAssist
{
    public class ModAPI : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            try
            {
                FlightLevelAssistConfig.Load();
                new Harmony("com.agfprojects.flightlevelassist").PatchAll();
                UnityEngine.Debug.Log("[FlightLevelAssist] Harmony patches registered.");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[FlightLevelAssist] Patch registration failed: " + ex);
            }
        }
    }
}
