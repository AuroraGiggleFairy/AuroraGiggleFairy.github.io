using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// DummyRocket is the loaded round, and its mesh is the flying rocket.
	/// The launcher magazine is 1000 so the gun stays "full", which used to
	/// spawn that many copies on the gun. Keep one hidden copy for the next shot.
	/// Fire turns that copy back on.
	/// </summary>
	internal static class RocketInMag
	{
		internal static bool HidesLoadedRound(ItemActionLauncher launcher)
		{
			return launcher != null
				&& launcher.Properties.Contains("VisibleInMag")
				&& !launcher.Properties.GetBool("VisibleInMag");
		}
	}

	[HarmonyPatch(typeof(ItemActionLauncher), nameof(ItemActionLauncher.StartHolding))]
	internal static class Patch_RocketInMagCount
	{
		[HarmonyPriority(Priority.Last)]
		private static void Prefix(ItemActionLauncher __instance, ItemActionData _actionData, ref int __state)
		{
			__state = -1;
			ItemValue item = _actionData?.invData?.itemValue;
			if (item == null || !RocketInMag.HidesLoadedRound(__instance))
			{
				return;
			}

			__state = ItemInventoryAccess.MetaOf(item);
			if (__state > 1)
			{
				ItemInventoryAccess.SetMeta(item, 1);
			}
		}

		[HarmonyPriority(Priority.First)]
		private static void Postfix(ItemActionData _actionData, int __state)
		{
			if (__state > 1 && _actionData?.invData?.itemValue != null)
			{
				ItemInventoryAccess.SetMeta(_actionData.invData.itemValue, __state);
			}
		}
	}

	[HarmonyPatch(typeof(ItemActionLauncher), nameof(ItemActionLauncher.ItemActionEffects))]
	internal static class Patch_RocketInMagRefill
	{
		private static void Postfix(ItemActionLauncher __instance, ItemActionData _actionData, int _firingState)
		{
			if (_firingState == 0 || !RocketInMag.HidesLoadedRound(__instance))
			{
				return;
			}

			ItemActionLauncher.ItemActionDataLauncher data = _actionData as ItemActionLauncher.ItemActionDataLauncher;
			ItemValue item = _actionData?.invData?.itemValue;
			if (data == null || item == null || ItemInventoryAccess.MetaOf(item) <= 0 || data.projectileTs.Count > 0)
			{
				return;
			}

			Transform shot = __instance.instantiateProjectile(_actionData);
			if (shot == null)
			{
				return;
			}

			data.projectileTs.Add(shot);
			shot.gameObject.SetActive(false);
		}
	}
}
