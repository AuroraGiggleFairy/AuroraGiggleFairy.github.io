using HarmonyLib;
using System;
using GameEvent.SequenceActions;

namespace AGFProjects.RemoveItemsRepeatableMod
{
    public class ModAPI : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            var harmony = new Harmony("com.agfprojects.removeitemsrepeatable");
            harmony.PatchAll();
            UnityEngine.Debug.Log("RemoveItemsRepeatableMod loaded successfully.");
        }
    }
}
