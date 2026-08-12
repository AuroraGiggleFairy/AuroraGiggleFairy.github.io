using System;
using System.Reflection;
using UnityEngine;

public static class ScreamerAlertEnhancedCapabilityHello
{
    private const string ProtocolCVar = ".agfSAProtocol";
    private const string HybridRoutingTypeName = "ScreamerAlertHybridRouting";
    private const string MarkCapabilityMethodName = "MarkClientCapability";
    private const float RetrySeconds = 3f;
    private const float HeartbeatSeconds = 20f;
    private static int _entityId = -1;
    private static float _nextSendAt = -1f;
    private static bool _serverDetected;
    private static bool _loggedSendFailure;
    private static bool _loggedTickFailure;
    private static bool _hybridRoutingResolved;
    private static MethodInfo _markClientCapability;

    public static void TrySendForLocalPlayerSpawn(int entityId)
    {
        _entityId = entityId;
        _serverDetected = false;
        _loggedSendFailure = false;
        ScreamerAlertEnhancedGate.ResetServerDetection();
        _nextSendAt = Time.realtimeSinceStartup;
    }

    public static void TrySendFromCommand(int entityId)
    {
        if (!ScreamerAlertEnhancedGate.ShouldRunCapabilityHandshake())
        {
            return;
        }

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
        try
        {
            if (!ScreamerAlertEnhancedGate.ShouldRunCapabilityHandshake())
            {
                return;
            }

            EntityPlayer player = GameManager.Instance?.World?.GetPrimaryPlayer();
            if (player == null || player.entityId < 0) return;
            _entityId = player.entityId;

            ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
            bool isLocalServer = manager != null && manager.IsServer && !GameManager.IsDedicatedServer;

            if (player.Buffs != null
                && player.Buffs.HasCustomVar(ProtocolCVar)
                && player.Buffs.GetCustomVar(ProtocolCVar) >= 2f)
            {
                // Leftover CVars from a removed ScreamerAlert install must not unlock the host path.
                if (!isLocalServer || ScreamerAlertEnhancedGate.IsScreamerPresentLocally())
                {
                    _serverDetected = true;
                    ScreamerAlertEnhancedGate.MarkServerScreamerDetected();
                }
            }
            else if (isLocalServer && ScreamerAlertEnhancedGate.IsScreamerPresentLocally())
            {
                // SP / listen-host: ScreamerAlert is local. Do not wait for Protocol CVar
                // published through Clients.List (local host is often absent from that list).
                _serverDetected = true;
                ScreamerAlertEnhancedGate.MarkServerScreamerDetected();
            }

            if (_serverDetected && Time.realtimeSinceStartup >= _nextSendAt)
            {
                TrySendHello();
            }
        }
        catch (Exception ex)
        {
            if (_loggedTickFailure)
            {
                return;
            }

            _loggedTickFailure = true;
            Logging.Warning("ScreamerAlertEnhancedCapabilityHello", "Capability handshake tick failed: " + ex.Message);
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
            if (_loggedSendFailure)
            {
                return;
            }

            _loggedSendFailure = true;
            Logging.Warning("ScreamerAlertEnhancedCapabilityHello", "Failed to send vanilla capability hello: " + ex.Message);
        }
    }

    private static void MarkLocalHostCapability()
    {
        if (GameManager.IsDedicatedServer) return;
        if (!ScreamerAlertEnhancedGate.IsScreamerPresentLocally()) return;

        MethodInfo method = ResolveMarkClientCapability();
        method?.Invoke(null, new object[] { _entityId, string.Empty });
        MarkAcknowledged();
    }

    private static MethodInfo ResolveMarkClientCapability()
    {
        if (_hybridRoutingResolved)
        {
            return _markClientCapability;
        }

        _hybridRoutingResolved = true;
        Type type = ScreamerAlertEnhancedGate.FindLoadedType(HybridRoutingTypeName);
        _markClientCapability = type?.GetMethod(MarkCapabilityMethodName, BindingFlags.Public | BindingFlags.Static);
        return _markClientCapability;
    }
}
