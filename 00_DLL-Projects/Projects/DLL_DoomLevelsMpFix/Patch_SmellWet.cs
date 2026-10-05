using HarmonyLib;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// Smell clears from the combined wetness rate, which includes standing in water and rain.
	/// Keep only the rain portion, so standing in water no longer washes smell off.
	/// </summary>
	[HarmonyPatch(typeof(PlayerStealth), "SmellTickWet")]
	internal static class Patch_SmellWet
	{
		private static bool Prefix(EntityPlayer ___player, ref float ___smellWetRate, ref float ___smellWet)
		{
			float rate = 0f;
			EntityPlayerLocal local = ___player as EntityPlayerLocal;
			if (local != null && local.shelterAbovePercent == 0f)
			{
				WeatherManager weather = WeatherManager.Instance;
				if (weather != null)
				{
					rate = weather.GetCurrentWetPercent(local) * 0.05f;
				}
			}

			___smellWetRate = rate;
			if (rate >= 0.01f)
			{
				___smellWet += rate;
			}

			return false;
		}
	}
}
