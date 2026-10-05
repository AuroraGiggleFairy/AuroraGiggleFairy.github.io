using System;
using HarmonyLib;

namespace LootTimerHolds
{
    public class ModAPI : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            try
            {
                GameVersion.Initialize();
                new Harmony("com.agfprojects.loottimerholds").PatchAll();
                Console.WriteLine("LootTimerHolds: Harmony patches registered.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("LootTimerHolds: Patch registration error: " + ex);
            }
        }
    }
}
