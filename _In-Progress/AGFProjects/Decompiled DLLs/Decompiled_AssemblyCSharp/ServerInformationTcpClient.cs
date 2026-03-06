using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public static class ServerInformationTcpClient
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum EGameServerInfoReadState
	{
		Size1,
		Size2,
		Size3,
		Size4,
		Size5,
		Break1,
		Break2,
		Data,
		Done,
		Error
	}

	public delegate void RulesRequestDone(bool _success, string _message, GameServerInfo _gsi);

	public static void RequestRules(GameServerInfo _gsi, bool _ignoreTimeouts, RulesRequestDone _callback)
	{
		ThreadManager.StartCoroutine(RequestRulesTcpCo(_gsi, _ignoreTimeouts, _callback));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator RequestRulesTcpCo(GameServerInfo _gsi, bool _ignoreTimeoutsAndRefusedConnections, RulesRequestDone _callback)
	{
		string ip = _gsi.GetValue(GameInfoString.IP);
		int port = _gsi.GetValue(GameInfoInt.Port);
		CountdownTimer timeout = new CountdownTimer(3f);
		TcpClient c = new TcpClient();
		Task connectAsync;
		try
		{
			connectAsync = c.ConnectAsync(ip, port);
		}
		catch (Exception ex)
		{
			Log.Warning($"NET: Requesting rules from TCP ({ip}:{port}) failed due to connection problems ({ex.Message})");
			_callback(_success: false, Localization.Get("netNoServerInformation"), _gsi);
			yield break;
		}
		while (!connectAsync.IsCompleted && !timeout.HasPassed())
		{
			yield return null;
		}
		if (timeout.HasPassed() && !connectAsync.IsCompleted)
		{
			c.Close();
			c.Dispose();
			if (!_ignoreTimeoutsAndRefusedConnections)
			{
				Log.Warning($"NET: Requesting rules from TCP ({ip}:{port}) failed due to connection problems (Timeout)");
			}
			_callback(_success: false, Localization.Get("netNoServerInformation"), _gsi);
			yield break;
		}
		if (connectAsync.IsFaulted)
		{
			Exception ex2 = connectAsync.Exception?.InnerException;
			if (_ignoreTimeoutsAndRefusedConnections)
			{
				SocketException obj = ex2 as SocketException;
				if (obj != null && obj.SocketErrorCode == SocketError.ConnectionRefused)
				{
					goto IL_0214;
				}
			}
			string arg = ex2?.Message.Replace("\r\n", " ").Replace("\n", " ");
			Log.Warning($"NET: Requesting rules from TCP ({ip}:{port}) failed due to connection problems ({arg})");
			goto IL_0214;
		}
		NetworkStream ns = c.GetStream();
		byte[] buf = MemoryPools.poolByte.Alloc(32768);
		int size = 0;
		int received = 0;
		bool legacyFormat = false;
		EGameServerInfoReadState state = EGameServerInfoReadState.Size1;
		while (!timeout.HasPassed() && state != EGameServerInfoReadState.Done && state != EGameServerInfoReadState.Error)
		{
			if (ns.CanRead)
			{
				while (ns.DataAvailable)
				{
					int num = ns.ReadByte();
					if (num < 0)
					{
						state = EGameServerInfoReadState.Error;
					}
					switch (state)
					{
					case EGameServerInfoReadState.Size1:
						if (num < 48 || num > 57)
						{
							legacyFormat = true;
							size = num << 8;
						}
						else
						{
							size = (num - 48) * 10000;
						}
						state = EGameServerInfoReadState.Size2;
						break;
					case EGameServerInfoReadState.Size2:
						if (legacyFormat)
						{
							size += num;
							state = ((size > 0) ? EGameServerInfoReadState.Data : EGameServerInfoReadState.Done);
						}
						else
						{
							size += (num - 48) * 1000;
							state = EGameServerInfoReadState.Size3;
						}
						break;
					case EGameServerInfoReadState.Size3:
						size += (num - 48) * 100;
						state = EGameServerInfoReadState.Size4;
						break;
					case EGameServerInfoReadState.Size4:
						size += (num - 48) * 10;
						state = EGameServerInfoReadState.Size5;
						break;
					case EGameServerInfoReadState.Size5:
						size += num - 48;
						state = EGameServerInfoReadState.Break1;
						break;
					case EGameServerInfoReadState.Break1:
						state = EGameServerInfoReadState.Break2;
						break;
					case EGameServerInfoReadState.Break2:
						state = ((size > 0) ? EGameServerInfoReadState.Data : EGameServerInfoReadState.Done);
						break;
					case EGameServerInfoReadState.Data:
						buf[received] = (byte)num;
						received++;
						if (received >= size)
						{
							state = EGameServerInfoReadState.Done;
						}
						break;
					}
				}
			}
			else
			{
				state = EGameServerInfoReadState.Error;
			}
			yield return null;
		}
		long elapsedMilliseconds = timeout.ElapsedMilliseconds;
		switch (state)
		{
		case EGameServerInfoReadState.Size1:
		case EGameServerInfoReadState.Size2:
		case EGameServerInfoReadState.Size3:
		case EGameServerInfoReadState.Size4:
		case EGameServerInfoReadState.Size5:
		case EGameServerInfoReadState.Break1:
		case EGameServerInfoReadState.Break2:
		case EGameServerInfoReadState.Data:
			Log.Warning($"NET: Requesting rules from TCP ({ip}:{port}) timed out");
			_callback(_success: false, Localization.Get("netLiteNetLibDisconnectReason_Timeout"), _gsi);
			break;
		case EGameServerInfoReadState.Error:
			Log.Warning($"NET: Requesting rules from TCP ({ip}:{port}) failed");
			_callback(_success: false, Localization.Get("netRequestingServerInformationFailed"), _gsi);
			break;
		case EGameServerInfoReadState.Done:
		{
			GameServerInfo gameServerInfo = new GameServerInfo(Encoding.UTF8.GetString(buf, 0, received));
			_gsi.Merge(gameServerInfo, EServerRelationType.Internet);
			_gsi.SetValue(GameInfoInt.Ping, (int)elapsedMilliseconds);
			_callback(_success: true, null, _gsi);
			break;
		}
		}
		MemoryPools.poolByte.Free(buf);
		c.Close();
		c.Dispose();
		yield break;
		IL_0214:
		_callback(_success: false, Localization.Get("netNoServerInformation"), _gsi);
		c.Close();
		c.Dispose();
	}
}
