using HarmonyLib;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// Chainsaw, chaingun, and plasma rifle hits cross the pain threshold and play a flinch.
	/// Clear that flag for those weapons. The projectile and the damage are unchanged.
	/// </summary>
	internal static class Patch_PainAnim
	{
		private static readonly FastTags<TagGroup.Global> NoPainTags = FastTags<TagGroup.Global>.Parse("chainsaw,chaingun,chainGun,PlasmaRifle");

		private static bool IsNoPainWeapon(DamageSource source)
		{
			if (source == null || source.AttackingItem == null)
			{
				return false;
			}

			ItemClass itemClass = source.AttackingItem.ItemClass;
			return itemClass != null && itemClass.HasAnyTags(NoPainTags);
		}

		[HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.ProcessDamageResponseLocal))]
		private static class Local
		{
			private static void Prefix(ref DamageResponse _dmResponse)
			{
				if (_dmResponse.PainHit && IsNoPainWeapon(_dmResponse.Source))
				{
					_dmResponse.PainHit = false;
				}
			}
		}

		[HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.damageEntityLocal))]
		private static class Returned
		{
			private static void Postfix(ref DamageResponse __result)
			{
				if (__result.PainHit && IsNoPainWeapon(__result.Source))
				{
					__result.PainHit = false;
				}
			}
		}
	}
}
