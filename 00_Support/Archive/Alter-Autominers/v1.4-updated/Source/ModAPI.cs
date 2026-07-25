using System;
using System.Reflection;
using HarmonyLib;

namespace Autominers
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod _modInstance)
		{
			try
			{
				Log.Out("[Autominers] Loading Harmony patches");
				new Harmony("com.alter.autominers").PatchAll(Assembly.GetExecutingAssembly());
			}
			catch (Exception ex)
			{
				Log.Error("[Autominers] Patch registration error: " + ex);
			}
		}
	}
}
