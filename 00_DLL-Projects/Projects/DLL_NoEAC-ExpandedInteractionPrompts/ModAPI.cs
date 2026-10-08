using System;
using HarmonyLib;

namespace ExpandedInteractionPrompts
{
    public class ModAPI : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            try
            {
                if (ModManager.GetMod("AGF-EnhancedAGF") != null)
                {
                    Console.WriteLine("[ExpandedInteractionPrompts] EnhancedAGF is loaded; skipping HUD patches.");
                    return;
                }

                new Harmony("com.agfprojects.expandedinteractionprompts").PatchAll();
                Console.WriteLine("[ExpandedInteractionPrompts] Harmony patches registered.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ExpandedInteractionPrompts] Patch registration error: " + ex);
            }
        }
    }
}
