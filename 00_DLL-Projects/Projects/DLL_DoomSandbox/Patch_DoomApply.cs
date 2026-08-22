using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace DoomSandbox
{
	/// <summary>Applies Doom sandbox semantics that vanilla % options cannot express.</summary>
	public static class Patch_DoomApply
	{
		[HarmonyPatch(typeof(EntityPlayer), "get_gameStage")]
		public static class Patch_FlatGameStage
		{
			public static void Postfix(ref int __result)
			{
				int add = (int)DoomSandboxRuntime.FlatGameStage;
				if (add != 0)
					__result += add;
			}
		}

		[HarmonyPatch(typeof(EntityPlayer), nameof(EntityPlayer.GetLootStage))]
		public static class Patch_FlatLootStage
		{
			public static void Postfix(ref int __result)
			{
				int add = (int)DoomSandboxRuntime.FlatLootStage;
				if (add != 0)
					__result += add;
			}
		}

		/// <summary>Enemy Health = real max HP multiplier (not Entity Incoming Damage).</summary>
		[HarmonyPatch(typeof(EffectManager), nameof(EffectManager.GetValue))]
		public static class Patch_EffectManager_GetValue
		{
			public static void Postfix(
				PassiveEffects _passiveEffect,
				ItemValue _originalItemValue,
				EntityAlive _entity,
				ref float __result)
			{
				if (_passiveEffect == PassiveEffects.HealthMax)
				{
					if (_entity != null && !(_entity is EntityPlayer))
					{
						float mult = DoomSandboxRuntime.EnemyHealthMult;
						if (Math.Abs(mult - 1f) > 0.0001f)
							__result *= mult;
					}
					return;
				}

				if (_passiveEffect != PassiveEffects.SpreadDegreesVertical
					&& _passiveEffect != PassiveEffects.SpreadDegreesHorizontal)
					return;

				if (_originalItemValue?.ItemClass == null)
					return;

				string name = _originalItemValue.ItemClass.Name;
				if (!DoomSandboxItemApplier.IsHitscanHandItem(name))
					return;

				__result = DoomSandboxRuntime.HitscanSpreadDegrees;
			}
		}

		/// <summary>
		/// Player Damage Taken: flat ±1 to incoming hit strength only.
		/// DoomArmour splits that strength into health vs stamina armour; do not touch stamina here.
		/// Skip while buffInvulnerability is active (resist zeroes the hit after this Prefix).
		/// </summary>
		[HarmonyPatch(typeof(EntityPlayer), nameof(EntityPlayer.DamageEntity))]
		public static class Patch_PlayerFlatDamage
		{
			const string InvulnerabilityBuff = "buffInvulnerability";

			public static void Prefix(EntityPlayer __instance, ref int _strength)
			{
				int flat = Mathf.RoundToInt(DoomSandboxRuntime.PlayerDamageFlat);
				if (flat == 0)
					return;

				if (__instance?.Buffs != null && __instance.Buffs.HasBuff(InvulnerabilityBuff))
					return;

				_strength = Math.Max(0, _strength + flat);
			}
		}

		/// <summary>
		/// Fast Monsters: halve ranged shot cooldown after vanilla Reset assigns it (≈2x fire rate).
		/// Former Commando (and BM) excluded — already near-zero cooldown hitscan.
		/// </summary>
		[HarmonyPatch(typeof(EAIRangedAttackTarget), nameof(EAIRangedAttackTarget.Reset))]
		public static class Patch_FastMonsters_RangedCooldown
		{
			static AccessTools.FieldRef<EAIRangedAttackTarget, float> _cooldownRef;
			static bool _cooldownResolved;

			static AccessTools.FieldRef<EAIRangedAttackTarget, float> CooldownRef
			{
				get
				{
					if (!_cooldownResolved)
					{
						_cooldownResolved = true;
						try
						{
							_cooldownRef = AccessTools.FieldRefAccess<EAIRangedAttackTarget, float>("cooldown");
						}
						catch (Exception ex)
						{
							DoomLog.Error("Fast Monsters: cooldown field missing: " + ex.Message);
						}
					}
					return _cooldownRef;
				}
			}

			public static void Postfix(EAIRangedAttackTarget __instance)
			{
				if (!DoomSandboxRuntime.FastMonsters)
					return;
				try
				{
					var entity = __instance?.theEntity;
					string className = entity?.EntityClass?.entityClassName;
					if (!string.IsNullOrEmpty(className)
						&& className.IndexOf("FormerCommando", StringComparison.OrdinalIgnoreCase) >= 0)
						return;

					var field = CooldownRef;
					if (field == null)
						return;
					field(__instance) *= 0.5f;
				}
				catch (Exception ex)
				{
					DoomLog.Error("Fast Monsters cooldown apply failed: " + ex.Message);
				}
			}
		}

		/// <summary>
		/// Skill Gain Level Cap: last level that still grants skill points.
		/// Levels above the cap grant no skill points.
		/// </summary>
		[HarmonyPatch(typeof(Progression), nameof(Progression.AddLevelExp))]
		public static class Patch_SkillGainLevelCap
		{
			public static void Prefix(Progression __instance, out int __state)
			{
				__state = __instance.Level;
			}

			public static void Postfix(Progression __instance, int __state)
			{
				int cap = Mathf.RoundToInt(DoomSandboxRuntime.SkillGainLevelCap);
				if (cap <= 0)
					return;

				int levelBefore = __state;
				int levelAfter = __instance.Level;
				if (levelAfter <= levelBefore)
					return;

				int grantLevels = Math.Max(0, Math.Min(levelAfter, cap) - levelBefore);
				int levelsGained = levelAfter - levelBefore;
				if (grantLevels >= levelsGained)
					return;

				// Remove skill points granted for levels past the cap.
				// Uses current SkillPointsPerLevel (sandbox Skill Gain Amount).
				int perLevel = Math.Max(0, Progression.SkillPointsPerLevel);
				int remove = (levelsGained - grantLevels) * perLevel;
				if (remove > 0)
					__instance.SkillPoints = Math.Max(0, __instance.SkillPoints - remove);
			}
		}
	}

	public static class DoomSandboxItemApplier
	{
		static readonly string[] ProjectileItems =
		{
			"meleeHandDemonImpProjectile",
			"meleeHandDemonNightmareImpProjectile",
			"meleeHandDemonBabyCacoProjectile",
			"meleeHandDemonCacodemonProjectile",
			"meleeHandDemonHellKnightProjectile",
			"meleeHandDemonBaronHellProjectile",
			"meleeHandDemonPainElementalProjectile",
			"meleeHandDemonRevenantProjectile",
			"meleeHandDemonMancubusProjectile",
			"meleeHandDemonCyberProjectile",
		};

		static readonly HashSet<string> HitscanHandItems =
			new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"meleeHandFormerSoldier",
				"meleeHandFormerSergeant",
				"meleeHandFormerCommando",
			};

		static readonly Dictionary<string, float> BaseProjectileVelocity =
			new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

		public static bool IsHitscanHandItem(string itemName) =>
			!string.IsNullOrEmpty(itemName) && HitscanHandItems.Contains(itemName);

		public static void Apply()
		{
			try
			{
				float mult = DoomSandboxRuntime.ProjectileSpeedMult;
				foreach (var name in ProjectileItems)
				{
					// Never touch Former Human / hitscan hands — spread only, not velocity.
					if (IsHitscanHandItem(name) || name.IndexOf("Former", StringComparison.OrdinalIgnoreCase) >= 0)
						continue;

					var item = ItemClass.GetItemClass(name, true);
					if (item?.Actions == null)
						continue;

					for (int a = 0; a < item.Actions.Length; a++)
					{
						var proj = item.Actions[a] as ItemActionProjectile;
						if (proj == null)
							continue;

						if (!BaseProjectileVelocity.ContainsKey(name))
							BaseProjectileVelocity[name] = proj.Velocity;

						proj.Velocity = BaseProjectileVelocity[name] * mult;
					}
				}
			}
			catch (Exception ex)
			{
				DoomLog.Error("Item apply failed: " + ex.Message);
			}
		}
	}
}
