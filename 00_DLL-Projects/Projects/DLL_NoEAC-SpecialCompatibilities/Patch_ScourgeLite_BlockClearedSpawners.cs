using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SpecialNoEACCompatibilities
{
	/// <summary>
	/// Scourge Lite records a POI clear as scourge_{centerX}_{centerZ}.
	/// Vanilla still respawns those sleeper volumes from Loot Respawn Days.
	/// Once that clear is set, skip the touch that would spawn them again.
	/// psc_resetPOI removes the clear, and spawning works again.
	/// </summary>
	[HarmonyPatch(typeof(SleeperVolume), "UpdatePlayerTouched")]
	public static class Patch_ScourgeLite_BlockClearedSpawners
	{
		static FieldInfo _masterClear;

		static bool Prepare()
		{
			if (ModManager.GetMod("POI_Scourge_Lite") == null)
			{
				return false;
			}

			Type handler = AccessTools.TypeByName("POIScourgeLite.ScourgeLiteHandler");
			_masterClear = handler == null ? null : AccessTools.Field(handler, "MasterClearDictionary");
			bool ready = _masterClear != null;
			if (ready)
			{
				Console.WriteLine("[NoEACCompatibilities] Scourge Lite cleared POIs will not respawn sleepers.");
			}

			return ready;
		}

		static bool Prefix(SleeperVolume __instance, EntityPlayer _playerTouched)
		{
			try
			{
				return !IsScourgeCleared(__instance, _playerTouched);
			}
			catch (Exception ex)
			{
				Console.WriteLine("[NoEACCompatibilities] Scourge clear check failed: " + ex.Message);
				return true;
			}
		}

		static bool IsScourgeCleared(SleeperVolume volume, EntityPlayer player)
		{
			PrefabInstance prefab = volume?.PrefabInstance;
			if (prefab == null)
			{
				return false;
			}

			Bounds bounds = prefab.GetAABB();
			Vector3 center = bounds.center;
			string key = "scourge_" + (int)center.x + "_" + (int)center.z;
			if (ClearValue(key) > 0f)
			{
				return true;
			}

			return player != null && player.Buffs != null && player.Buffs.GetCustomVar(key) > 0f;
		}

		static float ClearValue(string key)
		{
			object dict = _masterClear.GetValue(null);
			if (dict is IDictionary table && table.Contains(key) && table[key] is float value)
			{
				return value;
			}

			return 0f;
		}
	}
}
