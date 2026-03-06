using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class TelnetConsole : IConsoleServer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class LoginAttempts
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public int count;

		[PublicizedFrom(EAccessModifier.Private)]
		public DateTime lastAttempt = new DateTime(0L);

		public bool LogAttempt()
		{
			lastAttempt = DateTime.Now;
			count++;
			return count < maxLoginAttempts;
		}

		public bool IsBanned()
		{
			if ((DateTime.Now - lastAttempt).TotalSeconds > (double)blockTimeSeconds)
			{
				count = 0;
			}
			return count >= maxLoginAttempts;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int maxLoginAttempts;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int blockTimeSeconds;

	[PublicizedFrom(EAccessModifier.Private)]
	public TcpListener listener;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool authEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<TelnetConnection> connections = new List<TelnetConnection>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<int, LoginAttempts> loginAttemptsPerIP = new Dictionary<int, LoginAttempts>();

	public TelnetConsole()
	{
		try
		{
			int port = GamePrefs.GetInt(EnumGamePrefs.TelnetPort);
			authEnabled = GamePrefs.GetString(EnumGamePrefs.TelnetPassword).Length != 0;
			listener = new TcpListener(authEnabled ? IPAddress.Any : IPAddress.Loopback, port);
			maxLoginAttempts = GamePrefs.GetInt(EnumGamePrefs.TelnetFailedLoginLimit);
			blockTimeSeconds = GamePrefs.GetInt(EnumGamePrefs.TelnetFailedLoginsBlocktime);
			listener.Start();
			listener.BeginAcceptTcpClient(AcceptClient, null);
			Log.Out("Started Telnet on " + port);
		}
		catch (Exception ex)
		{
			Log.Out("Error in Telnet.ctor: " + ex);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AcceptClient(IAsyncResult _asyncResult)
	{
		if (listener?.Server == null || !listener.Server.IsBound)
		{
			return;
		}
		TcpClient tcpClient = listener.EndAcceptTcpClient(_asyncResult);
		EndPoint remoteEndPoint = tcpClient.Client.RemoteEndPoint;
		int hashCode;
		if (remoteEndPoint is IPEndPoint iPEndPoint)
		{
			hashCode = iPEndPoint.Address.GetHashCode();
		}
		else
		{
			hashCode = remoteEndPoint.GetHashCode();
			Log.Out("EndPoint is not an IPEndPoint but: " + remoteEndPoint.GetType());
		}
		lock (loginAttemptsPerIP)
		{
			if (!loginAttemptsPerIP.TryGetValue(hashCode, out var value))
			{
				value = new LoginAttempts();
				loginAttemptsPerIP[hashCode] = value;
			}
			if (!value.IsBanned())
			{
				TelnetConnection item = new TelnetConnection(this, tcpClient, hashCode, authEnabled);
				lock (connections)
				{
					connections.Add(item);
				}
			}
			else
			{
				tcpClient.Close();
				Log.Out("Telnet connection not accepted for too many login attempts: " + remoteEndPoint);
			}
		}
		listener.BeginAcceptTcpClient(AcceptClient, null);
	}

	public bool RegisterFailedLogin(TelnetConnection _con)
	{
		lock (loginAttemptsPerIP)
		{
			return loginAttemptsPerIP[_con.EndPointHash].LogAttempt();
		}
	}

	public void ConnectionClosed(TelnetConnection _con)
	{
		lock (connections)
		{
			connections.Remove(_con);
		}
	}

	public void Disconnect()
	{
		try
		{
			if (listener != null)
			{
				listener.Stop();
				listener = null;
			}
			List<TelnetConnection> list;
			lock (connections)
			{
				list = new List<TelnetConnection>(connections);
			}
			foreach (TelnetConnection item in list)
			{
				item.Close();
			}
		}
		catch (Exception ex)
		{
			Log.Out("Error in Telnet.Disconnect: " + ex);
		}
	}

	public void SendLine(string _line)
	{
		if (_line == null)
		{
			return;
		}
		lock (connections)
		{
			foreach (TelnetConnection connection in connections)
			{
				connection.SendLine(_line);
			}
		}
	}

	public void SendLog(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime)
	{
		lock (connections)
		{
			foreach (TelnetConnection connection in connections)
			{
				connection.SendLog(_formattedMessage, _plainMessage, _trace, _type, _timestamp, _uptime);
			}
		}
	}
}
