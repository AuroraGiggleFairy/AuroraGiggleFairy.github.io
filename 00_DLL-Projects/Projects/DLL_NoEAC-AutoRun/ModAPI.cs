using System;
using HarmonyLib;
using UnityEngine;

namespace AutoRun
{
    public class ModAPI : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            try
            {
                new Harmony("com.agfprojects.autorun").PatchAll();
                Debug.Log("[AutoRun] Harmony patches registered.");
            }
            catch (Exception ex)
            {
                Debug.LogError("[AutoRun] Patch registration failed: " + ex);
            }
        }
    }
}
