using HarmonyLib;

namespace AGFProjects.Fix4PerkPageReset
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod modInstance)
		{
			if (GameManager.IsDedicatedServer)
			{
				return;
			}

			new Harmony("com.agfprojects.fix4perkpagereset").PatchAll();
		}
	}
}
