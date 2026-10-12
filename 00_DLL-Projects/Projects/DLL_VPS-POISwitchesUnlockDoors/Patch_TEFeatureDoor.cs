using System;
using HarmonyLib;
using UnityEngine;

namespace POISwitchesUnlockDoors
{
	[HarmonyPatch(typeof(TEFeatureDoor), nameof(TEFeatureDoor.OnBlockTriggered))]
	public static class Patch_TEFeatureDoor_OnBlockTriggered
	{
		public static void Prefix(TEFeatureDoor __instance, TEFeatureLockable ___lockFeature)
		{
			try
			{
				TEFeatureLockable lockable = ___lockFeature ?? __instance?.Parent?.GetFeature<TEFeatureLockable>();
				if (lockable != null && lockable.IsLocked() && !lockable.HasPassword())
				{
					lockable.SetLocked(false);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("[POISwitchesUnlockDoors] Error in OnBlockTriggered prefix: " + ex);
			}
		}
	}
}
