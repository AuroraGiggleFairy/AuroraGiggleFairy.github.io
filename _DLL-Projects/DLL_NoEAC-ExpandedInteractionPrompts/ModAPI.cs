using HarmonyLib;

namespace ExpandedInteractionPrompts
{
    public class ModAPI : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            var harmony = new Harmony("com.agfprojects.expandedinteractionprompts");
            harmony.PatchAll();
        }


    }
}
