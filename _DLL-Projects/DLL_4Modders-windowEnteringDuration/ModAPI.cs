using HarmonyLib;
using System;

namespace AGFProjects.windowEnteringDuration
{
    public class ModAPI : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            Config.Load();
            var harmony = new Harmony("com.agfprojects.windowenteringduration");
            harmony.PatchAll();
        }
    }
}
