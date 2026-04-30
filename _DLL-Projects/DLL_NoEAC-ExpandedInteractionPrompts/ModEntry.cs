using HarmonyLib;

namespace ExpandedInteractionPrompts
{
    public class ModEntry
    {
        public static void Init()
        {
            var harmony = new Harmony("com.yourname.ExpandedInteractionPrompts");
            harmony.PatchAll();
        }
    }
}
