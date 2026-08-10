using System;
using System.Collections.Generic;
using SandboxOpt = global::SandboxOptions.SandboxOptions;

namespace DoomSandbox
{
	/// <summary>
	/// Maps worksheet names to vanilla SandboxOptions enum slots.
	/// Doom-only options borrow silly slots (Gravity / Big Heads / Tiny Zombies).
	/// </summary>
	public static class DoomSandboxNameMap
	{
		/// <summary>Enums whose vanilla effects we neutralize because Doom UI owns the slot.</summary>
		public static readonly SandboxOpt[] NeutralizeBorrowedEffects =
		{
			SandboxOpt.SillySounds,
			SandboxOpt.SillyBigHeads,
			SandboxOpt.SillyTinyZombies,
			SandboxOpt.SillyBlackandWhite,
		};

		static readonly Dictionary<string, SandboxOpt> Exact =
			new Dictionary<string, SandboxOpt>(StringComparer.OrdinalIgnoreCase)
			{
				{ "Player Damage Taken", SandboxOpt.IncomingDamage },
				{ "Incoming Damage", SandboxOpt.IncomingDamage },
				{ "Enemy Health", SandboxOpt.EntityIncomingDamage },
				{ "Entity Incoming Damage", SandboxOpt.EntityIncomingDamage },
				{ "Game stage modifier", SandboxOpt.GlobalGSModifier },
				{ "Global GameStage", SandboxOpt.GlobalGSModifier },
				{ "Loot stage modifier", SandboxOpt.GlobalLSModifier },
				{ "Global LootStage", SandboxOpt.GlobalLSModifier },

				{ "Enemy Spawn", SandboxOpt.EnemySpawnMode },
				{ "Max Enemy Type", SandboxOpt.MaxEnemyTier },

				{ "Day Enemy Density", SandboxOpt.BiomeDayEnemyDensity },
				{ "Biome Enemy Density", SandboxOpt.BiomeDayEnemyDensity },
				{ "Day Enemy Respawn", SandboxOpt.BiomeDayZombieRespawn },
				{ "Day Biome Enemy Respawn", SandboxOpt.BiomeDayZombieRespawn },
				{ "Biome Enemy Respawn", SandboxOpt.BiomeDayZombieRespawn },
				{ "Day Animal Density", SandboxOpt.BiomeDayAnimalDensity },
				{ "Day Biome Animal Density", SandboxOpt.BiomeDayAnimalDensity },
				{ "Day Animal Respawn", SandboxOpt.BiomeDayAnimalRespawn },
				{ "Day Biome Animal Respawn", SandboxOpt.BiomeDayAnimalRespawn },
				{ "Biome Animal Respawn", SandboxOpt.BiomeDayAnimalRespawn },
				{ "Night Enemy Density", SandboxOpt.BiomeNightEnemyDensity },
				{ "Night Enemy Respawn", SandboxOpt.BiomeNightZombieRespawn },
				{ "Night Biome Enemy Respawn", SandboxOpt.BiomeNightZombieRespawn },
				{ "Night Animal Density", SandboxOpt.BiomeNightAnimalDensity },
				{ "Night Biome Animal Density", SandboxOpt.BiomeNightAnimalDensity },
				{ "Night Animal Respawn", SandboxOpt.BiomeNightAnimalRespawn },
				{ "Night Biome Animal Respawn", SandboxOpt.BiomeNightAnimalRespawn },
				{ "Crouch Run Speed", SandboxOpt.CrouchRunSpeed },

				{ "Entity Block Damage", SandboxOpt.BlockDamageAI },
				{ "Blood Moon Block Damage", SandboxOpt.BlockDamageAIBM },
				{ "Zombie Day Speed", SandboxOpt.ZombieMove },
				{ "Zombie Night Speed", SandboxOpt.ZombieMoveNight },
				{ "Zombie Feral Speed", SandboxOpt.ZombieFeralMove },
				{ "Zombie Blood Moon Speed", SandboxOpt.ZombieBMMove },
				{ "Zombie AI Smell Mode", SandboxOpt.AISmellMode },
				{ "Zombie Feral Sense", SandboxOpt.ZombieFeralSense },
				{ "Level Health/Stam Bonus", SandboxOpt.PlayerLevelBonusApplied },
				{ "Skill Gain Amount", SandboxOpt.SkillPointsPerLevel },
				{ "Skill Gain Rate", SandboxOpt.SkillGainRate },
				{ "Jump Height", SandboxOpt.JumpStrength },
				{ "24 Day Cycle", SandboxOpt.DayNightLength },
				{ "Day Light Length", SandboxOpt.DayLightLength },
				{ "Mark Air Drops", SandboxOpt.AirDropMarker },
				{ "Air Drops", SandboxOpt.AirDropFrequency },
				{ "Air Drop Random Time", SandboxOpt.AirDropRandomTime },
				{ "Storm Frequency", SandboxOpt.StormFreq },
				{ "Heatmap Sensitivity", SandboxOpt.HeatMapSensitivity },
				{ "Show Day/Time", SandboxOpt.ShowDayTime },
				{ "Jar Refund", SandboxOpt.JarRefund },
				{ "Global Loot Abundance", SandboxOpt.GlobalLootCount },
				{ "Magazines Abundance", SandboxOpt.CraftingMagazinesLootCount },
				{ "Food Abundance", SandboxOpt.FoodLootCount },
				{ "Drink Abundance", SandboxOpt.DrinkLootCount },
				{ "Medical Abundance", SandboxOpt.MedicalLootCount },
				{ "Ammo Abundance", SandboxOpt.AmmoLootCount },
				{ "Resource Abundance", SandboxOpt.ResourceLootCount },
				{ "Armor Abundance", SandboxOpt.ArmorLootCount },
				{ "Melee Abundance", SandboxOpt.MeleeLootCount },
				{ "Ranged Abundance", SandboxOpt.RangedLootCount },
				{ "Dukes Abundance", SandboxOpt.DukesLootCount },
				{ "Loot Max Tier", SandboxOpt.LootMaxTier },
				{ "Loot Time", SandboxOpt.LootTimer },
				{ "Loot Bag Chance", SandboxOpt.LootBagChance },
				{ "Crop Growth", SandboxOpt.CropGrowthSpeed },
				{ "Smelter Type", SandboxOpt.SmeltingType },
				{ "Item Repair Types", SandboxOpt.RepairTypes },
				{ "Max Degrade Amount", SandboxOpt.MaxDegradationAmount },
				{ "Magazine Points", SandboxOpt.PointsPerMagazine },
				{ "Trading Enabled", SandboxOpt.TradersEnabled },
				{ "Vending Machines Enabled", SandboxOpt.VendingEnabled },
				{ "Trading Dialog", SandboxOpt.TraderDialog },
				{ "Global TraderStage", SandboxOpt.GlobalTSModifier },
				{ "Trader Item Abundance", SandboxOpt.TraderItemAbundance },
				{ "Vending Item Abundance", SandboxOpt.VendingItemAbundance },
				{ "Trader Reset Interval", SandboxOpt.TraderResetInterval },
				{ "Vending Reset Interval", SandboxOpt.VendingResetInterval },
				{ "Traders Sell Price", SandboxOpt.TraderSellPrices },
				{ "Traders Buy Price", SandboxOpt.TraderBuyPrices },
				{ "Quests per Tier", SandboxOpt.QuestsPerTier },
				{ "Quests per Day", SandboxOpt.QuestProgressionDailyLimit },
				{ "Hunger Multiplier", SandboxOpt.HungerMultiplier },
				{ "Thirst Multiplier", SandboxOpt.ThirstMultiplier },
				{ "Infection Chance", SandboxOpt.InfectionChance },
				{ "Celebrate Kills", SandboxOpt.SillyCelebrate },
				{ "Max Tech Type", SandboxOpt.MaxTechType },
				{ "Death Penalty", SandboxOpt.DeathPenalty },
				{ "Show XP", SandboxOpt.ShowXP },
				{ "Drop On Death", SandboxOpt.DropOnDeath },
				{ "Drop On Quit", SandboxOpt.DropOnQuit },
				{ "XP Multiplier", SandboxOpt.XPMultiplier },
				{ "Infection Rate", SandboxOpt.InfectionRate },
				{ "Allow Newbie Coat", SandboxOpt.NewbieCoat },
				{ "Blood Moon Frequency", SandboxOpt.BloodMoonFrequency },
				{ "Blood Moon Range", SandboxOpt.BloodMoonRange },
				{ "Blood Moon Count", SandboxOpt.BloodMoonEnemyCount },
				{ "Blood Moon Warning", SandboxOpt.BloodMoonWarning },
				{ "Storm Warning", SandboxOpt.StormWarning },
				{ "Allow Map", SandboxOpt.AllowMap },
				{ "Allow Compass", SandboxOpt.AllowCompass },
				{ "Allow Screen Markers", SandboxOpt.AllowScreenMarkers },
				{ "Show Location Info", SandboxOpt.ShowLocationInfo },
				{ "Biome GameStage", SandboxOpt.BiomeGSModifier },
				{ "Workstations in the Wild", SandboxOpt.WorkstationsInTheWild },
				{ "Loot Respawn Days", SandboxOpt.LootRespawnDays },
				{ "Treasure Map Chance", SandboxOpt.TreasureMapChance },
				{ "Mining Output", SandboxOpt.MiningOutput },
				{ "Crop Output", SandboxOpt.CropOutput },
				{ "Seed Drop Output", SandboxOpt.SeedDropOutput },
				{ "Harvesting Output", SandboxOpt.HarvestingOutput },
				{ "Crafting Max Tier", SandboxOpt.CraftingMaxTier },
				{ "Backpack Crafting", SandboxOpt.BackpackCrafting },
				{ "Workstation Crafting", SandboxOpt.WorkstationCrafting },
				{ "Crafting Time", SandboxOpt.CraftingTime },
				{ "Crafting Input", SandboxOpt.CraftingInput },
				{ "Crafting Output", SandboxOpt.CraftingOutput },
				{ "Scrapping Output", SandboxOpt.ScrappingOutput },
				{ "Dew Collector Time", SandboxOpt.DewCollectorTime },
				{ "Dew Collector Output", SandboxOpt.DewCollectorOutput },
				{ "Dew Collector Input", SandboxOpt.DewCollectorInput },
				{ "Apiary Time", SandboxOpt.ApiaryTime },
				{ "Apiary Output", SandboxOpt.ApiaryOutput },
				{ "Apiary Input", SandboxOpt.ApiaryInput },
				{ "Item Degradation", SandboxOpt.ItemDegradation },
				{ "Encumbrance Modifier", SandboxOpt.EncumbranceModifier },
				{ "Starter Skill Points", SandboxOpt.StarterSkillPoints },
				{ "Lose Items Death Type", SandboxOpt.LoseItemsOnDeathType },
				{ "Lose Items Death Count", SandboxOpt.LoseItemsOnDeathCount },
				{ "Degrade Items On Death", SandboxOpt.DegradeItemsOnDeath },
				{ "Degrade Amount On Death", SandboxOpt.DegradeAmountOnDeath },
				{ "Entity Damage", SandboxOpt.EntityDamage },
				{ "Headshot Mode", SandboxOpt.HeadshotMode },
				{ "Entity Health Bars", SandboxOpt.ShowHealthBars },
				{ "Show Entity Damage", SandboxOpt.ShowEnemyDamage },
				{ "Zombie Rage Chance", SandboxOpt.ZombieRageChance },
				{ "Zombies Eat Animals", SandboxOpt.ZombiesEatAnimals },
				{ "Allow Zombie Digging", SandboxOpt.AllowZombieDigging },
				{ "Biome Progression", SandboxOpt.BiomeProgression },
				{ "Temperature Survival", SandboxOpt.TemperatureSurvival },
				{ "Crafting Progression", SandboxOpt.CraftingProgression },
				{ "Trader Hours", SandboxOpt.TraderHours },
				{ "Trader Protection", SandboxOpt.TraderProtection },
				{ "Trader Max Tier", SandboxOpt.TraderMaxTier },
				{ "Trader Buy Limit", SandboxOpt.TraderBuyLimit },
				{ "Challenges Enabled", SandboxOpt.ChallengesEnabled },
				{ "Quests Enabled", SandboxOpt.QuestsEnabled },
				{ "Intro Challenges Enabled", SandboxOpt.IntroChallengesEnabled },
				{ "Intro Quest Enabled", SandboxOpt.IntroQuestEnabled },
				{ "Trader to Trader Quests", SandboxOpt.TraderToTraderQuestsEnabled },
				{ "Buried Quests Enabled", SandboxOpt.BuriedQuestsEnabled },
				{ "POI Quests Enabled", SandboxOpt.POIQuestsEnabled },
				{ "Ranged Damage", SandboxOpt.RangedDamage },
				{ "Melee Damage", SandboxOpt.MeleeDamage },
				{ "Block Damage", SandboxOpt.BlockDamage },
				{ "Terrain Damage", SandboxOpt.TerrainDamage },
				{ "Headshot Multiplier", SandboxOpt.HeadshotMultiplier },
				{ "Walk Speed", SandboxOpt.WalkSpeed },
				{ "Run Speed", SandboxOpt.RunSpeed },
				{ "Crouch Speed", SandboxOpt.CrouchSpeed },
				{ "Stamina Usage", SandboxOpt.StaminaUsage },
				{ "Vehicle Fuel Usage", SandboxOpt.VehicleFuelUsage },
				{ "Vehicle Entity Damage", SandboxOpt.VehicleEntityDamage },
				{ "Vehicle Block Damage", SandboxOpt.VehicleBlockDamage },
				{ "Vehicle Self Damage", SandboxOpt.VehicleSelfDamage },
				{ "Electrical Output", SandboxOpt.ElectricalOutput },

				// Doom-only options borrow silly slots (NOT Gravity — that scales player feel).
				{ "Enemy Projectile Speed", SandboxOpt.SillySounds },
				{ "Hitscanner Accuracy", SandboxOpt.SillyBigHeads },
				{ "Skill Gain Level Cap", SandboxOpt.SillyTinyZombies },
				{ "Fast Monsters", SandboxOpt.SillyBlackandWhite },

				// Real v3.1 options (no longer borrow targets).
				{ "Stack Size Multiplier", SandboxOpt.StackSizeMultiplier },
				{ "Book Abundance", SandboxOpt.BookLootCount },
				{ "Chicken Coop Time", SandboxOpt.ChickenCoopTime },
				{ "Chicken Coop Output", SandboxOpt.ChickenCoopOutput },
				{ "Chicken Coop Input", SandboxOpt.ChickenCoopInput },
				{ "Full Chicken Stress Event", SandboxOpt.FullChickenStressEvent },
			};

		/// <summary>Never silently borrow these into SillyBlackandWhite.</summary>
		static readonly HashSet<SandboxOpt> NeverBorrowTargets = new HashSet<SandboxOpt>
		{
			SandboxOpt.JarRefund,
			SandboxOpt.ZombieFeralSense,
			SandboxOpt.AISmellMode,
			SandboxOpt.TraderItemAbundance,
			SandboxOpt.VendingItemAbundance,
			SandboxOpt.TraderResetInterval,
			SandboxOpt.VendingResetInterval,
			SandboxOpt.AirDropFrequency,
			SandboxOpt.DayNightLength,
			SandboxOpt.DayLightLength,
			SandboxOpt.FullChickenStressEvent,
			SandboxOpt.MaxTechType,
			SandboxOpt.MaxEnemyTier,
			SandboxOpt.DeathPenalty,
			SandboxOpt.EncumbranceModifier,
			SandboxOpt.TraderHours,
			SandboxOpt.TraderProtection,
			SandboxOpt.RepairTypes,
			SandboxOpt.SmeltingType,
			SandboxOpt.DropOnDeath,
			SandboxOpt.BiomeDayEnemyDensity,
			SandboxOpt.BiomeDayZombieRespawn,
			SandboxOpt.LootRespawnDays,
		};

		/// <summary>
		/// Extra float-capable slots if Exact miss. SillyBlackandWhite is Fast Monsters; never Gravity.
		/// </summary>
		static readonly SandboxOpt[] BorrowSlots = Array.Empty<SandboxOpt>();

		public static bool TryResolve(
			string displayName,
			Dictionary<string, SandboxOpt> vanillaNameToEnum,
			HashSet<SandboxOpt> alreadyUsed,
			out SandboxOpt option)
		{
			if (Exact.TryGetValue(displayName, out option))
			{
				if (alreadyUsed == null || !alreadyUsed.Contains(option))
					return true;
				DoomLog.Error($"Exact map '{displayName}' → {option} already claimed; refusing borrow.");
				option = SandboxOpt.Max;
				return false;
			}

			if (vanillaNameToEnum != null && vanillaNameToEnum.TryGetValue(displayName, out option))
			{
				if (alreadyUsed == null || !alreadyUsed.Contains(option))
					return true;
				DoomLog.Error($"Vanilla name '{displayName}' → {option} already claimed; refusing borrow.");
				option = SandboxOpt.Max;
				return false;
			}

			var compact = displayName.Replace(" ", "").Replace("/", "").Replace("-", "");
			foreach (SandboxOpt v in Enum.GetValues(typeof(SandboxOpt)))
			{
				if (v == SandboxOpt.Max)
					continue;
				if (alreadyUsed != null && alreadyUsed.Contains(v))
					continue;
				if (string.Equals(v.ToString(), compact, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(v.ToString(), displayName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
				{
					option = v;
					return true;
				}
			}

			// Known vanilla worksheet names must never silently bind to SillyBlackandWhite.
			if (LooksLikeCriticalVanilla(displayName, vanillaNameToEnum))
			{
				DoomLog.Error($"Refusing SillyBlackandWhite borrow for critical option '{displayName}'.");
				option = SandboxOpt.Max;
				return false;
			}

			foreach (var borrow in BorrowSlots)
			{
				if (alreadyUsed != null && alreadyUsed.Contains(borrow))
					continue;
				option = borrow;
				DoomLog.Info($"Borrowed slot {borrow} for new option '{displayName}'");
				return true;
			}

			option = SandboxOpt.Max;
			return false;
		}

		static bool LooksLikeCriticalVanilla(
			string displayName,
			Dictionary<string, SandboxOpt> vanillaNameToEnum)
		{
			if (Exact.ContainsKey(displayName))
				return true;
			if (vanillaNameToEnum != null && vanillaNameToEnum.ContainsKey(displayName))
				return true;

			var compact = displayName.Replace(" ", "").Replace("/", "").Replace("-", "");
			foreach (SandboxOpt v in NeverBorrowTargets)
			{
				if (string.Equals(v.ToString(), compact, StringComparison.OrdinalIgnoreCase))
					return true;
			}
			return false;
		}
	}
}
