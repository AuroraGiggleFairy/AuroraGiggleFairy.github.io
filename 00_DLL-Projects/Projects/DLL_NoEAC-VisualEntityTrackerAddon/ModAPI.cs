using System;
using HarmonyLib;
using UnityEngine;

namespace VisualEntityTrackerAddon
{
    public class ModAPI : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            try
            {
                new Harmony("agf.noeac.visualentitytrackeraddon").PatchAll();
                ModEvents.ChatMessage.RegisterHandler((ref ModEvents.SChatMessageData data) =>
                {
                    return ChatCmdVisualEntityTrackerAddon.OnChatMessage(ref data);
                });
                ModEvents.PlayerSpawnedInWorld.RegisterHandler((ref ModEvents.SPlayerSpawnedInWorldData data) =>
                {
                    int entityId = data.EntityId;
                    if (entityId < 0)
                    {
                        entityId = data.ClientInfo != null ? data.ClientInfo.entityId : -1;
                    }

                    if (entityId >= 0)
                    {
                        VisualEntityTrackerAddonManager.ScheduleApply(entityId);
                    }
                });

                GameObject gameObject = GameObject.Find("VisualEntityTrackerAddonManager")
                    ?? new GameObject("VisualEntityTrackerAddonManager");
                if (gameObject.GetComponent<VisualEntityTrackerAddonManager>() == null)
                {
                    gameObject.AddComponent<VisualEntityTrackerAddonManager>();
                }

                UnityEngine.Object.DontDestroyOnLoad(gameObject);
                Console.WriteLine("[VisualEntityTracker] Initialized.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[VisualEntityTracker] Failed to initialize: " + ex);
            }
        }
    }
}
