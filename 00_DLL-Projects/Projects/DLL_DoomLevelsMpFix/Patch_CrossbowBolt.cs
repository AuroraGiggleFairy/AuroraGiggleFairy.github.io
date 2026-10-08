using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// The compound crossbow has no bolt in the model. The loaded round is a
	/// spawned mesh on ProjectileJoint, and that same mesh is the shot.
	/// Auto reload was starting while the fire event still owned the weapon,
	/// so the string never drew and the next bolt was never placed.
	/// A loaded mesh can also be taken by another player. Only a bolt that
	/// has stuck in the world can be picked up.
	/// </summary>
	internal static class CrossbowBolt
	{
		private static readonly int ReloadState = Animator.StringToHash("AdvancedCompoundCrossbowReload");

		internal static bool IsCrossbow(ItemActionLauncher launcher)
		{
			string[] names = launcher?.MagazineItemNames;
			if (names == null)
			{
				return false;
			}

			for (int i = 0; i < names.Length; i++)
			{
				if (!string.IsNullOrEmpty(names[i]) && names[i].IndexOf("Crossbow", System.StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}
			}

			return false;
		}

		internal static void KeepController(Animator anim, RuntimeAnimatorController original)
		{
			if (anim == null || original == null || anim.runtimeAnimatorController == original)
			{
				return;
			}

			RuntimeAnimatorController swapped = anim.runtimeAnimatorController;
			anim.runtimeAnimatorController = original;
			bool originalHas = anim.HasState(0, ReloadState);
			anim.runtimeAnimatorController = swapped;
			if (originalHas && !anim.HasState(0, ReloadState))
			{
				anim.runtimeAnimatorController = original;
			}
		}

		internal static void DrawString(Transform model)
		{
			Animator anim = model != null ? model.GetComponent<Animator>() : null;
			if (anim == null || !anim.HasState(0, ReloadState))
			{
				return;
			}

			anim.CrossFade(ReloadState, 0.05f, 0, 0f);
		}

		internal static void EnsureBolt(ItemActionLauncher launcher, ItemActionData action)
		{
			ItemActionLauncher.ItemActionDataLauncher data = action as ItemActionLauncher.ItemActionDataLauncher;
			ItemValue item = action?.invData?.itemValue;
			if (data == null || item == null)
			{
				return;
			}

			Transform joint = Joint(action.invData.model);
			if (joint != null)
			{
				data.projectileJointT = joint;
			}

			List<Transform> bolts = data.projectileTs;
			for (int i = bolts.Count - 1; i >= 0; i--)
			{
				Transform bolt = bolts[i];
				if (bolt == null)
				{
					bolts.RemoveAt(i);
					continue;
				}

				Place(bolt, joint);
			}

			if (ItemInventoryAccess.MetaOf(item) <= 0 || bolts.Count > 0)
			{
				return;
			}

			Transform shot = launcher.instantiateProjectile(action);
			if (shot != null)
			{
				bolts.Add(shot);
			}
		}

		internal static void Place(Transform bolt, Transform joint)
		{
			if (bolt == null || joint == null || bolt.parent == joint)
			{
				return;
			}

			bolt.SetParent(joint, false);
			bolt.localPosition = Vector3.zero;
			bolt.localRotation = Quaternion.identity;
			bolt.localScale = Vector3.one;
			Utils.SetLayerRecursively(bolt.gameObject, joint.gameObject.layer);
		}

		internal static Transform Joint(Transform model)
		{
			if (model == null)
			{
				return null;
			}

			if (model.name == "ProjectileJoint")
			{
				return model;
			}

			int count = model.childCount;
			for (int i = 0; i < count; i++)
			{
				Transform found = Joint(model.GetChild(i));
				if (found != null)
				{
					return found;
				}
			}

			return null;
		}
	}

	[HarmonyPatch(typeof(ProjectileMoveScript), nameof(ProjectileMoveScript.TryCollect))]
	internal static class Patch_CrossbowBoltNoTake
	{
		private static bool Prefix(ProjectileMoveScript __instance, ref bool __result)
		{
			if (__instance.state == ProjectileMoveScript.State.Sticky)
			{
				return true;
			}

			__result = false;
			return false;
		}
	}

	[HarmonyPatch(typeof(AvatarMultiBodyController), nameof(AvatarMultiBodyController.SetInRightHand))]
	internal static class Patch_CrossbowKeepStringAnim
	{
		private static void Prefix(Transform _transform, ref RuntimeAnimatorController __state)
		{
			__state = _transform != null ? _transform.GetComponent<Animator>()?.runtimeAnimatorController : null;
		}

		private static void Postfix(AvatarMultiBodyController __instance, Transform _transform, RuntimeAnimatorController __state)
		{
			ItemClass held = __instance?.Entity?.inventory?.holdingItem;
			if (held == null || !held.HasAnyTags(FastTags<TagGroup.Global>.Parse("crossbow")))
			{
				return;
			}

			Animator anim = _transform != null ? _transform.GetComponent<Animator>() : null;
			CrossbowBolt.KeepController(anim, __state);
		}
	}

	[HarmonyPatch(typeof(ItemActionLauncher), nameof(ItemActionLauncher.ReloadGun))]
	internal static class Patch_CrossbowReload
	{
		private static void Prefix(ItemActionLauncher __instance, ItemActionData _actionData)
		{
			if (!CrossbowBolt.IsCrossbow(__instance))
			{
				return;
			}

			EntityAlive entity = ItemInventoryAccess.Holding(_actionData?.invData);
			if (entity == null || entity.isEntityRemote)
			{
				return;
			}

			entity.emodel?.avatarController?.CancelEvent("WeaponFire");
		}

		private static void Postfix(ItemActionLauncher __instance, ItemActionData _actionData)
		{
			if (!CrossbowBolt.IsCrossbow(__instance))
			{
				return;
			}

			EntityAlive entity = ItemInventoryAccess.Holding(_actionData?.invData);
			if (entity == null || entity.isEntityRemote)
			{
				return;
			}

			CrossbowBolt.DrawString(_actionData?.invData?.model);
		}
	}

	[HarmonyPatch(typeof(ItemActionLauncher), nameof(ItemActionLauncher.instantiateProjectile))]
	internal static class Patch_CrossbowPlaceBolt
	{
		private static void Postfix(ItemActionLauncher __instance, ItemActionData _actionData, Transform __result)
		{
			if (__result == null || !CrossbowBolt.IsCrossbow(__instance))
			{
				return;
			}

			ItemActionLauncher.ItemActionDataLauncher data = _actionData as ItemActionLauncher.ItemActionDataLauncher;
			Transform joint = data != null ? data.projectileJointT : null;
			if (joint == null)
			{
				joint = CrossbowBolt.Joint(_actionData?.invData?.model);
				if (data != null)
				{
					data.projectileJointT = joint;
				}
			}

			CrossbowBolt.Place(__result, joint);
		}
	}

	[HarmonyPatch(typeof(ItemActionRanged), nameof(ItemActionRanged.OnHoldingUpdate))]
	internal static class Patch_CrossbowShowLoadedBolt
	{
		private static void Postfix(ItemActionRanged __instance, ItemActionData _actionData)
		{
			ItemActionLauncher launcher = __instance as ItemActionLauncher;
			if (launcher == null || !CrossbowBolt.IsCrossbow(launcher))
			{
				return;
			}

			CrossbowBolt.EnsureBolt(launcher, _actionData);
		}
	}
}
