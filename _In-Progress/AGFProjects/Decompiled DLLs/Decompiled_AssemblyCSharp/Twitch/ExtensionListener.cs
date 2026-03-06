using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UniLinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Twitch;

public class ExtensionListener
{
	[Serializable]
	public class BitCmdVerifyResponse
	{
		public bool canSend;

		public string userId;

		public string command;
	}

	[Serializable]
	public class InBetaResponse
	{
		public bool inBeta;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EXTENSION_ID = "k6ji189bf7i4ge8il4iczzw7kpgmjt";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string URL = "http://localhost:52775/";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HttpListener listener;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pushConfigChanges;

	[PublicizedFrom(EAccessModifier.Private)]
	public string JWT = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string opaqueId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string userId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string displayName = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string broadcaster_type = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool inBeta;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isConnected;

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<TwitchExtensionCommand> commands = new Queue<TwitchExtensionCommand>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchAction> tempCommandList = new List<TwitchAction>();

	public ExtensionListener()
	{
		listener = new HttpListener();
		listener.Prefixes.Add("http://localhost:52775/");
	}

	public void StopListener()
	{
		listener.Stop();
		setConfig("offline");
		displayName = string.Empty;
		TwitchManager.Current.CommandsChanged -= pushConfig;
		TwitchVotingManager votingManager = TwitchManager.Current.VotingManager;
		votingManager.VoteEventEnded = (OnGameEventVoteAction)Delegate.Remove(votingManager.VoteEventEnded, new OnGameEventVoteAction(pushConfig));
		TwitchVotingManager votingManager2 = TwitchManager.Current.VotingManager;
		votingManager2.VoteStarted = (OnGameEventVoteAction)Delegate.Remove(votingManager2.VoteStarted, new OnGameEventVoteAction(pushConfig));
	}

	public void RunListener()
	{
		Task.Run([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			StartListener();
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartListener()
	{
		TwitchManager.Current.CommandsChanged += pushConfig;
		TwitchVotingManager votingManager = TwitchManager.Current.VotingManager;
		votingManager.VoteEventEnded = (OnGameEventVoteAction)Delegate.Combine(votingManager.VoteEventEnded, new OnGameEventVoteAction(pushConfig));
		TwitchVotingManager votingManager2 = TwitchManager.Current.VotingManager;
		votingManager2.VoteStarted = (OnGameEventVoteAction)Delegate.Combine(votingManager2.VoteStarted, new OnGameEventVoteAction(pushConfig));
		listener.Start();
		while (listener.IsListening)
		{
			HttpListenerContext context = listener.GetContext();
			HttpListenerRequest request = context.Request;
			Log.Out(request.Url.LocalPath);
			HttpListenerResponse response = context.Response;
			response.AddHeader("Access-Control-Allow-Origin", "*");
			response.ContentType = "application/json";
			switch (request.Url.LocalPath)
			{
			case "/command":
				if (request.HttpMethod == "POST")
				{
					commands.Enqueue(new TwitchExtensionCommand(request));
				}
				response.ContentLength64 = 0L;
				break;
			case "/connect":
				if (request.HttpMethod == "GET")
				{
					JWT = request.QueryString["token"];
					userId = request.QueryString["userId"];
					opaqueId = request.QueryString["opaqueId"];
					Log.Out("TOKEN: " + JWT);
					pushConfigChanges = (isConnected = JWT != "" && userId != "" && opaqueId != "");
					if (!pushConfigChanges)
					{
						Log.Warning("Query string missing from connection call");
					}
				}
				response.ContentLength64 = 0L;
				break;
			case "/bitCmdVerification":
			{
				string actionName = request.QueryString["command"];
				string text = request.QueryString["userId"];
				response.ContentType = "application/json";
				string s = JsonConvert.SerializeObject(new BitCmdVerifyResponse
				{
					canSend = TwitchManager.Current.IsActionAvailable(actionName),
					userId = text
				});
				byte[] bytes = Encoding.ASCII.GetBytes(s);
				response.ContentLength64 = bytes.Length;
				response.OutputStream.Write(bytes, 0, bytes.Length);
				break;
			}
			}
			response.OutputStream.Close();
		}
	}

	public void OnPartyChanged()
	{
		pushConfigChanges = true;
	}

	public bool HasCommand()
	{
		return commands.Count > 0;
	}

	public TwitchExtensionCommand GetCommand()
	{
		lock (commands)
		{
			return commands.Dequeue();
		}
	}

	public void Update()
	{
		if (isConnected && (pushConfigChanges || Time.time - lastUpdate > 15f))
		{
			UpdateCooldown();
			pushConfigChanges = false;
			lastUpdate = Time.time;
		}
	}

	public void UpdateCooldown()
	{
		if (!JWT.Equals(string.Empty))
		{
			setConfig(GetPubSubStatus());
			lastUpdate = Time.time;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetPubSubStatus()
	{
		TwitchManager current = TwitchManager.Current;
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
	public void pushConfig()
	{
		pushConfigChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setConfig(string status)
	{
		GameManager.Instance.StartCoroutine(pushConfig(status, (status != "offline") ? getCommands() : string.Empty));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string getCommands()
	{
		StringBuilder stringBuilder = new StringBuilder();
		tempCommandList.Clear();
		foreach (TwitchAction action in TwitchManager.Current.AvailableCommands.Values)
		{
			if (!action.HasExtraConditions())
			{
				continue;
			}
			if (MathUtils.Min(TwitchActionManager.Current.CategoryList.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (TwitchActionManager.ActionCategory c) => c.Name == action.CategoryNames.Last()), 6) == -1)
			{
				Log.Warning("no type index found for " + action.Name);
			}
			else if (action.PointType == TwitchAction.PointTypes.Bits)
			{
				if (broadcaster_type == "partner" || broadcaster_type == "affiliate")
				{
					tempCommandList.Add(action);
				}
			}
			else
			{
				tempCommandList.Add(action);
			}
		}
		tempCommandList = (from c in tempCommandList
			orderby c.PointType, c.Command
			select c).ToList();
		for (int num = 0; num < tempCommandList.Count; num++)
		{
			TwitchAction action2 = tempCommandList[num];
			string text = "";
			int a = TwitchActionManager.Current.CategoryList.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (TwitchActionManager.ActionCategory c) => c.Name == action2.CategoryNames.Last());
			a = MathUtils.Min(a, 6);
			switch (action2.PointType)
			{
			case TwitchAction.PointTypes.PP:
				text = ((!action2.WaitingBlocked) ? ((!action2.CooldownBlocked) ? "!" : "&") : "*");
				break;
			case TwitchAction.PointTypes.SP:
				text = ((!action2.WaitingBlocked) ? ((!action2.CooldownBlocked) ? "#" : "(") : "+");
				break;
			case TwitchAction.PointTypes.Bits:
				text = ((!action2.WaitingBlocked) ? ((!action2.CooldownBlocked) ? "$" : ")") : "-");
				break;
			}
			if (!(text == ""))
			{
				stringBuilder.Append(a);
				string text2 = action2.Command.Replace("#", string.Empty);
				if (action2.IsPositive)
				{
					text2 = text2.ToUpper();
				}
				stringBuilder.Append(text2);
				stringBuilder.Append(text);
				stringBuilder.Append(action2.CurrentCost);
				stringBuilder.Append(",");
			}
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator pushConfig(string status, string cmds)
	{
		if (displayName == string.Empty)
		{
			using (UnityWebRequest req = UnityWebRequest.Get("https://api.twitch.tv/helix/users?id=" + userId))
			{
				req.SetRequestHeader("Content-Type", "application/json");
				req.SetRequestHeader("Client-Id", TwitchAuthentication.client_id);
				req.SetRequestHeader("Authorization", "Bearer " + TwitchManager.Current.Authentication.oauth.Substring(6));
				yield return req.SendWebRequest();
				if (req.result != UnityWebRequest.Result.Success)
				{
					Log.Warning("Could not get data from Twitch 'User' endpoint");
				}
				else
				{
					TwitchUserDataContainer twitchUserDataContainer = JsonConvert.DeserializeObject<TwitchUserDataContainer>(req.downloadHandler.text);
					displayName = twitchUserDataContainer.data[0].display_name;
					TwitchManager.Current.BroadcasterType = (broadcaster_type = twitchUserDataContainer.data[0].broadcaster_type);
				}
			}
			using UnityWebRequest req = UnityWebRequest.Get("https://2v3d0ewjcg.execute-api.us-east-1.amazonaws.com/prod/allowlist?displayName=" + displayName);
			yield return req.SendWebRequest();
			if (req.result != UnityWebRequest.Result.Success)
			{
				Log.Warning("InBeta Check Failed: " + req.downloadHandler.text);
			}
			else
			{
				InBetaResponse inBetaResponse = JsonConvert.DeserializeObject<InBetaResponse>(req.downloadHandler.text);
				inBeta = inBetaResponse.inBeta;
				Log.Out($"inBeta: {inBeta}");
			}
		}
		if (inBeta)
		{
			List<string> list = (from c in TwitchActionManager.Current.CategoryList.GetRange(1, 5)
				select c.DisplayName).ToList();
			list.Add("Other");
			List<string> players = new List<string>();
			if (TwitchManager.Current.LocalPlayer != null && TwitchManager.Current.LocalPlayer.Party != null)
			{
				foreach (EntityPlayer member in TwitchManager.Current.LocalPlayer.Party.MemberList)
				{
					if (!(member is EntityPlayerLocal))
					{
						players.Add(member.EntityName);
					}
				}
				players.Insert(0, displayName);
			}
			using (UnityWebRequest req = UnityWebRequest.Put("https://2v3d0ewjcg.execute-api.us-east-1.amazonaws.com/prod/set-developer-config", constructDevConfigContent(list, players)))
			{
				req.SetRequestHeader("Content-Type", "application/json");
				req.SetRequestHeader("Client-Id", "k6ji189bf7i4ge8il4iczzw7kpgmjt");
				req.SetRequestHeader("Authorization", "Bearer " + JWT);
				yield return req.SendWebRequest();
				if (req.result != UnityWebRequest.Result.Success)
				{
					Log.Warning("Failed to set the extension configuration: " + req.downloadHandler.text);
				}
				else
				{
					Log.Out("Extension Configuration set successfully");
				}
			}
			using (UnityWebRequest req = UnityWebRequest.Put("https://api.twitch.tv/helix/extensions/configurations", constructBroadcasterConfigContent(status, cmds)))
			{
				req.SetRequestHeader("Content-Type", "application/json");
				req.SetRequestHeader("Client-Id", "k6ji189bf7i4ge8il4iczzw7kpgmjt");
				req.SetRequestHeader("Authorization", "Bearer " + JWT);
				yield return req.SendWebRequest();
				if (req.result != UnityWebRequest.Result.Success)
				{
					Log.Warning($"Failed to set the extension configuration: {req.downloadHandler.data}");
				}
				else
				{
					Log.Out("Extension Configuration set successfully");
				}
			}
			yield return notifyStatusChange(constructStatusMessage(status, cmds, players));
		}
		else
		{
			Log.Warning("user is not a part of the extension beta. Cannot set configs.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator notifyStatusChange(string body)
	{
		using UnityWebRequest request = UnityWebRequest.Put("https://api.twitch.tv/helix/extensions/pubsub", body);
		request.method = "POST";
		request.SetRequestHeader("Content-Type", "application/json");
		request.SetRequestHeader("Client-Id", "k6ji189bf7i4ge8il4iczzw7kpgmjt");
		request.SetRequestHeader("Authorization", "Bearer " + JWT);
		yield return request.SendWebRequest();
		if (request.result != UnityWebRequest.Result.Success)
		{
			Log.Warning("Failed to broadcast status change: " + request.downloadHandler.text);
		}
		else
		{
			Log.Out("Status Change Pubsub Message sent successfully");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string constructDevConfigContent(List<string> eventTypes, List<string> players)
	{
		return JsonConvert.SerializeObject(new SetDevConfigRequestData
		{
			actionTypes = eventTypes,
			players = players
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string constructBroadcasterConfigContent(string status, string cmds)
	{
		return JsonConvert.SerializeObject(new SetConfigRequestData
		{
			extension_id = "k6ji189bf7i4ge8il4iczzw7kpgmjt",
			segment = "broadcaster",
			broadcaster_id = userId,
			version = "0.0.1",
			content = JsonConvert.SerializeObject(new ConfigContent
			{
				o = opaqueId,
				d = displayName,
				l = Array.FindIndex(Localization.knownLanguages, [PublicizedFrom(EAccessModifier.Internal)] (string l) => l == Localization.language).ToString(),
				s = status,
				c = cmds
			})
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string constructStatusMessage(string status, string cmds, List<string> party)
	{
		return JsonConvert.SerializeObject(new PubSubStatusRequestData
		{
			broadcaster_id = userId,
			target = new List<string> { "broadcast" },
			message = JsonConvert.SerializeObject(new PubSubStatusMessage
			{
				opaqueId = opaqueId,
				displayName = displayName,
				language = Array.FindIndex(Localization.knownLanguages, [PublicizedFrom(EAccessModifier.Internal)] (string l) => l == Localization.language).ToString(),
				status = status,
				commands = cmds,
				party = party,
				actionTypes = (from c in TwitchActionManager.Current.CategoryList.GetRange(1, 5)
					select c.DisplayName).ToList()
			})
		});
	}
}
