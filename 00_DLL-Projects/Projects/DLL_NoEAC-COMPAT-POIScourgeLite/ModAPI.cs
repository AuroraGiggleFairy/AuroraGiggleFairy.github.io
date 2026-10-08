using System;
using HarmonyLib;

namespace AGF.Compat.POIScourgeLite
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
				new Harmony("com.agfprojects.compat.poiscourgelite").PatchAll();
				Console.WriteLine("[AGF-POIScourgeLite] Harmony patches registered.");
			}
			catch (Exception ex)
			{
				Console.WriteLine("[AGF-POIScourgeLite] Patch registration error: " + ex);
			}
		}
	}
}
