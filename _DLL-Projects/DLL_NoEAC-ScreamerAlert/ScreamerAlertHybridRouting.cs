using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;

public static class ScreamerAlertHybridRouting
{
    private const float AlertRangeMeters = 120f;
    private const float WhisperCooldownSeconds = 20f;

    private static readonly HashSet<string> ModClientUsers = new HashSet<string>(StringComparer.Ordinal);
    private static readonly Dictionary<int, string> ModClientByEntityId = new Dictionary<int, string>();
    private static readonly Dictionary<int, float> NextScoutWhisperAtByEntityId = new Dictionary<int, float>();
    private static readonly Dictionary<int, float> NextHordeWhisperAtByEntityId = new Dictionary<int, float>();

    public static void MarkClientCapability(int entityId, string userCombined)
    {
        if (string.IsNullOrEmpty(userCombined))
        {
            return;
        }

        ModClientUsers.Add(userCombined);
        if (entityId >= 0)
        {
            ModClientByEntityId[entityId] = userCombined;
        }
    }

    public static void ForgetClientByEntityId(int entityId)
    {
        ModClientByEntityId.Remove(entityId);
        NextScoutWhisperAtByEntityId.Remove(entityId);
        NextHordeWhisperAtByEntityId.Remove(entityId);
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
        return !string.IsNullOrEmpty(userCombined) && ModClientUsers.Contains(userCombined);
    }

    public static void NotifyVanillaPlayersOnScoutSpawn(EntityAlive scoutEntity)
    {
        NotifyVanillaPlayersInRange(scoutEntity, "ScreamerAlert_Scout", NextScoutWhisperAtByEntityId);
    }

    public static void NotifyVanillaPlayersOnHordeSpawn(EntityAlive hordeEntity)
    {
        NotifyVanillaPlayersInRange(hordeEntity, "ScreamerAlert_Horde", NextHordeWhisperAtByEntityId);
    }

    private static void NotifyVanillaPlayersInRange(EntityAlive sourceEntity, string localizationKey, Dictionary<int, float> nextAllowedAt)
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

        string message = Localization.Get(localizationKey);
        if (string.IsNullOrEmpty(message))
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

            if (Vector3.Distance(player.position, sourceEntity.position) > AlertRangeMeters)
            {
                continue;
            }

            ClientInfo clientInfo = manager.Clients?.ForEntityId(player.entityId);
            if (HasClientCapability(clientInfo))
            {
                continue;
            }


                string playerKey = clientInfo?.InternalId?.CombinedString;
                if (string.IsNullOrEmpty(playerKey))
                {
                    playerKey = ScreamerAlertModeSettings.GetPlayerKeyFromEntityId(player.entityId);
                }

                ScreamerAlertMode mode = ScreamerAlertModeSettings.GetModeForPlayerKey(playerKey, ScreamerAlertMode.OnWithNumbers);
                if (mode == ScreamerAlertMode.Off)
                {
                    continue;
                }
            if (nextAllowedAt.TryGetValue(player.entityId, out float notBefore) && now < notBefore)
            {
                continue;
            }

            nextAllowedAt[player.entityId] = now + WhisperCooldownSeconds;
            GameManager.Instance.ChatMessageServer(null, EChatType.Whisper, -1, message, new List<int> { player.entityId }, EMessageSender.Server);
        }
    }
}
