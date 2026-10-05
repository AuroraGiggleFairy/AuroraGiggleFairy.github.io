using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace DamageTypeFix
{
	internal static class Bfg
	{
		static readonly FastTags<TagGroup.Global> ItemTag = FastTags<TagGroup.Global>.Parse("perkBFGExpert");
		static readonly FastTags<TagGroup.Global> ExplosionTag = FastTags<TagGroup.Global>.Parse("explosion");

		public static bool IsBfg(ItemValue item)
		{
			return IsBfg(item?.ItemClass);
		}

		public static bool IsBfg(ItemClass itemClass)
		{
			return itemClass != null && !itemClass.ItemTags.IsEmpty && itemClass.ItemTags.Test_AnySet(ItemTag);
		}

		public static FastTags<TagGroup.Global> ExplosionTags(ItemValue item)
		{
			return ExplosionTag | item.ItemClass.ItemTags;
		}
	}

	internal static class ExplosionDamageTypeRestore
	{
		public static void Apply(ref ExplosionData explosionData)
		{
			if (!CustomExplosionManager.GetCustomParticleComponents(explosionData.ParticleIndex, out ExplosionComponent component)
				|| component == null)
			{
				return;
			}

			explosionData.DamageType = component.BoundExplosionData.DamageType;
			if (Bfg.IsBfg(component.BoundItemClass))
			{
				explosionData.DamageType = EnumDamageTypes.Electrical;
			}
		}
	}

	[HarmonyPatch(typeof(NetPackageExplosionInitiate), "read")]
	internal static class Patch_NetPackageExplosionInitiate_read
	{
		static void Postfix(NetPackageExplosionInitiate __instance)
		{
			ExplosionData data = __instance.explosionData;
			ExplosionDamageTypeRestore.Apply(ref data);
			if (Bfg.IsBfg(__instance.itemValueExplosive))
			{
				data.DamageType = EnumDamageTypes.Electrical;
			}

			__instance.explosionData = data;
		}
	}

	[HarmonyPatch(typeof(Explosion), nameof(Explosion.AttackEntites))]
	internal static class Patch_Explosion_AttackEntites
	{
		static readonly FieldInfo ExplosionDataField = AccessTools.Field(typeof(Explosion), "explosionData");

		static void Prefix(Explosion __instance, ItemValue _itemValueExplosionSource, ref EnumDamageTypes damageType)
		{
			if (Bfg.IsBfg(_itemValueExplosionSource))
			{
				damageType = EnumDamageTypes.Electrical;
				return;
			}

			if (ExplosionDataField == null)
			{
				return;
			}

			ExplosionData data = (ExplosionData)ExplosionDataField.GetValue(__instance);
			if (CustomExplosionManager.GetCustomParticleComponents(data.ParticleIndex, out ExplosionComponent component)
				&& Bfg.IsBfg(component?.BoundItemClass))
			{
				damageType = EnumDamageTypes.Electrical;
			}
		}
	}

	[HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.DamageEntity))]
	internal static class Patch_EntityAlive_DamageEntity_Bfg
	{
		static void Prefix(EntityAlive __instance, DamageSource _damageSource, ref int _strength)
		{
			if (_strength <= 0 || _damageSource == null || !Bfg.IsBfg(_damageSource.AttackingItem))
			{
				return;
			}

			if (__instance is EntityPlayer)
			{
				_strength = 0;
				return;
			}

			if (_damageSource.GetDamageType() == EnumDamageTypes.Piercing)
			{
				return;
			}

			_damageSource.CreatorEntityId = -2;

			UnityEngine.Debug.Log("[DamageTypeFix] DamageEntity target=" + (__instance.EntityName ?? __instance.GetType().Name)
				+ " str=" + _strength
				+ " type=" + _damageSource.GetDamageType()
				+ " creator=" + _damageSource.CreatorEntityId);

			ItemValue item = _damageSource.AttackingItem;
			EntityAlive attacker = GameManager.Instance?.World?.GetEntity(_damageSource.getEntityId()) as EntityAlive;
			MinEventParams context = attacker?.MinEventContext;
			EntityAlive oldOther = context?.Other;
			FastTags<TagGroup.Global> tags = Bfg.ExplosionTags(item);

			if (context != null)
			{
				context.Other = null;
			}

			float withoutOther = EffectManager.GetValue(PassiveEffects.ExplosionEntityDamage, item, 0f, attacker, null, tags);

			if (context != null)
			{
				context.Other = __instance;
			}

			float withOther = EffectManager.GetValue(PassiveEffects.ExplosionEntityDamage, item, 0f, attacker, null, tags);

			if (context != null)
			{
				context.Other = oldOther;
			}

			int bonus = (int)(withOther - withoutOther);
			if (bonus > 0)
			{
				_strength += bonus;
			}
		}
	}
}
