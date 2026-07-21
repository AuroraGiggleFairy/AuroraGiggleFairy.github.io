using System;
using System.Collections.Generic;
using UnityEngine;

public static class ScreamerAlertHybridRouting
{
    private const string EntityOnlyCapabilityMarker = "#entity-only#";
    private const float AlertRangeMeters = 120f;
    private const float MergeWindowSeconds = 3f;
    private const float IncidentCooldownSeconds = 7f;

    private const string ScoutStampedMessageKey = "ScreamerAlert_Scout_ChatStamped";
    private const string HordeStampedMessageKey = "ScreamerAlert_Horde_ChatStamped";

    private sealed class AlertIncidentState
    {
        public bool HasSent;
        public float LastSentAt;
        public bool HasPendingFollowup;
    }

    private static readonly Dictionary<int, AlertIncidentState> ScoutIncidentStateByEntityId =
        new Dictionary<int, AlertIncidentState>();

    private static readonly Dictionary<int, AlertIncidentState> HordeIncidentStateByEntityId =
        new Dictionary<int, AlertIncidentState>();

    private static readonly Dictionary<string, int> ModClientUserRefCounts =
        new Dictionary<string, int>(StringComparer.Ordinal);

    private static readonly Dictionary<int, string> ModClientByEntityId =
        new Dictionary<int, string>();

    private static readonly Dictionary<int, long> CapabilityStampByEntityId =
        new Dictionary<int, long>();

    private static long CapabilityStampCounter;

    public static void MarkClientCapability(int entityId, string userCombined)
    {
        string normalizedUser = string.IsNullOrEmpty(userCombined)
            ? EntityOnlyCapabilityMarker
            : userCombined;

        if (entityId >= 0
            && ModClientByEntityId.TryGetValue(entityId, out string existingUser)
            && !string.IsNullOrEmpty(existingUser)
            && !string.Equals(existingUser, EntityOnlyCapabilityMarker, StringComparison.Ordinal))
        {
            RemoveUserRef_NoThrow(existingUser);
        }

        if (entityId >= 0)
        {
            ModClientByEntityId[entityId] = normalizedUser;
            CapabilityStampByEntityId[entityId] = ++CapabilityStampCounter;
        }

        if (string.Equals(normalizedUser, EntityOnlyCapabilityMarker, StringComparison.Ordinal))
        {
            return;
        }

        if (ModClientUserRefCounts.TryGetValue(normalizedUser, out int count))
        {
            ModClientUserRefCounts[normalizedUser] = count + 1;
        }
        else
        {
            ModClientUserRefCounts[normalizedUser] = 1;
        }
    }

    public static void ForgetClientByEntityId(int entityId)
    {
        ClearClientCapabilityByEntityId(entityId);
        ScoutIncidentStateByEntityId.Remove(entityId);
        HordeIncidentStateByEntityId.Remove(entityId);
    }

    public static void ClearClientCapabilityByEntityId(int entityId)
    {
        if (entityId < 0)
        {
            return;
        }

        if (ModClientByEntityId.TryGetValue(entityId, out string userCombined) && !string.IsNullOrEmpty(userCombined))
        {
            RemoveUserRef_NoThrow(userCombined);
        }

        ModClientByEntityId.Remove(entityId);
        CapabilityStampByEntityId.Remove(entityId);
    }

    public static bool HasClientCapability(ClientInfo clientInfo)
    {
        if (clientInfo == null)
        {
            return false;
        }

        if (clientInfo.entityId >= 0 && ModClientByEntityId.TryGetValue(clientInfo.entityId, out string byEntity) && !string.IsNullOrEmpty(byEntity))
        {
            return true;
        }

        string userCombined = clientInfo.InternalId?.CombinedString;
        return !string.IsNullOrEmpty(userCombined) && ModClientUserRefCounts.ContainsKey(userCombined);
    }

    public static bool HasClientCapabilityByEntityId(int entityId)
    {
        if (entityId < 0)
        {
            return false;
        }

        if (ModClientByEntityId.ContainsKey(entityId))
        {
            return true;
        }

        ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
        ClientInfo clientInfo = manager?.Clients?.ForEntityId(entityId);
        return HasClientCapability(clientInfo);
    }

    public static long GetCapabilityStampByEntityId(int entityId)
    {
        if (entityId < 0)
        {
            return 0L;
        }

        return CapabilityStampByEntityId.TryGetValue(entityId, out long stamp)
            ? stamp
            : 0L;
    }

    public static void NotifyVanillaPlayersOnScoutSpawn(EntityAlive scoutEntity)
    {
        NotifyVanillaPlayersInRange(scoutEntity, "ScreamerAlert_Scout", ScoutIncidentStateByEntityId);
    }

    public static void NotifyVanillaPlayersOnHordeSpawn(EntityAlive hordeEntity)
    {
        NotifyVanillaPlayersInRange(hordeEntity, "ScreamerAlert_Horde", HordeIncidentStateByEntityId);
    }

    public static void TickQueuedIncidents()
    {
        ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
        if (manager == null || !manager.IsServer)
        {
            return;
        }

        World world = GameManager.Instance?.World;
        if (world?.Players?.dict == null)
        {
            return;
        }

        ProcessPendingFollowups(world, "ScreamerAlert_Scout", ScoutIncidentStateByEntityId, IsScoutAlertActiveForPlayer);
        ProcessPendingFollowups(world, "ScreamerAlert_Horde", HordeIncidentStateByEntityId, IsHordeAlertActiveForPlayer);
    }

    private static void NotifyVanillaPlayersInRange(
        EntityAlive sourceEntity,
        string localizationKey,
        Dictionary<int, AlertIncidentState> incidentStateByEntityId)
    {
        if (sourceEntity == null || sourceEntity.IsDead())
        {
            return;
        }

        ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
        if (manager == null || !manager.IsServer)
        {
            return;
        }

        World world = GameManager.Instance?.World;
        if (world?.Players?.dict == null)
        {
            return;
        }

        float now = Time.time;
        foreach (EntityPlayer player in world.Players.dict.Values)
        {
            if (player == null || player.IsDead())
            {
                continue;
            }

            if (HasClientCapabilityByEntityId(player.entityId))
            {
                // Enhanced clients render screamer/horde alerts via UI, not chat whispers.
                continue;
            }

            if (Vector3.Distance(player.position, sourceEntity.position) > AlertRangeMeters)
            {
                continue;
            }

            if (!IsPlayerModeEnabled(player.entityId, manager))
            {
                continue;
            }

            HandleIncidentEventForPlayer(player.entityId, localizationKey, incidentStateByEntityId, world, now);
        }
    }

    private static void HandleIncidentEventForPlayer(
        int playerEntityId,
        string localizationKey,
        Dictionary<int, AlertIncidentState> incidentStateByEntityId,
        World world,
        float now)
    {
        if (!incidentStateByEntityId.TryGetValue(playerEntityId, out AlertIncidentState state) || state == null || !state.HasSent)
        {
            if (TrySendAlertToPlayer(playerEntityId, localizationKey, world))
            {
                incidentStateByEntityId[playerEntityId] = new AlertIncidentState
                {
                    HasSent = true,
                    LastSentAt = now,
                    HasPendingFollowup = false
                };
            }

            return;
        }

        float elapsed = now - state.LastSentAt;
        if (elapsed <= MergeWindowSeconds)
        {
            return;
        }

        if (elapsed < IncidentCooldownSeconds)
        {
            state.HasPendingFollowup = true;
            return;
        }

        if (TrySendAlertToPlayer(playerEntityId, localizationKey, world))
        {
            state.LastSentAt = now;
            state.HasPendingFollowup = false;
        }
    }

    private static void ProcessPendingFollowups(
        World world,
        string localizationKey,
        Dictionary<int, AlertIncidentState> incidentStateByEntityId,
        Func<EntityPlayer, bool> isAlertActiveForPlayer)
    {
        if (incidentStateByEntityId.Count == 0)
        {
            return;
        }

        float now = Time.time;
        List<int> entityIds = new List<int>(incidentStateByEntityId.Keys);
        for (int i = 0; i < entityIds.Count; i++)
        {
            int playerEntityId = entityIds[i];
            if (!incidentStateByEntityId.TryGetValue(playerEntityId, out AlertIncidentState state) || state == null)
            {
                continue;
            }

            if (!state.HasSent || !state.HasPendingFollowup)
            {
                continue;
            }

            if (now - state.LastSentAt < IncidentCooldownSeconds)
            {
                continue;
            }

            EntityPlayer player = world.GetEntity(playerEntityId) as EntityPlayer;
            if (player == null || player.IsDead())
            {
                incidentStateByEntityId.Remove(playerEntityId);
                continue;
            }

            if (HasClientCapabilityByEntityId(playerEntityId))
            {
                state.HasPendingFollowup = false;
                continue;
            }

            if (!IsPlayerModeEnabled(playerEntityId, null) || !isAlertActiveForPlayer(player))
            {
                state.HasPendingFollowup = false;
                continue;
            }

            if (TrySendAlertToPlayer(playerEntityId, localizationKey, world))
            {
                state.LastSentAt = now;
            }

            state.HasPendingFollowup = false;
        }
    }

    private static bool IsPlayerModeEnabled(int playerEntityId, ConnectionManager manager)
    {
        ConnectionManager resolvedManager = manager ?? SingletonMonoBehaviour<ConnectionManager>.Instance;
        ClientInfo clientInfo = resolvedManager?.Clients?.ForEntityId(playerEntityId);
        bool enhanced = HasClientCapabilityByEntityId(playerEntityId);
        string playerKey = clientInfo?.InternalId?.CombinedString;
        if (string.IsNullOrEmpty(playerKey))
        {
            playerKey = ScreamerAlertModeSettings.GetPlayerKeyFromEntityId(playerEntityId);
        }

        ScreamerAlertMode serverDefault = ScreamerAlertModeSettings.GetServerDefaultMode();
        if (!enhanced && serverDefault == ScreamerAlertMode.OnWithNumbers)
        {
            serverDefault = ScreamerAlertMode.On;
        }

        ScreamerAlertMode mode = ScreamerAlertModeSettings.GetModeForPlayerKey(playerKey, serverDefault);
        if (!enhanced && mode == ScreamerAlertMode.OnWithNumbers)
        {
            mode = ScreamerAlertMode.On;
        }

        return mode != ScreamerAlertMode.Off;
    }

    private static void RemoveUserRef_NoThrow(string userCombined)
    {
        if (string.IsNullOrEmpty(userCombined))
        {
            return;
        }

        if (!ModClientUserRefCounts.TryGetValue(userCombined, out int count))
        {
            return;
        }

        count--;
        if (count <= 0)
        {
            ModClientUserRefCounts.Remove(userCombined);
        }
        else
        {
            ModClientUserRefCounts[userCombined] = count;
        }
    }

    private static bool IsScoutAlertActiveForPlayer(EntityPlayer player)
    {
        return IsAnyTrackedEntityInRange(player, ScreamerAlertManager.Instance?.persistentScreamerIds, requireScoutZombie: true);
    }

    private static bool IsHordeAlertActiveForPlayer(EntityPlayer player)
    {
        return IsAnyTrackedEntityInRange(player, ScreamerAlertManager.Instance?.persistentHordeZombieIds, requireScoutZombie: false);
    }

    private static bool IsAnyTrackedEntityInRange(EntityPlayer player, HashSet<int> trackedEntityIds, bool requireScoutZombie)
    {
        if (player == null || player.IsDead() || trackedEntityIds == null || trackedEntityIds.Count == 0)
        {
            return false;
        }

        var worldEntities = GameManager.Instance?.World?.Entities;
        if (worldEntities?.dict == null)
        {
            return false;
        }

        foreach (int entityId in trackedEntityIds)
        {
            if (!worldEntities.dict.TryGetValue(entityId, out Entity entity) || entity == null || entity.IsDead())
            {
                continue;
            }

            if (requireScoutZombie)
            {
                if (!(entity is EntityAlive scout) || !scout.IsScoutZombie)
                {
                    continue;
                }
            }

            if (Vector3.Distance(player.position, entity.position) <= AlertRangeMeters)
            {
                return true;
            }
        }

        return false;
    }

    private static bool TrySendAlertToPlayer(int playerEntityId, string localizationKey, World world)
    {
        string message = BuildStampedMessage(localizationKey, world);
        if (string.IsNullOrEmpty(message))
        {
            return false;
        }

        GameManager.Instance.ChatMessageServer(null, EChatType.Whisper, -1, message, new List<int> { playerEntityId }, EMessageSender.Server);
        return true;
    }

    private static string BuildStampedMessage(string localizationKey, World world)
    {
        if (string.IsNullOrEmpty(localizationKey))
        {
            return string.Empty;
        }

        string templateKey = localizationKey == "ScreamerAlert_Horde"
            ? HordeStampedMessageKey
            : ScoutStampedMessageKey;

        string template = Localization.Get(templateKey);
        if (string.IsNullOrEmpty(template))
        {
            return Localization.Get(localizationKey);
        }

        if (world == null)
        {
            return template;
        }

        (int day, int hour, int minute) = GameUtils.WorldTimeToElements(world.worldTime);
        return string.Format(template, day, hour, minute);
    }
}
