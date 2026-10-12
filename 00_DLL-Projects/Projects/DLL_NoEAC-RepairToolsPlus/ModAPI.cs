using System.Reflection;
using HarmonyLib;

namespace RepairToolsPlus
{
    public class ModAPI : IModApi
    {
        public static Mod ModInstance { get; private set; }

        public void InitMod(Mod _modInstance)
        {
            ModInstance = _modInstance;
            Log.Out("[RepairToolsPlus] Initializing Repair Tools Plus Mod...");

            Harmony harmony = new Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            // Initialize the wireframe manager singleton GameObject
            RepairHighlightManager.Init();

            Log.Out("[RepairToolsPlus] Initialized successfully. Wireframe manager active.");
        }
    }
}
