using System;
using HarmonyLib;
using UnityEngine;

namespace SignErrorGuard
{
    public class ModAPI : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            try
            {
                new Harmony("com.agfprojects.signerrorguard").PatchAll();
                Debug.Log("[SignErrorGuard] Harmony patches registered.");
            }
            catch (Exception ex)
            {
                Debug.LogError("[SignErrorGuard] Patch registration failed: " + ex);
            }
        }
    }
}
