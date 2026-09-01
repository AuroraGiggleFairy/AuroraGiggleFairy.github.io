using HarmonyLib;

namespace AGFProjects.Fix4PerkPageReset
{
	/// <summary>
	/// Vanilla XUiC_SkillPerkInfoWindow.SkillChanged always pager.Reset()s to page 1.
	/// Buying a perk dirties the skills window group, which calls SkillChanged for the
	/// same perk — so a 10+ level perk jumps back to page 1. Keep the page when the
	/// selected perk did not change; still reset when the player picks a different perk.
	/// </summary>
	[HarmonyPatch(typeof(XUiC_SkillPerkInfoWindow), nameof(XUiC_SkillPerkInfoWindow.SkillChanged))]
	public static class Patch_SkillPerkInfoWindow_SkillChanged
	{
		static string lastPerkName;

		public static void Prefix(XUiC_Paging ___pager, out int __state)
		{
			__state = ___pager != null ? ___pager.GetPage() : 0;
		}

		public static void Postfix(XUiC_SkillPerkInfoWindow __instance, XUiC_Paging ___pager, int __state)
		{
			string perkName = GetSelectedPerkName(__instance);
			if (___pager != null && perkName != null && perkName == lastPerkName)
			{
				___pager.SetPage(__state);
			}

			lastPerkName = perkName;
		}

		static string GetSelectedPerkName(XUiC_SkillPerkInfoWindow window)
		{
			ProgressionValue skill = window?.xui?.SelectedSkill;
			if (skill?.ProgressionClass == null || !skill.ProgressionClass.IsPerk)
			{
				return null;
			}

			return skill.Name;
		}
	}
}
