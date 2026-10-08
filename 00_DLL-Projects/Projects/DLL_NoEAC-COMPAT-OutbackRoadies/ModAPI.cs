using System;
using HarmonyLib;

namespace AGF.Compat.OutbackRoadies
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
				new Harmony("com.agfprojects.compat.outbackroadies").PatchAll();
				Console.WriteLine("[AGF-OutbackRoadies] Harmony patches registered.");
			}
			catch (Exception ex)
			{
				Console.WriteLine("[AGF-OutbackRoadies] Patch registration error: " + ex);
			}
		}
	}
}
