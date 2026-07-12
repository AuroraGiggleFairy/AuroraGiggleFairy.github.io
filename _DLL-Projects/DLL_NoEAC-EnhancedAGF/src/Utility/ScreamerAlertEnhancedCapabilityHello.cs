using System;
using System.Reflection;
using HarmonyLib;
using Platform;
using UnityEngine;

public static class ScreamerAlertEnhancedCapabilityHello
{
    private const int MaxHelloAttempts = 3;
    private const float FirstRetryDelaySeconds = 3f;
    private const float SecondRetryDelaySeconds = 10f;
    private const float ProbeCooldownSeconds = 0.2f;

    private static bool _resolved;
    private static bool _acknowledged;
    private static int _attemptCount;
    private static int _lastEntityId = -1;
    private static float _nextRetryAtRealtime = -1f;

    private static Type _helloPackageType;
    private static MethodInfo _getPackageGenericMethod;
    private static MethodInfo _setupMethod;
    private static int _lastProbeNonce = int.MinValue;
    private static float _lastProbeAtRealtime = -1f;

    public static void TrySendForLocalPlayerSpawn(int entityId)
    {
        _lastEntityId = entityId;
        _acknowledged = false;
        _attemptCount = 0;
        _nextRetryAtRealtime = -1f;
        TrySendHello(entityId);
    }

    public static void TrySendFromCommand(int entityId)
    {
        if (entityId >= 0)
        {
            _lastEntityId = entityId;
        }

        TrySendHello(_lastEntityId);
    }

    public static void TrySendFromProbe(int entityId, int nonce)
    {
        float now = Time.realtimeSinceStartup;
        if (nonce == _lastProbeNonce && _lastProbeAtRealtime >= 0f && (now - _lastProbeAtRealtime) < ProbeCooldownSeconds)
        {
            return;
        }

        _lastProbeNonce = nonce;
        _lastProbeAtRealtime = now;

        if (entityId >= 0)
        {
            _lastEntityId = entityId;
        }

        // Probe flow requires a fresh hello even if prior ack state was true.
        _acknowledged = false;
        _attemptCount = 0;
        _nextRetryAtRealtime = -1f;
        TrySendHello(_lastEntityId);
    }

    public static void TickRetry()
    {
        if (_acknowledged)
        {
            return;
        }

        if (_attemptCount >= MaxHelloAttempts)
        {
            return;
        }

        if (_nextRetryAtRealtime < 0f)
        {
            return;
        }

        if (Time.realtimeSinceStartup < _nextRetryAtRealtime)
        {
            return;
        }

        TrySendHello(_lastEntityId);
    }

    public static void MarkAcknowledged()
    {
        _acknowledged = true;
        _nextRetryAtRealtime = -1f;
    }

    private static void TrySendHello(int entityId)
    {
        try
        {
            if (_acknowledged)
            {
                return;
            }

            if (_attemptCount >= MaxHelloAttempts)
            {
                return;
            }

            if (entityId < 0)
            {
                return;
            }

            ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
            if (manager == null)
            {
                return;
            }

            if (manager.IsServer)
            {
                TryMarkCapabilityForLocalHost(entityId);
                return;
            }

            if (!ScreamerAlertEnhancedGate.IsScreamerPresentLocally())
            {
                return;
            }

            if (!ResolveReflection())
            {
                return;
            }

            object packageObject = _getPackageGenericMethod.MakeGenericMethod(_helloPackageType).Invoke(null, null);
            if (!(packageObject is NetPackage package))
            {
                return;
            }

            string userCombined = PlatformManager.InternalLocalUserIdentifier?.CombinedString ?? string.Empty;
            _setupMethod.Invoke(packageObject, new object[] { entityId, userCombined });
            manager.SendToServer(package);

            _attemptCount++;
            if (_attemptCount >= MaxHelloAttempts)
            {
                _nextRetryAtRealtime = -1f;
            }
            else
            {
                float delay = _attemptCount == 1 ? FirstRetryDelaySeconds : SecondRetryDelaySeconds;
                _nextRetryAtRealtime = Time.realtimeSinceStartup + delay;
            }
        }
        catch (Exception ex)
        {
            Logging.Warning("ScreamerAlertEnhancedCapabilityHello", "Failed to send capability hello: " + ex.Message);
        }
    }

    private static void TryMarkCapabilityForLocalHost(int entityId)
    {
        try
        {
            if (GameManager.IsDedicatedServer || entityId < 0)
            {
                return;
            }

            if (!ScreamerAlertEnhancedGate.IsScreamerPresentLocally())
            {
                return;
            }

            Type hybridRoutingType = AccessTools.TypeByName("ScreamerAlertHybridRouting");
            MethodInfo markCapabilityMethod = hybridRoutingType?.GetMethod(
                "MarkClientCapability",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(int), typeof(string) },
                null);

            if (markCapabilityMethod == null)
            {
                return;
            }

            string userCombined = PlatformManager.InternalLocalUserIdentifier?.CombinedString ?? string.Empty;
            markCapabilityMethod.Invoke(null, new object[] { entityId, userCombined });
            MarkAcknowledged();
        }
        catch (Exception ex)
        {
            Logging.Warning("ScreamerAlertEnhancedCapabilityHello", "Failed to mark host capability: " + ex.Message);
        }
    }

    private static bool ResolveReflection()
    {
        if (_resolved)
        {
            return _helloPackageType != null && _getPackageGenericMethod != null && _setupMethod != null;
        }

        _resolved = true;

        _helloPackageType = AccessTools.TypeByName("NetPackageScreamerAlertClientHello");
        if (_helloPackageType == null || !typeof(NetPackage).IsAssignableFrom(_helloPackageType))
        {
            return false;
        }

        MethodInfo[] methods = typeof(NetPackageManager).GetMethods(BindingFlags.Public | BindingFlags.Static);
        for (int i = 0; i < methods.Length; i++)
        {
            MethodInfo method = methods[i];
            if (method.Name == "GetPackage" && method.IsGenericMethodDefinition && method.GetParameters().Length == 0)
            {
                _getPackageGenericMethod = method;
                break;
            }
        }

        _setupMethod = _helloPackageType.GetMethod(
            "Setup",
            BindingFlags.Instance | BindingFlags.Public,
            null,
            new[] { typeof(int), typeof(string) },
            null);

        return _getPackageGenericMethod != null && _setupMethod != null;
    }
}