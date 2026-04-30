
using HarmonyLib;
using System;

namespace ConsoleOpacityMod
{
    public class ModAPI : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            try
            {
                ConsoleOpacityConfig.LoadConfig();
                var harmony = new Harmony("console.opacity.mod");
                harmony.PatchAll();
                ConsoleOpacityConfig.Log("IModApi: Harmony patch applied and config loaded.");
            }
            catch (Exception ex)
            {
                ConsoleOpacityConfig.Log($"IModApi init error: {ex.Message}");
            }
        }
    }
}
