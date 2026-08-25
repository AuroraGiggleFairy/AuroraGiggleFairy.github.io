using System;
using HarmonyLib;

public class ModAPI : IModApi
{
	public void InitMod(Mod _modInstance)
	{
		try
		{
			if (GameManager.IsDedicatedServer)
			{
				Console.WriteLine("[HideDLCCosmetics] Dedicated server — client-only, not loading.");
				return;
			}

			new Harmony("com.agfprojects.hidedlccosmetics").PatchAll();
			Console.WriteLine("[HideDLCCosmetics] Harmony patches registered.");
		}
		catch (Exception ex)
		{
			Console.WriteLine("[HideDLCCosmetics] Patch registration error: " + ex);
		}
	}
}
