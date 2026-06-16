using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class TelnetConnection : ConsoleConnectionAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int MAX_CHARS_PER_CONVERSION = 64;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BlockingQueue<string> toClientQueue = new BlockingQueue<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly TelnetConsole telnet;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool authenticated;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool authEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly TcpClient client;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly EndPoint endpoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool closed;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedDescription;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly NetworkStream clientStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StreamReader reader;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringBuilder readStringBuilder = new StringBuilder();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly char[] charBuffer = new char[64];

	[PublicizedFrom(EAccessModifier.Private)]
	public bool closeConnection;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly byte[] byteBuffer = new byte[256];

	public bool IsClosed => closed;

	public bool IsAuthenticated
	{
		get
		{
			if (authEnabled)
			{
				return authenticated;
			}
			return true;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int EndPointHash { get; }

	public bool ConnectionUsable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (client.Connected)
			{
				return !closed;
			}
			return false;
		}
	}

	public TelnetConnection(TelnetConsole _owner, TcpClient _client, int _addressHash, bool _authEnabled)
	{
		telnet = _owner;
		authEnabled = _authEnabled;
		client = _client;
		endpoint = _client.Client.RemoteEndPoint;
		EndPointHash = _addressHash;
		Log.Out("Telnet connection from: " + endpoint);
		clientStream = _client.GetStream();
		reader = new StreamReader(clientStream, Encoding.UTF8);
		ThreadManager.StartThread("TelnetClient_" + endpoint, null, HandlerThread, ThreadEnd);
		if (_authEnabled)
		{
			toClientQueue.Enqueue("Please enter password:");
		}
		else
		{
			LoginMessage();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LoginMessage()
	{
		toClientQueue.Enqueue("*** Connected with 7DTD server.");
		toClientQueue.Enqueue("*** Server version: " + Constants.cVersionInformation.LongString + " Compatibility Version: " + Constants.cVersionInformation.LongStringNoBuild);
		toClientQueue.Enqueue(string.Empty);
		toClientQueue.Enqueue("Server IP:   " + (string.IsNullOrEmpty(GamePrefs.GetString(EnumGamePrefs.ServerIP)) ? "Any" : GamePrefs.GetString(EnumGamePrefs.ServerIP)));
		toClientQueue.Enqueue("Server port: " + GamePrefs.GetInt(EnumGamePrefs.ServerPort));
		toClientQueue.Enqueue("Max players: " + GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount));
		toClientQueue.Enqueue("Game mode:   " + GamePrefs.GetString(EnumGamePrefs.GameMode));
		toClientQueue.Enqueue("World:       " + GamePrefs.GetString(EnumGamePrefs.GameWorld));
		toClientQueue.Enqueue("Game name:   " + GamePrefs.GetString(EnumGamePrefs.GameName));
		toClientQueue.Enqueue("Difficulty:  " + GamePrefs.GetInt(EnumGamePrefs.GameDifficulty));
		toClientQueue.Enqueue(string.Empty);
		toClientQueue.Enqueue("Press 'help' to get a list of all commands. Press 'exit' to end session.");
		toClientQueue.Enqueue(string.Empty);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int HandlerThread(ThreadManager.ThreadInfo _tInfo)
	{
		if (!ConnectionUsable || closeConnection)
		{
			return -1;
		}
		try
		{
			if (!handleReading())
			{
				return -1;
			}
			handleWriting();
		}
		catch (IOException ex)
		{
			if (ex.InnerException is SocketException { SocketErrorCode: SocketError.ConnectionAborted })
			{
				Log.Warning("Connection closed by host in TelnetClient_" + endpoint);
				return -1;
			}
			Log.Error("IOException in TelnetClient_" + endpoint?.ToString() + ": " + ex.Message);
			Log.Exception(ex);
			return -1;
		}
		return 25;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool handleReading()
	{
		int num;
		while (ConnectionUsable && clientStream.CanRead && client.Available > 0 && (num = reader.Read(charBuffer, 0, charBuffer.Length)) > 0)
		{
			for (int i = 0; i < num; i++)
			{
				char c = charBuffer[i];
				if (c == '\r' || c == '\n')
				{
					if (!submitInput())
					{
						return false;
					}
				}
				else
				{
					readStringBuilder.Append(c);
				}
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool submitInput()
	{
		readStringBuilder.Trim();
		if (readStringBuilder.Length <= 0)
		{
			return true;
		}
		string text = readStringBuilder.ToString();
		if (!IsAuthenticated)
		{
			authenticate(text);
		}
		else
		{
			if (text.EqualsCaseInsensitive("exit"))
			{
				return false;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteAsync(text, this);
		}
		readStringBuilder.Length = 0;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void authenticate(string _line)
	{
		if (!_line.Equals(GamePrefs.GetString(EnumGamePrefs.TelnetPassword)))
		{
			if (telnet.RegisterFailedLogin(this))
			{
				toClientQueue.Enqueue("Password incorrect, please enter password:");
				return;
			}
			toClientQueue.Enqueue("Too many failed login attempts!");
			closeConnection = true;
		}
		else
		{
			authenticated = true;
			toClientQueue.Enqueue("Logon successful.");
			toClientQueue.Enqueue(string.Empty);
			toClientQueue.Enqueue(string.Empty);
			toClientQueue.Enqueue(string.Empty);
			LoginMessage();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleWriting()
	{
		while (ConnectionUsable && clientStream.CanWrite && toClientQueue.HasData())
		{
			string text = toClientQueue.Dequeue();
			if (text == null)
			{
				clientStream.WriteByte(0);
				continue;
			}
			int num;
			for (int i = 0; i < text.Length; i += num)
			{
				num = Math.Min(64, text.Length - i);
				int bytes = Encoding.UTF8.GetBytes(text, i, num, byteBuffer, 0);
				clientStream.Write(byteBuffer, 0, bytes);
			}
			clientStream.WriteByte(13);
			clientStream.WriteByte(10);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ThreadEnd(ThreadManager.ThreadInfo _threadInfo, bool _exitForException)
	{
		Close();
	}

	public void Close(bool _kickedForLogins = false)
	{
		if (!closed)
		{
			closed = true;
			toClientQueue.Close();
			if (client.Connected)
			{
				client.GetStream().Close();
				client.Close();
			}
			telnet.ConnectionClosed(this);
			if (_kickedForLogins)
			{
				Log.Out("Telnet connection closed for too many login attempts: " + endpoint);
			}
			else
			{
				Log.Out("Telnet connection closed: " + endpoint);
			}
		}
	}

	public override void SendLine(string _line)
	{
		if (!closed && IsAuthenticated)
		{
			toClientQueue.Enqueue(_line);
		}
		else
		{
			toClientQueue.Enqueue(null);
		}
	}

	public override void SendLines(List<string> _output)
	{
		for (int i = 0; i < _output.Count; i++)
		{
			SendLine(_output[i]);
		}
	}

	public override void SendLog(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime)
	{
		if (IsLogLevelEnabled(_type))
		{
			SendLine(_formattedMessage);
		}
	}

	public override string GetDescription()
	{
		return cachedDescription ?? (cachedDescription = "Telnet from " + endpoint);
	}
}
