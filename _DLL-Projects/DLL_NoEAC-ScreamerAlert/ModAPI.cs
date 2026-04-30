
using System;
using HarmonyLib;
using Platform;
using UnityEngine;

namespace ScreamerAlert
{
public class ModAPI : IModApi
{
    public void InitMod(Mod modInstance)
    {
        new Harmony("com.agfprojects.screameralert").PatchAll();
        ModEvents.PlayerSpawnedInWorld.RegisterHandler((ref ModEvents.SPlayerSpawnedInWorldData data) =>
        {
            ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
            if (manager == null)
            {
                return;
            }

            if (data.IsLocalPlayer && !manager.IsServer)
            {
                string userCombined = PlatformManager.InternalLocalUserIdentifier?.CombinedString ?? string.Empty;
                manager.SendToServer(NetPackageManager.GetPackage<NetPackageScreamerAlertClientHello>().Setup(data.EntityId, userCombined));
            }
        });
        ModEvents.PlayerDisconnected.RegisterHandler((ref ModEvents.SPlayerDisconnectedData data) =>
        {
            int entityId = data.ClientInfo?.entityId ?? -1;
            if (entityId >= 0)
            {
                ScreamerAlertHybridRouting.ForgetClientByEntityId(entityId);
            }
        });
        ModEvents.ChatMessage.RegisterHandler((ref ModEvents.SChatMessageData data) =>
        {
            return ChatCmdScreamerAlert.OnChatMessage(ref data);
        });
        try
        {
            GameObject gameObject = GameObject.Find("ScreamerAlertManager") ?? new GameObject("ScreamerAlertManager");
            if (gameObject.GetComponent<ScreamerAlertManager>() == null)
            {
                gameObject.AddComponent<ScreamerAlertManager>();
            }
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            GameObject gameObject2 = GameObject.Find("ScreamerAlertsController");
            if (gameObject2 == null)
            {
                gameObject2 = new GameObject("ScreamerAlertsController");
                gameObject2.AddComponent<ScreamerAlertsController>();
            }
            else if (gameObject2.GetComponent<ScreamerAlertsController>() == null)
            {
                gameObject2.AddComponent<ScreamerAlertsController>();
            }
            UnityEngine.Object.DontDestroyOnLoad(gameObject2);
            _ = gameObject2.GetComponent<ScreamerAlertsController>() != null;
        }
        catch (Exception)
        {
        }
    }
}
}
