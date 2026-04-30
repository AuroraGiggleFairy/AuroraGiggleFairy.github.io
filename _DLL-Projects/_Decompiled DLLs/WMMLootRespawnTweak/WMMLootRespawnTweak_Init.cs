using System.Reflection;
using HarmonyLib;

public class WMMLootRespawnTweak_Init : IModApi
{
	public void InitMod(Mod _modInstance)
	{
		Log.Out(" Loading Patch: " + GetType().ToString());
		Harmony harmony = new Harmony(GetType().ToString());
		harmony.PatchAll(Assembly.GetExecutingAssembly());
	}
}
