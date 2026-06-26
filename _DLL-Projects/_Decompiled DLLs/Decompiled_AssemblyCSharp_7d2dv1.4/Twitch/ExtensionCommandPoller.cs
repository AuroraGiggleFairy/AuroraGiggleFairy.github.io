using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UniLinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Twitch;

public class ExtensionCommandPoller
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const long BIT_ACTION_TIMEOUT_MS = 30000L;

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<ExtensionAction> commandQueue;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<string> transactionHistory;

	[PublicizedFrom(EAccessModifier.Private)]
	public string login;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastPollTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool queueExists;

	public void Init()
	{
		commandQueue = new Queue<ExtensionAction>();
		transactionHistory = new HashSet<string>();
		lastPollTime = Time.time;
		login = TwitchManager.Current.Authentication.userName;
		GameManager.Instance.StartCoroutine(CreateQueue());
	}

	public void Cleanup()
	{
		commandQueue.Clear();
		commandQueue = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInCooldown()
	{
		switch (TwitchManager.Current.CooldownType)
		{
		case TwitchManager.CooldownTypes.Startup:
		case TwitchManager.CooldownTypes.Time:
		case TwitchManager.CooldownTypes.BloodMoonDisabled:
		case TwitchManager.CooldownTypes.QuestDisabled:
			return true;
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cooldownUpdateable()
	{
		if (TwitchManager.Current.CooldownType == TwitchManager.CooldownTypes.Startup || TwitchManager.Current.CooldownType == TwitchManager.CooldownTypes.Time)
		{
			return Time.realtimeSinceStartup - lastPollTime > 30f;
		}
		return false;
	}

	public void Update()
	{
		if (queueExists && TwitchManager.Current.AllowActions && Time.realtimeSinceStartup - lastPollTime > 3f && !isInCooldown() && TwitchManager.Current.Authentication.oauth != "")
		{
			GameManager.Instance.StartCoroutine(PollQueue());
			lastPollTime = Time.realtimeSinceStartup;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator PollQueue()
	{
		using UnityWebRequest req = UnityWebRequest.Get("https://2v3d0ewjcg.execute-api.us-east-1.amazonaws.com/prod/broadcaster/actions");
		req.SetRequestHeader("Authorization", TwitchManager.Current.Authentication.userID + " " + TwitchManager.Current.Authentication.oauth.Substring(6));
		req.SetRequestHeader("Content-Type", "application/json");
		yield return req.SendWebRequest();
		if (req.result != UnityWebRequest.Result.Success)
		{
			Log.Warning($"Could not retrieve commands with status code {req.responseCode}: {req.downloadHandler.text}");
			yield break;
		}
		try
		{
			ExtensionActionResponse extensionActionResponse = JsonConvert.DeserializeObject<ExtensionActionResponse>(req.downloadHandler.text);
			if (commandQueue == null)
			{
				yield break;
			}
			if (extensionActionResponse.bitActions.Count > 0)
			{
				long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
				extensionActionResponse.bitActions.Sort([PublicizedFrom(EAccessModifier.Internal)] (ExtensionBitAction a, ExtensionBitAction b) => a.time_created.CompareTo(b.time_created));
				extensionActionResponse.bitActions.ForEach([PublicizedFrom(EAccessModifier.Internal)] (ExtensionBitAction bitAction) =>
				{
					if (transactionHistory.Contains(bitAction.txn_id))
					{
						Log.Warning("duplicate transaction received with id " + bitAction.txn_id);
					}
					else
					{
						transactionHistory.Add(bitAction.txn_id);
						int result;
						string value;
						if (currentTime - bitAction.time_created <= 30000)
						{
							Log.Out("bit action " + bitAction.command + " received from " + bitAction.username + " with txn_id " + bitAction.txn_id);
							commandQueue.Enqueue(bitAction);
						}
						else if (int.TryParse(bitAction.username, out result) && TwitchManager.Current.ViewerData.IdToUsername.TryGetValue(result, out value))
						{
							TwitchManager current = TwitchManager.Current;
							current.AddToBitPot((int)((float)bitAction.cost * current.BitPotPercentage));
							current.ViewerData.AddCredit(value, bitAction.cost, displayNewTotal: false);
							ViewerEntry viewerEntry = TwitchManager.Current.ViewerData.GetViewerEntry(value);
							current.extensionManager.PushUserBalance((bitAction.username, viewerEntry.BitCredits));
						}
						else
						{
							Log.Warning("could not give credit to user id " + bitAction.username);
						}
					}
				});
				DeleteTransactionFromTable(extensionActionResponse.bitActions.Select([PublicizedFrom(EAccessModifier.Internal)] (ExtensionBitAction a) => a.txn_id).ToList());
			}
			extensionActionResponse.standardActions.ForEach([PublicizedFrom(EAccessModifier.Private)] (ExtensionAction cmd) =>
			{
				if (cmd.command.Equals("#refreshcredit"))
				{
					if (int.TryParse(cmd.username, out var result) && TwitchManager.Current.ViewerData.IdToUsername.TryGetValue(result, out var value))
					{
						ViewerEntry viewerEntry = TwitchManager.Current.ViewerData.GetViewerEntry(value);
						TwitchManager.Current.extensionManager.PushUserBalance((cmd.username, viewerEntry.BitCredits));
						TwitchManager.Current.extensionManager.PushViewerChatState(cmd.username, hasChatted: true);
						Log.Out("added " + cmd.username + " to new chatters");
					}
					else
					{
						TwitchManager.Current.extensionManager.PushViewerChatState(cmd.username, hasChatted: false);
					}
				}
				else
				{
					commandQueue.Enqueue(cmd);
				}
			});
		}
		catch (Exception ex)
		{
			Log.Warning("command poller encountered issue receving this data: " + req.downloadHandler.text + "\n excption thrown: " + ex.Message);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator CreateQueue()
	{
		using UnityWebRequest req = UnityWebRequest.Put("https://2v3d0ewjcg.execute-api.us-east-1.amazonaws.com/prod/command-queue", "{}");
		req.SetRequestHeader("Authorization", login + " " + TwitchManager.Current.Authentication.oauth.Substring(6));
		req.SetRequestHeader("Content-Type", "application/json");
		yield return req.SendWebRequest();
		if (req.result != UnityWebRequest.Result.Success)
		{
			Log.Warning("Could not create queue");
			yield break;
		}
		JObject jObject = JObject.Parse(req.downloadHandler.text);
		queueExists = jObject != null && jObject.TryGetValue("message", out JToken value) && value.ToString() == "success";
		if (!queueExists)
		{
			Log.Warning("Could not create queue");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DeleteTransactionFromTable(List<string> transactions)
	{
		GameManager.Instance.StartCoroutine(DeleteTransactionFromTableCoroutine(transactions));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator DeleteTransactionFromTableCoroutine(List<string> transactions)
	{
		string bodyData = JsonConvert.SerializeObject(new ExtensionDeleteBitActionsRequestData
		{
			transactions = transactions
		});
		using UnityWebRequest req = UnityWebRequest.Put("https://2v3d0ewjcg.execute-api.us-east-1.amazonaws.com/prod/broadcaster/actions", bodyData);
		req.method = "DELETE";
		req.SetRequestHeader("Authorization", TwitchManager.Current.Authentication.userID + " " + TwitchManager.Current.Authentication.oauth.Substring(6));
		req.SetRequestHeader("Content-Type", "application/json");
		yield return req.SendWebRequest();
		if (req.result != UnityWebRequest.Result.Success)
		{
			Log.Warning("Failed to delete the transactions");
		}
	}

	public bool HasCommand()
	{
		return commandQueue.Count > 0;
	}

	public ExtensionAction GetCommand()
	{
		return commandQueue.Dequeue();
	}
}
