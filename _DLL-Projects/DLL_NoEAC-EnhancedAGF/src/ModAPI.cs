using System;
using HarmonyLib;

public class ModAPI : IModApi
{
    public void InitMod(Mod modInstance)
    {
        try
        {
            Harmony harmony = new Harmony("com.agfprojects.enhancedagf");
            harmony.PatchAll();
            ScreamerAlertEnhancedBindingsPatch.TryInstall(harmony);
            ModEvents.PlayerSpawnedInWorld.RegisterHandler((ref ModEvents.SPlayerSpawnedInWorldData data) =>
            {
                if (!data.IsLocalPlayer)
                {
                    return;
                }

                ScreamerAlertEnhancedCapabilityHello.TrySendForLocalPlayerSpawn(data.EntityId);
            });
            ModEvents.GameUpdate.RegisterHandler((ref ModEvents.SGameUpdateData _) =>
            {
                ScreamerAlertEnhancedCapabilityHello.TickRetry();
            });
            Logging.Inform("EnhancedAGF Harmony patches registered.");
        }
        catch (Exception ex)
        {
            Logging.Error("EnhancedAGF failed to register Harmony patches: " + ex);
        }
    }
}
