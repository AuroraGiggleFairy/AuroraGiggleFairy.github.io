using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	[HarmonyPatch(typeof(XUiC_MapArea), nameof(XUiC_MapArea.updateMapSection))]
	internal static class Patch_MapPointFilter
	{
		private static void Postfix(XUiC_MapArea __instance)
		{
			Texture2D tex = __instance.mapTexture;
			if (tex == null || tex.filterMode == FilterMode.Point)
			{
				return;
			}

			tex.filterMode = FilterMode.Point;
			tex.anisoLevel = 0;
		}
	}
}
