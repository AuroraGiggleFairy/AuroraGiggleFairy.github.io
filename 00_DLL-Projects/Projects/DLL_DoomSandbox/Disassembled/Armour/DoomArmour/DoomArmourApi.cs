using HarmonyLib;

namespace DoomArmour;

public class DoomArmourApi : IModApi
{
	public void InitMod(Mod _modInstance)
	{
		new Harmony("com.doommod.armour").PatchAll();
		Log.Out("[DoomArmour] Loaded from " + _modInstance.Path);
	}
}
