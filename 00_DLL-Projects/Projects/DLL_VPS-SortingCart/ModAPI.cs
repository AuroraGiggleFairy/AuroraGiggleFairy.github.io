using System;
using HarmonyLib;

namespace SortingCart
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod _modInstance)
		{
			try
			{
				new Harmony("com.agfprojects.sortingcart").PatchAll();
				Log.Info("Loaded. Based on Kanaverum / Asylum Robotic Inbox. Maintained by AGF.");
			}
			catch (Exception ex)
			{
				Log.Error("Init failed", ex);
			}
		}
	}
}
