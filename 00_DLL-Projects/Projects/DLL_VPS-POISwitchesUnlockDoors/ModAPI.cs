using System;
using HarmonyLib;
using UnityEngine;

namespace POISwitchesUnlockDoors
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod _modInstance)
		{
			try
			{
				new Harmony("com.agfprojects.poiswitchesunlockdoors").PatchAll();
				Debug.Log("[POISwitchesUnlockDoors] Harmony patches registered.");
			}
			catch (Exception ex)
			{
				Debug.LogError("[POISwitchesUnlockDoors] Patch registration failed: " + ex);
			}
		}
	}
}
