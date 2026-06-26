using System.Collections.Generic;
using UnityEngine;

namespace Twitch;

public class TwitchVote
{
	public enum VoteDisplayTypes
	{
		Single,
		GoodBad,
		Special,
		HordeBuffed
	}

	public static string PropTitleVarKey = "title_var_key";

	public static string PropDisplayVarKey = "display_var_key";

	public static string PropTitle = "title";

	public static string PropTitleKey = "title_key";

	public static string PropDescription = "description";

	public static string PropDescriptionKey = "description_key";

	public static string PropDisplay = "display";

	public static string PropDisplayKey = "display_key";

	public static string PropEventName = "event_name";

	public static string PropVoteType = "vote_type";

	public static string PropGroup = "group";

	public static string PropTitleFormatKey = "title_format_key";

	public static string PropDescriptionFormatKey = "description_format_key";

	public static string PropDisplayFormatKey = "display_format_key";

	public static string PropStartGameStage = "start_gamestage";

	public static string PropEndGameStage = "end_gamestage";

	public static string PropAllowedStartHour = "allowed_start_hour";

	public static string PropAllowedEndHour = "allowed_end_hour";

	public static string PropDisplayType = "display_type";

	public static string PropTitleColor = "title_color";

	public static string PropVoteLine1 = "line1_desc";

	public static string PropVoteLine1Key = "line1_desc_key";

	public static string PropVoteLine2 = "line2_desc";

	public static string PropVoteLine2Key = "line2_desc_key";

	public static string PropEnabled = "enabled";

	public static string PropMaxTimesPerDay = "max_times_per_day";

	public static string PropVoteTip = "vote_tip";

	public static string PropVoteTipKey = "vote_tip_key";

	public static string PropPresets = "presets";

	public static HashSet<string> ExtendsExcludes = new HashSet<string> { PropStartGameStage, PropEndGameStage };

	public string VoteName;

	public string Title;

	public string Description;

	public string Display = "";

	public string GameEvent;

	public string[] VoteTypes;

	public TwitchVoteType MainVoteType;

	public string Group = "";

	public bool Enabled = true;

	public bool OriginalEnabled;

	public string TitleColor = "";

	public int StartGameStage = -1;

	public int EndGameStage = -1;

	public int AllowedStartHour;

	public int AllowedEndHour = 24;

	public string VoteLine1 = "";

	public string VoteLine2 = "";

	public int MaxTimesPerDay = -1;

	public int CurrentDayCount;

	public float tempCooldownSet;

	public float tempCooldown;

	public string VoteTip = "";

	public DynamicProperties Properties;

	public List<TwitchActionCooldownAddition> CooldownAdditions;

	public List<BaseTwitchVoteRequirement> VoteRequirements;

	public VoteDisplayTypes DisplayType = VoteDisplayTypes.GoodBad;

	public List<string> PresetNames;

	public string VoteDescription
	{
		get
		{
			switch (DisplayType)
			{
			case VoteDisplayTypes.Single:
			case VoteDisplayTypes.Special:
				return Title;
			case VoteDisplayTypes.HordeBuffed:
				return Title + " (" + VoteLine1 + ")";
			case VoteDisplayTypes.GoodBad:
				return Title + " / " + VoteLine1;
			default:
				return "";
			}
		}
	}

	public string VoteHeight
	{
		get
		{
			VoteDisplayTypes displayType = DisplayType;
			if (displayType == VoteDisplayTypes.Single || displayType == VoteDisplayTypes.Special)
			{
				return "-50";
			}
			return "-90";
		}
	}

	public bool IsInPreset(TwitchVotePreset preset)
	{
		if (PresetNames != null || preset.IsEmpty)
		{
			if (PresetNames != null)
			{
				return PresetNames.Contains(preset.Name);
			}
			return false;
		}
		return true;
	}

	public bool CanUse(int hour, int gamestage, EntityPlayer player)
	{
		if ((StartGameStage == -1 || StartGameStage <= gamestage) && (EndGameStage == -1 || EndGameStage >= gamestage) && hour >= AllowedStartHour && hour <= AllowedEndHour && (MaxTimesPerDay == -1 || MaxTimesPerDay > CurrentDayCount))
		{
			if (tempCooldown > 0f && TwitchManager.Current.CurrentUnityTime - tempCooldownSet < tempCooldown)
			{
				return false;
			}
			tempCooldown = 0f;
			tempCooldownSet = 0f;
			if (VoteRequirements == null)
			{
				return true;
			}
			for (int i = 0; i < VoteRequirements.Count; i++)
			{
				if (!VoteRequirements[i].CanPerform(player))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public virtual void ParseProperties(DynamicProperties properties)
	{
		Properties = properties;
		string optionalValue = "";
		string optionalValue2 = "";
		properties.ParseLocalizedString(PropTitleVarKey, ref optionalValue);
		properties.ParseLocalizedString(PropDisplayVarKey, ref optionalValue2);
		if (optionalValue == "" && optionalValue2 != "")
		{
			optionalValue = optionalValue2;
		}
		else if (optionalValue != "" && optionalValue2 == "")
		{
			optionalValue2 = optionalValue;
		}
		properties.ParseLocalizedString(PropTitleKey, ref Title);
		if (Title == "")
		{
			properties.ParseString(PropTitle, ref Title);
		}
		string optionalValue3 = "";
		properties.ParseLocalizedString(PropTitleFormatKey, ref optionalValue3);
		if (optionalValue3 != "")
		{
			Title = string.Format(optionalValue3, optionalValue);
		}
		properties.ParseLocalizedString(PropDescriptionKey, ref Description);
		if (Description == "")
		{
			properties.ParseString(PropDescription, ref Description);
		}
		optionalValue3 = "";
		properties.ParseLocalizedString(PropDescriptionFormatKey, ref optionalValue3);
		if (optionalValue3 != "")
		{
			Description = string.Format(optionalValue3, optionalValue);
		}
		if (properties.Values.ContainsKey(PropDisplayKey))
		{
			Display = Localization.Get(properties.Values[PropDisplayKey]);
		}
		else
		{
			properties.ParseString(PropDisplay, ref Display);
		}
		optionalValue3 = "";
		properties.ParseLocalizedString(PropDisplayFormatKey, ref optionalValue3);
		if (optionalValue3 != "")
		{
			Display = string.Format(optionalValue3, optionalValue2);
		}
		properties.ParseString(PropEventName, ref GameEvent);
		properties.ParseString(PropGroup, ref Group);
		properties.ParseInt(PropStartGameStage, ref StartGameStage);
		properties.ParseInt(PropEndGameStage, ref EndGameStage);
		properties.ParseInt(PropAllowedStartHour, ref AllowedStartHour);
		properties.ParseInt(PropAllowedEndHour, ref AllowedEndHour);
		properties.ParseString(PropVoteLine1, ref VoteLine1);
		properties.ParseString(PropVoteLine2, ref VoteLine2);
		properties.ParseEnum(PropDisplayType, ref DisplayType);
		properties.ParseInt(PropMaxTimesPerDay, ref MaxTimesPerDay);
		string optionalValue4 = "";
		properties.ParseString(PropVoteType, ref optionalValue4);
		if (optionalValue4 != "")
		{
			VoteTypes = optionalValue4.Split(',');
			MainVoteType = TwitchManager.Current.VotingManager.GetVoteType(VoteTypes[0]);
		}
		properties.ParseBool(PropEnabled, ref Enabled);
		OriginalEnabled = Enabled;
		properties.ParseString(PropTitleColor, ref TitleColor);
		if (Display == "")
		{
			Display = Title;
		}
		Properties.ParseLocalizedString(PropVoteLine1Key, ref VoteLine1);
		Properties.ParseLocalizedString(PropVoteLine2Key, ref VoteLine2);
		VoteTip = Description;
		Properties.ParseString(PropVoteTip, ref VoteTip);
		Properties.ParseLocalizedString(PropVoteTipKey, ref VoteTip);
		if (properties.Values.ContainsKey(PropPresets))
		{
			PresetNames = new List<string>();
			PresetNames.AddRange(properties.Values[PropPresets].Split(','));
		}
		if (!GameEventManager.GameEventSequences.ContainsKey(GameEvent))
		{
			Debug.LogError($"TwitchVote: Game Event Sequence '{GameEvent}' does not exist!");
		}
	}

	public void AddCooldownAddition(TwitchActionCooldownAddition newCooldown)
	{
		if (CooldownAdditions == null)
		{
			CooldownAdditions = new List<TwitchActionCooldownAddition>();
		}
		CooldownAdditions.Add(newCooldown);
	}

	public void AddVoteRequirement(BaseTwitchVoteRequirement voteRequirement)
	{
		if (VoteRequirements == null)
		{
			VoteRequirements = new List<BaseTwitchVoteRequirement>();
		}
		VoteRequirements.Add(voteRequirement);
	}

	public void HandleVoteComplete()
	{
		if (CooldownAdditions == null)
		{
			return;
		}
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
}
