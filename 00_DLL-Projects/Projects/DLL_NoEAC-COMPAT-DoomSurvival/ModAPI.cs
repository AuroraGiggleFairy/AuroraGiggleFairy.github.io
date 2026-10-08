using System;
using HarmonyLib;

namespace AGF.Compat.DoomSurvival
{
	public class ModAPI : IModApi
	{
		internal static string ModName = "";

		internal static string ModPath;

		public void InitMod(Mod modInstance)
		{
			try
			{
				ModName = modInstance != null ? modInstance.Name : "";
				ModPath = modInstance != null ? modInstance.Path : null;
				new Harmony("com.agfprojects.compat.doomsurvival").PatchAll();
				Console.WriteLine("[AGF-DoomSurvival] Harmony patches registered.");
			}
			catch (Exception ex)
			{
				Console.WriteLine("[AGF-DoomSurvival] Patch registration error: " + ex);
			}
		}
	}
}
