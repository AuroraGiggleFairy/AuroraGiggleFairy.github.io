using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Twitch.PubSub;

public class TwitchPubSub
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum MessageTypes
	{
		Standard,
		HypeStart
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ClientWebSocket socket;

	[PublicizedFrom(EAccessModifier.Private)]
	public System.Timers.Timer pingTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public System.Timers.Timer pongTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public System.Timers.Timer reconnectTimer = new System.Timers.Timer();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pingAcknowledged;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool reconnect = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchTopic[] topics;

	[PublicizedFrom(EAccessModifier.Private)]
	public CancellationTokenSource cts;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TimeSpan[] _ReconnectTimeouts = new TimeSpan[6]
	{
		TimeSpan.FromSeconds(1.0),
		TimeSpan.FromSeconds(5.0),
		TimeSpan.FromSeconds(10.0),
		TimeSpan.FromSeconds(30.0),
		TimeSpan.FromMinutes(1.0),
		TimeSpan.FromMinutes(5.0)
	};

	public event EventHandler<PubSubBitRedemptionMessage.BitRedemptionData> OnBitsRedeemed;

	public event EventHandler<PubSubSubscriptionRedemptionMessage> OnSubscriptionRedeemed;

	public event EventHandler<PubSubChannelPointMessage.ChannelRedemptionData> OnChannelPointsRedeemed;

	public event EventHandler<PubSubGoalMessage.Goal> OnGoalAchieved;

	public void Connect(string userID)
	{
		if (cts != null)
		{
			cts.Cancel();
		}
		cts = new CancellationTokenSource();
		Task.Run([PublicizedFrom(EAccessModifier.Internal)] () => StartAsync(new TwitchTopic[5]
		{
			TwitchTopic.ChannelPoints(userID),
			TwitchTopic.Bits(userID),
			TwitchTopic.Subscription(userID),
			TwitchTopic.HypeTrain(userID),
			TwitchTopic.CreatorGoal(userID)
		}, cts.Token));
	}

	public void Disconnect()
	{
		cts.Cancel();
		reconnect = false;
	}

	public async Task StartAsync(TwitchTopic[] newTopics, CancellationToken token)
	{
		topics = newTopics;
		reconnect = false;
		pingTimer = new System.Timers.Timer(TimeSpan.FromSeconds(30.0).TotalMilliseconds);
		pingTimer.Elapsed += PingTimer_Elapsed;
		pingTimer.Start();
		await StartListening(newTopics);
		while (!token.IsCancellationRequested)
		{
			byte[] array = new byte[1024];
			ArraySegment<byte> messageBuffer = new ArraySegment<byte>(array);
			StringBuilder completeMessage = new StringBuilder();
			WebSocketReceiveResult webSocketReceiveResult = await socket.ReceiveAsync(messageBuffer, token);
			completeMessage.Append(Encoding.UTF8.GetString(messageBuffer.Array));
			while (!webSocketReceiveResult.EndOfMessage)
			{
				array = new byte[1024];
				messageBuffer = new ArraySegment<byte>(array);
				webSocketReceiveResult = await socket.ReceiveAsync(messageBuffer, token);
				completeMessage.Append(Encoding.UTF8.GetString(messageBuffer.Array));
			}
			if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close)
			{
				reconnect = true;
				break;
			}
			try
			{
				string text = completeMessage.ToString();
				HandleMessage(text, text.Contains("hype-train-start") ? MessageTypes.HypeStart : MessageTypes.Standard);
			}
			catch (Exception ex)
			{
				Debug.LogWarning(completeMessage.ToString());
				Debug.LogWarning(ex.ToString());
				if (ex.InnerException != null)
				{
					Debug.LogWarning(ex.InnerException.ToString());
				}
				reconnect = true;
			}
			if (!reconnect)
			{
				continue;
			}
			if (!_ReconnectTimeouts.Any([PublicizedFrom(EAccessModifier.Internal)] (TimeSpan t) => t.TotalMilliseconds == reconnectTimer.Interval))
			{
				reconnectTimer.Interval = _ReconnectTimeouts[0].TotalMilliseconds;
			}
			else if (_ReconnectTimeouts.Last().TotalMilliseconds == reconnectTimer.Interval)
			{
				reconnect = false;
			}
			else
			{
				for (int num = 0; num < _ReconnectTimeouts.Length; num++)
				{
					if (_ReconnectTimeouts[num].TotalMilliseconds == reconnectTimer.Interval)
					{
						reconnectTimer.Interval = _ReconnectTimeouts[num + 1].TotalMilliseconds;
						break;
					}
				}
			}
			await Task.Delay((int)reconnectTimer.Interval);
			break;
		}
		if (reconnect)
		{
			Task.Run([PublicizedFrom(EAccessModifier.Internal)] () => StartAsync(newTopics, token));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleMessage(string receivedMessage, MessageTypes msgType)
	{
		switch (msgType)
		{
		case MessageTypes.Standard:
		{
			JObject jObject = JObject.Parse(receivedMessage);
			string text = jObject["type"].Value<string>();
			if ((!(text == "RESPONSE") || !(jObject["error"].Value<string>() != "")) && !(text == "RESPONSE") && !HandlePongMessage(receivedMessage) && !HandleReconnectMessage(receivedMessage))
			{
				HandleRedemptionsMessages(receivedMessage);
			}
			break;
		}
		case MessageTypes.HypeStart:
			TwitchManager.Current.StartHypeTrain();
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public async Task StartListening(IEnumerable<TwitchTopic> topics)
	{
		socket = new ClientWebSocket();
		PubSubListenMessage msg = new PubSubListenMessage();
		msg.data = new PubSubListenMessage.PubSubListenData
		{
			auth_token = TwitchManager.Current.Authentication.oauth.Substring(6),
			topics = topics.Select([PublicizedFrom(EAccessModifier.Internal)] (TwitchTopic t) => t.TopicString).ToArray()
		};
		await socket.ConnectAsync(new Uri("wss://pubsub-edge.twitch.tv"), CancellationToken.None).ContinueWith([PublicizedFrom(EAccessModifier.Internal)] (Task t) => SendMessageOnSocket(JsonConvert.SerializeObject(msg)));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PingTimer_Elapsed(object sender, ElapsedEventArgs e)
	{
		string message = "{ \"type\": \"PING\" }";
		SendMessageOnSocket(message).GetAwaiter().GetResult();
		pongTimer = new System.Timers.Timer(TimeSpan.FromSeconds(10.0).TotalMilliseconds);
		pongTimer.Elapsed += PongTimer_Elapsed;
		pongTimer.Start();
		pingAcknowledged = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PongTimer_Elapsed(object sender, ElapsedEventArgs e)
	{
		if (!pingAcknowledged)
		{
			reconnect = true;
			pongTimer.Dispose();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Task SendMessageOnSocket(string message)
	{
		if (socket.State != WebSocketState.Open)
		{
			return Task.CompletedTask;
		}
		byte[] bytes = Encoding.ASCII.GetBytes(message);
		return socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HandlePongMessage(string message)
	{
		if (message.Contains("\"PONG\""))
		{
			pingAcknowledged = true;
			pongTimer.Stop();
			pongTimer.Dispose();
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HandleReconnectMessage(string message)
	{
		if (message.Contains("\"RECONNECT\""))
		{
			reconnect = true;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HandleRedemptionsMessages(string message)
	{
		JObject jObject = JObject.Parse(message);
		if (jObject["type"].Value<string>() == "MESSAGE")
		{
			string text = jObject["data"]["topic"].Value<string>();
			if (text.StartsWith("channel-points-channel-v1"))
			{
				string message2 = jObject["data"]["message"].Value<string>();
				PubSubChannelPointMessage pubSubChannelPointMessage = null;
				try
				{
					pubSubChannelPointMessage = PubSubChannelPointMessage.Deserialize(message2);
				}
				catch (Exception ex)
				{
					Debug.LogError(ex.ToString());
					Debug.LogError(message2);
				}
				if (this.OnChannelPointsRedeemed != null && pubSubChannelPointMessage != null)
				{
					this.OnChannelPointsRedeemed(null, pubSubChannelPointMessage.data);
				}
				return true;
			}
			if (text.StartsWith("channel-bits-events"))
			{
				string message3 = jObject["data"]["message"].Value<string>();
				PubSubBitRedemptionMessage pubSubBitRedemptionMessage = null;
				try
				{
					pubSubBitRedemptionMessage = PubSubBitRedemptionMessage.Deserialize(message3);
				}
				catch (Exception ex2)
				{
					Debug.LogError(ex2.ToString());
					Debug.LogError(message3);
				}
				if (this.OnBitsRedeemed != null && pubSubBitRedemptionMessage != null)
				{
					this.OnBitsRedeemed(null, pubSubBitRedemptionMessage.data);
				}
				return true;
			}
			if (text.StartsWith("channel-subscribe-events"))
			{
				string message4 = jObject["data"]["message"].Value<string>();
				PubSubSubscriptionRedemptionMessage pubSubSubscriptionRedemptionMessage = null;
				try
				{
					pubSubSubscriptionRedemptionMessage = PubSubSubscriptionRedemptionMessage.Deserialize(message4);
				}
				catch (Exception ex3)
				{
					Debug.LogError(ex3.ToString());
					Debug.LogError(message4);
				}
				if (this.OnSubscriptionRedeemed != null && pubSubSubscriptionRedemptionMessage != null)
				{
					this.OnSubscriptionRedeemed(null, pubSubSubscriptionRedemptionMessage);
				}
				return true;
			}
			if (text.StartsWith("creator-goals-events"))
			{
				string message5 = jObject["data"]["message"].Value<string>();
				PubSubGoalMessage pubSubGoalMessage = null;
				try
				{
					pubSubGoalMessage = PubSubGoalMessage.Deserialize(message5);
				}
				catch (Exception ex4)
				{
					Debug.LogError(ex4.ToString());
					Debug.LogError(message5);
				}
				if (pubSubGoalMessage.type == "goal_achieved" && this.OnGoalAchieved != null && pubSubGoalMessage != null)
				{
					this.OnGoalAchieved(null, pubSubGoalMessage.data.goal);
				}
				return true;
			}
			if (text.StartsWith("hype-train-events-v1"))
			{
				try
				{
					string text2 = jObject["data"]["message"].ToString();
					Debug.LogWarning(text2);
					if (text2.Contains("hype-train-start"))
					{
						TwitchManager.Current.StartHypeTrain();
					}
					else if (text2.Contains("hype-train-level-up"))
					{
						TwitchManager.Current.IncrementHypeTrainLevel();
					}
					else if (text2.Contains("hype-train-end"))
					{
						TwitchManager.Current.EndHypeTrain();
					}
				}
				catch (Exception ex5)
				{
					Debug.LogWarning("Hype Train Error: " + message);
					Debug.LogWarning("Hype Train Exception: " + ex5.ToString());
				}
				return true;
			}
		}
		return false;
	}

	public void Cleanup()
	{
		if (pingTimer != null)
		{
			pingTimer.Dispose();
		}
		if (pongTimer != null)
		{
			pongTimer.Dispose();
		}
		if (socket != null)
		{
			socket.Dispose();
		}
	}
}
