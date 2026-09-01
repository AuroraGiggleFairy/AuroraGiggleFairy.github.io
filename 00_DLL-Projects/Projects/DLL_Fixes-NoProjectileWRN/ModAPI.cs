using System;
using HarmonyLib;
using UnityEngine;

namespace NoProjectileWRN
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod modInstance)
		{
			try
			{
				new Harmony("com.agfprojects.noprojectilewrn").PatchAll();
				Debug.Log("[NoProjectileWRN] Harmony patches registered.");
			}
			catch (Exception ex)
			{
				Debug.LogError("[NoProjectileWRN] Patch registration failed: " + ex);
			}
		}
	}
}
