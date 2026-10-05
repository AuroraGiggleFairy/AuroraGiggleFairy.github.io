using HarmonyLib;
using UnityEngine;

namespace AutoRun
{
    [HarmonyPatch]
    internal static class FlightAssistHudBindings
    {
        private const string FlightAssistVisible = "flaVehicleFlightAssistVisible";
        private const string FlightAssistModeText = "flaVehicleFlightAssistModeText";
        private const string FlightAssistModeTooltip = "flaVehicleFlightAssistModeTooltip";
        private const string FlightAssistOffIconTooltip = "flaVehicleFlightAssistOffIconTooltip";
        private const string FlightAssistOffVisible = "flaVehicleFlightAssistOffVisible";
        private const string FlightAssistHoverVisible = "flaVehicleFlightAssistHoverVisible";
        private const string FlightAssistYLockVisible = "flaVehicleFlightAssistYLockVisible";

        private static int lastModeSignature = int.MinValue;
        private static int dirtyFrame = -1;
        private static int lastCruiseSignature = int.MinValue;
        private static int cruiseDirtyFrame = -1;

        [HarmonyPatch(typeof(XUiC_HUDStatBar), "GetBindingValueInternal")]
        [HarmonyPrefix]
        private static bool HudStatBarBindingsPrefix(ref bool __result, XUiC_HUDStatBar __instance, ref string _value, string _bindingName)
        {
            if (__instance == null || !IsFlightAssistBinding(_bindingName))
            {
                return true;
            }

            EntityPlayerLocal localPlayer = __instance.localPlayer ?? __instance.xui?.playerUI?.entityPlayer;
            EntityVehicle entityVehicle = (localPlayer?.AttachedToEntity as EntityVehicle) ?? __instance.vehicle;
            if (!TryResolveBinding(entityVehicle, _bindingName, out _value))
            {
                return true;
            }

            __result = true;
            return false;
        }

        [HarmonyPatch(typeof(XUiC_HUDStatBar), "hasChanged")]
        [HarmonyPostfix]
        private static void HasChangedPostfix(XUiC_HUDStatBar __instance, ref bool __result)
        {
            if (__instance == null)
            {
                return;
            }

            EntityPlayerLocal cruisePlayer = __instance.localPlayer ?? __instance.xui?.playerUI?.entityPlayer;
            int cruiseSignature = AutoRunStateStore.IsAnyIndicatorActive(cruisePlayer) ? 1 : 0;
            if (cruiseSignature != lastCruiseSignature)
            {
                lastCruiseSignature = cruiseSignature;
                cruiseDirtyFrame = Time.frameCount;
            }

            if (Time.frameCount == cruiseDirtyFrame)
            {
                __result = true;
            }

            if (__instance.statGroup != HUDStatGroups.Vehicle)
            {
                return;
            }

            int signature = CurrentModeSignature(__instance);
            if (signature != lastModeSignature)
            {
                lastModeSignature = signature;
                dirtyFrame = Time.frameCount;
            }

            if (Time.frameCount == dirtyFrame)
            {
                __result = true;
            }
        }

        private static int CurrentModeSignature(XUiC_HUDStatBar statBar)
        {
            EntityPlayerLocal localPlayer = statBar.localPlayer ?? statBar.xui?.playerUI?.entityPlayer;
            EntityVehicle entityVehicle = (localPlayer?.AttachedToEntity as EntityVehicle) ?? statBar.vehicle;
            if (!IsLikelyFlyingVehicle(entityVehicle))
            {
                return 0;
            }

            FlightLevelStateStore.State state = FlightLevelStateStore.TryGet(entityVehicle);
            bool enabled = Patch_FlightAssist_MoveByAttachedEntity.IsEnabledFor(entityVehicle);
            int mode = state != null ? (int)state.Mode : 0;
            int pattern = state != null ? (int)state.ControlPattern : 0;
            int holding = state != null && state.HardLockActive ? 16 : 0;
            int positioning = IsPositioning(state, enabled) ? 32 : 0;
            return 1 + (enabled ? 8 : 0) + (mode * 2) + pattern + holding + positioning;
        }

        private static bool IsPositioning(FlightLevelStateStore.State state, bool enabled)
        {
            if (!enabled || state == null)
            {
                return false;
            }

            if (state.ControlPattern == FlightControlPattern.HelicopterLike && state.Mode == LockMode.Hover)
            {
                return !state.HardLockActive || Time.time < state.HoverHorizontalReleaseUntil;
            }

            if (state.ControlPattern == FlightControlPattern.HelicopterLike && state.Mode == LockMode.ForwardOnPlane)
            {
                return !state.HardLockActive || !state.HeliForwardThrustOn;
            }

            return !state.HardLockActive && !state.LockSettledOnce;
        }

        private static bool IsFlightAssistBinding(string bindingName)
        {
            switch (bindingName)
            {
                case FlightAssistVisible:
                case FlightAssistModeText:
                case FlightAssistModeTooltip:
                case FlightAssistOffIconTooltip:
                case FlightAssistOffVisible:
                case FlightAssistHoverVisible:
                case FlightAssistYLockVisible:
                    return true;
                default:
                    return false;
            }
        }

        private static bool TryResolveBinding(EntityVehicle entityVehicle, string bindingName, out string value)
        {
            value = null;

            bool isFlyingVehicle = IsLikelyFlyingVehicle(entityVehicle);
            FlightLevelStateStore.State state = FlightLevelStateStore.TryGet(entityVehicle);
            bool enabled = Patch_FlightAssist_MoveByAttachedEntity.IsEnabledFor(entityVehicle);
            bool isHelicopterPattern = state != null && state.ControlPattern == FlightControlPattern.HelicopterLike;
            bool heightHolding = state != null && state.HardLockActive;
            bool positioning = IsPositioning(state, enabled);
            bool isHoverMode = enabled && heightHolding && isHelicopterPattern && state != null && state.Mode == LockMode.Hover && !positioning;
            bool isYLockMode = enabled && heightHolding && !positioning && (!isHelicopterPattern || state.Mode == LockMode.ForwardOnPlane);

            switch (bindingName)
            {
                case FlightAssistVisible:
                    value = isFlyingVehicle ? "true" : "false";
                    return true;
                case FlightAssistModeText:
                    if (!isFlyingVehicle)
                    {
                        value = Localization.Get("xuiFlightAssist_Off_Name");
                        return true;
                    }

                    value = positioning
                        ? Localization.Get("xuiFlightAssist_Positioning_Name")
                        : (isHoverMode
                            ? Localization.Get("xuiFlightAssist_Hover_Name")
                            : (isYLockMode ? Localization.Get("xuiFlightAssist_YLock_Name") : Localization.Get("xuiFlightAssist_Off_Name")));
                    return true;
                case FlightAssistModeTooltip:
                    value = BuildModeTooltipText();
                    return true;
                case FlightAssistOffIconTooltip:
                    value = BuildTooltipTextFromTemplate("xuiFlightAssist_Off_Icon_Tooltip", "Flight Assist is currently off. Press {0} to activate.");
                    return true;
                case FlightAssistOffVisible:
                    value = isFlyingVehicle && !enabled ? "true" : "false";
                    return true;
                case FlightAssistHoverVisible:
                    value = isHoverMode ? "true" : "false";
                    return true;
                case FlightAssistYLockVisible:
                    value = isYLockMode ? "true" : "false";
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsLikelyFlyingVehicle(EntityVehicle vehicle)
        {
            if (vehicle == null)
            {
                return false;
            }

            try
            {
                Vehicle definition = vehicle.GetVehicle();
                DynamicProperties properties = definition != null ? definition.Properties : null;
                if (properties == null)
                {
                    return false;
                }

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
            catch
            {
                return false;
            }

            return false;
        }

        private static string BuildModeTooltipText()
        {
            return BuildTooltipTextFromTemplate("xuiFlightAssist_ModeText_Text_Tooltip", "Press {0} to cycle Flight Assist modes.");
        }

        private static string BuildTooltipTextFromTemplate(string key, string fallback)
        {
            string template = Localization.Get(key);
            if (string.IsNullOrEmpty(template))
            {
                template = fallback;
            }

            string activationInput = GetActivationInputDisplay();
            if (template.IndexOf("{0}", System.StringComparison.Ordinal) >= 0)
            {
                return string.Format(template, activationInput);
            }

            return template;
        }

        private static string GetActivationInputDisplay()
        {
            return AutoRunBindingManager.GetActivationKeyLabel();
        }
    }
}
