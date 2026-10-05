using HarmonyLib;

namespace SpecialNoEACCompatibilities
{
	[HarmonyPatch(typeof(Localization), nameof(Localization.LoadPatchDictionaries))]
	public static class Patch_Localization_LoadPatchDictionaries
	{
		static void Postfix(string _modName, bool _loadingInGame)
		{
			if (_modName != ModAPI.ModName)
			{
				return;
			}

			CompatLocLoader.ApplyAfterOurModLoc(_loadingInGame);
		}
	}
}
