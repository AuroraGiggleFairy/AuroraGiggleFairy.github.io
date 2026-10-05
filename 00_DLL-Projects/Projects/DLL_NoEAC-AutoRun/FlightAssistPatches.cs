using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace AutoRun
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
            public bool PlaneHeightOn;
            public bool PlaneLockCancelled;
            public float HoverResumeAfter;
            public bool HeliHoverOn;
            public bool HeliHoverReturnToCruise;
            public bool HeliHoverReturnSprint;
            public bool WasCruiseOn;
            public bool HeliForwardThrustOn;
            public bool LockSettledOnce;
            public bool HoverPilotOwnsPitch;
            public bool WasHoverPilotPitching;
            public float HoverHeldPitchDeg;
            public bool HeliManual;
            public bool HeliCruiseClimbHeld;
            public bool HeliCruiseDescendHeld;
            public bool WasHeliCruiseHeightHeld;
            public int PlaneAssistStep;
            public float NextFlightZTime;
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

    internal static class FlightAssistCruise
    {
        public static bool IsCruiseOn(EntityVehicle vehicle)
        {
            if (vehicle == null)
            {
                return false;
            }

            AutoRunStateStore.State state = AutoRunStateStore.GetOrCreateForVehicle(vehicle);
            return state != null && state.VehicleEnabled;
        }

        public static void ReleaseCruise(EntityVehicle vehicle, EntityPlayerLocal player)
        {
            if (vehicle == null)
            {
                return;
            }

            AutoRunStateStore.State state = AutoRunStateStore.GetOrCreateForVehicle(vehicle);
            if (state == null)
            {
                return;
            }

            state.VehicleEnabled = false;
            state.VehicleSprintLocked = false;
            state.VehicleWasForwardPressed = true;
            state.VehicleWasTurboPressed = false;
            AutoRunStateStore.SetVehicleIndicator(player, enabled: false);
        }

        public static void EngageCruise(EntityVehicle vehicle, EntityPlayerLocal player, bool sprintLocked)
        {
            if (vehicle == null)
            {
                return;
            }

            AutoRunStateStore.State state = AutoRunStateStore.GetOrCreateForVehicle(vehicle);
            if (state == null)
            {
                return;
            }

            state.VehicleEnabled = true;
            state.VehicleSprintLocked = sprintLocked;
            state.VehicleWasForwardPressed = true;
            PlayerActionsVehicle actions = player != null ? LocalPlayerUI.GetUIForPlayer(player)?.playerInput?.VehicleActions : null;
            state.VehicleWasTurboPressed = actions != null && actions.Turbo != null && actions.Turbo.IsPressed;
            AutoRunStateStore.SetVehicleIndicator(player, enabled: true);
        }
    }

    [HarmonyPatch(typeof(EntityVehicle), nameof(EntityVehicle.MoveByAttachedEntity))]
    internal static class Patch_FlightAssist_MoveByAttachedEntity
    {
        private const float ManualAltitudeNudgeRate = 2.4f;
        private const float AssistActivationTransitionSeconds = 0.55f;
        private const float ForwardModeTransitionSeconds = 0.35f;
        private const float HoverModeTransitionSeconds = 1.40f;
        private const float HoverMomentumHoldSeconds = 0.45f;
        private const float ControlMechanismAxisDominanceRatio = 1.25f;
        private const float ControlMechanismMinAxis = 0.01f;

        private static readonly FieldInfo LiveForcesField = AccessTools.Field(typeof(EntityVehicle), "forces");
        private static readonly FieldInfo LiveMotorsField = AccessTools.Field(typeof(EntityVehicle), "motors");
        private static MethodInfo gyroTryGetFlightMode;

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
            if (TryResolveGyroFlightMode(vehicleEntity, out pattern))
            {
                return true;
            }

            if (TryResolveLiveForces(vehicleEntity, out pattern))
            {
                return true;
            }

            return TryResolveXmlProperties(vehicleEntity, out pattern);
        }

        private static bool TryResolveGyroFlightMode(EntityVehicle vehicleEntity, out FlightControlPattern pattern)
        {
            pattern = FlightControlPattern.Unknown;
            if (gyroTryGetFlightMode == null)
            {
                Type api = AccessTools.TypeByName("GyroFlightModes.GyroFlightModesApi");
                gyroTryGetFlightMode = api == null ? null : AccessTools.Method(api, "TryGetFlightMode");
                if (gyroTryGetFlightMode == null)
                {
                    gyroTryGetFlightMode = typeof(Patch_FlightAssist_MoveByAttachedEntity).GetMethod(nameof(MissingGyroApi), BindingFlags.NonPublic | BindingFlags.Static);
                }
            }

            if (gyroTryGetFlightMode == null || gyroTryGetFlightMode.Name == nameof(MissingGyroApi))
            {
                return false;
            }

            object[] args = { vehicleEntity, null };
            try
            {
                if (!(gyroTryGetFlightMode.Invoke(null, args) is bool found) || !found)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            bool helicopter = args[1] != null && string.Equals(args[1].ToString(), "Heli", StringComparison.Ordinal);
            pattern = helicopter ? FlightControlPattern.HelicopterLike : FlightControlPattern.BasicPlane;
            return true;
        }

        private static bool MissingGyroApi()
        {
            return false;
        }

        private static bool TryResolveLiveForces(EntityVehicle vehicleEntity, out FlightControlPattern pattern)
        {
            pattern = FlightControlPattern.Unknown;
            if (vehicleEntity == null)
            {
                return false;
            }

            int helicopterEvidence = 0;
            int planeEvidence = 0;
            ScoreMotorArray(LiveMotorsField?.GetValue(vehicleEntity) as Array, ref planeEvidence);
            ScoreForceArray(LiveForcesField?.GetValue(vehicleEntity) as Array, ref helicopterEvidence, ref planeEvidence);
            return PickPattern(helicopterEvidence, planeEvidence, out pattern);
        }

        private static void ScoreMotorArray(Array motors, ref int planeEvidence)
        {
            if (motors == null)
            {
                return;
            }

            for (int i = 0; i < motors.Length; i++)
            {
                object motor = motors.GetValue(i);
                object trigger = motor == null ? null : AccessTools.Field(motor.GetType(), "trigger")?.GetValue(motor);
                if (trigger != null && string.Equals(trigger.ToString(), "InputForward", StringComparison.OrdinalIgnoreCase))
                {
                    planeEvidence += 2;
                }
            }
        }

        private static void ScoreForceArray(Array forces, ref int helicopterEvidence, ref int planeEvidence)
        {
            if (forces == null)
            {
                return;
            }

            for (int i = 0; i < forces.Length; i++)
            {
                object force = forces.GetValue(i);
                if (force == null)
                {
                    continue;
                }

                object trigger = AccessTools.Field(force.GetType(), "trigger")?.GetValue(force);
                if (trigger == null || !string.Equals(trigger.ToString(), "InputForward", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                object vector = AccessTools.Field(force.GetType(), "force")?.GetValue(force);
                if (!(vector is Vector3 forceVector))
                {
                    continue;
                }

                ScoreAxes(forceVector.y, forceVector.z, ref helicopterEvidence, ref planeEvidence);
            }
        }

        private static void ScoreAxes(float y, float z, ref int helicopterEvidence, ref int planeEvidence)
        {
            float absY = Mathf.Abs(y);
            float absZ = Mathf.Abs(z);
            if (absY >= ControlMechanismMinAxis && absY >= absZ * ControlMechanismAxisDominanceRatio)
            {
                helicopterEvidence += 2;
            }
            else if (absZ >= ControlMechanismMinAxis && absZ >= absY * ControlMechanismAxisDominanceRatio)
            {
                planeEvidence += 2;
            }
        }

        private static bool PickPattern(int helicopterEvidence, int planeEvidence, out FlightControlPattern pattern)
        {
            pattern = FlightControlPattern.Unknown;
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

        private static bool TryResolveXmlProperties(EntityVehicle vehicleEntity, out FlightControlPattern pattern)
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

        [HarmonyPriority(Priority.Last)]
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

            ResolveControlPattern(__instance, _player, state);
            GetActivationInputState(_player, state, out bool levelKeyPressed);
            if (levelKeyPressed)
            {
                ToggleHeightKey(__instance, _player, state);
            }

            SyncCruiseAndHeight(__instance, _player, state);

            bool helicopterLike = state.ControlPattern == FlightControlPattern.HelicopterLike;
            bool heliAltitudeUp = false;
            bool heliAltitudeDown = false;
            if (helicopterLike)
            {
                heliAltitudeUp = __instance.movementInput.moveForward > 0.05f;
                heliAltitudeDown = __instance.movementInput.moveForward < -0.05f;
            }

            if (helicopterLike)
            {
                bool climbHeld = false;
                bool descendHeld = false;
                PlayerActionsVehicle vehicleActions = LocalPlayerUI.GetUIForPlayer(_player)?.playerInput?.VehicleActions;
                if (vehicleActions != null)
                {
                    climbHeld = vehicleActions.MoveForward != null && vehicleActions.MoveForward.IsPressed;
                    descendHeld = vehicleActions.MoveBack != null && vehicleActions.MoveBack.IsPressed;
                    if (EffectManager.GetValue(PassiveEffects.FlipControls, null, 0f, _player) > 0f)
                    {
                        bool swapHeld = climbHeld;
                        climbHeld = descendHeld;
                        descendHeld = swapHeld;
                    }
                }

                if (climbHeld && descendHeld)
                {
                    climbHeld = false;
                    descendHeld = false;
                }

                state.HeliCruiseClimbHeld = climbHeld;
                state.HeliCruiseDescendHeld = descendHeld;
            }
            else
            {
                state.HeliCruiseClimbHeld = false;
                state.HeliCruiseDescendHeld = false;
            }

            if (helicopterLike && state.Mode != state.LastMode)
            {
                if (state.Mode == LockMode.ForwardOnPlane)
                {
                    // Re-acquire hard lock for hover->forward transitions so Y-lock does not
                    // engage until forward pitch settles near the forward engage target.
                    state.HoverPilotOwnsPitch = false;
                    state.WasHoverPilotPitching = false;
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
                if (helicopterLike)
                {
                    state.HeliManual = true;
                    state.HeliForwardThrustOn = false;
                    DisableAssist(state, __instance);
                    state.HeliManual = true;
                    RestoreRealStick(__instance, _player);
                    return;
                }

                if (state.PlaneHeightOn)
                {
                    CancelPlaneHeightKeepCruise(state);
                    return;
                }
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

        }

        private static void ToggleHeightKey(EntityVehicle vehicle, EntityPlayerLocal player, FlightLevelStateStore.State state)
        {
            if (Time.time < state.NextFlightZTime)
            {
                return;
            }

            state.NextFlightZTime = Time.time + 0.2f;
            state.HoverResumeAfter = 0f;
            bool helicopterLike = state.ControlPattern == FlightControlPattern.HelicopterLike;
            bool cruiseOn = FlightAssistCruise.IsCruiseOn(vehicle);
            if (helicopterLike)
            {
                if (state.HeliManual && !cruiseOn)
                {
                    state.HeliManual = false;
                    return;
                }

                if (cruiseOn)
                {
                    FlightAssistCruise.ReleaseCruise(vehicle, player);
                    state.HeliForwardThrustOn = false;
                    state.HeliManual = false;
                    return;
                }

                bool keepSprint = AutoRunStateStore.GetOrCreateForVehicle(vehicle) != null
                    && AutoRunStateStore.GetOrCreateForVehicle(vehicle).VehicleSprintLocked;
                FlightAssistCruise.EngageCruise(vehicle, player, keepSprint);
                state.HeliManual = false;
                return;
            }

            state.PlaneAssistStep = (state.PlaneAssistStep + 1) % 3;

            if (state.PlaneAssistStep == 0)
            {
                FlightAssistCruise.ReleaseCruise(vehicle, player);
                state.PlaneHeightOn = false;
                state.PlaneLockCancelled = false;
                return;
            }

            if (state.PlaneAssistStep == 1)
            {
                FlightAssistCruise.EngageCruise(vehicle, player, sprintLocked: true);
                state.PlaneHeightOn = false;
                state.PlaneLockCancelled = false;
                return;
            }

            state.PlaneHeightOn = true;
            state.PlaneLockCancelled = false;
        }

        internal static void CancelPlaneHeightKeepCruise(FlightLevelStateStore.State state)
        {
            state.PlaneHeightOn = false;
            state.PlaneLockCancelled = true;
            state.PlaneAssistStep = 1;
            state.Enabled = false;
            state.HardLockActive = false;
        }

        private static void SyncCruiseAndHeight(EntityVehicle vehicle, EntityPlayerLocal player, FlightLevelStateStore.State state)
        {
            bool helicopterLike = state.ControlPattern == FlightControlPattern.HelicopterLike;
            bool cruiseOn = FlightAssistCruise.IsCruiseOn(vehicle);
            bool cruisePressed = cruiseOn && !state.WasCruiseOn;
            state.WasCruiseOn = cruiseOn;

            if (helicopterLike && cruisePressed)
            {
                state.HeliHoverOn = false;
                state.HeliHoverReturnToCruise = false;
                state.HeliHoverReturnSprint = false;
            }

            if (helicopterLike && cruiseOn)
            {
                EnsureMode(vehicle, state, LockMode.ForwardOnPlane, hoverEntry: false);
                ApplyHeliCruiseThrottle(vehicle, player, state);
                return;
            }

            if (helicopterLike)
            {
                state.HeliForwardThrustOn = false;
                if (state.HeliManual)
                {
                    if (state.Enabled)
                    {
                        state.Enabled = false;
                        state.HardLockActive = false;
                    }

                    RestoreRealStick(vehicle, player);
                    return;
                }

                if (Time.time >= state.HoverResumeAfter && VehicleHasFuel(vehicle))
                {
                    RestoreRealStick(vehicle, player);
                    EnsureMode(vehicle, state, LockMode.Hover, hoverEntry: true);
                }

                return;
            }

            if (!helicopterLike && !cruiseOn)
            {
                state.PlaneHeightOn = false;
                state.PlaneAssistStep = 0;
                state.PlaneLockCancelled = false;
            }
            else if (!helicopterLike && state.PlaneHeightOn)
            {
                EnsureMode(vehicle, state, LockMode.ForwardOnPlane, hoverEntry: false);
                return;
            }

            if (state.Enabled)
            {
                state.Enabled = false;
                state.HardLockActive = false;
            }
        }

        private static void EnsureMode(EntityVehicle vehicle, FlightLevelStateStore.State state, LockMode mode, bool hoverEntry)
        {
            if (state.Enabled && state.Mode == mode)
            {
                return;
            }

            bool keepHoverAltitude = hoverEntry
                && state.Enabled
                && state.Mode == LockMode.ForwardOnPlane
                && state.HardLockActive;
            float keptAltitude = state.TargetAltitude;

            StartForcedFlatLock(vehicle, state);
            state.Mode = mode;
            state.Enabled = true;
            if (hoverEntry)
            {
                state.HoverHorizontalReleaseUntil = Time.time + HoverModeTransitionSeconds;
                if (keepHoverAltitude)
                {
                    state.TargetAltitude = keptAltitude;
                    state.HardLockActive = true;
                    state.HasLastObservedAltitude = true;
                    state.LastObservedAltitude = keptAltitude;
                    state.HoverVerticalReleaseUntil = 0f;
                    state.HoverMomentumHoldUntil = 0f;
                    state.AssistActivationReleaseUntil = 0f;
                }
                else
                {
                    state.HoverVerticalReleaseUntil = Time.time + HoverModeTransitionSeconds;
                    state.HoverMomentumHoldUntil = Time.time + HoverMomentumHoldSeconds;
                }
            }
        }

        private static bool VehicleHasFuel(EntityVehicle vehicle)
        {
            Vehicle data = vehicle != null ? vehicle.GetVehicle() : null;
            return data == null || data.GetFuelLevel() > 0f;
        }

        private static void RestoreRealStick(EntityVehicle vehicle, EntityPlayerLocal player)
        {
            if (vehicle?.movementInput == null)
            {
                return;
            }

            PlayerActionsVehicle actions = player != null ? LocalPlayerUI.GetUIForPlayer(player)?.playerInput?.VehicleActions : null;
            if (actions == null)
            {
                vehicle.movementInput.moveForward = 0f;
                return;
            }

            float forward = actions.Move.Y;
            if (EffectManager.GetValue(PassiveEffects.FlipControls, null, 0f, player) > 0f)
            {
                forward *= -1f;
            }

            vehicle.movementInput.moveForward = forward;
        }

        internal static void ApplyGroundVerticalLift(EntityVehicle vehicle)
        {
            if (vehicle?.vehicleRB == null || vehicle.GetWheelsOnGround() <= 0)
            {
                return;
            }

            Transform body = vehicle.PhysicsTransform != null ? vehicle.PhysicsTransform : vehicle.transform;
            if (body == null)
            {
                return;
            }

            float lift = 0f;
            Array motors = LiveMotorsField?.GetValue(vehicle) as Array;
            if (motors != null && motors.Length > 0)
            {
                object motor = motors.GetValue(0);
                if (motor != null)
                {
                    float rpm = ReadFloat(motor, "rpm");
                    float rpmMax = ReadFloat(motor, "rpmMax");
                    if (rpmMax > 0.001f)
                    {
                        lift += 0.195f * Mathf.Clamp01(rpm / rpmMax);
                    }
                }
            }

            if (vehicle.movementInput != null && vehicle.movementInput.moveForward > 0f)
            {
                lift += 0.03f * vehicle.movementInput.moveForward;
            }

            if (lift <= 0.001f)
            {
                return;
            }

            Vector3 correction = (Vector3.up - body.up) * lift;
            if (correction.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            vehicle.vehicleRB.AddForce(correction, ForceMode.VelocityChange);
        }

        private static float ReadFloat(object target, string fieldName)
        {
            object value = AccessTools.Field(target.GetType(), fieldName)?.GetValue(target);
            return value is float number ? number : 0f;
        }

        private static void ApplyHeliCruiseThrottle(EntityVehicle vehicle, EntityPlayerLocal player, FlightLevelStateStore.State state)
        {
            if (vehicle?.movementInput == null)
            {
                return;
            }

            bool tiltedEnough = Patch_EntityVehicle_PhysicsFixedUpdate.IsHeliNoseForwardEnough(vehicle);
            if (state.HeliForwardThrustOn)
            {
                if (Patch_EntityVehicle_PhysicsFixedUpdate.IsHeliNoseTooFlatForThrust(vehicle))
                {
                    state.HeliForwardThrustOn = false;
                }
            }
            else if (tiltedEnough)
            {
                state.HeliForwardThrustOn = true;
                FlightAssistCruise.EngageCruise(vehicle, player, sprintLocked: true);
            }

            if (state.HeliForwardThrustOn)
            {
                return;
            }

            RestoreRealStick(vehicle, player);
        }

        private static void ResolveControlPattern(EntityVehicle vehicleEntity, EntityPlayerLocal player, FlightLevelStateStore.State state)
        {
            FlightControlPattern resolved = FlightControlPattern.HelicopterLike;
            if (TryResolveControlPatternFromControlMechanism(vehicleEntity, out FlightControlPattern detected))
            {
                resolved = detected;
            }

            if (state.ControlPattern == resolved)
            {
                return;
            }

            FlightControlPattern previous = state.ControlPattern;
            state.ControlPattern = resolved;
            if (previous == FlightControlPattern.Unknown)
            {
                return;
            }

            state.HardLockActive = false;
            state.PitchSettledTime = 0f;
            state.SmoothedTargetPitchDeg = 0f;
            state.TargetAltitude = vehicleEntity.vehicleRB != null ? vehicleEntity.vehicleRB.position.y : vehicleEntity.position.y;
            bool cruiseOn = FlightAssistCruise.IsCruiseOn(vehicleEntity);
            if (resolved == FlightControlPattern.HelicopterLike)
            {
                state.HeliHoverOn = !cruiseOn && state.PlaneHeightOn;
                state.Mode = cruiseOn ? LockMode.ForwardOnPlane : LockMode.Hover;
                if (!cruiseOn && state.HeliHoverOn)
                {
                    state.HoverVerticalReleaseUntil = Time.time + HoverModeTransitionSeconds;
                    state.HoverHorizontalReleaseUntil = Time.time + HoverModeTransitionSeconds;
                    state.HoverMomentumHoldUntil = Time.time + HoverMomentumHoldSeconds;
                }
            }
            else
            {
                if (state.HeliHoverOn)
                {
                    state.PlaneHeightOn = true;
                    state.HeliHoverOn = false;
                }

                state.Mode = LockMode.ForwardOnPlane;
                state.HoverVerticalReleaseUntil = 0f;
                state.HoverHorizontalReleaseUntil = 0f;
                state.HoverMomentumHoldUntil = 0f;
                if (!state.PlaneHeightOn)
                {
                    state.Enabled = false;
                    state.HardLockActive = false;
                }
            }

            state.LastMode = state.Mode;
            state.AssistActivationReleaseUntil = Time.time + AssistActivationTransitionSeconds;
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

            bool hotkeyHeld = !keyboardBlockedByTextEntry && AutoRunBindingManager.IsActivationPressed(player);

            pressed = hotkeyHeld && !state.WasHotkeyHeld;
            state.WasHotkeyHeld = hotkeyHeld;
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
            state.HoverPilotOwnsPitch = false;
            state.WasHoverPilotPitching = false;
            state.HoverHeldPitchDeg = 0f;
            state.HeliManual = false;
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
            state.LockSettledOnce = false;
            state.PlaneYLockAutoRunCoupled = false;
            state.PlaneYLockUsingExternalAutoRun = false;
            Debug.Log("[FlightLevelAssist] Assist armed on press for vehicle " + vehicle.entityId + ".");
        }

        internal static void DisableAssist(FlightLevelStateStore.State state, EntityVehicle vehicle = null, bool suppressExternalAutoRunDisable = false)
        {
            if (state.ControlPattern == FlightControlPattern.HelicopterLike && vehicle != null)
            {
                FlightAssistCruise.ReleaseCruise(vehicle, vehicle.AttachedMainEntity as EntityPlayerLocal);
            }

            state.Enabled = false;
            state.HardLockActive = false;
            state.PlaneHeightOn = false;
            state.HeliHoverOn = false;
            state.HeliHoverReturnToCruise = false;
            state.HeliHoverReturnSprint = false;
            state.HeliForwardThrustOn = false;
            state.LockSettledOnce = false;
            state.TimeSinceArmed = 0f;
            state.PitchSettledTime = 0f;
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

        internal static bool IsLikelyFlyingVehicle(EntityVehicle vehicle)
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
        private const float HeliCruiseVerticalSpeed = 4f;
        private const float HeliCruiseVerticalAccel = 5f;
        private const float HeliHoverVerticalBoost = 0.015f;
        private const float HelicopterForwardTurboPitchBonusDeg = -2f;
        private const float HelicopterForwardHardMinPitchDeg = -33f;
        private const float HelicopterForwardPitchEaseBandDeg = 8f;
        private const float HelicopterForwardPitchEaseCut = 0.8f;
        private const float HelicopterForwardPitchEaseDampBoost = 14f;
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
        private const float HelicopterForwardThrustReleasePitchDeg = -12f;

        internal static bool IsHeliNoseForwardEnough(EntityVehicle vehicle)
        {
            return NosePitchDeg(vehicle) <= HelicopterForwardHardLockEngagePitchDeg + HelicopterForwardHardLockPitchToleranceDeg;
        }

        internal static bool IsHeliNoseTooFlatForThrust(EntityVehicle vehicle)
        {
            return NosePitchDeg(vehicle) > HelicopterForwardThrustReleasePitchDeg;
        }

        private static float NosePitchDeg(EntityVehicle vehicle)
        {
            Transform transform = vehicle != null ? vehicle.PhysicsTransform : null;
            if (transform == null && vehicle != null)
            {
                transform = vehicle.transform;
            }

            if (transform == null)
            {
                return 0f;
            }

            return Mathf.Asin(Mathf.Clamp(transform.forward.y, -1f, 1f)) * Mathf.Rad2Deg;
        }

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
                Patch_FlightAssist_MoveByAttachedEntity.DisableAssist(state, __instance, suppressExternalAutoRunDisable: true);
                return;
            }

            if (!Patch_FlightAssist_MoveByAttachedEntity.IsEnabledFor(__instance))
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
                Patch_FlightAssist_MoveByAttachedEntity.DisableAssist(state, __instance);
                Debug.Log("[FlightLevelAssist] Canceled due to no fuel for vehicle " + __instance.entityId + ".");
                return;
            }

            // Only apply crash-style cancel once hard lock is active so extreme starting angles
            // can still be leveled during the pre-lock correction phase.
            if (state.HardLockActive && (Mathf.Abs(pitchDeg) >= CrashPitchCancelDeg || Mathf.Abs(pitchRateDegPerSec) >= CrashPitchRateCancelDegPerSec))
            {
                Patch_FlightAssist_MoveByAttachedEntity.DisableAssist(state, __instance);
                state.HoverResumeAfter = Time.time + 1.5f;
                Debug.Log("[FlightLevelAssist] Canceled due to crash-level pitch change for vehicle " + __instance.entityId + ".");
                return;
            }

            if (state.ControlPattern == FlightControlPattern.HelicopterLike)
            {
                Patch_FlightAssist_MoveByAttachedEntity.ApplyGroundVerticalLift(__instance);
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
                        state.HardLockActive = false;
                        state.PitchSettledTime = 0f;
                        state.LowMomentumSince = 0f;
                        Patch_FlightAssist_MoveByAttachedEntity.CancelPlaneHeightKeepCruise(state);
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

            bool boostInputActive = __instance.movementInput != null
                && __instance.movementInput.running
                && __instance.movementInput.moveForward > 0.05f;
            bool heliForwardMode = state.HardLockActive
                && state.ControlPattern == FlightControlPattern.HelicopterLike
                && state.Mode == LockMode.ForwardOnPlane;
            bool helicopterForwardRequested = state.ControlPattern == FlightControlPattern.HelicopterLike
                && state.Mode == LockMode.ForwardOnPlane;
            bool helicopterHoverRequested = state.ControlPattern == FlightControlPattern.HelicopterLike
                && state.Mode == LockMode.Hover;

            if (!state.HardLockActive && helicopterForwardRequested)
            {
                // Held W is the forward thrust while the nose is tilted. Do not treat it as a climb.
                state.TargetAltitude = Mathf.Lerp(state.TargetAltitude, currentAltitude, TransitionTargetAltitudeLerpPerTick);
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
                bool cruiseHeightHeld = state.HeliCruiseClimbHeld || state.HeliCruiseDescendHeld;
                bool outsideAltitudeBand = !cruiseHeightHeld
                    && (Mathf.Abs(altitudeError) > HelicopterForwardAltitudeBand
                    || Mathf.Abs(verticalSpeed) > HelicopterForwardVerticalSpeedBand);
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
                if (!cruiseHeightHeld && altitudeError > HelicopterForwardDiveGuardAltitude)
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

            bool pilotPitching = __instance.movementInput != null
                && (__instance.movementInput.jump || __instance.movementInput.down);
            bool hoverYieldsToPitch = helicopterHoverRequested && pilotPitching;
            if (!hoverYieldsToPitch && Mathf.Abs(pitchErrorDeg) > PitchDeadZoneDeg)
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
                if (helicopterForwardRequested)
                {
                    float nearTarget = 1f - Mathf.Clamp01(Mathf.Abs(pitchErrorDeg) / HelicopterForwardPitchEaseBandDeg);
                    float roomToStop = pitchDeg - HelicopterForwardHardMinPitchDeg;
                    float nearStop = 1f - Mathf.Clamp01(roomToStop / HelicopterForwardPitchEaseBandDeg);
                    float ease = Mathf.Max(nearTarget, nearStop);
                    if (pitchTorque > 0f)
                    {
                        pitchTorque *= 1f - (ease * HelicopterForwardPitchEaseCut);
                    }

                    pitchTorque -= pitchRateDegPerSec * PitchRateDampingGain * ease * HelicopterForwardPitchEaseDampBoost;
                    pitchTorque = Mathf.Clamp(pitchTorque, 0f - maxTorque, maxTorque);
                }

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

                bool momentumReady = !requiresMomentumForYLock || currentForwardSpeed >= basicPlaneMomentumThreshold;
                if (state.PitchSettledTime >= PitchSettleDurationSec && momentumReady)
                {
                    state.HardLockActive = true;
                    state.LockSettledOnce = true;
                    if (!helicopterForwardRequested)
                    {
                        state.TargetAltitude = currentAltitude;
                    }
                    state.AssistActivationReleaseUntil = Time.time + AssistActivationTransitionSeconds;

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

                bool cruiseHeightHeld = helicopterForwardRequested
                    && (state.HeliCruiseClimbHeld || state.HeliCruiseDescendHeld);
                if (helicopterForwardRequested && state.WasHeliCruiseHeightHeld && !cruiseHeightHeld)
                {
                    state.TargetAltitude = __instance.vehicleRB.position.y;
                }

                state.WasHeliCruiseHeightHeld = cruiseHeightHeld;

                bool forwardTransitionGrace = heliForwardMode && Time.time < state.ForwardModeTransitionUntil;
                bool hoverReleaseGrace = helicopterHoverMode && Time.time < state.HoverVerticalReleaseUntil;
                bool hoverMomentumHold = helicopterHoverMode && Time.time < state.HoverMomentumHoldUntil;
                bool activationGrace = Time.time < state.AssistActivationReleaseUntil;
                bool hoverPilotPitch = helicopterHoverMode
                    && __instance.movementInput != null
                    && (__instance.movementInput.jump || __instance.movementInput.down);
                bool softenVerticalLock = forwardTransitionGrace || hoverReleaseGrace || activationGrace || hoverPilotPitch || cruiseHeightHeld;
                float forwardLockBlend = heliForwardMode
                    ? Mathf.Clamp01(state.ForwardAlignTime / ForwardAlignHoldSeconds)
                    : 1f;
                Vector3 position = __instance.vehicleRB.position;
                if (heliForwardMode && (forwardTransitionGrace || forwardLockBlend < 0.999f))
                {
                    state.TargetAltitude = Mathf.Lerp(state.TargetAltitude, position.y, TransitionTargetAltitudeLerpPerTick);
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
                            if (cruiseHeightHeld)
                            {
                                state.TargetAltitude = Mathf.Lerp(state.TargetAltitude, position.y, TransitionTargetAltitudeLerpPerTick);
                            }
                            else
                            {
                                float followStrength = Mathf.Lerp(
                                    TransitionTargetAltitudeLerpPerTick,
                                    HoverVerticalLockPositionLerpPerTick,
                                    forwardLockBlend);
                                float correctedY = Mathf.Lerp(position.y, state.TargetAltitude, followStrength);
                                __instance.vehicleRB.MovePosition(new Vector3(position.x, correctedY, position.z));
                            }
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
                        if (heliForwardMode && !cruiseHeightHeld)
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

                if (heliForwardMode && cruiseHeightHeld)
                {
                    Vector3 cruiseVelocity = __instance.vehicleRB.velocity;
                    float verticalSpeed = state.HeliCruiseClimbHeld ? HeliCruiseVerticalSpeed : -HeliCruiseVerticalSpeed;
                    float easedVertical = Mathf.MoveTowards(cruiseVelocity.y, verticalSpeed, HeliCruiseVerticalAccel * Time.fixedDeltaTime);
                    __instance.vehicleRB.velocity = new Vector3(cruiseVelocity.x, easedVertical, cruiseVelocity.z);
                }
                else if (helicopterHoverMode && (state.HeliCruiseClimbHeld || state.HeliCruiseDescendHeld))
                {
                    float hoverNudge = state.HeliCruiseClimbHeld ? HeliHoverVerticalBoost : -HeliHoverVerticalBoost;
                    __instance.vehicleRB.AddForce(Vector3.up * hoverNudge, ForceMode.VelocityChange);
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
