using System;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	[HarmonyPatch(typeof(EntityAsyncManager), nameof(EntityAsyncManager.EnsureEntity))]
	internal static class Patch_SkipForceSpawnWait
	{
		private static bool Prefix()
		{
			return GameManager.IsDedicatedServer;
		}
	}

	internal static class LootDropPreload
	{
		private static readonly string[] Assets =
		{
			"@:Entities/LootContainers/backpack03Prefab.prefab",
			"#@modfolder(DoomMod_Standalone):Resources/IZY_VanilaReplacer_Pack_MainResource.unity3d?IZY_DROP_PistolM9.Prefab",
			"#@modfolder(DoomMod_Standalone):Resources/IZY_VanilaReplacer_Pack_MainResource.unity3d?IZY_DROP_PumpSG.Prefab",
			"#@modfolder(DoomMod_Standalone):Resources/IZY_HVW_Pack_MainResource.unity3d?DROP_HVW_M134.Prefab"
		};

		internal static void Run()
		{
			if (GameManager.IsDedicatedServer)
			{
				return;
			}

			for (int i = 0; i < Assets.Length; i++)
			{
				try
				{
					LoadManager.LoadAsset<GameObject>(
						Assets[i],
						(Action<GameObject>)null,
						(LoadManager.LoadGroup)null,
						false,
						true,
						false);
				}
				catch (Exception e)
				{
					Debug.LogWarning("[DoomMultiplayer] loot preload skipped " + Assets[i] + ": " + e.Message);
				}
			}
		}
	}
}
