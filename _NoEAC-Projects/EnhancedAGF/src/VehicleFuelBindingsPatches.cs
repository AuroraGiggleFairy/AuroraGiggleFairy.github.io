using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;

[HarmonyPatch]
public static class VehicleFuelBindingsPatches
{
    private const float FuelDisplayScale = 25f;
    private const float PitchPixelsPerDegree = 0.67f;
    private const float PitchMaxPixelOffset = 40f;
    private const float SpeedNeedleMinAngle = 180f;
    private const float SpeedNeedleMaxAngle = 90f;
    private const float GroundClearanceSampleIntervalSeconds = 0.15f;
    private const float GroundClearanceReferenceDeltaMeters = 0.5f;

    private const string DurabilityCurrent = "agfVehicleDurabilityCurrent";
    private const string DurabilityMax = "agfVehicleDurabilityMax";
    private const string DurabilityCurrentWithMax = "agfVehicleDurabilityCurrentWithMax";
    private const string DurabilityPercent = "agfVehicleDurabilityPercent";

    private const string SpeedCurrent = "agfVehicleSpeedCurrent";
    private const string SpeedMax = "agfVehicleSpeedMax";
    private const string SpeedCurrentWithMax = "agfVehicleSpeedCurrentWithMax";
    private const string SpeedFill = "agfVehicleSpeedFill";
    private const string ReverseSpeedCurrent = "agfVehicleReverseSpeedCurrent";
    private const string ReverseSpeedMax = "agfVehicleReverseSpeedMax";
    private const string ReverseSpeedCurrentWithMax = "agfVehicleReverseSpeedCurrentWithMax";
    private const string ReverseSpeedFill = "agfVehicleReverseSpeedFill";

    private const string TurboVisible = "agfVehicleTurboVisible";
    private const string CruiseVisible = "agfVehicleCruiseVisible";
    private const string PlayerMountedVisible = "agfVehiclePlayerMountedVisible";
    private const string FuelCurrent = "agfVehicleFuelCurrent";
    private const string FuelMax = "agfVehicleFuelMax";
    private const string FuelCurrentWithMax = "agfVehicleFuelCurrentWithMax";
    private const string FuelPercent = "agfVehicleFuelPercent";
    private const string PitchDegrees = "agfVehiclePitchDegrees";
    private const string RollDegrees = "agfVehicleRollDegrees";
    private const string FlightAssistVisible = "agfVehicleFlightAssistVisible";
    private const string FlightAssistModeText = "agfVehicleFlightAssistModeText";
    private const string FlightAssistOffVisible = "agfVehicleFlightAssistOffVisible";
    private const string FlightAssistHoverVisible = "agfVehicleFlightAssistHoverVisible";
    private const string FlightAssistYLockVisible = "agfVehicleFlightAssistYLockVisible";
    private const string GroundClearanceMeters = "agfVehicleGroundClearanceMeters";
    private const string GroundClearanceDebug = "agfVehicleGroundClearanceDebug";

    private static bool terrainHeightLookupResolved;
    private static readonly List<MethodInfo> terrainHeightMethods = new List<MethodInfo>();
    private static bool worldBlockLookupResolved;
    private static MethodInfo worldGetBlockMethod;
    private static bool worldGetBlockUsesVector3i;
    private static Type worldVector3iType;
    private static ConstructorInfo worldVector3iCtor;
    private static bool worldHeightLookupResolved;
    private static readonly List<MethodInfo> worldGetHeightMethods = new List<MethodInfo>();
    private static bool distanceToGroundLookupResolved;
    private static MethodInfo entityDistanceToGroundMethod;
    private static MethodInfo vehicleDistanceToGroundMethod;
    private static int lastGroundClearanceVehicleKey = int.MinValue;
    private static int lastGroundClearanceColumnX = int.MinValue;
    private static int lastGroundClearanceColumnZ = int.MinValue;
    private static float lastGroundClearanceReferenceY = float.NaN;
    private static float lastGroundClearanceMeters;
    private static float lastGroundClearanceSampleTime = float.NegativeInfinity;
    private static float lastGroundClearanceWorldY = float.NaN;
    private static float lastGroundClearanceGroundY = float.NaN;
    private static float lastGroundClearanceNativeDistance = float.NaN;
    private static string lastGroundClearanceSource = "none";
    private static string lastWorldHeightProbe = "n/a";
    private static int lastValidGroundVehicleKey = int.MinValue;
    private static float lastValidGroundHeight = float.NaN;
    private const string AutoRunVehicleEnabledCVar = "agf_autorun_vehicle_enabled";
    private static bool getCVarMethodResolved;
    private static MethodInfo getCVarMethod;

    private enum FlightAssistMode
    {
        Off,
        Hover,
        YLock
    }

    [HarmonyPatch(typeof(XUiC_VehicleFrameWindow), "GetBindingValueInternal")]
    [HarmonyPrefix]
    private static bool VehicleFrameFuelBindingsPrefix(ref bool __result, XUiC_VehicleFrameWindow __instance, ref string value, string bindingName)
    {
        if (string.Equals(bindingName, PlayerMountedVisible, System.StringComparison.OrdinalIgnoreCase))
        {
            EntityPlayerLocal localPlayer = __instance?.xui?.playerUI?.entityPlayer;
            value = IsPlayerMounted(localPlayer) ? "true" : "false";
            __result = true;
            return false;
        }

        EntityVehicle entityVehicle = __instance?.Vehicle;
        if (!TryResolveVehicleWindowBinding(entityVehicle?.GetVehicle(), entityVehicle, bindingName, out value))
        {
            return true;
        }

        __result = true;
        return false;
    }

    [HarmonyPatch(typeof(XUiC_VehicleStats), "GetBindingValueInternal")]
    [HarmonyPrefix]
    private static bool VehicleStatsFuelBindingsPrefix(ref bool __result, XUiC_VehicleStats __instance, ref string value, string bindingName)
    {
        if (string.Equals(bindingName, PlayerMountedVisible, System.StringComparison.OrdinalIgnoreCase))
        {
            EntityPlayerLocal localPlayer = __instance?.xui?.playerUI?.entityPlayer;
            value = IsPlayerMounted(localPlayer) ? "true" : "false";
            __result = true;
            return false;
        }

        EntityVehicle entityVehicle = __instance?.xui?.vehicle;
        Vehicle vehicle = entityVehicle?.GetVehicle();
        if (!TryResolveVehicleWindowBinding(vehicle, entityVehicle, bindingName, out value))
        {
            return true;
        }

        __result = true;
        return false;
    }

    [HarmonyPatch(typeof(XUiC_HUDStatBar), "GetBindingValueInternal")]
    [HarmonyPrefix]
    private static bool HudVehicleFuelBindingsPrefix(ref bool __result, XUiC_HUDStatBar __instance, ref string _value, string _bindingName)
    {
        if (string.Equals(_bindingName, PlayerMountedVisible, System.StringComparison.OrdinalIgnoreCase))
        {
            EntityPlayerLocal localPlayer = __instance?.localPlayer ?? __instance?.xui?.playerUI?.entityPlayer;
            _value = IsPlayerMounted(localPlayer) ? "true" : "false";
            __result = true;
            return false;
        }

        if (__instance == null || !IsAgfVehicleBinding(_bindingName))
        {
            return true;
        }

        EntityPlayerLocal localPlayerForVehicle = __instance.localPlayer ?? __instance.xui?.playerUI?.entityPlayer;
        EntityVehicle entityVehicle = (localPlayerForVehicle?.AttachedToEntity as EntityVehicle) ?? __instance.vehicle;
        Vehicle vehicle = entityVehicle?.GetVehicle();

        if (!TryResolveVehicleWindowBinding(vehicle, entityVehicle, _bindingName, out _value))
        {
            return true;
        }

        __result = true;
        return false;
    }

    [HarmonyPatch(typeof(XUiC_HUDStatBar), "Update")]
    [HarmonyPostfix]
    private static void HudStatBarUpdatePostfix(XUiC_HUDStatBar __instance)
    {
        if (__instance == null || __instance.statGroup != HUDStatGroups.Vehicle)
        {
            return;
        }

        EntityPlayerLocal localPlayer = __instance.localPlayer ?? __instance.xui?.playerUI?.entityPlayer;
        ApplyVehicleAttitudeIndicator(__instance, localPlayer);
    }

    [HarmonyPatch(typeof(XUiC_HUDStatBar), "hasChanged")]
    [HarmonyPostfix]
    private static void HudVehicleHasChangedPostfix(XUiC_HUDStatBar __instance, ref bool __result)
    {
        if (__result || __instance == null)
        {
            return;
        }

        if (__instance.statGroup != HUDStatGroups.Vehicle)
        {
            return;
        }

        EntityPlayerLocal localPlayer = __instance.localPlayer ?? __instance.xui?.playerUI?.entityPlayer;
        if (localPlayer?.AttachedToEntity is EntityVehicle)
        {
            // Keep vehicle HUD bindings hot while mounted so AGF speed bindings do not stall
            // behind fuel-only refresh triggers.
            __result = true;
        }
    }

    private static bool TryResolveVehicleWindowBinding(Vehicle vehicle, EntityVehicle entityVehicle, string bindingName, out string value)
    {
        value = null;
        if (string.IsNullOrEmpty(bindingName))
        {
            return false;
        }

        switch (bindingName)
        {
            case DurabilityCurrent:
                value = ((int)(vehicle?.GetHealth() ?? 0f)).ToString();
                return true;
            case DurabilityMax:
                value = ((int)(vehicle?.GetMaxHealth() ?? 0f)).ToString();
                return true;
            case DurabilityCurrentWithMax:
            {
                int current = (int)(vehicle?.GetHealth() ?? 0f);
                int max = (int)(vehicle?.GetMaxHealth() ?? 0f);
                value = current + "/" + max;
                return true;
            }
            case DurabilityPercent:
                value = ((int)((vehicle?.GetHealthPercent() ?? 0f) * 100f)).ToString();
                return true;

            case SpeedCurrent:
                value = FormatSpeedValue(GetCurrentSpeed(vehicle, entityVehicle), vehicle?.MaxPossibleSpeed ?? 0f);
                return true;
            case SpeedMax:
                value = ((int)(vehicle?.MaxPossibleSpeed ?? 0f)).ToString();
                return true;
            case SpeedCurrentWithMax:
            {
                float maxSpeed = vehicle?.MaxPossibleSpeed ?? 0f;
                string current = FormatSpeedValue(GetCurrentSpeed(vehicle, entityVehicle), maxSpeed);
                int max = (int)maxSpeed;
                value = current + "/" + max;
                return true;
            }
            case SpeedFill:
            {
                float current = GetCurrentSpeed(vehicle, entityVehicle);
                float max = vehicle?.MaxPossibleSpeed ?? 0f;
                value = (max > 0f ? Mathf.Clamp01(current / max) : 0f).ToCultureInvariantString();
                return true;
            }
            case ReverseSpeedCurrent:
                value = FormatSpeedValue(GetCurrentReverseSpeed(vehicle, entityVehicle), GetReverseSpeedMax(vehicle));
                return true;
            case ReverseSpeedMax:
                value = ((int)GetReverseSpeedMax(vehicle)).ToString();
                return true;
            case ReverseSpeedCurrentWithMax:
            {
                float maxReverseSpeed = GetReverseSpeedMax(vehicle);
                string current = FormatSpeedValue(GetCurrentReverseSpeed(vehicle, entityVehicle), maxReverseSpeed);
                int max = (int)maxReverseSpeed;
                value = current + "/" + max;
                return true;
            }
            case ReverseSpeedFill:
            {
                float current = GetCurrentReverseSpeed(vehicle, entityVehicle);
                float max = GetReverseSpeedMax(vehicle);
                value = (max > 0f ? Mathf.Clamp01(current / max) : 0f).ToCultureInvariantString();
                return true;
            }

            case TurboVisible:
                value = ((vehicle != null) && vehicle.IsTurbo) ? "true" : "false";
                return true;
            case CruiseVisible:
            {
                EntityPlayerLocal player = entityVehicle?.AttachedMainEntity as EntityPlayerLocal;
                value = IsTruthy(ReadPlayerCVar(player, AutoRunVehicleEnabledCVar)) ? "true" : "false";
                return true;
            }

            case FuelCurrent:
                value = FormatFuelCount(vehicle?.GetFuelLevel() ?? 0f);
                return true;
            case FuelMax:
                value = FormatFuelCount(vehicle?.GetMaxFuelLevel() ?? 0f);
                return true;
            case FuelCurrentWithMax:
            {
                string current = FormatFuelCount(vehicle?.GetFuelLevel() ?? 0f);
                string max = FormatFuelCount(vehicle?.GetMaxFuelLevel() ?? 0f);
                value = current + "/" + max;
                return true;
            }
            case FuelPercent:
                value = ((int)((vehicle?.GetFuelPercent() ?? 0f) * 100f)).ToString();
                return true;
            case PitchDegrees:
                value = Mathf.RoundToInt(GetVehiclePitchDegreesForDisplay(entityVehicle)).ToString(System.Globalization.CultureInfo.InvariantCulture);
                return true;
            case RollDegrees:
                value = Mathf.RoundToInt(GetVehicleRollDegrees(entityVehicle)).ToString(System.Globalization.CultureInfo.InvariantCulture);
                return true;
            case FlightAssistVisible:
                value = IsFlightAssistVehicle(entityVehicle) ? "true" : "false";
                return true;
            case FlightAssistModeText:
            {
                FlightAssistMode mode = ResolveFlightAssistMode(entityVehicle);
                value = mode == FlightAssistMode.Hover ? "Hover" : (mode == FlightAssistMode.YLock ? "Y-Lock" : "Off");
                return true;
            }
            case FlightAssistOffVisible:
                value = ResolveFlightAssistMode(entityVehicle) == FlightAssistMode.Off ? "true" : "false";
                return true;
            case FlightAssistHoverVisible:
                value = ResolveFlightAssistMode(entityVehicle) == FlightAssistMode.Hover ? "true" : "false";
                return true;
            case FlightAssistYLockVisible:
                value = ResolveFlightAssistMode(entityVehicle) == FlightAssistMode.YLock ? "true" : "false";
                return true;
            case GroundClearanceMeters:
                value = Mathf.RoundToInt(GetVehicleGroundClearanceMeters(entityVehicle)).ToString(System.Globalization.CultureInfo.InvariantCulture);
                return true;
            case GroundClearanceDebug:
                value = GetVehicleGroundClearanceDebug(entityVehicle);
                return true;
            default:
                return false;
        }
    }

    private static string FormatFuelCount(float internalFuelValue)
    {
        return ((int)(internalFuelValue * FuelDisplayScale)).ToString();
    }

    private static string FormatSpeedValue(float speedMetersPerSecond, float maxSpeedMetersPerSecond)
    {
        float speed = Mathf.Max(0f, speedMetersPerSecond);
        float max = Mathf.Max(0f, maxSpeedMetersPerSecond);

        int rounded;
        if (max > 0f && speed > max)
        {
            // Above nominal max speed (falling/physics) uses normal rounding.
            rounded = Mathf.RoundToInt(speed);
        }
        else
        {
            // Within nominal range, floor by default and only round up at .85+.
            int floored = Mathf.FloorToInt(speed);
            float fractional = speed - floored;

            // Special low-speed case: show 1 when moving at least 0.25 m/s.
            if (floored == 0 && speed >= 0.25f)
            {
                rounded = 1;
            }
            else
            {
                rounded = (fractional >= 0.85f) ? floored + 1 : floored;
            }
        }

        return rounded.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private static bool IsAgfVehicleBinding(string bindingName)
    {
        if (string.IsNullOrEmpty(bindingName))
        {
            return false;
        }

        switch (bindingName)
        {
            case DurabilityCurrent:
            case DurabilityMax:
            case DurabilityCurrentWithMax:
            case DurabilityPercent:
            case SpeedCurrent:
            case SpeedMax:
            case SpeedCurrentWithMax:
            case SpeedFill:
            case ReverseSpeedCurrent:
            case ReverseSpeedMax:
            case ReverseSpeedCurrentWithMax:
            case ReverseSpeedFill:
            case TurboVisible:
            case CruiseVisible:
            case FuelCurrent:
            case FuelMax:
            case FuelCurrentWithMax:
            case FuelPercent:
            case PitchDegrees:
            case RollDegrees:
            case FlightAssistVisible:
            case FlightAssistModeText:
            case FlightAssistOffVisible:
            case FlightAssistHoverVisible:
            case FlightAssistYLockVisible:
            case GroundClearanceMeters:
            case GroundClearanceDebug:
            case PlayerMountedVisible:
                return true;
            default:
                return false;
        }
    }

    private static bool IsFlightAssistVehicle(EntityVehicle entityVehicle)
    {
        return FlightAssistReflection.IsLikelyFlyingVehicle(entityVehicle);
    }

    private static float ReadPlayerCVar(EntityPlayerLocal player, string cvarName)
    {
        if (player == null || string.IsNullOrEmpty(cvarName))
        {
            return 0f;
        }

        if (!getCVarMethodResolved)
        {
            getCVarMethod = player.GetType().GetMethod("GetCVar", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(string) }, null);
            getCVarMethodResolved = true;
        }

        if (getCVarMethod == null)
        {
            return 0f;
        }

        try
        {
            object raw = getCVarMethod.Invoke(player, new object[] { cvarName });
            if (raw == null)
            {
                return 0f;
            }

            if (raw is float f)
            {
                return f;
            }

            if (raw is double d)
            {
                return (float)d;
            }

            if (raw is int i)
            {
                return i;
            }

            if (float.TryParse(raw.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed))
            {
                return parsed;
            }
        }
        catch
        {
        }

        return 0f;
    }

    private static bool IsTruthy(float value)
    {
        return value > 0.5f;
    }

    private static FlightAssistMode ResolveFlightAssistMode(EntityVehicle entityVehicle)
    {
        if (!IsFlightAssistVehicle(entityVehicle))
        {
            return FlightAssistMode.Off;
        }

        if (!FlightAssistReflection.TryGetState(entityVehicle, out bool enabled, out bool hardLockActive, out string modeName, out string controlPatternName) || !enabled)
        {
            return FlightAssistMode.Off;
        }

        if (string.IsNullOrEmpty(modeName))
        {
            if (IsHelicopterLikeControlPattern(controlPatternName))
            {
                return hardLockActive ? FlightAssistMode.YLock : FlightAssistMode.Hover;
            }

            // Plane-like patterns should present as y-lock immediately.
            return FlightAssistMode.YLock;
        }

        if (IsHelicopterLikeControlPattern(controlPatternName))
        {
            return string.Equals(modeName, "Hover", StringComparison.OrdinalIgnoreCase)
                ? FlightAssistMode.Hover
                : FlightAssistMode.YLock;
        }

        // Plane-like patterns expose a single flight-assist mode: y-lock.
        return FlightAssistMode.YLock;
    }

    private static bool IsHelicopterLikeControlPattern(string controlPatternName)
    {
        if (string.IsNullOrEmpty(controlPatternName))
        {
            return false;
        }

        return controlPatternName.IndexOf("helicopter", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static class FlightAssistReflection
    {
        private const string PatchTypeName = "FlightLevelAssist.Patch_EntityVehicle_MoveByAttachedEntity";
        private const string StateStoreTypeName = "FlightLevelAssist.FlightLevelStateStore";

        private static Type patchType;
        private static Type stateStoreType;
        private static MethodInfo isLikelyFlyingVehicleMethod;
        private static MethodInfo isEnabledForMethod;
        private static MethodInfo tryGetStateMethod;
        private static FieldInfo stateEnabledField;
        private static FieldInfo stateHardLockActiveField;
        private static FieldInfo stateModeField;
        private static FieldInfo stateControlPatternField;

        internal static bool IsLikelyFlyingVehicle(EntityVehicle vehicle)
        {
            if (vehicle == null)
            {
                return false;
            }

            EnsureResolved();

            if (isLikelyFlyingVehicleMethod != null)
            {
                try
                {
                    return (bool)isLikelyFlyingVehicleMethod.Invoke(null, new object[] { vehicle });
                }
                catch
                {
                }
            }

            try
            {
                Vehicle definition = vehicle.GetVehicle();
                DynamicProperties properties = definition != null ? definition.Properties : null;
                if (properties != null)
                {
                    for (int i = 0; i < 99; i++)
                    {
                        if (!properties.Classes.TryGetValue("force" + i, out DynamicProperties forceProps) || forceProps == null)
                        {
                            continue;
                        }

                        if (!string.IsNullOrEmpty(forceProps.GetString("ceiling")))
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        internal static bool TryGetState(EntityVehicle vehicle, out bool enabled, out bool hardLockActive, out string modeName, out string controlPatternName)
        {
            enabled = false;
            hardLockActive = false;
            modeName = null;
            controlPatternName = null;

            if (vehicle == null)
            {
                return false;
            }

            EnsureResolved();
            bool foundAny = false;

            if (isEnabledForMethod != null)
            {
                try
                {
                    enabled = (bool)isEnabledForMethod.Invoke(null, new object[] { vehicle });
                    foundAny = true;
                }
                catch
                {
                }
            }

            if (tryGetStateMethod != null)
            {
                try
                {
                    object state = tryGetStateMethod.Invoke(null, new object[] { vehicle });
                    if (state != null)
                    {
                        if (stateEnabledField != null)
                        {
                            enabled = (bool)stateEnabledField.GetValue(state);
                            foundAny = true;
                        }

                        if (stateHardLockActiveField != null)
                        {
                            hardLockActive = (bool)stateHardLockActiveField.GetValue(state);
                            foundAny = true;
                        }

                        if (stateModeField != null)
                        {
                            object modeValue = stateModeField.GetValue(state);
                            modeName = modeValue?.ToString();
                            foundAny = true;
                        }

                        if (stateControlPatternField != null)
                        {
                            object patternValue = stateControlPatternField.GetValue(state);
                            controlPatternName = patternValue?.ToString();
                            foundAny = true;
                        }
                    }
                }
                catch
                {
                }
            }

            return foundAny;
        }

        private static void EnsureResolved()
        {
            if (patchType != null && stateStoreType != null)
            {
                return;
            }

            patchType = FindType(PatchTypeName);
            stateStoreType = FindType(StateStoreTypeName);

            if (patchType != null)
            {
                isLikelyFlyingVehicleMethod = patchType.GetMethod("IsLikelyFlyingVehicle", BindingFlags.NonPublic | BindingFlags.Static);
                isEnabledForMethod = patchType.GetMethod("IsEnabledFor", BindingFlags.Public | BindingFlags.Static);
            }

            if (stateStoreType != null)
            {
                tryGetStateMethod = stateStoreType.GetMethod("TryGet", BindingFlags.Public | BindingFlags.Static);
                Type stateType = stateStoreType.GetNestedType("State", BindingFlags.Public | BindingFlags.NonPublic);
                if (stateType != null)
                {
                    stateEnabledField = stateType.GetField("Enabled", BindingFlags.Public | BindingFlags.Instance);
                    stateHardLockActiveField = stateType.GetField("HardLockActive", BindingFlags.Public | BindingFlags.Instance);
                    stateModeField = stateType.GetField("Mode", BindingFlags.Public | BindingFlags.Instance);
                    stateControlPatternField = stateType.GetField("ControlPattern", BindingFlags.Public | BindingFlags.Instance);
                }
            }
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type found = assembly.GetType(fullName, false, false);
                if (found != null)
                {
                    return found;
                }
            }

            return Type.GetType(fullName + ", FlightLevelAssist", false, false);
        }
    }

    private static float GetCurrentSpeed(Vehicle vehicle, EntityVehicle entityVehicle)
    {
        float speedFromVehicleForward = Mathf.Abs(vehicle?.CurrentForwardVelocity ?? 0f);
        float speedFromVehicleVector = vehicle?.CurrentVelocity.magnitude ?? 0f;
        float speedFromEntityVector = 0f;
        float speedFromEntityForward = 0f;

        if (entityVehicle != null)
        {
            Vector3 velocityPerSecond = entityVehicle.GetVelocityPerSecond();
            Vector3 rbVelocity = entityVehicle.GetRBVelocity();

            speedFromEntityVector = Mathf.Max(velocityPerSecond.magnitude, rbVelocity.magnitude);
            if (entityVehicle.transform != null)
            {
                Vector3 forward = entityVehicle.transform.forward;
                speedFromEntityForward = Mathf.Max(Mathf.Abs(Vector3.Dot(velocityPerSecond, forward)), Mathf.Abs(Vector3.Dot(rbVelocity, forward)));
            }
        }

        return Mathf.Max(Mathf.Max(speedFromVehicleForward, speedFromVehicleVector), Mathf.Max(speedFromEntityVector, speedFromEntityForward));
    }

    private static float GetCurrentReverseSpeed(Vehicle vehicle, EntityVehicle entityVehicle)
    {
        float reverseFromVehicle = Mathf.Max(0f, 0f - (vehicle?.CurrentForwardVelocity ?? 0f));
        float reverseFromEntityVelocity = 0f;
        float reverseFromEntityRb = 0f;

        if (entityVehicle != null && entityVehicle.transform != null)
        {
            Vector3 forward = entityVehicle.transform.forward;
            float forwardFromVelocity = Vector3.Dot(entityVehicle.GetVelocityPerSecond(), forward);
            float forwardFromRb = Vector3.Dot(entityVehicle.GetRBVelocity(), forward);
            reverseFromEntityVelocity = Mathf.Max(0f, 0f - forwardFromVelocity);
            reverseFromEntityRb = Mathf.Max(0f, 0f - forwardFromRb);
        }

        return Mathf.Max(reverseFromVehicle, Mathf.Max(reverseFromEntityVelocity, reverseFromEntityRb));
    }

    private static float GetReverseSpeedMax(Vehicle vehicle)
    {
        if (vehicle == null)
        {
            return 0f;
        }

        return Mathf.Max(Mathf.Abs(vehicle.VelocityMaxBackward), Mathf.Abs(vehicle.VelocityMaxTurboBackward));
    }

    private static float GetVehiclePitchDegrees(EntityVehicle entityVehicle)
    {
        if (entityVehicle?.transform == null)
        {
            return 0f;
        }

        return NormalizeSignedDegrees(entityVehicle.transform.eulerAngles.x);
    }

    private static float GetVehiclePitchDegreesForDisplay(EntityVehicle entityVehicle)
    {
        // Keep HUD text convention intuitive: nose-down is negative, nose-up is positive.
        return 0f - GetVehiclePitchDegrees(entityVehicle);
    }

    private static float GetVehicleRollDegrees(EntityVehicle entityVehicle)
    {
        if (entityVehicle?.transform == null)
        {
            return 0f;
        }

        return NormalizeSignedDegrees(entityVehicle.transform.eulerAngles.z);
    }

    private static float NormalizeSignedDegrees(float eulerDegrees)
    {
        float normalized = eulerDegrees % 360f;
        if (normalized > 180f)
        {
            normalized -= 360f;
        }

        if (normalized < -180f)
        {
            normalized += 360f;
        }

        return normalized;
    }

    private static float GetVehicleGroundClearanceMeters(EntityVehicle entityVehicle)
    {
        if (entityVehicle?.transform == null)
        {
            lastGroundClearanceWorldY = float.NaN;
            lastGroundClearanceReferenceY = float.NaN;
            lastGroundClearanceGroundY = float.NaN;
            lastGroundClearanceNativeDistance = float.NaN;
            lastGroundClearanceSource = "no-vehicle";
            return 0f;
        }

        Vector3 worldPos = entityVehicle.position;
        if (!IsFinite(worldPos.x) || !IsFinite(worldPos.y) || !IsFinite(worldPos.z))
        {
            worldPos = entityVehicle.transform.position;
        }
        float referenceY = GetVehicleGroundReferenceY(entityVehicle, worldPos.y);
        // Some vehicle bounds providers return non-world values; keep reference close to actual world Y.
        if (referenceY < worldPos.y - 10f || referenceY > worldPos.y + 5f)
        {
            referenceY = worldPos.y;
        }
        int columnX = Mathf.FloorToInt(worldPos.x);
        int columnZ = Mathf.FloorToInt(worldPos.z);
        int vehicleKey = entityVehicle.GetHashCode();
        float sampleTime = Time.realtimeSinceStartup;

        bool sameVehicle = vehicleKey == lastGroundClearanceVehicleKey;
        bool sameColumn = columnX == lastGroundClearanceColumnX && columnZ == lastGroundClearanceColumnZ;
        bool sameReferenceBand = !float.IsNaN(lastGroundClearanceReferenceY)
            && Mathf.Abs(referenceY - lastGroundClearanceReferenceY) <= GroundClearanceReferenceDeltaMeters;
        bool sampleIsFresh = (sampleTime - lastGroundClearanceSampleTime) < GroundClearanceSampleIntervalSeconds;

        if (sameVehicle && sameColumn && sameReferenceBand && sampleIsFresh)
        {
            if (!string.IsNullOrEmpty(lastGroundClearanceSource) && !lastGroundClearanceSource.EndsWith("*", StringComparison.Ordinal))
            {
                lastGroundClearanceSource = lastGroundClearanceSource + "*";
            }
            return lastGroundClearanceMeters;
        }

        // Terrain-column AGL: follows terrain edits/holes while ignoring structure blocks.
        float groundHeight = TryGetTerrainColumnSurfaceHeight(worldPos.x, worldPos.z, referenceY);
        float nativeDistance = float.NaN;
        string source = "terrain-column";
        if (float.IsNaN(groundHeight))
        {
            // Fallback for worlds where terrain block probing is unavailable.
            groundHeight = TryGetTerrainHeight(worldPos.x, worldPos.z, referenceY);
            source = float.IsNaN(groundHeight) ? "terrain-none" : "terrain-method";
        }

        if (float.IsNaN(groundHeight))
        {
            // Robust fallback: first solid block surface below the vehicle, regardless of terrain classification.
            groundHeight = TryGetBlockSurfaceHeight(worldPos.x, worldPos.z, referenceY);
            if (!float.IsNaN(groundHeight))
            {
                source = "block-solid";
            }
        }

        if (float.IsNaN(groundHeight))
        {
            // Deterministic fallback used widely by game code for terrain sampling.
            groundHeight = TryGetWorldHeight(worldPos.x, worldPos.z, referenceY);
            if (!float.IsNaN(groundHeight))
            {
                source = "world-height";
            }
            else
            {
                source = "world-height-none";
            }
        }

        if (float.IsNaN(groundHeight) && TryGetNativeDistanceToGroundMeters(entityVehicle, out nativeDistance))
        {
            // Ignore suspicious zero-distance fallbacks while clearly above map origin.
            if (nativeDistance > 0.05f || referenceY <= 2f)
            {
                groundHeight = referenceY - Mathf.Max(0f, nativeDistance);
                source = "native-distance";
            }
            else
            {
                source = "native-rejected-zero";
            }
        }

        if (float.IsNaN(groundHeight)
            && vehicleKey == lastValidGroundVehicleKey
            && !float.IsNaN(lastValidGroundHeight))
        {
            groundHeight = lastValidGroundHeight;
            source = "cached-ground";
        }

        if (float.IsNaN(groundHeight) && IsVehicleLikelyGrounded(entityVehicle, referenceY))
        {
            // Final safety net: while grounded and stationary, seed ground at underside reference.
            groundHeight = referenceY;
            source = "assume-grounded";
        }

        if (!float.IsNaN(groundHeight))
        {
            lastValidGroundVehicleKey = vehicleKey;
            lastValidGroundHeight = groundHeight;
        }

        float rawClearance = float.IsNaN(groundHeight) ? 0f : Mathf.Max(0f, referenceY - groundHeight);
        float clearance = StabilizeLowClearance(rawClearance);

        lastGroundClearanceVehicleKey = vehicleKey;
        lastGroundClearanceColumnX = columnX;
        lastGroundClearanceColumnZ = columnZ;
        lastGroundClearanceWorldY = worldPos.y;
        lastGroundClearanceReferenceY = referenceY;
        lastGroundClearanceGroundY = groundHeight;
        lastGroundClearanceNativeDistance = nativeDistance;
        lastGroundClearanceSource = source;
        lastGroundClearanceMeters = clearance;
        lastGroundClearanceSampleTime = sampleTime;

        return clearance;
    }

    private static string GetVehicleGroundClearanceDebug(EntityVehicle entityVehicle)
    {
        float clearance = GetVehicleGroundClearanceMeters(entityVehicle);
        return string.Format(
            System.Globalization.CultureInfo.InvariantCulture,
            "src:{0} y:{1:0.0} r:{2:0.0} g:{3} nd:{4} a:{5:0.00}",
            lastGroundClearanceSource,
            lastGroundClearanceWorldY,
            lastGroundClearanceReferenceY,
            FormatDebugFloat(lastGroundClearanceGroundY, "0.0"),
                lastWorldHeightProbe,
            clearance);
    }

    private static string FormatDebugFloat(float value, string format)
    {
        if (float.IsNaN(value))
        {
            return "nan";
        }

        if (float.IsInfinity(value))
        {
            return value > 0f ? "inf" : "-inf";
        }

        return value.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private static float StabilizeLowClearance(float clearance)
    {
        if (clearance < 1.25f)
        {
            return clearance < 0.25f ? 0f : 1f;
        }

        return clearance;
    }

    private static float ResolveGroundHeightCandidate(float referenceY, float terrainHeight, float blockHeight)
    {
        bool terrainValid = !float.IsNaN(terrainHeight) && terrainHeight <= referenceY + 2f;
        bool blockValid = !float.IsNaN(blockHeight) && blockHeight <= referenceY + 2f;

        // Guard against baseline-like terrain responses (e.g. near 0 while vehicle is much higher).
        if (terrainValid && !blockValid && referenceY > 12f && terrainHeight <= 2f)
        {
            terrainValid = false;
        }

        if (!terrainValid && !blockValid)
        {
            return float.NaN;
        }

        if (terrainValid && blockValid)
        {
            // Prefer the highest valid ground under the vehicle (closest from below).
            return Mathf.Max(terrainHeight, blockHeight);
        }

        return terrainValid ? terrainHeight : blockHeight;
    }

    private static float GetVehicleGroundReferenceY(EntityVehicle entityVehicle, float defaultY)
    {
        if (TryGetBoundsMinY(entityVehicle, out float minY))
        {
            return minY;
        }

        if (TryGetVehicleHeight(entityVehicle, out float height) && height > 0f)
        {
            return defaultY - (height * 0.5f);
        }

        return defaultY;
    }

    private static bool TryGetBoundsMinY(EntityVehicle entityVehicle, out float minY)
    {
        minY = 0f;

        object boundsObj = GetMemberValue(entityVehicle, "boundingBox")
            ?? GetMemberValue(entityVehicle, "BoundingBox")
            ?? GetMemberValue(entityVehicle, "bounds")
            ?? GetMemberValue(entityVehicle, "Bounds");
        if (boundsObj == null)
        {
            return false;
        }

        if (boundsObj is Bounds unityBounds)
        {
            minY = unityBounds.min.y;
            return true;
        }

        object minObj = GetMemberValue(boundsObj, "min") ?? GetMemberValue(boundsObj, "Min");
        if (minObj is Vector3 minVec)
        {
            minY = minVec.y;
            return true;
        }

        if (TryExtractY(minObj, out minY))
        {
            return true;
        }

        return false;
    }

    private static bool TryGetVehicleHeight(EntityVehicle entityVehicle, out float height)
    {
        height = 0f;

        object heightObj = GetMemberValue(entityVehicle, "Height")
            ?? GetMemberValue(entityVehicle, "height");
        if (heightObj != null)
        {
            try
            {
                height = Convert.ToSingle(heightObj, System.Globalization.CultureInfo.InvariantCulture);
                return height > 0f;
            }
            catch
            {
            }
        }

        MethodInfo getHeightMethod = entityVehicle.GetType().GetMethod("GetHeight", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
        if (getHeightMethod != null)
        {
            try
            {
                object raw = getHeightMethod.Invoke(entityVehicle, null);
                height = Convert.ToSingle(raw, System.Globalization.CultureInfo.InvariantCulture);
                return height > 0f;
            }
            catch
            {
            }
        }

        return false;
    }

    private static bool TryGetNativeDistanceToGroundMeters(EntityVehicle entityVehicle, out float distance)
    {
        distance = 0f;

        EnsureDistanceToGroundResolvers(entityVehicle.GetType());

        if (entityDistanceToGroundMethod != null)
        {
            try
            {
                object raw = entityDistanceToGroundMethod.Invoke(entityVehicle, null);
                distance = Convert.ToSingle(raw, System.Globalization.CultureInfo.InvariantCulture);
                if (!float.IsNaN(distance) && !float.IsInfinity(distance))
                {
                    return true;
                }
            }
            catch
            {
            }
        }

        Vehicle vehicle = entityVehicle.GetVehicle();
        if (vehicleDistanceToGroundMethod != null && vehicle != null)
        {
            try
            {
                object raw = vehicleDistanceToGroundMethod.Invoke(vehicle, null);
                distance = Convert.ToSingle(raw, System.Globalization.CultureInfo.InvariantCulture);
                if (!float.IsNaN(distance) && !float.IsInfinity(distance))
                {
                    return true;
                }
            }
            catch
            {
            }
        }

        return false;
    }

    private static bool IsVehicleLikelyGrounded(EntityVehicle entityVehicle, float referenceY)
    {
        if (entityVehicle == null)
        {
            return false;
        }

        if (TryGetGroundedFlag(entityVehicle, out bool grounded))
        {
            return grounded;
        }

        // Heuristic fallback only near local baseline to avoid falsely pinning hover flight to zero AGL.
        if (Mathf.Abs(referenceY) > 6f)
        {
            return false;
        }

        Vector3 velocity = entityVehicle.GetVelocityPerSecond();
        Vector3 rbVelocity = entityVehicle.GetRBVelocity();
        float vertical = Mathf.Min(Mathf.Abs(velocity.y), Mathf.Abs(rbVelocity.y));
        float planar = Mathf.Min(
            new Vector2(velocity.x, velocity.z).magnitude,
            new Vector2(rbVelocity.x, rbVelocity.z).magnitude);

        return vertical <= 0.35f && planar <= 0.9f;
    }

    private static bool TryGetGroundedFlag(EntityVehicle entityVehicle, out bool grounded)
    {
        grounded = false;
        string[] memberCandidates =
        {
            "isOnGround", "IsOnGround", "onGround", "OnGround", "isGrounded", "IsGrounded", "grounded", "Grounded"
        };

        for (int i = 0; i < memberCandidates.Length; i++)
        {
            object value = GetMemberValue(entityVehicle, memberCandidates[i]);
            if (value is bool boolValue)
            {
                grounded = boolValue;
                return true;
            }
        }

        Vehicle vehicle = entityVehicle.GetVehicle();
        if (vehicle != null)
        {
            for (int i = 0; i < memberCandidates.Length; i++)
            {
                object value = GetMemberValue(vehicle, memberCandidates[i]);
                if (value is bool boolValue)
                {
                    grounded = boolValue;
                    return true;
                }
            }
        }

        return false;
    }

    private static void EnsureDistanceToGroundResolvers(Type entityVehicleType)
    {
        if (distanceToGroundLookupResolved)
        {
            return;
        }

        distanceToGroundLookupResolved = true;
        entityDistanceToGroundMethod = null;
        vehicleDistanceToGroundMethod = null;

        if (entityVehicleType != null)
        {
            string[] methodCandidates =
            {
                "GetDistanceToGround",
                "GetDistanceFromGround",
                "DistanceToGround"
            };

            foreach (string methodName in methodCandidates)
            {
                MethodInfo method = entityVehicleType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
                if (method != null)
                {
                    entityDistanceToGroundMethod = method;
                    break;
                }
            }
        }

        Type vehicleType = typeof(Vehicle);
        if (vehicleType != null)
        {
            string[] methodCandidates =
            {
                "GetDistanceToGround",
                "GetDistanceFromGround",
                "DistanceToGround"
            };

            foreach (string methodName in methodCandidates)
            {
                MethodInfo method = vehicleType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
                if (method != null)
                {
                    vehicleDistanceToGroundMethod = method;
                    break;
                }
            }
        }
    }

    private static object GetMemberValue(object instance, string memberName)
    {
        if (instance == null || string.IsNullOrEmpty(memberName))
        {
            return null;
        }

        Type type = instance.GetType();
        PropertyInfo property = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property != null)
        {
            try
            {
                return property.GetValue(instance, null);
            }
            catch
            {
            }
        }

        FieldInfo field = type.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            try
            {
                return field.GetValue(instance);
            }
            catch
            {
            }
        }

        return null;
    }

    private static bool TryExtractY(object instance, out float y)
    {
        y = 0f;
        if (instance == null)
        {
            return false;
        }

        object yObj = GetMemberValue(instance, "y") ?? GetMemberValue(instance, "Y");
        if (yObj == null)
        {
            return false;
        }

        try
        {
            y = Convert.ToSingle(yObj, System.Globalization.CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static float TryGetBlockSurfaceHeight(float x, float z, float referenceY)
    {
        World world = GameManager.Instance?.World;
        if (world == null)
        {
            return float.NaN;
        }

        int ix = Mathf.FloorToInt(x);
        int iz = Mathf.FloorToInt(z);
        int startY = Mathf.Max(1, Mathf.FloorToInt(referenceY + 1f));
        int minY = startY - 192;

        for (int y = startY; y >= minY; y--)
        {
            object blockValue = null;
            bool gotBlock = false;

            try
            {
                blockValue = world.GetBlock(ix, y, iz);
                gotBlock = true;
            }
            catch
            {
                if (!TryInvokeWorldGetBlock(world, ix, y, iz, out blockValue))
                {
                    continue;
                }

                gotBlock = true;
            }

            if (!gotBlock)
            {
                continue;
            }

            if (IsSolidWorldBlock(blockValue))
            {
                return y + 1f;
            }
        }

        return float.NaN;
    }

    private static float TryGetTerrainColumnSurfaceHeight(float x, float z, float referenceY)
    {
        World world = GameManager.Instance?.World;
        if (world == null)
        {
            return float.NaN;
        }

        int ix = Mathf.FloorToInt(x);
        int iz = Mathf.FloorToInt(z);
        int startY = Mathf.Max(1, Mathf.FloorToInt(referenceY + 1f));
        int minY = startY - 512;

        for (int y = startY; y >= minY; y--)
        {
            object blockValue = null;
            bool gotBlock = false;

            try
            {
                blockValue = world.GetBlock(ix, y, iz);
                gotBlock = true;
            }
            catch
            {
                if (!TryInvokeWorldGetBlock(world, ix, y, iz, out blockValue))
                {
                    continue;
                }

                gotBlock = true;
            }

            if (!gotBlock)
            {
                continue;
            }

            if (IsTerrainWorldBlock(blockValue))
            {
                return y + 1f;
            }
        }

        return float.NaN;
    }

    private static void EnsureWorldBlockResolver(Type worldType)
    {
        if (worldBlockLookupResolved)
        {
            return;
        }

        worldBlockLookupResolved = true;
        worldGetBlockMethod = null;
        worldGetBlockUsesVector3i = false;
        worldVector3iType = null;
        worldVector3iCtor = null;

        if (worldType == null)
        {
            return;
        }

        for (Type typeCursor = worldType; typeCursor != null; typeCursor = typeCursor.BaseType)
        {
            MethodInfo[] methods = typeCursor.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (!string.Equals(method.Name, "GetBlock", StringComparison.Ordinal)
                    && !string.Equals(method.Name, "GetBlockNoDamage", StringComparison.Ordinal)
                    && !string.Equals(method.Name, "GetBlockValue", StringComparison.Ordinal))
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length >= 3
                    && IsNumericType(parameters[0].ParameterType)
                    && IsNumericType(parameters[1].ParameterType)
                    && IsNumericType(parameters[2].ParameterType)
                    && SupportsAdditionalDefaultableParameters(parameters, 3))
                {
                    worldGetBlockMethod = method;
                    worldGetBlockUsesVector3i = false;
                    return;
                }

                if (parameters.Length >= 4
                    && IsNumericType(parameters[0].ParameterType)
                    && IsNumericType(parameters[1].ParameterType)
                    && IsNumericType(parameters[2].ParameterType)
                    && parameters[3].ParameterType == typeof(bool)
                    && SupportsAdditionalDefaultableParameters(parameters, 4))
                {
                    worldGetBlockMethod = method;
                    worldGetBlockUsesVector3i = false;
                    return;
                }

                if (parameters.Length >= 1
                    && parameters[0].ParameterType.Name == "Vector3i"
                    && SupportsAdditionalDefaultableParameters(parameters, 1))
                {
                    ConstructorInfo ctor = parameters[0].ParameterType.GetConstructor(new[] { typeof(int), typeof(int), typeof(int) });
                    if (ctor != null)
                    {
                        worldGetBlockMethod = method;
                        worldGetBlockUsesVector3i = true;
                        worldVector3iType = parameters[0].ParameterType;
                        worldVector3iCtor = ctor;
                        return;
                    }
                }
            }
        }
    }

    private static float TryGetWorldHeight(float x, float z, float referenceY)
    {
        World world = GameManager.Instance?.World;
        if (world == null)
        {
            return float.NaN;
        }

        int ix = Mathf.FloorToInt(x);
        int iz = Mathf.FloorToInt(z);
        try
        {
            float terrainGeneratorCandidate = world.GetHeightAt(x, z);
            if (terrainGeneratorCandidate >= -8192f && terrainGeneratorCandidate <= 8192f
                && !(terrainGeneratorCandidate > referenceY + 0.6f))
            {
                lastWorldHeightProbe = "GetHeightAt/2:" + terrainGeneratorCandidate.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                return terrainGeneratorCandidate;
            }

            lastWorldHeightProbe = "GetHeightAt/2-rej:" + terrainGeneratorCandidate.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            lastWorldHeightProbe = "GetHeightAt/2-ex:" + ex.GetType().Name;
        }

        try
        {
            float directCandidate = world.GetHeight(ix, iz);
            if (directCandidate >= -8192f && directCandidate <= 8192f
                && !(directCandidate > referenceY + 0.6f))
            {
                lastWorldHeightProbe = "GetHeight/2:" + directCandidate.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                return directCandidate;
            }

            lastWorldHeightProbe = "GetHeight/2-rej:" + directCandidate.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            lastWorldHeightProbe = "GetHeight/2-ex:" + ex.GetType().Name;
        }

        EnsureWorldHeightResolver(world.GetType());
        if (worldGetHeightMethods.Count == 0)
        {
            if (string.IsNullOrEmpty(lastWorldHeightProbe))
            {
                lastWorldHeightProbe = "no-meth";
            }
            return float.NaN;
        }

        float bestCandidate = float.NaN;
        float bestDistance = float.MaxValue;
        string probe = "none";

        for (int i = 0; i < worldGetHeightMethods.Count; i++)
        {
            MethodInfo method = worldGetHeightMethods[i];
            try
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (!TryBuildTerrainHeightArgs(parameters, x, z, out object[] args))
                {
                    continue;
                }

                object raw = method.Invoke(world, args);
                if (raw == null)
                {
                    continue;
                }

                float candidate = Convert.ToSingle(raw, System.Globalization.CultureInfo.InvariantCulture);
                if (candidate < -8192f || candidate > 8192f)
                {
                    continue;
                }

                // Ground sample should not be above underside reference except tiny tolerance.
                if (candidate > referenceY + 0.6f)
                {
                    continue;
                }

                float distance = Mathf.Abs(referenceY - candidate);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCandidate = candidate;
                    probe = method.Name + "/" + parameters.Length + ":" + candidate.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            catch
            {
            }
        }

        lastWorldHeightProbe = probe;
        return bestCandidate;
    }

    private static void EnsureWorldHeightResolver(Type worldType)
    {
        if (worldHeightLookupResolved)
        {
            return;
        }

        worldHeightLookupResolved = true;
        worldGetHeightMethods.Clear();

        if (worldType == null)
        {
            return;
        }

        for (Type typeCursor = worldType; typeCursor != null; typeCursor = typeCursor.BaseType)
        {
            MethodInfo[] methods = typeCursor.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (!string.Equals(method.Name, "GetHeight", StringComparison.Ordinal) || !IsNumericType(method.ReturnType))
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (SupportsTerrainHeightSignature(parameters))
                {
                    worldGetHeightMethods.Add(method);
                }
            }
        }
    }

    private static bool TryInvokeWorldGetBlock(World world, int x, int y, int z, out object blockValue)
    {
        blockValue = null;
        if (worldGetBlockMethod == null || world == null)
        {
            return false;
        }

        try
        {
            object[] args;
            if (worldGetBlockUsesVector3i)
            {
                object vec = worldVector3iCtor?.Invoke(new object[] { x, y, z });
                if (vec == null)
                {
                    return false;
                }

                ParameterInfo[] parameters = worldGetBlockMethod.GetParameters();
                args = new object[parameters.Length];
                args[0] = vec;
                for (int i = 1; i < parameters.Length; i++)
                {
                    if (!TryCreateDefaultArgument(parameters[i].ParameterType, out object defaultArg))
                    {
                        return false;
                    }

                    args[i] = defaultArg;
                }
            }
            else
            {
                ParameterInfo[] parameters = worldGetBlockMethod.GetParameters();
                args = new object[parameters.Length];
                args[0] = ConvertCoordinate(x, parameters[0].ParameterType);
                args[1] = ConvertCoordinate(y, parameters[1].ParameterType);
                args[2] = ConvertCoordinate(z, parameters[2].ParameterType);
                for (int i = 3; i < parameters.Length; i++)
                {
                    if (!TryCreateDefaultArgument(parameters[i].ParameterType, out object defaultArg))
                    {
                        return false;
                    }

                    args[i] = defaultArg;
                }
            }

            blockValue = worldGetBlockMethod.Invoke(world, args);
            return blockValue != null;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsSolidWorldBlock(object blockValue)
    {
        if (blockValue == null)
        {
            return false;
        }

        object isAirObj = GetMemberValue(blockValue, "isair") ?? GetMemberValue(blockValue, "IsAir");
        if (isAirObj is bool isAir)
        {
            return !isAir;
        }

        object isEmptyObj = GetMemberValue(blockValue, "isempty") ?? GetMemberValue(blockValue, "IsEmpty");
        if (isEmptyObj is bool isEmpty)
        {
            return !isEmpty;
        }

        object typeObj = GetMemberValue(blockValue, "type") ?? GetMemberValue(blockValue, "Type");
        if (typeObj != null)
        {
            try
            {
                return Convert.ToInt32(typeObj, System.Globalization.CultureInfo.InvariantCulture) > 0;
            }
            catch
            {
            }
        }

        object blockObj = GetMemberValue(blockValue, "Block") ?? GetMemberValue(blockValue, "block");
        if (blockObj != null)
        {
            object blockAirObj = GetMemberValue(blockObj, "isair") ?? GetMemberValue(blockObj, "IsAir");
            if (blockAirObj is bool blockIsAir)
            {
                return !blockIsAir;
            }
        }

        return false;
    }

    private static bool IsTerrainWorldBlock(object blockValue)
    {
        if (!IsSolidWorldBlock(blockValue))
        {
            return false;
        }

        object blockObj = GetMemberValue(blockValue, "Block")
            ?? GetMemberValue(blockValue, "block")
            ?? blockValue;

        object isTerrainObj = GetMemberValue(blockObj, "isTerrain")
            ?? GetMemberValue(blockObj, "IsTerrain");
        if (isTerrainObj is bool isTerrain)
        {
            return isTerrain;
        }

        object shapeObj = GetMemberValue(blockObj, "shape")
            ?? GetMemberValue(blockObj, "Shape");
        if (shapeObj != null)
        {
            object shapeTerrainObj = GetMemberValue(shapeObj, "isTerrain")
                ?? GetMemberValue(shapeObj, "IsTerrain");
            if (shapeTerrainObj is bool shapeTerrain)
            {
                return shapeTerrain;
            }

            try
            {
                MethodInfo isTerrainMethod = shapeObj.GetType().GetMethod("IsTerrain", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
                if (isTerrainMethod != null)
                {
                    object terrainResult = isTerrainMethod.Invoke(shapeObj, Array.Empty<object>());
                    if (terrainResult is bool terrainByMethod)
                    {
                        return terrainByMethod;
                    }
                }
            }
            catch
            {
            }
        }

        string blockName = Convert.ToString(
            GetMemberValue(blockObj, "blockName")
            ?? GetMemberValue(blockObj, "BlockName")
            ?? GetMemberValue(blockObj, "Name")
            ?? GetMemberValue(blockValue, "name")
            ?? GetMemberValue(blockValue, "Name"),
            System.Globalization.CultureInfo.InvariantCulture);

        string materialName = Convert.ToString(
            GetMemberValue(GetMemberValue(blockObj, "material") ?? GetMemberValue(blockObj, "Material"), "Name")
            ?? GetMemberValue(blockObj, "material")
            ?? GetMemberValue(blockObj, "Material"),
            System.Globalization.CultureInfo.InvariantCulture);

        string text = ((blockName ?? string.Empty) + " " + (materialName ?? string.Empty)).ToLowerInvariant();
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        string[] terrainTokens =
        {
            "terrain", "dirt", "soil", "grass", "sand", "gravel", "clay", "stone", "rock", "snow", "asphalt"
        };
        for (int i = 0; i < terrainTokens.Length; i++)
        {
            if (text.Contains(terrainTokens[i]))
            {
                return true;
            }
        }

        string[] structureTokens =
        {
            "wood", "metal", "steel", "concrete", "brick", "glass", "door", "window", "frame", "ladder", "fence", "pipe", "plate", "wedge", "stairs", "ramp"
        };
        for (int i = 0; i < structureTokens.Length; i++)
        {
            if (text.Contains(structureTokens[i]))
            {
                return false;
            }
        }

        return false;
    }

    private static float TryGetTerrainHeight(float x, float z, float referenceY)
    {
        World world = GameManager.Instance?.World;
        if (world == null)
        {
            return float.NaN;
        }

        int ix = Mathf.FloorToInt(x);
        int iz = Mathf.FloorToInt(z);
        try
        {
            float directCandidate = world.GetTerrainHeight(ix, iz);
            if (directCandidate >= -8192f && directCandidate <= 8192f
                && !(directCandidate > referenceY + 0.6f))
            {
                return directCandidate;
            }
        }
        catch
        {
        }

        EnsureTerrainHeightResolver(world.GetType());
        if (terrainHeightMethods.Count == 0)
        {
            return float.NaN;
        }

        float bestCandidate = float.NaN;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < terrainHeightMethods.Count; i++)
        {
            if (!TryInvokeTerrainHeightMethod(world, terrainHeightMethods[i], x, z, out float candidate))
            {
                continue;
            }

            if (candidate < -8192f || candidate > 8192f)
            {
                continue;
            }

            // Terrain should generally be at or below the vehicle underside reference.
            if (candidate > referenceY + 0.6f)
            {
                continue;
            }

            float distance = Mathf.Abs(referenceY - candidate);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestCandidate = candidate;
            }
        }

        return bestCandidate;
    }

    private static void EnsureTerrainHeightResolver(Type worldType)
    {
        if (terrainHeightLookupResolved)
        {
            return;
        }

        terrainHeightLookupResolved = true;
        terrainHeightMethods.Clear();

        if (worldType == null)
        {
            return;
        }

        string[] candidateNames =
        {
            "GetTerrainHeight",
            "GetTerrainHeightAt",
            "GetHeight"
        };

        for (Type typeCursor = worldType; typeCursor != null; typeCursor = typeCursor.BaseType)
        {
            MethodInfo[] methods = typeCursor.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                bool candidateName = false;
                for (int j = 0; j < candidateNames.Length; j++)
                {
                    if (string.Equals(method.Name, candidateNames[j], StringComparison.Ordinal))
                    {
                        candidateName = true;
                        break;
                    }
                }

                if (!candidateName || !IsNumericType(method.ReturnType))
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (SupportsTerrainHeightSignature(parameters))
                {
                    terrainHeightMethods.Add(method);
                }
            }
        }
    }

    private static bool TryInvokeTerrainHeightMethod(World world, MethodInfo method, float x, float z, out float value)
    {
        value = float.NaN;

        try
        {
            ParameterInfo[] parameters = method.GetParameters();
            object[] args;

            if (!TryBuildTerrainHeightArgs(parameters, x, z, out args))
            {
                return false;
            }

            object raw = method.Invoke(world, args);
            if (raw == null)
            {
                return false;
            }

            value = Convert.ToSingle(raw, System.Globalization.CultureInfo.InvariantCulture);
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
        catch
        {
            return false;
        }
    }

    private static bool SupportsTerrainHeightSignature(ParameterInfo[] parameters)
    {
        if (parameters == null)
        {
            return false;
        }

        if (parameters.Length == 1)
        {
            string singleName = parameters[0].ParameterType.Name;
            return string.Equals(singleName, "Vector3i", StringComparison.Ordinal)
                || string.Equals(singleName, "Vector2i", StringComparison.Ordinal);
        }

        if (parameters.Length < 2 || !IsNumericType(parameters[0].ParameterType) || !IsNumericType(parameters[1].ParameterType))
        {
            return false;
        }

        for (int i = 2; i < parameters.Length; i++)
        {
            Type type = parameters[i].ParameterType;
            if (!(type == typeof(bool) || IsNumericType(type) || type.IsEnum))
            {
                return false;
            }
        }

        return true;
    }

    private static bool SupportsAdditionalDefaultableParameters(ParameterInfo[] parameters, int startIndex)
    {
        for (int i = startIndex; i < parameters.Length; i++)
        {
            Type paramType = parameters[i].ParameterType;
            if (paramType == typeof(bool) || paramType.IsEnum || IsNumericType(paramType))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private static bool TryCreateDefaultArgument(Type parameterType, out object value)
    {
        value = null;

        if (parameterType == typeof(bool))
        {
            value = false;
            return true;
        }

        if (parameterType.IsEnum)
        {
            Array enumValues = Enum.GetValues(parameterType);
            value = (enumValues != null && enumValues.Length > 0)
                ? enumValues.GetValue(0)
                : Activator.CreateInstance(parameterType);
            return true;
        }

        if (IsNumericType(parameterType))
        {
            value = ConvertCoordinate(0, parameterType);
            return true;
        }

        return false;
    }

    private static bool TryBuildTerrainHeightArgs(ParameterInfo[] parameters, float x, float z, out object[] args)
    {
        args = null;

        if (parameters == null)
        {
            return false;
        }

        if (parameters.Length == 1)
        {
            string singleName = parameters[0].ParameterType.Name;
            if (string.Equals(singleName, "Vector3i", StringComparison.Ordinal))
            {
                args = new object[] { Activator.CreateInstance(parameters[0].ParameterType, Mathf.FloorToInt(x), 0, Mathf.FloorToInt(z)) };
                return args[0] != null;
            }

            if (string.Equals(singleName, "Vector2i", StringComparison.Ordinal))
            {
                args = new object[] { Activator.CreateInstance(parameters[0].ParameterType, Mathf.FloorToInt(x), Mathf.FloorToInt(z)) };
                return args[0] != null;
            }

            return false;
        }

        if (parameters.Length < 2 || !IsNumericType(parameters[0].ParameterType) || !IsNumericType(parameters[1].ParameterType))
        {
            return false;
        }

        args = new object[parameters.Length];
        args[0] = ConvertCoordinate(x, parameters[0].ParameterType);
        args[1] = ConvertCoordinate(z, parameters[1].ParameterType);

        for (int i = 2; i < parameters.Length; i++)
        {
            Type type = parameters[i].ParameterType;
            if (type == typeof(bool))
            {
                args[i] = false;
                continue;
            }

            if (type.IsEnum)
            {
                Array enumValues = Enum.GetValues(type);
                args[i] = (enumValues != null && enumValues.Length > 0)
                    ? enumValues.GetValue(0)
                    : Activator.CreateInstance(type);
                continue;
            }

            if (IsNumericType(type))
            {
                args[i] = ConvertCoordinate(0, type);
                continue;
            }

            return false;
        }

        return true;
    }

    private static object ConvertCoordinate(float coordinate, Type targetType)
    {
        if (targetType == typeof(float))
        {
            return coordinate;
        }

        if (targetType == typeof(double))
        {
            return (double)coordinate;
        }

        return Mathf.FloorToInt(coordinate);
    }

    private static object ConvertCoordinate(int coordinate, Type targetType)
    {
        if (targetType == typeof(float))
        {
            return (float)coordinate;
        }

        if (targetType == typeof(double))
        {
            return (double)coordinate;
        }

        return coordinate;
    }

    private static bool IsNumericType(Type type)
    {
        return type == typeof(int)
            || type == typeof(float)
            || type == typeof(double)
            || type == typeof(short)
            || type == typeof(byte)
            || type == typeof(long)
            || type == typeof(uint)
            || type == typeof(ushort);
    }

    private static bool IsPlayerMounted(EntityPlayerLocal localPlayer)
    {
        EntityVehicle attachedVehicle = localPlayer?.AttachedToEntity as EntityVehicle;
        return attachedVehicle != null && attachedVehicle.HasDriver;
    }

    private static bool IsMountedNoFuelVehicle(EntityPlayerLocal localPlayer)
    {
        EntityVehicle attachedVehicle = localPlayer?.AttachedToEntity as EntityVehicle;
        Vehicle vehicle = attachedVehicle?.GetVehicle();
        if (attachedVehicle == null || vehicle == null)
        {
            return false;
        }

        return attachedVehicle.HasDriver && !vehicle.HasEnginePart();
    }

    private static void ApplyVehicleAttitudeIndicator(XUiC_HUDStatBar statBar, EntityPlayerLocal localPlayer)
    {
        Transform rootTransform = statBar?.ViewComponent?.UiTransform;
        if (rootTransform == null)
        {
            return;
        }

        EntityVehicle entityVehicle = (localPlayer?.AttachedToEntity as EntityVehicle) ?? statBar.vehicle;
        float pitchDegrees = GetVehiclePitchDegreesForDisplay(entityVehicle);

        // No-roll attitude: move the horizon vertically while keeping aircraft reference fixed.
        float yOffset = Mathf.Clamp(0f - (pitchDegrees * PitchPixelsPerDegree), 0f - PitchMaxPixelOffset, PitchMaxPixelOffset);
        Transform horizonMover =
            FindFromSelfOrAncestors(rootTransform, "vehiclePitchViewport/vehiclePitchHorizonMover") ??
            FindFromSelfOrAncestors(rootTransform, "vehiclePitch/vehiclePitchViewport/vehiclePitchHorizonMover") ??
            FindFromSelfOrAncestors(rootTransform, "vehicleStatsAGF/vehiclePitch/vehiclePitchViewport/vehiclePitchHorizonMover") ??
            FindFromSelfOrAncestors(rootTransform, "vehiclePitchHorizonMover");
        if (horizonMover != null)
        {
            Vector3 localPos = horizonMover.localPosition;
            horizonMover.localPosition = new Vector3(localPos.x, yOffset, localPos.z);
        }

        // Keep legacy marker containers static if present in old XML variants.
        Transform rotator =
            FindFromSelfOrAncestors(rootTransform, "vehiclePitchNeedleRotator") ??
            FindFromSelfOrAncestors(rootTransform, "vehiclePitchRotator") ??
            FindFromSelfOrAncestors(rootTransform, "vehiclePitch/vehiclePitchNeedleRotator") ??
            FindFromSelfOrAncestors(rootTransform, "vehiclePitch/vehiclePitchRotator");
        if (rotator != null)
        {
            rotator.localRotation = Quaternion.identity;
        }

        ApplyVehicleSpeedNeedle(rootTransform, entityVehicle);
    }

    private static void ApplyVehicleSpeedNeedle(Transform rootTransform, EntityVehicle entityVehicle)
    {
        Transform needleRotator =
            FindFromSelfOrAncestors(rootTransform, "speedNeedleRotator") ??
            FindFromSelfOrAncestors(rootTransform, "vehicleSpeedometer/speedSquareGauge/speedNeedleRotator") ??
            FindFromSelfOrAncestors(rootTransform, "vehicleAuxPanelAGF/vehicleSpeedometer/speedSquareGauge/speedNeedleRotator") ??
            FindFromSelfOrAncestors(rootTransform, "vehicleSpeedometer/speedQuarterGauge/speedNeedleRotator") ??
            FindFromSelfOrAncestors(rootTransform, "vehicleAuxPanelAGF/vehicleSpeedometer/speedQuarterGauge/speedNeedleRotator");
        if (needleRotator == null)
        {
            return;
        }

        Vehicle vehicle = entityVehicle?.GetVehicle();
        float current = GetCurrentSpeed(vehicle, entityVehicle);
        float max = vehicle?.MaxPossibleSpeed ?? 0f;
        float normalized = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        float angle = Mathf.Lerp(SpeedNeedleMinAngle, SpeedNeedleMaxAngle, normalized);
        needleRotator.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private static Transform FindFromSelfOrAncestors(Transform start, string relativePath)
    {
        for (Transform current = start; current != null; current = current.parent)
        {
            Transform found = current.Find(relativePath);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

}
