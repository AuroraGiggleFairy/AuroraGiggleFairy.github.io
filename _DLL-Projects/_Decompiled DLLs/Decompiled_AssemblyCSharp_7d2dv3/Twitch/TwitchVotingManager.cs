using System.Collections.Generic;
using Audio;
using Challenges;
using UniLinq;
using UnityEngine;

namespace Twitch;

public class TwitchVotingManager
{
	public enum VoteStateTypes
	{
		Init,
		WaitingForNextVote,
		ReadyForVoteStart,
		RequestedVoteStart,
		VoteReady,
		VoteStarted,
		VoteFinished,
		WaitingForActive,
		EventActive
	}

	public enum BossVoteSettings
	{
		Disabled,
		Standard,
		Daily
	}

	public class DailyVoteEntry
	{
		public ulong VoteStartTime;

		public ulong VoteEndTime;

		public int LastVoteDay;

		public int Index = -1;
	}

	public class VoteDayTimeRange
	{
		public string Name;

		public int StartHour;

		public int EndHour;
	}

	public TwitchManager Owner;

	public VoteStateTypes CurrentVoteState;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_VoteStarted;

	[PublicizedFrom(EAccessModifier.Private)]
	public string chatOutput_VoteFinished;

	[PublicizedFrom(EAccessModifier.Private)]
	public string dayTimeRangeOutput;

	[PublicizedFrom(EAccessModifier.Private)]
	public string VoteOptionA;

	[PublicizedFrom(EAccessModifier.Private)]
	public string VoteOptionB;

	[PublicizedFrom(EAccessModifier.Private)]
	public string VoteOptionC;

	[PublicizedFrom(EAccessModifier.Private)]
	public string VoteOptionD;

	[PublicizedFrom(EAccessModifier.Private)]
	public string VoteOptionE;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxDailyVotes = 4;

	public int lastGameDay = 1;

	public bool WinnerShowing;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentVoteDayTimeRange = 2;

	public List<VoteDayTimeRange> VoteDayTimeRanges = new List<VoteDayTimeRange>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<DailyVoteEntry> DailyVoteTimes = new List<DailyVoteEntry>();

	public bool AllowVotesDuringBloodmoon;

	public bool AllowVotesDuringQuests;

	public bool AllowVotesInSafeZone;

	public List<TwitchVoteType> NextVotes = new List<TwitchVoteType>();

	public bool VoteInProgress;

	public float VoteTime = 60f;

	public int ViewerDefeatReward = 250;

	public float VoteStartDelayTimeRemaining;

	public float VoteEventTimeRemaining;

	public float VoteTimeRemaining;

	public bool UIDirty;

	public bool VoteEventComplete;

	public List<TwitchVoteEntry> voteList = new List<TwitchVoteEntry>();

	public List<string> voterlist = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchVoteEntry> tempVoteList = new List<TwitchVoteEntry>();

	public OnGameEventVoteAction VoteStarted;

	public OnGameEventVoteAction VoteEventStarted;

	public OnGameEventVoteAction VoteEventEnded;

	public Dictionary<string, TwitchVoteType> VoteTypes = new Dictionary<string, TwitchVoteType>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int voteGroupIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchVoteGroup> VoteGroups = new List<TwitchVoteGroup>();

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchVoteType QueuedVoteType;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchVote> tempSortList = new List<TwitchVote>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> tempVoteGroupList = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int day = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hour = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool readyForVote;

	public TwitchVoteEntry CurrentEvent;

	public int MaxDailyVotes
	{
		get
		{
			return maxDailyVotes;
		}
		set
		{
			if (maxDailyVotes != value)
			{
				maxDailyVotes = value;
				lastGameDay = 1;
			}
		}
	}

	public int CurrentVoteDayTimeRange
	{
		get
		{
			return currentVoteDayTimeRange;
		}
		set
		{
			if (currentVoteDayTimeRange != value)
			{
				currentVoteDayTimeRange = value;
				lastGameDay = 1;
			}
		}
	}

	public bool VotingEnabled => !Owner.CurrentVotePreset.IsEmpty;

	public bool VotingIsActive
	{
		get
		{
			if (VotingEnabled && CurrentVoteState != VoteStateTypes.WaitingForNextVote && CurrentVoteState != VoteStateTypes.Init && CurrentVoteState != VoteStateTypes.ReadyForVoteStart && CurrentVoteState != VoteStateTypes.RequestedVoteStart)
			{
				return CurrentVoteState != VoteStateTypes.VoteReady;
			}
			return false;
		}
	}

	public int VoteCount
	{
		get
		{
			if (voterlist == null)
			{
				return 0;
			}
			return voterlist.Count;
		}
	}

	public string VoteTypeText => CurrentVoteType.Title;

	public string VoteTip
	{
		get
		{
			if (CurrentEvent != null)
			{
				return CurrentEvent.VoteClass.VoteTip;
			}
			return "";
		}
	}

	public string VoteOffset
	{
		get
		{
			if (CurrentEvent != null)
			{
				return CurrentEvent.VoteClass.VoteHeight;
			}
			return "0";
		}
	}

	public bool UseMystery => CurrentVoteType.UseMystery;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int NeededLines
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public TwitchVoteType CurrentVoteType
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public TwitchVotingManager(TwitchManager owner)
	{
		Owner = owner;
		SetupVoteDayTimeRanges();
	}

	public void CleanupData()
	{
		VoteTypes.Clear();
		VoteGroups.Clear();
	}

	public void SetupLocalization()
	{
		chatOutput_VoteStarted = Localization.Get("TwitchChat_VoteStarted");
		chatOutput_VoteFinished = Localization.Get("TwitchChat_VoteFinished");
		dayTimeRangeOutput = Localization.Get("xuiOptionsTwitchVoteDayTimeRangeDisplay");
		VoteOptionA = Localization.Get("TwitchVoteOption_A");
		VoteOptionB = Localization.Get("TwitchVoteOption_B");
		VoteOptionC = Localization.Get("TwitchVoteOption_C");
		VoteOptionD = Localization.Get("TwitchVoteOption_D");
		VoteOptionE = Localization.Get("TwitchVoteOption_E");
	}

	public void AddVoteType(TwitchVoteType voteType)
	{
		VoteTypes.Add(voteType.Name, voteType);
		for (int i = 0; i < VoteGroups.Count; i++)
		{
			if (VoteGroups[i].Name == voteType.Group)
			{
				VoteGroups[i].VoteTypes.Add(voteType);
				return;
			}
		}
		TwitchVoteGroup twitchVoteGroup = new TwitchVoteGroup(voteType.Group);
		twitchVoteGroup.VoteTypes.Add(voteType);
		VoteGroups.Add(twitchVoteGroup);
	}

	public TwitchVoteType GetVoteType(string voteTypeName)
	{
		if (VoteTypes.ContainsKey(voteTypeName))
		{
			return VoteTypes[voteTypeName];
		}
		return null;
	}

	public void AddVote(int index, string userName)
	{
		if (!voterlist.Contains(userName) && voteList.Count > index)
		{
			voteList[index].VoteCount++;
			Manager.PlayInsidePlayerHead("twitch_vote_received");
			voterlist.Add(userName);
			UIDirty = true;
			for (int i = 0; i < voteList.Count; i++)
			{
				voteList[i].UIDirty = true;
			}
		}
	}

	public void ClearVotes()
	{
		for (int i = 0; i < voteList.Count; i++)
		{
			voteList[i].VoteCount = 0;
			voteList[i].VoterNames.Clear();
		}
		voterlist.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalculateVoteTimes()
	{
		DailyVoteTimes.Clear();
		GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
		VoteDayTimeRange voteDayTimeRange = VoteDayTimeRanges[CurrentVoteDayTimeRange];
		float num = (float)(voteDayTimeRange.EndHour - voteDayTimeRange.StartHour) / (float)MaxDailyVotes;
		float num2 = voteDayTimeRange.StartHour;
		for (int i = 0; i < MaxDailyVotes; i++)
		{
			float num3 = -1f;
			if (i == 0)
			{
				gameRandom.RandomRange(0f, num);
			}
			else
			{
				gameRandom.RandomRange(num - 1f, num);
			}
			int num4 = (int)(num2 + num3);
			int minutes = gameRandom.RandomRange(0, 59);
			DailyVoteEntry dailyVoteEntry = new DailyVoteEntry();
			dailyVoteEntry.VoteStartTime = GameUtils.DayTimeToWorldTime(1, num4, minutes);
			dailyVoteEntry.VoteEndTime = GameUtils.DayTimeToWorldTime(1, num4 + 1, minutes);
			dailyVoteEntry.Index = i + 1;
			num2 += num;
			DailyVoteTimes.Add(dailyVoteEntry);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetVoteTypeDay()
	{
		foreach (TwitchVoteType value in VoteTypes.Values)
		{
			value.CurrentDayCount = 0;
		}
		foreach (TwitchVote value2 in TwitchActionManager.TwitchVotes.Values)
		{
			value2.CurrentDayCount = 0;
		}
	}

	public bool SetupVoteList(List<TwitchVoteEntry> voteList)
	{
		World world = GameManager.Instance.World;
		TwitchVoteType currentVoteType = CurrentVoteType;
		string name = currentVoteType.Name;
		TwitchVote twitchVote = null;
		int highestGameStage = Owner.HighestGameStage;
		ClearVotes();
		voteList.Clear();
		tempSortList.Clear();
		tempVoteGroupList.Clear();
		EntityPlayer localPlayer = Owner.LocalPlayer;
		if (currentVoteType.GuaranteedGroup != "")
		{
			foreach (TwitchVote value3 in TwitchActionManager.TwitchVotes.Values)
			{
				if (value3.Enabled && value3.IsInPreset(Owner.CurrentVotePreset) && value3.VoteTypes.Contains(name) && value3.Group == currentVoteType.GuaranteedGroup && value3.CanUse(hour, highestGameStage, localPlayer))
				{
					tempSortList.Add(value3);
				}
			}
			for (int i = 0; i < tempSortList.Count * 3; i++)
			{
				int num = world.GetGameRandom().RandomRange(tempSortList.Count);
				int num2 = world.GetGameRandom().RandomRange(tempSortList.Count);
				if (num != num2)
				{
					TwitchVote value = tempSortList[num];
					tempSortList[num] = tempSortList[num2];
					tempSortList[num2] = value;
				}
			}
			twitchVote = tempSortList[0];
			tempSortList.Clear();
		}
		foreach (TwitchVote value4 in TwitchActionManager.TwitchVotes.Values)
		{
			if (value4.Enabled && value4.IsInPreset(Owner.CurrentVotePreset) && value4.VoteTypes.Contains(name) && value4.CanUse(hour, highestGameStage, localPlayer))
			{
				tempSortList.Add(value4);
			}
		}
		for (int j = 0; j < tempSortList.Count * 3; j++)
		{
			int num3 = world.GetGameRandom().RandomRange(tempSortList.Count);
			int num4 = world.GetGameRandom().RandomRange(tempSortList.Count);
			if (num3 != num4)
			{
				TwitchVote value2 = tempSortList[num3];
				tempSortList[num3] = tempSortList[num4];
				tempSortList[num4] = value2;
			}
		}
		NeededLines = 1;
		int num5 = 0;
		if (twitchVote != null)
		{
			tempSortList.Insert(Random.Range(0, 3), twitchVote);
		}
		for (int k = 0; k < tempSortList.Count; k++)
		{
			TwitchVote twitchVote2 = tempSortList[k];
			if (!(twitchVote2.Group != "") || !tempVoteGroupList.Contains(twitchVote2.Group))
			{
				if (twitchVote2.Group != "")
				{
					tempVoteGroupList.Add(twitchVote2.Group);
				}
				string voteCommand = VoteOptionA;
				switch (num5)
				{
				case 1:
					voteCommand = VoteOptionB;
					break;
				case 2:
					voteCommand = VoteOptionC;
					break;
				case 3:
					voteCommand = VoteOptionD;
					break;
				case 4:
					voteCommand = VoteOptionE;
					break;
				}
				if (twitchVote2.VoteLine1 != "" && NeededLines < 2)
				{
					NeededLines = 2;
				}
				if (twitchVote2.VoteLine2 != "" && NeededLines < 3)
				{
					NeededLines = 3;
				}
				voteList.Add(new TwitchVoteEntry(voteCommand, twitchVote2)
				{
					Owner = this,
					Index = num5
				});
				num5++;
				if (num5 == currentVoteType.VoteChoiceCount)
				{
					break;
				}
			}
		}
		if (voteList.Count == 0)
		{
			return false;
		}
		return true;
	}

	public TwitchVoteEntry GetVoteWinner()
	{
		tempVoteList.Clear();
		int num = -1;
		for (int i = 0; i < voteList.Count; i++)
		{
			if (voteList[i].VoteCount > num)
			{
				num = voteList[i].VoteCount;
				tempVoteList.Clear();
				tempVoteList.Add(voteList[i]);
			}
			else if (voteList[i].VoteCount == num)
			{
				tempVoteList.Add(voteList[i]);
			}
		}
		return tempVoteList[GameManager.Instance.World.GetGameRandom().RandomRange(0, tempVoteList.Count)];
	}

	public void ResetVoteOnDeath()
	{
		VoteStateTypes currentVoteState = CurrentVoteState;
		if ((uint)(currentVoteState - 5) <= 1u)
		{
			readyForVote = false;
			Owner.LocalPlayer.TwitchVoteLock = TwitchVoteLockTypes.None;
			CurrentVoteState = VoteStateTypes.WaitingForNextVote;
			ResetVoteGroupsForVote();
			if (VoteEventEnded != null)
			{
				VoteEventEnded();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetVoteGroupsForVote()
	{
		for (int i = 0; i < VoteGroups.Count; i++)
		{
			VoteGroups[i].SkippedThisVote = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckAllVoteGroupsSkipped()
	{
		for (int i = 0; i < VoteGroups.Count; i++)
		{
			if (!VoteGroups[i].SkippedThisVote)
			{
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool AllowVoting()
	{
		if (Owner.CooldownType != TwitchManager.CooldownTypes.None && ((Owner.CooldownType != TwitchManager.CooldownTypes.QuestCooldown && Owner.CooldownType != TwitchManager.CooldownTypes.QuestDisabled) || !AllowVotesDuringQuests))
		{
			if (Owner.CooldownType == TwitchManager.CooldownTypes.BloodMoonCooldown || Owner.CooldownType == TwitchManager.CooldownTypes.BloodMoonDisabled)
			{
				return AllowVotesDuringBloodmoon;
			}
			return false;
		}
		return true;
	}

	public void Update(float deltaTime)
	{
		switch (CurrentVoteState)
		{
		case VoteStateTypes.Init:
			GameEventManager.Current.GameEventApproved += Current_GameEventApproved;
			GameEventManager.Current.GameEventCompleted += Current_GameEventCompleted;
			GameEventManager.Current.GameEntitySpawned += Current_GameEntitySpawned;
			GameEventManager.Current.GameEntityKilled += Current_GameEntityKilled;
			CurrentVoteState = VoteStateTypes.WaitingForNextVote;
			ShuffleVoteGroups();
			ShuffleVoteGroupVoteTypes();
			break;
		case VoteStateTypes.WaitingForNextVote:
		{
			if (QueuedVoteType != null)
			{
				CurrentVoteType = QueuedVoteType;
				if (SetupVoteList(voteList))
				{
					CurrentVoteState = VoteStateTypes.ReadyForVoteStart;
					Owner.RefreshVoteLockedLevel();
				}
				QueuedVoteType = null;
			}
			if (VoteStartDelayTimeRemaining > 0f)
			{
				VoteStartDelayTimeRemaining -= deltaTime;
				break;
			}
			World world = GameManager.Instance.World;
			ulong worldTime = world.worldTime;
			day = GameUtils.WorldTimeToDays(worldTime);
			worldTime %= 24000;
			hour = GameUtils.WorldTimeToHours(worldTime);
			if (day == 1)
			{
				break;
			}
			VoteDayTimeRange voteDayTimeRange = VoteDayTimeRanges[CurrentVoteDayTimeRange];
			bool flag = !AllowVotesDuringBloodmoon && world.IsWorldEvent(World.WorldEvent.BloodMoon);
			if (VoteInProgress)
			{
				if (flag)
				{
					CancelVote();
				}
				if (hour < voteDayTimeRange.StartHour || hour > voteDayTimeRange.EndHour)
				{
					CancelVote();
				}
			}
			else
			{
				if (flag || hour < voteDayTimeRange.StartHour || hour > voteDayTimeRange.EndHour)
				{
					break;
				}
				if (day != lastGameDay)
				{
					CalculateVoteTimes();
					ResetVoteTypeDay();
					lastGameDay = day;
				}
				for (int i = 0; i < DailyVoteTimes.Count; i++)
				{
					if (DailyVoteTimes[i].LastVoteDay == day)
					{
						continue;
					}
					if (worldTime > DailyVoteTimes[i].VoteStartTime && worldTime < DailyVoteTimes[i].VoteEndTime)
					{
						if (!SetReadyForVote(DailyVoteTimes[i].Index))
						{
							if (CheckAllVoteGroupsSkipped())
							{
								DailyVoteTimes[i].LastVoteDay = day;
								ResetVoteGroupsForVote();
							}
							break;
						}
						ResetVoteGroupsForVote();
						if (SetupVoteList(voteList))
						{
							DailyVoteTimes[i].LastVoteDay = day;
							CurrentVoteState = VoteStateTypes.ReadyForVoteStart;
							Owner.RefreshVoteLockedLevel();
						}
					}
					else if (worldTime > DailyVoteTimes[i].VoteEndTime)
					{
						DailyVoteTimes[i].LastVoteDay = day;
					}
				}
			}
			break;
		}
		case VoteStateTypes.ReadyForVoteStart:
			if (VoteStartDelayTimeRemaining > 0f)
			{
				VoteStartDelayTimeRemaining -= deltaTime;
			}
			else if (VotingEnabled && Owner.VoteLockedLevel == TwitchVoteLockTypes.None && AllowVoting() && (!CurrentVoteType.SpawnBlocked || Owner.ReadyForVote))
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageTwitchVoteScheduling>().Setup());
				}
				else
				{
					TwitchVoteScheduler.Current.AddParticipant(Owner.LocalPlayer.entityId);
				}
				CurrentVoteState = VoteStateTypes.RequestedVoteStart;
			}
			break;
		case VoteStateTypes.VoteReady:
			if (VotingEnabled && Owner.VoteLockedLevel == TwitchVoteLockTypes.None && !CheckVoteLock() && AllowVoting() && (!CurrentVoteType.SpawnBlocked || Owner.ReadyForVote))
			{
				StartVote();
			}
			break;
		case VoteStateTypes.VoteStarted:
			if (VotingEnabled && AllowVoting())
			{
				VoteTimeRemaining -= Time.deltaTime;
				if (VoteTimeRemaining <= 0f)
				{
					CurrentVoteState = VoteStateTypes.VoteFinished;
				}
			}
			break;
		case VoteStateTypes.VoteFinished:
			CurrentEvent = GetVoteWinner();
			CurrentEvent.ActiveSpawns.Clear();
			CurrentEvent.VoteClass.CurrentDayCount++;
			Owner.ircClient.SendChannelMessage(string.Format(chatOutput_VoteFinished, CurrentEvent.VoteClass.VoteDescription), useQueue: true);
			CurrentVoteState = VoteStateTypes.WaitingForActive;
			VoteTimeRemaining = 2f;
			QuestEventManager.Current.TwitchEventReceived(TwitchObjectiveTypes.VoteComplete, CurrentEvent.VoteClass.Group);
			break;
		case VoteStateTypes.WaitingForActive:
			if (VoteTimeRemaining > 0f)
			{
				VoteTimeRemaining -= deltaTime;
			}
			else if (GameEventManager.Current.HandleAction(CurrentEvent.VoteClass.GameEvent, Owner.LocalPlayer, Owner.LocalPlayer, twitchActivated: true, " ", "vote", Owner.AllowCrateSharing))
			{
				GameEventManager.Current.HandleGameEventApproved(CurrentEvent.VoteClass.GameEvent, Owner.LocalPlayer.entityId, " ", "vote");
			}
			else
			{
				VoteTimeRemaining = 10f;
			}
			break;
		case VoteStateTypes.EventActive:
			if (VoteEventTimeRemaining < 0f)
			{
				if (VoteEventComplete)
				{
					HandleGameEventEnded(playSound: true);
				}
			}
			else
			{
				VoteEventTimeRemaining -= deltaTime;
			}
			break;
		case VoteStateTypes.RequestedVoteStart:
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckVoteLock()
	{
		if (!AllowVotesDuringQuests && QuestEventManager.Current.QuestBounds.width != 0f)
		{
			return true;
		}
		if (!AllowVotesInSafeZone && Owner.IsSafe)
		{
			return true;
		}
		return false;
	}

	public bool IsHighest(TwitchVoteEntry vote)
	{
		for (int i = 0; i < voteList.Count; i++)
		{
			if (i != vote.Index && voteList[i].VoteCount > vote.VoteCount)
			{
				return false;
			}
		}
		return true;
	}

	public bool SetReadyForVote(int index)
	{
		return GetNextVoteType();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetCurrentVote()
	{
		VoteTimeRemaining = VoteTime;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupVoteDayTimeRanges()
	{
		VoteDayTimeRanges.Clear();
		VoteDayTimeRanges.Add(new VoteDayTimeRange
		{
			Name = Localization.Get("TwitchVoteDayTimeRange_Short"),
			StartHour = 8,
			EndHour = 16
		});
		VoteDayTimeRanges.Add(new VoteDayTimeRange
		{
			Name = Localization.Get("TwitchVoteDayTimeRange_Average"),
			StartHour = 6,
			EndHour = 18
		});
		VoteDayTimeRanges.Add(new VoteDayTimeRange
		{
			Name = Localization.Get("TwitchVoteDayTimeRange_Extended"),
			StartHour = 4,
			EndHour = 20
		});
		VoteDayTimeRanges.Add(new VoteDayTimeRange
		{
			Name = Localization.Get("TwitchVoteDayTimeRange_All"),
			StartHour = 0,
			EndHour = 23
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShuffleVoteGroups()
	{
		for (int i = 0; i <= VoteGroups.Count * VoteGroups.Count; i++)
		{
			int num = Random.Range(0, VoteGroups.Count);
			int num2 = Random.Range(0, VoteGroups.Count);
			if (num != num2)
			{
				TwitchVoteGroup value = VoteGroups[num];
				VoteGroups[num] = VoteGroups[num2];
				VoteGroups[num2] = value;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShuffleVoteGroupVoteTypes()
	{
		for (int i = 0; i < VoteGroups.Count; i++)
		{
			VoteGroups[i].ShuffleVoteTypes();
		}
	}

	public void CancelVote()
	{
		if (CurrentVoteState == VoteStateTypes.WaitingForNextVote && readyForVote)
		{
			readyForVote = false;
		}
	}

	public void RequestApprovedToStart()
	{
		CurrentVoteState = VoteStateTypes.VoteReady;
	}

	public void StartVote()
	{
		VoteTimeRemaining = VoteTime;
		voterlist.Clear();
		if (VoteStarted != null)
		{
			VoteStarted();
		}
		Owner.UIDirty = true;
		Owner.LocalPlayer.TwitchVoteLock = ((!CurrentVoteType.ActionLockout) ? TwitchVoteLockTypes.VoteLocked : TwitchVoteLockTypes.ActionsLocked);
		Owner.ircClient.SendChannelMessage(chatOutput_VoteStarted, useQueue: true);
		Manager.BroadcastPlay(Owner.LocalPlayer.position, "twitch_vote_started");
		readyForVote = false;
		CurrentVoteType.CurrentDayCount++;
		CurrentVoteState = VoteStateTypes.VoteStarted;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_GameEntitySpawned(string gameEventID, int entityID, string tag)
	{
		if (CurrentEvent != null && !(tag != "vote") && gameEventID == CurrentEvent.VoteClass.GameEvent)
		{
			CurrentEvent.ActiveSpawns.Add(entityID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_GameEntityKilled(int entityID)
	{
		if (CurrentEvent != null && CurrentEvent.ActiveSpawns.Contains(entityID))
		{
			CurrentEvent.ActiveSpawns.Remove(entityID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_GameEventCompleted(string gameEventID, int targetEntityID, string extraData, string tag)
	{
		if (CurrentEvent != null && CurrentEvent.VoteClass.GameEvent == gameEventID && tag == "vote")
		{
			VoteEventComplete = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleGameEventEnded(bool playSound)
	{
		if (CurrentVoteType.CooldownOnEnd && Owner.AllowActions && Owner.CurrentCooldownPreset.CooldownType == CooldownPreset.CooldownTypes.Fill)
		{
			Owner.SetCooldown(Owner.CurrentCooldownPreset.NextCooldownTime, TwitchManager.CooldownTypes.MaxReached);
		}
		CurrentEvent.VoteClass.HandleVoteComplete();
		CurrentEvent = null;
		if (playSound)
		{
			Manager.BroadcastPlay(Owner.LocalPlayer.position, "twitch_vote_ended");
		}
		Owner.LocalPlayer.TwitchVoteLock = TwitchVoteLockTypes.None;
		CurrentVoteState = VoteStateTypes.WaitingForNextVote;
		ResetVoteGroupsForVote();
		if (VoteEventEnded != null)
		{
			VoteEventEnded();
		}
		VoteStartDelayTimeRemaining = 10f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool GetNextVoteType()
	{
		if (voteGroupIndex == -1)
		{
			voteGroupIndex = GameManager.Instance.World.GetGameRandom().RandomRange(VoteGroups.Count);
		}
		bool flag = GameManager.Instance.World.IsWorldEvent(World.WorldEvent.BloodMoon);
		TwitchVoteGroup twitchVoteGroup = VoteGroups[voteGroupIndex];
		if (!twitchVoteGroup.SkippedThisVote)
		{
			for (int i = 0; i < twitchVoteGroup.VoteTypes.Count; i++)
			{
				TwitchVoteType nextVoteType = twitchVoteGroup.GetNextVoteType();
				if (nextVoteType.IsInPreset(Owner.CurrentVotePreset.Name) && !nextVoteType.ManualStart && nextVoteType.CanUse() && hour >= nextVoteType.AllowedStartHour && hour <= nextVoteType.AllowedEndHour && (!nextVoteType.IsBoss || Owner.CurrentVotePreset.BossVoteSetting != BossVoteSettings.Disabled) && (nextVoteType.AllowedWithActions || !Owner.AllowActions) && ((nextVoteType.IsBoss && Owner.CurrentVotePreset.BossVoteSetting == BossVoteSettings.Daily) || ((nextVoteType.BloodMoonDay || Owner.nextBMDay != day) && (nextVoteType.BloodMoonAllowed || !flag))))
				{
					CurrentVoteType = nextVoteType;
					voteGroupIndex++;
					if (voteGroupIndex >= VoteGroups.Count)
					{
						voteGroupIndex = 0;
					}
					return true;
				}
			}
			twitchVoteGroup.SkippedThisVote = true;
		}
		voteGroupIndex++;
		if (voteGroupIndex >= VoteGroups.Count)
		{
			voteGroupIndex = 0;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_GameEventApproved(string gameEventID, int targetEntityID, string extraData, string tag)
	{
		if (CurrentVoteState == VoteStateTypes.WaitingForActive && CurrentEvent.VoteClass.GameEvent == gameEventID)
		{
			CurrentVoteState = VoteStateTypes.EventActive;
			VoteEventComplete = false;
			VoteEventTimeRemaining = 10f;
			CurrentEvent.VoterNames.AddRange(voterlist);
			Owner.AddVoteHistory(CurrentEvent.VoteClass);
			if (VoteEventStarted != null)
			{
				VoteEventStarted();
			}
		}
	}

	public void HandleMessage(TwitchIRCClient.TwitchChatMessage message)
	{
		if (CurrentVoteState == VoteStateTypes.VoteStarted)
		{
			if (message.Message.EqualsCaseInsensitive(VoteOptionA))
			{
				AddVote(0, message.UserName);
			}
			else if (message.Message.EqualsCaseInsensitive(VoteOptionB))
			{
				AddVote(1, message.UserName);
			}
			else if (message.Message.EqualsCaseInsensitive(VoteOptionC))
			{
				AddVote(2, message.UserName);
			}
			else if (message.Message.EqualsCaseInsensitive(VoteOptionD))
			{
				AddVote(3, message.UserName);
			}
			else if (message.Message.EqualsCaseInsensitive(VoteOptionE))
			{
				AddVote(4, message.UserName);
			}
		}
	}

	public List<string> HandleKiller(TwitchVoteEntry voteEntry)
	{
		if (CurrentEvent == null && voteEntry == null)
		{
			return null;
		}
		if (CurrentEvent != null)
		{
			_ = CurrentEvent.VoterNames;
			CurrentEvent.Complete = true;
			HandleGameEventEnded(playSound: false);
			return voterlist;
		}
		return voteEntry?.VoterNames;
	}

	public string GetDayTimeRange(int tempVoteDayTimeRange)
	{
		VoteDayTimeRange voteDayTimeRange = VoteDayTimeRanges[tempVoteDayTimeRange];
		if (voteDayTimeRange.StartHour == 0 && voteDayTimeRange.EndHour == 23)
		{
			return voteDayTimeRange.Name;
		}
		return string.Format(dayTimeRangeOutput, voteDayTimeRange.StartHour, voteDayTimeRange.EndHour);
	}

	public void QueueVote(string voteType)
	{
		if (VoteTypes.ContainsKey(voteType))
		{
			QueuedVoteType = VoteTypes[voteType];
		}
	}

	public void ForceEndVote()
	{
		if (CurrentVoteState == VoteStateTypes.VoteStarted)
		{
			CurrentEvent = null;
			Owner.LocalPlayer.TwitchVoteLock = TwitchVoteLockTypes.None;
			CurrentVoteState = VoteStateTypes.WaitingForNextVote;
			if (VoteEventEnded != null)
			{
				VoteEventEnded();
			}
		}
	}
}
