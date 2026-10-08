using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// 3.2 destroyed and recreated the held gun after the first-person body
	/// rebuild, then showed the right hand and ran the gun's hand hide.
	/// 3.3 rebuilds that body and can show the right hand again afterwards,
	/// which leaves the new hands visible. Hide those same parts once the
	/// rebuild and the later show have both finished.
	/// </summary>
	internal static class HeldHands
	{
		private static readonly FastTags<TagGroup.Global> CustomGunTags = FastTags<TagGroup.Global>.Parse("IZY");

		private static readonly string[] HideNames = { "hands", "hands_A", "body" };

		internal static void Hide(EntityAlive alive)
		{
			EntityPlayerLocal player = alive as EntityPlayerLocal;
			if (player == null || !player.bFirstPersonView || player.inventory == null)
			{
				return;
			}

			if (player.PlayerUI != null && player.PlayerUI.windowManager.IsWindowOpen("character"))
			{
				return;
			}

			ItemClass held = player.inventory.holdingItem;
			if (held == null || !held.HasAnyTags(CustomGunTags))
			{
				return;
			}

			Transform camera = player.cameraTransform;
			if (camera == null)
			{
				return;
			}

			Transform weapon = player.inventory.GetHoldingItemTransform();
			Transform[] transforms = camera.GetComponentsInChildren<Transform>(true);
			for (int i = 0; i < transforms.Length; i++)
			{
				Transform transform = transforms[i];
				if (!IsHoldtypePart(transform.name))
				{
					continue;
				}

				if (weapon != null && (transform == weapon || transform.IsChildOf(weapon)))
				{
					continue;
				}

				if (transform.gameObject.activeSelf)
				{
					transform.gameObject.SetActive(false);
				}
			}
		}

		private static bool IsHoldtypePart(string name)
		{
			for (int i = 0; i < HideNames.Length; i++)
			{
				if (name == HideNames[i])
				{
					return true;
				}
			}

			return false;
		}
	}

	[HarmonyPatch(typeof(EntityAlive), "OnAvatarRebuilt")]
	internal static class Patch_HeldHandsOnRebuild
	{
		private static bool Prepare()
		{
			return AccessTools.Method(typeof(EntityAlive), "OnAvatarRebuilt") != null;
		}

		private static void Postfix(EntityAlive __instance)
		{
			HeldHands.Hide(__instance);
		}
	}

	[HarmonyPatch(typeof(EntityAlive), "ShowHoldingItem")]
	internal static class Patch_HeldHandsOnShow
	{
		private static void Postfix(EntityAlive __instance, bool _show)
		{
			if (_show)
			{
				HeldHands.Hide(__instance);
			}
		}
	}
}
