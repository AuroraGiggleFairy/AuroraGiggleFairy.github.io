using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UniLinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Twitch;

public class ExtensionConfigManager
{
	public class ConfigModel
	{
		public string displayName;

		public List<string> party;

		public List<string> categories;

		public string IdentityGrantHeader = Localization.Get("TwitchInfo_IdentityGrantHeader");

		public string IdentityGrantSubtext = Localization.Get("TwitchInfo_IdentityGrantSubtext");

		public string LoadingText = Localization.Get("loadActionLoading");

		public string OfflineHeader = Localization.Get("TwitchInfo_OfflineHeader");

		public string OfflineSubtext1 = Localization.Get("TwitchInfo_OfflineSubtext1");

		public string OfflineSubtext2 = Localization.Get("TwitchInfo_OfflineSubtext2");

		public string CommandDescriptionsText = Localization.Get("TwitchInfo_CommandDescriptionsText");

		public string ChatPromptHeader = Localization.Get("TwitchInfo_ChatPromptHeader");

		public string ChatPromptSubtext = Localization.Get("TwitchInfo_ChatPromptSubtext");

		public string ActionsOffText = Localization.Get("TwitchInfo_ActionsOffText");

		public string CooldownText = Localization.Get("TwitchInfo_CooldownText");

		public string PausedText = Localization.Get("TwitchCooldownStatus_Paused");

		public string ActionPresetLabel = Localization.Get("xuiOptionsTwitchActionPreset");

		public string VotePresetLabel = Localization.Get("xuiOptionsTwitchVotePreset");

		public string EventPresetLabel = Localization.Get("xuiOptionsTwitchCustomEvents");

		public string TopKillerLabel = Localization.Get("TwitchInfo_TopKiller");

		public string TopGoodLabel = Localization.Get("TwitchInfo_TopGood");

		public string TopBadLabel = Localization.Get("TwitchInfo_TopEvil");

		public string BestHelperLabel = string.Format(Localization.Get("TwitchInfo_CurrentGood"), TwitchManager.LeaderboardStats.GoodRewardTime);

		public string TotalGoodActionsLabel = Localization.Get("TwitchInfo_TotalGood");

		public string TotalBadActionsLabel = Localization.Get("TwitchInfo_TotalBad");

		public string LargestPimpPotLabel = Localization.Get("TwitchInfo_LargestPimpPot");

		public string DifficultyLabel = Localization.Get("goDifficultyShort");

		public string DayCycleLabel = Localization.Get("goDayLength");

		public string ModdedLabel = Localization.Get("goModded");

		public string PPRateLabel = Localization.Get("TwitchInfo_PPRate");

		public Dictionary<string, List<CommandModel>> commands;
	}

	public class CommandModel
	{
		public string name;

		public string baseCommand;

		public string command;

		public bool isPositive;

		public string spends;

		public int cost;

		public string cooldownType;

		public int cooldownIndex;

		public byte bitPosition;

		public bool streamerOnly;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<string> emptyParty = new List<string>(0);

	[PublicizedFrom(EAccessModifier.Private)]
	public string displayName = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string broadcasterType = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string jwt = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasUpdated;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float pushConfigTimeout = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastPushTime = float.NegativeInfinity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool waitingToPush;

	public void Init()
	{
		TwitchManager.Current.CommandsChanged += pushConfig;
		TwitchVotingManager votingManager = TwitchManager.Current.VotingManager;
		votingManager.VoteEventEnded = (OnGameEventVoteAction)Delegate.Combine(votingManager.VoteEventEnded, new OnGameEventVoteAction(pushConfig));
		TwitchVotingManager votingManager2 = TwitchManager.Current.VotingManager;
		votingManager2.VoteStarted = (OnGameEventVoteAction)Delegate.Combine(votingManager2.VoteStarted, new OnGameEventVoteAction(pushConfig));
		pushConfig();
	}

	public bool UpdatedConfig()
	{
		if (hasUpdated)
		{
			hasUpdated = false;
			return true;
		}
		return false;
	}

	public void Cleanup()
	{
		TwitchManager.Current.CommandsChanged -= pushConfig;
		TwitchVotingManager votingManager = TwitchManager.Current.VotingManager;
		votingManager.VoteEventEnded = (OnGameEventVoteAction)Delegate.Remove(votingManager.VoteEventEnded, new OnGameEventVoteAction(pushConfig));
		TwitchVotingManager votingManager2 = TwitchManager.Current.VotingManager;
		votingManager2.VoteStarted = (OnGameEventVoteAction)Delegate.Remove(votingManager2.VoteStarted, new OnGameEventVoteAction(pushConfig));
	}

	public void OnPartyChanged()
	{
		pushConfig();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void pushConfig()
	{
		lastPushTime = Time.time;
		if (!waitingToPush)
		{
			waitingToPush = true;
			GameManager.Instance.StartCoroutine(pushConfigAfterTimeout());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator pushConfigAfterTimeout()
	{
		while (Time.time - lastPushTime < 1f)
		{
			yield return null;
		}
		waitingToPush = false;
		yield return UpdateConfig();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator UpdateConfig()
	{
		if (displayName == string.Empty)
		{
			yield return GetDisplayName();
		}
		if (TwitchManager.Current == null || TwitchManager.Current.Authentication == null)
		{
			Log.Warning("attempted to updated config with no Auth object");
			yield break;
		}
		Dictionary<string, List<CommandModel>> commands = GetCommands();
		List<string> activeCategories = GetActiveCategories(commands.Keys.ToList());
		string bodyData = JsonConvert.SerializeObject(new ConfigModel
		{
			displayName = TwitchManager.Current.Authentication.userName,
			party = GetPlayers(),
			categories = activeCategories,
			commands = commands
		});
		using UnityWebRequest req = UnityWebRequest.Put("https://2v3d0ewjcg.execute-api.us-east-1.amazonaws.com/prod/broadcaster/config", bodyData);
		req.SetRequestHeader("Authorization", TwitchManager.Current.Authentication.userID + " " + TwitchManager.Current.Authentication.oauth.Substring(6));
		req.SetRequestHeader("Content-Type", "application/json");
		yield return req.SendWebRequest();
		if (req.result != UnityWebRequest.Result.Success)
		{
			Log.Warning($"Could not update config on backend: {req.result}");
			yield break;
		}
		Log.Out("Successfully updated broadcaster config");
		hasUpdated = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator GetDisplayName()
	{
		using UnityWebRequest req = UnityWebRequest.Get("https://api.twitch.tv/helix/users");
		req.SetRequestHeader("Content-Type", "application/json");
		req.SetRequestHeader("Client-Id", TwitchAuthentication.client_id);
		req.SetRequestHeader("Authorization", "Bearer " + TwitchManager.Current.Authentication.oauth.Substring(6));
		yield return req.SendWebRequest();
		if (req.result != UnityWebRequest.Result.Success)
		{
			Log.Warning($"Could not get user data from Twitch: {req.result}");
			yield break;
		}
		Log.Out("Successfully retrieved user data from Twitch");
		JObject jObject = JObject.Parse(req.downloadHandler.text);
		displayName = jObject["data"][0]["display_name"].ToString();
		broadcasterType = jObject["data"][0]["broadcaster_type"].ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> GetActiveCategories(List<string> categories)
	{
		return (from c in TwitchActionManager.Current.CategoryList
			select c.Name into name
			where categories.Contains(name)
			select name).ToList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> GetPlayers()
	{
		if (TwitchManager.Current.LocalPlayer != null && TwitchManager.Current.LocalPlayer.Party != null && TwitchManager.Current.LocalPlayer.Party.MemberList != null)
		{
			return (from e in TwitchManager.Current.LocalPlayer.Party.MemberList
				where !(e is EntityPlayerLocal) && !e.TwitchEnabled
				select e.EntityName).ToList();
		}
		return emptyParty;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, List<CommandModel>> GetCommands()
	{
		Dictionary<string, List<CommandModel>> dictionary = new Dictionary<string, List<CommandModel>>();
		TwitchAction[] array = TwitchManager.Current.AvailableCommands.Values.Where([PublicizedFrom(EAccessModifier.Private)] (TwitchAction a) => a.HasExtraConditions() && (CanUseBitCommands() || a.PointType != TwitchAction.PointTypes.Bits)).ToArray();
		int num = 0;
		TwitchAction[] array2 = array;
		foreach (TwitchAction twitchAction in array2)
		{
			CommandModel item = new CommandModel
			{
				name = twitchAction.Command.Replace("#", string.Empty).Replace("_", " ").ToUpper(),
				baseCommand = twitchAction.BaseCommand,
				command = twitchAction.Command,
				isPositive = twitchAction.IsPositive,
				spends = twitchAction.PointType.ToString(),
				cost = twitchAction.CurrentCost,
				cooldownType = (twitchAction.WaitingBlocked ? "wait" : (twitchAction.CooldownBlocked ? "regular" : "full")),
				cooldownIndex = num / 32,
				bitPosition = (byte)(num % 32),
				streamerOnly = twitchAction.StreamerOnly
			};
			if (!dictionary.TryGetValue(twitchAction.MainCategory.Name, out var value))
			{
				dictionary.Add(twitchAction.MainCategory.Name, value = new List<CommandModel>());
			}
			value.Add(item);
			num++;
		}
		return dictionary;
	}

	public bool CanUseBitCommands()
	{
		if (!(broadcasterType == "affiliate"))
		{
			return broadcasterType == "partner";
		}
		return true;
	}
}
