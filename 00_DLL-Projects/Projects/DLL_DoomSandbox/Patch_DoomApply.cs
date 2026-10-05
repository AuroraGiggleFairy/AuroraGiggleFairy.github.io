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
					// Vehicles store durability on the item (MaxUseTimes). Stat.Tick
					// reapplies HealthMax every tick, so this multiplier lifts the bar
					// (4000 -> 4400 at 110%) and the next vehicle sync writes the item
					// value back, which drops the repair.
					if (_entity != null && !(_entity is EntityPlayer) && !(_entity is EntityVehicle))
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
		/// Player Damage Taken: ±1 to remaining health AND ±1 to the doomArmour CVar,
		/// after DoomArmour's green 40% / blue 60% split. Do not add to the pre-split total.
		/// Health and armour are floored at 0 so a −1 never heals. Incoming 0 (BFG, etc.)
		/// is left alone so we do not invent damage. −1 refunds 1 of this hit's armour spend
		/// except the last HUD point: that hit breaks the pool (0) even if DoomArmour
		/// adrenaline/grace/reinforced netted a 0 spend. A fully emptied pool stays 0.
		/// Skip while buffInvulnerability is active.
		/// </summary>
		internal static class PlayerFlatDamageState
		{
			public const string InvulnerabilityBuff = "buffInvulnerability";
			public const string PoolVar = "doomArmour";
			public const string PctVar = ".doomArmourPct";
			public const string ArmourHarmonyId = "com.doommod.armour";

			[ThreadStatic] public static bool Apply;
			[ThreadStatic] public static float ArmourBefore;
		}

		[HarmonyPatch(typeof(EntityAlive), "damageEntityLocal")]
		[HarmonyPriority(Priority.First)]
		[HarmonyBefore(PlayerFlatDamageState.ArmourHarmonyId)]
		public static class Patch_PlayerFlatDamage_Capture
		{
			public static void Prefix(EntityAlive __instance, int _strength)
			{
				PlayerFlatDamageState.Apply = false;
				PlayerFlatDamageState.ArmourBefore = 0f;

				if (Mathf.RoundToInt(DoomSandboxRuntime.PlayerDamageFlat) == 0)
					return;
				if (_strength <= 0)
					return;
				if (!(__instance is EntityPlayer player))
					return;
				if (player.Buffs != null && player.Buffs.HasBuff(PlayerFlatDamageState.InvulnerabilityBuff))
					return;

				if (player.Buffs != null)
					PlayerFlatDamageState.ArmourBefore = Mathf.Max(0f, player.Buffs.GetCustomVar(PlayerFlatDamageState.PoolVar));
				PlayerFlatDamageState.Apply = true;
			}
		}

		[HarmonyPatch(typeof(EntityAlive), "damageEntityLocal")]
		[HarmonyAfter(PlayerFlatDamageState.ArmourHarmonyId)]
		public static class Patch_PlayerFlatDamage
		{
			public static void Prefix(EntityAlive __instance, ref int _strength)
			{
				if (!PlayerFlatDamageState.Apply)
					return;
				PlayerFlatDamageState.Apply = false;

				int flat = Mathf.RoundToInt(DoomSandboxRuntime.PlayerDamageFlat);
				if (flat == 0)
					return;

				_strength = Math.Max(0, _strength + flat);

				var player = __instance as EntityPlayer;
				if (player == null)
					return;

				ApplyArmourFlat(player, flat, PlayerFlatDamageState.ArmourBefore);
			}

			static void ApplyArmourFlat(EntityPlayer player, int flat, float armourBefore)
			{
				var buffs = player.Buffs;
				if (buffs == null)
					return;

				float held = Mathf.Max(0f, buffs.GetCustomVar(PlayerFlatDamageState.PoolVar));
				float next;
				if (flat > 0)
				{
					next = Mathf.Max(0f, held - flat);
				}
				else
				{
					int shownBefore = Mathf.RoundToInt(armourBefore);
					// Last visible point: break. Do not wait for DoomArmour to leave 0 —
					// adrenaline / grace / reinforced can net a 0 spend and keep HUD at 1.
					if (shownBefore <= 1 && armourBefore > 0.0001f)
						next = 0f;
					else if (Mathf.RoundToInt(held) <= 0)
						next = 0f;
					else
					{
						float spent = armourBefore - held;
						if (spent <= 0f)
							return;
						next = Mathf.Min(armourBefore, held + (-flat));
					}
				}

				float max = 0f;
				if (player.Stats?.Stamina != null)
					max = player.Stats.Stamina.ModifiedMax;
				next = Mathf.Clamp(next, 0f, max);
				if (Mathf.Abs(next - held) < 0.0001f)
					return;

				buffs.SetCustomVar(PlayerFlatDamageState.PoolVar, next, true, CVarOperation.set, true);
				float pct = max <= 0f ? 0f : next / max;
				buffs.SetCustomVar(PlayerFlatDamageState.PctVar, pct, false, CVarOperation.set, false);
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
