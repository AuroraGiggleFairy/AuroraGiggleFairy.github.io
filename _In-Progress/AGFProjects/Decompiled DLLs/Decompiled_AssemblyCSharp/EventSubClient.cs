using System;
using System.Collections;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class EventSubClient
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ClientWebSocket ws;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Uri twitchWsUri = new Uri("wss://eventsub.wss.twitch.tv/ws");

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine receiveCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRunning;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sessionID = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string broadcasterUserID;

	[PublicizedFrom(EAccessModifier.Private)]
	public string accessToken;

	[PublicizedFrom(EAccessModifier.Private)]
	public string clientId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cleanedUp;

	public event Action<JObject> OnEventReceived;

	public EventSubClient(string broadcasterID, string accessToken, string clientId)
	{
		broadcasterUserID = broadcasterID;
		this.accessToken = accessToken;
		this.clientId = clientId;
	}

	public void Connect()
	{
		if (!isRunning)
		{
			ws = new ClientWebSocket();
			ws.ConnectAsync(twitchWsUri, CancellationToken.None).Wait();
			Log.Out("Connected to Twitch EventSub WebSocket");
			isRunning = true;
			receiveCoroutine = GameManager.Instance.StartCoroutine(ReceiveLoopCoroutine());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ReceiveLoopCoroutine()
	{
		byte[] buffer = new byte[8192];
		while (isRunning && ws.State == WebSocketState.Open)
		{
			ArraySegment<byte> segment = new ArraySegment<byte>(buffer);
			Task<WebSocketReceiveResult> receiveTask = ws.ReceiveAsync(segment, CancellationToken.None);
			while (!receiveTask.IsCompleted)
			{
				yield return null;
			}
			WebSocketReceiveResult result = receiveTask.Result;
			if (result.MessageType == WebSocketMessageType.Close)
			{
				Log.Out("WebSocket closed by server.");
				isRunning = false;
				break;
			}
			StringBuilder messageBuilder = new StringBuilder();
			messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
			while (!result.EndOfMessage)
			{
				receiveTask = ws.ReceiveAsync(segment, CancellationToken.None);
				while (!receiveTask.IsCompleted)
				{
					yield return null;
				}
				result = receiveTask.Result;
				messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
			}
			string text = messageBuilder.ToString();
			Log.Out("Received raw message: " + text);
			HandleMessage(text);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleMessage(string json)
	{
		EventSubMessage eventSubMessage = JsonConvert.DeserializeObject<EventSubMessage>(json);
		if (eventSubMessage == null)
		{
			Log.Warning("Invalid EventSub message received.");
			return;
		}
		switch (eventSubMessage.Metadata.MessageType)
		{
		case "session_welcome":
			sessionID = eventSubMessage.Payload["session"]?["id"]?.ToString() ?? string.Empty;
			Log.Out("Session ID: " + sessionID);
			GameManager.Instance.StartCoroutine(SubscribeToEvents());
			break;
		case "notification":
			this.OnEventReceived?.Invoke(eventSubMessage.Payload);
			break;
		case "session_keepalive":
			Log.Out("Received Keep-Alive");
			break;
		case "session_reconnect":
		{
			string text = eventSubMessage.Payload["session"]?["reconnect_url"]?.ToString();
			if (!string.IsNullOrEmpty(text))
			{
				Log.Out("Reconnecting to: " + text);
				Reconnect(text);
			}
			break;
		}
		default:
			Log.Out("Unhandled message type: " + eventSubMessage.Metadata.MessageType);
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SubscribeToEvents()
	{
		yield return CreateEventSubSubscription("channel.subscribe");
		yield return CreateEventSubSubscription("channel.channel_points_custom_reward_redemption.add");
		yield return CreateEventSubSubscription("channel.subscription.message");
		yield return CreateEventSubSubscription("channel.subscription.gift");
		yield return CreateEventSubSubscription("channel.bits.use");
		yield return CreateEventSubSubscription("channel.hype_train.begin");
		yield return CreateEventSubSubscription("channel.hype_train.progress");
		yield return CreateEventSubSubscription("channel.hype_train.end");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator CreateEventSubSubscription(string eventType)
	{
		string s = new JObject
		{
			["type"] = eventType,
			["version"] = (eventType.Contains("hype_train") ? "2" : "1"),
			["condition"] = new JObject { ["broadcaster_user_id"] = broadcasterUserID },
			["transport"] = new JObject
			{
				["method"] = "websocket",
				["session_id"] = sessionID
			}
		}.ToString();
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		UnityWebRequest req = new UnityWebRequest("https://api.twitch.tv/helix/eventsub/subscriptions", "POST");
		req.uploadHandler = new UploadHandlerRaw(bytes);
		req.downloadHandler = new DownloadHandlerBuffer();
		req.SetRequestHeader("Content-Type", "application/json");
		req.SetRequestHeader("Authorization", "Bearer " + accessToken);
		req.SetRequestHeader("Client-Id", clientId);
		yield return req.SendWebRequest();
		if (req.result == UnityWebRequest.Result.Success)
		{
			Log.Out("Successfully subscribed to " + eventType);
			yield break;
		}
		Log.Warning("Failed to subscribe to " + eventType + ": " + req.error + " | " + req.downloadHandler.text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Reconnect(string newUrl)
	{
		if (isRunning)
		{
			Disconnect();
			ws = new ClientWebSocket();
			ws.ConnectAsync(new Uri(newUrl), CancellationToken.None).Wait();
			Log.Out("Reconnected to new WebSocket URL");
			isRunning = true;
			receiveCoroutine = GameManager.Instance.StartCoroutine(ReceiveLoopCoroutine());
		}
	}

	public void Disconnect()
	{
		if (isRunning)
		{
			isRunning = false;
			if (receiveCoroutine != null)
			{
				GameManager.Instance.StopCoroutine(receiveCoroutine);
			}
			if (ws.State == WebSocketState.Open)
			{
				ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None).Wait();
			}
			Log.Out("Disconnected from Twitch EventSub WebSocket");
		}
	}

	public void Cleanup()
	{
		if (!cleanedUp)
		{
			cleanedUp = true;
			Disconnect();
			ws?.Dispose();
			Log.Out("EventSubClient resources cleaned up.");
		}
	}
}
