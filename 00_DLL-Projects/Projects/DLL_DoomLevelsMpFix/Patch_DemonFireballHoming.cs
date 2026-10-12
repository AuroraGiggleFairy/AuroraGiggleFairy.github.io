using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// Half of the Revenant's shots fly at their target and can turn toward
	/// that target at 120 degrees per second. A full circle takes 3 seconds.
	/// The owner rolls that once and sends the target id with the shot, so
	/// every machine turns the same missile. After 15 seconds the turn stops
	/// and the shot keeps the direction it has.
	/// </summary>
	internal static class DemonFireballHoming
	{
		internal const float TurnDegreesPerSecond = 120f;

		internal const float HomeSeconds = 15f;

		private static readonly FieldInfo VelocityField = AccessTools.Field(typeof(ProjectileMoveScript), "velocity");

		private static readonly Dictionary<int, HomingShot> Shots = new Dictionary<int, HomingShot>();

		private sealed class HomingShot
		{
			internal int TargetId;
			internal float Until;
		}

		internal static void Remember(ProjectileMoveScript shot, int targetId)
		{
			if (shot == null || targetId <= 0 || !DemonFireballAim.IsRevenantProjectile(shot))
			{
				return;
			}

			Shots[shot.GetInstanceID()] = new HomingShot
			{
				TargetId = targetId,
				Until = Time.time + HomeSeconds
			};
		}

		internal static void Forget(ProjectileMoveScript shot)
		{
			if (shot != null)
			{
				Shots.Remove(shot.GetInstanceID());
			}
		}

		internal static void Steer(ProjectileMoveScript shot)
		{
			if (shot == null || VelocityField == null)
			{
				return;
			}

			int id = shot.GetInstanceID();
			if (!Shots.TryGetValue(id, out HomingShot homing))
			{
				return;
			}

			if (Time.time >= homing.Until)
			{
				Shots.Remove(id);
				return;
			}

			World world = GameManager.Instance?.World;
			EntityAlive target = world?.GetEntity(homing.TargetId) as EntityAlive;
			if (target == null || target.IsDead())
			{
				return;
			}

			Vector3 velocity = (Vector3)VelocityField.GetValue(shot);
			float speed = velocity.magnitude;
			if (speed < 0.01f)
			{
				return;
			}

			Vector3 toTarget = target.getChestPosition() - (shot.transform.position + Origin.position);
			if (toTarget.sqrMagnitude < 0.0001f)
			{
				return;
			}

			float maxRadians = TurnDegreesPerSecond * Mathf.Deg2Rad * Time.fixedDeltaTime;
			Vector3 turned = Vector3.RotateTowards(velocity / speed, toTarget.normalized, maxRadians, 0f);
			VelocityField.SetValue(shot, turned * speed);
		}
	}

	/// <summary>
	/// The owner rolls the 50 percent. A negative effect value is the target id.
	/// Straight shots stay at zero, which is what these balls already send.
	/// </summary>
	[HarmonyPatch(typeof(ItemActionVomit), "GetActionEffectsValues")]
	internal static class Patch_DemonFireballHomingRoll
	{
		private static void Postfix(ItemActionVomit __instance, ItemActionData _actionData, ref int __result)
		{
			if (__result != 0 || __instance == null)
			{
				return;
			}

			EntityAlive entity = ItemInventoryAccess.Holding(_actionData?.invData);
			if (entity == null || entity.isEntityRemote || entity.rand == null || !DemonFireballAim.IsRevenantVomit(__instance, _actionData))
			{
				return;
			}

			EntityAlive target = entity.GetAttackTarget();
			if (target == null || target.IsDead() || target.entityId <= 0)
			{
				return;
			}

			if (entity.rand.RandomFloat >= 0.5f)
			{
				return;
			}

			__result = -target.entityId;
		}
	}

	[HarmonyPatch(typeof(ProjectileMoveScript), nameof(ProjectileMoveScript.Fire))]
	internal static class Patch_DemonFireballHomingArm
	{
		private static void Postfix(ProjectileMoveScript __instance)
		{
			DemonFireballHoming.Remember(__instance, DemonFireballAim.HomingTargetId);
		}
	}

	[HarmonyPatch(typeof(ProjectileMoveScript), "FixedUpdate")]
	internal static class Patch_DemonFireballHomingTurn
	{
		private static void Prefix(ProjectileMoveScript __instance)
		{
			DemonFireballHoming.Steer(__instance);
		}
	}

	[HarmonyPatch(typeof(ProjectileMoveScript), nameof(ProjectileMoveScript.OnDestroy))]
	internal static class Patch_DemonFireballHomingForget
	{
		private static void Prefix(ProjectileMoveScript __instance)
		{
			DemonFireballHoming.Forget(__instance);
		}
	}
}
