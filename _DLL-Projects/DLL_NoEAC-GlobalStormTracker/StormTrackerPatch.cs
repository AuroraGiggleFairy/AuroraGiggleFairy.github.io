using HarmonyLib;
using UnityEngine;

namespace StormTracker
{
    [HarmonyPatch(typeof(GameManager), "Awake")]
    public class StormTrackerPatch
    {
        static void Postfix()
        {
            Debug.Log("[StormTracker] GameManager.Awake patched.");
            // XUiController.Register does not exist in vanilla; controller will be resolved by class name if not registered.
        }
    }
}
