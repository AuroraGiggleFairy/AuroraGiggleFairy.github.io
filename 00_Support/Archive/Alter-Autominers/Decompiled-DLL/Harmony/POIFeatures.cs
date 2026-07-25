using System.Reflection;
using HarmonyLib;

namespace Harmony;

public class POIFeatures : IModApi
{
	public void InitMod(Mod _modInstance)
	{
		Log.Out(" Loading Patch: " + GetType());
		HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(GetType().ToString());
		harmony.PatchAll(Assembly.GetExecutingAssembly());
	}
}
