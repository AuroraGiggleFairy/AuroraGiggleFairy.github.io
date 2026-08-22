using HarmonyLib;

namespace DoomInfighting;

public class DoomInfightingApi : IModApi
{
	public void InitMod(Mod _modInstance)
	{
		new Harmony("com.doommod.infighting").PatchAll();
		ModEvents.WorldShuttingDown.RegisterHandler(OnWorldShuttingDown);
		Log.Out("[DoomInfighting] Loaded from " + _modInstance.Path);
	}

	private void OnWorldShuttingDown(ref ModEvents.SWorldShuttingDownData _data)
	{
		Threshold.Clear();
	}
}
