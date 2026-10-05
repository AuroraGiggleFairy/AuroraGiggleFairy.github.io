using System;
using HarmonyLib;

namespace MultiLookStorage
{
	public class ModApi : IModApi
	{
		public void InitMod(Mod modInstance)
		{
			try
			{
				GameVersion.Initialize();
				PackageEmit.Prepare(typeof(NetPackageMultiLookRequest), typeof(NetPackageMultiLookResult));
				new Harmony("com.agfprojects.multilookstorage").PatchAll();
				Console.WriteLine("MultiLookStorage: player storage can be opened by more than one player.");
			}
			catch (Exception ex)
			{
				Console.WriteLine("MultiLookStorage: patch registration failed: " + ex);
			}
		}
	}
}
