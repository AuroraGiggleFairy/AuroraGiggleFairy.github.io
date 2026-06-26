using System;
using System.Collections.Generic;
using GameSparks.Api;
using GameSparks.Api.Messages;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using GameSparks.Core;
using UnityEngine;

public class GameSparksTestUI : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<string> myLogQueue = new Queue<string>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string myLog = "";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string fbToken = "accessToken";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string dismissMessageId = "messageId";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int itemHeight = 30;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int itemWidth = 200;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool testing;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool working;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool result;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int counter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int numTest;

	public Texture cursor;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		Application.logMessageReceivedThreaded += HandleLog;
		Screen.orientation = ScreenOrientation.AutoRotation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		GSMessageHandler._AllMessages = HandleGameSparksMessageReceived;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleGameSparksMessageReceived(GSMessage message)
	{
		HandleLog("MSG:" + message.JSONString);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLog(string logString)
	{
		GS.GSPlatform.ExecuteOnMainThread([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			HandleLog(logString, null, LogType.Log);
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLog(string logString, string stackTrace, LogType logType)
	{
		if (myLogQueue.Count > 30)
		{
			myLogQueue.Dequeue();
		}
		myLogQueue.Enqueue(logString);
		myLog = "";
		string[] array = myLogQueue.ToArray();
		foreach (string text in array)
		{
			myLog = myLog + "\n\n" + text;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGUI()
	{
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		GUI.skin.textField.alignment = TextAnchor.MiddleCenter;
		GUI.skin.textArea.alignment = TextAnchor.LowerLeft;
		GUILayout.BeginHorizontal();
		GUILayout.Label(GS.Available ? "AVAILABLE" : "NOT AVAILABLE", GUILayout.Width(200f), GUILayout.Height(30f));
		GUILayout.Label("SDK Version: " + GS.Version.ToString(), GUILayout.Width(200f), GUILayout.Height(30f));
		GUILayout.EndHorizontal();
		GUILayout.Label(GS.Authenticated ? "AUTHENTICATED" : "NOT AUTHENTICATED", GUILayout.Width(200f), GUILayout.Height(30f));
		if (GUILayout.Button("Clear Log", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			myLog = "";
			myLogQueue.Clear();
		}
		if (GUILayout.Button("Logout", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			GS.Reset();
		}
		if (GUILayout.Button("Disconnect", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			GS.Disconnect();
		}
		if (!GS.Available && GUILayout.Button("Reconnect", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			GS.Reconnect();
		}
		if (GUILayout.Button("DeviceAuthenticationRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new DeviceAuthenticationRequest().Send([PublicizedFrom(EAccessModifier.Private)] (AuthenticationResponse response) =>
			{
				HandleLog("DeviceAuthenticationRequest.JSON:" + response.JSONString);
				HandleLog("DeviceAuthenticationRequest.HasErrors:" + response.HasErrors);
				HandleLog("DeviceAuthenticationRequest.UserId:" + response.UserId);
			});
		}
		if (GUILayout.Button("durableAccountDetailsRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new AccountDetailsRequest().SetDurable(durable: true).Send(null);
		}
		if (GUILayout.Button("accountDetailsRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new AccountDetailsRequest().Send([PublicizedFrom(EAccessModifier.Private)] (AccountDetailsResponse response) =>
			{
				HandleLog("AccountDetailsRequest.UserId:" + response.UserId);
			});
		}
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("facebookConnectRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new FacebookConnectRequest().SetAccessToken(fbToken).Send([PublicizedFrom(EAccessModifier.Private)] (AuthenticationResponse response) =>
			{
				HandleLog("FacebookConnectRequest.HasErrors:" + response.HasErrors);
				HandleLog("FacebookConnectRequest.UserId:" + response.UserId);
			});
		}
		fbToken = GUILayout.TextField(fbToken, GUILayout.Width(200f), GUILayout.Height(30f));
		GUILayout.EndHorizontal();
		if (GUILayout.Button("listAchievementsRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new ListAchievementsRequest().Send([PublicizedFrom(EAccessModifier.Private)] (ListAchievementsResponse response) =>
			{
				foreach (ListAchievementsResponse._Achievement achievement in response.Achievements)
				{
					HandleLog("ListAchievementsRequest:shortCode:" + achievement.ShortCode);
				}
			});
		}
		if (GUILayout.Button("listGameFriendsRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new ListGameFriendsRequest().Send([PublicizedFrom(EAccessModifier.Private)] (ListGameFriendsResponse response) =>
			{
				foreach (ListGameFriendsResponse._Player friend in response.Friends)
				{
					HandleLog("ListGameFriendsRequest.DisplayName:" + friend.DisplayName);
				}
			});
		}
		if (GUILayout.Button("listVirtualGoodsRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new ListVirtualGoodsRequest().Send([PublicizedFrom(EAccessModifier.Private)] (ListVirtualGoodsResponse response) =>
			{
				foreach (ListVirtualGoodsResponse._VirtualGood virtualGood in response.VirtualGoods)
				{
					HandleLog("ListVirtualGoodsRequest.Description:" + virtualGood.Description);
				}
			});
		}
		if (GUILayout.Button("listChallengeTypeRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new ListChallengeTypeRequest().Send([PublicizedFrom(EAccessModifier.Private)] (ListChallengeTypeResponse response) =>
			{
				foreach (ListChallengeTypeResponse._ChallengeType challengeTemplate in response.ChallengeTemplates)
				{
					HandleLog("ListAchievementsRequest.Challenge:" + challengeTemplate.ChallengeShortCode);
				}
			});
		}
		if (GUILayout.Button("authenticationRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new AuthenticationRequest().SetUserName("gabs").SetPassword("gabs").Send([PublicizedFrom(EAccessModifier.Internal)] (AuthenticationResponse AR) =>
			{
				if (AR.HasErrors)
				{
					Debug.Log("Didnt Work");
				}
				else
				{
					Debug.Log("Worked");
				}
			});
		}
		if (GUILayout.Button("leaderboardData", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new LeaderboardDataRequest().SetLeaderboardShortCode("HSCORE").SetEntryCount(10L).Send([PublicizedFrom(EAccessModifier.Internal)] (LeaderboardDataResponse leadResponse) =>
			{
				if (leadResponse.HasErrors)
				{
					Debug.Log("Leaderboard data retrieval failed ...");
					return;
				}
				Debug.Log("Leaderboard data retrieval succeeded ..." + leadResponse);
				foreach (LeaderboardDataResponse._LeaderboardData datum in leadResponse.Data)
				{
					Debug.Log("Rank: " + datum.Rank + "    UserName: " + datum.UserName + "    Score: " + datum.GetNumberValue("SCORE"));
				}
			});
		}
		if (GUILayout.Button("listMessageRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new ListMessageRequest().Send([PublicizedFrom(EAccessModifier.Private)] (ListMessageResponse response) =>
			{
				foreach (GSData message in response.MessageList)
				{
					HandleLog("ListMessageRequest.MessageList:" + message.GetString("messageId"));
				}
			});
		}
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("dismissMessageRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new DismissMessageRequest().SetMessageId(dismissMessageId).Send([PublicizedFrom(EAccessModifier.Private)] (DismissMessageResponse response) =>
			{
				HandleLog("DismissMessageRequest.HasErrors:" + response.HasErrors);
			});
		}
		dismissMessageId = GUILayout.TextField(dismissMessageId, GUILayout.Width(200f), GUILayout.Height(30f));
		GUILayout.EndHorizontal();
		if (GUILayout.Button("TRACE " + (GS.TraceMessages ? "ON" : "OFF"), GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			GS.TraceMessages = !GS.TraceMessages;
		}
		GUI.TextArea(new Rect(420f, 5f, Screen.width - 425, Screen.height - 10), myLog);
	}

	public void Update()
	{
	}
}
