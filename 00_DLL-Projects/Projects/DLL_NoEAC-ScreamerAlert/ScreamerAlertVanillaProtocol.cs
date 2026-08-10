using System.Collections.Generic;
using UnityEngine;

public static class ScreamerAlertVanillaProtocol
{
    public const int Version = 2;
    public const string ProtocolCVar = ".agfSAProtocol";
    public const string ScoutCountCVar = ".agfSAScoutCount";
    public const string HordeCountCVar = ".agfSAHordeCount";
    public const string ModeCVar = ".agfSAMode";

    private const float AlertRangeSqr = 120f * 120f;

    private sealed class PublishedState
    {
        public int ScoutCount = int.MinValue;
        public int HordeCount = int.MinValue;
        public int Mode = int.MinValue;
        public bool ProtocolSent;
    }

    private static readonly Dictionary<int, PublishedState> LastStateByEntityId =
        new Dictionary<int, PublishedState>();

    public static void PublishToEnhancedClients()
    {
        ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
        World world = GameManager.Instance?.World;
        if (manager == null || !manager.IsServer || world == null)
        {
            return;
        }

        bool publishedLocalHost = false;
        var clients = manager.Clients?.List;
        if (clients != null)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                ClientInfo client = clients[i];
                if (client == null)
                {
                    continue;
                }

                EntityPlayer player = world.GetEntity(client.entityId) as EntityPlayer;
                if (player == null || player.IsDead())
                {
                    continue;
                }

                bool isLocalHostPlayer = IsLocalHostPlayer(player);
                if (isLocalHostPlayer)
                {
                    publishedLocalHost = true;
                }

                PublishPlayerState(player, isLocalHostPlayer ? null : client);
            }
        }

        // SP / listen-host: primary player is often not present in Clients.List.
        // Apply Protocol + counts directly so EnhancedAGF can unlock UI without net ClientInfo.
        if (!publishedLocalHost)
        {
            PublishToLocalHostPlayer(world);
        }
    }

    public static void ForgetPlayer(int entityId)
    {
        LastStateByEntityId.Remove(entityId);
    }

    public static void ForceRefresh(int entityId)
    {
        LastStateByEntityId.Remove(entityId);
    }

    private static void PublishToLocalHostPlayer(World world)
    {
        if (GameManager.IsDedicatedServer || world == null)
        {
            return;
        }

        EntityPlayer localPlayer = world.GetPrimaryPlayer();
        if (localPlayer == null || localPlayer.IsDead() || localPlayer.entityId < 0)
        {
            return;
        }

        PublishPlayerState(localPlayer, null);
    }

    private static void PublishPlayerState(EntityPlayer player, ClientInfo client)
    {
        if (player == null || player.IsDead())
        {
            return;
        }

        if (!LastStateByEntityId.TryGetValue(player.entityId, out PublishedState state))
        {
            state = new PublishedState();
            LastStateByEntityId[player.entityId] = state;
        }

        if (!state.ProtocolSent)
        {
            SetValue(client, player, ProtocolCVar, Version);
            state.ProtocolSent = true;
        }

        if (client != null)
        {
            if (!ScreamerAlertHybridRouting.HasClientCapability(client))
            {
                return;
            }
        }
        else if (!ScreamerAlertHybridRouting.HasClientCapabilityByEntityId(player.entityId))
        {
            return;
        }

        int scoutCount = CountTrackedInRange(player, ScreamerAlertManager.Instance?.persistentScreamerIds, true);
        int hordeCount = CountTrackedInRange(player, ScreamerAlertManager.Instance?.persistentHordeZombieIds, false);
        int mode = (int)ResolveMode(player.entityId);
        if (state.ScoutCount != scoutCount)
        {
            SetValue(client, player, ScoutCountCVar, scoutCount);
            state.ScoutCount = scoutCount;
        }
        if (state.HordeCount != hordeCount)
        {
            SetValue(client, player, HordeCountCVar, hordeCount);
            state.HordeCount = hordeCount;
        }
        if (state.Mode != mode)
        {
            SetValue(client, player, ModeCVar, mode);
            state.Mode = mode;
        }
    }

    private static void SetValue(ClientInfo client, EntityPlayer player, string name, float value)
    {
        if (client == null)
        {
            ApplyLocalValue(player, name, value);
            return;
        }

        NetPackageModifyCVar package = NetPackageManager.GetPackage<NetPackageModifyCVar>();
        if (package != null)
        {
            client.SendPackage(package.Setup(player, name, value, CVarOperation.set));
        }
    }

    private static void ApplyLocalValue(EntityPlayer player, string name, float value)
    {
        if (player?.Buffs == null || string.IsNullOrEmpty(name))
        {
            return;
        }

        // Local-only write: do not net-sync (listen-host / SP path).
        player.Buffs.SetCustomVar(name, value, false, CVarOperation.set);
    }

    private static bool IsLocalHostPlayer(EntityPlayer player)
    {
        if (GameManager.IsDedicatedServer || player == null)
        {
            return false;
        }

        EntityPlayer primary = GameManager.Instance?.World?.GetPrimaryPlayer();
        return primary != null && primary.entityId == player.entityId;
    }

    private static int CountTrackedInRange(EntityPlayer player, HashSet<int> trackedIds, bool requireScout)
    {
        if (player == null || trackedIds == null || trackedIds.Count == 0)
        {
            return 0;
        }

        var entities = GameManager.Instance?.World?.Entities?.dict;
        if (entities == null)
        {
            return 0;
        }

        int count = 0;
        foreach (int id in trackedIds)
        {
            if (!entities.TryGetValue(id, out Entity entity) || entity == null || entity.IsDead())
            {
                continue;
            }
            if (requireScout && (!(entity is EntityAlive alive) || !alive.IsScoutZombie))
            {
                continue;
            }
            if ((player.position - entity.position).sqrMagnitude <= AlertRangeSqr)
            {
                count++;
            }
        }
        return count;
    }

    private static ScreamerAlertMode ResolveMode(int entityId)
    {
        string playerKey = ScreamerAlertModeSettings.GetPlayerKeyFromEntityId(entityId);
        return ScreamerAlertModeSettings.GetModeForPlayerKey(playerKey, ScreamerAlertModeSettings.GetServerDefaultMode());
    }
}
