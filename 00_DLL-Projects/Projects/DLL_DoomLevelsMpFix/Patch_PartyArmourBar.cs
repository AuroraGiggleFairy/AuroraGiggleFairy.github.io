using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	[HarmonyPatch]
	internal static class Patch_PartyArmourBar
	{
		private static readonly FieldInfo FillField;
		private static readonly FieldInfo HeldField;
		private static readonly FieldInfo MaxField;

		static Patch_PartyArmourBar()
		{
			System.Type type = AccessTools.TypeByName("DoomArmour.XUiC_DoomArmourBar");
			if (type == null)
			{
				return;
			}

			FillField = AccessTools.Field(type, "_fill");
			HeldField = AccessTools.Field(type, "_held");
			MaxField = AccessTools.Field(type, "_max");
		}

		private static bool Prepare()
		{
			return TargetMethod() != null;
		}

		private static MethodBase TargetMethod()
		{
			System.Type type = AccessTools.TypeByName("DoomArmour.XUiC_DoomArmourBar");
			return type != null ? AccessTools.Method(type, "Update") : null;
		}

		private static void Postfix(XUiController __instance)
		{
			XUiC_PartyEntry entry = FindPartyEntry(__instance);
			if (entry == null || entry.Player != null)
			{
				return;
			}

			int id = PartyHud.FallbackId(entry);
			if (id <= 0 || !InstanceSync.TryVitals(id, out _, out _, out float pct))
			{
				return;
			}

			float held = pct;
			float max = 1f;

			if (FillField != null)
			{
				FillField.SetValue(__instance, pct);
			}

			if (HeldField != null)
			{
				HeldField.SetValue(__instance, held);
			}

			if (MaxField != null)
			{
				MaxField.SetValue(__instance, max);
			}

			XUiV_Sprite bar = __instance.ViewComponent as XUiV_Sprite;
			if (bar == null)
			{
				XUiController child = __instance.GetChildById("BarArmour");
				bar = child?.ViewComponent as XUiV_Sprite;
			}

			if (bar != null)
			{
				bar.Fill = pct;
			}
		}

		private static XUiC_PartyEntry FindPartyEntry(XUiController node)
		{
			while (node != null)
			{
				if (node is XUiC_PartyEntry entry)
				{
					return entry;
				}

				node = node.Parent;
			}

			return null;
		}
	}
}
