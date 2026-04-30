using HarmonyLib;
using System;

namespace ItemTypeIconColor
{
    public class ModAPI : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            try
            {
                var harmony = new Harmony("com.agfprojects.itemtypeiconcolor");
                harmony.PatchAll();
            }
            catch (Exception ex)
            {
                // ...log removed...
            }
        }
    }
}