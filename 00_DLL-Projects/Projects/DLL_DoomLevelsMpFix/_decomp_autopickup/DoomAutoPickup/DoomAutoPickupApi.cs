using HarmonyLib;

namespace DoomAutoPickup;

public class DoomAutoPickupApi : IModApi
{
	public void InitMod(Mod _modInstance)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		new Harmony("com.doom.autopickup").PatchAll();
	}
}
