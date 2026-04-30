using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Twitch;

public class TwitchAction
{
	public enum SpecialRequirements
	{
		None,
		HasSpawnedEntities,
		NoSpawnedEntities,
		Bloodmoon,
		NotBloodmoon,
		NotBloodmoonDay,
		EarlyDay,
		Daytime,
		Night,
		IsCooldown,
		InLandClaim,
		NotInLandClaim,
		Safe,
		NotSafe,
		NoFullProgression,
		NotOnVehicle,
		NotInTrader,
		Encumbrance,
		WeatherGracePeriod,
		NotOnQuest,
		OnQuest
	}

	public enum OnlyUsableTypes
	{
		Everyone,
		Broadcaster,
		Mods,
		VIPs,
		Subs,
		Name
	}

	public enum PointTypes
	{
		PP,
		SP,
		Bits
	}

	public enum RespawnCountTypes
	{
		None,
		Both,
		BlocksOnly,
		SpawnsOnly
	}

	public string Name = "";

	public string Title = "";

	public string BaseCommand = "";

	public string Command = "";

	public string EventName = "";

	public List<string> CategoryNames = new List<string>();

	public int DefaultCost = 5;

	public int ModifiedCost = 5;

	public int CurrentCost = 5;

	public int StartGameStage;

	public string Description;

	public float OriginalCooldown;

	public float Cooldown;

	public bool IsPositive;

	public bool AddsToCooldown;

	public int CooldownAddAmount = -1;

	public bool CooldownBlocked;

	public bool WaitingBlocked;

	public bool OnCooldown;

	public bool Enabled = true;

	public bool SingleDayUse;

	public bool DelayNotify;

	public bool TwitchNotify = true;

	public bool PlayBitSound = true;

	public string RandomGroup = "";

	public string Replaces = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastUse;

	public int AllowedDay = -1;

	public int groupIndex;

	public float tempCooldownSet;

	public float tempCooldown;

	public bool IgnoreCooldown;

	public bool IgnoreDiscount;

	public float ModifiedCooldown;

	public List<TwitchActionCooldownAddition> CooldownAdditions;

	public bool ShowInActionList = true;

	public List<SpecialRequirements> SpecialRequirementList;

	public bool HideOnDisable;

	public OnlyUsableTypes OnlyUsableByType;

	public string[] OnlyUsableBy;

	public bool OriginalEnabled = true;

	public PointTypes PointType;

	public TwitchActionManager.ActionCategory MainCategory;

	public TwitchActionManager.ActionCategory DisplayCategory;

	public List<string> PresetNames;

	public bool OnlyShowInPreset;

	public float VoteCooldownAddition;

	public bool StreamerOnly;

	public RespawnCountTypes RespawnCountType;

	public int MinRespawnCount = -1;

	public int MaxRespawnCount = -1;

	public int RespawnThreshold;

	public DynamicProperties Properties;

	public static string PropCommand = "command";

	public static string PropCommandKey = "command_key";

	public static string PropTitle = "title";

	public static string PropTitleKey = "title_key";

	public static string PropCategory = "category";

	public static string PropDisplayCategory = "display_category";

	public static string PropEventName = "event_name";

	public static string PropDefaultCost = "default_cost";

	public static string PropDescription = "description";

	public static string PropDescriptionKey = "description_key";

	public static string PropStartGameStage = "start_gamestage";

	public static string PropCooldown = "cooldown";

	public static string PropIsPositive = "is_positive";

	public static string PropSpecialOnly = "special_only";

	public static string PropAddCooldown = "add_cooldown";

	public static string PropCooldownAddAmount = "cooldown_add_amount";

	public static string PropCooldownBlocked = "cooldown_blocked";

	public static string PropWaitingBlocked = "waiting_blocked";

	public static string PropEnabled = "enabled";

	public static string PropSingleDayUse = "single_day";

	public static string PropRandomGroup = "random_group";

	public static string PropReplaces = "replaces";

	public static string PropShowInActionList = "show_in_action_list";

	public static string PropPlayBitSound = "play_bit_sound";

	public static string PropSpecialRequirement = "special_requirement";

	public static string PropTwitchNotify = "twitch_notify";

	public static string PropDelayNotify = "delay_notify";

	public static string PropHideOnDisable = "hide_on_disable";

	public static string PropOnlyUsableByType = "only_usable_type";

	public static string PropOnlyUsableBy = "only_usable_by";

	public static string PropIgnoreCooldown = "ignore_cooldown";

	public static string PropIgnoreDiscount = "ignore_discount";

	public static string PropPointTypes = "point_type";

	public static string PropPresets = "presets";

	public static string PropOnlyShowInPreset = "only_show_in_preset";

	public static string PropVoteCooldownAddition = "vote_cooldown_add";

	public static string PropStreamerOnly = "streamer_only";

	public static string PropMinRespawnCount = "min_respawn_count";

	public static string PropMaxRespawnCount = "max_respawn_count";

	public static string PropRespawnCountType = "respawn_count_type";

	public static string PropRespawnThreshold = "respawn_threshold";

	public static HashSet<string> ExtendsExcludes = new HashSet<string> { PropShowInActionList, PropCommandKey, PropCommand, PropTitleKey, PropDescriptionKey };

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<int> bitPrices = new List<int>
	{
		10, 15, 20, 25, 30, 35, 40, 45, 50, 55,
		60, 65, 70, 75, 80, 85, 90, 95, 100, 105,
		110, 115, 120, 125, 130, 135, 140, 145, 150, 155,
		160, 165, 170, 175, 180, 185, 190, 195, 200, 250,
		300, 350, 400, 450, 500, 550, 600, 650, 700, 750,
		800, 850, 900, 950, 1000, 1100, 1200, 1250, 1300, 1400,
		1500, 1600, 1700, 1750, 1800, 1900, 2000, 2250, 2500, 2750,
		3000, 3500, 4000, 4500, 5000, 5500, 6000, 6500, 7000, 7500,
		8000, 8500, 9000, 9500, 10000
	};

	public bool CanUse => Enabled;

	public bool RandomDaily => RandomGroup != "";

	public bool SpecialOnly => PointType == PointTypes.SP;

	public bool HasModifiedPrice()
	{
		return DefaultCost != ModifiedCost;
	}

	public bool HasExtraConditions()
	{
		if ((SingleDayUse || RandomDaily) && AllowedDay == -1)
		{
			return false;
		}
		if (OnCooldown || OnlyUsableByType != OnlyUsableTypes.Everyone)
		{
			return false;
		}
		return true;
	}

	public static int GetAdjustedBitPriceCeil(int price)
	{
		int num = bitPrices.Find([PublicizedFrom(EAccessModifier.Internal)] (int p) => p >= price);
		if (num == 0)
		{
			return bitPrices[bitPrices.Count - 1];
		}
		return num;
	}

	public static int GetAdjustedBitPriceFloor(int price)
	{
		return bitPrices.FindLast([PublicizedFrom(EAccessModifier.Internal)] (int p) => p <= price);
	}

	public static int GetAdjustedBitPriceFloorNoZero(int price)
	{
		return Mathf.Max(GetAdjustedBitPriceFloor(price), bitPrices[0]);
	}

	public int GetModifiedDiscountCost()
	{
		return GetAdjustedBitPriceFloorNoZero((int)((float)ModifiedCost * TwitchManager.Current.BitPriceMultiplier));
	}

	public void DecreaseCost()
	{
		if (PointType == PointTypes.Bits)
		{
			ModifiedCost = bitPrices[(int)MathUtils.Clamp(bitPrices.IndexOf(ModifiedCost) - 1, 0f, bitPrices.Count - 1)];
		}
		else if (ModifiedCost > 25)
		{
			ModifiedCost -= 25;
		}
	}

	public void IncreaseCost()
	{
		if (PointType == PointTypes.Bits)
		{
			ModifiedCost = bitPrices[(int)MathUtils.Clamp(bitPrices.IndexOf(ModifiedCost) + 1, 0f, bitPrices.Count - 1)];
		}
		else if (ModifiedCost < 2000)
		{
			ModifiedCost += 25;
		}
	}

	public void ResetToDefaultCost()
	{
		if (PointType == PointTypes.Bits)
		{
			ModifiedCost = GetAdjustedBitPriceFloorNoZero(DefaultCost);
		}
		else
		{
			ModifiedCost = DefaultCost;
		}
	}

	public bool CheckUsable(TwitchIRCClient.TwitchChatMessage message)
	{
		return OnlyUsableByType switch
		{
			OnlyUsableTypes.Broadcaster => message.isBroadcaster, 
			OnlyUsableTypes.Mods => message.isMod, 
			OnlyUsableTypes.VIPs => message.isVIP, 
			OnlyUsableTypes.Name => OnlyUsableBy.ContainsCaseInsensitive(message.UserName), 
			OnlyUsableTypes.Subs => message.isSub, 
			_ => true, 
		};
	}

	public void Init()
	{
		if (CategoryNames.Count > 0)
		{
			MainCategory = TwitchActionManager.Current.GetCategory(CategoryNames[CategoryNames.Count - 1]);
			if (DisplayCategory == null)
			{
				DisplayCategory = MainCategory;
			}
		}
		OnInit();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnInit()
	{
	}

	public virtual TwitchActionEntry SetupActionEntry()
	{
		return new TwitchActionEntry();
	}

	public bool IsInPreset(TwitchActionPreset preset)
	{
		if ((!preset.IsEmpty && PresetNames != null && PresetNames.Contains(preset.Name)) || preset.AddedActions.Contains(Name))
		{
			return !preset.RemovedActions.Contains(Name);
		}
		return false;
	}

	public bool IsInPresetForList(TwitchActionPreset preset)
	{
		if (preset.IsEmpty || PresetNames == null || !PresetNames.Contains(preset.Name))
		{
			return preset.AddedActions.Contains(Name);
		}
		return true;
	}

	public bool IsInPresetDefault(TwitchActionPreset preset)
	{
		if (!preset.IsEmpty)
		{
			if (PresetNames != null)
			{
				return PresetNames.Contains(preset.Name);
			}
			return false;
		}
		return false;
	}

	public bool CanPerformAction(EntityPlayer target, TwitchActionEntry entry)
	{
		if (entry.Target == null)
		{
			entry.Target = target;
		}
		if (OnPerformAction(target, entry))
		{
			return true;
		}
		return false;
	}

	public void SetQueued()
	{
		lastUse = Time.time;
		if (CooldownAdditions != null)
		{
			float actionCooldownModifier = TwitchManager.Current.ActionCooldownModifier;
			for (int i = 0; i < CooldownAdditions.Count; i++)
			{
				TwitchActionCooldownAddition twitchActionCooldownAddition = CooldownAdditions[i];
				if (twitchActionCooldownAddition.IsAction && TwitchActionManager.TwitchActions.ContainsKey(twitchActionCooldownAddition.ActionName))
				{
					TwitchAction twitchAction = TwitchActionManager.TwitchActions[twitchActionCooldownAddition.ActionName];
					twitchAction.tempCooldown = twitchActionCooldownAddition.CooldownTime * actionCooldownModifier;
					twitchAction.tempCooldownSet = Time.time;
				}
				else if (!twitchActionCooldownAddition.IsAction && TwitchActionManager.TwitchVotes.ContainsKey(twitchActionCooldownAddition.ActionName))
				{
					TwitchVote twitchVote = TwitchActionManager.TwitchVotes[twitchActionCooldownAddition.ActionName];
					twitchVote.tempCooldown = twitchActionCooldownAddition.CooldownTime * actionCooldownModifier;
					twitchVote.tempCooldownSet = Time.time;
				}
			}
		}
		if (VoteCooldownAddition != 0f)
		{
			TwitchVotingManager votingManager = TwitchManager.Current.VotingManager;
			votingManager.VoteStartDelayTimeRemaining = Math.Max(VoteCooldownAddition, votingManager.VoteStartDelayTimeRemaining);
		}
		if (SingleDayUse)
		{
			AllowedDay = -1;
		}
	}

	public virtual bool ParseProperties(DynamicProperties properties)
	{
		Properties = properties;
		properties.ParseString(PropTitle, ref Title);
		if (properties.Values.ContainsKey(PropTitleKey))
		{
			Title = Localization.Get(properties.Values[PropTitleKey]);
		}
		if (properties.Values.ContainsKey(PropCommand))
		{
			BaseCommand = (Command = properties.Values[PropCommand].ToLower());
			if (!Regex.IsMatch(Command, "^#[a-zA-Z0-9]+(_[a-zA-Z0-9]+)*$"))
			{
				return false;
			}
		}
		if (properties.Values.ContainsKey(PropCommandKey))
		{
			Command = Localization.Get(properties.Values[PropCommandKey]).ToLower();
			if (Localization.LocalizationChecks)
			{
				if (Command.StartsWith("l_"))
				{
					Command = Command.Substring(3).Insert(0, "#l_");
				}
				else if (Command.StartsWith("ul_"))
				{
					Command = Command.Substring(3).Insert(0, "#ul_");
				}
				else if (Command.StartsWith("le_"))
				{
					Command = Command.Substring(3).Insert(0, "#le_");
				}
			}
			if (!Regex.IsMatch(Command, "^#[\\p{L}\\p{N}]+([-_][\\p{L}\\p{N}]+)*$"))
			{
				return false;
			}
		}
		if (properties.Values.ContainsKey(PropCategory))
		{
			CategoryNames.AddRange(properties.Values[PropCategory].Split(','));
		}
		if (properties.Values.ContainsKey(PropDisplayCategory))
		{
			DisplayCategory = TwitchActionManager.Current.GetCategory(properties.Values[PropDisplayCategory]);
		}
		properties.ParseString(PropEventName, ref EventName);
		properties.ParseString(PropDescription, ref Description);
		if (properties.Values.ContainsKey(PropDescriptionKey))
		{
			Description = Localization.Get(properties.Values[PropDescriptionKey]);
		}
		properties.ParseInt(PropDefaultCost, ref DefaultCost);
		properties.ParseInt(PropStartGameStage, ref StartGameStage);
		properties.ParseBool(PropIsPositive, ref IsPositive);
		bool optionalValue = false;
		properties.ParseBool(PropSpecialOnly, ref optionalValue);
		if (optionalValue)
		{
			PointType = PointTypes.SP;
		}
		properties.ParseBool(PropAddCooldown, ref AddsToCooldown);
		properties.ParseInt(PropCooldownAddAmount, ref CooldownAddAmount);
		properties.ParseBool(PropCooldownBlocked, ref CooldownBlocked);
		properties.ParseBool(PropWaitingBlocked, ref WaitingBlocked);
		if (properties.Values.ContainsKey(PropCooldown))
		{
			OriginalCooldown = (Cooldown = StringParsers.ParseFloat(properties.Values[PropCooldown]));
			lastUse = Time.time - Cooldown;
			ModifiedCooldown = Cooldown;
		}
		properties.ParseBool(PropEnabled, ref Enabled);
		OriginalEnabled = Enabled;
		properties.ParseBool(PropSingleDayUse, ref SingleDayUse);
		properties.ParseString(PropRandomGroup, ref RandomGroup);
		properties.ParseBool(PropShowInActionList, ref ShowInActionList);
		if (properties.Values.ContainsKey(PropSpecialRequirement))
		{
			string[] array = Properties.Values[PropSpecialRequirement].Split(',');
			SpecialRequirementList = new List<SpecialRequirements>();
			string[] array2 = array;
			foreach (string text in array2)
			{
				SpecialRequirements result = SpecialRequirements.None;
				if (Enum.TryParse<SpecialRequirements>(text, ignoreCase: true, out result))
				{
					SpecialRequirementList.Add(result);
				}
				else
				{
					Log.Error("TwitchAction " + Title + " has unknown ShapeCategory " + text);
				}
			}
		}
		properties.ParseString(PropReplaces, ref Replaces);
		properties.ParseBool(PropTwitchNotify, ref TwitchNotify);
		properties.ParseBool(PropDelayNotify, ref DelayNotify);
		properties.ParseBool(PropPlayBitSound, ref PlayBitSound);
		properties.ParseBool(PropHideOnDisable, ref HideOnDisable);
		properties.ParseBool(PropIgnoreCooldown, ref IgnoreCooldown);
		properties.ParseBool(PropIgnoreDiscount, ref IgnoreDiscount);
		properties.ParseBool(PropStreamerOnly, ref StreamerOnly);
		properties.ParseEnum(PropOnlyUsableByType, ref OnlyUsableByType);
		string optionalValue2 = "";
		properties.ParseString(PropOnlyUsableBy, ref optionalValue2);
		if (optionalValue2 != "")
		{
			OnlyUsableBy = optionalValue2.Split(',');
		}
		properties.ParseEnum(PropPointTypes, ref PointType);
		ResetToDefaultCost();
		UpdateCost();
		if (CooldownAddAmount == -1)
		{
			CooldownAddAmount = DefaultCost;
		}
		properties.ParseFloat(PropVoteCooldownAddition, ref VoteCooldownAddition);
		if (properties.Values.ContainsKey(PropPresets))
		{
			PresetNames = new List<string>();
			PresetNames.AddRange(properties.Values[PropPresets].Split(','));
		}
		properties.ParseBool(PropOnlyShowInPreset, ref OnlyShowInPreset);
		if (properties.Contains(PropMinRespawnCount) || properties.Contains(PropMaxRespawnCount))
		{
			properties.ParseInt(PropMinRespawnCount, ref MinRespawnCount);
			properties.ParseInt(PropMaxRespawnCount, ref MaxRespawnCount);
			RespawnCountType = RespawnCountTypes.Both;
		}
		else
		{
			RespawnCountType = RespawnCountTypes.None;
		}
		properties.ParseEnum(PropRespawnCountType, ref RespawnCountType);
		properties.ParseInt(PropRespawnThreshold, ref RespawnThreshold);
		return true;
	}

	public bool UpdateCost(float bitPriceModifier = 1f)
	{
		int currentCost = CurrentCost;
		if (PointType == PointTypes.Bits)
		{
			if (!IgnoreDiscount && bitPriceModifier != 1f)
			{
				CurrentCost = GetAdjustedBitPriceFloorNoZero((int)((float)ModifiedCost * bitPriceModifier));
			}
			else
			{
				CurrentCost = GetAdjustedBitPriceFloorNoZero(ModifiedCost);
			}
		}
		else
		{
			CurrentCost = ModifiedCost;
		}
		return currentCost != CurrentCost;
	}

	public virtual bool IsReady(TwitchManager twitchManager)
	{
		if (SpecialRequirementList != null)
		{
			for (int i = 0; i < SpecialRequirementList.Count; i++)
			{
				switch (SpecialRequirementList[i])
				{
				case SpecialRequirements.HasSpawnedEntities:
					if (twitchManager.actionSpawnLiveList.Count == 0)
					{
						return false;
					}
					break;
				case SpecialRequirements.NoSpawnedEntities:
					if (twitchManager.actionSpawnLiveList.Count > 0)
					{
						return false;
					}
					break;
				case SpecialRequirements.Bloodmoon:
					if (!twitchManager.isBMActive)
					{
						return false;
					}
					break;
				case SpecialRequirements.NotBloodmoon:
					if (twitchManager.isBMActive)
					{
						return false;
					}
					break;
				case SpecialRequirements.NotBloodmoonDay:
					if (SkyManager.IsBloodMoonVisible())
					{
						return false;
					}
					if (GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime) == GameStats.GetInt(EnumGameStats.BloodMoonDay))
					{
						return false;
					}
					break;
				case SpecialRequirements.EarlyDay:
				{
					int num2 = GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);
					if ((float)num2 > SkyManager.GetDuskTime() - 5f || (float)num2 < SkyManager.GetDawnTime())
					{
						return false;
					}
					break;
				}
				case SpecialRequirements.Daytime:
				{
					int num = GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);
					if ((float)num > SkyManager.GetDuskTime() || (float)num < SkyManager.GetDawnTime())
					{
						return false;
					}
					break;
				}
				case SpecialRequirements.Night:
					GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);
					if (!SkyManager.IsDark())
					{
						return false;
					}
					break;
				case SpecialRequirements.IsCooldown:
					if (!TwitchManager.HasInstance || !twitchManager.IsReady)
					{
						return false;
					}
					if (twitchManager.CurrentCooldownPreset.CooldownType != CooldownPreset.CooldownTypes.Fill)
					{
						return false;
					}
					if (twitchManager.CooldownType != TwitchManager.CooldownTypes.MaxReached)
					{
						return false;
					}
					break;
				case SpecialRequirements.InLandClaim:
					if (!twitchManager.LocalPlayerInLandClaim)
					{
						return false;
					}
					break;
				case SpecialRequirements.NotInLandClaim:
					if (twitchManager.LocalPlayerInLandClaim)
					{
						return false;
					}
					break;
				case SpecialRequirements.Safe:
					if (!twitchManager.LocalPlayer.TwitchSafe)
					{
						return false;
					}
					break;
				case SpecialRequirements.NotSafe:
					if (twitchManager.LocalPlayer.TwitchSafe)
					{
						return false;
					}
					break;
				case SpecialRequirements.NotInTrader:
					if (twitchManager.LocalPlayer.IsInTrader)
					{
						return false;
					}
					break;
				case SpecialRequirements.NoFullProgression:
					if (!twitchManager.IsReady)
					{
						return false;
					}
					if (!twitchManager.UseProgression || twitchManager.OverrideProgession)
					{
						return false;
					}
					break;
				case SpecialRequirements.NotOnVehicle:
					if (!twitchManager.IsReady)
					{
						return false;
					}
					if (twitchManager.LocalPlayer.AttachedToEntity != null)
					{
						return false;
					}
					break;
				case SpecialRequirements.Encumbrance:
					if ((int)EffectManager.GetValue(PassiveEffects.CarryCapacity, null, 0f, twitchManager.LocalPlayer) <= 30)
					{
						return false;
					}
					break;
				case SpecialRequirements.WeatherGracePeriod:
					if (GameManager.Instance.World.GetWorldTime() <= 30000)
					{
						return false;
					}
					break;
				case SpecialRequirements.NotOnQuest:
					if (QuestEventManager.Current.QuestBounds.width != 0f)
					{
						return false;
					}
					break;
				case SpecialRequirements.OnQuest:
					if (QuestEventManager.Current.QuestBounds.width == 0f)
					{
						return false;
					}
					break;
				}
			}
		}
		bool flag = (!SingleDayUse && !RandomDaily) || AllowedDay == GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
		if (tempCooldown > 0f && twitchManager.CurrentUnityTime - tempCooldownSet < tempCooldown)
		{
			return false;
		}
		tempCooldown = 0f;
		tempCooldownSet = 0f;
		return twitchManager.CurrentUnityTime - lastUse >= ModifiedCooldown && flag;
	}

	public void UpdateModifiedCooldown(float modifier)
	{
		ModifiedCooldown = modifier * Cooldown;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnPerformAction(EntityPlayer Target, TwitchActionEntry entry)
	{
		return false;
	}

	public void AddCooldownAddition(TwitchActionCooldownAddition newCooldown)
	{
		if (CooldownAdditions == null)
		{
			CooldownAdditions = new List<TwitchActionCooldownAddition>();
		}
		CooldownAdditions.Add(newCooldown);
	}

	public void ResetCooldown(float currentUnityTime)
	{
		lastUse = currentUnityTime - ModifiedCooldown;
		tempCooldown = 0f;
		tempCooldownSet = 0f;
	}

	public void SetCooldown(float currentUnityTime, float newCooldownTime)
	{
		lastUse = currentUnityTime - ModifiedCooldown;
		tempCooldown = newCooldownTime;
		tempCooldownSet = currentUnityTime;
	}
}
