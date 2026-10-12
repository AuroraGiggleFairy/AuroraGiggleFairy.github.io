using System;
using HarmonyLib;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// Plasma rifle hits skip the organic material tap (flesh, stone, wood, dirt, and the rest).
	/// The plasma impact sound stays.
	/// </summary>
	internal static class Patch_PlasmaSlap
	{
		private const string OrganicHit = "organichit";

		private static readonly FastTags<TagGroup.Global> PlasmaRifleTag =
			FastTags<TagGroup.Global>.Parse("PlasmaRifle");

		[ThreadStatic]
		private static int depth;

		private static bool IsPlasmaRifle(ItemValue item)
		{
			return item?.ItemClass != null && item.ItemClass.ItemTags.Test_AnySet(PlasmaRifleTag);
		}

		[HarmonyPatch(typeof(ItemActionAttack), nameof(ItemActionAttack.Hit))]
		private static class HitScope
		{
			private static void Prefix(ItemValue damagingItemValue)
			{
				if (IsPlasmaRifle(damagingItemValue))
				{
					depth++;
				}
			}

			private static void Postfix(ItemValue damagingItemValue)
			{
				if (IsPlasmaRifle(damagingItemValue) && depth > 0)
				{
					depth--;
				}
			}
		}

		[HarmonyPatch(typeof(ParticleEffect), nameof(ParticleEffect.SpawnParticleEffect))]
		private static class MuteOrganicHit
		{
			private static void Prefix(ParticleEffect _pe)
			{
				if (depth > 0 && _pe != null && _pe.soundName != null &&
					_pe.soundName.StartsWith(OrganicHit, StringComparison.Ordinal))
				{
					_pe.soundName = null;
				}
			}
		}
	}
}
