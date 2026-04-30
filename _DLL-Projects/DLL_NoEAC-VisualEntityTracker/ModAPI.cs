using System;
using HarmonyLib;

namespace NoEACVisualEntityTracker
{
    public class ModAPI : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            try
            {
                var harmony = new Harmony("agf.noeac.visualentitytracker");
                harmony.PatchAll();
                ModEvents.ChatMessage.RegisterHandler((ref ModEvents.SChatMessageData data) =>
                {
                    return ChatCmdVisualEntityTracker.OnChatMessage(ref data);
                });
                ModEvents.PlayerSpawnedInWorld.RegisterHandler((ref ModEvents.SPlayerSpawnedInWorldData data) =>
                {
                    if (data.IsLocalPlayer)
                    {
                        VisualEntityTrackerService.OnLocalPlayerSpawned();
                    }
                });
                Console.WriteLine("[NoEACVisualEntityTracker] Initialized. Harmony patches applied.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[NoEACVisualEntityTracker] Failed to initialize: " + ex);
            }
        }
    }
}
