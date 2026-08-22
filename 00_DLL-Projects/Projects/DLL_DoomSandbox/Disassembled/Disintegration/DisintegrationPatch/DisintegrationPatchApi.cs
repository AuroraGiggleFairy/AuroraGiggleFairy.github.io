using HarmonyLib;

namespace DisintegrationPatch;

public class DisintegrationPatchApi : IModApi
{
	public void InitMod(Mod _modInstance)
	{
		new Harmony("com.doommod.disintegration").PatchAll();
		Log.Out("[DisintegrationPatch] Loaded from " + _modInstance.Path);
	}
}
