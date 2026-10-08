using HarmonyLib;

namespace AGFProjects.DestroyBiomeBadgeFix
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod modInstance)
		{
			GameVersion.Initialize();
			var harmony = new Harmony("com.agfprojects.destroybiomebadgefix");
			harmony.PatchAll();
			UnityEngine.Debug.Log("[AGF] DestroyBiomeBadgeFix loaded (clothing slots protected).");
		}
	}
}
