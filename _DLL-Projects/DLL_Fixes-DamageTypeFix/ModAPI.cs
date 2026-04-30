using System;
using HarmonyLib;
using UnityEngine;

namespace DamageTypeFix
{
    public class ModAPI : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            try
            {
                new Harmony("com.agfprojects.damagetypefix").PatchAll();
                Debug.Log("[DamageTypeFix] Harmony patches registered.");
            }
            catch (Exception ex)
            {
                Debug.LogError("[DamageTypeFix] Patch registration failed: " + ex);
            }
        }
    }
}
