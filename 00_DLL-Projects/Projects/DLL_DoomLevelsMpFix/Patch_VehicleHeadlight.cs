using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// Vehicle spotlights keep a hard shadow edge that blacks out the beam.
	/// Drop the shadow and keep the light per-pixel. Cone, range, and brightness stay as they are.
	/// </summary>
	[HarmonyPatch(typeof(VPHeadlight), "Update")]
	internal static class Patch_VehicleHeadlight
	{
		private static readonly AccessTools.FieldRef<VPHeadlight, List<Light>> LightsRef =
			AccessTools.FieldRefAccess<VPHeadlight, List<Light>>("lights");

		private static readonly AccessTools.FieldRef<VPHeadlight, List<Light>> ModLightsRef =
			AccessTools.FieldRefAccess<VPHeadlight, List<Light>>("modLights");

		private static bool _announced;

		private static void Postfix(VPHeadlight __instance)
		{
			if (GameManager.IsDedicatedServer || __instance == null)
			{
				return;
			}

			Hold(LightsRef(__instance));
			Hold(ModLightsRef(__instance));
		}

		private static void Hold(List<Light> lights)
		{
			if (lights == null)
			{
				return;
			}

			for (int i = 0; i < lights.Count; i++)
			{
				Light light = lights[i];
				if (light == null)
				{
					continue;
				}

				if (light.type != LightType.Spot)
				{
					light.type = LightType.Spot;
				}

				if (light.cookie != null)
				{
					light.cookie = null;
				}

				if (light.shadows != LightShadows.None)
				{
					light.shadows = LightShadows.None;
				}

				if (light.renderMode != LightRenderMode.ForcePixel)
				{
					light.renderMode = LightRenderMode.ForcePixel;
				}

				if (!_announced)
				{
					_announced = true;
					Debug.Log("[DoomMultiplayer] vehicle headlights keep the beam");
				}
			}
		}
	}
}
