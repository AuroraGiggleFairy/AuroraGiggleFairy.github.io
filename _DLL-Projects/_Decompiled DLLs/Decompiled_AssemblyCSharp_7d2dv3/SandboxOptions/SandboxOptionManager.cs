using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

namespace SandboxOptions;

[Preserve]
public class SandboxOptionManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static SandboxOptionManager instance = null;

	public const byte FileVersion = 1;

	public const string CustomPresetName = "Custom";

	public const string CustomGroupName = "Custom";

	public const string UserGroupName = "User";

	public const string ModdedGroupName = "Modded";

	public const string IconCustomPresets = "Data/Sandbox/icons/user_custom";

	public List<SandboxOptionCategory> SandboxOptionCategories = new List<SandboxOptionCategory>();

	public Dictionary<SandboxOptions, BaseSandboxOption> SandboxOptionsDict = new Dictionary<SandboxOptions, BaseSandboxOption>();

	public Dictionary<string, SandboxOptionValueSet> ValueSets = new Dictionary<string, SandboxOptionValueSet>();

	public DictionaryList<string, List<BaseSandboxOption>> OptionsByCategory = new DictionaryList<string, List<BaseSandboxOption>>();

	public List<SandboxOptionPreset> SandboxPresets = new List<SandboxOptionPreset>();

	public static readonly SandboxOptionPreset CustomPreset = new SandboxOptionPreset
	{
		Name = "Custom",
		LocalizedName = "sandboxPresetGroupCustom",
		Group = "Custom",
		IsCustomPreset = true,
		Icon = "Data/Sandbox/icons/user_custom"
	};

	public string WorldName;

	public string GameName;

	public string CurrentPresetName = "";

	public static readonly char currentVersion = 'A';

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 originalGravity = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<SandboxOptions> overrideList;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool initRun;

	public static SandboxOptionManager Current
	{
		get
		{
			if (instance == null)
			{
				instance = new SandboxOptionManager();
			}
			return instance;
		}
	}

	public static bool HasInstance => instance != null;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte CurrentFileVersion { get; set; }

	public bool IsInit => initRun;

	[PublicizedFrom(EAccessModifier.Private)]
	public SandboxOptionManager()
	{
	}

	public void Cleanup()
	{
		instance = null;
	}

	public void Init()
	{
		originalGravity = Physics.gravity;
		if (!initRun)
		{
			SetupOptions();
			LoadPresets();
			initRun = true;
		}
		else
		{
			Log.Warning("SandboxOptionManager Init called when it's already init");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupOptions()
	{
		ValueSets.Add("DamageValues", new SandboxOptionValueSetFloat
		{
			DisplayValues = new string[1] { "none" },
			FloatValues = new float[13]
			{
				0f, 0.25f, 0.35f, 0.5f, 0.65f, 0.75f, 0.85f, 1f, 1.25f, 1.5f,
				2f, 2.5f, 3f
			},
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("DamageValuesNoNone", new SandboxOptionValueSetFloat
		{
			FloatValues = new float[12]
			{
				0.25f, 0.35f, 0.5f, 0.65f, 0.75f, 0.85f, 1f, 1.25f, 1.5f, 2f,
				2.5f, 3f
			},
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("PlayerSpeedValues", new SandboxOptionValueSetFloat
		{
			DisplayValues = new string[1] { "none" },
			FloatValues = new float[15]
			{
				0f, 0.25f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f, 1.1f, 1.2f,
				1.3f, 1.4f, 1.5f, 2f, 3f
			},
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("PlayerSpeedValuesWithNone", new SandboxOptionValueSetFloat
		{
			FloatValues = new float[14]
			{
				0.25f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f, 1.1f, 1.2f, 1.3f,
				1.4f, 1.5f, 2f, 3f
			},
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("SpeedValues", new SandboxOptionValueSetFloat
		{
			DisplayValues = new string[1] { "none" },
			FloatValues = new float[9] { 0f, 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f, 3f },
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("StaminaUsage", new SandboxOptionValueSetFloat
		{
			DisplayValues = new string[1] { "none" },
			FloatValues = new float[9] { 0f, 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f, 2f },
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("LootAbundanceValues", new SandboxOptionValueSetFloat
		{
			DisplayValues = new string[1] { "none" },
			FloatValues = new float[14]
			{
				0f, 0.25f, 0.35f, 0.5f, 0.65f, 0.75f, 0.85f, 1f, 1.25f, 1.5f,
				2f, 3f, 4f, 5f
			},
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("ZombieRageChance", new SandboxOptionValueSetFloat
		{
			DisplayValues = new string[1] { "none" },
			FloatValues = new float[10] { 0f, 0.15f, 0.3f, 0.35f, 0.4f, 0.5f, 0.6f, 0.75f, 0.9f, 1f },
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("ZombieSpeeds", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[5] { "goZMWalk", "goZMJog", "goZMRun", "goZMSprint", "goZMNightmare" },
			IntValues = new int[5] { 0, 1, 2, 3, 4 }
		});
		ValueSets.Add("AISmellMode", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[6] { "none", "goZMWalk", "goZMJog", "goZMRun", "goZMSprint", "goZMNightmare" },
			IntValues = new int[6] { 0, 1, 2, 3, 4, 5 }
		});
		ValueSets.Add("JumpStrength", new SandboxOptionValueSetFloat
		{
			DisplayValues = new string[1] { "none" },
			FloatValues = new float[7] { 0f, 0.5f, 1f, 1.25f, 1.5f, 2f, 3f },
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("StaminaRegen", new SandboxOptionValueSetFloat
		{
			FloatValues = new float[8] { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f, 3f },
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("XPGain", new SandboxOptionValueSetFloat
		{
			DisplayValues = new string[1] { "none" },
			FloatValues = new float[11]
			{
				0f, 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f, 2f, 3f,
				5f
			},
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("JarRefund", new SandboxOptionValueSetFloat
		{
			DisplayValues = new string[1] { "none" },
			FloatValues = new float[12]
			{
				0f, 0.05f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f,
				0.9f, 1f
			},
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("BarterValues", new SandboxOptionValueSetFloat
		{
			DisplayValues = new string[1] { "none" },
			FloatValues = new float[11]
			{
				0f, 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f, 2f, 3f,
				4f
			},
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("DisabledLowDefaultHigh", new SandboxOptionValueSetFloat
		{
			DisplayValues = new string[6] { "goDisabled", "goVeryLow", "goLow", "goDefault", "goHigh", "goVeryHigh" },
			FloatValues = new float[6] { 0f, 0.25f, 0.5f, 1f, 1.5f, 2f }
		});
		ValueSets.Add("LowDefaultHigh", new SandboxOptionValueSetFloat
		{
			DisplayValues = new string[5] { "goVeryLow", "goLow", "goDefault", "goHigh", "goVeryHigh" },
			FloatValues = new float[5] { 0.25f, 0.5f, 1f, 1.5f, 2f }
		});
		ValueSets.Add("Encumbrance", new SandboxOptionValueSetFloat
		{
			DisplayValues = new string[6] { "goDisabled", "goLow", "goDefault", "goHigh", "goVeryHigh", "xuiOptionsVideoTexQualityFull" },
			FloatValues = new float[6] { 10f, 1.35f, 1f, 0.7f, 0.35f, 0f }
		});
		ValueSets.Add("SkillGainRate", new SandboxOptionValueSetInt
		{
			IntValues = new int[5] { 1, 2, 3, 4, 5 }
		});
		ValueSets.Add("PointsPer", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[1] { "none" },
			IntValues = new int[8] { 0, 1, 2, 3, 4, 5, 6, 7 }
		});
		ValueSets.Add("StarterSkillPoints", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[1] { "none" },
			IntValues = new int[11]
			{
				0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
				10
			}
		});
		ValueSets.Add("BloodMoonFrequency", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[2] { "goDisabled", "goDay" },
			IntValues = new int[14]
			{
				0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
				10, 14, 20, 30
			},
			DisplayFormat = "goDays"
		});
		ValueSets.Add("BloodMoonRange", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[2] { "goDays", "goDay" },
			IntValues = new int[9] { 0, 1, 2, 3, 4, 7, 10, 14, 20 },
			DisplayFormat = "goDays"
		});
		ValueSets.Add("BloodMoonWarning", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[3] { "goDisabled", "goMorning", "goEvening" },
			IntValues = new int[3] { 0, 1, 2 }
		});
		ValueSets.Add("BloodMoonCount", new SandboxOptionValueSetInt
		{
			IntValues = new int[9] { 4, 6, 8, 10, 12, 16, 24, 32, 64 },
			DisplayFormat = "goEnemies"
		});
		ValueSets.Add("AirDrops", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[2] { "goDisabled", "goAirDropValue" },
			AlternateDisplayValues = new string[7] { "", "1", "1-3", "3", "3-7", "7", "1-7" },
			IntValues = new int[7] { 0, 1, 2, 3, 4, 5, 6 },
			DisplayFormat = "goDays"
		});
		ValueSets.Add("AirDropRandomTime", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[7] { "none", "goMorning", "goMidDayOnly", "goEvening", "goNightOnly", "goAllDay", "goAnyValue" },
			IntValues = new int[7] { 0, 1, 2, 3, 4, 5, 6 }
		});
		ValueSets.Add("StormFrequency", new SandboxOptionValueSetFloat
		{
			DisplayValues = new string[1] { "none" },
			FloatValues = new float[8] { 0f, 0.5f, 1f, 1.5f, 2f, 3f, 4f, 5f },
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("QuestPerTier", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[1] { "none" },
			IntValues = new int[16]
			{
				0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
				10, 11, 12, 13, 14, 15
			}
		});
		ValueSets.Add("QuestPerDay", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[1] { "goUnlimited" },
			IntValues = new int[11]
			{
				-1, 1, 2, 3, 4, 5, 6, 7, 8, 9,
				10
			}
		});
		ValueSets.Add("TraderArea", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[3] { "xuiYes", "goClaimable", "goNotClaimable" },
			IntValues = new int[3] { 0, 1, 2 }
		});
		ValueSets.Add("TraderResetInterval", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[2] { "xuiDefault", "goDay" },
			IntValues = new int[9] { -1, 1, 2, 3, 4, 5, 6, 7, 14 },
			DisplayFormat = "goDays"
		});
		ValueSets.Add("ItemTierOptions", new SandboxOptionValueSetInt
		{
			IntValues = new int[6] { 1, 2, 3, 4, 5, 6 }
		});
		ValueSets.Add("DewCollectorInput", new SandboxOptionValueSetFloat
		{
			DisplayValues = new string[1] { "none" },
			FloatValues = new float[4] { 0f, 1f, 2f, 3f },
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("ApiaryInput", new SandboxOptionValueSetFloat
		{
			DisplayValues = new string[1] { "none" },
			FloatValues = new float[9] { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f, 1.5f, 2f, 3f },
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("CollectorOutput", new SandboxOptionValueSetFloat
		{
			FloatValues = new float[5] { 1f, 2f, 3f, 4f, 5f },
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("BackpackCrafting", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[4] { "xuiNo", "xuiYes", "goLimited", "goWorkbenchOnly" },
			IntValues = new int[4] { 0, 1, 2, 3 }
		});
		ValueSets.Add("DeathPenalty", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[4] { "none", "goXPOnly", "goInjured", "goPermaDeath" },
			IntValues = new int[4] { 0, 1, 2, 3 }
		});
		ValueSets.Add("DropOnDeath", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[5] { "none", "lblAll", "goToolbelt", "goBackpack", "goDeleteAll" },
			IntValues = new int[5] { 0, 1, 2, 3, 4 }
		});
		ValueSets.Add("DropOnQuit", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[4] { "none", "lblAll", "goToolbelt", "goBackpack" },
			IntValues = new int[4] { 0, 1, 2, 3 }
		});
		ValueSets.Add("LoseItemsOnDeathType", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[6] { "none", "lblAll", "goToolbelt", "goBackpack", "goEquipment", "goCarriedOnly" },
			IntValues = new int[6] { 0, 1, 2, 3, 4, 5 }
		});
		ValueSets.Add("DegradeItemsOnDeath", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[4] { "none", "xuiDurability", "xuiMaxDurability", "xuiBoth" },
			IntValues = new int[4] { 0, 1, 2, 3 }
		});
		ValueSets.Add("TraderHourPresets", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[7] { "xuiDefault", "goMorning", "goMidDayOnly", "goEvening", "goNightOnly", "goOnlyClosedOnBM", "goAlwaysOpen" },
			IntValues = new int[7] { 0, 1, 2, 3, 4, 5, 6 }
		});
		ValueSets.Add("YesNo", new SandboxOptionValueSetBool
		{
			DisplayValues = new string[2] { "xuiNo", "xuiYes" },
			BoolValues = new bool[2] { false, true }
		});
		ValueSets.Add("Celebrate", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[3] { "xuiNo", "xuiYes", "goHeadshotOnly" },
			IntValues = new int[3] { 0, 1, 2 }
		});
		ValueSets.Add("ShowXP", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[4] { "lblAll", "goBarOnly", "goNotificationsOnly", "none" },
			IntValues = new int[4] { 0, 1, 2, 3 }
		});
		ValueSets.Add("HeadshotMode", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[3] { "none", "goHeadshotOnly", "goHeadshotFinisher" },
			IntValues = new int[3] { 0, 1, 2 }
		});
		ValueSets.Add("MaxEnemyType", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[6] { "goNormals", "goStrongs", "goSpecials", "goFerals", "goRadiated", "goElites" },
			IntValues = new int[6] { 0, 1, 2, 3, 4, 5 }
		});
		ValueSets.Add("MaxTechType", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[5] { "none", "goTech0", "goTech1", "goTech2", "goTech3" },
			IntValues = new int[5] { 0, 1, 2, 3, 4 }
		});
		ValueSets.Add("LoseItemCount", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[10] { "1-3", "1-5", "1-10", "1-20", "3-5", "5-7", "5-10", "7-10", "10-15", "15-20" },
			IntValues = new int[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
		});
		ValueSets.Add("DayNightLength", new SandboxOptionValueSetInt
		{
			IntValues = new int[8] { 10, 20, 30, 40, 50, 60, 90, 120 }
		});
		ValueSets.Add("DayLightLength", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[11]
			{
				"goAlwaysNight", "4", "6", "8", "10", "12", "14", "16", "18", "20",
				"goAlwaysDay"
			},
			IntValues = new int[11]
			{
				0, 4, 6, 8, 10, 12, 14, 16, 18, 20,
				24
			}
		});
		ValueSets.Add("LootRespawnDays", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[1] { "goDisabled" },
			IntValues = new int[9] { -1, 5, 7, 10, 15, 20, 30, 40, 50 }
		});
		ValueSets.Add("MaxChunkAge", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[1] { "goDisabled" },
			IntValues = new int[12]
			{
				-1, 1, 3, 5, 7, 10, 20, 30, 40, 50,
				75, 100
			}
		});
		ValueSets.Add("Gravity", new SandboxOptionValueSetFloat
		{
			FloatValues = new float[6] { 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f },
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("SlowToFast", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[6] { "xuiDefault", "goVerySlow", "goSlow", "goNormal", "goFast", "goVeryFast" },
			IntValues = new int[6] { 0, 1, 2, 3, 4, 5 }
		});
		ValueSets.Add("BiomeEnemyDensity", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[6] { "xuiDefault", "goVeryLow", "goLow", "goMedium", "goHigh", "goVeryHigh" },
			IntValues = new int[6] { 0, 1, 2, 3, 4, 5 }
		});
		ValueSets.Add("SmeltingType", new SandboxOptionValueSetBool
		{
			DisplayValues = new string[2] { "goSmelter", "lblContextActionRecipes" },
			BoolValues = new bool[2] { false, true }
		});
		ValueSets.Add("RepairTypes", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[4] { "none", "goRepairOnly", "goCombineOnly", "xuiBoth" },
			IntValues = new int[4] { 0, 1, 2, 3 }
		});
		ValueSets.Add("MaxDegradationAmounts", new SandboxOptionValueSetFloat
		{
			DisplayValues = new string[1] { "none" },
			FloatValues = new float[6] { 0f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f },
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("CropGrowthSpeed", new SandboxOptionValueSetFloat
		{
			DisplayValues = new string[2] { "none", "xuiInstant" },
			FloatValues = new float[9] { 1000f, 0f, 0.2f, 0.5f, 0.75f, 1f, 1.5f, 2f, 3f },
			DisplayFormat = "goPercent"
		});
		ValueSets.Add("ZombieFeralSense", new SandboxOptionValueSetInt
		{
			DisplayValues = new string[4] { "goDisabled", "goZMDay", "goZMNight", "goZMAll" },
			IntValues = new int[4] { 0, 1, 2, 3 }
		});
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.RangedDamage, "Ranged Damage", "General", "DamageValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.MeleeDamage, "Melee Damage", "General", "DamageValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.BlockDamage, "Block Damage", "General", "DamageValues", 1f)
		{
			OverrideOptionName = "goBlockDamagePlayer",
			OverrideDescriptionName = "goBlockDamagePlayerDesc"
		});
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.TerrainDamage, "Terrain Damage", "General", "DamageValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.HeadshotMultiplier, "Headshot Multiplier", "General", "DamageValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.IncomingDamage, "Incoming Damage", "General", "DamageValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.WalkSpeed, "Walk Speed", "General", "PlayerSpeedValuesWithNone", 1f, newUISection: true));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.RunSpeed, "Run Speed", "General", "PlayerSpeedValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.CrouchSpeed, "Crouch Speed", "General", "PlayerSpeedValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.JumpStrength, "Jump Height", "General", "JumpStrength", 1f));
		SandboxOptionFloat.DisabledOptionsOnValue disabledOptions = new SandboxOptionFloat.DisabledOptionsOnValue(new SandboxOptions[1] { SandboxOptions.StaminaRegen }, 0f);
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.StaminaRegen, "Stamina Regen", "General", "StaminaRegen", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.StaminaUsage, "Stamina Usage", "General", "StaminaUsage", 1f, newUISection: false, disabledOptions));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.XPMultiplier, "XP Multiplier", "General", "XPGain", 1f, newUISection: true));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.ShowXP, "Show XP", "General", "ShowXP", 0));
		SandboxOptionInt.DisabledOptionsOnValue disabledOptions2 = new SandboxOptionInt.DisabledOptionsOnValue(new SandboxOptions[1] { SandboxOptions.SkillPointsPerLevel }, 0);
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.PlayerLevelBonusApplied, "Level Health/Stam Bonus", "General", "YesNo", defaultValue: true));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.SkillGainRate, "Skill Gain Rate", "General", "SkillGainRate", 1, newUISection: false, disabledOptions2));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.SkillPointsPerLevel, "Skill Gain Amount", "General", "PointsPer", 1));
		disabledOptions2 = new SandboxOptionInt.DisabledOptionsOnValue(new SandboxOptions[1] { SandboxOptions.LoseItemsOnDeathCount }, 0);
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.DeathPenalty, "Death Penalty", "General", "DeathPenalty", 1, newUISection: true));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.LoseItemsOnDeathType, "Lose Items Death Type", "General", "LoseItemsOnDeathType", 0));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.LoseItemsOnDeathCount, "Lose Items Death Count", "General", "LoseItemCount", 1));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.DegradeItemsOnDeath, "Degrade Items On Death", "General", "DegradeItemsOnDeath", 0));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.DegradeAmountOnDeath, "Degrade Amount On Death", "General", "MaxDegradationAmounts", 0.1f));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.DropOnDeath, "Drop On Death", "General", "DropOnDeath", 1));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.DropOnQuit, "Drop On Quit", "General", "DropOnQuit", 0));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.InfectionRate, "Infection Rate", "General", "SpeedValues", 1f, newUISection: true));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.NewbieCoat, "Allow Newbie Coat", "General", "YesNo", defaultValue: true));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.EncumbranceModifier, "Encumbrance Modifier", "General", "Encumbrance", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.JarRefund, "Jar Refund", "General", "JarRefund", 0.6f));
		SandboxOptionBoolean.DisabledOptionsOnValue disabledOptions3 = new SandboxOptionBoolean.DisabledOptionsOnValue(new SandboxOptions[18]
		{
			SandboxOptions.MaxEnemyTier,
			SandboxOptions.BiomeEnemyDensity,
			SandboxOptions.BiomeZombieRespawn,
			SandboxOptions.BiomeAnimalRespawn,
			SandboxOptions.EntityDamage,
			SandboxOptions.EntityIncomingDamage,
			SandboxOptions.BlockDamageAI,
			SandboxOptions.BlockDamageAIBM,
			SandboxOptions.ZombieMove,
			SandboxOptions.ZombieMoveNight,
			SandboxOptions.ZombieFeralMove,
			SandboxOptions.ZombieBMMove,
			SandboxOptions.ZombieFeralSense,
			SandboxOptions.AISmellMode,
			SandboxOptions.ZombieRageChance,
			SandboxOptions.AllowZombieDigging,
			SandboxOptions.ZombiesEatAnimals,
			SandboxOptions.HeadshotMode
		}, value: false);
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.EnemySpawnMode, "Enemy Spawn", "Entities", "YesNo", defaultValue: true, newUISection: false, disabledOptions3));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.MaxEnemyTier, "Max Enemy Type", "Entities", "MaxEnemyType", 5));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.BiomeEnemyDensity, "Biome Enemy Density", "Entities", "BiomeEnemyDensity", 0));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.BiomeZombieRespawn, "Biome Enemy Respawn", "Entities", "SlowToFast", 0));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.BiomeAnimalRespawn, "Biome Animal Respawn", "Entities", "SlowToFast", 0));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.EntityDamage, "Entity Damage", "Entities", "DamageValues", 1f, newUISection: true));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.EntityIncomingDamage, "Entity Incoming Damage", "Entities", "DamageValuesNoNone", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.BlockDamageAI, "Entity Block Damage", "Entities", "DamageValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.BlockDamageAIBM, "Blood Moon Block Damage", "Entities", "DamageValues", 1f));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.HeadshotMode, "Headshot Mode", "Entities", "HeadshotMode", 0));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.ShowHealthBars, "Entity Health Bars", "Entities", "YesNo", defaultValue: false));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.ShowEnemyDamage, "Show Entity Damage", "Entities", "YesNo", defaultValue: false));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.ZombieMove, "Zombie Day Speed", "Entities", "ZombieSpeeds", 0, newUISection: true));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.ZombieMoveNight, "Zombie Night Speed", "Entities", "ZombieSpeeds", 3));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.ZombieFeralMove, "Zombie Feral Speed", "Entities", "ZombieSpeeds", 3));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.ZombieBMMove, "Zombie Blood Moon Speed", "Entities", "ZombieSpeeds", 3));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.ZombieFeralSense, "Zombie Feral Sense", "Entities", "ZombieFeralSense", 0, newUISection: true));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.AISmellMode, "Zombie AI Smell Mode", "Entities", "AISmellMode", 3));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.ZombieRageChance, "Zombie Rage Chance", "Entities", "ZombieRageChance", 0.15f));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.AllowZombieDigging, "Allow Zombie Digging", "Entities", "YesNo", defaultValue: true));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.ZombiesEatAnimals, "Zombies Eat Animals", "Entities", "YesNo", defaultValue: true));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.GlobalGSModifier, "Global GameStage", "World", "LowDefaultHigh", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.BiomeGSModifier, "Biome GameStage", "World", "LowDefaultHigh", 1f));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.BiomeProgression, "Biome Progression", "World", "YesNo", defaultValue: true));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.TemperatureSurvival, "Temperature Survival", "World", "YesNo", defaultValue: true));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.MaxTechType, "Max Tech Type", "World", "MaxTechType", 4));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.WorkstationsInTheWild, "Workstations in the Wild", "World", "JarRefund", 0f));
		disabledOptions2 = new SandboxOptionInt.DisabledOptionsOnValue(new SandboxOptions[3]
		{
			SandboxOptions.BloodMoonRange,
			SandboxOptions.BloodMoonEnemyCount,
			SandboxOptions.BloodMoonWarning
		}, 0);
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.BloodMoonFrequency, "Blood Moon Frequency", "World", "BloodMoonFrequency", 7, newUISection: true, disabledOptions2));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.BloodMoonRange, "Blood Moon Range", "World", "BloodMoonRange", 0));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.BloodMoonEnemyCount, "Blood Moon Count", "World", "BloodMoonCount", 8));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.BloodMoonWarning, "Blood Moon Warning", "World", "BloodMoonWarning", 1));
		disabledOptions2 = new SandboxOptionInt.DisabledOptionsOnValue(new SandboxOptions[2]
		{
			SandboxOptions.AirDropRandomTime,
			SandboxOptions.AirDropMarker
		}, 0);
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.AirDropFrequency, "Air Drops", "World", "AirDrops", 3, newUISection: false, disabledOptions2));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.AirDropRandomTime, "Air Drop Random Time", "World", "AirDropRandomTime", 0));
		disabledOptions = new SandboxOptionFloat.DisabledOptionsOnValue(new SandboxOptions[1] { SandboxOptions.StormWarning }, 0f);
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.StormFreq, "Storm Frequency", "World", "StormFrequency", 1f, newUISection: true, disabledOptions));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.StormWarning, "Storm Warning", "World", "YesNo", defaultValue: true));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.HeatMapSensitivity, "Heatmap Sensitivity", "World", "DisabledLowDefaultHigh", 1f));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.DayNightLength, "24 Day Cycle", "World", "DayNightLength", 60));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.DayLightLength, "Day Light Length", "World", "DayLightLength", 18));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.AirDropMarker, "Mark Air Drops", "World", "YesNo", defaultValue: true, newUISection: true)
		{
			OverrideOptionName = "goMarkAirDrops",
			OverrideDescriptionName = "goMarkAirDropsDesc"
		});
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.AllowMap, "Allow Map", "World", "YesNo", defaultValue: true));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.AllowCompass, "Allow Compass", "World", "YesNo", defaultValue: true));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.AllowScreenMarkers, "Allow Screen Markers", "World", "YesNo", defaultValue: true));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.ShowLocationInfo, "Show Location Info", "World", "YesNo", defaultValue: true));
		disabledOptions3 = new SandboxOptionBoolean.DisabledOptionsOnValue(new SandboxOptions[1] { SandboxOptions.BloodMoonWarning }, value: false);
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.ShowDayTime, "Show Day/Time", "World", "YesNo", defaultValue: true, newUISection: false, disabledOptions3));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.LootMaxTier, "Loot Max Tier", "Resources", "ItemTierOptions", 6));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.GlobalLSModifier, "Global LootStage", "Resources", "LowDefaultHigh", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.BiomeLSModifier, "Biome LootStage", "Resources", "LowDefaultHigh", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.POITierLSModifier, "POI Tier LootStage", "Resources", "LowDefaultHigh", 1f));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.LootRespawnDays, "Loot Respawn Days", "Resources", "LootRespawnDays", 7, newUISection: true));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.LootTimer, "Loot Time", "Resources", "SpeedValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.LootBagChance, "Loot Bag Chance", "Resources", "LootAbundanceValues", 1f));
		disabledOptions = new SandboxOptionFloat.DisabledOptionsOnValue(new SandboxOptions[11]
		{
			SandboxOptions.FoodLootCount,
			SandboxOptions.DrinkLootCount,
			SandboxOptions.MedicalLootCount,
			SandboxOptions.AmmoLootCount,
			SandboxOptions.ResourceLootCount,
			SandboxOptions.ArmorLootCount,
			SandboxOptions.MeleeLootCount,
			SandboxOptions.RangedLootCount,
			SandboxOptions.DukesLootCount,
			SandboxOptions.CraftingMagazinesLootCount,
			SandboxOptions.TreasureMapChance
		}, 0f);
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.GlobalLootCount, "Global Loot Abundance", "Resources", "LootAbundanceValues", 1f, newUISection: true, disabledOptions)
		{
			OverrideOptionName = "goLootAbundance",
			OverrideDescriptionName = "goLootAbundanceDesc"
		});
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.FoodLootCount, "Food Abundance", "Resources", "LootAbundanceValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.DrinkLootCount, "Drink Abundance", "Resources", "LootAbundanceValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.MedicalLootCount, "Medical Abundance", "Resources", "LootAbundanceValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.AmmoLootCount, "Ammo Abundance", "Resources", "LootAbundanceValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.ResourceLootCount, "Resource Abundance", "Resources", "LootAbundanceValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.ArmorLootCount, "Armor Abundance", "Resources", "LootAbundanceValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.MeleeLootCount, "Melee Abundance", "Resources", "LootAbundanceValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.RangedLootCount, "Ranged Abundance", "Resources", "LootAbundanceValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.DukesLootCount, "Dukes Abundance", "Resources", "LootAbundanceValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.CraftingMagazinesLootCount, "Magazines Abundance", "Resources", "LootAbundanceValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.TreasureMapChance, "Treasure Map Chance", "Resources", "PlayerSpeedValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.MiningOutput, "Mining Output", "Resources", "LootAbundanceValues", 1f, newUISection: true));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.CropOutput, "Crop Output", "Resources", "LootAbundanceValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.SeedDropOutput, "Seed Drop Output", "Resources", "LootAbundanceValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.HarvestingOutput, "Harvesting Output", "Resources", "LootAbundanceValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.CropGrowthSpeed, "Crop Growth", "Resources", "CropGrowthSpeed", 1f));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.CraftingProgression, "Crafting Progression", "Crafting", "YesNo", defaultValue: true));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.CraftingMaxTier, "Crafting Max Tier", "Crafting", "ItemTierOptions", 6));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.PointsPerMagazine, "Magazine Points", "Crafting", "PointsPer", 1));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.BackpackCrafting, "Backpack Crafting", "Crafting", "BackpackCrafting", 1));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.WorkstationCrafting, "Workstation Crafting", "Crafting", "YesNo", defaultValue: true));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.SmeltingType, "Smelter Type", "Crafting", "SmeltingType", defaultValue: false));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.CraftingTime, "Crafting Time", "Crafting", "SpeedValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.CraftingInput, "Crafting Input", "Crafting", "SpeedValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.CraftingOutput, "Crafting Output", "Crafting", "StaminaRegen", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.ScrappingOutput, "Scrapping Output", "Crafting", "SpeedValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.DewCollectorTime, "Dew Collector Time", "Crafting", "SpeedValues", 1f, newUISection: true));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.DewCollectorOutput, "Dew Collector Output", "Crafting", "CollectorOutput", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.DewCollectorInput, "Dew Collector Input", "Crafting", "DewCollectorInput", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.ApiaryTime, "Apiary Time", "Crafting", "SpeedValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.ApiaryOutput, "Apiary Output", "Crafting", "CollectorOutput", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.ApiaryInput, "Apiary Input", "Crafting", "ApiaryInput", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.ItemDegradation, "Item Degradation", "Crafting", "DisabledLowDefaultHigh", 1f, newUISection: true));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.RepairTypes, "Item Repair Types", "Crafting", "RepairTypes", 3));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.MaxDegradationAmount, "Max Degrade Amount", "Crafting", "MaxDegradationAmounts", 0f));
		disabledOptions3 = new SandboxOptionBoolean.DisabledOptionsOnValue(new SandboxOptions[7]
		{
			SandboxOptions.TraderMaxTier,
			SandboxOptions.TraderItemAbundance,
			SandboxOptions.TraderResetInterval,
			SandboxOptions.TraderSellPrices,
			SandboxOptions.TraderBuyPrices,
			SandboxOptions.TraderBuyLimit,
			SandboxOptions.GlobalTSModifier
		}, value: false);
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.TradersEnabled, "Trading Enabled", "Traders", "YesNo", defaultValue: true, newUISection: false, disabledOptions3));
		disabledOptions3 = new SandboxOptionBoolean.DisabledOptionsOnValue(new SandboxOptions[2]
		{
			SandboxOptions.VendingItemAbundance,
			SandboxOptions.VendingResetInterval
		}, value: false);
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.VendingEnabled, "Vending Machines Enabled", "Traders", "YesNo", defaultValue: true, newUISection: false, disabledOptions3));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.TraderHours, "Trader Hours", "Traders", "TraderHourPresets", 0));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.TraderProtection, "Trader Protection", "Traders", "TraderArea", 0));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.TraderDialog, "Trading Dialog", "Traders", "YesNo", defaultValue: true));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.GlobalTSModifier, "Global TraderStage", "Traders", "LowDefaultHigh", 1f, newUISection: true));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.TraderMaxTier, "Trader Max Tier", "Traders", "ItemTierOptions", 6));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.TraderItemAbundance, "Trader Item Abundance", "Traders", "LowDefaultHigh", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.VendingItemAbundance, "Vending Item Abundance", "Traders", "LowDefaultHigh", 1f));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.TraderResetInterval, "Trader Reset Interval", "Traders", "TraderResetInterval", -1));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.VendingResetInterval, "Vending Reset Interval", "Traders", "TraderResetInterval", -1));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.TraderSellPrices, "Traders Sell Price", "Traders", "BarterValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.TraderBuyPrices, "Traders Buy Price", "Traders", "BarterValues", 1f));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.TraderBuyLimit, "Trader Buy Limit", "Traders", "StarterSkillPoints", 3));
		disabledOptions3 = new SandboxOptionBoolean.DisabledOptionsOnValue(new SandboxOptions[1] { SandboxOptions.IntroChallengesEnabled }, value: false);
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.ChallengesEnabled, "Challenges Enabled", "Tasks", "YesNo", defaultValue: true, newUISection: false, disabledOptions3));
		disabledOptions3 = new SandboxOptionBoolean.DisabledOptionsOnValue(new SandboxOptions[6]
		{
			SandboxOptions.IntroQuestEnabled,
			SandboxOptions.TraderToTraderQuestsEnabled,
			SandboxOptions.BuriedQuestsEnabled,
			SandboxOptions.POIQuestsEnabled,
			SandboxOptions.QuestsPerTier,
			SandboxOptions.QuestProgressionDailyLimit
		}, value: false);
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.QuestsEnabled, "Quests Enabled", "Tasks", "YesNo", defaultValue: true));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.IntroChallengesEnabled, "Intro Challenges Enabled", "Tasks", "YesNo", defaultValue: true));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.IntroQuestEnabled, "Intro Quest Enabled", "Tasks", "YesNo", defaultValue: true));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.TraderToTraderQuestsEnabled, "Trader to Trader Quests", "Tasks", "YesNo", defaultValue: true));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.BuriedQuestsEnabled, "Buried Quests Enabled", "Tasks", "YesNo", defaultValue: true));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.POIQuestsEnabled, "POI Quests Enabled", "Tasks", "YesNo", defaultValue: true));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.QuestsPerTier, "Quests per Tier", "Tasks", "QuestPerTier", 10));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.QuestProgressionDailyLimit, "Quests per Day", "Tasks", "QuestPerDay", 4));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.StarterSkillPoints, "Starter Skill Points", "Tasks", "StarterSkillPoints", 4));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.VehicleFuelUsage, "Vehicle Fuel Usage", "Misc", "DamageValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.VehicleEntityDamage, "Vehicle Entity Damage", "Misc", "DamageValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.VehicleBlockDamage, "Vehicle Block Damage", "Misc", "DamageValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.VehicleSelfDamage, "Vehicle Self Damage", "Misc", "DamageValues", 1f));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.ElectricalOutput, "Electrical Output", "Misc", "StaminaRegen", 1f, newUISection: true));
		AddSandboxOption(new SandboxOptionInt(SandboxOptions.SillyCelebrate, "Celebrate Kills", "Misc", "Celebrate", 0, newUISection: true));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.SillyBigHeads, "Big Heads", "Misc", "YesNo", defaultValue: false));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.SillyTinyZombies, "Tiny Zombies", "Misc", "YesNo", defaultValue: false));
		AddSandboxOption(new SandboxOptionFloat(SandboxOptions.SillyLowGravity, "Gravity", "Misc", "Gravity", 1f));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.SillySounds, "Silly Sounds", "Misc", "YesNo", defaultValue: false));
		AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.SillyBlackandWhite, "Black and White", "Misc", "YesNo", defaultValue: false));
		InitValueSets();
	}

	public void InitValueSets()
	{
		foreach (SandboxOptionValueSet value in ValueSets.Values)
		{
			value.Init();
		}
	}

	public List<string> GetAllPresetGroups()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < SandboxPresets.Count; i++)
		{
			string item = SandboxPresets[i].Group;
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public bool GetPresetsForGroup(string presetGroupName)
	{
		for (int i = 0; i < SandboxPresets.Count; i++)
		{
			if (SandboxPresets[i].Group == presetGroupName)
			{
				return true;
			}
		}
		return false;
	}

	public static BaseSandboxOption.OptionTypes GetOptionType(SandboxOptions optionType)
	{
		if (Current.SandboxOptionsDict.ContainsKey(optionType))
		{
			return Current.SandboxOptionsDict[optionType].OptionType;
		}
		return BaseSandboxOption.OptionTypes.Invalid;
	}

	public static bool GetBool(SandboxOptions optionType)
	{
		if (Current.SandboxOptionsDict.ContainsKey(optionType))
		{
			if (overrideList != null && overrideList.Contains(optionType))
			{
				return Current.SandboxOptionsDict[optionType].GetDefaultBoolValue();
			}
			return Current.SandboxOptionsDict[optionType].GetBoolValue();
		}
		return false;
	}

	public static float GetFloat(SandboxOptions optionType)
	{
		if (Current.SandboxOptionsDict.ContainsKey(optionType))
		{
			if (overrideList != null && overrideList.Contains(optionType))
			{
				return Current.SandboxOptionsDict[optionType].GetDefaultFloatValue();
			}
			return Current.SandboxOptionsDict[optionType].GetFloatValue();
		}
		return 0f;
	}

	public static int GetInt(SandboxOptions optionType)
	{
		if (Current.SandboxOptionsDict.ContainsKey(optionType))
		{
			if (overrideList != null && overrideList.Contains(optionType))
			{
				return Current.SandboxOptionsDict[optionType].GetDefaultIntValue();
			}
			return Current.SandboxOptionsDict[optionType].GetIntValue();
		}
		return 0;
	}

	public static int GetIndex(SandboxOptions optionType)
	{
		if (Current.SandboxOptionsDict.ContainsKey(optionType))
		{
			return Current.SandboxOptionsDict[optionType].GetValueIndex();
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddSandboxOption(BaseSandboxOption option)
	{
		if (!OptionsByCategory.dict.ContainsKey(option.CategoryName))
		{
			OptionsByCategory.Add(option.CategoryName, new List<BaseSandboxOption>());
		}
		OptionsByCategory.dict[option.CategoryName].Add(option);
		SandboxOptionsDict.Add(option.Option, option);
	}

	public static BaseSandboxOption GetOption(SandboxOptions optionType)
	{
		if (Current.SandboxOptionsDict.ContainsKey(optionType))
		{
			return instance.SandboxOptionsDict[optionType];
		}
		return null;
	}

	public bool SetOption(SandboxOptions optionType, string value)
	{
		BaseSandboxOption baseSandboxOption = SandboxOptionsDict[optionType];
		baseSandboxOption.SetValue(value);
		return baseSandboxOption.IsChanged();
	}

	public void SetOption(SandboxOptions optionType, int value)
	{
		if (SandboxOptionsDict[optionType] is SandboxOptionInt sandboxOptionInt)
		{
			sandboxOptionInt.SetInt(value);
			if (sandboxOptionInt.DisabledOptions != null)
			{
				SandboxOptionInt.DisabledOptionsOnValue disabledOptions = sandboxOptionInt.DisabledOptions;
				bool flag = (disabledOptions.Inverted ? (value == disabledOptions.Value) : (value != disabledOptions.Value));
				string disabledByText = (flag ? "" : string.Format(Localization.Get("xuiSandboxDisabledBy"), sandboxOptionInt.OptionNameText, sandboxOptionInt.ValueOptions.GetDisplayAtIndex(sandboxOptionInt.GetValueIndex())));
				for (int i = 0; i < disabledOptions.DisabledOptions.Length; i++)
				{
					BaseSandboxOption baseSandboxOption = SandboxOptionsDict[disabledOptions.DisabledOptions[i]];
					baseSandboxOption.IsEnabled = flag;
					baseSandboxOption.DisabledByText = disabledByText;
				}
			}
		}
		else
		{
			if (!(SandboxOptionsDict[optionType] is SandboxOptionFloat sandboxOptionFloat))
			{
				return;
			}
			sandboxOptionFloat.SetFloat((float)value / 100f);
			if (sandboxOptionFloat.DisabledOptions != null)
			{
				SandboxOptionFloat.DisabledOptionsOnValue disabledOptions2 = sandboxOptionFloat.DisabledOptions;
				bool flag2 = (disabledOptions2.Inverted ? ((float)value == disabledOptions2.Value) : ((float)value != disabledOptions2.Value));
				string disabledByText2 = (flag2 ? "" : string.Format(Localization.Get("xuiSandboxDisabledBy"), sandboxOptionFloat.OptionNameText, sandboxOptionFloat.ValueOptions.GetDisplayAtIndex(sandboxOptionFloat.GetValueIndex())));
				for (int j = 0; j < disabledOptions2.DisabledOptions.Length; j++)
				{
					BaseSandboxOption baseSandboxOption2 = SandboxOptionsDict[disabledOptions2.DisabledOptions[j]];
					baseSandboxOption2.IsEnabled = flag2;
					baseSandboxOption2.DisabledByText = disabledByText2;
				}
			}
		}
	}

	public void SetOption(SandboxOptions optionType, float value)
	{
		if (!(SandboxOptionsDict[optionType] is SandboxOptionFloat sandboxOptionFloat))
		{
			return;
		}
		sandboxOptionFloat.SetFloat(value);
		if (sandboxOptionFloat.DisabledOptions != null)
		{
			SandboxOptionFloat.DisabledOptionsOnValue disabledOptions = sandboxOptionFloat.DisabledOptions;
			bool flag = (disabledOptions.Inverted ? (value == disabledOptions.Value) : (value != disabledOptions.Value));
			string disabledByText = (flag ? "" : string.Format(Localization.Get("xuiSandboxDisabledBy"), sandboxOptionFloat.OptionNameText, sandboxOptionFloat.ValueOptions.GetDisplayAtIndex(sandboxOptionFloat.GetValueIndex())));
			for (int i = 0; i < disabledOptions.DisabledOptions.Length; i++)
			{
				BaseSandboxOption baseSandboxOption = SandboxOptionsDict[disabledOptions.DisabledOptions[i]];
				baseSandboxOption.IsEnabled = flag;
				baseSandboxOption.DisabledByText = disabledByText;
			}
		}
	}

	public void SetOption(SandboxOptions optionType, bool value)
	{
		if (!(SandboxOptionsDict[optionType] is SandboxOptionBoolean sandboxOptionBoolean))
		{
			return;
		}
		sandboxOptionBoolean.SetBool(value);
		if (sandboxOptionBoolean.DisabledOptions != null)
		{
			SandboxOptionBoolean.DisabledOptionsOnValue disabledOptions = sandboxOptionBoolean.DisabledOptions;
			bool flag = (disabledOptions.Inverted ? (value == disabledOptions.Value) : (value != disabledOptions.Value));
			string disabledByText = (flag ? "" : string.Format(Localization.Get("xuiSandboxDisabledBy"), sandboxOptionBoolean.OptionNameText, sandboxOptionBoolean.ValueOptions.GetDisplayAtIndex(sandboxOptionBoolean.GetValueIndex())));
			for (int i = 0; i < disabledOptions.DisabledOptions.Length; i++)
			{
				BaseSandboxOption baseSandboxOption = SandboxOptionsDict[disabledOptions.DisabledOptions[i]];
				baseSandboxOption.IsEnabled = flag;
				baseSandboxOption.DisabledByText = disabledByText;
			}
		}
	}

	public void SetOptionToDefault(SandboxOptions optionType)
	{
		SandboxOptionsDict[optionType].SetToDefault();
	}

	public void OutputToConsole()
	{
		for (int i = 0; i < 152; i++)
		{
			if (SandboxOptionsDict.ContainsKey((SandboxOptions)i))
			{
				BaseSandboxOption baseSandboxOption = SandboxOptionsDict[(SandboxOptions)i];
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"{baseSandboxOption.OptionName}({baseSandboxOption.Option}): {baseSandboxOption.GetValueText()}");
			}
		}
	}

	public void ResetAllToDefault()
	{
		foreach (BaseSandboxOption value in SandboxOptionsDict.Values)
		{
			value.SetToDefault();
		}
	}

	public void SetWorldAndGame(string world, string game)
	{
		WorldName = world;
		GameName = game;
	}

	public SandboxOptionPreset GetPreset(string presetName)
	{
		for (int i = 0; i < SandboxPresets.Count; i++)
		{
			SandboxOptionPreset sandboxOptionPreset = SandboxPresets[i];
			if (sandboxOptionPreset.Name.EqualsCaseInsensitive(presetName))
			{
				return sandboxOptionPreset;
			}
		}
		return null;
	}

	public SandboxOptionPreset GetPresetByCode(string sandboxCode)
	{
		if (string.IsNullOrEmpty(sandboxCode))
		{
			return null;
		}
		for (int i = 0; i < SandboxPresets.Count; i++)
		{
			SandboxOptionPreset sandboxOptionPreset = SandboxPresets[i];
			if (sandboxOptionPreset.SandboxCode.EqualsCaseInsensitive(sandboxCode))
			{
				return sandboxOptionPreset;
			}
		}
		return null;
	}

	public SandboxOptionPreset GetDefaultPreset()
	{
		for (int num = SandboxPresets.Count - 1; num >= 0; num--)
		{
			SandboxOptionPreset sandboxOptionPreset = SandboxPresets[num];
			if (sandboxOptionPreset.IsDefault)
			{
				return sandboxOptionPreset;
			}
		}
		return null;
	}

	public void DeletePreset(string presetName)
	{
		for (int i = 0; i < SandboxPresets.Count; i++)
		{
			if (SandboxPresets[i].Name.EqualsCaseInsensitive(presetName))
			{
				SandboxPresets.RemoveAt(i);
				break;
			}
		}
		string text = GameIO.GetUserGameDataDir() + "/Presets/";
		if (!SdDirectory.Exists(text))
		{
			SdDirectory.CreateDirectory(text);
		}
		SdFile.Delete(text + presetName + ".xml");
		SaveDataUtils.SaveDataManager.CommitAsync();
	}

	public void SaveCurrentToPreset(SandboxOptionPreset preset)
	{
		preset.PresetValues.Clear();
		foreach (KeyValuePair<string, List<BaseSandboxOption>> item in OptionsByCategory.dict)
		{
			List<BaseSandboxOption> value = item.Value;
			for (int i = 0; i < value.Count; i++)
			{
				BaseSandboxOption baseSandboxOption = value[i];
				if (baseSandboxOption.IsChanged())
				{
					preset.PresetValues.Add(baseSandboxOption.Option, baseSandboxOption.GetValueIndex());
				}
			}
		}
	}

	public SandboxOptionPreset SaveCurrentToNewPreset(string presetName, string groupName, bool isUserPreset = false)
	{
		SandboxOptionPreset sandboxOptionPreset = new SandboxOptionPreset();
		sandboxOptionPreset.Name = presetName;
		sandboxOptionPreset.Group = groupName;
		sandboxOptionPreset.IsUserPreset = isUserPreset;
		sandboxOptionPreset.Icon = "Data/Sandbox/icons/user_custom";
		SaveCurrentToPreset(sandboxOptionPreset);
		return sandboxOptionPreset;
	}

	public void SavePresetToFile(SandboxOptionPreset preset)
	{
		string text = GameIO.GetUserGameDataDir() + "/Presets/";
		if (!SdDirectory.Exists(text))
		{
			SdDirectory.CreateDirectory(text);
		}
		string path = text + preset.Name + ".xml";
		SdFile.Delete(path);
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.CreateXmlDeclaration();
		XmlElement node = xmlDocument.AddXmlElement("preset");
		node.AddXmlElement("property").SetAttrib("name", "code").SetAttrib("value", preset.SandboxCode);
		node.AddXmlElement("property").SetAttrib("name", "description").SetAttrib("value", preset.Description ?? "");
		node.AddXmlElement("property").SetAttrib("name", "icon").SetAttrib("value", preset.Icon ?? "");
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, List<BaseSandboxOption>> item in OptionsByCategory.dict)
		{
			List<BaseSandboxOption> value = item.Value;
			bool flag = false;
			for (int i = 0; i < value.Count; i++)
			{
				BaseSandboxOption baseSandboxOption = value[i];
				if (baseSandboxOption.IsChanged())
				{
					if (!flag)
					{
						flag = true;
						stringBuilder.AppendLine();
						stringBuilder.AppendLine("\t\t *** " + item.Key.ToUpper() + " ***");
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendLine("\t\t\t" + baseSandboxOption.OptionNameText + ": " + baseSandboxOption.GetValueTextFromIndex(baseSandboxOption.GetValueIndex()));
				}
			}
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Append("\t");
			node.AddXmlComment(stringBuilder.ToString());
		}
		using Stream w = SdFile.OpenWrite(path);
		using (XmlTextWriter xmlTextWriter = new XmlTextWriter(w, Encoding.UTF8))
		{
			xmlTextWriter.Formatting = Formatting.Indented;
			xmlTextWriter.Indentation = 1;
			xmlTextWriter.IndentChar = '\t';
			xmlDocument.Save(xmlTextWriter);
		}
		SaveDataUtils.SaveDataManager.CommitAsync();
	}

	public void SavePresetToDict(SandboxOptionPreset preset)
	{
		for (int i = 0; i < SandboxPresets.Count; i++)
		{
			if (SandboxPresets[i].Name.EqualsCaseInsensitive(preset.Name))
			{
				SandboxPresets[i] = preset;
				return;
			}
		}
		SandboxPresets.Add(preset);
	}

	public void SavePreset(SandboxOptionPreset preset, bool addToDictionary, bool saveToFile)
	{
		if (saveToFile)
		{
			SavePresetToFile(preset);
		}
		if (addToDictionary)
		{
			SavePresetToDict(preset);
		}
	}

	public SandboxOptionPreset SaveCurrentSettings(string presetName, bool addToDictionary, bool saveToFile, bool isUserPreset = false)
	{
		SandboxOptionPreset sandboxOptionPreset = SaveCurrentToNewPreset(presetName, "User", isUserPreset);
		SavePreset(sandboxOptionPreset, addToDictionary, saveToFile);
		return sandboxOptionPreset;
	}

	public void LoadPresets()
	{
		SandboxPresets.Clear();
		LoadInternalPresets();
		string path = GameIO.GetUserGameDataDir() + "/Presets/";
		if (SdDirectory.Exists(path))
		{
			string[] files = SdDirectory.GetFiles(path);
			foreach (string path2 in files)
			{
				using Stream stream = SdFile.OpenRead(path2);
				XmlFile xmlFile = new XmlFile(stream);
				if (xmlFile != null)
				{
					SandboxOptionPreset sandboxOptionPreset = LoadPreset(xmlFile);
					if (sandboxOptionPreset != null)
					{
						sandboxOptionPreset.Name = Path.GetFileNameWithoutExtension(path2);
						SandboxPresets.Add(sandboxOptionPreset);
					}
				}
			}
		}
		SandboxPresets.Add(CustomPreset);
	}

	public SandboxOptionPreset LoadPreset(XmlFile _xmlFile)
	{
		SandboxOptionPreset sandboxOptionPreset = null;
		XElement root = _xmlFile.XmlDoc.Root;
		if (root == null || !root.HasElements)
		{
			return null;
		}
		sandboxOptionPreset = new SandboxOptionPreset
		{
			Name = _xmlFile.Filename,
			Icon = "Data/Sandbox/icons/user_custom"
		};
		string code = "";
		foreach (XElement item in root.Elements())
		{
			if (item.Name == "property" && item.HasAttribute("name") && item.HasAttribute("value"))
			{
				string attribute = item.GetAttribute("name");
				string attribute2 = item.GetAttribute("value");
				switch (attribute)
				{
				case "code":
					code = attribute2;
					break;
				case "description":
					sandboxOptionPreset.Description = attribute2;
					break;
				case "icon":
					sandboxOptionPreset.Icon = attribute2;
					break;
				}
			}
		}
		StoreOptionsInPresetFromCode(sandboxOptionPreset, code);
		sandboxOptionPreset.IsUserPreset = true;
		sandboxOptionPreset.Group = "User";
		return sandboxOptionPreset;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LoadInternalPresets()
	{
		XElement root = XDocument.Parse(((TextAsset)Resources.Load("Data/Sandbox/sandbox_presets")).text, LoadOptions.SetLineInfo).Root;
		if (root == null || !root.HasElements)
		{
			return;
		}
		foreach (XElement item in root.Elements())
		{
			if (item.Name == "preset")
			{
				LoadPresetFromXml(item);
			}
		}
	}

	public void LoadPresetFromXml(XElement childElement, string presetGroupOverride = null, bool isModded = false)
	{
		if (!childElement.HasAttribute("name") || !childElement.HasAttribute("code") || (!childElement.HasAttribute("description") && !childElement.HasAttribute("description_key")))
		{
			return;
		}
		SandboxOptionPreset sandboxOptionPreset = new SandboxOptionPreset();
		sandboxOptionPreset.Name = childElement.GetAttribute("name");
		sandboxOptionPreset.LocalizedName = childElement.GetAttribute("localized_name");
		sandboxOptionPreset.Description = childElement.GetAttribute("description");
		sandboxOptionPreset.DescriptionKey = childElement.GetAttribute("description_key");
		sandboxOptionPreset.Icon = childElement.GetAttribute("icon");
		if (childElement.HasAttribute("default") && childElement.GetAttribute("default").EqualsCaseInsensitive("true"))
		{
			sandboxOptionPreset.IsDefault = true;
		}
		childElement.ParseAttribute("difficulty_rating", ref sandboxOptionPreset.DifficultyRating);
		if (sandboxOptionPreset.DifficultyRating > 10)
		{
			sandboxOptionPreset.DifficultyRating = 10;
		}
		if (childElement.HasAttribute("always_show"))
		{
			string[] array = childElement.GetAttribute("always_show").Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				SandboxOptions result = SandboxOptions.Max;
				if (Enum.TryParse<SandboxOptions>(array[i], out result))
				{
					if (sandboxOptionPreset.AlwaysShow == null)
					{
						sandboxOptionPreset.AlwaysShow = new List<SandboxOptions>();
					}
					sandboxOptionPreset.AlwaysShow.Add(result);
				}
			}
		}
		sandboxOptionPreset.IsModded = isModded;
		if (string.IsNullOrEmpty(presetGroupOverride))
		{
			childElement.ParseAttribute("category", ref sandboxOptionPreset.Group);
		}
		else
		{
			sandboxOptionPreset.Group = presetGroupOverride;
		}
		if (StoreOptionsInPresetFromCode(sandboxOptionPreset, childElement.GetAttribute("code")))
		{
			SandboxPresets.Add(sandboxOptionPreset);
		}
	}

	public void SetValuesFromPreset(SandboxOptionPreset preset)
	{
		foreach (SandboxOptions key in preset.PresetValues.Keys)
		{
			SandboxOptionsDict[key].SetValueFromIndex(preset.PresetValues[key]);
		}
	}

	public void UpdateWorldOptionsWithSandboxOptions()
	{
		if (GetBool(SandboxOptions.SillyBigHeads))
		{
			GameEventManager.Current.SetGameEventFlag(GameEventManager.GameEventFlagTypes.BigHeadSandbox, value: true, -1f, isPermanent: true);
		}
		if (GetBool(SandboxOptions.SillyTinyZombies))
		{
			GameEventManager.Current.SetGameEventFlag(GameEventManager.GameEventFlagTypes.TinyZombiesSandbox, value: true, -1f, isPermanent: true);
		}
	}

	public void UpdateInGameValuesWithSandboxOptions(bool forceLoad = false)
	{
		World world = GameManager.Instance?.World;
		if (world == null && !forceLoad)
		{
			return;
		}
		AIDirectorBloodMoonComponent.BloodMoonFrequency = GetInt(SandboxOptions.BloodMoonFrequency);
		AIDirectorBloodMoonComponent.BloodMoonRange = GetInt(SandboxOptions.BloodMoonRange);
		AIDirectorBloodMoonComponent.BloodMoonEnemyCount = GetInt(SandboxOptions.BloodMoonEnemyCount);
		TraderInfo.TraderDialog = GetBool(SandboxOptions.TraderDialog);
		World.SandboxUseTraderArea = (TraderAreaStates)GetInt(SandboxOptions.TraderProtection);
		World.BiomeProgressionEnabled = GetBool(SandboxOptions.BiomeProgression);
		World.TemperatureSurvival = GetBool(SandboxOptions.TemperatureSurvival);
		World.StormFrequency = GetFloat(SandboxOptions.StormFreq);
		World.MapEnabled = GetBool(SandboxOptions.AllowMap);
		TraderManager.VendingEnabled = GetBool(SandboxOptions.VendingEnabled);
		TraderInfo.GlobalResetInterval = GetInt(SandboxOptions.TraderResetInterval);
		if (TraderInfo.GlobalResetInterval != -1)
		{
			TraderInfo.GlobalResetIntervalInTicks = TraderInfo.GlobalResetInterval * 24000;
		}
		else
		{
			TraderInfo.GlobalResetIntervalInTicks = -1;
		}
		TraderInfo.VendingResetInterval = GetInt(SandboxOptions.VendingResetInterval);
		if (TraderInfo.VendingResetInterval != -1)
		{
			TraderInfo.VendingResetIntervalInTicks = TraderInfo.VendingResetInterval * 24000;
		}
		else
		{
			TraderInfo.VendingResetIntervalInTicks = -1;
		}
		TraderInfo.TraderMaxTier = GetInt(SandboxOptions.TraderMaxTier);
		TraderInfo.TraderBuyLimit = GetInt(SandboxOptions.TraderBuyLimit);
		TraderInfo.TraderItemAbundance = GetFloat(SandboxOptions.TraderItemAbundance);
		TraderInfo.VendingItemAbundance = GetFloat(SandboxOptions.VendingItemAbundance);
		TraderInfo.TraderHoursPreset = (TraderInfo.TraderHourPresets)GetInt(SandboxOptions.TraderHours);
		LootContainer.GlobalCountModifier = GetFloat(SandboxOptions.GlobalLootCount);
		LootContainer.AmmoCountModifier = GetFloat(SandboxOptions.AmmoLootCount);
		LootContainer.FoodCountModifier = GetFloat(SandboxOptions.FoodLootCount);
		LootContainer.DrinkCountModifier = GetFloat(SandboxOptions.DrinkLootCount);
		LootContainer.MedicalCountModifier = GetFloat(SandboxOptions.MedicalLootCount);
		LootContainer.ResourceCountModifier = GetFloat(SandboxOptions.ResourceLootCount);
		LootContainer.ArmorCountModifier = GetFloat(SandboxOptions.ArmorLootCount);
		LootContainer.MeleeCountModifier = GetFloat(SandboxOptions.MeleeLootCount);
		LootContainer.RangedCountModifier = GetFloat(SandboxOptions.RangedLootCount);
		LootContainer.DukesCountModifier = GetFloat(SandboxOptions.DukesLootCount);
		LootContainer.MagazinesCountModifier = GetFloat(SandboxOptions.CraftingMagazinesLootCount);
		LootContainer.TreasureMapChance = GetFloat(SandboxOptions.TreasureMapChance);
		LootContainer.LootMaxTier = GetInt(SandboxOptions.LootMaxTier);
		LootContainer.NoLoot = LootContainer.GlobalCountModifier == 0f;
		LootContainer.LootTimerModifier = GetFloat(SandboxOptions.LootTimer);
		LootContainer.LootBagChance = GetFloat(SandboxOptions.LootBagChance);
		XUiM_Recipes.CraftingTimeModifier = GetFloat(SandboxOptions.CraftingTime);
		XUiM_Recipes.CraftingInputModifier = GetFloat(SandboxOptions.CraftingInput);
		XUiM_Recipes.CraftingOutputModifier = GetFloat(SandboxOptions.CraftingOutput);
		XUiM_Recipes.DisableSmelter = GetBool(SandboxOptions.SmeltingType);
		XUiM_Recipes.DewCollectorTimeModifier = GetFloat(SandboxOptions.DewCollectorTime);
		XUiM_Recipes.DewCollectorOutput = GetFloat(SandboxOptions.DewCollectorOutput);
		XUiM_Recipes.DewCollectorInput = GetFloat(SandboxOptions.DewCollectorInput);
		XUiM_Recipes.ApiaryTimeModifier = GetFloat(SandboxOptions.ApiaryTime);
		XUiM_Recipes.ApiaryOutput = GetFloat(SandboxOptions.ApiaryOutput);
		XUiM_Recipes.ApiaryInput = GetFloat(SandboxOptions.ApiaryInput);
		XUiM_Recipes.ScrappingOutputModifier = GetFloat(SandboxOptions.ScrappingOutput);
		XUiM_Recipes.MiningOutputModifier = GetFloat(SandboxOptions.MiningOutput);
		XUiM_Recipes.SeedDropOutputModifier = GetFloat(SandboxOptions.SeedDropOutput);
		XUiM_Recipes.CropOutputModifier = GetFloat(SandboxOptions.CropOutput);
		XUiM_Recipes.HarvestingOutputModifier = GetFloat(SandboxOptions.HarvestingOutput);
		XUiM_Recipes.CraftingMaxTier = GetInt(SandboxOptions.CraftingMaxTier);
		XUiM_Recipes.CraftingProgression = GetBool(SandboxOptions.CraftingProgression);
		XUiM_Recipes.BackpackCrafting = (BackpackCraftingOptions)GetInt(SandboxOptions.BackpackCrafting);
		if (XUiM_Recipes.BackpackCrafting != BackpackCraftingOptions.Enabled)
		{
			XUiM_Recipes.UpdateRecipesforBackpackCrafting();
		}
		XUiM_Recipes.WorkstationCrafting = GetBool(SandboxOptions.WorkstationCrafting);
		Quest.QuestsPerTier = GetInt(SandboxOptions.QuestsPerTier);
		Progression.SkillPointsGainRate = GetInt(SandboxOptions.SkillGainRate);
		Progression.SkillPointsPerLevel = ((Progression.SkillPointsPerLevelXML != 1) ? Progression.SkillPointsPerLevelXML : GetInt(SandboxOptions.SkillPointsPerLevel));
		XUiC_CompassWindow.ShowCompass = GetBool(SandboxOptions.AllowCompass);
		XUiC_OnScreenIcons.ShowIcons = GetBool(SandboxOptions.AllowScreenMarkers);
		XUiC_Location.ShowLocation = (XUiC_EnteringArea.ShowLocation = GetBool(SandboxOptions.ShowLocationInfo));
		DamageText.SandboxEnabled = GetBool(SandboxOptions.ShowEnemyDamage);
		AIDirector.HeatMapSensitivityModifier = GetFloat(SandboxOptions.HeatMapSensitivity);
		Physics.gravity = originalGravity * GetFloat(SandboxOptions.SillyLowGravity);
		EntityMoveHelper.AllowZombieDigging = GetBool(SandboxOptions.AllowZombieDigging);
		ItemActionAttack.MeleeDamagePercent = GetFloat(SandboxOptions.MeleeDamage);
		ItemActionAttack.RangedDamagePercent = GetFloat(SandboxOptions.RangedDamage);
		ItemActionAttack.BlockDamagePercent = GetFloat(SandboxOptions.BlockDamage);
		ItemActionAttack.HeadshotMultiplier = GetFloat(SandboxOptions.HeadshotMultiplier);
		ItemActionAttack.TerrainDamagePercent = GetFloat(SandboxOptions.TerrainDamage);
		ItemActionAttack.IncomingDamageModifier = GetFloat(SandboxOptions.IncomingDamage);
		ItemActionAttack.EntityIncomingDamageModifier = GetFloat(SandboxOptions.EntityIncomingDamage);
		ItemActionAttack.StaminaUsageMultiplier = GetFloat(SandboxOptions.StaminaUsage);
		ItemActionAttack.EntityPlayerDamagePercent = GetFloat(SandboxOptions.EntityDamage);
		ItemActionAttack.EntityBlockDamagePercent = GetFloat(SandboxOptions.BlockDamageAI);
		ItemActionAttack.BMBlockDamagePercent = GetFloat(SandboxOptions.BlockDamageAIBM);
		EntityAlive.HeadshotMode = (EntityAlive.HeadShotOnlyModes)GetInt(SandboxOptions.HeadshotMode);
		EntityAlive.CelebrateMode = (EntityAlive.CelebrateModes)GetInt(SandboxOptions.SillyCelebrate);
		EntityPlayer.GlobalGameStageModifier = GetFloat(SandboxOptions.GlobalGSModifier);
		EntityPlayer.BiomeGameStageModifier = GetFloat(SandboxOptions.BiomeGSModifier);
		EntityPlayer.GlobalLootStageModifier = GetFloat(SandboxOptions.GlobalLSModifier);
		EntityPlayer.BiomeLootStageModifier = GetFloat(SandboxOptions.BiomeLSModifier);
		EntityPlayer.POITierLootStageModifier = GetFloat(SandboxOptions.POITierLSModifier);
		EntityPlayer.GlobalTraderStageModifier = GetFloat(SandboxOptions.GlobalTSModifier);
		EntityPlayer.FallDamageModifier = ((GetFloat(SandboxOptions.JumpStrength) == 0f) ? 0f : (1f / GetFloat(SandboxOptions.JumpStrength)));
		EntityVehicle.VehicleFuelUsageModifier = GetFloat(SandboxOptions.VehicleFuelUsage);
		EntityVehicle.VehicleEntityDamageModifier = GetFloat(SandboxOptions.VehicleEntityDamage);
		EntityVehicle.VehicleBlockDamageModifier = GetFloat(SandboxOptions.VehicleBlockDamage);
		EntityVehicle.VehicleSelfDamageModifier = GetFloat(SandboxOptions.VehicleSelfDamage);
		PowerSource.PowerOutputModifier = GetFloat(SandboxOptions.ElectricalOutput);
		Progression.XPGain = GetFloat(SandboxOptions.XPMultiplier);
		Progression.ShowXPType = (Progression.ShowXPTypes)GetInt(SandboxOptions.ShowXP);
		ItemAction.ItemDegradationModifier = GetFloat(SandboxOptions.ItemDegradation);
		ItemAction.ItemMaxDegrationAmount = GetFloat(SandboxOptions.MaxDegradationAmount);
		ItemAction.RepairType = (ItemAction.RepairTypes)GetInt(SandboxOptions.RepairTypes);
		EntityHuman.SetupRageChance(GetFloat(SandboxOptions.ZombieRageChance), GetIndex(SandboxOptions.ZombieRageChance));
		List<EntityPlayerLocal> list = world?.GetLocalPlayers();
		ChallengeJournal.AllowChallenges = GetBool(SandboxOptions.ChallengesEnabled);
		ChallengeJournal.IntroChallengesEnabled = GetBool(SandboxOptions.IntroChallengesEnabled);
		QuestJournal.IntroQuestEnabled = GetBool(SandboxOptions.IntroQuestEnabled);
		QuestJournal.BuriedQuestsEnabled = GetBool(SandboxOptions.BuriedQuestsEnabled);
		QuestJournal.POIQuestsEnabled = GetBool(SandboxOptions.POIQuestsEnabled);
		SkyManager.isAllTimeDay = GetInt(SandboxOptions.DayLightLength) == 24;
		SkyManager.isAllTimeNight = GetInt(SandboxOptions.DayLightLength) == 0;
		GameManager gameManager = GameManager.Instance;
		if ((object)gameManager != null && !gameManager.IsEditMode())
		{
			GameStats.Set(EnumGameStats.TimeOfDayIncPerSec, 24000 / (GetInt(SandboxOptions.DayNightLength) * 60));
		}
		ItemClass.MaxTechType = (ItemClass.ItemTechTypes)GetInt(SandboxOptions.MaxTechType);
		EntityFactory.MaxEntityTier = (EntityClass.EntityTierTypes)GetInt(SandboxOptions.MaxEnemyTier);
		Log.Out($"MaxEnemyTier IsEnabled: {IsEnabled(SandboxOptions.MaxEnemyTier)}");
		EntityFactory.EnemySpawnMode = GetBool(SandboxOptions.EnemySpawnMode);
		ChunkAreaBiomeSpawnData.RespawnDelayIndexEnemies = GetInt(SandboxOptions.BiomeZombieRespawn);
		ChunkAreaBiomeSpawnData.RespawnDelayIndexAnimals = GetInt(SandboxOptions.BiomeAnimalRespawn);
		ChunkAreaBiomeSpawnData.RespawnCountOverride = GetInt(SandboxOptions.BiomeEnemyDensity);
		EAISetNearestCorpseAsTarget.ZombiesEatAnimalCorpses = GetBool(SandboxOptions.ZombiesEatAnimals);
		EAIManager.FeralSense = GetInt(SandboxOptions.ZombieFeralSense);
		SetupBloodMoonWarningTimes();
		SetupAirDropTimeRanges();
		GameStats.Set(EnumGameStats.GlobalGSModifier, GetInt(SandboxOptions.GlobalGSModifier));
		GameStats.Set(EnumGameStats.BiomeGSModifier, GetInt(SandboxOptions.BiomeGSModifier));
		GameStats.Set(EnumGameStats.GlobalLSModifier, GetInt(SandboxOptions.GlobalLSModifier));
		GameStats.Set(EnumGameStats.BiomeLSModifier, GetInt(SandboxOptions.BiomeLSModifier));
		int value = GetInt(SandboxOptions.XPMultiplier);
		int value2 = GetInt(SandboxOptions.BlockDamage);
		int value3 = GetInt(SandboxOptions.BlockDamageAI);
		int value4 = GetInt(SandboxOptions.BlockDamageAIBM);
		int value5 = GetInt(SandboxOptions.GlobalLootCount);
		int value6 = GetInt(SandboxOptions.LootRespawnDays);
		if (!(SingletonMonoBehaviour<ConnectionManager>.Instance != null) || !SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			GamePrefs.Set(EnumGamePrefs.XPMultiplier, value);
			GamePrefs.Set(EnumGamePrefs.BlockDamagePlayer, value2);
			GamePrefs.Set(EnumGamePrefs.BlockDamageAI, value3);
			GamePrefs.Set(EnumGamePrefs.BlockDamageAIBM, value4);
			GamePrefs.Set(EnumGamePrefs.LootAbundance, value5);
			GamePrefs.Set(EnumGamePrefs.LootRespawnDays, value6);
		}
		GameStats.Set(EnumGameStats.XPMultiplier, value);
		GameStats.Set(EnumGameStats.BlockDamagePlayer, value2);
		GameStats.Set(EnumGameStats.BlockDamageAI, value3);
		GameStats.Set(EnumGameStats.BlockDamageAIBM, value4);
		GameStats.Set(EnumGameStats.LootAbundance, value5);
		GameStats.Set(EnumGameStats.LootRespawnDays, value6);
		GameStats.Set(EnumGameStats.DayNightLength, GetInt(SandboxOptions.DayNightLength));
		GameStats.Set(EnumGameStats.DayLightLength, GetInt(SandboxOptions.DayLightLength));
		SetupLostItemsOnDeathValues();
		GameStats.Set(EnumGameStats.BloodMoonEnemyCount, GetInt(SandboxOptions.BloodMoonEnemyCount));
		GameStats.Set(EnumGameStats.EnemySpawnMode, GetBool(SandboxOptions.EnemySpawnMode));
		GameStats.Set(EnumGameStats.BloodMoonWarning, GetInt(SandboxOptions.BloodMoonWarning));
		GameStats.Set(EnumGameStats.StormFreq, GetInt(SandboxOptions.StormFreq));
		GameStats.Set(EnumGameStats.BiomeProgression, GetBool(SandboxOptions.BiomeProgression));
		EntityPlayerLocal.DegradeOnDeathType = (EntityPlayerLocal.DegradeOnDeathTypes)GetInt(SandboxOptions.DegradeItemsOnDeath);
		EntityPlayerLocal.DegradeAmountOnDeath = GetFloat(SandboxOptions.DegradeAmountOnDeath);
		BlockPlantGrowing.CropGrowthModifier = GetFloat(SandboxOptions.CropGrowthSpeed);
		if (list != null && list.Count > 0)
		{
			EntityPlayerLocal entityPlayerLocal = list[0];
			entityPlayerLocal.Stats.UpdateSandboxOptions();
			if (!QuestJournal.IntroQuestEnabled)
			{
				entityPlayerLocal.Buffs.SetCustomVar("IntroComplete", 1f);
			}
			entityPlayerLocal.Buffs.SetCustomVar("_carrySandboxModifier", GetFloat(SandboxOptions.EncumbranceModifier));
			entityPlayerLocal.Buffs.SetCustomVar("TraderToTraderQuests", GetBool(SandboxOptions.TraderToTraderQuestsEnabled) ? 1 : 0);
			entityPlayerLocal.Buffs.SetCustomVar("_InfectionRate", GetFloat(SandboxOptions.InfectionRate) * 1.25f);
			entityPlayerLocal.Buffs.SetCustomVar("_InfectionCureRate", GetFloat(SandboxOptions.InfectionRate) * -3f);
			EntityPlayerLocal.StormWarning = GetBool(SandboxOptions.StormWarning);
			entityPlayerLocal.Progression.UpdateForSandbox();
			Manager.Instance.bUseAltSounds = GetBool(SandboxOptions.SillySounds);
			if (GetBool(SandboxOptions.SillyBlackandWhite))
			{
				if (entityPlayerLocal.Buffs.GetBuff("sandbox_blackandwhite") == null)
				{
					entityPlayerLocal.Buffs.AddBuff("sandbox_blackandwhite");
				}
			}
			else if (entityPlayerLocal.Buffs.GetBuff("sandbox_blackandwhite") != null)
			{
				entityPlayerLocal.Buffs.RemoveBuff("sandbox_blackandwhite");
			}
			if (!ChallengeJournal.IntroChallengesEnabled)
			{
				entityPlayerLocal.challengeJournal.CompleteIntroChallenges();
			}
			if ((!ChallengeJournal.AllowChallenges || !ChallengeJournal.IntroChallengesEnabled) && Quest.StarterQuest != "" && entityPlayerLocal.QuestJournal.FindQuest(Quest.StarterQuest) == null && entityPlayerLocal.Buffs.GetCustomVar("StarterQuest") == 0f)
			{
				GameEventManager.Current.HandleAction("challenge_group_reward_basics", null, entityPlayerLocal, twitchActivated: false);
			}
		}
		UpdateWorldOptionsWithSandboxOptions();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupBloodMoonWarningTimes()
	{
		switch (GetInt(SandboxOptions.BloodMoonWarning))
		{
		case 0:
			World.BloodMoonWarningHour = -1;
			break;
		case 1:
			World.BloodMoonWarningHour = 8;
			break;
		case 2:
			World.BloodMoonWarningHour = 18;
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupAirDropTimeRanges()
	{
		ulong minTimeOfDay = 12000uL;
		ulong maxTimeOfDay = 12000uL;
		int num = 3;
		int num2 = 3;
		switch (GetInt(SandboxOptions.AirDropRandomTime))
		{
		case 1:
			minTimeOfDay = 4000uL;
			maxTimeOfDay = 10000uL;
			break;
		case 2:
			minTimeOfDay = 10000uL;
			maxTimeOfDay = 14000uL;
			break;
		case 3:
			minTimeOfDay = 15000uL;
			maxTimeOfDay = 20000uL;
			break;
		case 4:
			minTimeOfDay = 20000uL;
			maxTimeOfDay = 4000uL;
			break;
		case 5:
			minTimeOfDay = 4000uL;
			maxTimeOfDay = 20000uL;
			break;
		case 6:
			minTimeOfDay = 0uL;
			maxTimeOfDay = 23999uL;
			break;
		}
		switch (GetInt(SandboxOptions.AirDropFrequency))
		{
		case 0:
			num = (num2 = 0);
			break;
		case 1:
			num = (num2 = 1);
			break;
		case 2:
			num = 1;
			num2 = 3;
			break;
		case 3:
			num = (num2 = 3);
			break;
		case 4:
			num = 3;
			num2 = 7;
			break;
		case 5:
			num = (num2 = 7);
			break;
		case 6:
			num = 1;
			num2 = 7;
			break;
		}
		if (GetInt(SandboxOptions.AirDropFrequency) != 3)
		{
			GameStats.Set(EnumGameStats.AirDropFrequency, (num == num2) ? num : 3);
		}
		GameStats.Set(EnumGameStats.AirDropMarker, GetBool(SandboxOptions.AirDropMarker));
		AIDirectorAirDropComponent.MinTimeOfDay = minTimeOfDay;
		AIDirectorAirDropComponent.MaxTimeOfDay = maxTimeOfDay;
		AIDirectorAirDropComponent.MinDayCount = num;
		AIDirectorAirDropComponent.MaxDayCount = num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupLostItemsOnDeathValues()
	{
		switch (GetInt(SandboxOptions.LoseItemsOnDeathCount))
		{
		case 1:
			EntityPlayerLocal.LostItemOnDeathMin = 1;
			EntityPlayerLocal.LostItemOnDeathMax = 3;
			break;
		case 2:
			EntityPlayerLocal.LostItemOnDeathMin = 1;
			EntityPlayerLocal.LostItemOnDeathMax = 5;
			break;
		case 3:
			EntityPlayerLocal.LostItemOnDeathMin = 1;
			EntityPlayerLocal.LostItemOnDeathMax = 10;
			break;
		case 4:
			EntityPlayerLocal.LostItemOnDeathMin = 1;
			EntityPlayerLocal.LostItemOnDeathMax = 20;
			break;
		case 5:
			EntityPlayerLocal.LostItemOnDeathMin = 3;
			EntityPlayerLocal.LostItemOnDeathMax = 5;
			break;
		case 6:
			EntityPlayerLocal.LostItemOnDeathMin = 5;
			EntityPlayerLocal.LostItemOnDeathMax = 7;
			break;
		case 7:
			EntityPlayerLocal.LostItemOnDeathMin = 5;
			EntityPlayerLocal.LostItemOnDeathMax = 10;
			break;
		case 8:
			EntityPlayerLocal.LostItemOnDeathMin = 7;
			EntityPlayerLocal.LostItemOnDeathMax = 10;
			break;
		case 9:
			EntityPlayerLocal.LostItemOnDeathMin = 10;
			EntityPlayerLocal.LostItemOnDeathMax = 15;
			break;
		case 10:
			EntityPlayerLocal.LostItemOnDeathMin = 15;
			EntityPlayerLocal.LostItemOnDeathMax = 20;
			break;
		}
		GameStats.Set(EnumGameStats.DeathPenalty, GetInt(SandboxOptions.DeathPenalty));
		GameStats.Set(EnumGameStats.DropOnDeath, GetInt(SandboxOptions.DropOnDeath));
		GameStats.Set(EnumGameStats.DropOnQuit, GetInt(SandboxOptions.DropOnQuit));
	}

	public bool TryGetPresetDescription(string presetName, out string description)
	{
		SandboxOptionPreset preset = GetPreset(presetName);
		description = "";
		if (preset == null)
		{
			return false;
		}
		description = preset.Description;
		return true;
	}

	public bool GetChangedPresetOptions(string presetName, out List<(string name, string value, bool isDefault)> valuesList)
	{
		SandboxOptionPreset preset = GetPreset(presetName);
		return GetChangedPresetOptions(preset, out valuesList);
	}

	public bool GetChangedPresetOptions(SandboxOptionPreset preset, out List<(string name, string value, bool isDefault)> valuesList)
	{
		valuesList = null;
		if (preset == null)
		{
			return false;
		}
		valuesList = new List<(string, string, bool)>();
		foreach (SandboxOptions key2 in preset.PresetValues.Keys)
		{
			BaseSandboxOption baseSandboxOption = SandboxOptionsDict[key2];
			valuesList.Add((baseSandboxOption.OptionNameText, baseSandboxOption.GetValueTextFromIndex(preset.PresetValues[key2]), baseSandboxOption.GetDefaultIndex() == preset.PresetValues[key2]));
		}
		if (preset.AlwaysShow != null)
		{
			for (int i = 0; i < preset.AlwaysShow.Count; i++)
			{
				SandboxOptions key = preset.AlwaysShow[i];
				BaseSandboxOption baseSandboxOption2 = SandboxOptionsDict[key];
				string optionName = baseSandboxOption2.OptionNameText;
				if (valuesList.FindIndex([PublicizedFrom(EAccessModifier.Internal)] ((string name, string value, bool isDefault) _tuple) => _tuple.name.Equals(optionName)) < 0)
				{
					valuesList.Add((optionName, baseSandboxOption2.GetDefaultValueText(), true));
				}
			}
		}
		return true;
	}

	public bool StoreOptionsInPresetFromCode(SandboxOptionPreset p, string code)
	{
		if ((code == "" || code[0] != currentVersion) && p.IsUserPreset)
		{
			return false;
		}
		ResetAllToDefault();
		if (code != "")
		{
			code = code.Substring(1);
			int num = code.Length / 3;
			for (int i = 0; i < num; i++)
			{
				int num2 = i * 3;
				string value = code.Substring(num2, 2);
				char value2 = code[num2 + 2];
				SandboxOptions key = (SandboxOptions)Alpha2ToIndex(value);
				int num3 = AlphaToIndex(value2);
				if (SandboxOptionsDict.ContainsKey(key) && SandboxOptionsDict[key].GetValueSet().IsValidIndex(num3))
				{
					p.PresetValues.Add(key, num3);
				}
			}
		}
		return true;
	}

	public bool LoadOptionsFromCode(string code)
	{
		ResetAllToDefault();
		if (code == "" || code[0] != currentVersion)
		{
			return false;
		}
		code = code.Substring(1);
		int num = code.Length / 3;
		for (int i = 0; i < num; i++)
		{
			int num2 = i * 3;
			string value = code.Substring(num2, 2);
			char value2 = code[num2 + 2];
			SandboxOptions key = (SandboxOptions)Alpha2ToIndex(value);
			int valueFromIndex = AlphaToIndex(value2);
			if (SandboxOptionsDict.ContainsKey(key))
			{
				SandboxOptionsDict[key].SetValueFromIndex(valueFromIndex);
			}
		}
		return true;
	}

	public bool LoadOptionsFromCode(string code, SandboxOptionPreset preset)
	{
		if (code == "" || code[0] != currentVersion || preset == null)
		{
			return false;
		}
		preset.PresetValues.Clear();
		code = code.Substring(1);
		int num = code.Length / 3;
		for (int i = 0; i < num; i++)
		{
			int num2 = i * 3;
			string value = code.Substring(num2, 2);
			char value2 = code[num2 + 2];
			SandboxOptions key = (SandboxOptions)Alpha2ToIndex(value);
			int num3 = AlphaToIndex(value2);
			if (SandboxOptionsDict.ContainsKey(key) && SandboxOptionsDict[key].GetValueSet().IsValidIndex(num3))
			{
				preset.PresetValues.Add(key, num3);
			}
		}
		return true;
	}

	public static char IndexToAlpha(int index)
	{
		if (index < 0 || index > 26)
		{
			throw new ArgumentOutOfRangeException("Index", $"Index was out of range of the code conversion: {index}");
		}
		return (char)(65 + index % 26);
	}

	public static string IndexToAlpha2(int index)
	{
		if (index < 0 || index >= 676)
		{
			throw new ArgumentOutOfRangeException("Index", $"Index was out of range of the code conversion: {index}");
		}
		char c = (char)(65 + index / 26);
		char c2 = (char)(65 + index % 26);
		return $"{c}{c2}";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int AlphaToIndex(char value)
	{
		value = char.ToUpper(value);
		if (value < 'A' || value > 'Z')
		{
			throw new ArgumentException("Value must contain only A–Z.");
		}
		return value - 65;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int Alpha2ToIndex(string value)
	{
		if (string.IsNullOrWhiteSpace(value) || value.Length != 2)
		{
			throw new ArgumentException("Value must be exactly 2 letters.");
		}
		value = value.ToUpperInvariant();
		if (value[0] < 'A' || value[0] > 'Z' || value[1] < 'A' || value[1] > 'Z')
		{
			throw new ArgumentException("Value must contain only A–Z.");
		}
		return (value[0] - 65) * 26 + (value[1] - 65);
	}

	public void RemoveOverrides()
	{
		for (int num = SandboxPresets.Count - 1; num >= 0; num--)
		{
			if (SandboxPresets[num].IsModded)
			{
				SandboxPresets.RemoveAt(num);
			}
		}
		if (overrideList != null)
		{
			overrideList = null;
		}
	}

	public bool IsEnabled(SandboxOptions option)
	{
		if (!SandboxOptionsDict[option].IsEnabled)
		{
			return false;
		}
		return true;
	}

	public bool IsOverriden(SandboxOptions option)
	{
		if (overrideList != null)
		{
			return overrideList.Contains(option);
		}
		return false;
	}

	public void AddOverride(SandboxOptions option)
	{
		if (overrideList == null)
		{
			overrideList = new List<SandboxOptions>();
		}
		if (!overrideList.Contains(option))
		{
			overrideList.Add(option);
		}
	}
}
