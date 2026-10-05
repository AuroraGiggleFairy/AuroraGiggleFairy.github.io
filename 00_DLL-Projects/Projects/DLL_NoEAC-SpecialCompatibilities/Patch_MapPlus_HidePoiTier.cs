using System;
using System.Reflection;
using HarmonyLib;

namespace SpecialNoEACCompatibilities
{
	/// <summary>
	/// MapPlus appends the POI skull tier, such as (T5), after the map name.
	/// Doom does not use that tier. Skip it when a Doom mod is loaded.
	/// MapPlus without Doom still shows the tier.
	/// </summary>
	[HarmonyPatch]
	public static class Patch_MapPlus_HidePoiTier
	{
		static bool Prepare()
		{
			if (ModManager.GetMod("DoomMod_Standalone") == null && ModManager.GetMod("DoomClassicMaps") == null)
			{
				return false;
			}

			Type hover = AccessTools.TypeByName("MapPlus.MapPlusHover");
			return hover != null && AccessTools.Method(hover, "FormatDisplay") != null;
		}

		static MethodBase TargetMethod()
		{
			return AccessTools.Method(AccessTools.TypeByName("MapPlus.MapPlusHover"), "FormatDisplay");
		}

		static bool Prefix(string name, ref string __result)
		{
			__result = name ?? string.Empty;
			return false;
		}
	}
}
