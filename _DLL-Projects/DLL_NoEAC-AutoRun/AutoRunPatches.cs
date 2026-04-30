using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace AutoRun
{
    internal static class AutoRunStateStore
    {
        internal sealed class State
        {
            public bool VehicleEnabled;
            public float VehicleLastForwardTapTime;
            public bool VehicleWasForwardPressed;
            public bool VehicleForwardForcedLastFrame;
            public bool VehicleTurboEnabled;
            public float VehicleLastTurboTapTime;
            public bool VehicleWasTurboPressed;
            public float VehicleSprintResumeTime;

            public bool OnFootEnabled;
            public float OnFootLastForwardTapTime;
            public bool OnFootWasForwardPressed;
            public bool OnFootForwardForcedLastFrame;
            public bool OnFootSprintEnabled;
            public float OnFootLastSprintTapTime;
            public bool OnFootWasSprintPressed;
            public float OnFootSprintResumeTime;
            public bool OnFootRunningForcedLastFrame;
        }

        private static readonly Dictionary<int, State> VehicleStates = new Dictionary<int, State>();
        private static readonly Dictionary<int, State> PlayerStates = new Dictionary<int, State>();

        private const string CVarVehicleEnabled = "agf_autorun_vehicle_enabled";
        private const string CVarOnFootEnabled = "agf_autorun_onfoot_enabled";

        public static State GetOrCreateForVehicle(EntityVehicle vehicle)
        {
            if (vehicle == null)
            {
                return null;
            }

            if (!VehicleStates.TryGetValue(vehicle.entityId, out State state))
            {
                state = new State();
                VehicleStates[vehicle.entityId] = state;
            }

            return state;
        }

        public static State GetOrCreateForPlayer(EntityPlayerLocal player)
        {
            if (player == null)
            {
                return null;
            }

            if (!PlayerStates.TryGetValue(player.entityId, out State state))
            {
                state = new State();
                PlayerStates[player.entityId] = state;
            }

            return state;
        }

        public static void SetVehicleIndicator(EntityPlayerLocal player, bool enabled)
        {
            if (player == null)
            {
                return;
            }

            player.SetCVar(CVarVehicleEnabled, enabled ? 1f : 0f);
        }

        public static void SetOnFootIndicator(EntityPlayerLocal player, bool enabled)
        {
            if (player == null)
            {
                return;
            }

            player.SetCVar(CVarOnFootEnabled, enabled ? 1f : 0f);
        }

        public static void DisableAllVehicleStates()
        {
            foreach (KeyValuePair<int, State> kvp in VehicleStates)
            {
                State state = kvp.Value;
                if (state == null)
                {
                    continue;
                }

                state.VehicleEnabled = false;
                state.VehicleWasForwardPressed = false;
                state.VehicleTurboEnabled = false;
                state.VehicleWasTurboPressed = false;
                state.VehicleLastForwardTapTime = 0f;
                state.VehicleLastTurboTapTime = 0f;
                state.VehicleSprintResumeTime = 0f;
                state.VehicleForwardForcedLastFrame = false;
            }
        }
    }

    [HarmonyPatch(typeof(EntityVehicle), nameof(EntityVehicle.MoveByAttachedEntity))]
    internal static class Patch_EntityVehicle_MoveByAttachedEntity
    {
        private const float ForwardPressThreshold = 0.55f;
        private const float ReversePressThreshold = -0.15f;
        private const float DoubleTapWindowSeconds = 0.33f;
        private const float MinDoubleTapGapSeconds = 0.10f;
        private const float TurboDoubleTapWindowSeconds = 0.33f;
        private const float MinTurboDoubleTapGapSeconds = 0.10f;
        private const float SprintResumeDelaySeconds = 0.75f;

        private static bool IsForwardKeyHeld()
        {
            return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        }

        private static bool CanApplyLockedSprint(EntityPlayerLocal player, ref float sprintResumeTime)
        {
            if (player == null)
            {
                return true;
            }

            if (player.bExhausted)
            {
                float resumeAt = Time.time + SprintResumeDelaySeconds;
                if (resumeAt > sprintResumeTime)
                {
                    sprintResumeTime = resumeAt;
                }
                return false;
            }

            return Time.time >= sprintResumeTime;
        }

        public static void Postfix(EntityVehicle __instance, EntityPlayerLocal _player)
        {
            if (__instance == null || _player == null || __instance.movementInput == null)
            {
                return;
            }

            AutoRunStateStore.State state = AutoRunStateStore.GetOrCreateForVehicle(__instance);
            if (state == null)
            {
                return;
            }

            // Passengers should not affect driver auto-run state.
            if (_player != __instance.AttachedMainEntity)
            {
                return;
            }

            if (__instance.AttachedMainEntity == null)
            {
                state.VehicleEnabled = false;
                state.VehicleWasForwardPressed = false;
                state.VehicleForwardForcedLastFrame = false;
                state.VehicleTurboEnabled = false;
                state.VehicleWasTurboPressed = false;
                AutoRunStateStore.SetVehicleIndicator(_player, enabled: false);
                return;
            }

            bool forwardPressed = __instance.movementInput.moveForward >= ForwardPressThreshold;
            if (state.VehicleForwardForcedLastFrame && !IsForwardKeyHeld())
            {
                // Ignore our previously injected forward value so real forward key edges are detectable.
                forwardPressed = false;
            }
            state.VehicleForwardForcedLastFrame = false;

            bool reversePressed = __instance.movementInput.moveForward <= ReversePressThreshold;
            bool turboPressed = __instance.movementInput.running;

            if (forwardPressed && !state.VehicleWasForwardPressed)
            {
                if (state.VehicleEnabled)
                {
                    // While active, pressing forward again returns full control to the player.
                    state.VehicleEnabled = false;
                    state.VehicleTurboEnabled = false;
                    state.VehicleSprintResumeTime = 0f;
                    state.VehicleLastForwardTapTime = 0f;
                    AutoRunStateStore.SetVehicleIndicator(_player, enabled: false);
                }
                else
                {
                    float now = Time.time;
                    float tapDelta = now - state.VehicleLastForwardTapTime;
                    if (tapDelta <= DoubleTapWindowSeconds && tapDelta >= MinDoubleTapGapSeconds)
                    {
                        state.VehicleEnabled = true;
                        // If shift is held while enabling auto-run, latch turbo too.
                        state.VehicleTurboEnabled = turboPressed;
                        state.VehicleSprintResumeTime = 0f;
                        AutoRunStateStore.SetVehicleIndicator(_player, enabled: true);
                        state.VehicleLastForwardTapTime = 0f;
                    }
                    else
                    {
                        state.VehicleLastForwardTapTime = now;
                    }
                }
            }

            state.VehicleWasForwardPressed = forwardPressed;

            if (!state.VehicleEnabled)
            {
                state.VehicleTurboEnabled = false;
                state.VehicleSprintResumeTime = 0f;
                AutoRunStateStore.SetVehicleIndicator(_player, enabled: false);
                state.VehicleWasTurboPressed = turboPressed;
                return;
            }

            if (turboPressed && !state.VehicleWasTurboPressed)
            {
                if (state.VehicleTurboEnabled)
                {
                    // While locked, any manual sprint press returns sprint control to the player.
                    state.VehicleTurboEnabled = false;
                    state.VehicleLastTurboTapTime = 0f;
                }
                else
                {
                    float now = Time.time;
                    float tapDelta = now - state.VehicleLastTurboTapTime;
                    if (tapDelta <= TurboDoubleTapWindowSeconds && tapDelta >= MinTurboDoubleTapGapSeconds)
                    {
                        state.VehicleTurboEnabled = true;
                        state.VehicleLastTurboTapTime = 0f;
                    }
                    else
                    {
                        state.VehicleLastTurboTapTime = now;
                    }
                }
            }
            state.VehicleWasTurboPressed = turboPressed;

            if (reversePressed)
            {
                state.VehicleEnabled = false;
                state.VehicleTurboEnabled = false;
                state.VehicleSprintResumeTime = 0f;
                AutoRunStateStore.SetVehicleIndicator(_player, enabled: false);
                return;
            }

            // Emulate held forward input, with optional locked turbo.
            __instance.movementInput.moveForward = 1f;
            state.VehicleForwardForcedLastFrame = true;
            if (state.VehicleTurboEnabled && CanApplyLockedSprint(_player, ref state.VehicleSprintResumeTime))
            {
                __instance.movementInput.running = true;
            }
            AutoRunStateStore.SetVehicleIndicator(_player, enabled: true);
        }
    }

    [HarmonyPatch(typeof(EntityPlayerLocal), nameof(EntityPlayerLocal.MoveByInput))]
    internal static class Patch_EntityPlayerLocal_MoveByInput
    {
        private const float DoubleTapWindowSeconds = 0.33f;
        private const float MinDoubleTapGapSeconds = 0.10f;
        private const float SprintDoubleTapWindowSeconds = 0.33f;
        private const float MinSprintDoubleTapGapSeconds = 0.10f;
        private const float SprintResumeDelaySeconds = 0.75f;

        private static bool IsForwardKeyHeld()
        {
            return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        }

        private static bool IsSprintKeyHeld()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        private static bool CanApplyLockedSprint(EntityPlayerLocal player, ref float sprintResumeTime)
        {
            if (player == null)
            {
                return true;
            }

            if (player.bExhausted)
            {
                float resumeAt = Time.time + SprintResumeDelaySeconds;
                if (resumeAt > sprintResumeTime)
                {
                    sprintResumeTime = resumeAt;
                }
                return false;
            }

            return Time.time >= sprintResumeTime;
        }

        public static void Prefix(EntityPlayerLocal __instance)
        {
            if (__instance == null)
            {
                return;
            }

            AutoRunStateStore.State state = AutoRunStateStore.GetOrCreateForPlayer(__instance);
            if (state == null)
            {
                return;
            }

            // On-foot auto-run is suspended while mounted; vehicle patch handles that mode.
            if (__instance.AttachedToEntity != null)
            {
                if (state.OnFootEnabled)
                {
                    state.OnFootEnabled = false;
                    AutoRunStateStore.SetOnFootIndicator(__instance, enabled: false);
                }
                state.OnFootSprintEnabled = false;
                state.OnFootSprintResumeTime = 0f;
                state.OnFootWasForwardPressed = false;
                state.OnFootForwardForcedLastFrame = false;
                state.OnFootWasSprintPressed = false;
                state.OnFootRunningForcedLastFrame = false;
                AutoRunStateStore.SetVehicleIndicator(__instance, enabled: false);
                return;
            }

            // Ensure vehicle indicator is off when the local player is no longer driving.
            AutoRunStateStore.DisableAllVehicleStates();
            AutoRunStateStore.SetVehicleIndicator(__instance, enabled: false);

            if (__instance.movementInput == null)
            {
                return;
            }

            bool forwardPressed = __instance.movementInput.moveForward >= 0.5f;
            if (state.OnFootForwardForcedLastFrame && !IsForwardKeyHeld())
            {
                // Ignore our previously injected forward value so real forward key edges are detectable.
                forwardPressed = false;
            }
            state.OnFootForwardForcedLastFrame = false;

            bool backPressed = __instance.movementInput.moveForward <= -0.5f;
            bool sprintPressed = __instance.movementInput.running;
            if (state.OnFootRunningForcedLastFrame && !IsSprintKeyHeld())
            {
                // Ignore the sprint value we injected last frame so double-tap detection
                // can still see real key-up/key-down edges while auto-run is active.
                sprintPressed = false;
            }
            state.OnFootRunningForcedLastFrame = false;

            if (forwardPressed && !state.OnFootWasForwardPressed)
            {
                if (state.OnFootEnabled)
                {
                    // While active, pressing forward again returns full control to the player.
                    state.OnFootEnabled = false;
                    state.OnFootSprintEnabled = false;
                    state.OnFootSprintResumeTime = 0f;
                    state.OnFootLastForwardTapTime = 0f;
                    AutoRunStateStore.SetOnFootIndicator(__instance, enabled: false);
                }
                else
                {
                    float now = Time.time;
                    float tapDelta = now - state.OnFootLastForwardTapTime;
                    if (tapDelta <= DoubleTapWindowSeconds && tapDelta >= MinDoubleTapGapSeconds)
                    {
                        state.OnFootEnabled = true;
                        // If shift is held while enabling auto-run, latch sprint too.
                        state.OnFootSprintEnabled = sprintPressed;
                        state.OnFootSprintResumeTime = 0f;
                        AutoRunStateStore.SetOnFootIndicator(__instance, enabled: true);
                        state.OnFootLastForwardTapTime = 0f;
                    }
                    else
                    {
                        state.OnFootLastForwardTapTime = now;
                    }
                }
            }

            state.OnFootWasForwardPressed = forwardPressed;

            if (!state.OnFootEnabled)
            {
                state.OnFootSprintEnabled = false;
                state.OnFootSprintResumeTime = 0f;
                state.OnFootRunningForcedLastFrame = false;
                AutoRunStateStore.SetOnFootIndicator(__instance, enabled: false);
                state.OnFootWasSprintPressed = sprintPressed;
                return;
            }

            if (sprintPressed && !state.OnFootWasSprintPressed)
            {
                if (state.OnFootSprintEnabled)
                {
                    // While locked, any manual sprint press returns sprint control to the player.
                    state.OnFootSprintEnabled = false;
                    state.OnFootLastSprintTapTime = 0f;
                }
                else
                {
                    float now = Time.time;
                    float tapDelta = now - state.OnFootLastSprintTapTime;
                    if (tapDelta <= SprintDoubleTapWindowSeconds && tapDelta >= MinSprintDoubleTapGapSeconds)
                    {
                        state.OnFootSprintEnabled = true;
                        state.OnFootLastSprintTapTime = 0f;
                    }
                    else
                    {
                        state.OnFootLastSprintTapTime = now;
                    }
                }
            }
            state.OnFootWasSprintPressed = sprintPressed;

            if (backPressed)
            {
                state.OnFootEnabled = false;
                state.OnFootSprintEnabled = false;
                state.OnFootSprintResumeTime = 0f;
                state.OnFootForwardForcedLastFrame = false;
                state.OnFootRunningForcedLastFrame = false;
                AutoRunStateStore.SetOnFootIndicator(__instance, enabled: false);
                return;
            }

            if (__instance.movementInput != null)
            {
                __instance.movementInput.moveForward = 1f;
                state.OnFootForwardForcedLastFrame = true;
                if (state.OnFootSprintEnabled && CanApplyLockedSprint(__instance, ref state.OnFootSprintResumeTime))
                {
                    __instance.movementInput.running = true;
                    state.OnFootRunningForcedLastFrame = true;
                }
            }

            AutoRunStateStore.SetOnFootIndicator(__instance, enabled: true);
        }
    }
}
