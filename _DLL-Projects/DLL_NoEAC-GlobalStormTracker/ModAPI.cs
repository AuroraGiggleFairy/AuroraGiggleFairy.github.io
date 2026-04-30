using HarmonyLib;
using System;

namespace StormTracker
{
    public class ModAPI : IModApi
    {


        public void InitMod(Mod modInstance)
        {
            var harmony = new Harmony("com.agfprojects.stormtracker");
            harmony.PatchAll();
        }
    }
}
