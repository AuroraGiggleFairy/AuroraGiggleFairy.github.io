using System.Reflection;
using HarmonyLib;

namespace BiomePaints
{
    public class ModAPI : IModApi
    {
        public static Mod ModInstance { get; private set; }

        public void InitMod(Mod _modInstance)
        {
            ModInstance = _modInstance;
            Log.Out("[BiomePaints] Initializing Biome Paints Mod...");
            Harmony harmony = new Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Out("[BiomePaints] Patches applied successfully.");
        }
    }
}
