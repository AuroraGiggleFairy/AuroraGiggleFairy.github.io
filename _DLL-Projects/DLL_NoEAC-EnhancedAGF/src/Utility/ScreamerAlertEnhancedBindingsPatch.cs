using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

public static class ScreamerAlertEnhancedBindingsPatch
{
    private static Type _controllerType;
    private static Type _managerType;
    private static FieldInfo _scoutMessageField;
    private static FieldInfo _hordeMessageField;
    private static FieldInfo _managerInstanceField;
    private static FieldInfo _persistentScreamerIdsField;
    private static FieldInfo _syncedScreamerIdsField;
    private static FieldInfo _persistentHordeIdsField;
    private const float NearbyDistance = 120f;
    private const float CountRefreshIntervalSeconds = 0.2f;
    private static float _nextScoutRefreshAt;
    private static float _nextHordeRefreshAt;
    private static int _cachedScoutCount;
    private static int _cachedHordeCount;

    public static void TryInstall(Harmony harmony)
    {
        try
        {
            _controllerType = AccessTools.TypeByName("ScreamerAlertsController");
            if (_controllerType == null)
            {
                Logging.Inform("ScreamerAlertEnhancedBindings", "ScreamerAlertsController not found; enhanced runtime patch not installed.");
                return;
            }

            _scoutMessageField = AccessTools.Field(_controllerType, "screamerAlertMessage");
            _hordeMessageField = AccessTools.Field(_controllerType, "screamerHordeAlertMessage");

            _managerType = AccessTools.TypeByName("ScreamerAlertManager");
            _managerInstanceField = _managerType != null ? AccessTools.Field(_managerType, "Instance") : null;
            _persistentScreamerIdsField = _managerType != null ? AccessTools.Field(_managerType, "persistentScreamerIds") : null;
            _syncedScreamerIdsField = _managerType != null ? AccessTools.Field(_managerType, "syncedScreamerIds") : null;
            _persistentHordeIdsField = _managerType != null ? AccessTools.Field(_managerType, "persistentHordeZombieIds") : null;

            MethodInfo scoutGetter = AccessTools.Method(_controllerType, "GetScreamerAlertMessage");
            MethodInfo hordeGetter = AccessTools.Method(_controllerType, "GetScreamerHordeAlertMessage");
            if (scoutGetter == null && hordeGetter == null)
            {
                Logging.Inform("ScreamerAlertEnhancedBindings", "Screamer getter methods not found; enhanced runtime patch not installed.");
                return;
            }

            int patchedCount = 0;
            if (scoutGetter != null)
            {
                MethodInfo scoutPostfix = AccessTools.Method(typeof(ScreamerAlertEnhancedBindingsPatch), nameof(GetScreamerAlertMessagePostfix));
                harmony.Patch(scoutGetter, postfix: new HarmonyMethod(scoutPostfix));
                patchedCount++;
            }

            if (hordeGetter != null)
            {
                MethodInfo hordePostfix = AccessTools.Method(typeof(ScreamerAlertEnhancedBindingsPatch), nameof(GetScreamerHordeAlertMessagePostfix));
                harmony.Patch(hordeGetter, postfix: new HarmonyMethod(hordePostfix));
                patchedCount++;
            }

            Logging.Inform("ScreamerAlertEnhancedBindings", "Enhanced runtime Screamer mode patch installed on " + patchedCount + " method(s).");
        }
        catch (Exception ex)
        {
            Logging.Warning("ScreamerAlertEnhancedBindings", "Failed to install runtime patch: " + ex.Message);
        }
    }

    public static void GetScreamerAlertMessagePostfix(object __instance, ref string __result)
    {
        try
        {
            ApplyModeToResult(__instance, _scoutMessageField, false, ref __result);
        }
        catch (Exception ex)
        {
            Logging.Warning("ScreamerAlertEnhancedBindings", "Postfix failed: " + ex.Message);
        }
    }

    public static void GetScreamerHordeAlertMessagePostfix(object __instance, ref string __result)
    {
        try
        {
            ApplyModeToResult(__instance, _hordeMessageField, true, ref __result);
        }
        catch (Exception ex)
        {
            Logging.Warning("ScreamerAlertEnhancedBindings", "Postfix failed: " + ex.Message);
        }
    }

    private static void ApplyModeToResult(object controllerInstance, FieldInfo rawField, bool isHorde, ref string result)
    {
        if (!ScreamerAlertEnhancedGate.ShouldApplyRuntimeBehavior())
        {
            return;
        }

        ScreamerAlertMode mode = ScreamerAlertModeSettings.GetModeForLocalPlayer(ScreamerAlertMode.OnWithNumbers);
        if (mode == ScreamerAlertMode.Off)
        {
            result = string.Empty;
            return;
        }

        if (mode == ScreamerAlertMode.On)
        {
            result = ScreamerAlertModeSettings.StripNumberSuffix(result);
            return;
        }

        string rawMessage = result;
        if (controllerInstance != null && rawField != null)
        {
            string reflectedRaw = rawField.GetValue(controllerInstance) as string;
            if (reflectedRaw != null)
            {
                rawMessage = reflectedRaw;
            }
        }

        if (string.IsNullOrEmpty(rawMessage))
        {
            result = string.Empty;
            return;
        }

        int nearbyCount = isHorde ? GetNearbyHordeCount() : GetNearbyScoutCount();
        result = AppendCountSuffix(rawMessage, nearbyCount);
    }

    private static int GetNearbyScoutCount()
    {
        float now = Time.time;
        if (now < _nextScoutRefreshAt)
        {
            return _cachedScoutCount;
        }

        _nextScoutRefreshAt = now + CountRefreshIntervalSeconds;

        EntityPlayerLocal localPlayer = GameManager.Instance?.World?.GetPrimaryPlayer();
        if (localPlayer == null)
        {
            _cachedScoutCount = 0;
            return 0;
        }

        int countFromManager = CountNearbyIds(ReadPreferredScreamerIds(), localPlayer);
        if (countFromManager >= 0)
        {
            _cachedScoutCount = countFromManager;
            return _cachedScoutCount;
        }

        _cachedScoutCount = 0;
        return _cachedScoutCount;
    }

    private static int GetNearbyHordeCount()
    {
        float now = Time.time;
        if (now < _nextHordeRefreshAt)
        {
            return _cachedHordeCount;
        }

        _nextHordeRefreshAt = now + CountRefreshIntervalSeconds;

        EntityPlayerLocal localPlayer = GameManager.Instance?.World?.GetPrimaryPlayer();
        if (localPlayer == null)
        {
            _cachedHordeCount = 0;
            return 0;
        }

        int count = CountNearbyIds(ReadIdSet(_persistentHordeIdsField), localPlayer);
        if (count >= 0)
        {
            _cachedHordeCount = count;
            return _cachedHordeCount;
        }

        _cachedHordeCount = 0;
        return _cachedHordeCount;
    }

    private static IEnumerable<int> ReadPreferredScreamerIds()
    {
        if (ConnectionManager.Instance != null && ConnectionManager.Instance.IsServer)
        {
            IEnumerable<int> serverIds = ReadIdSet(_persistentScreamerIdsField);
            if (serverIds != null)
            {
                return serverIds;
            }
        }

        IEnumerable<int> syncedIds = ReadIdSet(_syncedScreamerIdsField);
        if (syncedIds != null)
        {
            return syncedIds;
        }

        return ReadIdSet(_persistentScreamerIdsField);
    }

    private static IEnumerable<int> ReadIdSet(FieldInfo setField)
    {
        object manager = _managerInstanceField != null ? _managerInstanceField.GetValue(null) : null;
        if (manager == null || setField == null)
        {
            return null;
        }

        object setObject = setField.GetValue(manager);
        if (setObject is IEnumerable<int> typedEnumerable)
        {
            return typedEnumerable;
        }

        if (setObject is IEnumerable enumerable)
        {
            return EnumerateIds(enumerable);
        }

        return null;
    }

    private static IEnumerable<int> EnumerateIds(IEnumerable source)
    {
        foreach (object item in source)
        {
            if (item is int id)
            {
                yield return id;
            }
            else if (item != null && int.TryParse(item.ToString(), out int parsed))
            {
                yield return parsed;
            }
        }
    }

    private static int CountNearbyIds(IEnumerable<int> ids, EntityPlayerLocal localPlayer)
    {
        if (ids == null)
        {
            return -1;
        }

        var worldEntities = GameManager.Instance?.World?.Entities;
        if (worldEntities == null)
        {
            return 0;
        }

        int count = 0;
        foreach (int id in ids)
        {
            if (!worldEntities.dict.TryGetValue(id, out Entity entity) || entity == null || entity.IsDead())
            {
                continue;
            }

            if (Vector3.Distance(localPlayer.position, entity.position) <= NearbyDistance)
            {
                count++;
            }
        }

        return count;
    }

    private static string AppendCountSuffix(string text, int count)
    {
        string baseText = ScreamerAlertModeSettings.StripNumberSuffix(text);
        if (string.IsNullOrEmpty(baseText))
        {
            return string.Empty;
        }

        if (count < 0)
        {
            count = 0;
        }

        return baseText + " [FFFFFF](" + count + ")[-]";
    }
}
