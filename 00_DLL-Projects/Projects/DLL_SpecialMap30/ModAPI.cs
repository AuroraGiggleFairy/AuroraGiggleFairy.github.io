using System;
using HarmonyLib;
using UnityEngine;

namespace SpecialMap30
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod modInstance)
		{
			try
			{
				PitchLock.ModPath = modInstance.Path;
				PitchLock.LoadZone();
				new Harmony("com.doom.specialmap30").PatchAll();
				Debug.Log("[SpecialMap30] InitMod OK. Zone=" + PitchLock.DescribeZone());
			}
			catch (Exception ex)
			{
				Debug.LogError("[SpecialMap30] InitMod FAILED: " + ex);
			}
		}
	}
}
