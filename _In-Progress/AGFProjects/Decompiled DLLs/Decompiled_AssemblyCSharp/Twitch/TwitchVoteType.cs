using System.Collections.Generic;

namespace Twitch;

public class TwitchVoteType
{
	public static string PropTitle = "title";

	public static string PropTitleKey = "title_key";

	public static string PropSpawnBlocked = "spawn_blocked";

	public static string PropExcludeTimeIndex = "exclude_time_index";

	public static string PropMaxTimesPerDay = "max_times_per_day";

	public static string PropAllowedStartHour = "allowed_start_hour";

	public static string PropAllowedEndHour = "allowed_end_hour";

	public static string PropBloodMoonDay = "blood_moon_day";

	public static string PropBloodMoonAllowed = "blood_moon_allowed";

	public static string PropGuaranteedGroups = "guaranteed_group";

	public static string PropCooldownOnEnd = "cooldown_on_end";

	public static string PropUseMystery = "use_mystery";

	public static string PropActionLockout = "action_lockout";

	public static string PropGroup = "group";

	public static string PropEnabled = "enabled";

	public static string PropVoteChoiceCount = "vote_choice_count";

	public static string PropAllowedWithActions = "allowed_with_actions";

	public static string PropIsBoss = "is_boss";

	public static string PropManualStart = "manual_start";

	public static string PropIcon = "icon";

	public static string PropPresets = "presets";

	public string Name;

	public string Title;

	public string Icon;

	public string Group;

	public bool SpawnBlocked = true;

	public bool BloodMoonDay = true;

	public bool BloodMoonAllowed = true;

	public bool CooldownOnEnd;

	public bool UseMystery;

	public bool ActionLockout;

	public bool AllowedWithActions = true;

	public int MaxTimesPerDay = -1;

	public int AllowedStartHour;

	public int AllowedEndHour = 24;

	public int VoteChoiceCount = 3;

	public int CurrentDayCount;

	public string GuaranteedGroup = "";

	public bool Enabled = true;

	public bool ManualStart;

	public bool IsBoss;

	public List<string> PresetNames;

	public bool IsInPreset(string preset)
	{
		if (PresetNames != null)
		{
			return PresetNames.Contains(preset);
		}
		return true;
	}

	public bool CanUse()
	{
		if (!Enabled)
		{
			return false;
		}
		if (MaxTimesPerDay != -1 && MaxTimesPerDay <= CurrentDayCount)
		{
			return false;
		}
		return true;
	}

	public virtual void ParseProperties(DynamicProperties properties)
	{
		properties.ParseString(PropTitle, ref Title);
		if (properties.Values.ContainsKey(PropTitleKey))
		{
			Title = Localization.Get(properties.Values[PropTitleKey]);
		}
		properties.ParseString(PropIcon, ref Icon);
		properties.ParseBool(PropSpawnBlocked, ref SpawnBlocked);
		properties.ParseInt(PropMaxTimesPerDay, ref MaxTimesPerDay);
		properties.ParseInt(PropAllowedStartHour, ref AllowedStartHour);
		properties.ParseInt(PropAllowedEndHour, ref AllowedEndHour);
		properties.ParseBool(PropBloodMoonDay, ref BloodMoonDay);
		properties.ParseBool(PropBloodMoonAllowed, ref BloodMoonAllowed);
		properties.ParseString(PropGuaranteedGroups, ref GuaranteedGroup);
		properties.ParseBool(PropCooldownOnEnd, ref CooldownOnEnd);
		properties.ParseBool(PropUseMystery, ref UseMystery);
		properties.ParseBool(PropActionLockout, ref ActionLockout);
		properties.ParseString(PropGroup, ref Group);
		properties.ParseBool(PropEnabled, ref Enabled);
		properties.ParseBool(PropAllowedWithActions, ref AllowedWithActions);
		properties.ParseInt(PropVoteChoiceCount, ref VoteChoiceCount);
		properties.ParseBool(PropIsBoss, ref IsBoss);
		properties.ParseBool(PropManualStart, ref ManualStart);
		if (properties.Values.ContainsKey(PropPresets))
		{
			PresetNames = new List<string>();
			PresetNames.AddRange(properties.Values[PropPresets].Split(','));
		}
	}
}
