using System;
using HarmonyLib;

namespace FuelAutoShutOff
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod modInstance)
		{
			try
			{
				GameVersion.Initialize();
				new Harmony("com.agfprojects.fuelautoshutoff").PatchAll();
				Console.WriteLine("FuelAutoShutOff: Harmony patches registered.");
			}
			catch (Exception ex)
			{
				Console.WriteLine("FuelAutoShutOff: Patch registration error: " + ex);
			}
		}
	}
}
