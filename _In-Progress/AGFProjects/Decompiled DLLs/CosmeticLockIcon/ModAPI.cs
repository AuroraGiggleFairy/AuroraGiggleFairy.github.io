using System;
using HarmonyLib;

public class ModAPI : IModApi
{
	public void InitMod(Mod modInstance)
	{
		try
		{
			new Harmony("com.agfprojects.cosmeticunlockicon").PatchAll();
			Console.WriteLine("CosmeticUnlockIcon: Harmony patches registered.");
		}
		catch (Exception ex)
		{
			Console.WriteLine("CosmeticUnlockIcon: Patch registration error: " + ex);
		}
	}
}
