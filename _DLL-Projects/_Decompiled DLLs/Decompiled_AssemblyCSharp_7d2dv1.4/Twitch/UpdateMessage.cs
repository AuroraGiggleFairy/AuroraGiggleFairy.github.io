using System.Collections.Generic;

namespace Twitch;

public class UpdateMessage
{
	public string updateSignature;

	public string status;

	public int[] actionCooldowns;

	public Dictionary<string, int> bitBalances;

	public Dictionary<string, bool> hasChatted;

	public string ActionPresetKey = TwitchManager.Current.CurrentActionPreset.Name;

	public string VotePresetKey = TwitchManager.Current.CurrentVotePreset.Name;

	public string EventPresetKey = TwitchManager.Current.CurrentEventPreset.Name;

	public int Difficulty = GameStats.GetInt(EnumGameStats.GameDifficulty) + 1;

	public int DayMinutes = GamePrefs.GetInt(EnumGamePrefs.DayNightLength);

	public int PPRate = (int)TwitchManager.Current.ViewerData.PointRate;

	public bool IsModded = IsGameModded();

	public int GoodRewardTime = TwitchManager.LeaderboardStats.GoodRewardTime;

	public string ActionPresetValue = TwitchManager.Current.CurrentActionPreset.Title;

	public string VotePresetValue = TwitchManager.Current.CurrentVotePreset.Title;

	public string EventPresetValue = TwitchManager.Current.CurrentEventPreset.Title;

	public string TopKillerValue = TwitchManager.LeaderboardStats.TopKillerViewer?.Name ?? "--";

	public string TopGoodValue = TwitchManager.LeaderboardStats.TopGoodViewer?.Name ?? "--";

	public string TopBadValue = TwitchManager.LeaderboardStats.TopBadViewer?.Name ?? "--";

	public string BestHelperValue = TwitchManager.LeaderboardStats.CurrentGoodViewer?.Name ?? "--";

	public string TotalGoodActionsValue = TwitchManager.LeaderboardStats.TotalGood.ToString();

	public string TotalBadActionsValue = TwitchManager.LeaderboardStats.TotalBad.ToString();

	public string LargestPimpPotValue = TwitchManager.LeaderboardStats.LargestPimpPot.ToString();

	public string DifficultyValue = DifficultyValueLocalized();

	public string DayCycleValue = string.Format(Localization.Get("goMinutes"), GamePrefs.GetInt(EnumGamePrefs.DayNightLength));

	public string PPRateValue = GetLocalizedPPRateValue();

	public string ModdedValue = ModdedValueLocalized();

	[PublicizedFrom(EAccessModifier.Private)]
	public static string DifficultyValueLocalized()
	{
		int num = GameStats.GetInt(EnumGameStats.GameDifficulty) + 1;
		return Localization.Get($"goDifficulty{num}" + ((num == 2) ? "_nodefault" : ""));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool IsGameModded()
	{
		return (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo)?.GetValue(GameInfoBool.ModdedConfig) ?? false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string ModdedValueLocalized()
	{
		GameServerInfo gameServerInfo = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
		if (gameServerInfo != null)
		{
			if (!gameServerInfo.GetValue(GameInfoBool.ModdedConfig))
			{
				return Localization.Get("xuiComboYesNoOff");
			}
			return Localization.Get("xuiComboYesNoOn");
		}
		return "--";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetLocalizedPPRateValue()
	{
		if (TwitchManager.Current.ViewerData.PointRate == 1f)
		{
			return Localization.Get("xuiTwitchPointGenerationStandard");
		}
		if (TwitchManager.Current.ViewerData.PointRate == 2f)
		{
			return Localization.Get("xuiTwitchPointGenerationDouble");
		}
		if (TwitchManager.Current.ViewerData.PointRate == 3f)
		{
			return Localization.Get("xuiTwitchPointGenerationTriple");
		}
		if (TwitchManager.Current.ViewerData.PointRate == 0f)
		{
			return Localization.Get("goDisabled");
		}
		return Localization.Get("xuiTwitchPointGenerationStandard");
	}
}
