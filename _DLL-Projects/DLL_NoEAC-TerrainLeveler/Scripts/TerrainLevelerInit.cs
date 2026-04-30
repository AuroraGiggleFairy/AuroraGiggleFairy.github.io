using HarmonyLib;
using System;

public class TerrainLevelerInit : IModApi
{
    public void InitMod(Mod _modInstance)
    {
        // Actually instantiate the class, not just typeof, to force early loading
        var dummy = new AGFLevelerAction();
        dummy = null;
        var harmony = new Harmony("AGFProjects.TerrainLeveler");
        harmony.PatchAll();
    }
}
