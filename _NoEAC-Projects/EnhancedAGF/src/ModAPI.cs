using System;
using HarmonyLib;

public class ModAPI : IModApi
{
    public void InitMod(Mod modInstance)
    {
        try
        {
            new Harmony("com.agfprojects.enhancedagf").PatchAll();
            Logging.Inform("EnhancedAGF Harmony patches registered.");
        }
        catch (Exception ex)
        {
            Logging.Error("EnhancedAGF failed to register Harmony patches: " + ex);
        }
    }
}
