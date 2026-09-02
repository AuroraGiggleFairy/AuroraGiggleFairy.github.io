using System;
using HarmonyLib;

namespace GyroFlightModes
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod _modInstance)
		{
			try
			{
				new Harmony("com.agfprojects.gyroflightmodes").PatchAll();
				Console.WriteLine("[GyroFlightModes] Harmony patches registered.");
			}
			catch (Exception ex)
			{
				Console.WriteLine("[GyroFlightModes] Patch registration error: " + ex);
			}
		}
	}
}
