using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScreamerAlertsController : MonoBehaviour
{
    private static Dictionary<int, bool> playerAlertVisibility = new Dictionary<int, bool>();
    private const float UiEvalIntervalSeconds = 0.2f;
    private const float AlertRangeSqr = 120f * 120f;
    internal string screamerHordeAlertMessage = string.Empty;
    internal int nearbyHordeCount;
    internal int nearbyScoutCount;
    internal float hordeAlertEndTime;
    internal Vector3 hordeAlertPosition = Vector3.zero;
    public static ScreamerAlertsController Instance;
    public string screamerAlertMessage = string.Empty;
    public static bool CompassVisible = false;
    private float nextUiEvalAt;
    private string lastUiScoutMessage = string.Empty;
    private string lastUiHordeMessage = string.Empty;
    private int lastUiScoutCount = -1;
    private int lastUiHordeCount = -1;
    private ScreamerAlertMode lastUiMode = (ScreamerAlertMode)(-1);

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
        if (Time.time < nextUiEvalAt)
        {
            return;
        }

        nextUiEvalAt = Time.time + UiEvalIntervalSeconds;

        UpdateAlertMessage();
        UpdateHordeAlert();

        RefreshUiBindingsIfNeeded();
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
        int nearHordeCount = 0;
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
                    Vector3 delta = entityPlayer.position - entity.position;
                    if (delta.sqrMagnitude <= AlertRangeSqr)
                    {
                        nearHordeCount++;
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

        nearbyHordeCount = nearHordeCount;

        // Horde alert is now based only on live horde zombies and proximity
        string nextHordeMessage = nearHordeCount > 0 ? Localization.Get("ScreamerAlert_Horde") : string.Empty;
        if (nextHordeMessage != screamerHordeAlertMessage)
        {
            screamerHordeAlertMessage = nextHordeMessage;
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
        int nearScoutCount = 0;
        var toRemove = new List<int>();
        if (screamerIds != null && entityPlayer != null && !entityPlayer.IsDead())
        {
            var worldEntities = GameManager.Instance.World?.Entities;
            var screamerPositions = ScreamerAlertManager.Instance.syncedScreamerPositions;
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

                Vector3 delta = entityPlayer.position - pos;
                if (delta.sqrMagnitude <= AlertRangeSqr)
                {
                    nearScoutCount++;
                }
            }
        }

        nearbyScoutCount = nearScoutCount;

        // Remove dead or missing screamers from the tracked set
        foreach (var id in toRemove)
        {
            if (ConnectionManager.Instance.IsServer)
                ScreamerAlertManager.Instance.persistentScreamerIds.Remove(id);
            else
                ScreamerAlertManager.Instance.syncedScreamerIds.Remove(id);
        }
        string text = nearScoutCount > 0 ? Localization.Get("ScreamerAlert_Scout") : "";
        if (string.IsNullOrEmpty(text))
        {
            if (!string.IsNullOrEmpty(screamerAlertMessage))
            {
                screamerAlertMessage = "";
            }
        }
        else if (text != screamerAlertMessage)
        {
            screamerAlertMessage = text;
        }
    }

    private void RefreshUiBindingsIfNeeded()
    {
        string scoutMessage = screamerAlertMessage ?? string.Empty;
        string hordeMessage = screamerHordeAlertMessage ?? string.Empty;
        int scoutCount = nearbyScoutCount;
        int hordeCount = nearbyHordeCount;
        ScreamerAlertMode mode = ScreamerAlertModeSettings.GetModeForLocalPlayer(ScreamerAlertMode.OnWithNumbers);

        bool changed = scoutMessage != lastUiScoutMessage
            || hordeMessage != lastUiHordeMessage
            || scoutCount != lastUiScoutCount
            || hordeCount != lastUiHordeCount
            || mode != lastUiMode;

        if (!changed)
        {
            return;
        }

        lastUiScoutMessage = scoutMessage;
        lastUiHordeMessage = hordeMessage;
        lastUiScoutCount = scoutCount;
        lastUiHordeCount = hordeCount;
        lastUiMode = mode;
        UpdateScreamerAlertUI();
    }

    public string GetScreamerAlertMessage()
    {
        ScreamerAlertMode mode = GetServerBaselineLocalMode();
        if (mode == ScreamerAlertMode.Off)
        {
            return string.Empty;
        }

        string baseMessage = screamerAlertMessage;
        return ScreamerAlertModeSettings.StripNumberSuffix(baseMessage);
    }

    public string GetScreamerHordeAlertMessage()
    {
        ScreamerAlertMode mode = GetServerBaselineLocalMode();
        if (mode == ScreamerAlertMode.Off)
        {
            return string.Empty;
        }

        string baseMessage = screamerHordeAlertMessage;
        return ScreamerAlertModeSettings.StripNumberSuffix(baseMessage);
    }

    private static ScreamerAlertMode GetServerBaselineLocalMode()
    {
        ScreamerAlertMode defaultMode = ScreamerAlertModeSettings.GetServerDefaultMode();
        if (defaultMode == ScreamerAlertMode.OnWithNumbers)
        {
            defaultMode = ScreamerAlertMode.On;
        }

        ScreamerAlertMode storedMode = ScreamerAlertModeSettings.GetModeForLocalPlayer(defaultMode);
        return storedMode == ScreamerAlertMode.Off ? ScreamerAlertMode.Off : ScreamerAlertMode.On;
    }

    public void UpdateScreamerAlertUI()
    {
        XUiC_ScreamerAlerts.Instance?.RefreshBindingsSelfAndChildren();
    }
}
