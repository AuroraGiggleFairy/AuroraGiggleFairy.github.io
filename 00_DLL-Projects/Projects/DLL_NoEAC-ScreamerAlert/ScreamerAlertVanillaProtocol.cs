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
        var clients = manager?.Clients?.List;
        if (manager == null || !manager.IsServer || world == null || clients == null)
        {
            return;
        }

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

            if (!LastStateByEntityId.TryGetValue(player.entityId, out PublishedState state))
            {
                state = new PublishedState();
                LastStateByEntityId[player.entityId] = state;
            }

            if (!state.ProtocolSent)
            {
                SendValue(client, player, ProtocolCVar, Version);
                state.ProtocolSent = true;
            }

            if (!ScreamerAlertHybridRouting.HasClientCapability(client))
            {
                continue;
            }

            int scoutCount = CountTrackedInRange(player, ScreamerAlertManager.Instance?.persistentScreamerIds, true);
            int hordeCount = CountTrackedInRange(player, ScreamerAlertManager.Instance?.persistentHordeZombieIds, false);
            int mode = (int)ResolveMode(player.entityId);
            if (state.ScoutCount != scoutCount)
            {
                SendValue(client, player, ScoutCountCVar, scoutCount);
                state.ScoutCount = scoutCount;
            }
            if (state.HordeCount != hordeCount)
            {
                SendValue(client, player, HordeCountCVar, hordeCount);
                state.HordeCount = hordeCount;
            }
            if (state.Mode != mode)
            {
                SendValue(client, player, ModeCVar, mode);
                state.Mode = mode;
            }
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

    private static void SendValue(ClientInfo client, EntityPlayer player, string name, float value)
    {
        NetPackageModifyCVar package = NetPackageManager.GetPackage<NetPackageModifyCVar>();
        if (package != null)
        {
            client.SendPackage(package.Setup(player, name, value, CVarOperation.set));
        }
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