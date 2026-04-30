using HarmonyLib;
using System;

namespace AGFProjects.DestroyBiomeBadgeFix
{
    public class ModAPI : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            var harmony = new Harmony("com.agfprojects.destroybiomebadgefix");
            harmony.PatchAll();
            UnityEngine.Debug.Log("DestroyBiomeBadgeFix loaded successfully.");
        }
    }
}
