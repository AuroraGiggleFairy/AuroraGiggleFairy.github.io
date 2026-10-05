using System;
using HarmonyLib;

namespace SpecialNoEACCompatibilities
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
				new Harmony("com.agfprojects.specialnoeaccompatibilities").PatchAll();
				Console.WriteLine("[NoEACCompatibilities] Harmony patches registered.");
			}
			catch (Exception ex)
			{
				Console.WriteLine("[NoEACCompatibilities] Patch registration error: " + ex);
			}
		}
	}
}
