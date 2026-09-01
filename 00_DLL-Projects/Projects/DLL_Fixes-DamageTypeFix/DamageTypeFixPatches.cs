using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace DamageTypeFix
{
	internal static class BfgPlayerIgnore
	{
		static readonly FastTags<TagGroup.Global> BfgTag = FastTags<TagGroup.Global>.Parse("perkBFGExpert");

		public static bool IsBfg(DamageSource source)
		{
			ItemClass itemClass = source?.AttackingItem?.ItemClass;
			if (itemClass == null)
			{
				return false;
			}

			if (!itemClass.ItemTags.IsEmpty && itemClass.ItemTags.Test_AnySet(BfgTag))
			{
				return true;
			}

			string name = itemClass.Name;
			return !string.IsNullOrEmpty(name)
				&& (name.IndexOf("BFG", StringComparison.OrdinalIgnoreCase) >= 0
					|| name.StartsWith("DummyBFG", StringComparison.OrdinalIgnoreCase));
		}
	}

	/// <summary>
	/// BFG must deal 0 to players: health and DoomArmour pool.
	/// DoomArmour.DamageSplit spends armour in damageEntityLocal before Electrical resist
	/// zeroes remaining health, so strength must be 0 before that Prefix.
	/// </summary>
	[HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.DamageEntity))]
	internal static class Patch_EntityAlive_DamageEntity_BfgPlayers
	{
		private static void Prefix(EntityAlive __instance, DamageSource _damageSource, ref int _strength)
		{
			if (_strength <= 0 || !(__instance is EntityPlayer))
			{
				return;
			}

			if (BfgPlayerIgnore.IsBfg(_damageSource))
			{
				_strength = 0;
			}
		}
	}

	/// <summary>
	/// Restores item XML Explosion.DamageType when ExplosionData arrives as default Heat.
	/// Heat explosions are treated specially by entity damage rules; BFG ammo uses Electrical
	/// so party/ally friendly-fire rules apply correctly in multiplayer.
	/// </summary>
	[HarmonyPatch(typeof(GameManager), nameof(GameManager.ExplosionServer))]
	internal static class Patch_GameManager_ExplosionServer
	{
		private static bool Prefix(ref ExplosionData _explosionData, ItemValue _itemValueExplosionSource)
		{
			try
			{
				if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					return true;
				}

				if (_explosionData.DamageType != EnumDamageTypes.Heat)
				{
					return true;
				}

				if (TryResolveExplosionDamageType(_itemValueExplosionSource, out EnumDamageTypes resolvedType)
					&& resolvedType != EnumDamageTypes.Heat)
				{
					_explosionData.DamageType = resolvedType;
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[DamageTypeFix] Failed to resolve explosion damage type: " + ex.Message);
			}

			return true;
		}

		private static bool TryResolveExplosionDamageType(ItemValue itemValue, out EnumDamageTypes resolvedType)
		{
			resolvedType = EnumDamageTypes.Heat;
			if (itemValue == null)
			{
				return false;
			}

			ItemClass itemClass = itemValue.ItemClass;
			if (itemClass == null)
			{
				return false;
			}

			// Fast path: projectile action already parsed Explosion (BFG cell / rockets).
			if (TryResolveFromActions(itemClass, out resolvedType))
			{
				return true;
			}

			// Vanilla ProjectileMoveScript passes the launcher, not the ammo cell.
			if (TryResolveFromSelectedAmmo(itemValue, itemClass, out resolvedType))
			{
				return true;
			}

			// XML / DynamicProperties fallbacks (flat keys, nested Explosion class, Action*.Explosion).
			if (TryResolveFromProperties(itemClass.Properties, out resolvedType))
			{
				return true;
			}

			return false;
		}

		private static bool TryResolveFromSelectedAmmo(ItemValue launcherValue, ItemClass launcherClass, out EnumDamageTypes resolvedType)
		{
			resolvedType = EnumDamageTypes.Heat;
			ItemAction[] actions = launcherClass.Actions;
			if (actions == null)
			{
				return false;
			}

			for (int i = 0; i < actions.Length; i++)
			{
				if (!(actions[i] is ItemActionRanged ranged) || ranged.MagazineItemNames == null || ranged.MagazineItemNames.Length == 0)
				{
					continue;
				}

				int ammoIndex = launcherValue.SelectedAmmoTypeIndex;
				if (ammoIndex < 0 || ammoIndex >= ranged.MagazineItemNames.Length)
				{
					continue;
				}

				ItemClass ammoClass = ItemClass.GetItemClass(ranged.MagazineItemNames[ammoIndex]);
				if (ammoClass == null)
				{
					continue;
				}

				if (TryResolveFromActions(ammoClass, out resolvedType))
				{
					return true;
				}

				if (TryResolveFromProperties(ammoClass.Properties, out resolvedType))
				{
					return true;
				}
			}

			return false;
		}

		private static bool TryResolveFromActions(ItemClass itemClass, out EnumDamageTypes resolvedType)
		{
			resolvedType = EnumDamageTypes.Heat;
			ItemAction[] actions = itemClass.Actions;
			if (actions == null)
			{
				return false;
			}

			for (int i = 0; i < actions.Length; i++)
			{
				if (actions[i] is ItemActionProjectile projectile
					&& projectile.Explosion.DamageType != EnumDamageTypes.Heat)
				{
					resolvedType = projectile.Explosion.DamageType;
					return true;
				}
			}

			return false;
		}

		private static bool TryResolveFromProperties(DynamicProperties properties, out EnumDamageTypes resolvedType)
		{
			resolvedType = EnumDamageTypes.Heat;
			if (properties == null)
			{
				return false;
			}

			if (TryParseKey(properties, "Explosion.DamageType", out resolvedType))
			{
				return true;
			}

			if (TryParseNestedExplosion(properties, out resolvedType))
			{
				return true;
			}

			if (properties.Classes != null)
			{
				foreach (KeyValuePair<string, DynamicProperties> actionClass in properties.Classes)
				{
					if (actionClass.Value == null)
					{
						continue;
					}

					if (TryParseKey(actionClass.Value, "Explosion.DamageType", out resolvedType))
					{
						return true;
					}

					if (TryParseNestedExplosion(actionClass.Value, out resolvedType))
					{
						return true;
					}
				}
			}

			// V3: Values is Dictionary<string,string> (no DictionarySave.Dict wrapper).
			if (properties.Values != null)
			{
				foreach (KeyValuePair<string, string> kvp in properties.Values)
				{
					if (kvp.Key == null || kvp.Value == null)
					{
						continue;
					}

					if (!kvp.Key.EndsWith("Explosion.DamageType", StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}

					if (EnumUtils.TryParse(kvp.Value, out resolvedType, true))
					{
						return true;
					}
				}
			}

			return false;
		}

		private static bool TryParseNestedExplosion(DynamicProperties properties, out EnumDamageTypes damageType)
		{
			damageType = EnumDamageTypes.Heat;
			if (properties?.Classes == null)
			{
				return false;
			}

			if (!properties.Classes.TryGetValue("Explosion", out DynamicProperties explosion) || explosion == null)
			{
				return false;
			}

			return TryParseKey(explosion, "DamageType", out damageType);
		}

		private static bool TryParseKey(DynamicProperties properties, string key, out EnumDamageTypes damageType)
		{
			damageType = EnumDamageTypes.Heat;
			if (properties?.Values == null)
			{
				return false;
			}

			if (!properties.Values.TryGetValue(key, out string value) || string.IsNullOrEmpty(value))
			{
				return false;
			}

			return EnumUtils.TryParse(value, out damageType, true);
		}
	}
}
