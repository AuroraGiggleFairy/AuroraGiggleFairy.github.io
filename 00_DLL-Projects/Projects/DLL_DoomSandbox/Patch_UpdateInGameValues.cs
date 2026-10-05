using System;
using HarmonyLib;
using SandboxOptManager = global::SandboxOptions.SandboxOptionManager;

namespace DoomSandbox
{
	[HarmonyPatch(typeof(SandboxOptManager), nameof(SandboxOptManager.UpdateInGameValuesWithSandboxOptions))]
	public static class Patch_UpdateInGameValuesWithSandboxOptions
	{
		public static void Prefix()
		{
			var mgr = SandboxOptManager.Current;
			DoomSandboxRebuildGate.EnsureReady(mgr, "UpdateInGameValues.Prefix");
			DoomSandboxRebuildGate.ApplySandboxCode(mgr, "UpdateInGameValues.Prefix");
		}

		public static void Postfix()
		{
			DoomSandboxRuntime.RefreshFromManager();

			// Neutralize remapped vanilla % systems so Doom semantics can own them.
			ItemActionAttack.IncomingDamageModifier = 1f;
			ItemActionAttack.EntityIncomingDamageModifier = 1f;
			EntityPlayer.GlobalGameStageModifier = 1f;
			EntityPlayer.GlobalLootStageModifier = 1f;

			// Gravity must never follow a borrowed Doom slot (player movement feel).
			try
			{
				var mgr = SandboxOptManager.Current;
				if (mgr != null && mgr.SandboxOptionsDict.TryGetValue(
					global::SandboxOptions.SandboxOptions.SillyLowGravity, out var gravOpt))
				{
					var gf = gravOpt as global::SandboxOptions.SandboxOptionFloat;
					if (gf != null)
					{
						gf.DefaultValue = 1f;
						gf.CurrentValue = 1f;
					}
				}
				UnityEngine.Physics.gravity = SandboxOptManager.originalGravity;
			}
			catch { /* ignore */ }
			try
			{
				GameEventManager.Current?.SetGameEventFlag(
					GameEventManager.GameEventFlagTypes.BigHeadSandbox, false, -1f, isPermanent: true);
				GameEventManager.Current?.SetGameEventFlag(
					GameEventManager.GameEventFlagTypes.TinyZombiesSandbox, false, -1f, isPermanent: true);
			}
			catch { /* ignore */ }

			// SillyBlackandWhite is Fast Monsters — strip vanilla B&W visual if UpdateInGameValues added it.
			try
			{
				var locals = GameManager.Instance?.World?.GetLocalPlayers();
				if (locals != null)
				{
					for (int i = 0; i < locals.Count; i++)
					{
						var local = locals[i];
						if (local?.Buffs != null && local.Buffs.GetBuff("sandbox_blackandwhite") != null)
							local.Buffs.RemoveBuff("sandbox_blackandwhite");
					}
				}
			}
			catch { /* ignore */ }

			// Doom armour pool uses stamina — keep regen multiplier at none (0).
			try
			{
				var mgr = SandboxOptManager.Current;
				mgr?.SetOption(global::SandboxOptions.SandboxOptions.StaminaRegen, 0f);
			}
			catch { /* ignore */ }

			DoomSandboxItemApplier.Apply();

			// Sync sandbox → GamePrefs/GameStats. Vanilla UpdateInGameValues often skips these,
			// and ggs / GameModeAbstract.Init read prefs (JarRefund default 60, DayLightLength 18).
			try
			{
				SyncSandboxToGamePrefs();
			}
			catch (Exception ex)
			{
				DoomLog.Error("SyncSandboxToGamePrefs failed: " + ex.Message);
			}

			// Guard against DayNightLength <= 0 freezing time (vanilla divides by minutes*60).
			try
			{
				int dayNight = GameStats.GetInt(EnumGameStats.DayNightLength);
				if (dayNight <= 0)
				{
					dayNight = 60;
					GameStats.Set(EnumGameStats.DayNightLength, dayNight);
					var mgr = SandboxOptManager.Current;
					mgr?.SetOption(global::SandboxOptions.SandboxOptions.DayNightLength, dayNight);
					DoomLog.Info("DayNightLength was <= 0; restored to 60 real-time minutes.");
				}
				int inc = 24000 / (dayNight * 60);
				if (inc <= 0)
					inc = 1;
				GameStats.Set(EnumGameStats.TimeOfDayIncPerSec, inc);
			}
			catch { /* ignore */ }

			try
			{
				int dayLight = SandboxOptManager.GetInt(global::SandboxOptions.SandboxOptions.DayLightLength);
				int airDrop = SandboxOptManager.GetInt(global::SandboxOptions.SandboxOptions.AirDropFrequency);
				float jar = SandboxOptManager.GetFloat(global::SandboxOptions.SandboxOptions.JarRefund);
				int vReset = SandboxOptManager.GetInt(global::SandboxOptions.SandboxOptions.VendingResetInterval);
				float vAbund = SandboxOptManager.GetFloat(global::SandboxOptions.SandboxOptions.VendingItemAbundance);
				int maxTech = SandboxOptManager.GetInt(global::SandboxOptions.SandboxOptions.MaxTechType);
				DoomLog.Info(
					$"Sandbox sync: DayLight={dayLight} (dusk~{(dayLight < 18 ? 12 + dayLight / 2 : 22)}), " +
					$"AirDropOpt={airDrop}, JarRefund={jar:0.##} (ggs%={(int)Math.Round(jar * 100f)}), " +
					$"VendingReset={vReset}, VendingAbundance={vAbund:0.##}, MaxTechType={maxTech}");
			}
			catch { /* ignore */ }

			DoomLog.Info(
				$"Applied: EnemyHP={DoomSandboxRuntime.EnemyHealthMult:0.##}, " +
				$"PlayerDmgFlat={DoomSandboxRuntime.PlayerDamageFlat:0.##}, " +
				$"ProjSpeed={DoomSandboxRuntime.ProjectileSpeedMult:0.##}, " +
				$"Hitscan={DoomSandboxRuntime.HitscanSpreadDegrees:0.##}, " +
				$"FlatGS={DoomSandboxRuntime.FlatGameStage:0.##}, " +
				$"FlatLS={DoomSandboxRuntime.FlatLootStage:0.##}, " +
				$"SkillCap={DoomSandboxRuntime.SkillGainLevelCap:0}, " +
				$"FastMonsters={DoomSandboxRuntime.FastMonsters}");
		}

		static void SyncSandboxToGamePrefs()
		{
			if (SandboxOptManager.Current == null || !SandboxOptManager.Current.IsInit)
				return;

			float jar = SandboxOptManager.GetFloat(global::SandboxOptions.SandboxOptions.JarRefund);
			int jarPct = (int)Math.Round(jar * 100f);
			if (jarPct < 0) jarPct = 0;
			if (jarPct > 100) jarPct = 100;
			GameStats.Set(EnumGameStats.JarRefund, jarPct);
			GamePrefs.Set(EnumGamePrefs.JarRefund, jarPct);

			int dayLight = SandboxOptManager.GetInt(global::SandboxOptions.SandboxOptions.DayLightLength);
			GameStats.Set(EnumGameStats.DayLightLength, dayLight);
			GamePrefs.Set(EnumGamePrefs.DayLightLength, dayLight);

			int dayNight = SandboxOptManager.GetInt(global::SandboxOptions.SandboxOptions.DayNightLength);
			if (dayNight > 0)
			{
				GameStats.Set(EnumGameStats.DayNightLength, dayNight);
				GamePrefs.Set(EnumGamePrefs.DayNightLength, dayNight);
			}

			// AirDropFrequency GameStat is a day count hint; ranges intentionally store 3 in vanilla.
			// Still sync Prefs from the component min when fixed interval, else leave range marker.
			int airOpt = SandboxOptManager.GetInt(global::SandboxOptions.SandboxOptions.AirDropFrequency);
			int airPref;
			switch (airOpt)
			{
				case 0: airPref = 0; break;
				case 1: airPref = 1; break;
				case 3: airPref = 3; break;
				case 5: airPref = 7; break;
				default: airPref = 3; break; // 2/4/6 are ranges — vanilla stores 3
			}
			GamePrefs.Set(EnumGamePrefs.AirDropFrequency, airPref);
		}
	}

	/// <summary>Stamina is Doom armour — never apply vanilla stamina regen.</summary>
	[HarmonyPatch(typeof(EntityStats), nameof(EntityStats.UpdateSandboxOptions))]
	public static class Patch_EntityStats_UpdateSandboxOptions
	{
		public static void Postfix(EntityStats __instance)
		{
			if (__instance?.Stamina != null)
				__instance.Stamina.GainSandboxModifier = 0f;
		}
	}
}
