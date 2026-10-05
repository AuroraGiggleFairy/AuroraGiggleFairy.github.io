using System.Reflection;
using DoomLevels;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// Some switches use one texture for both states, such as the E1M2 light LITE4.
	/// The lookup still asks for a second material and warns when that name is not
	/// in the bundle. Keep the texture already on the switch and do not warn.
	/// </summary>
	[HarmonyPatch(typeof(Switch), "ResolveFace")]
	internal static class Patch_SwitchMaterial
	{
		private static readonly FieldInfo Off = AccessTools.Field(typeof(Switch), "_off");
		private static readonly FieldInfo On = AccessTools.Field(typeof(Switch), "_on");
		private static readonly FieldInfo Face = AccessTools.Field(typeof(Switch), "_face");
		private static readonly FieldInfo OffMaterial = AccessTools.Field(typeof(Switch), "_offMaterial");
		private static readonly FieldInfo OnMaterial = AccessTools.Field(typeof(Switch), "_onMaterial");

		private static bool Prefix(Switch __instance)
		{
			if (__instance == null || Off == null || On == null)
			{
				return true;
			}

			string off = Off.GetValue(__instance) as string;
			string on = On.GetValue(__instance) as string;
			if (string.IsNullOrEmpty(off) || off != on)
			{
				return true;
			}

			if (Face != null)
			{
				Face.SetValue(__instance, null);
			}

			if (OffMaterial != null)
			{
				OffMaterial.SetValue(__instance, null);
			}

			if (OnMaterial != null)
			{
				OnMaterial.SetValue(__instance, null);
			}

			Renderer[] renderers = __instance.GetComponentsInChildren<Renderer>(true);
			for (int i = 0; i < renderers.Length; i++)
			{
				Material current = renderers[i].sharedMaterial;
				if (current == null || !current.name.EndsWith("/" + off))
				{
					continue;
				}

				if (Face != null)
				{
					Face.SetValue(__instance, renderers[i]);
				}

				if (OffMaterial != null)
				{
					OffMaterial.SetValue(__instance, current);
				}

				if (OnMaterial != null)
				{
					OnMaterial.SetValue(__instance, current);
				}

				break;
			}

			return false;
		}
	}
}
