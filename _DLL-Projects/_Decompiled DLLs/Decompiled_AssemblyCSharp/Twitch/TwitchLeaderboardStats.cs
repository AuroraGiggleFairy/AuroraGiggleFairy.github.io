using System.Collections.Generic;
using Challenges;
using UniLinq;

namespace Twitch;

public class TwitchLeaderboardStats
{
	public class StatEntry
	{
		public string Name;

		public string UserColor;

		public int Kills;

		public int GoodActions;

		public int BadActions;

		public int BitsUsed;

		public int CurrentGoodActions;

		public int CurrentActions;
	}

	public int LargestPimpPot;

	public int LargestBitPot;

	public int TotalGood;

	public int TotalBad;

	public int TotalActions;

	public int TotalBits;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_GoodReward;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ingameOutput_GoodReward;

	public StatEntry TopKillerViewer;

	public StatEntry TopGoodViewer;

	public StatEntry TopBadViewer;

	public StatEntry MostBitsSpentViewer;

	public StatEntry CurrentGoodViewer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CurrentGoodDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTime = 1f;

	public float nextGoodTime = -1f;

	public int GoodRewardAmount = 1000;

	[PublicizedFrom(EAccessModifier.Private)]
	public int goodRewardTime = 15;

	public Dictionary<string, StatEntry> StatEntries = new Dictionary<string, StatEntry>();

	public int GoodRewardTime
	{
		get
		{
			return goodRewardTime;
		}
		set
		{
			if (goodRewardTime != value)
			{
				goodRewardTime = value;
				nextGoodTime = goodRewardTime * 60;
			}
		}
	}

	public event OnLeaderboardStatsChanged StatsChanged;

	public event OnLeaderboardStatsChanged LeaderboardChanged;

	public void SetupLocalization()
	{
		chatOutput_GoodReward = Localization.Get("TwitchChat_GoodReward");
		ingameOutput_GoodReward = Localization.Get("TwitchInGame_GoodReward");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleStatsChanged()
	{
		if (this.StatsChanged != null)
		{
			this.StatsChanged();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLeaderboardChanged()
	{
		if (this.LeaderboardChanged != null)
		{
			this.LeaderboardChanged();
		}
	}

	public void UpdateStats(float deltaTime)
	{
		if (CurrentGoodDirty)
		{
			lastTime -= deltaTime;
			if (lastTime <= 0f)
			{
				List<StatEntry> list = (from entry in StatEntries.Values
					where entry.CurrentGoodActions > 0
					orderby entry.CurrentGoodActions descending
					select entry).ToList();
				if (list.Count > 0)
				{
					if (CurrentGoodViewer != list[0])
					{
						CurrentGoodViewer = list[0];
						HandleStatsChanged();
					}
				}
				else if (CurrentGoodViewer != null)
				{
					CurrentGoodViewer = null;
					HandleStatsChanged();
				}
				CurrentGoodDirty = false;
				lastTime = 1f;
			}
		}
		if (nextGoodTime == -1f)
		{
			nextGoodTime = GoodRewardTime * 60;
		}
		if (CurrentGoodViewer == null)
		{
			return;
		}
		nextGoodTime -= deltaTime;
		if (nextGoodTime <= 0f)
		{
			TwitchManager current = TwitchManager.Current;
			if (current.CurrentActionPreset.UseHelperReward && CurrentGoodViewer != null)
			{
				ViewerEntry viewerEntry = TwitchManager.Current.ViewerData.GetViewerEntry(CurrentGoodViewer.Name.ToLower());
				viewerEntry.StandardPoints += GoodRewardAmount;
				current.ircClient.SendChannelMessage(string.Format(chatOutput_GoodReward, CurrentGoodViewer.Name, viewerEntry.CombinedPoints, GoodRewardAmount, Localization.Get("TwitchPoints_PP")), useQueue: true);
				current.AddToInGameChatQueue(string.Format(ingameOutput_GoodReward, viewerEntry.UserColor, CurrentGoodViewer.Name, GoodRewardAmount, Localization.Get("TwitchPoints_PP")));
				current.LocalPlayer.PlayOneShot("twitch_top_helper");
				ClearAllCurrentGood();
				QuestEventManager.Current.TwitchEventReceived(TwitchObjectiveTypes.HelperReward, "");
			}
			nextGoodTime = GoodRewardTime * 60;
		}
	}

	public void CheckTopKiller(StatEntry newData)
	{
		if (TopKillerViewer == null || TopKillerViewer.Kills < newData.Kills)
		{
			TopKillerViewer = newData;
			HandleStatsChanged();
		}
		else if (TopKillerViewer == newData)
		{
			HandleStatsChanged();
		}
	}

	public void CheckTopGood(StatEntry newData)
	{
		if (TopGoodViewer == null || TopGoodViewer.GoodActions < newData.GoodActions)
		{
			TopGoodViewer = newData;
			HandleStatsChanged();
		}
		else if (TopGoodViewer == newData)
		{
			HandleStatsChanged();
		}
		HandleLeaderboardChanged();
		CurrentGoodDirty = true;
	}

	public void CheckTopBad(StatEntry newData)
	{
		if (TopBadViewer == null || TopBadViewer.BadActions < newData.BadActions)
		{
			TopBadViewer = newData;
			HandleStatsChanged();
		}
		else if (TopBadViewer == newData)
		{
			HandleStatsChanged();
		}
		HandleLeaderboardChanged();
		CurrentGoodDirty = true;
	}

	public void CheckMostBitsSpent(StatEntry newData)
	{
		if (MostBitsSpentViewer == null || MostBitsSpentViewer.BitsUsed < newData.BitsUsed)
		{
			MostBitsSpentViewer = newData;
			HandleStatsChanged();
		}
		else if (MostBitsSpentViewer == newData)
		{
			HandleStatsChanged();
		}
		HandleLeaderboardChanged();
	}

	public StatEntry AddKill(string name, string userColor)
	{
		if (!StatEntries.ContainsKey(name))
		{
			StatEntries.Add(name, new StatEntry());
		}
		StatEntry statEntry = StatEntries[name];
		statEntry.Name = name;
		statEntry.UserColor = userColor;
		statEntry.Kills++;
		return statEntry;
	}

	public StatEntry AddGoodActionUsed(string name, string userColor, bool isBits)
	{
		if (!StatEntries.ContainsKey(name))
		{
			StatEntries.Add(name, new StatEntry());
		}
		int num = ((!isBits) ? 1 : 2);
		StatEntry statEntry = StatEntries[name];
		statEntry.Name = name;
		statEntry.UserColor = userColor;
		statEntry.GoodActions += num;
		statEntry.CurrentGoodActions += num;
		statEntry.CurrentActions++;
		return statEntry;
	}

	public StatEntry AddBadActionUsed(string name, string userColor, bool isBits)
	{
		if (!StatEntries.ContainsKey(name))
		{
			StatEntries.Add(name, new StatEntry());
		}
		int num = ((!isBits) ? 1 : 2);
		StatEntry statEntry = StatEntries[name];
		statEntry.Name = name;
		statEntry.UserColor = userColor;
		statEntry.BadActions += num;
		statEntry.CurrentGoodActions -= num;
		statEntry.CurrentActions++;
		return statEntry;
	}

	public StatEntry AddBitsUsed(string name, string userColor, int amount)
	{
		if (!StatEntries.ContainsKey(name))
		{
			StatEntries.Add(name, new StatEntry());
		}
		StatEntry statEntry = StatEntries[name];
		statEntry.Name = name;
		statEntry.UserColor = userColor;
		statEntry.BitsUsed += amount;
		return statEntry;
	}

	public void ClearAllCurrentGood()
	{
		foreach (StatEntry value in StatEntries.Values)
		{
			value.CurrentGoodActions = 0;
			value.CurrentActions = 0;
		}
		CurrentGoodViewer = null;
		HandleStatsChanged();
		HandleLeaderboardChanged();
	}
}
