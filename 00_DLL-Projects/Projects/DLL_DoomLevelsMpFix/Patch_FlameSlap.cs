using System;
using HarmonyLib;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// Flamethrower hits skip the bullet tap on stone, wood, dirt, and the other surfaces.
	/// The flame loop stays.
	/// </summary>
	internal static class Patch_FlameSlap
	{
		private const string BulletHit = "bullethit";

		private static readonly FastTags<TagGroup.Global> FlameTag =
			FastTags<TagGroup.Global>.Parse("flamethrower");

		[ThreadStatic]
		private static int depth;

		private static bool IsFlamethrower(ItemValue item)
		{
			return item?.ItemClass != null && item.ItemClass.ItemTags.Test_AnySet(FlameTag);
		}

		[HarmonyPatch(typeof(ItemActionAttack), nameof(ItemActionAttack.Hit))]
		private static class HitScope
		{
			private static void Prefix(ItemValue damagingItemValue)
			{
				if (IsFlamethrower(damagingItemValue))
				{
					depth++;
				}
			}

			private static void Postfix(ItemValue damagingItemValue)
			{
				if (IsFlamethrower(damagingItemValue) && depth > 0)
				{
					depth--;
				}
			}
		}

		[HarmonyPatch(typeof(ParticleEffect), nameof(ParticleEffect.SpawnParticleEffect))]
		private static class MuteBulletHit
		{
			private static void Prefix(ParticleEffect _pe)
			{
				if (depth > 0 && _pe != null && _pe.soundName != null &&
					_pe.soundName.StartsWith(BulletHit, StringComparison.Ordinal))
				{
					_pe.soundName = null;
				}
			}
		}
	}
}
