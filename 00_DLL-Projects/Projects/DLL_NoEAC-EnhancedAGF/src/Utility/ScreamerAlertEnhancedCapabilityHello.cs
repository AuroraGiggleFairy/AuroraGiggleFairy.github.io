using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

public static class ScreamerAlertEnhancedCapabilityHello
{
    private const string ProtocolCVar = ".agfSAProtocol";
    private const float RetrySeconds = 3f;
    private const float HeartbeatSeconds = 20f;
    private static int _entityId = -1;
    private static float _nextSendAt = -1f;
    private static bool _serverDetected;

    public static void TrySendForLocalPlayerSpawn(int entityId)
    {
        _entityId = entityId;
        _serverDetected = false;
        _nextSendAt = Time.realtimeSinceStartup;
    }

    public static void TrySendFromCommand(int entityId)
    {
        if (entityId >= 0) _entityId = entityId;
        TrySendHello();
    }

    public static void TrySendFromProbe(int entityId, int nonce)
    {
        _ = nonce;
        TrySendFromCommand(entityId);
    }

    public static void TickRetry()
    {
        EntityPlayer player = GameManager.Instance?.World?.GetPrimaryPlayer();
        if (player == null || player.entityId < 0) return;
        _entityId = player.entityId;

        if (player.Buffs != null
            && player.Buffs.HasCustomVar(ProtocolCVar)
            && player.Buffs.GetCustomVar(ProtocolCVar) >= 2f)
        {
            _serverDetected = true;
            ScreamerAlertEnhancedGate.MarkServerScreamerDetected();
        }

        if (_serverDetected && Time.realtimeSinceStartup >= _nextSendAt)
        {
            TrySendHello();
        }
    }

    public static void MarkAcknowledged()
    {
        _serverDetected = true;
        _nextSendAt = Time.realtimeSinceStartup + HeartbeatSeconds;
    }

    private static void TrySendHello()
    {
        try
        {
            ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
            if (manager == null || _entityId < 0) return;
            if (manager.IsServer)
            {
                MarkLocalHostCapability();
                return;
            }
            if (!_serverDetected)
            {
                _nextSendAt = Time.realtimeSinceStartup + RetrySeconds;
                return;
            }

            NetPackageChat package = NetPackageManager.GetPackage<NetPackageChat>();
            if (package != null)
            {
                manager.SendToServer(package.Setup(
                    EChatType.Global,
                    _entityId,
                    "/agfsa proto hello 2",
                    null,
                    EMessageSender.SenderIdAsPlayer,
                    GeneratedTextManager.BbCodeSupportMode.Supported));
            }
            _nextSendAt = Time.realtimeSinceStartup + HeartbeatSeconds;
        }
        catch (Exception ex)
        {
            _nextSendAt = Time.realtimeSinceStartup + RetrySeconds;
            Logging.Warning("ScreamerAlertEnhancedCapabilityHello", "Failed to send vanilla capability hello: " + ex.Message);
        }
    }

    private static void MarkLocalHostCapability()
    {
        if (GameManager.IsDedicatedServer) return;
        Type type = AccessTools.TypeByName("ScreamerAlertHybridRouting");
        MethodInfo method = type?.GetMethod("MarkClientCapability", BindingFlags.Public | BindingFlags.Static);
        method?.Invoke(null, new object[] { _entityId, string.Empty });
        MarkAcknowledged();
    }
}