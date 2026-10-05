using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// OmniSprint's swim postfix scales the finished throttle's world X while sprint is held.
	/// By then that axis is east/west, and a third-person move vector is already a world direction,
	/// so every swim-sprint input keeps the heading from when you entered the water.
	/// After that postfix, point the same horizontal speed along the current camera.
	/// With OmniSprint absent, this does nothing. Walk-swim is unchanged.
	/// </summary>
	[HarmonyPatch(typeof(EntityPlayerLocal), "SwimModeUpdateThrottle")]
	[HarmonyPriority(Priority.Last)]
	internal static class Patch_SwimSprintDirection
	{
		private const float StrafeScale = 0.7f;

		private static int _omni = -1;

		private static void Postfix(EntityPlayerLocal __instance)
		{
			if (__instance == null || !__instance.MovementRunning || __instance.vp_FPController == null || !OmniSprintLoaded())
			{
				return;
			}

			float forward;
			float strafe;
			if (!TryReadInput(__instance, out forward, out strafe))
			{
				return;
			}

			Vector3 look = __instance.GetLookVector();
			Vector3 flatLook = new Vector3(look.x, 0f, look.z);
			if (flatLook.sqrMagnitude < 0.0001f)
			{
				return;
			}

			flatLook.Normalize();
			Vector3 flatRight = new Vector3(flatLook.z, 0f, -flatLook.x);
			Vector3 desired = flatLook * forward + flatRight * (strafe * StrafeScale);
			if (desired.sqrMagnitude < 0.0001f)
			{
				return;
			}

			Vector3 throttle = __instance.vp_FPController.m_MotorThrottle;
			throttle.x *= StrafeScale;
			Vector3 horizontal = new Vector3(throttle.x, 0f, throttle.z);
			float speed = horizontal.magnitude;
			if (speed < 0.0001f)
			{
				return;
			}

			desired.y = 0f;
			desired.Normalize();
			throttle.x = desired.x * speed;
			throttle.z = desired.z * speed;
			__instance.vp_FPController.m_MotorThrottle = throttle;
		}

		private static bool TryReadInput(EntityPlayerLocal player, out float forward, out float strafe)
		{
			forward = 0f;
			strafe = 0f;
			Vector3 move = player.moveDirection;
			bool thirdPerson = !player.bFirstPersonView;
			bool locked = player.vp_FPCamera != null && player.vp_FPCamera.Locked3rdPerson;
			if (thirdPerson && locked)
			{
				Vector3 bodyForward = player.transform.forward;
				Vector3 bodyRight = player.transform.right;
				bodyForward.y = 0f;
				bodyRight.y = 0f;
				if (bodyForward.sqrMagnitude < 0.0001f || bodyRight.sqrMagnitude < 0.0001f)
				{
					return false;
				}

				bodyForward.Normalize();
				bodyRight.Normalize();
				forward = Vector3.Dot(move, bodyForward);
				strafe = Vector3.Dot(move, bodyRight);
				return true;
			}

			if (thirdPerson && player.CameraRelativeMovement)
			{
				Vector3 look = player.GetLookVector();
				Vector3 axisForward = new Vector3(look.x, 0f, look.z);
				if (axisForward.sqrMagnitude < 0.0001f)
				{
					return false;
				}

				axisForward.Normalize();
				Vector3 axisRight = new Vector3(axisForward.z, 0f, -axisForward.x);
				forward = Vector3.Dot(move, axisForward);
				strafe = Vector3.Dot(move, axisRight);
				return true;
			}

			forward = move.z;
			strafe = move.x;
			return true;
		}

		private static bool OmniSprintLoaded()
		{
			if (_omni < 0)
			{
				_omni = AccessTools.TypeByName("Omni.Patch_EntityPlayerLocal_SwimModeUpdateThrottle") != null ? 1 : 0;
			}

			return _omni == 1;
		}
	}
}
