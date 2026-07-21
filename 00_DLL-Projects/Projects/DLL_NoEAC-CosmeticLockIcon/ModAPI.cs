using System;
using HarmonyLib;

public class ModAPI : IModApi
{
	public void InitMod(Mod modInstance)
	{
		try
		{
			new Harmony("com.agfprojects.cosmeticlockicon").PatchAll();
			Console.WriteLine("CosmeticLockIcon: Harmony patches registered.");
		}
		catch (Exception ex)
		{
			Console.WriteLine("CosmeticLockIcon: Patch registration error: " + ex);
		}
	}
}
