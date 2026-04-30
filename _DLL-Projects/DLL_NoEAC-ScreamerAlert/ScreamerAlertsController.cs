using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScreamerAlertsController : MonoBehaviour
{
    private static Dictionary<int, bool> playerAlertVisibility = new Dictionary<int, bool>();
    internal string screamerHordeAlertMessage = string.Empty;
    internal float hordeAlertEndTime;
    internal Vector3 hordeAlertPosition = Vector3.zero;
    public static ScreamerAlertsController Instance;
    public string screamerAlertMessage = string.Empty;
    public static bool CompassVisible = false;

    public static bool GetPlayerAlertVisibility(int entityId)
    {
        if (playerAlertVisibility.TryGetValue(entityId, out var value))
        {
            return value;
        }
        return true;
    }

    public static void SetPlayerAlertVisibility(int entityId, bool visible)
    {
        playerAlertVisibility[entityId] = visible;
    }

    private void Update()
    {
        UpdateAlertMessage();
        UpdateHordeAlert();
        ScreamerAlertMode mode = ScreamerAlertModeSettings.GetModeForLocalPlayer(ScreamerAlertMode.OnWithNumbers);
        if (mode == ScreamerAlertMode.Off)
        {
            screamerAlertMessage = string.Empty;
            screamerHordeAlertMessage = string.Empty;
        }

        if (((XUiC_ScreamerAlerts.Instance != null) ? XUiC_ScreamerAlerts.Instance.ViewComponent : null) != null)
        {
            bool isVisible = !string.IsNullOrEmpty(GetScreamerAlertMessage()) || !string.IsNullOrEmpty(GetScreamerHordeAlertMessage());
            XUiC_ScreamerAlerts.Instance.ViewComponent.IsVisible = isVisible;
            XUiC_ScreamerAlerts.Instance.RefreshBindingsSelfAndChildren();
        }
    }

    public void TriggerScreamerHordeAlert(Vector3 hordePosition)
    {
        hordeAlertPosition = hordePosition;
        hordeAlertEndTime = Time.time + 10f;
        UpdateHordeAlert();
    }

    private void UpdateHordeAlert()
    {
        EntityPlayer entityPlayer = GameManager.Instance.World?.GetPrimaryPlayer();
        bool playerNearHorde = false;
        var toRemove = new List<int>();
        if (entityPlayer != null && !entityPlayer.IsDead())
        {
            foreach (int entityId in ScreamerAlertManager.Instance.persistentHordeZombieIds)
            {
                var worldEntities = GameManager.Instance.World?.Entities;
                if (worldEntities != null && worldEntities.dict.TryGetValue(entityId, out var entity) && entity != null)
                {
                    if (entity.IsDead())
                    {
                        toRemove.Add(entityId);
                        continue;
                    }
                    float dist = Vector3.Distance(entityPlayer.position, entity.position);
                    if (dist <= 120f)
                    {
                        playerNearHorde = true;
                    }
                }
                else
                {
                    toRemove.Add(entityId);
                }
            }
        }
        // Remove dead or missing zombies from the tracked set
        foreach (var id in toRemove)
            ScreamerAlertManager.Instance.persistentHordeZombieIds.Remove(id);
        // Horde alert is now based only on live horde zombies and proximity
        screamerHordeAlertMessage = playerNearHorde ? Localization.Get("ScreamerAlert_Horde") : string.Empty;
        if (XUiC_ScreamerAlerts.Instance != null)
        {
            XUiC_ScreamerAlerts.Instance.RefreshBindingsSelfAndChildren();
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void OnOpenCompassWindow()
    {
    }

    public void OnCloseCompassWindow()
    {
    }

    public void OnPlayerEventClearAlert()
    {
    }

    public void UpdateAlertMessage()
    {
        if (ScreamerAlertManager.Instance == null)
            return;

        // Use server-synced screamer IDs on multiplayer clients, local list in singleplayer/server
        // Use syncedScreamerIds for all counting and proximity checks on clients
        var screamerIds = ConnectionManager.Instance.IsServer
            ? ScreamerAlertManager.Instance?.persistentScreamerIds
            : ScreamerAlertManager.Instance?.syncedScreamerIds;

        // Log screamer and horde counts for both singleplayer and client
        // (Removed screamerCount and hordeCount, handled in ScreamerAlertManager log)
        // Only log on server, handled in ScreamerAlertManager
        EntityPlayer entityPlayer = GameManager.Instance.World?.GetPrimaryPlayer();
        bool playerNearScout = false;
        var toRemove = new List<int>();
        if (screamerIds != null)
        {
            var worldEntities = GameManager.Instance.World?.Entities;
            var players = GameManager.Instance.World?.Players?.dict?.Values;
            var screamerPositions = ScreamerAlertManager.Instance.syncedScreamerPositions;
            if (players != null)
            {
                foreach (var player in players)
                {
                    if (player == null || player.IsDead()) continue;
                    foreach (int entityId in screamerIds)
                    {
                        Vector3 pos = Vector3.zero;
                        if (worldEntities != null && worldEntities.dict.TryGetValue(entityId, out var entity) && entity != null && !entity.IsDead())
                        {
                            pos = entity.position;
                        }
                        else if (screamerPositions != null && screamerPositions.TryGetValue(entityId, out var syncedPos))
                        {
                            pos = syncedPos;
                        }
                        else
                        {
                            toRemove.Add(entityId);
                            continue;
                        }
                        float dist = Vector3.Distance(player.position, pos);
                        if (dist <= 120f)
                        {
                            if (player == entityPlayer) playerNearScout = true;
                        }
                    }
                }
            }
            else if (entityPlayer != null && !entityPlayer.IsDead())
            {
                foreach (int entityId in screamerIds)
                {
                    Vector3 pos = Vector3.zero;
                    if (worldEntities != null && worldEntities.dict.TryGetValue(entityId, out var entity) && entity != null && !entity.IsDead())
                    {
                        pos = entity.position;
                    }
                    else if (screamerPositions != null && screamerPositions.TryGetValue(entityId, out var syncedPos))
                    {
                        pos = syncedPos;
                    }
                    else
                    {
                        toRemove.Add(entityId);
                        continue;
                    }
                    float dist = Vector3.Distance(entityPlayer.position, pos);
                    if (dist <= 120f)
                    {
                        playerNearScout = true;
                    }
                }
            }
        }
        // Remove dead or missing screamers from the tracked set
        foreach (var id in toRemove)
        {
            if (ConnectionManager.Instance.IsServer)
                ScreamerAlertManager.Instance.persistentScreamerIds.Remove(id);
            else
                ScreamerAlertManager.Instance.syncedScreamerIds.Remove(id);
        }
        string text = playerNearScout ? Localization.Get("ScreamerAlert_Scout") : "";
        if (string.IsNullOrEmpty(text))
        {
            if (!string.IsNullOrEmpty(screamerAlertMessage))
            {
                screamerAlertMessage = "";
                UpdateScreamerAlertUI();
                if (XUiC_ScreamerAlerts.Instance != null)
                {
                    XUiC_ScreamerAlerts.Instance.RefreshBindingsSelfAndChildren();
                }
            }
        }
        else if (text != screamerAlertMessage)
        {
            screamerAlertMessage = text;
            UpdateScreamerAlertUI();
            if (XUiC_ScreamerAlerts.Instance != null)
            {
                XUiC_ScreamerAlerts.Instance.RefreshBindingsSelfAndChildren();
            }
        }
    }

    public string GetScreamerAlertMessage()
    {
        ScreamerAlertMode mode = ScreamerAlertModeSettings.GetModeForLocalPlayer(ScreamerAlertMode.OnWithNumbers);
        if (mode == ScreamerAlertMode.Off)
        {
            return string.Empty;
        }

        // Show correct screamer count: use persistentScreamerIds on server, syncedScreamerIds on clients
        int screamerCount = ConnectionManager.Instance.IsServer
            ? (ScreamerAlertManager.Instance?.persistentScreamerIds?.Count ?? 0)
            : (ScreamerAlertManager.Instance?.syncedScreamerIds?.Count ?? 0);

        string baseMessage = screamerAlertMessage;
        if (mode == ScreamerAlertMode.On)
        {
            return ScreamerAlertModeSettings.StripNumberSuffix(baseMessage);
        }

        if (!string.IsNullOrEmpty(baseMessage))
        {
            return $"{baseMessage} [FFFFFF]({screamerCount})[-]";
        }
        return baseMessage;
    }

    public string GetScreamerHordeAlertMessage()
    {
        ScreamerAlertMode mode = ScreamerAlertModeSettings.GetModeForLocalPlayer(ScreamerAlertMode.OnWithNumbers);
        if (mode == ScreamerAlertMode.Off)
        {
            return string.Empty;
        }

        // Append the number of tracked horde zombies in white
        int hordeCount = ScreamerAlertManager.Instance?.persistentHordeZombieIds?.Count ?? 0;

        string baseMessage = screamerHordeAlertMessage;
        if (mode == ScreamerAlertMode.On)
        {
            return ScreamerAlertModeSettings.StripNumberSuffix(baseMessage);
        }

        if (!string.IsNullOrEmpty(baseMessage))
        {
            return $"{baseMessage} [FFFFFF]({hordeCount})[-]";
        }
        return baseMessage;
    }

    public void UpdateScreamerAlertUI()
    {
        // Placeholder for any additional UI update logic if needed
    }
}
