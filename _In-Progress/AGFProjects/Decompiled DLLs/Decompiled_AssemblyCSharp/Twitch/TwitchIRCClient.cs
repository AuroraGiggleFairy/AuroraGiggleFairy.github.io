using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Twitch;

public class TwitchIRCClient
{
	public class TwitchChatMessage
	{
		public enum MessageTypes
		{
			Invalid = -1,
			Message,
			Output,
			Authenticated,
			Raid,
			Charity
		}

		public bool isMod;

		public bool isVIP;

		public bool isSub;

		public bool isBroadcaster;

		public string UserName;

		public int UserID;

		public string UserNameColor = "FFFFFF";

		public string Message;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public virtual MessageTypes MessageType
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public TwitchChatMessage(string message)
		{
			if (message.IndexOf(TWITCH_SYSTEM_STRING) != -1)
			{
				if (message.StartsWith(TWITCH_CONNECTION_STRING))
				{
					Message = message;
					MessageType = MessageTypes.Authenticated;
					return;
				}
				int num = -1;
				int num2 = -1;
				if (message.Contains(PRIV_MSG_STRING))
				{
					string[] array = message.Split(';');
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i].StartsWith("@badge-info"))
						{
							if (array[i].Length >= 15 && (array[i][12] == 'f' || array[i][12] == 's'))
							{
								isSub = true;
							}
						}
						else if (array[i].StartsWith("badges"))
						{
							if (array[i].Contains("broadcaster"))
							{
								isSub = true;
								isMod = true;
								isVIP = true;
								isBroadcaster = true;
							}
							else if (array[i].Contains("vip"))
							{
								isVIP = true;
							}
						}
						else if (array[i].StartsWith("mod"))
						{
							if (array[i][4] == '1')
							{
								isMod = true;
							}
						}
						else if (array[i].StartsWith("user-type"))
						{
							message = message.Substring(message.IndexOf('@', 1) + 1);
						}
						else if (array[i].StartsWith("user-id"))
						{
							UserID = Convert.ToInt32(array[i].Substring(8));
						}
						else if (array[i].StartsWith("room-id"))
						{
							num2 = Convert.ToInt32(array[i].Substring(8));
						}
						else if (array[i].StartsWith("source-room-id"))
						{
							num = Convert.ToInt32(array[i].Substring(15));
						}
						else if (array[i].StartsWith("color"))
						{
							if (array[i].Length > 7)
							{
								UserNameColor = array[i].Substring(7);
							}
						}
						else if (array[i].StartsWith("reply-parent-msg-body"))
						{
							MessageType = MessageTypes.Invalid;
							return;
						}
					}
					if (num2 != num && num != -1)
					{
						MessageType = MessageTypes.Invalid;
						return;
					}
					message.IndexOf(PRIV_MSG_STRING_PARSE);
					string userName = message[..message.IndexOf('.', 1)];
					int num3 = message.IndexOf(":");
					message = message.Substring(num3 + 1);
					UserName = userName;
					Message = message;
					MessageType = MessageTypes.Message;
					return;
				}
				if (message.Contains(MSG_RAID_STRING))
				{
					string[] array2 = message.Split(';');
					for (int j = 0; j < array2.Length; j++)
					{
						if (array2[j].StartsWith("msg-param-displayName"))
						{
							UserName = array2[j].Substring(22);
						}
						else if (array2[j].StartsWith("msg-param-viewerCount"))
						{
							Message = array2[j].Substring(22);
						}
						else if (array2[j].StartsWith("user-id"))
						{
							UserID = Convert.ToInt32(array2[j].Substring(8));
						}
					}
					MessageType = MessageTypes.Raid;
					return;
				}
				if (message.Contains(MSG_CHARITY_STRING))
				{
					string[] array3 = message.Split(';');
					for (int k = 0; k < array3.Length; k++)
					{
						if (array3[k].StartsWith("display-name"))
						{
							UserName = array3[k].Substring(13);
						}
						else if (array3[k].StartsWith("msg-param-donation-amount"))
						{
							Message = array3[k].Substring(26);
						}
						else if (array3[k].StartsWith("user-id"))
						{
							UserID = Convert.ToInt32(array3[k].Substring(8));
						}
					}
					MessageType = MessageTypes.Charity;
					return;
				}
			}
			Message = message;
			MessageType = MessageTypes.Output;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string userName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string channel;

	[PublicizedFrom(EAccessModifier.Private)]
	public string password;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ip;

	[PublicizedFrom(EAccessModifier.Private)]
	public int port;

	[PublicizedFrom(EAccessModifier.Private)]
	public TcpClient tcpClient;

	[PublicizedFrom(EAccessModifier.Private)]
	public StreamReader inputStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public StreamWriter outputStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float pingMaxTimer = 100f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float PingTimer = pingMaxTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pingTimerRunning;

	public List<string> outputQueue = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static string TWITCH_SYSTEM_STRING = "tmi.twitch.tv";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string TWITCH_CONNECTION_STRING = ":tmi.twitch.tv 001";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PRIV_MSG_STRING = "PRIVMSG";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PRIV_MSG_STRING_PARSE = "PRIVMSG #";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string MSG_RAID_STRING = "msg-id=raid";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string MSG_CHARITY_STRING = "msg-id=charitydonation";

	public bool IsConnected => tcpClient.Connected;

	public TwitchIRCClient(string ip, int port, string channel, string password)
	{
		userName = channel;
		this.password = password;
		this.channel = channel;
		this.ip = ip;
		this.port = port;
		Reconnect();
	}

	public void Reconnect()
	{
		tcpClient = new TcpClient(ip, port);
		inputStream = new StreamReader(tcpClient.GetStream());
		outputStream = new StreamWriter(tcpClient.GetStream());
		outputStream.WriteLine("PASS " + password);
		outputStream.WriteLine("NICK " + userName);
		outputStream.WriteLine("JOIN #" + channel);
		pingTimerRunning = true;
		outputStream.Flush();
	}

	public void Disconnect()
	{
		if (tcpClient != null)
		{
			tcpClient.Close();
		}
		if (inputStream != null)
		{
			inputStream.Close();
		}
		if (outputStream != null)
		{
			outputStream.Close();
		}
	}

	public bool Update(float deltaTime)
	{
		if (pingTimerRunning)
		{
			PingTimer -= deltaTime;
			if (PingTimer <= 0f)
			{
				if (tcpClient.Connected)
				{
					SendIrcMessage("PING irc.twitch.tv", useQueue: false);
				}
				else
				{
					Reconnect();
					pingTimerRunning = false;
				}
				PingTimer = 250f;
			}
		}
		if (outputQueue.Count > 0)
		{
			outputStream.WriteLine(outputQueue[0]);
			outputQueue.RemoveAt(0);
			outputStream.Flush();
		}
		return true;
	}

	public void SendIrcMessage(string message, bool useQueue)
	{
		if (!tcpClient.Connected)
		{
			Reconnect();
		}
		if (useQueue)
		{
			outputQueue.Add(message);
			return;
		}
		outputStream.WriteLine(message);
		outputStream.Flush();
	}

	public void SendIrcMessages(List<string> messages, bool useQueue)
	{
		if (useQueue)
		{
			outputQueue.AddRange(messages);
			return;
		}
		for (int i = 0; i < messages.Count; i++)
		{
			outputStream.WriteLine(messages[i]);
		}
		outputStream.Flush();
	}

	public void SendChannelMessage(string message, bool useQueue)
	{
		if (useQueue)
		{
			outputQueue.Add("PRIVMSG #" + userName + " :/me " + message);
			return;
		}
		outputStream.WriteLine("PRIVMSG #" + userName + " :/me " + message);
		outputStream.Flush();
	}

	public void SendChannelMessages(List<string> messages, bool useQueue)
	{
		if (useQueue)
		{
			for (int i = 0; i < messages.Count; i++)
			{
				outputQueue.Add("PRIVMSG #" + userName + " :/me " + messages[i]);
			}
			return;
		}
		for (int j = 0; j < messages.Count; j++)
		{
			outputStream.WriteLine("PRIVMSG #" + userName + " :/me " + messages[j]);
		}
		outputStream.Flush();
	}

	public bool AvailableMessage()
	{
		return tcpClient.Available > 0;
	}

	public TwitchChatMessage ReadMessage()
	{
		return ParseMessage();
	}

	public TwitchChatMessage ParseMessage()
	{
		return new TwitchChatMessage(inputStream.ReadLine());
	}

	public void SendChatMessage(string message)
	{
		SendIrcMessage(string.Format(":{0}!{0}@{0}.tmi.twitch.tv PRIVMSG #{1} :{2}", userName, channel, message), useQueue: true);
	}
}
