using System;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// A level teleport re-enables the player animator before the rig has its helper
	/// transforms. The constraints then fail to bind LeftBottle, ConstraintTargetSpine,
	/// and the bottle blend. Create the missing helpers on the animator that is about
	/// to enable, so the constraints have a real target.
	/// </summary>
	[HarmonyPatch(typeof(EntityPlayerLocal), nameof(EntityPlayerLocal.EnableCamera), typeof(bool))]
	internal static class Patch_PlayerRigTargets
	{
		private static readonly string[] Paths =
		{
			"Origin/Hips/Spine/LeftBottle",
			"Origin/Hips/LeftUpLeg/ConstraintTargetLeftUpLeg",
			"Origin/Hips/Spine/ConstraintTargetSpine",
			"RigConstraints_body",
			"RigConstraints_body/BottleBlendConstraint"
		};

		internal static void Apply(EntityPlayerLocal player)
		{
			if (player == null)
			{
				return;
			}

			Transform root = player.RootTransform;
			if (root == null)
			{
				return;
			}

			Type animatorType = Type.GetType("UnityEngine.Animator, UnityEngine.AnimationModule");
			if (root == null || animatorType == null)
			{
				return;
			}

			Component[] animators = root.GetComponentsInChildren(animatorType, true);
			for (int i = 0; i < animators.Length; i++)
			{
				Component animator = animators[i];
				if (animator == null || animator.GetComponent<RigTargetsReady>() != null)
				{
					continue;
				}

				Transform at = animator.transform;
				for (int p = 0; p < Paths.Length; p++)
				{
					Ensure(at, Paths[p]);
				}

				animator.gameObject.AddComponent<RigTargetsReady>();
			}
		}

		private static void Prefix(EntityPlayerLocal __instance, bool _b)
		{
			if (_b)
			{
				Apply(__instance);
			}
		}

		private static void Ensure(Transform root, string path)
		{
			Transform current = root;
			string[] parts = path.Split('/');
			for (int i = 0; i < parts.Length; i++)
			{
				Transform next = current.Find(parts[i]);
				if (next == null)
				{
					var created = new GameObject(parts[i]);
					created.transform.SetParent(current, false);
					next = created.transform;
				}

				current = next;
			}
		}
	}

	internal sealed class RigTargetsReady : MonoBehaviour
	{
	}
}
