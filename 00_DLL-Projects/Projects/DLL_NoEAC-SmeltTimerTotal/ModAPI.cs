using System;
using HarmonyLib;

public class ModAPI : IModApi
{
	public void InitMod(Mod modInstance)
	{
		try
		{
			// Client UI only — safe if somehow present on dedicated (patches no-op without forge UI).
			GameVersion.Initialize();
			new Harmony("com.agfprojects.smelttimertotal").PatchAll();
			Console.WriteLine("SmeltTimerTotal: Harmony patches registered.");
		}
		catch (Exception ex)
		{
			Console.WriteLine("SmeltTimerTotal: Patch registration error: " + ex);
		}
	}
}
