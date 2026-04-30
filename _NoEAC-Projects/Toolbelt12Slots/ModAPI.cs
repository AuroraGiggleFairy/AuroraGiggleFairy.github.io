using System;
using HarmonyLib;

namespace Toolbelt12Slots
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod modInstance)
		{
			try
			{
				new Harmony("com.agfprojects.toolbelt12slots").PatchAll();
				Console.WriteLine("Toolbelt12Slots: Harmony patches registered.");
			}
			catch (Exception ex)
			{
				Console.WriteLine("Toolbelt12Slots: Patch registration error: " + ex);
			}
		}
	}
}
