using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;

namespace Platform.Steam;

public class NetworkServerSteam : IPlatformNetworkServer, INetworkServer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum EConnectionState
	{
		Disconnected,
		Authenticating,
		Connected
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class ConnectionInformation
	{
		public EConnectionState State;

		public uint Ip;

		public bool PacketsPendingSend;

		public UserIdentifierSteam UserIdentifier;

		public int LastPingIndex = -1;

		public readonly int[] Pings = new int[50];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IProtocolManagerProtocolInterface protoManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int PingCount = 50;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly MicroStopwatch mswPing = new MicroStopwatch();

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo handlerThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly AutoResetEvent signalThread = new AutoResetEvent(initialState: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BlockingQueue<NetworkCommonSteam.SendInfo> sendBufs = new BlockingQueue<NetworkCommonSteam.SendInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BlockingQueue<NetworkCommonSteam.SendInfo> sendBufsUnreliable = new BlockingQueue<NetworkCommonSteam.SendInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BlockingQueue<CSteamID> acceptQueue = new BlockingQueue<CSteamID>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BlockingQueue<CSteamID> dropQueue = new BlockingQueue<CSteamID>();

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool flushBuffers;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BlockingQueue<CSteamID> disconnectQueue = new BlockingQueue<CSteamID>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<CSteamID, ConnectionInformation> connections = new Dictionary<CSteamID, ConnectionInformation>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Callback<P2PSessionRequest_t> m_P2PSessionRequest;

	[PublicizedFrom(EAccessModifier.Private)]
	public string serverPassword;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<CSteamID, ConnectionInformation> checkConnections = new Dictionary<CSteamID, ConnectionInformation>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int checkPerFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] passwordValidPacket = new byte[2] { 1, 50 };

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] passwordInvalidPacket = new byte[2] { 0, 50 };

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly byte[] recvBuf = new byte[1048576];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] timeData = new byte[9] { 0, 0, 0, 0, 0, 0, 0, 0, 60 };

	public bool ServerRunning
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (protoManager.IsServer && handlerThread != null)
			{
				return owner.ServerListAnnouncer.GameServerInitialized;
			}
			return false;
		}
	}

	public NetworkServerSteam(IPlatform _owner, IProtocolManagerProtocolInterface _protoManager)
	{
		owner = _owner;
		protoManager = _protoManager;
		owner.Api.ClientApiInitialized += [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			m_P2PSessionRequest = Callback<P2PSessionRequest_t>.CreateGameServer(P2PSessionRequest);
		};
	}

	public NetworkConnectionError StartServer(int _basePort, string _password)
	{
		if (ServerRunning)
		{
			Log.Error("[Steamworks.NET] NET: Server already running");
			return NetworkConnectionError.AlreadyConnectedToServer;
		}
		serverPassword = (string.IsNullOrEmpty(_password) ? null : _password);
		handlerThread = ThreadManager.StartThread("SteamNetworkingServer", threadHandlerMethod, null, null, true, false);
		Log.Out("[Steamworks.NET] NET: Server started");
		return NetworkConnectionError.NoError;
	}

	public void SetServerPassword(string _password)
	{
		serverPassword = (string.IsNullOrEmpty(_password) ? null : _password);
	}

	public void StopServer()
	{
		if (!ServerRunning)
		{
			return;
		}
		handlerThread.WaitForEnd();
		handlerThread = null;
		checkConnections.Clear();
		foreach (KeyValuePair<CSteamID, ConnectionInformation> connection in connections)
		{
			if (connection.Value.State != EConnectionState.Disconnected)
			{
				connection.Value.State = EConnectionState.Disconnected;
				SteamGameServerNetworking.CloseP2PSessionWithUser(connection.Key);
			}
		}
		connections.Clear();
		sendBufs.Clear();
		sendBufsUnreliable.Clear();
		acceptQueue.Clear();
		dropQueue.Clear();
		disconnectQueue.Clear();
		Log.Out("[Steamworks.NET] NET: Server stopped");
	}

	public void DropClient(ClientInfo _clientInfo, bool _clientDisconnect)
	{
		CSteamID id = new CSteamID(((UserIdentifierSteam)_clientInfo.PlatformId).SteamId);
		ThreadManager.StartCoroutine(dropLater(id, 0.2f));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator dropLater(CSteamID _id, float _delay)
	{
		yield return new WaitForSeconds(_delay);
		if (ServerRunning)
		{
			if (connections.TryGetValue(_id, out var value))
			{
				CSteamID cSteamID = _id;
				Log.Out("[Steamworks.NET] NET: Dropping client: " + cSteamID.ToString());
				value.State = EConnectionState.Disconnected;
				OnPlayerDisconnected(_id);
			}
			dropQueue.Enqueue(_id);
		}
	}

	public NetworkError SendData(ClientInfo _clientInfo, int _channel, ArrayListMP<byte> _data, bool reliableDelivery = true)
	{
		if (ServerRunning)
		{
			CSteamID recipient = new CSteamID(((UserIdentifierSteam)_clientInfo.PlatformId).SteamId);
			_data[_data.Count - 1] = (byte)_channel;
			if (GameManager.unreliableNetPackets && !reliableDelivery && _data.Count <= GetMaximumPacketSize(_clientInfo))
			{
				sendBufsUnreliable.Enqueue(new NetworkCommonSteam.SendInfo(recipient, _data));
			}
			else
			{
				sendBufs.Enqueue(new NetworkCommonSteam.SendInfo(recipient, _data));
			}
			signalThread.Set();
		}
		else
		{
			Log.Warning("[Steamworks.NET] NET: Tried to send a package to a client while not being a server");
		}
		return NetworkError.Ok;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void P2PSessionRequest(P2PSessionRequest_t _par)
	{
		if (ServerRunning)
		{
			Log.Out("[Steamworks.NET] NET: P2PSessionRequest from: " + _par.m_steamIDRemote.m_SteamID);
			acceptQueue.Enqueue(_par.m_steamIDRemote);
		}
	}

	public void Update()
	{
		if (ServerRunning)
		{
			while (disconnectQueue.HasData())
			{
				CSteamID id = disconnectQueue.Dequeue();
				OnPlayerDisconnected(id);
			}
		}
	}

	public void LateUpdate()
	{
		flushBuffers = true;
		signalThread.Set();
	}

	public string GetIP(ClientInfo _cInfo)
	{
		if (!connections.TryGetValue(new CSteamID(((UserIdentifierSteam)_cInfo.PlatformId).SteamId), out var value))
		{
			return string.Empty;
		}
		return NetworkUtils.ToAddr(value.Ip);
	}

	public int GetPing(ClientInfo _cInfo)
	{
		if (!connections.TryGetValue(new CSteamID(((UserIdentifierSteam)_cInfo.PlatformId).SteamId), out var value))
		{
			return -1;
		}
		int num = 0;
		for (int i = 0; i < 50; i++)
		{
			num += value.Pings[i];
		}
		return num / 50;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPlayerDisconnected(CSteamID _id)
	{
		UserIdentifierSteam userIdentifier = new UserIdentifierSteam(_id);
		ClientInfo cInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForUserId(userIdentifier);
		SingletonMonoBehaviour<ConnectionManager>.Instance.Net_PlayerDisconnected(cInfo);
	}

	public string GetServerPorts(int _basePort)
	{
		return "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void threadHandlerMethod(ThreadManager.ThreadInfo _threadinfo)
	{
		while (!_threadinfo.TerminationRequested())
		{
			if (!ServerRunning)
			{
				signalThread.WaitOne(100);
				continue;
			}
			signalThread.WaitOne(6);
			while (acceptQueue.HasData())
			{
				CSteamID cSteamID = acceptQueue.Dequeue();
				UserIdentifierSteam userIdentifier = new UserIdentifierSteam(cSteamID);
				connections[cSteamID] = new ConnectionInformation
				{
					State = EConnectionState.Authenticating,
					UserIdentifier = userIdentifier
				};
				SteamGameServerNetworking.AcceptP2PSessionWithUser(cSteamID);
			}
			CheckConnections();
			ReceivePackets();
			while (sendBufs.HasData())
			{
				NetworkCommonSteam.SendInfo sendInfo = sendBufs.Dequeue();
				CSteamID recipient = sendInfo.Recipient;
				if (connections.TryGetValue(recipient, out var value) && value.State == EConnectionState.Connected)
				{
					if (!SteamGameServerNetworking.SendP2PPacket(recipient, sendInfo.Data.Items, (uint)sendInfo.Data.Count, EP2PSend.k_EP2PSendReliableWithBuffering))
					{
						CSteamID cSteamID2 = recipient;
						Log.Error("[Steamworks.NET] NET: Could not send package to client " + cSteamID2.ToString());
					}
					else
					{
						value.PacketsPendingSend = true;
					}
				}
			}
			while (sendBufsUnreliable.HasData())
			{
				NetworkCommonSteam.SendInfo sendInfo2 = sendBufsUnreliable.Dequeue();
				CSteamID recipient2 = sendInfo2.Recipient;
				if (connections.TryGetValue(recipient2, out var value2) && value2.State == EConnectionState.Connected)
				{
					if (!SteamGameServerNetworking.SendP2PPacket(recipient2, sendInfo2.Data.Items, (uint)sendInfo2.Data.Count, EP2PSend.k_EP2PSendUnreliable))
					{
						CSteamID cSteamID2 = recipient2;
						Log.Error("[Steamworks.NET] NET: Could not send package to client " + cSteamID2.ToString());
					}
					else
					{
						value2.PacketsPendingSend = true;
					}
				}
			}
			if (flushBuffers)
			{
				flushBuffers = false;
				global::Utils.GetBytes(mswPing.ElapsedMilliseconds, timeData);
				foreach (KeyValuePair<CSteamID, ConnectionInformation> connection in connections)
				{
					if (connection.Value.State == EConnectionState.Connected && connection.Value.PacketsPendingSend)
					{
						connection.Value.PacketsPendingSend = false;
						FlushBuffer(connection.Key);
					}
				}
			}
			while (dropQueue.HasData())
			{
				SteamGameServerNetworking.CloseP2PSessionWithUser(dropQueue.Dequeue());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckConnections()
	{
		if (checkConnections.Count == 0)
		{
			foreach (KeyValuePair<CSteamID, ConnectionInformation> connection in connections)
			{
				if (connection.Value.State != EConnectionState.Disconnected)
				{
					checkConnections.Add(connection.Key, connection.Value);
				}
			}
			checkPerFrame = (checkConnections.Count + 9) / 10;
		}
		int num = 0;
		while (num < checkPerFrame && checkConnections.Count != 0)
		{
			Dictionary<CSteamID, ConnectionInformation>.Enumerator enumerator2 = checkConnections.GetEnumerator();
			enumerator2.MoveNext();
			KeyValuePair<CSteamID, ConnectionInformation> current2 = enumerator2.Current;
			enumerator2.Dispose();
			if (current2.Value.State != EConnectionState.Disconnected)
			{
				if (SteamGameServerNetworking.GetP2PSessionState(current2.Key, out var pConnectionState))
				{
					if (pConnectionState.m_bConnectionActive == 0 && pConnectionState.m_bConnecting == 0)
					{
						Log.Out("[Steamworks.NET] NET: Connection closed: " + current2.Key.ToString());
						SteamGameServerNetworking.CloseP2PSessionWithUser(current2.Key);
						current2.Value.State = EConnectionState.Disconnected;
						disconnectQueue.Enqueue(current2.Key);
					}
					else if (current2.Value.Ip == 0)
					{
						current2.Value.Ip = pConnectionState.m_nRemoteIP;
					}
				}
				else
				{
					Log.Out("[Steamworks.NET] NET: No connection to client: " + current2.Key.ToString());
					SteamGameServerNetworking.CloseP2PSessionWithUser(current2.Key);
					current2.Value.State = EConnectionState.Disconnected;
					disconnectQueue.Enqueue(current2.Key);
				}
			}
			num++;
			checkConnections.Remove(current2.Key);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReceivePackets()
	{
		long elapsedMilliseconds = mswPing.ElapsedMilliseconds;
		uint pcubMsgSize;
		CSteamID psteamIDRemote;
		bool flag = SteamGameServerNetworking.ReadP2PPacket(recvBuf, (uint)recvBuf.Length, out pcubMsgSize, out psteamIDRemote);
		while (flag)
		{
			if (!connections.TryGetValue(psteamIDRemote, out var value) || value.State == EConnectionState.Disconnected)
			{
				CSteamID cSteamID = psteamIDRemote;
				Log.Out("[Steamworks.NET] NET: Received package from an unconnected client: " + cSteamID.ToString());
			}
			else if (pcubMsgSize != 0)
			{
				pcubMsgSize--;
				NetworkCommonSteam.ESteamNetChannels eSteamNetChannels = (NetworkCommonSteam.ESteamNetChannels)recvBuf[pcubMsgSize];
				switch (eSteamNetChannels)
				{
				case NetworkCommonSteam.ESteamNetChannels.Authentication:
					if (value.State == EConnectionState.Authenticating)
					{
						string password = Encoding.UTF8.GetString(recvBuf, 0, (int)pcubMsgSize);
						if (!((!Authenticate(psteamIDRemote, password)) ? SteamGameServerNetworking.SendP2PPacket(psteamIDRemote, passwordInvalidPacket, (uint)passwordInvalidPacket.Length, EP2PSend.k_EP2PSendReliable) : SteamGameServerNetworking.SendP2PPacket(psteamIDRemote, passwordValidPacket, (uint)passwordValidPacket.Length, EP2PSend.k_EP2PSendReliable)))
						{
							CSteamID cSteamID = psteamIDRemote;
							Log.Error("[Steamworks.NET] NET: Could not send package to client " + cSteamID.ToString());
						}
					}
					break;
				case NetworkCommonSteam.ESteamNetChannels.Ping:
					UpdatePing(psteamIDRemote, recvBuf, elapsedMilliseconds);
					break;
				case NetworkCommonSteam.ESteamNetChannels.NetpackageChannel0:
				case NetworkCommonSteam.ESteamNetChannels.NetpackageChannel1:
					if (value.State == EConnectionState.Connected)
					{
						if (pcubMsgSize != 0)
						{
							byte[] array = MemoryPools.poolByte.Alloc((int)pcubMsgSize);
							Array.Copy(recvBuf, array, pcubMsgSize);
							ClientInfo cInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForUserId(value.UserIdentifier);
							SingletonMonoBehaviour<ConnectionManager>.Instance.Net_DataReceivedServer(cInfo, (int)eSteamNetChannels, array, (int)pcubMsgSize);
						}
					}
					else
					{
						CSteamID cSteamID = psteamIDRemote;
						Log.Out("[Steamworks.NET] NET: Received package from an unauthenticated client: " + cSteamID.ToString());
					}
					break;
				default:
				{
					CSteamID cSteamID = psteamIDRemote;
					Log.Out("[Steamworks.NET] NET: Received package on an unknown channel from: " + cSteamID.ToString());
					break;
				}
				}
			}
			flag = SteamGameServerNetworking.ReadP2PPacket(recvBuf, (uint)recvBuf.Length, out pcubMsgSize, out psteamIDRemote);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool Authenticate(CSteamID _id, string _password)
	{
		bool flag = string.IsNullOrEmpty(serverPassword) || _password == serverPassword;
		Log.Out("[Steamworks.NET] NET: Received authentication package from " + _id.ToString() + ": " + (flag ? "valid" : "invalid") + " password");
		if (!flag)
		{
			connections[_id].State = EConnectionState.Authenticating;
			return false;
		}
		connections[_id].State = EConnectionState.Connected;
		OnPlayerConnected(_id);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPlayerConnected(CSteamID _id)
	{
		ClientInfo clientInfo = new ClientInfo
		{
			PlatformId = new UserIdentifierSteam(_id),
			network = this,
			netConnection = new INetConnection[2]
		};
		for (int i = 0; i < 2; i++)
		{
			clientInfo.netConnection[i] = new NetConnectionSteam(i, clientInfo, null, clientInfo.InternalId.CombinedString);
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.AddClient(clientInfo);
		SingletonMonoBehaviour<ConnectionManager>.Instance.Net_PlayerConnected(clientInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FlushBuffer(CSteamID _id)
	{
		if (!SteamGameServerNetworking.SendP2PPacket(_id, timeData, (uint)timeData.Length, EP2PSend.k_EP2PSendReliable))
		{
			CSteamID cSteamID = _id;
			Log.Error("[Steamworks.NET] NET: Could not flush the buffer to client " + cSteamID.ToString());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdatePing(CSteamID _sourceId, byte[] _data, long _curTime)
	{
		long num = BitConverter.ToInt64(_data, 0);
		int num2 = (int)(_curTime - num);
		ConnectionInformation connectionInformation = connections[_sourceId];
		connectionInformation.LastPingIndex++;
		if (connectionInformation.LastPingIndex >= 50)
		{
			connectionInformation.LastPingIndex = 0;
		}
		connectionInformation.Pings[connectionInformation.LastPingIndex] = num2;
	}

	public void SetLatencySimulation(bool _enable, int _minLatency, int _maxLatency)
	{
	}

	public void SetPacketLossSimulation(bool _enable, int _chance)
	{
	}

	public void EnableStatistics()
	{
	}

	public void DisableStatistics()
	{
	}

	public string PrintNetworkStatistics()
	{
		return "";
	}

	public void ResetNetworkStatistics()
	{
	}

	public int GetMaximumPacketSize(ClientInfo _cInfo, bool reliable = false)
	{
		return 1200;
	}

	public int GetBadPacketCount(ClientInfo _cInfo)
	{
		return 0;
	}
}
