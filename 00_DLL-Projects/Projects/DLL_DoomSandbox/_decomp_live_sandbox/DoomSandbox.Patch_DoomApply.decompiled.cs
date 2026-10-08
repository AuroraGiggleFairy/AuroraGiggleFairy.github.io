using System;
using HarmonyLib;
using UnityEngine;
using XMLData.Item;

namespace DoomSandbox;

public static class Patch_DoomApply
{
	[HarmonyPatch(typeof(EntityPlayer), "get_gameStage")]
	public static class Patch_FlatGameStage
	{
		public static void Postfix(ref int __result)
		{
			int num = (int)DoomSandboxRuntime.FlatGameStage;
			if (num != 0)
			{
				__result += num;
			}
		}
	}

	[HarmonyPatch(typeof(EntityPlayer), "GetLootStage")]
	public static class Patch_FlatLootStage
	{
		public static void Postfix(ref int __result)
		{
			int num = (int)DoomSandboxRuntime.FlatLootStage;
			if (num != 0)
			{
				__result += num;
			}
		}
	}

	[HarmonyPatch(typeof(EffectManager), "GetValue")]
	public static class Patch_EffectManager_GetValue
	{
		public static void Postfix(PassiveEffects _passiveEffect, ItemValue _originalItemValue, EntityAlive _entity, ref float __result)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Invalid comparison between Unknown and I4
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Invalid comparison between Unknown and I4
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Invalid comparison between Unknown and I4
			if ((int)_passiveEffect == 104)
			{
				if ((Object)(object)_entity != (Object)null && !(_entity is EntityPlayer))
				{
					float enemyHealthMult = DoomSandboxRuntime.EnemyHealthMult;
					if (Math.Abs(enemyHealthMult - 1f) > 0.0001f)
					{
						__result *= enemyHealthMult;
					}
				}
			}
			else if (((int)_passiveEffect == 31 || (int)_passiveEffect == 32) && ((_originalItemValue != null) ? _originalItemValue.ItemClass : null) != null && DoomSandboxItemApplier.IsHitscanHandItem(((ItemData)_originalItemValue.ItemClass).Name))
			{
				__result = DoomSandboxRuntime.HitscanSpreadDegrees;
			}
		}
	}

	internal static class PlayerFlatDamageState
	{
		public const string InvulnerabilityBuff = "buffInvulnerability";

		public const string PoolVar = "doomArmour";

		public const string PctVar = ".doomArmourPct";

		public const string ArmourHarmonyId = "com.doommod.armour";

		[ThreadStatic]
		public static bool Apply;

		[ThreadStatic]
		public static float ArmourBefore;
	}

	[HarmonyPatch(typeof(EntityAlive), "damageEntityLocal")]
	[HarmonyPriority(800)]
	[HarmonyBefore(new string[] { "com.doommod.armour" })]
	public static class Patch_PlayerFlatDamage_Capture
	{
		public static void Prefix(EntityAlive __instance, int _strength)
		{
			PlayerFlatDamageState.Apply = false;
			PlayerFlatDamageState.ArmourBefore = 0f;
			if (Mathf.RoundToInt(DoomSandboxRuntime.PlayerDamageFlat) == 0 || _strength <= 0)
			{
				return;
			}
			EntityPlayer val = (EntityPlayer)(object)((__instance is EntityPlayer) ? __instance : null);
			if (val != null && (((EntityAlive)val).Buffs == null || !((EntityAlive)val).Buffs.HasBuff("buffInvulnerability")))
			{
				if (((EntityAlive)val).Buffs != null)
				{
					PlayerFlatDamageState.ArmourBefore = Mathf.Max(0f, ((EntityAlive)val).Buffs.GetCustomVar("doomArmour"));
				}
				PlayerFlatDamageState.Apply = true;
			}
		}
	}

	[HarmonyPatch(typeof(EntityAlive), "damageEntityLocal")]
	[HarmonyAfter(new string[] { "com.doommod.armour" })]
	public static class Patch_PlayerFlatDamage
	{
		public static void Prefix(EntityAlive __instance, ref int _strength)
		{
			if (!PlayerFlatDamageState.Apply)
			{
				return;
			}
			PlayerFlatDamageState.Apply = false;
			int num = Mathf.RoundToInt(DoomSandboxRuntime.PlayerDamageFlat);
			if (num != 0)
			{
				_strength = Math.Max(0, _strength + num);
				EntityPlayer val = (EntityPlayer)(object)((__instance is EntityPlayer) ? __instance : null);
				if (!((Object)(object)val == (Object)null))
				{
					ApplyArmourFlat(val, num, PlayerFlatDamageState.ArmourBefore);
				}
			}
		}

		private static void ApplyArmourFlat(EntityPlayer player, int flat, float armourBefore)
		{
			EntityBuffs buffs = ((EntityAlive)player).Buffs;
			if (buffs == null)
			{
				return;
			}
			float num = Mathf.Max(0f, buffs.GetCustomVar("doomArmour"));
			float num2;
			if (flat > 0)
			{
				num2 = Mathf.Max(0f, num - (float)flat);
			}
			else
			{
				if (armourBefore - num <= 0f)
				{
					return;
				}
				num2 = ((!(num <= 0f)) ? Mathf.Min(armourBefore, num + (float)(-flat)) : 0f);
			}
			float num3 = 0f;
			if (((EntityAlive)player).Stats?.Stamina != null)
			{
				num3 = ((EntityAlive)player).Stats.Stamina.ModifiedMax;
			}
			num2 = Mathf.Clamp(num2, 0f, num3);
			if (!(Mathf.Abs(num2 - num) < 0.0001f))
			{
				buffs.SetCustomVar("doomArmour", num2, true, (CVarOperation)0, true);
				float num4 = ((num3 <= 0f) ? 0f : (num2 / num3));
				buffs.SetCustomVar(".doomArmourPct", num4, false, (CVarOperation)0, false);
			}
		}
	}

	[HarmonyPatch(typeof(EAIRangedAttackTarget), "Reset")]
	public static class Patch_FastMonsters_RangedCooldown
	{
		private static FieldRef<EAIRangedAttackTarget, float> _cooldownRef;

		private static bool _cooldownResolved;

		private static FieldRef<EAIRangedAttackTarget, float> CooldownRef
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
			{
				return;
			}
			try
			{
				EntityAlive obj = ((EAIBase)(__instance?)).theEntity;
				string text = ((obj == null) ? null : ((Entity)obj).EntityClass?.entityClassName);
				if (string.IsNullOrEmpty(text) || text.IndexOf("FormerCommando", StringComparison.OrdinalIgnoreCase) < 0)
				{
					FieldRef<EAIRangedAttackTarget, float> cooldownRef = CooldownRef;
					if (cooldownRef != null)
					{
						cooldownRef.Invoke(__instance) *= 0.5f;
					}
				}
			}
			catch (Exception ex)
			{
				DoomLog.Error("Fast Monsters cooldown apply failed: " + ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(Progression), "AddLevelExp")]
	public static class Patch_SkillGainLevelCap
	{
		public static void Prefix(Progression __instance, out int __state)
		{
			__state = __instance.Level;
		}

		public static void Postfix(Progression __instance, int __state)
		{
			int num = Mathf.RoundToInt(DoomSandboxRuntime.SkillGainLevelCap);
			if (num <= 0)
			{
				return;
			}
			int level = __instance.Level;
			if (level <= __state)
			{
				return;
			}
			int num2 = Math.Max(0, Math.Min(level, num) - __state);
			int num3 = level - __state;
			if (num2 < num3)
			{
				int num4 = Math.Max(0, Progression.SkillPointsPerLevel);
				int num5 = (num3 - num2) * num4;
				if (num5 > 0)
				{
					__instance.SkillPoints = Math.Max(0, __instance.SkillPoints - num5);
				}
			}
		}
	}
}
