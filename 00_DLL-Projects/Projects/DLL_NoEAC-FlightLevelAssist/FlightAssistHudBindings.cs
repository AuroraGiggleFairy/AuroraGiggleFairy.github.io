using HarmonyLib;
using UnityEngine;

namespace FlightLevelAssist
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
            bool enabled = Patch_EntityVehicle_MoveByAttachedEntity.IsEnabledFor(entityVehicle);
            bool isHelicopterPattern = state != null && state.ControlPattern == FlightControlPattern.HelicopterLike;
            bool isHoverMode = enabled && isHelicopterPattern && state != null && state.Mode == LockMode.Hover;
            bool isYLockMode = enabled && (!isHelicopterPattern || (state != null && state.Mode == LockMode.ForwardOnPlane));

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

                    value = isHoverMode
                        ? Localization.Get("xuiFlightAssist_Hover_Name")
                        : (isYLockMode ? Localization.Get("xuiFlightAssist_YLock_Name") : Localization.Get("xuiFlightAssist_Off_Name"));
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
            string keyText = FlightLevelAssistConfig.LevelAssistHotkey.ToString().ToUpperInvariant();
            FlightLevelAssistConfig.ControllerActivationAction controllerAction = FlightLevelAssistConfig.ControllerActivation;
            if (controllerAction == FlightLevelAssistConfig.ControllerActivationAction.None)
            {
                return keyText;
            }

            return keyText + " or " + GetControllerActionDisplay(controllerAction);
        }

        private static string GetControllerActionDisplay(FlightLevelAssistConfig.ControllerActivationAction action)
        {
            switch (action)
            {
                case FlightLevelAssistConfig.ControllerActivationAction.ToggleTurnMode:
                    return "Toggle Turn Mode";
                case FlightLevelAssistConfig.ControllerActivationAction.HonkHorn:
                    return "Honk Horn";
                case FlightLevelAssistConfig.ControllerActivationAction.ToggleFlashlight:
                    return "Toggle Flashlight";
                case FlightLevelAssistConfig.ControllerActivationAction.Scoreboard:
                    return "Scoreboard";
                case FlightLevelAssistConfig.ControllerActivationAction.Inventory:
                    return "Inventory";
                case FlightLevelAssistConfig.ControllerActivationAction.Activate:
                    return "Activate";
                default:
                    return action.ToString();
            }
        }
    }
}
