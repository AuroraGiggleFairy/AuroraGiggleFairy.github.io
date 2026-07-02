using System;
using Platform;

public class NetPackageScreamerAlertModeRequest : NetPackage
{
    private int _entityId = -1;
    private int _requestedMode = (int)ScreamerAlertMode.On;
    private bool _queryOnly;

    public NetPackageScreamerAlertModeRequest Setup(int entityId, ScreamerAlertMode requestedMode, bool queryOnly)
    {
        _entityId = entityId;
        _requestedMode = (int)requestedMode;
        _queryOnly = queryOnly;
        return this;
    }

    public override void read(PooledBinaryReader reader)
    {
        _entityId = reader.ReadInt32();
        _requestedMode = reader.ReadInt32();
        _queryOnly = reader.ReadBoolean();
    }

    public override void write(PooledBinaryWriter writer)
    {
        base.write(writer);
        writer.Write(_entityId);
        writer.Write(_requestedMode);
        writer.Write(_queryOnly);
    }

    public override void ProcessPackage(World world, GameManager callbacks)
    {
        _ = world;
        _ = callbacks;

        ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
        if (manager == null || !manager.IsServer || _entityId < 0)
        {
            return;
        }

        bool enhanced = ScreamerAlertHybridRouting.HasClientCapabilityByEntityId(_entityId);
        if (!enhanced && !GameManager.IsDedicatedServer)
        {
            EntityPlayer localPlayer = GameManager.Instance?.World?.GetPrimaryPlayer();
            if (localPlayer != null && localPlayer.entityId == _entityId)
            {
                enhanced = IsEnhancedAgfInstalledLocally();
            }
        }

        ScreamerAlertMode effectiveMode = _queryOnly
            ? ResolveCurrentServerMode(_entityId, enhanced, manager)
            : ApplyRequestedMode(_entityId, enhanced, manager);

        ClientInfo targetClient = manager.Clients?.ForEntityId(_entityId);
        if (targetClient != null)
        {
            NetPackageScreamerAlertModeAck ack = NetPackageManager.GetPackage<NetPackageScreamerAlertModeAck>();
            if (ack != null)
            {
                targetClient.SendPackage(ack.Setup(_entityId, effectiveMode, enhanced));
            }
        }

        if (!GameManager.IsDedicatedServer)
        {
            EntityPlayer localPlayer = GameManager.Instance?.World?.GetPrimaryPlayer();
            if (localPlayer != null && localPlayer.entityId == _entityId)
            {
                XUiC_ScreamerAlertOptions.OnAuthoritativeModeAck(effectiveMode, enhanced);
            }
        }
    }

    public override int GetLength()
    {
        // entityId + requestedMode + query flag
        return 4 + 4 + 1;
    }

    private ScreamerAlertMode ApplyRequestedMode(int entityId, bool enhanced, ConnectionManager manager)
    {
        ScreamerAlertMode requested = ParseRequestedMode(_requestedMode);
        requested = NormalizeModeForCapability(requested, enhanced);

        if (!ScreamerAlertModeSettings.SetModeForEntityId(entityId, requested))
        {
            return ResolveCurrentServerMode(entityId, enhanced, manager);
        }

        return requested;
    }

    private static ScreamerAlertMode ResolveCurrentServerMode(int entityId, bool enhanced, ConnectionManager manager)
    {
        string playerKey = manager?.Clients?.ForEntityId(entityId)?.InternalId?.CombinedString;
        if (string.IsNullOrEmpty(playerKey))
        {
            playerKey = ScreamerAlertModeSettings.GetPlayerKeyFromEntityId(entityId);
        }

        ScreamerAlertMode serverDefault = NormalizeModeForCapability(ScreamerAlertModeSettings.GetServerDefaultMode(), enhanced);
        ScreamerAlertMode mode = ScreamerAlertModeSettings.GetModeForPlayerKey(playerKey, serverDefault);
        return NormalizeModeForCapability(mode, enhanced);
    }

    private static ScreamerAlertMode ParseRequestedMode(int value)
    {
        switch (value)
        {
            case 0:
                return ScreamerAlertMode.Off;
            case 1:
                return ScreamerAlertMode.On;
            case 2:
                return ScreamerAlertMode.OnWithNumbers;
            default:
                return ScreamerAlertMode.On;
        }
    }

    private static ScreamerAlertMode NormalizeModeForCapability(ScreamerAlertMode mode, bool enhanced)
    {
        if (!enhanced && mode == ScreamerAlertMode.OnWithNumbers)
        {
            return ScreamerAlertMode.On;
        }

        return mode;
    }

    private static bool IsEnhancedAgfInstalledLocally()
    {
        try
        {
            AppDomain domain = AppDomain.CurrentDomain;
            if (domain == null)
            {
                return false;
            }

            foreach (var assembly in domain.GetAssemblies())
            {
                if (assembly == null)
                {
                    continue;
                }

                string assemblyName = assembly.GetName()?.Name;
                if (!string.IsNullOrEmpty(assemblyName) && assemblyName.IndexOf("EnhancedAGF", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                if (assembly.GetType("ScreamerAlertEnhancedGate", throwOnError: false) != null)
                {
                    return true;
                }
            }
        }
        catch
        {
        }

        return false;
    }
}
