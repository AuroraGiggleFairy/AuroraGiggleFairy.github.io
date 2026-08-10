using System;
using System.Text.RegularExpressions;
using SandboxOpt = global::SandboxOptions.SandboxOptions;

namespace DoomSandbox
{
	/// <summary>
	/// Maps worksheet choice labels onto vanilla GamePrefs ints/floats/bools.
	/// Index-based ladders are wrong whenever Doom's reduced choice list
	/// doesn't match vanilla IntValues order 1:1 (Tech3, density, BM days, etc.).
	/// </summary>
	public static class DoomSandboxValueMaps
	{
		public static bool MustRemainInt(SandboxOpt enumId)
		{
			switch (enumId)
			{
				case SandboxOpt.AirDropFrequency:
				case SandboxOpt.AirDropRandomTime:
				case SandboxOpt.DayLightLength:
				case SandboxOpt.DayNightLength:
				case SandboxOpt.ZombieFeralSense:
				case SandboxOpt.AISmellMode:
				case SandboxOpt.FullChickenStressEvent:
				case SandboxOpt.TraderResetInterval:
				case SandboxOpt.VendingResetInterval:
				case SandboxOpt.BloodMoonFrequency:
				case SandboxOpt.BloodMoonRange:
				case SandboxOpt.BloodMoonEnemyCount:
				case SandboxOpt.BloodMoonWarning:
				case SandboxOpt.MaxTechType:
				case SandboxOpt.MaxEnemyTier:
				case SandboxOpt.DeathPenalty:
				case SandboxOpt.DropOnDeath:
				case SandboxOpt.DropOnQuit:
				case SandboxOpt.LoseItemsOnDeathType:
				case SandboxOpt.DegradeItemsOnDeath:
				case SandboxOpt.ShowXP:
				case SandboxOpt.ShowLocationInfo:
				case SandboxOpt.BackpackCrafting:
				case SandboxOpt.SillyCelebrate:
				case SandboxOpt.HeadshotMode:
				case SandboxOpt.RepairTypes:
				case SandboxOpt.TraderHours:
				case SandboxOpt.TraderProtection:
				case SandboxOpt.LootRespawnDays:
				case SandboxOpt.ZombieMove:
				case SandboxOpt.ZombieMoveNight:
				case SandboxOpt.ZombieFeralMove:
				case SandboxOpt.ZombieBMMove:
				case SandboxOpt.BiomeDayEnemyDensity:
				case SandboxOpt.BiomeNightEnemyDensity:
				case SandboxOpt.BiomeDayAnimalDensity:
				case SandboxOpt.BiomeNightAnimalDensity:
				case SandboxOpt.BiomeDayZombieRespawn:
				case SandboxOpt.BiomeNightZombieRespawn:
				case SandboxOpt.BiomeDayAnimalRespawn:
				case SandboxOpt.BiomeNightAnimalRespawn:
				case SandboxOpt.SkillGainRate:
				case SandboxOpt.SkillPointsPerLevel:
				case SandboxOpt.LootMaxTier:
				case SandboxOpt.CraftingMaxTier:
				case SandboxOpt.TraderMaxTier:
				case SandboxOpt.QuestsPerTier:
				case SandboxOpt.QuestProgressionDailyLimit:
				case SandboxOpt.LoseItemsOnDeathCount:
				case SandboxOpt.PointsPerMagazine:
				case SandboxOpt.StarterSkillPoints:
					return true;
				default:
					return false;
			}
		}

		public static bool NeedsSemanticInt(SandboxOpt enumId, string optionName)
		{
			if (MustRemainInt(enumId) && enumId != SandboxOpt.DayNightLength
				&& enumId != SandboxOpt.SkillGainRate
				&& enumId != SandboxOpt.SkillPointsPerLevel
				&& enumId != SandboxOpt.LootMaxTier
				&& enumId != SandboxOpt.CraftingMaxTier
				&& enumId != SandboxOpt.TraderMaxTier
				&& enumId != SandboxOpt.BloodMoonEnemyCount
				&& enumId != SandboxOpt.PointsPerMagazine
				&& enumId != SandboxOpt.StarterSkillPoints)
				return true;

			if (string.IsNullOrEmpty(optionName))
				return false;

			return optionName.IndexOf("Density", StringComparison.OrdinalIgnoreCase) >= 0
				|| optionName.IndexOf("Respawn", StringComparison.OrdinalIgnoreCase) >= 0
				|| optionName.IndexOf("Blood Moon", StringComparison.OrdinalIgnoreCase) >= 0
				|| optionName.IndexOf("Drop On", StringComparison.OrdinalIgnoreCase) >= 0
				|| optionName.IndexOf("Show XP", StringComparison.OrdinalIgnoreCase) >= 0
				|| optionName.IndexOf("Show Location", StringComparison.OrdinalIgnoreCase) >= 0
				|| optionName.IndexOf("Backpack Crafting", StringComparison.OrdinalIgnoreCase) >= 0
				|| optionName.IndexOf("Celebrate", StringComparison.OrdinalIgnoreCase) >= 0
				|| optionName.IndexOf("Repair", StringComparison.OrdinalIgnoreCase) >= 0
				|| optionName.IndexOf("Trader Hours", StringComparison.OrdinalIgnoreCase) >= 0
				|| optionName.IndexOf("Trader Protection", StringComparison.OrdinalIgnoreCase) >= 0
				|| optionName.IndexOf("Loot Respawn", StringComparison.OrdinalIgnoreCase) >= 0
				|| optionName.IndexOf("Speed", StringComparison.OrdinalIgnoreCase) >= 0
				|| optionName.IndexOf("Headshot Mode", StringComparison.OrdinalIgnoreCase) >= 0
				|| optionName.IndexOf("Lose Items", StringComparison.OrdinalIgnoreCase) >= 0
				|| optionName.IndexOf("Degrade Items", StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public static bool TryMapInt(SandboxOpt enumId, string optionName, string choice, out int value)
		{
			value = 0;
			if (string.IsNullOrEmpty(choice))
				return false;
			var t = choice.Trim();

			if (Is(enumId, optionName, SandboxOpt.AirDropFrequency, "Air Drops")
				&& !NameHas(optionName, "Random") && !NameHas(optionName, "Mark"))
			{
				if (IsDisabled(t) || IsNone(t)) { value = 0; return true; }
				if (Eq(t, "3-7") || Eq(t, "3–7")) { value = 4; return true; }
				if (Eq(t, "1-3")) { value = 2; return true; }
				if (Eq(t, "1-7")) { value = 6; return true; }
				if (Eq(t, "7") || Eq(t, "7 days")) { value = 5; return true; }
				if (Eq(t, "3") || Eq(t, "3 days")) { value = 3; return true; }
				if (Eq(t, "1") || Eq(t, "1 day")) { value = 1; return true; }
				return false;
			}

			if (Is(enumId, optionName, SandboxOpt.ZombieFeralSense, "Feral Sense"))
			{
				if (IsDisabled(t) || IsNone(t)) { value = 0; return true; }
				if (Eq(t, "day")) { value = 1; return true; }
				if (Eq(t, "night")) { value = 2; return true; }
				if (Eq(t, "all")) { value = 3; return true; }
				return false;
			}

			if (Is(enumId, optionName, SandboxOpt.FullChickenStressEvent, "Chicken Stress"))
			{
				if (IsNone(t) || IsDisabled(t)) { value = 0; return true; }
				if (Eq(t, "random")) { value = 1; return true; }
				if (Has(t, "drop")) { value = 2; return true; }
				if (Has(t, "angry")) { value = 3; return true; }
				if (Has(t, "horde")) { value = 4; return true; }
				if (Has(t, "murder")) { value = 5; return true; }
				return false;
			}

			if (Is(enumId, optionName, SandboxOpt.AISmellMode, "Smell"))
			{
				return TryMapZombieSpeedClass(t, smellMode: true, out value);
			}

			if (enumId == SandboxOpt.ZombieMove || enumId == SandboxOpt.ZombieMoveNight
				|| enumId == SandboxOpt.ZombieFeralMove || enumId == SandboxOpt.ZombieBMMove
				|| NameHas(optionName, "Zombie") && NameHas(optionName, "Speed"))
			{
				return TryMapZombieSpeedClass(t, smellMode: false, out value);
			}

			if (Is(enumId, optionName, SandboxOpt.DayLightLength, "Day Light"))
			{
				if (Eq(t, "always night") || Eq(t, "all night")) { value = 0; return true; }
				if (Eq(t, "always day") || Eq(t, "all day")) { value = 24; return true; }
				return int.TryParse(t, System.Globalization.NumberStyles.Integer,
					System.Globalization.CultureInfo.InvariantCulture, out value);
			}

			if (enumId == SandboxOpt.TraderResetInterval || enumId == SandboxOpt.VendingResetInterval
				|| NameHas(optionName, "Reset Interval"))
			{
				if (Eq(t, "default") || Eq(t, "never") || Starts(t, "never/") || Starts(t, "never /"))
				{ value = -1; return true; }
				var dayMatch = Regex.Match(t, @"^(\d+)\s*days?$", RegexOptions.IgnoreCase);
				if (dayMatch.Success && int.TryParse(dayMatch.Groups[1].Value, out value))
					return true;
				return int.TryParse(t, System.Globalization.NumberStyles.Integer,
					System.Globalization.CultureInfo.InvariantCulture, out value);
			}

			if (Is(enumId, optionName, SandboxOpt.AirDropRandomTime, "Air Drop Random"))
			{
				if (IsNone(t) || IsDisabled(t)) { value = 0; return true; }
				if (Eq(t, "morning")) { value = 1; return true; }
				if (Has(t, "mid")) { value = 2; return true; }
				if (Eq(t, "evening")) { value = 3; return true; }
				if (Eq(t, "night")) { value = 4; return true; }
				if (Eq(t, "all day") || Eq(t, "allday")) { value = 5; return true; }
				if (Eq(t, "any") || Starts(t, "any")) { value = 6; return true; }
				return false;
			}

			if (Is(enumId, optionName, SandboxOpt.MaxTechType, "Max Tech"))
			{
				if (IsNone(t) || IsDisabled(t)) { value = 0; return true; }
				var tech = Regex.Match(t, @"tech\s*([0-3])", RegexOptions.IgnoreCase);
				if (tech.Success && int.TryParse(tech.Groups[1].Value, out var techN))
				{ value = techN + 1; return true; }
				if (Eq(t, "t0")) { value = 1; return true; }
				if (Eq(t, "t1")) { value = 2; return true; }
				if (Eq(t, "t2")) { value = 3; return true; }
				if (Eq(t, "t3")) { value = 4; return true; }
				return false;
			}

			if (Is(enumId, optionName, SandboxOpt.MaxEnemyTier, "Max Enemy"))
			{
				if (Has(t, "normal")) { value = 0; return true; }
				if (Has(t, "strong")) { value = 1; return true; }
				if (Has(t, "special")) { value = 2; return true; }
				if (Has(t, "feral")) { value = 3; return true; }
				if (Has(t, "radiat")) { value = 4; return true; }
				if (Has(t, "elite")) { value = 5; return true; }
				return false;
			}

			if (Is(enumId, optionName, SandboxOpt.DeathPenalty, "Death Penalty"))
			{
				if (IsNone(t) || IsDisabled(t)) { value = 0; return true; }
				if (Has(t, "xp")) { value = 1; return true; }
				if (Has(t, "injur")) { value = 2; return true; }
				if (Has(t, "perma") || Has(t, "hardcore")) { value = 3; return true; }
				return false;
			}

			// BiomeEnemyDensity: none=-1, Default=0, VeryLow=1, Low=2, Medium=3, High=4, VeryHigh=5
			if (IsDensity(enumId, optionName))
			{
				if (IsNone(t) || IsDisabled(t)) { value = -1; return true; }
				if (Eq(t, "default") || Starts(t, "default (")) { value = 0; return true; }
				if (Has(t, "very low")) { value = 1; return true; }
				if (Eq(t, "low") || Starts(t, "low (")) { value = 2; return true; }
				if (Eq(t, "medium") || Eq(t, "med")) { value = 3; return true; }
				if (Has(t, "very high")) { value = 5; return true; }
				if (Eq(t, "high") || Starts(t, "high (")) { value = 4; return true; }
				return false;
			}

			// SlowToFast respawn: Default=0, VerySlow=1, Slow=2, Normal=3, Fast=4, VeryFast=5
			if (IsRespawn(enumId, optionName))
			{
				if (Eq(t, "default")) { value = 0; return true; }
				if (Has(t, "very slow")) { value = 1; return true; }
				if (Eq(t, "slow")) { value = 2; return true; }
				if (Eq(t, "normal")) { value = 3; return true; }
				if (Has(t, "very fast")) { value = 5; return true; }
				if (Eq(t, "fast")) { value = 4; return true; }
				return false;
			}

			if (Is(enumId, optionName, SandboxOpt.BloodMoonFrequency, "Blood Moon Frequency"))
			{
				if (IsDisabled(t) || IsNone(t)) { value = 0; return true; }
				var m = Regex.Match(t, @"^(\d+)\s*days?$", RegexOptions.IgnoreCase);
				if (m.Success && int.TryParse(m.Groups[1].Value, out value))
					return true;
				return int.TryParse(t, out value);
			}

			if (Is(enumId, optionName, SandboxOpt.BloodMoonRange, "Blood Moon Range"))
			{
				var m = Regex.Match(t, @"^(\d+)\s*days?$", RegexOptions.IgnoreCase);
				if (m.Success && int.TryParse(m.Groups[1].Value, out value))
					return true;
				return int.TryParse(t, out value);
			}

			if (Is(enumId, optionName, SandboxOpt.BloodMoonWarning, "Blood Moon Warning"))
			{
				if (IsDisabled(t) || IsNone(t)) { value = 0; return true; }
				if (Eq(t, "morning")) { value = 1; return true; }
				if (Eq(t, "evening")) { value = 2; return true; }
				return false;
			}

			if (Is(enumId, optionName, SandboxOpt.LootRespawnDays, "Loot Respawn"))
			{
				if (IsDisabled(t) || IsNone(t) || Eq(t, "never")) { value = -1; return true; }
				return int.TryParse(t, out value);
			}

			if (Is(enumId, optionName, SandboxOpt.DropOnDeath, "Drop On Death")
				|| Is(enumId, optionName, SandboxOpt.DropOnQuit, "Drop On Quit")
				|| Is(enumId, optionName, SandboxOpt.LoseItemsOnDeathType, "Lose Items Death Type"))
			{
				if (IsNone(t)) { value = 0; return true; }
				if (Eq(t, "all")) { value = 1; return true; }
				if (Has(t, "toolbelt")) { value = 2; return true; }
				if (Has(t, "backpack")) { value = 3; return true; }
				if (Has(t, "equipment")) { value = 4; return true; }
				if (Has(t, "carried")) { value = 5; return true; }
				if (Has(t, "delete")) { value = 6; return true; } // DropOnDeath only; harmless if unused
				return false;
			}

			if (Is(enumId, optionName, SandboxOpt.DegradeItemsOnDeath, "Degrade Items On Death"))
			{
				if (IsNone(t)) { value = 0; return true; }
				if (Has(t, "max")) { value = 2; return true; }
				if (Has(t, "both")) { value = 3; return true; }
				if (Has(t, "durab")) { value = 1; return true; }
				return false;
			}

			if (Is(enumId, optionName, SandboxOpt.ShowXP, "Show XP"))
			{
				if (Eq(t, "all")) { value = 0; return true; }
				if (Has(t, "bar")) { value = 1; return true; }
				if (Has(t, "notif")) { value = 2; return true; }
				if (IsNone(t)) { value = 3; return true; }
				return false;
			}

			if (Is(enumId, optionName, SandboxOpt.ShowLocationInfo, "Show Location"))
			{
				if (Eq(t, "no") || Eq(t, "false")) { value = 0; return true; }
				if (Has(t, "name")) { value = 2; return true; }
				if (Eq(t, "yes") || Eq(t, "true")) { value = 1; return true; }
				return false;
			}

			if (Is(enumId, optionName, SandboxOpt.BackpackCrafting, "Backpack Crafting"))
			{
				if (Eq(t, "no") || Eq(t, "false")) { value = 0; return true; }
				if (Eq(t, "yes") || Eq(t, "true")) { value = 1; return true; }
				if (Has(t, "limited")) { value = 2; return true; }
				if (Has(t, "workbench")) { value = 3; return true; }
				return false;
			}

			if (Is(enumId, optionName, SandboxOpt.SillyCelebrate, "Celebrate"))
			{
				if (Eq(t, "no") || Eq(t, "false")) { value = 0; return true; }
				if (Has(t, "headshot")) { value = 2; return true; }
				if (Eq(t, "yes") || Eq(t, "true")) { value = 1; return true; }
				return false;
			}

			if (Is(enumId, optionName, SandboxOpt.HeadshotMode, "Headshot Mode"))
			{
				if (IsNone(t) || IsDisabled(t)) { value = 0; return true; }
				if (Has(t, "finish")) { value = 2; return true; }
				if (Has(t, "headshot") || Has(t, "only")) { value = 1; return true; }
				return false;
			}

			if (Is(enumId, optionName, SandboxOpt.RepairTypes, "Repair"))
			{
				if (IsNone(t)) { value = 0; return true; }
				if (Has(t, "repair") && !Has(t, "combine")) { value = 1; return true; }
				if (Has(t, "combine") && !Has(t, "repair")) { value = 2; return true; }
				if (Has(t, "both")) { value = 3; return true; }
				return false;
			}

			if (Is(enumId, optionName, SandboxOpt.TraderHours, "Trader Hours"))
			{
				if (Eq(t, "default")) { value = 0; return true; }
				if (Eq(t, "morning")) { value = 1; return true; }
				if (Has(t, "mid")) { value = 2; return true; }
				if (Eq(t, "evening")) { value = 3; return true; }
				if (Eq(t, "night")) { value = 4; return true; }
				if (Has(t, "blood") || Has(t, "bm")) { value = 5; return true; }
				if (Has(t, "always")) { value = 6; return true; }
				return false;
			}

			if (Is(enumId, optionName, SandboxOpt.TraderProtection, "Trader Protection"))
			{
				// Vanilla TraderArea: Yes=0, Claimable=1, NotClaimable=2
				if (Eq(t, "yes") || Eq(t, "true")) { value = 0; return true; }
				if (Has(t, "not") && Has(t, "claim")) { value = 2; return true; }
				if (Has(t, "claim")) { value = 1; return true; }
				return false;
			}

			if (Is(enumId, optionName, SandboxOpt.QuestProgressionDailyLimit, "Quests per Day"))
			{
				if (IsNone(t) || IsDisabled(t) || Has(t, "unlimited")) { value = -1; return true; }
				return int.TryParse(t, out value);
			}

			return false;
		}

		public static bool TryMapFloat(SandboxOpt enumId, string optionName, string choice, out float value)
		{
			value = 0f;
			if (string.IsNullOrEmpty(choice))
				return false;
			var t = choice.Trim();

			// Encumbrance: Disabled=10, Low=1.35, Default=1, High=0.7, VeryHigh=0.35, Full=0
			if (Is(enumId, optionName, SandboxOpt.EncumbranceModifier, "Encumbrance"))
			{
				if (IsDisabled(t) || IsNone(t)) { value = 10f; return true; }
				if (Has(t, "very high")) { value = 0.35f; return true; }
				if (Eq(t, "high") || Starts(t, "high (")) { value = 0.7f; return true; }
				if (Eq(t, "default") || Starts(t, "default (")) { value = 1f; return true; }
				if (Eq(t, "low") || Starts(t, "low (")) { value = 1.35f; return true; }
				if (Has(t, "full")) { value = 0f; return true; }
				return false;
			}

			return false;
		}

		public static bool TryMapBool(SandboxOpt enumId, string optionName, string choice, out bool value)
		{
			value = false;
			if (string.IsNullOrEmpty(choice))
				return false;
			var t = choice.Trim();

			// SmeltingType: Smelter=false, Recipes=true
			if (Is(enumId, optionName, SandboxOpt.SmeltingType, "Smelter"))
			{
				if (Has(t, "recipe")) { value = true; return true; }
				if (Has(t, "smelter")) { value = false; return true; }
				return false;
			}

			return false;
		}

		static bool TryMapZombieSpeedClass(string t, bool smellMode, out int value)
		{
			value = 0;
			if (Eq(t, "walk")) { value = smellMode ? 1 : 0; return true; }
			if (Eq(t, "jog")) { value = smellMode ? 2 : 1; return true; }
			if (Eq(t, "run")) { value = smellMode ? 3 : 2; return true; }
			if (Eq(t, "sprint")) { value = smellMode ? 4 : 3; return true; }
			if (Eq(t, "nightmare")) { value = smellMode ? 5 : 4; return true; }
			if (smellMode && (IsNone(t) || IsDisabled(t))) { value = 0; return true; }
			return false;
		}

		static bool IsDensity(SandboxOpt enumId, string optionName)
		{
			switch (enumId)
			{
				case SandboxOpt.BiomeDayEnemyDensity:
				case SandboxOpt.BiomeNightEnemyDensity:
				case SandboxOpt.BiomeDayAnimalDensity:
				case SandboxOpt.BiomeNightAnimalDensity:
					return true;
			}
			return NameHas(optionName, "Density");
		}

		static bool IsRespawn(SandboxOpt enumId, string optionName)
		{
			switch (enumId)
			{
				case SandboxOpt.BiomeDayZombieRespawn:
				case SandboxOpt.BiomeNightZombieRespawn:
				case SandboxOpt.BiomeDayAnimalRespawn:
				case SandboxOpt.BiomeNightAnimalRespawn:
					return true;
			}
			return NameHas(optionName, "Respawn");
		}

		static bool Is(SandboxOpt enumId, string optionName, SandboxOpt want, string namePart)
		{
			if (enumId == want)
				return true;
			return NameHas(optionName, namePart);
		}

		static bool NameHas(string optionName, string part) =>
			optionName != null && optionName.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0;

		static bool Eq(string a, string b) =>
			string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

		static bool Has(string a, string b) =>
			a.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0;

		static bool Starts(string a, string b) =>
			a.StartsWith(b, StringComparison.OrdinalIgnoreCase);

		static bool IsNone(string t) => Eq(t, "none") || Eq(t, "off");

		static bool IsDisabled(string t) => Eq(t, "disabled") || Eq(t, "off");
	}
}
