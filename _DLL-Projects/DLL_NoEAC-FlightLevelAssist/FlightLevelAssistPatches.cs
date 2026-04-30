using System;
using System.Collections.Generic;
using System.Globalization;
using HarmonyLib;
using UnityEngine;

namespace FlightLevelAssist
{
    internal enum FlightControlPattern
    {
        Unknown,
        BasicPlane,
        HelicopterLike
    }

    internal enum LockMode
    {
        Hover,
        ForwardOnPlane
    }

    internal static class FlightLevelStateStore
    {
        internal sealed class State
        {
            public bool Enabled;
            public bool HardLockActive;
            public float TargetAltitude;
            public float TimeSinceArmed;
            public float PitchSettledTime;
            public bool WasHotkeyHeld;
            public bool HasLastObservedAltitude;
            public float LastObservedAltitude;
            public float LockSuppressUntil;
            public FlightControlPattern ControlPattern;
            public LockMode Mode;
            public float SmoothedTargetPitchDeg;
            public float ForwardAlignTime;
            public bool HoverAltitudeInputActive;
            public bool WasHoverAltitudeInputActive;
            public LockMode LastMode;
            public float HoverVerticalReleaseUntil;
            public float HoverHorizontalReleaseUntil;
            public float HoverMomentumHoldUntil;
            public float AssistActivationReleaseUntil;
            public float ForwardModeTransitionUntil;
            public bool HasLearnedForwardPitchNormal;
            public bool HasLearnedForwardPitchTurbo;
            public float LearnedForwardPitchNormalDeg;
            public float LearnedForwardPitchTurboDeg;
            public float BestForwardScoreNormal;
            public float BestForwardScoreTurbo;
            public float LowMomentumSince;
            public bool PlaneYLockAutoRunCoupled;
            public bool PlaneYLockUsingExternalAutoRun;
        }

        private static readonly Dictionary<int, State> States = new Dictionary<int, State>();

        public static State GetOrCreate(EntityVehicle vehicle)
        {
            if (vehicle == null)
            {
                return null;
            }

            int key = vehicle.entityId;
            if (!States.TryGetValue(key, out State state))
            {
                state = new State();
                States[key] = state;
            }

            return state;
        }

        public static State TryGet(EntityVehicle vehicle)
        {
            if (vehicle == null)
            {
                return null;
            }

            States.TryGetValue(vehicle.entityId, out State state);
            return state;
        }
    }

    internal static class AutoRunInterop
    {
        private const string VehicleEnabledCVar = "agf_autorun_vehicle_enabled";

        private static readonly Type AutoRunStateStoreType = AccessTools.TypeByName("AutoRun.AutoRunStateStore");
        private static readonly System.Reflection.MethodInfo GetOrCreateForVehicleMethod = AutoRunStateStoreType != null
            ? AccessTools.Method(AutoRunStateStoreType, "GetOrCreateForVehicle", new[] { typeof(EntityVehicle) })
            : null;

        private static readonly System.Reflection.FieldInfo VehicleEnabledField;
        private static readonly System.Reflection.FieldInfo VehicleTurboEnabledField;
        private static readonly System.Reflection.FieldInfo VehicleWasForwardPressedField;
        private static readonly System.Reflection.FieldInfo VehicleForwardForcedLastFrameField;
        private static readonly System.Reflection.FieldInfo VehicleWasTurboPressedField;
        private static readonly System.Reflection.FieldInfo VehicleSprintResumeTimeField;

        static AutoRunInterop()
        {
            if (AutoRunStateStoreType == null)
            {
                return;
            }

            Type stateType = AccessTools.TypeByName("AutoRun.AutoRunStateStore+State");
            if (stateType == null)
            {
                return;
            }

            VehicleEnabledField = AccessTools.Field(stateType, "VehicleEnabled");
            VehicleTurboEnabledField = AccessTools.Field(stateType, "VehicleTurboEnabled");
            VehicleWasForwardPressedField = AccessTools.Field(stateType, "VehicleWasForwardPressed");
            VehicleForwardForcedLastFrameField = AccessTools.Field(stateType, "VehicleForwardForcedLastFrame");
            VehicleWasTurboPressedField = AccessTools.Field(stateType, "VehicleWasTurboPressed");
            VehicleSprintResumeTimeField = AccessTools.Field(stateType, "VehicleSprintResumeTime");
        }

        public static bool IsAvailable()
        {
            return GetOrCreateForVehicleMethod != null
                && VehicleEnabledField != null
                && VehicleTurboEnabledField != null;
        }

        private static object TryGetVehicleState(EntityVehicle vehicle)
        {
            if (!IsAvailable() || vehicle == null)
            {
                return null;
            }

            try
            {
                return GetOrCreateForVehicleMethod.Invoke(null, new object[] { vehicle });
            }
            catch
            {
                return null;
            }
        }

        public static bool EnableVehicleAutoRun(EntityVehicle vehicle, EntityPlayerLocal player, bool lockTurbo)
        {
            object state = TryGetVehicleState(vehicle);
            if (state == null)
            {
                return false;
            }

            bool forwardHeld = vehicle != null
                && vehicle.movementInput != null
                && vehicle.movementInput.moveForward >= 0.55f;

            VehicleEnabledField.SetValue(state, true);
            VehicleTurboEnabledField.SetValue(state, lockTurbo);
            VehicleSprintResumeTimeField?.SetValue(state, 0f);
            VehicleWasForwardPressedField?.SetValue(state, forwardHeld);
            VehicleForwardForcedLastFrameField?.SetValue(state, false);
            VehicleWasTurboPressedField?.SetValue(state, lockTurbo);
            player?.SetCVar(VehicleEnabledCVar, 1f);
            return true;
        }

        public static void DisableVehicleAutoRun(EntityVehicle vehicle, EntityPlayerLocal player)
        {
            object state = TryGetVehicleState(vehicle);
            if (state != null)
            {
                VehicleEnabledField.SetValue(state, false);
                VehicleTurboEnabledField.SetValue(state, false);
                VehicleSprintResumeTimeField?.SetValue(state, 0f);
                VehicleWasForwardPressedField?.SetValue(state, false);
                VehicleForwardForcedLastFrameField?.SetValue(state, false);
                VehicleWasTurboPressedField?.SetValue(state, false);
            }

            player?.SetCVar(VehicleEnabledCVar, 0f);
        }

        public static bool IsVehicleAutoRunEnabled(EntityVehicle vehicle)
        {
            object state = TryGetVehicleState(vehicle);
            if (state == null)
            {
                return false;
            }

            object value = VehicleEnabledField.GetValue(state);
            return value is bool enabled && enabled;
        }
    }

    [HarmonyPatch(typeof(EntityVehicle), nameof(EntityVehicle.MoveByAttachedEntity))]
    internal static class Patch_EntityVehicle_MoveByAttachedEntity
    {
        private const float ManualAltitudeNudgeRate = 2.4f;
        private const float AssistActivationTransitionSeconds = 0.55f;
        private const float ForwardModeTransitionSeconds = 0.35f;
        private const float HoverModeTransitionSeconds = 1.40f;
        private const float HoverMomentumHoldSeconds = 0.45f;
        private const float ControlMechanismAxisDominanceRatio = 1.25f;
        private const float ControlMechanismMinAxis = 0.01f;

        private static bool IsPlaneLikeForYLockAutoRun(EntityVehicle vehicle, FlightLevelStateStore.State state)
        {
            return state != null && state.ControlPattern == FlightControlPattern.BasicPlane;
        }

        private static bool TryParseForceVector(string raw, out float x, out float y, out float z)
        {
            x = 0f;
            y = 0f;
            z = 0f;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            string[] parts = raw.Split(',');
            if (parts.Length < 3)
            {
                return false;
            }

            return float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out x)
                && float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out y)
                && float.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out z);
        }

        private static bool TryResolveControlPatternFromControlMechanism(EntityVehicle vehicleEntity, out FlightControlPattern pattern)
        {
            pattern = FlightControlPattern.Unknown;

            Vehicle vehicle = vehicleEntity != null ? vehicleEntity.GetVehicle() : null;
            DynamicProperties properties = vehicle != null ? vehicle.Properties : null;
            if (properties == null)
            {
                return false;
            }

            int helicopterEvidence = 0;
            int planeEvidence = 0;

            for (int i = 0; i < 99; i++)
            {
                if (properties.Classes.TryGetValue("motor" + i, out DynamicProperties motorProps) && motorProps != null)
                {
                    string motorTrigger = motorProps.GetString("trigger");
                    if (string.Equals(motorTrigger, "inputForward", StringComparison.OrdinalIgnoreCase))
                    {
                        planeEvidence += 2;
                    }
                }

                if (!properties.Classes.TryGetValue("force" + i, out DynamicProperties forceProps) || forceProps == null)
                {
                    continue;
                }

                string forceTrigger = forceProps.GetString("trigger");
                if (!string.Equals(forceTrigger, "inputForward", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!TryParseForceVector(forceProps.GetString("force"), out float fx, out float fy, out float fz))
                {
                    continue;
                }

                float absY = Mathf.Abs(fy);
                float absZ = Mathf.Abs(fz);
                if (absY >= ControlMechanismMinAxis && absY >= absZ * ControlMechanismAxisDominanceRatio)
                {
                    helicopterEvidence += 2;
                }
                else if (absZ >= ControlMechanismMinAxis && absZ >= absY * ControlMechanismAxisDominanceRatio)
                {
                    planeEvidence += 2;
                }
            }

            if (planeEvidence == 0 && helicopterEvidence == 0)
            {
                return false;
            }

            if (planeEvidence > helicopterEvidence)
            {
                pattern = FlightControlPattern.BasicPlane;
                return true;
            }

            if (helicopterEvidence > planeEvidence)
            {
                pattern = FlightControlPattern.HelicopterLike;
                return true;
            }

            return false;
        }

        public static void Postfix(EntityVehicle __instance, EntityPlayerLocal _player)
        {
            if (__instance == null || _player == null || __instance.movementInput == null)
            {
                return;
            }

            if (_player != __instance.AttachedMainEntity)
            {
                return;
            }

            if (!IsLikelyFlyingVehicle(__instance))
            {
                return;
            }

            FlightLevelStateStore.State state = FlightLevelStateStore.GetOrCreate(__instance);
            if (state == null)
            {
                return;
            }

            ResolveControlPattern(__instance, state);

            bool helicopterLike = state.ControlPattern == FlightControlPattern.HelicopterLike;
            bool heliAltitudeUp = false;
            bool heliAltitudeDown = false;
            if (helicopterLike)
            {
                // Helicopter altitude axis comes from moveForward (W/S by default).
                heliAltitudeUp = __instance.movementInput.moveForward > 0.05f;
                heliAltitudeDown = __instance.movementInput.moveForward < -0.05f;
            }

            GetActivationInputState(_player, state, out bool levelKeyPressed);

            if (levelKeyPressed)
            {
                if (!state.Enabled)
                {
                    StartForcedFlatLock(__instance, state);
                    // Helicopter patterns start in hover; plane patterns use only y-lock mode.
                    state.Mode = helicopterLike ? LockMode.Hover : LockMode.ForwardOnPlane;
                }
                else if (helicopterLike)
                {
                    // While assist is active, each press toggles hover/forward mode.
                    state.Mode = state.Mode == LockMode.Hover ? LockMode.ForwardOnPlane : LockMode.Hover;
                }
            }

            if (helicopterLike && state.Mode != state.LastMode)
            {
                if (state.Mode == LockMode.ForwardOnPlane)
                {
                    // Re-acquire hard lock for hover->forward transitions so Y-lock does not
                    // engage until forward pitch settles near the forward engage target.
                    state.HardLockActive = false;
                    state.PitchSettledTime = 0f;
                    state.ForwardModeTransitionUntil = Time.time + ForwardModeTransitionSeconds;
                    state.AssistActivationReleaseUntil = Time.time + AssistActivationTransitionSeconds;
                    // Rebase to current altitude on forward switch to avoid snap-back to a stale hover lock plane.
                    state.TargetAltitude = __instance.vehicleRB != null ? __instance.vehicleRB.position.y : __instance.position.y;
                }
                else
                {
                    state.ForwardModeTransitionUntil = 0f;
                    state.AssistActivationReleaseUntil = Time.time + AssistActivationTransitionSeconds;
                    state.HoverHorizontalReleaseUntil = Time.time + HoverModeTransitionSeconds;
                    state.HoverVerticalReleaseUntil = Time.time + HoverModeTransitionSeconds;
                    state.HoverMomentumHoldUntil = Time.time + HoverMomentumHoldSeconds;
                    state.TargetAltitude = __instance.vehicleRB != null ? __instance.vehicleRB.position.y : __instance.position.y;
                }

                state.LastMode = state.Mode;
            }

            if (!state.Enabled)
            {
                return;
            }

            bool pilotClimb = __instance.movementInput.jump;
            bool pilotDescend = __instance.movementInput.down;

            bool heliForwardMode = state.HardLockActive
                && helicopterLike
                && state.Mode == LockMode.ForwardOnPlane;

            state.HoverAltitudeInputActive = false;

            // Manual pitch controls cancel assist for both hover and y-lock modes.
            if (pilotClimb || pilotDescend)
            {
                DisableAssist(state, __instance);
                Debug.Log("[FlightLevelAssist] Canceled by pitch input for vehicle " + __instance.entityId + ".");
                return;
            }

            // Level mode altitude behavior:
            // - helicopter-like: keep normal W/S vertical controls by following current altitude while held
            // - basic-plane: keep manual nudge behavior on jump/down axis
            if (state.HardLockActive && !heliForwardMode)
            {
                if (helicopterLike)
                {
                    if (heliAltitudeUp || heliAltitudeDown)
                    {
                        state.HoverAltitudeInputActive = true;
                        // Let helicopter altitude controls behave normally while lock tracks the resulting altitude.
                        state.TargetAltitude = __instance.vehicleRB != null ? __instance.vehicleRB.position.y : __instance.position.y;
                    }
                }
                else
                {
                    float nudge = 0f;
                    if (pilotClimb)
                    {
                        nudge += 1f;
                    }
                    if (pilotDescend)
                    {
                        nudge -= 1f;
                    }
                    if (nudge != 0f)
                    {
                        state.TargetAltitude += nudge * ManualAltitudeNudgeRate * Time.deltaTime;
                    }
                }
            }

            // Forward cruise mode intentionally drives helicopter-like up axis
            // so forward-on-plane can sustain speed while pitch transitions.
            // This mode does not synthesize input; pilot keeps direct control of up/turbo.

            bool planeLikeYLockMode = state.Enabled && IsPlaneLikeForYLockAutoRun(__instance, state);
            if (planeLikeYLockMode)
            {
                if (!state.PlaneYLockAutoRunCoupled)
                {
                    bool turboHeld = __instance.movementInput.running;
                    state.PlaneYLockUsingExternalAutoRun = AutoRunInterop.EnableVehicleAutoRun(__instance, _player, turboHeld);
                    state.PlaneYLockAutoRunCoupled = true;
                }

                if (state.PlaneYLockUsingExternalAutoRun)
                {
                    if (!AutoRunInterop.IsVehicleAutoRunEnabled(__instance))
                    {
                        DisableAssist(state, __instance);
                        Debug.Log("[FlightLevelAssist] Y-lock canceled because vehicle auto-run was disabled for vehicle " + __instance.entityId + ".");
                        return;
                    }
                }
                else
                {
                    // Fallback when AutoRun mod is unavailable: hold forward while Y-lock is active.
                    __instance.movementInput.moveForward = 1f;
                }
            }
        }

        private static void ResolveControlPattern(EntityVehicle vehicleEntity, FlightLevelStateStore.State state)
        {
            if (state.ControlPattern != FlightControlPattern.Unknown)
            {
                return;
            }

            if (TryResolveControlPatternFromControlMechanism(vehicleEntity, out FlightControlPattern resolved))
            {
                state.ControlPattern = resolved;
                return;
            }

            // Start in helicopter-like mode and allow observed control behavior to retarget.
            state.ControlPattern = FlightControlPattern.HelicopterLike;
        }

        private static void GetActivationInputState(EntityPlayerLocal player, FlightLevelStateStore.State state, out bool pressed)
        {
            bool keyboardBlockedByTextEntry = IsTextEntryFocused(player);
            if (keyboardBlockedByTextEntry)
            {
                // Block all activation paths while typing/searching in text inputs.
                pressed = false;
                state.WasHotkeyHeld = false;
                return;
            }

            bool hotkeyHeld = !keyboardBlockedByTextEntry && Input.GetKey(FlightLevelAssistConfig.LevelAssistHotkey);
            bool hotkeyDown = !keyboardBlockedByTextEntry && Input.GetKeyDown(FlightLevelAssistConfig.LevelAssistHotkey);

            // Some hooks miss the exact down frame; rising edge on held key keeps activation reliable.
            pressed = hotkeyDown || (hotkeyHeld && !state.WasHotkeyHeld);
            state.WasHotkeyHeld = hotkeyHeld;

            if (!TryGetConfiguredControllerAction(player, out object action))
            {
                return;
            }

            pressed |= ReadActionBool(action, "WasPressed");
        }

        private static bool IsTextEntryFocused(EntityPlayerLocal player)
        {
            try
            {
                LocalPlayerUI localPlayerUI = LocalPlayerUI.GetUIForPlayer(player) ?? LocalPlayerUI.primaryUI;
                if (localPlayerUI?.windowManager != null)
                {
                    return localPlayerUI.windowManager.IsInputActive();
                }

                // If no player UI is available, avoid blocking controls blindly.
                return false;
            }
            catch
            {
                // Fail open: if focus detection errors, keep normal controls working.
            }

            return false;
        }

        private static bool TryGetConfiguredControllerAction(EntityPlayerLocal player, out object action)
        {
            action = null;
            FlightLevelAssistConfig.ControllerActivationAction controllerAction = FlightLevelAssistConfig.ControllerActivation;
            if (controllerAction == FlightLevelAssistConfig.ControllerActivationAction.None)
            {
                return false;
            }

            LocalPlayerUI ui = LocalPlayerUI.GetUIForPlayer(player);
            if (ui == null || ui.playerInput == null)
            {
                return false;
            }

            PlayerActionsVehicle vehicleActions = ui.playerInput.VehicleActions;
            if (vehicleActions == null)
            {
                return false;
            }

            switch (controllerAction)
            {
            case FlightLevelAssistConfig.ControllerActivationAction.ToggleTurnMode:
                action = vehicleActions.ToggleTurnMode;
                break;
            case FlightLevelAssistConfig.ControllerActivationAction.HonkHorn:
                action = vehicleActions.HonkHorn;
                break;
            case FlightLevelAssistConfig.ControllerActivationAction.ToggleFlashlight:
                action = vehicleActions.ToggleFlashlight;
                break;
            case FlightLevelAssistConfig.ControllerActivationAction.Scoreboard:
                action = vehicleActions.Scoreboard;
                break;
            case FlightLevelAssistConfig.ControllerActivationAction.Inventory:
                action = vehicleActions.Inventory;
                break;
            case FlightLevelAssistConfig.ControllerActivationAction.Activate:
                break;
            default:
                action = null;
                break;
            }

            return action != null;
        }

        private static bool ReadActionBool(object action, string propertyName)
        {
            if (action == null)
            {
                return false;
            }

            var property = action.GetType().GetProperty(propertyName);
            if (property == null || property.PropertyType != typeof(bool))
            {
                return false;
            }

            object value = property.GetValue(action, null);
            return value is bool flag && flag;
        }

        private static void StartForcedFlatLock(EntityVehicle vehicle, FlightLevelStateStore.State state)
        {
            // Preserve resolved pattern from the current input frame.
            FlightControlPattern preservedPattern = state.ControlPattern;
            bool helicopterLike = preservedPattern == FlightControlPattern.HelicopterLike;

            state.Enabled = true;
            state.HardLockActive = false;
            state.TargetAltitude = 0f;
            state.TimeSinceArmed = 0f;
            state.PitchSettledTime = 0f;
            state.HasLastObservedAltitude = false;
            state.LastObservedAltitude = 0f;
            state.LockSuppressUntil = 0f;
            state.ControlPattern = preservedPattern;
            state.Mode = LockMode.Hover;
            state.SmoothedTargetPitchDeg = 0f;
            state.ForwardAlignTime = 0f;
            state.HoverAltitudeInputActive = false;
            state.WasHoverAltitudeInputActive = false;
            state.LastMode = state.Mode;
            if (helicopterLike)
            {
                state.HoverVerticalReleaseUntil = Time.time + HoverModeTransitionSeconds;
                state.HoverHorizontalReleaseUntil = Time.time + HoverModeTransitionSeconds;
                state.HoverMomentumHoldUntil = Time.time + HoverMomentumHoldSeconds;
                state.TargetAltitude = vehicle != null && vehicle.vehicleRB != null
                    ? vehicle.vehicleRB.position.y
                    : (vehicle != null ? vehicle.position.y : 0f);
            }
            else
            {
                state.HoverVerticalReleaseUntil = 0f;
                state.HoverHorizontalReleaseUntil = 0f;
                state.HoverMomentumHoldUntil = 0f;
            }
            state.AssistActivationReleaseUntil = Time.time + AssistActivationTransitionSeconds;
            state.ForwardModeTransitionUntil = 0f;
            state.HasLearnedForwardPitchNormal = false;
            state.HasLearnedForwardPitchTurbo = false;
            state.LearnedForwardPitchNormalDeg = 0f;
            state.LearnedForwardPitchTurboDeg = 0f;
            state.BestForwardScoreNormal = float.NegativeInfinity;
            state.BestForwardScoreTurbo = float.NegativeInfinity;
            state.LowMomentumSince = 0f;
            state.PlaneYLockAutoRunCoupled = false;
            state.PlaneYLockUsingExternalAutoRun = false;
            Debug.Log("[FlightLevelAssist] Assist armed on press for vehicle " + vehicle.entityId + ".");
        }

        internal static void DisableAssist(FlightLevelStateStore.State state, EntityVehicle vehicle = null, bool suppressExternalAutoRunDisable = false)
        {
            if (!suppressExternalAutoRunDisable && state.PlaneYLockAutoRunCoupled && vehicle != null)
            {
                AutoRunInterop.DisableVehicleAutoRun(vehicle, vehicle.AttachedMainEntity as EntityPlayerLocal);
            }

            state.Enabled = false;
            state.HardLockActive = false;
            state.TimeSinceArmed = 0f;
            state.PitchSettledTime = 0f;
            state.WasHotkeyHeld = false;
            state.HasLastObservedAltitude = false;
            state.LastObservedAltitude = 0f;
            state.LockSuppressUntil = 0f;
            state.ControlPattern = FlightControlPattern.Unknown;
            state.Mode = LockMode.Hover;
            state.SmoothedTargetPitchDeg = 0f;
            state.ForwardAlignTime = 0f;
            state.HoverAltitudeInputActive = false;
            state.WasHoverAltitudeInputActive = false;
            state.LastMode = state.Mode;
            state.HoverVerticalReleaseUntil = 0f;
            state.HoverHorizontalReleaseUntil = 0f;
            state.HoverMomentumHoldUntil = 0f;
            state.AssistActivationReleaseUntil = 0f;
            state.ForwardModeTransitionUntil = 0f;
            state.HasLearnedForwardPitchNormal = false;
            state.HasLearnedForwardPitchTurbo = false;
            state.LearnedForwardPitchNormalDeg = 0f;
            state.LearnedForwardPitchTurboDeg = 0f;
            state.BestForwardScoreNormal = float.NegativeInfinity;
            state.BestForwardScoreTurbo = float.NegativeInfinity;
            state.LowMomentumSince = 0f;
            state.PlaneYLockAutoRunCoupled = false;
            state.PlaneYLockUsingExternalAutoRun = false;
        }

        public static bool IsEnabledFor(EntityVehicle vehicle)
        {
            FlightLevelStateStore.State state = FlightLevelStateStore.TryGet(vehicle);
            return state != null && state.Enabled;
        }

        private static bool HasCeilingForceProperty(EntityVehicle vehicle)
        {
            Vehicle definition = vehicle != null ? vehicle.GetVehicle() : null;
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

            return false;
        }

        private static bool IsLikelyFlyingVehicle(EntityVehicle vehicle)
        {
            if (vehicle == null)
            {
                return false;
            }

            try
            {
                return HasCeilingForceProperty(vehicle);
            }
            catch
            {
                return false;
            }
        }

    }

    [HarmonyPatch(typeof(EntityVehicle), "PhysicsFixedUpdate")]
    internal static class Patch_EntityVehicle_PhysicsFixedUpdate
    {
        private const float AssistActivationTransitionSeconds = 0.55f;
        private const float ForwardModeTransitionSeconds = 0.35f;

        // Match vanilla helicopter manual pitch torque and sprint multiplier.
        private const float BasePitchTorque = 0.01f;
        private const float SprintPitchTorqueScale = 6f;
        private const float PitchRampSeconds = 0.65f;
        private const float MinSpeedTorqueScale = 0.65f;
        private const float MaxSpeedTorqueScale = 1.25f;
        private const float PitchDeadZoneDeg = 0.1f;
        private const float PitchFullScaleErrorDeg = 4f;
        private const float PitchRateDampingGain = 0.00012f;

        // Forward-lock mode for helicopter-like controls: push toward max forward speed,
        // but keep altitude lock as the primary constraint.
        private const float HelicopterForwardBasePitchDeg = -26f;
        private const float HelicopterForwardTurboPitchBonusDeg = -2f;
        private const float HelicopterForwardHardMinPitchDeg = -33f;
        private const float HelicopterForwardRecoverMaxPitchDeg = -4f;
        private const float HelicopterForwardSpeedDeficitPitchGainDeg = 10f;
        private const float HelicopterForwardTurboExtraDeficitPitchGainDeg = 6f;
        private const float HelicopterForwardTurboMinDeltaFromNormalDeg = 3f;
        private const float HelicopterForwardAltitudeRecoverGain = 4f;
        private const float HelicopterForwardVSpeedRecoverGain = 0.8f;
        private const float HelicopterForwardAltitudeBand = 0.08f;
        private const float HelicopterForwardVerticalSpeedBand = 0.12f;
        private const float HelicopterForwardDiveGuardAltitude = 0.35f;
        private const float HelicopterForwardDiveGuardMaxPitchDeg = -10f;
        private const float ForwardLearningAltitudeBand = 0.20f;
        private const float ForwardLearningVerticalSpeedBand = 0.35f;
        private const float ForwardLearningSpeedMinRatio = 0.35f;
        private const float ForwardScoreAltitudePenalty = 22f;
        private const float ForwardScoreVerticalSpeedPenalty = 10f;
        private const float ForwardScoreImproveThreshold = 0.015f;
        private const float ForwardBestScoreDecayPerTick = 0.0008f;
        private const float ForwardLearnRateFast = 0.30f;
        private const float ForwardLearnRateSlow = 0.02f;
        private const float PitchTargetSlewDegPerSec = 12f;
        private const float BasicPlanePitchTargetSlewDegPerSec = 6f;
        private const float BasicPlaneMomentumMinRatio = 0.34f;
        private const float BasicPlaneMomentumMinSpeed = 7f;
        private const float BasicPlaneMomentumDropGraceSeconds = 0.50f;
        private const float ForwardPitchTargetSlewDegPerSec = 22f;
        private const float ForwardTransitionPitchSlewDegPerSec = 10f;
        private const float ForwardTransitionPitchScale = 0.55f;
        private const float ForwardTransitionPitchTorqueBoost = 1.15f;
        private const float ForwardPitchTorqueBoost = 1.6f;
        private const float HoverTransitionPitchSlewDegPerSec = 7f;
        private const float HoverTransitionPitchTorqueScale = 0.70f;
        private const float BasicPlanePitchTorqueScale = 0.45f;
        private const float ForwardPitchEngageToleranceDeg = 1.25f;
        private const float ForwardPitchRateEngageDegPerSec = 8f;
        private const float ForwardAlignHoldSeconds = 0.20f;
        private const float HelicopterForwardHardLockEngagePitchDeg = HelicopterForwardBasePitchDeg;
        private const float HelicopterForwardHardLockPitchToleranceDeg = 2.0f;

        // Natural pitch correction phase before hard lock engagement.
        private const float PitchSettleToleranceDeg = 0.75f;
        private const float PitchRateSettleDegPerSec = 3.5f;
        private const float PitchSettleDurationSec = 0.22f;

        // Hover stabilization for helicopter-like controls: keep mostly stationary in XZ.
        private const float HoverHorizontalDampingPerTick = 0.015f;
        private const float HoverHorizontalReleaseDampingPerTick = 0.004f;
        private const float HoverVerticalReleaseGraceSeconds = 1.40f;
        private const float HoverVerticalReleaseDampingPerTick = 0.015f;
        private const float HoverVerticalLockDampingPerTick = 0.12f;
        private const float HoverVerticalLockPositionLerpPerTick = 0.12f;
        private const float HoverReleaseTargetFollowPerTick = 0.18f;
        private const float ForwardTransitionVerticalDampingPerTick = 0.03f;
        private const float TransitionTargetAltitudeLerpPerTick = 0.05f;

        // Hard altitude lock behavior: strict world Y-plane pin.
        private const float LockPlaneSnapTolerance = 0.001f;
        private const float MaxVerticalLockStepPerTick = 0.35f;

        // External correction protection (network/chunk/engine snap).
        private const float ExternalVerticalStepThreshold = 6f;
        private const float ExternalCorrectionGraceSeconds = 0.30f;

        // Auto-cancel on crash-like pitch disruption.
        private const float CrashPitchCancelDeg = 38f;
        private const float CrashPitchRateCancelDegPerSec = 180f;

        // Roll damping keeps wobble controlled while allowing turn/strafe behavior.
        private const float HoldRollDampingGain = 0.006f;
        private const float MaxHoldRollDampingTorque = 0.01f;

        private static bool RequiresMomentumForYLock(EntityVehicle vehicle, FlightLevelStateStore.State state)
        {
            return state != null && state.ControlPattern == FlightControlPattern.BasicPlane;
        }

        public static void Postfix(EntityVehicle __instance)
        {
            if (__instance == null || __instance.vehicleRB == null)
            {
                return;
            }

            FlightLevelStateStore.State state = FlightLevelStateStore.GetOrCreate(__instance);
            if (state == null)
            {
                return;
            }

            // Always clear assist when unattended so lock cannot persist between riders.
            if (!__instance.HasDriver || __instance.AttachedMainEntity == null)
            {
                Patch_EntityVehicle_MoveByAttachedEntity.DisableAssist(state, __instance, suppressExternalAutoRunDisable: true);
                return;
            }

            if (!Patch_EntityVehicle_MoveByAttachedEntity.IsEnabledFor(__instance))
            {
                return;
            }

            Transform t = __instance.PhysicsTransform;
            if (t == null)
            {
                t = __instance.transform;
            }

            Vector3 localAngVel = t.InverseTransformDirection(__instance.vehicleRB.angularVelocity);

            // Use forward.y to get nose-up (+) / nose-down (-) pitch regardless of Euler wrap.
            float pitchDeg = Mathf.Asin(Mathf.Clamp(t.forward.y, -1f, 1f)) * Mathf.Rad2Deg;
            float pitchRateDegPerSec = localAngVel.x * Mathf.Rad2Deg;

            Vehicle vehicle = __instance.GetVehicle();
            if (vehicle != null && vehicle.GetFuelLevel() <= 0f)
            {
                Patch_EntityVehicle_MoveByAttachedEntity.DisableAssist(state, __instance);
                Debug.Log("[FlightLevelAssist] Canceled due to no fuel for vehicle " + __instance.entityId + ".");
                return;
            }

            // Only apply crash-style cancel once hard lock is active so extreme starting angles
            // can still be leveled during the pre-lock correction phase.
            if (state.HardLockActive && (Mathf.Abs(pitchDeg) >= CrashPitchCancelDeg || Mathf.Abs(pitchRateDegPerSec) >= CrashPitchRateCancelDegPerSec))
            {
                Patch_EntityVehicle_MoveByAttachedEntity.DisableAssist(state, __instance);
                Debug.Log("[FlightLevelAssist] Canceled due to crash-level pitch change for vehicle " + __instance.entityId + ".");
                return;
            }

            state.TimeSinceArmed += Time.fixedDeltaTime;
            float currentAltitude = __instance.vehicleRB.position.y;

            if (state.HasLastObservedAltitude && state.HardLockActive)
            {
                float stepDelta = currentAltitude - state.LastObservedAltitude;
                if (Mathf.Abs(stepDelta) >= ExternalVerticalStepThreshold)
                {
                    // Rebase to observed corrected altitude and briefly pause enforcement
                    // to avoid lock fighting engine/network correction.
                    state.TargetAltitude = currentAltitude;
                    state.LockSuppressUntil = Time.time + ExternalCorrectionGraceSeconds;
                    Debug.Log("[FlightLevelAssist] External Y correction detected; rebasing lock target for vehicle " + __instance.entityId + ".");
                }
            }

            float currentForwardSpeed = Mathf.Max(0f, vehicle != null ? vehicle.CurrentForwardVelocity : 0f);
            float normalForwardSpeedCap = vehicle != null ? Mathf.Max(1f, vehicle.VelocityMaxForward) : 1f;
            float turboForwardSpeedCap = vehicle != null
                ? Mathf.Max(normalForwardSpeedCap, vehicle.VelocityMaxTurboForward)
                : normalForwardSpeedCap;

            float basicPlaneMomentumThreshold = Mathf.Max(BasicPlaneMomentumMinSpeed, normalForwardSpeedCap * BasicPlaneMomentumMinRatio);
            bool requiresMomentumForYLock = RequiresMomentumForYLock(__instance, state);

            if (state.HardLockActive && requiresMomentumForYLock)
            {
                if (currentForwardSpeed < basicPlaneMomentumThreshold)
                {
                    if (state.LowMomentumSince <= 0f)
                    {
                        state.LowMomentumSince = Time.time;
                    }
                    else if (Time.time - state.LowMomentumSince >= BasicPlaneMomentumDropGraceSeconds)
                    {
                        Patch_EntityVehicle_MoveByAttachedEntity.DisableAssist(state, __instance);
                        Debug.Log(
                            "[FlightLevelAssist] Y-lock disengaged due to low momentum on vehicle "
                            + __instance.entityId
                            + ": "
                            + currentForwardSpeed.ToString("F1")
                            + " / "
                            + basicPlaneMomentumThreshold.ToString("F1")
                            + " m/s.");
                        return;
                    }
                }
                else
                {
                    state.LowMomentumSince = 0f;
                }
            }
            else
            {
                state.LowMomentumSince = 0f;
            }

            bool boostInputActive = __instance.movementInput != null && __instance.movementInput.moveForward > 0.05f;
            bool heliForwardMode = state.HardLockActive
                && state.ControlPattern == FlightControlPattern.HelicopterLike
                && state.Mode == LockMode.ForwardOnPlane;
            bool helicopterForwardRequested = state.ControlPattern == FlightControlPattern.HelicopterLike
                && state.Mode == LockMode.ForwardOnPlane;
            bool helicopterHoverRequested = state.ControlPattern == FlightControlPattern.HelicopterLike
                && state.Mode == LockMode.Hover;

            if (!state.HardLockActive && helicopterForwardRequested)
            {
                // Keep forward-mode target altitude aligned during pitch setup to avoid
                // hard-lock engaging against a stale target plane.
                bool heliPilotVerticalIntent = __instance.movementInput != null && Mathf.Abs(__instance.movementInput.moveForward) > 0.05f;
                float follow = heliPilotVerticalIntent ? 1f : TransitionTargetAltitudeLerpPerTick;
                state.TargetAltitude = Mathf.Lerp(state.TargetAltitude, currentAltitude, follow);
            }

            // Hover/level stabilization should not depend on forward speed-shaping regime.
            float stabilizationSpeedCap = turboForwardSpeedCap;
            float activeForwardSpeedCap = (vehicle != null && heliForwardMode && boostInputActive)
                ? turboForwardSpeedCap
                : normalForwardSpeedCap;

            // Control pattern is resolved during activation input processing.
            // Do not override unknown here to a helicopter fallback.

            float normalizedSpeed = Mathf.Clamp01(currentForwardSpeed / stabilizationSpeedCap);
            float speedScale = Mathf.Lerp(MinSpeedTorqueScale, MaxSpeedTorqueScale, normalizedSpeed);
            float rampScale = Mathf.Lerp(0.75f, 1f, Mathf.Clamp01(state.TimeSinceArmed / PitchRampSeconds));

            float targetPitchDeg = 0f;
            if (state.HardLockActive
                && state.ControlPattern == FlightControlPattern.HelicopterLike
                && state.Mode == LockMode.ForwardOnPlane)
            {
                bool boostRegimeActive = boostInputActive;
                float configuredBaseForwardPitch = HelicopterForwardBasePitchDeg + (boostRegimeActive ? HelicopterForwardTurboPitchBonusDeg : 0f);

                float altitudeError = state.TargetAltitude - currentAltitude;
                float verticalSpeed = __instance.vehicleRB.velocity.y;
                float learningSpeedRatio = Mathf.Clamp01(currentForwardSpeed / activeForwardSpeedCap);
                bool learningWindow = Mathf.Abs(altitudeError) <= ForwardLearningAltitudeBand
                    && Mathf.Abs(verticalSpeed) <= ForwardLearningVerticalSpeedBand
                    && learningSpeedRatio >= ForwardLearningSpeedMinRatio;
                if (learningWindow)
                {
                    float score = currentForwardSpeed
                        - Mathf.Abs(altitudeError) * ForwardScoreAltitudePenalty
                        - Mathf.Abs(verticalSpeed) * ForwardScoreVerticalSpeedPenalty;

                    if (boostRegimeActive)
                    {
                        if (!state.HasLearnedForwardPitchTurbo)
                        {
                            state.HasLearnedForwardPitchTurbo = true;
                            state.LearnedForwardPitchTurboDeg = pitchDeg;
                            state.BestForwardScoreTurbo = score;
                        }
                        else
                        {
                            state.BestForwardScoreTurbo = Mathf.Max(state.BestForwardScoreTurbo - ForwardBestScoreDecayPerTick, -9999f);
                            if (score >= state.BestForwardScoreTurbo + ForwardScoreImproveThreshold)
                            {
                                state.BestForwardScoreTurbo = score;
                                state.LearnedForwardPitchTurboDeg = Mathf.Lerp(state.LearnedForwardPitchTurboDeg, pitchDeg, ForwardLearnRateFast);
                            }
                            else
                            {
                                state.LearnedForwardPitchTurboDeg = Mathf.Lerp(state.LearnedForwardPitchTurboDeg, pitchDeg, ForwardLearnRateSlow);
                            }
                        }
                    }
                    else
                    {
                        if (!state.HasLearnedForwardPitchNormal)
                        {
                            state.HasLearnedForwardPitchNormal = true;
                            state.LearnedForwardPitchNormalDeg = pitchDeg;
                            state.BestForwardScoreNormal = score;
                        }
                        else
                        {
                            state.BestForwardScoreNormal = Mathf.Max(state.BestForwardScoreNormal - ForwardBestScoreDecayPerTick, -9999f);
                            if (score >= state.BestForwardScoreNormal + ForwardScoreImproveThreshold)
                            {
                                state.BestForwardScoreNormal = score;
                                state.LearnedForwardPitchNormalDeg = Mathf.Lerp(state.LearnedForwardPitchNormalDeg, pitchDeg, ForwardLearnRateFast);
                            }
                            else
                            {
                                state.LearnedForwardPitchNormalDeg = Mathf.Lerp(state.LearnedForwardPitchNormalDeg, pitchDeg, ForwardLearnRateSlow);
                            }
                        }
                    }
                }

                float baseForwardPitch = configuredBaseForwardPitch;
                if (boostRegimeActive && state.HasLearnedForwardPitchTurbo)
                {
                    baseForwardPitch = state.LearnedForwardPitchTurboDeg;
                }
                else if (!boostRegimeActive && state.HasLearnedForwardPitchNormal)
                {
                    baseForwardPitch = state.LearnedForwardPitchNormalDeg;
                }
                baseForwardPitch = Mathf.Clamp(baseForwardPitch, HelicopterForwardHardMinPitchDeg, HelicopterForwardRecoverMaxPitchDeg);

                float speedRatio = Mathf.Clamp01(currentForwardSpeed / activeForwardSpeedCap);
                float speedDeficit = 1f - speedRatio;
                float speedPitchBias = speedDeficit * HelicopterForwardSpeedDeficitPitchGainDeg;
                if (boostRegimeActive)
                {
                    speedPitchBias += speedDeficit * HelicopterForwardTurboExtraDeficitPitchGainDeg;
                }
                float desiredForwardPitch = Mathf.Clamp(
                    baseForwardPitch - speedPitchBias,
                    HelicopterForwardHardMinPitchDeg,
                    HelicopterForwardRecoverMaxPitchDeg);

                if (boostRegimeActive)
                {
                    float normalBasePitch = state.HasLearnedForwardPitchNormal
                        ? state.LearnedForwardPitchNormalDeg
                        : HelicopterForwardBasePitchDeg;
                    float turboFloor = normalBasePitch - HelicopterForwardTurboMinDeltaFromNormalDeg;
                    desiredForwardPitch = Mathf.Min(desiredForwardPitch, turboFloor);
                }

                float recoverPitch = 0f;
                bool outsideAltitudeBand = Mathf.Abs(altitudeError) > HelicopterForwardAltitudeBand
                    || Mathf.Abs(verticalSpeed) > HelicopterForwardVerticalSpeedBand;
                if (outsideAltitudeBand)
                {
                    recoverPitch = Mathf.Clamp(
                        (altitudeError * HelicopterForwardAltitudeRecoverGain)
                        - (verticalSpeed * HelicopterForwardVSpeedRecoverGain),
                        0f,
                        Mathf.Abs(desiredForwardPitch - HelicopterForwardRecoverMaxPitchDeg));
                }

                targetPitchDeg = Mathf.Clamp(
                    desiredForwardPitch + recoverPitch,
                    HelicopterForwardHardMinPitchDeg,
                    HelicopterForwardRecoverMaxPitchDeg);

                if (Time.time < state.ForwardModeTransitionUntil)
                {
                    // Blend in forward pitch authority after hover so the transition feels natural.
                    targetPitchDeg *= ForwardTransitionPitchScale;
                }

                // If we are materially below the lock plane, immediately relax forward pitch to arrest dive.
                if (altitudeError > HelicopterForwardDiveGuardAltitude)
                {
                    targetPitchDeg = Mathf.Max(targetPitchDeg, HelicopterForwardDiveGuardMaxPitchDeg);
                }
            }

            if (!state.HardLockActive && helicopterForwardRequested)
            {
                // Before hard lock engages, move toward a forward-flight pitch first so Y-lock
                // does not clamp altitude while the vehicle is still near hover attitude.
                targetPitchDeg = Mathf.Clamp(
                    HelicopterForwardHardLockEngagePitchDeg,
                    HelicopterForwardHardMinPitchDeg,
                    HelicopterForwardRecoverMaxPitchDeg);
            }

            float pitchSlewDegPerSec;
            if (state.HardLockActive
                && state.ControlPattern == FlightControlPattern.HelicopterLike
                && state.Mode == LockMode.ForwardOnPlane)
            {
                float forwardTransitionBlend = Mathf.Clamp01(
                    1f - ((state.ForwardModeTransitionUntil - Time.time) / ForwardModeTransitionSeconds));
                pitchSlewDegPerSec = Mathf.Lerp(
                    ForwardTransitionPitchSlewDegPerSec,
                    ForwardPitchTargetSlewDegPerSec,
                    forwardTransitionBlend);
            }
            else if (state.ControlPattern == FlightControlPattern.BasicPlane)
            {
                pitchSlewDegPerSec = BasicPlanePitchTargetSlewDegPerSec;
            }
            else
            {
                pitchSlewDegPerSec = PitchTargetSlewDegPerSec;
            }
            if (helicopterHoverRequested && Time.time < state.HoverHorizontalReleaseUntil)
            {
                pitchSlewDegPerSec = Mathf.Min(pitchSlewDegPerSec, HoverTransitionPitchSlewDegPerSec);
            }
            float pitchTargetStep = pitchSlewDegPerSec * Time.fixedDeltaTime;
            state.SmoothedTargetPitchDeg = Mathf.MoveTowards(state.SmoothedTargetPitchDeg, targetPitchDeg, pitchTargetStep);
            targetPitchDeg = state.SmoothedTargetPitchDeg;

            float pitchErrorDeg = pitchDeg - targetPitchDeg;

            if (heliForwardMode)
            {
                bool nearForwardTarget = Mathf.Abs(pitchErrorDeg) <= ForwardPitchEngageToleranceDeg
                    && Mathf.Abs(pitchRateDegPerSec) <= ForwardPitchRateEngageDegPerSec;
                state.ForwardAlignTime = nearForwardTarget
                    ? state.ForwardAlignTime + Time.fixedDeltaTime
                    : 0f;
            }
            else
            {
                state.ForwardAlignTime = 0f;
            }

            if (Mathf.Abs(pitchErrorDeg) > PitchDeadZoneDeg)
            {
                float torqueScale = (__instance.movementInput != null && __instance.movementInput.running) ? SprintPitchTorqueScale : 1f;
                float maxTorque = BasePitchTorque * torqueScale * speedScale * rampScale;
                if (heliForwardMode)
                {
                    float forwardTransitionBlend = Mathf.Clamp01(
                        1f - ((state.ForwardModeTransitionUntil - Time.time) / ForwardModeTransitionSeconds));
                    maxTorque *= Mathf.Lerp(
                        ForwardTransitionPitchTorqueBoost,
                        ForwardPitchTorqueBoost,
                        forwardTransitionBlend);
                }
                else if (state.ControlPattern == FlightControlPattern.BasicPlane)
                {
                    maxTorque *= BasicPlanePitchTorqueScale;
                }
                if (helicopterHoverRequested && Time.time < state.HoverHorizontalReleaseUntil)
                {
                    maxTorque *= HoverTransitionPitchTorqueScale;
                }
                float pTerm = Mathf.Clamp(pitchErrorDeg / PitchFullScaleErrorDeg, -1f, 1f) * maxTorque;
                float dTerm = 0f - pitchRateDegPerSec * PitchRateDampingGain;
                float pitchTorque = Mathf.Clamp(pTerm + dTerm, 0f - maxTorque, maxTorque);
                __instance.vehicleRB.AddRelativeTorque(new Vector3(pitchTorque, 0f, 0f), ForceMode.VelocityChange);
            }

            if (!state.HardLockActive)
            {
                float settlePitchTolerance = helicopterForwardRequested
                    ? HelicopterForwardHardLockPitchToleranceDeg
                    : PitchSettleToleranceDeg;
                float settlePitchErrorDeg = helicopterForwardRequested
                    ? (pitchDeg - HelicopterForwardHardLockEngagePitchDeg)
                    : pitchErrorDeg;
                bool pitchSettled = Mathf.Abs(settlePitchErrorDeg) <= settlePitchTolerance
                    && Mathf.Abs(pitchRateDegPerSec) <= PitchRateSettleDegPerSec;
                state.PitchSettledTime = pitchSettled ? state.PitchSettledTime + Time.fixedDeltaTime : 0f;

                if (state.PitchSettledTime >= PitchSettleDurationSec)
                {
                    state.HardLockActive = true;
                    if (!helicopterForwardRequested)
                    {
                        state.TargetAltitude = currentAltitude;
                    }
                    state.AssistActivationReleaseUntil = Time.time + AssistActivationTransitionSeconds;

                    if (requiresMomentumForYLock)
                    {
                        EntityPlayerLocal localDriver = __instance.AttachedMainEntity as EntityPlayerLocal;
                        bool turboHeld = __instance.movementInput != null && __instance.movementInput.running;
                        state.PlaneYLockUsingExternalAutoRun = AutoRunInterop.EnableVehicleAutoRun(__instance, localDriver, turboHeld);
                        state.PlaneYLockAutoRunCoupled = true;
                    }

                    Debug.Log("[FlightLevelAssist] Hard lock engaged for vehicle " + __instance.entityId + " at Y=" + state.TargetAltitude.ToString("F3") + ".");
                }
            }

            if (state.HardLockActive && Time.time >= state.LockSuppressUntil)
            {
                bool helicopterHoverMode = state.ControlPattern == FlightControlPattern.HelicopterLike
                    && state.Mode == LockMode.Hover;
                bool helicopterHoverVerticalInput = state.ControlPattern == FlightControlPattern.HelicopterLike
                    && helicopterHoverMode
                    && state.HoverAltitudeInputActive;

                if (helicopterHoverMode)
                {
                    if (state.WasHoverAltitudeInputActive && !helicopterHoverVerticalInput)
                    {
                        state.HoverVerticalReleaseUntil = Time.time + HoverVerticalReleaseGraceSeconds;
                        state.TargetAltitude = __instance.vehicleRB.position.y;
                    }
                    state.WasHoverAltitudeInputActive = helicopterHoverVerticalInput;
                }
                else
                {
                    state.WasHoverAltitudeInputActive = false;
                }

                bool forwardTransitionGrace = heliForwardMode && Time.time < state.ForwardModeTransitionUntil;
                bool hoverReleaseGrace = helicopterHoverMode && Time.time < state.HoverVerticalReleaseUntil;
                bool hoverMomentumHold = helicopterHoverMode && Time.time < state.HoverMomentumHoldUntil;
                bool activationGrace = Time.time < state.AssistActivationReleaseUntil;
                bool softenVerticalLock = forwardTransitionGrace || hoverReleaseGrace || activationGrace;
                float forwardLockBlend = heliForwardMode
                    ? Mathf.Clamp01(state.ForwardAlignTime / ForwardAlignHoldSeconds)
                    : 1f;
                bool heliPilotVerticalIntent = state.ControlPattern == FlightControlPattern.HelicopterLike
                    && __instance.movementInput != null
                    && Mathf.Abs(__instance.movementInput.moveForward) > 0.05f;

                Vector3 position = __instance.vehicleRB.position;
                if (heliForwardMode && (forwardTransitionGrace || forwardLockBlend < 0.999f || heliPilotVerticalIntent))
                {
                    float follow = heliPilotVerticalIntent ? 1f : TransitionTargetAltitudeLerpPerTick;
                    state.TargetAltitude = Mathf.Lerp(state.TargetAltitude, position.y, follow);
                }
                float altitudeError = state.TargetAltitude - position.y;
                if (!helicopterHoverVerticalInput && Mathf.Abs(altitudeError) > LockPlaneSnapTolerance)
                {
                    if (helicopterHoverMode)
                    {
                        if (hoverMomentumHold)
                        {
                            state.TargetAltitude = Mathf.Lerp(state.TargetAltitude, position.y, TransitionTargetAltitudeLerpPerTick);
                        }
                        else if (!softenVerticalLock)
                        {
                            float correctedY = Mathf.Lerp(position.y, state.TargetAltitude, HoverVerticalLockPositionLerpPerTick);
                            __instance.vehicleRB.MovePosition(new Vector3(position.x, correctedY, position.z));
                        }
                        else
                        {
                            float targetFollow = hoverReleaseGrace
                                ? HoverReleaseTargetFollowPerTick
                                : TransitionTargetAltitudeLerpPerTick;
                            state.TargetAltitude = Mathf.Lerp(state.TargetAltitude, position.y, targetFollow);
                        }
                    }
                    else
                    {
                        if (heliForwardMode)
                        {
                            float followStrength = Mathf.Lerp(
                                TransitionTargetAltitudeLerpPerTick,
                                HoverVerticalLockPositionLerpPerTick,
                                forwardLockBlend);
                            float correctedY = Mathf.Lerp(position.y, state.TargetAltitude, followStrength);
                            __instance.vehicleRB.MovePosition(new Vector3(position.x, correctedY, position.z));
                        }
                        else if (!softenVerticalLock)
                        {
                            float correctedY = Mathf.MoveTowards(position.y, state.TargetAltitude, MaxVerticalLockStepPerTick);
                            __instance.vehicleRB.MovePosition(new Vector3(position.x, correctedY, position.z));
                        }
                        else
                        {
                            state.TargetAltitude = Mathf.Lerp(state.TargetAltitude, position.y, TransitionTargetAltitudeLerpPerTick);
                        }
                    }
                }

                Vector3 velocity = __instance.vehicleRB.velocity;
                if (!helicopterHoverVerticalInput && velocity.y != 0f)
                {
                    if (helicopterHoverMode)
                    {
                        if (hoverMomentumHold)
                        {
                            __instance.vehicleRB.velocity = new Vector3(velocity.x, velocity.y, velocity.z);
                        }
                        else if (!softenVerticalLock)
                        {
                            __instance.vehicleRB.velocity = new Vector3(velocity.x, velocity.y * Mathf.Clamp01(1f - HoverVerticalLockDampingPerTick), velocity.z);
                        }
                        else
                        {
                            float verticalDamp = hoverReleaseGrace
                                ? (1f - HoverVerticalReleaseDampingPerTick)
                                : (1f - ForwardTransitionVerticalDampingPerTick);
                            __instance.vehicleRB.velocity = new Vector3(velocity.x, velocity.y * Mathf.Clamp01(verticalDamp), velocity.z);
                        }
                    }
                    else
                    {
                        if (heliForwardMode)
                        {
                            float dampPerTick = Mathf.Lerp(
                                ForwardTransitionVerticalDampingPerTick,
                                HoverVerticalLockDampingPerTick,
                                forwardLockBlend);
                            __instance.vehicleRB.velocity = new Vector3(
                                velocity.x,
                                velocity.y * Mathf.Clamp01(1f - dampPerTick),
                                velocity.z);
                        }
                        else if (forwardTransitionGrace)
                        {
                            // During hover->forward transition, keep natural vertical momentum.
                            __instance.vehicleRB.velocity = new Vector3(velocity.x, velocity.y, velocity.z);
                        }
                        else if (!softenVerticalLock)
                        {
                            __instance.vehicleRB.velocity = new Vector3(velocity.x, 0f, velocity.z);
                        }
                        else
                        {
                            float verticalDamp = hoverReleaseGrace
                                ? (1f - HoverVerticalReleaseDampingPerTick)
                                : (1f - ForwardTransitionVerticalDampingPerTick);
                            __instance.vehicleRB.velocity = new Vector3(velocity.x, velocity.y * Mathf.Clamp01(verticalDamp), velocity.z);
                        }
                    }
                }

                if (helicopterHoverMode)
                {
                    Vector3 hoverVelocity = __instance.vehicleRB.velocity;
                    if (!hoverMomentumHold)
                    {
                        float horizontalDamping = Time.time < state.HoverHorizontalReleaseUntil
                            ? HoverHorizontalReleaseDampingPerTick
                            : HoverHorizontalDampingPerTick;
                        float dampFactor = Mathf.Clamp01(1f - horizontalDamping);
                        __instance.vehicleRB.velocity = new Vector3(
                            hoverVelocity.x * dampFactor,
                            hoverVelocity.y,
                            hoverVelocity.z * dampFactor);
                    }
                }
            }

            if (state.Enabled)
            {
                float rollRateDegPerSec = localAngVel.z * Mathf.Rad2Deg;
                float rollDampingTorque = Mathf.Clamp(0f - rollRateDegPerSec * HoldRollDampingGain, 0f - MaxHoldRollDampingTorque, MaxHoldRollDampingTorque);
                if (rollDampingTorque != 0f)
                {
                    __instance.vehicleRB.AddRelativeTorque(new Vector3(0f, 0f, rollDampingTorque), ForceMode.VelocityChange);
                }
            }

            state.HasLastObservedAltitude = true;
            state.LastObservedAltitude = currentAltitude;
        }
    }
}
