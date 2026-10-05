using System;
using HarmonyLib;

public class ModAPI : IModApi
{
    public void InitMod(Mod modInstance)
    {
        try
        {
            DoomHudGate.Resolve();
            Harmony harmony = new Harmony("com.agfprojects.enhancedagf");
            harmony.PatchAll();
            if (DoomHudGate.IsLoaded)
            {
                StatControllers.Bindings.RegisterDoomHudBindings();
            }

            Patch_DoomToolbeltWindow_Update.TryInstall(harmony);
            ScreamerAlertEnhancedBindingsPatch.TryInstall(harmony);
            ModEvents.PlayerSpawnedInWorld.RegisterHandler((ref ModEvents.SPlayerSpawnedInWorldData data) =>
            {
                ExpandedInteractionPrompts.GrowthStagePrompt.OnPlayerSpawned(data.EntityId);
                if (!data.IsLocalPlayer)
                {
                    return;
                }

                ScreamerAlertEnhancedCapabilityHello.TrySendForLocalPlayerSpawn(data.EntityId);
            });
            ModEvents.GameUpdate.RegisterHandler((ref ModEvents.SGameUpdateData _) =>
            {
                ExpandedInteractionPrompts.GrowthStagePrompt.ServerTick();
                ScreamerAlertEnhancedCapabilityHello.TickRetry();
                if (ScreamerAlertEnhancedGate.IsScreamerInPlay())
                {
                    ScreamerAlertEnhancedState.Tick();
                }
                PlayerBindingInjectorPatches.Tick();
            });
            Logging.Inform("EnhancedAGF Harmony patches registered.");
            LogConnections();
        }
        catch (Exception ex)
        {
            Logging.Error("EnhancedAGF failed to register Harmony patches: " + ex);
        }
    }

    private static void LogConnections()
    {
        bool lockable = ExpandedInteractionPrompts.LockableStationsClient.IsAvailable;
        bool screamer = ScreamerAlertEnhancedGate.IsScreamerPresentLocally();
        string lockableText = lockable ? "linked" : "not on this PC";
        string screamerText = screamer ? "linked" : "not on this PC";

        if (GameManager.IsDedicatedServer)
        {
            Logging.Inform(
                "Connections (dedicated server): client HUD is off. Lockable Stations " + lockableText
                + ". Screamer Alert " + screamerText + ".");
            return;
        }

        Logging.Inform(
            "Connections (this PC): Lockable Stations " + lockableText
            + ". Screamer Alert " + screamerText
            + ". A joining client still follows Screamer Alert when the server has it.");
    }
}
