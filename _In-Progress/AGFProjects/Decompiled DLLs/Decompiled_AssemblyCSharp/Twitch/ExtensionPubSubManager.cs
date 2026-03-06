using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using UniLinq;
using UnityEngine.Networking;

namespace Twitch;

public class ExtensionPubSubManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<string> target = new List<string> { "broadcast" };

	[PublicizedFrom(EAccessModifier.Private)]
	public string jwt;

	[PublicizedFrom(EAccessModifier.Private)]
	public string updateSignature;

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<(string, int)> UserBitBalances = new Queue<(string, int)>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, int> bitBalancesByUser = new Dictionary<string, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<(string, bool)> viewerChatStateQueue = new Queue<(string, bool)>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, bool> chattersToSend = new Dictionary<string, bool>();

	public void SetJWT(string jwt)
	{
		this.jwt = jwt;
	}

	public void Update(bool updatedViewerConfig)
	{
		if (updatedViewerConfig)
		{
			updateSignature = Guid.NewGuid().ToString();
		}
		SendUpdate();
	}

	public void PushUserBalance((string, int) userBalance)
	{
		Log.Out($"Adding balance of {userBalance.Item2} to user {userBalance.Item1}");
		Log.Out(new StackTrace(fNeedFileInfo: true).ToString());
		UserBitBalances.Enqueue(userBalance);
	}

	public void PushViewerChatState(string id, bool hasChatted)
	{
		viewerChatStateQueue.Enqueue((id, hasChatted));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendUpdate()
	{
		GameManager.Instance.StartCoroutine(UpdateViewers());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator UpdateViewers()
	{
		int num = Utils.FastMin(UserBitBalances.Count, 100);
		for (int i = 0; i < num; i++)
		{
			(string, int) tuple = UserBitBalances.Dequeue();
			if (bitBalancesByUser.ContainsKey(tuple.Item1))
			{
				bitBalancesByUser[tuple.Item1] = tuple.Item2;
			}
			else
			{
				bitBalancesByUser.Add(tuple.Item1, tuple.Item2);
			}
		}
		num = Utils.FastMin(viewerChatStateQueue.Count, 100);
		for (int j = 0; j < num; j++)
		{
			(string, bool) tuple2 = viewerChatStateQueue.Dequeue();
			if (!chattersToSend.ContainsKey(tuple2.Item1))
			{
				chattersToSend.Add(tuple2.Item1, tuple2.Item2);
			}
		}
		string message = JsonConvert.SerializeObject(new UpdateMessage
		{
			updateSignature = updateSignature,
			status = getStatus(),
			actionCooldowns = getActionCooldowns(),
			bitBalances = bitBalancesByUser,
			hasChatted = chattersToSend
		});
		bitBalancesByUser.Clear();
		chattersToSend.Clear();
		string bodyData = JsonConvert.SerializeObject(new PubSubStatusRequestData
		{
			broadcaster_id = TwitchManager.Current.Authentication.userID,
			target = target,
			message = message
		});
		using UnityWebRequest request = UnityWebRequest.Put("https://api.twitch.tv/helix/extensions/pubsub", bodyData);
		request.method = "POST";
		request.SetRequestHeader("Content-Type", "application/json");
		request.SetRequestHeader("Client-Id", "k6ji189bf7i4ge8il4iczzw7kpgmjt");
		request.SetRequestHeader("Authorization", "Bearer " + jwt);
		yield return request.SendWebRequest();
		if (request.result != UnityWebRequest.Result.Success)
		{
			Log.Warning("Failed to broadcast status change: " + request.downloadHandler.text);
		}
		else if (request.responseCode == 403)
		{
			TwitchManager.Current.extensionManager.RetrieveJWT();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string getStatus()
	{
		TwitchManager current = TwitchManager.Current;
		if (!current.TwitchActive)
		{
			return "paused";
		}
		if (current.CurrentActionPreset.IsEmpty || current.VoteLockedLevel == TwitchVoteLockTypes.ActionsLocked)
		{
			return "actionsDisabled";
		}
		if (current.IsVoting || (current.LocalPlayer != null && current.LocalPlayer.TwitchVoteLock == TwitchVoteLockTypes.ActionsLocked))
		{
			return "full";
		}
		switch (TwitchManager.Current.CooldownType)
		{
		case TwitchManager.CooldownTypes.Startup:
		case TwitchManager.CooldownTypes.Time:
		case TwitchManager.CooldownTypes.BloodMoonDisabled:
		case TwitchManager.CooldownTypes.QuestDisabled:
			return "full";
		case TwitchManager.CooldownTypes.MaxReached:
		case TwitchManager.CooldownTypes.BloodMoonCooldown:
		case TwitchManager.CooldownTypes.QuestCooldown:
			return "regular";
		case TwitchManager.CooldownTypes.MaxReachedWaiting:
		case TwitchManager.CooldownTypes.SafeCooldown:
		case TwitchManager.CooldownTypes.SafeCooldownExit:
			return "wait";
		default:
			return "online";
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] getActionCooldowns()
	{
		TwitchAction[] array = TwitchManager.Current.AvailableCommands.Values.Where([PublicizedFrom(EAccessModifier.Internal)] (TwitchAction a) => a.HasExtraConditions() && (TwitchManager.Current.extensionManager.CanUseBitCommands() || a.PointType != TwitchAction.PointTypes.Bits)).ToArray();
		int[] array2 = new int[array.Count() / 32 + ((array.Count() % 32 != 0) ? 1 : 0)];
		int num = 0;
		TwitchAction[] array3 = array;
		foreach (TwitchAction twitchAction in array3)
		{
			array2[num / 32] |= ((!twitchAction.IsReady(TwitchManager.Current)) ? (1 << num % 32) : 0);
			num++;
		}
		return array2;
	}
}
