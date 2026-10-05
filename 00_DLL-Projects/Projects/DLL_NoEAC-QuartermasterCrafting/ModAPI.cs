using System;
using HarmonyLib;

namespace QuartermasterCrafting
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod _modInstance)
		{
			try
			{
				GameVersion.Initialize();
				PackageEmit.Prepare(typeof(NetPackageQuartermasterRequest), typeof(NetPackageQuartermasterReply));
				new Harmony("com.agfprojects.quartermastercrafting").PatchAll();
				Log.Info("Loaded.");
			}
			catch (Exception ex)
			{
				Log.Error("Init failed", ex);
			}
		}
	}
}
