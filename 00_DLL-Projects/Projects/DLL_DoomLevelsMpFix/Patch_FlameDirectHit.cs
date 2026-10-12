using HarmonyLib;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// A flamethrower shot that touches a body deals no hit damage. The burn
	/// from the cylinder is the weapon's damage. This runs after sandbox
	/// ranged-damage percent, and it blocks the game's minimum of 1.
	/// </summary>
	[HarmonyPatch(typeof(ItemActionAttack), nameof(ItemActionAttack.Hit))]
	internal static class Patch_FlameDirectHit
	{
		private static readonly FastTags<TagGroup.Global> FlameTag =
			FastTags<TagGroup.Global>.Parse("flamethrower");

		private static void Prefix(ItemValue damagingItemValue, ref float _entityDamage, ref bool _forceNoDamage)
		{
			if (damagingItemValue?.ItemClass == null || !damagingItemValue.ItemClass.ItemTags.Test_AnySet(FlameTag))
			{
				return;
			}

			_entityDamage = 0f;
			_forceNoDamage = true;
		}
	}
}
