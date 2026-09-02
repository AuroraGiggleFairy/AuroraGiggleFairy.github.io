using System;
using HarmonyLib;
using UnityEngine;

namespace HonkOpensYourDoors
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod _modInstance)
		{
			try
			{
				new Harmony("com.agfprojects.honkopensyourdoors").PatchAll();
				Debug.Log("[HonkOpensYourDoors] Harmony patches registered.");
			}
			catch (Exception ex)
			{
				Debug.LogError("[HonkOpensYourDoors] Patch registration failed: " + ex);
			}
		}
	}
}
