using System;
using HarmonyLib;

public class ModAPI : IModApi
{
    public void InitMod(Mod modInstance)
    {
        try
        {
            new Harmony("com.agfprojects.anticm").PatchAll();
            Console.WriteLine("AntiCM: Harmony patches registered.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("AntiCM: Patch registration error: " + ex);
        }
    }
}
