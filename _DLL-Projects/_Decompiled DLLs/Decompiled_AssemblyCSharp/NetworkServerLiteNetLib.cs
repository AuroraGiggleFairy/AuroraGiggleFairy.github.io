using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using UnityEngine.Networking;

public class NetworkServerLiteNetLib : INetworkServer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class LiteNetLibAuthWrapperServer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public class LNLAuthConnectionState
		{
			public enum EState
			{
				Authenticating,
				Connected
			}

			public EState State;

			public readonly Guid Challenge = Guid.NewGuid();

			public readonly DateTime StartTime = DateTime.Now;

			public TimeSpan Age => DateTime.Now - StartTime;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly TimeSpan ConnectionStateCheckInterval = TimeSpan.FromSeconds(10.0);

		[PublicizedFrom(EAccessModifier.Private)]
		public const int ConnectionRateLimitMilliseconds = 500;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly TimeSpan MaxDurationInAuthState = new TimeSpan(0, 0, 0, 10);

		[PublicizedFrom(EAccessModifier.Private)]
		public const int ChallengePackageSize = 17;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly NetworkServerLiteNetLib owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, DateTime> lastConnectAttemptTimes = new Dictionary<string, DateTime>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<int, LNLAuthConnectionState> authStates = new Dictionary<int, LNLAuthConnectionState>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<int> clearList = new List<int>();

		[PublicizedFrom(EAccessModifier.Private)]
		public DateTime nextStateChecksRun;

		public LiteNetLibAuthWrapperServer(NetworkServerLiteNetLib _owner)
		{
			owner = _owner;
		}

		public void Update()
		{
			DateTime now = DateTime.Now;
			if (now < nextStateChecksRun)
			{
				return;
			}
			nextStateChecksRun = now + ConnectionStateCheckInterval;
			lock (authStates)
			{
				clearList.Clear();
				foreach (var (item, lNLAuthConnectionState2) in authStates)
				{
					if (lNLAuthConnectionState2.State == LNLAuthConnectionState.EState.Authenticating && !(lNLAuthConnectionState2.Age <= MaxDurationInAuthState))
					{
						clearList.Add(item);
					}
				}
				foreach (int clear in clearList)
				{
					authStates.Remove(clear);
				}
			}
		}

		public void ConnectionRequestCheck(ConnectionRequest _request)
		{
			string text = _request.RemoteEndPoint.Address.ToString();
			DateTime now = DateTime.Now;
			lastConnectAttemptTimes.TryGetValue(text, out var value);
			TimeSpan timeSpan = now - value;
			lastConnectAttemptTimes[text] = now;
			if (timeSpan.TotalMilliseconds < 500.0)
			{
				Log.Out("NET: Rejecting connection request from " + text + ": Limiting connect rate from that IP!");
				_request.Reject(rejectRateLimit);
				return;
			}
			foreach (ClientInfo item in SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List)
			{
				if (!item.loginDone && item.ip == text)
				{
					Log.Out("NET: Rejecting connection request from " + text + ": A connection attempt from that IP is currently being processed!");
					_request.Reject(rejectPendingConnection);
					return;
				}
			}
			if (_request.Data.GetString() != owner.serverPassword)
			{
				_request.Reject(rejectInvalidPassword);
			}
			else
			{
				_request.Accept();
			}
		}

		public void OnPeerConnectedEvent(NetPeer _peer)
		{
			LNLAuthConnectionState value;
			lock (authStates)
			{
				if (authStates.TryGetValue(_peer.Id, out value) && ConnectionManager.VerboseNetLogging)
				{
					Log.Out($"NET: LiteNetLib: New peer {_peer} with previously used peer id {_peer.Id}");
				}
				value = new LNLAuthConnectionState();
				authStates[_peer.Id] = value;
			}
			byte[] array = new byte[17]
			{
				202, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0
			};
			value.Challenge.WriteToBuffer(array, 1);
			if (ConnectionManager.VerboseNetLogging)
			{
				Log.Out($"NET: LiteNetLib: Sending challenge {value.Challenge} to new peer {_peer} / {_peer.Id}");
			}
			_peer.Send(array, 0, array.Length, DeliveryMethod.ReliableOrdered);
		}

		public void OnPeerDisconnectedEvent(NetPeer _peer, DisconnectInfo _disconnectInfo)
		{
			LNLAuthConnectionState value;
			lock (authStates)
			{
				if (!authStates.TryGetValue(_peer.Id, out value))
				{
					return;
				}
			}
			lock (authStates)
			{
				authStates.Remove(_peer.Id);
			}
			if (value.State == LNLAuthConnectionState.EState.Authenticating)
			{
				Log.Out($"NET: LiteNetLib: Peer disconnected in auth state: {_peer} / {_peer.Id}");
			}
			else
			{
				owner.PeerDisconnectedEvent(_peer, _disconnectInfo);
			}
		}

		public void OnNetworkReceiveEvent(NetPeer _peer, NetPacketReader _reader, byte _channel, DeliveryMethod _deliveryMethod)
		{
			LNLAuthConnectionState value;
			lock (authStates)
			{
				if (!authStates.TryGetValue(_peer.Id, out value))
				{
					return;
				}
			}
			if (value.State == LNLAuthConnectionState.EState.Authenticating)
			{
				int availableBytes = _reader.AvailableBytes;
				if (availableBytes != 17)
				{
					Log.Out($"NET: LiteNetLib: Received invalid package (length={availableBytes}) from an client in auth state: {_peer} / {_peer.Id}");
					_peer.Disconnect();
					return;
				}
				byte[] array = MemoryPools.poolByte.Alloc(availableBytes);
				_reader.GetBytes(array, availableBytes);
				if (array[0] != 202)
				{
					Log.Out($"NET: LiteNetLib: Received invalid package (channel={array[0]}) from an client in auth state: {_peer} / {_peer.Id}");
					_peer.Disconnect();
					MemoryPools.poolByte.Free(array);
					return;
				}
				byte[] array2 = new byte[16];
				Array.Copy(array, 1, array2, 0, 16);
				Guid guid = new Guid(array2);
				if (guid != value.Challenge)
				{
					Log.Out($"NET: LiteNetLib: Received invalid challenge {guid} from an client in auth state: {_peer} / {_peer.Id}");
					_peer.Disconnect();
					MemoryPools.poolByte.Free(array);
					return;
				}
				if (ConnectionManager.VerboseNetLogging)
				{
					Log.Out($"NET: LiteNetLib: Received correct challenge {guid} from new peer {_peer} / {_peer.Id}, accepting");
				}
				value.State = LNLAuthConnectionState.EState.Connected;
				owner.PeerConnectedEvent(_peer);
				MemoryPools.poolByte.Free(array);
			}
			else
			{
				owner.NetworkReceiveEvent(_peer, _reader, _deliveryMethod);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] rejectInvalidPassword = new byte[2];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] rejectRateLimit = new byte[2] { 1, 0 };

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] rejectPendingConnection = new byte[2] { 2, 0 };

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] disconnectServerShutdown = new byte[2] { 3, 0 };

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] disconnectFromClientSide = new byte[2] { 4, 0 };

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IProtocolManagerProtocolInterface protoManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public string serverPassword;

	[PublicizedFrom(EAccessModifier.Private)]
	public NetManager server;

	[PublicizedFrom(EAccessModifier.Private)]
	public LiteNetLibAuthWrapperServer authWrapper;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ClientInfo> dropClientsQueue = new List<ClientInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<NetPeer> getPeersList = new List<NetPeer>();

	public NetworkServerLiteNetLib(IProtocolManagerProtocolInterface _protoManager)
	{
		protoManager = _protoManager;
	}

	public void Update()
	{
		ClientInfo clientInfo;
		do
		{
			clientInfo = null;
			lock (dropClientsQueue)
			{
				int num = dropClientsQueue.Count - 1;
				if (num >= 0)
				{
					clientInfo = dropClientsQueue[num];
					dropClientsQueue.RemoveAt(num);
				}
			}
			if (clientInfo != null)
			{
				DropClient(clientInfo, _clientDisconnect: false);
			}
		}
		while (clientInfo != null);
		authWrapper?.Update();
	}

	public void LateUpdate()
	{
	}

	public NetworkConnectionError StartServer(int _basePort, string _password)
	{
		serverPassword = (string.IsNullOrEmpty(_password) ? "" : _password);
		EventBasedNetListener eventBasedNetListener = new EventBasedNetListener();
		server = new NetManager(eventBasedNetListener);
		authWrapper = new LiteNetLibAuthWrapperServer(this);
		NetworkCommonLiteNetLib.InitConfig(server);
		eventBasedNetListener.ConnectionRequestEvent += authWrapper.ConnectionRequestCheck;
		eventBasedNetListener.PeerConnectedEvent += authWrapper.OnPeerConnectedEvent;
		eventBasedNetListener.PeerDisconnectedEvent += authWrapper.OnPeerDisconnectedEvent;
		eventBasedNetListener.NetworkReceiveEvent += authWrapper.OnNetworkReceiveEvent;
		eventBasedNetListener.NetworkErrorEvent += [PublicizedFrom(EAccessModifier.Internal)] (IPEndPoint _endpoint, SocketError _code) =>
		{
			Log.Error("NET: LiteNetLib: Network error: {0}", _code);
		};
		if (server.Start(_basePort + 2))
		{
			Log.Out("NET: LiteNetLib server started");
			return NetworkConnectionError.NoError;
		}
		Log.Out("NET: LiteNetLib server could not be started");
		return NetworkConnectionError.CreateSocketOrThreadFailure;
	}

	public void SetServerPassword(string _password)
	{
		serverPassword = _password ?? "";
	}

	public void StopServer()
	{
		NetManager netManager = server;
		if (netManager != null && netManager.IsRunning)
		{
			List<NetPeer> list = new List<NetPeer>();
			server.GetPeersNonAlloc(list, ConnectionState.Any);
			for (int i = 0; i < list.Count; i++)
			{
				server.DisconnectPeer(list[i], disconnectServerShutdown);
			}
			server.Stop();
			server = null;
			authWrapper = null;
		}
		Log.Out("NET: LiteNetLib server stopped");
	}

	public void DropClient(ClientInfo _clientInfo, bool _clientDisconnect)
	{
		OnPlayerDisconnected(_clientInfo.litenetPeerConnectId);
		NetPeer peerByConnectId = GetPeerByConnectId(_clientInfo.litenetPeerConnectId);
		if (peerByConnectId != null)
		{
			server.DisconnectPeer(peerByConnectId, disconnectFromClientSide);
		}
	}

	public NetworkError SendData(ClientInfo _cInfo, int _channel, ArrayListMP<byte> _data, bool _reliableDelivery = true)
	{
		NetPeer peerByConnectId = GetPeerByConnectId(_cInfo.litenetPeerConnectId);
		if (peerByConnectId == null)
		{
			Log.Warning("NET: LiteNetLib: SendData requested for unknown client {0}", _cInfo.ToString());
			lock (dropClientsQueue)
			{
				if (!dropClientsQueue.Contains(_cInfo))
				{
					dropClientsQueue.Add(_cInfo);
				}
			}
			return NetworkError.WrongConnection;
		}
		_data[0] = (byte)_channel;
		if (ConnectionManager.VerboseNetLogging)
		{
			Log.Out("Sending data to peer {2}: ch={0}, size={1}", _channel, _data.Count, _cInfo.InternalId.CombinedString);
		}
		peerByConnectId.Send(_data.Items, 0, _data.Count, _reliableDelivery ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable);
		return NetworkError.Ok;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PeerConnectedEvent(NetPeer _peer)
	{
		Log.Out($"NET: LiteNetLib: Connect from: {_peer} / {_peer.Id}");
		ClientInfo clientInfo = new ClientInfo
		{
			litenetPeerConnectId = _peer.Id,
			network = this,
			netConnection = new INetConnection[2]
		};
		for (int i = 0; i < 2; i++)
		{
			INetConnection[] netConnection = clientInfo.netConnection;
			int num = i;
			int channel = i;
			int id = _peer.Id;
			netConnection[num] = new NetConnectionSimple(channel, clientInfo, null, id.ToString(), 1);
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.AddClient(clientInfo);
		SingletonMonoBehaviour<ConnectionManager>.Instance.Net_PlayerConnected(clientInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PeerDisconnectedEvent(NetPeer _peer, DisconnectInfo _info)
	{
		Log.Out($"NET: LiteNetLib: Client disconnect from: {_peer} / {_peer.Id} ({_info.Reason.ToStringCached()})");
		if (_info.Reason == DisconnectReason.Timeout)
		{
			Log.Out($"NET: LiteNetLib: TimeSinceLastPacket: {_peer.TimeSinceLastPacket}");
		}
		ThreadManager.AddSingleTaskMainThread("PlayerDisconnectLiteNetLib", [PublicizedFrom(EAccessModifier.Internal)] (object _) =>
		{
			Log.Out($"NET: LiteNetLib: MT: Client disconnect from: {_peer} / {_peer.Id} ({_info.Reason.ToStringCached()})");
			OnPlayerDisconnected(_peer.Id);
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NetworkReceiveEvent(NetPeer _peer, NetPacketReader _reader, DeliveryMethod _deliveryMethod)
	{
		ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForLiteNetPeer(_peer.Id);
		if (clientInfo == null)
		{
			Log.Out("NET: LiteNetLib: Received package from an unknown client: " + _peer);
			return;
		}
		if (_reader.AvailableBytes == 0)
		{
			Log.Out("NET: LiteNetLib: Received package with zero size from: " + clientInfo);
			return;
		}
		int availableBytes = _reader.AvailableBytes;
		byte[] array = MemoryPools.poolByte.Alloc(availableBytes);
		_reader.GetBytes(array, availableBytes);
		if (ConnectionManager.VerboseNetLogging)
		{
			Log.Out("Received data from peer {2}: ch={0}, size={1}", array[0], availableBytes, clientInfo.InternalId.CombinedString);
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.Net_DataReceivedServer(clientInfo, array[0], array, availableBytes);
	}

	public string GetIP(ClientInfo _cInfo)
	{
		NetPeer peerByConnectId = GetPeerByConnectId(_cInfo.litenetPeerConnectId);
		if (peerByConnectId == null)
		{
			Log.Warning("NET: LiteNetLib: IP requested for unknown client {0}", _cInfo.ToString());
			return string.Empty;
		}
		return peerByConnectId.Address.ToString();
	}

	public int GetPing(ClientInfo _cInfo)
	{
		NetPeer peerByConnectId = GetPeerByConnectId(_cInfo.litenetPeerConnectId);
		if (peerByConnectId == null)
		{
			Log.Warning("NET: LiteNetLib: Ping requested for unknown client {0}", _cInfo.ToString());
			return -1;
		}
		return peerByConnectId.Ping;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPlayerDisconnected(long _peerConnectId)
	{
		ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForLiteNetPeer(_peerConnectId);
		if (clientInfo != null)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.Net_PlayerDisconnected(clientInfo);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPeer GetPeerByConnectId(long _connectId)
	{
		if (server == null)
		{
			return null;
		}
		lock (getPeersList)
		{
			server.GetPeersNonAlloc(getPeersList, ConnectionState.Any);
			for (int i = 0; i < getPeersList.Count; i++)
			{
				if (getPeersList[i].Id == _connectId)
				{
					return getPeersList[i];
				}
			}
		}
		return null;
	}

	public int GetBadPacketCount(ClientInfo _cInfo)
	{
		return GetPeerByConnectId(_cInfo.litenetPeerConnectId)?.badPacketCount ?? 0;
	}

	public string GetServerPorts(int _basePort)
	{
		return _basePort + 2 + "/UDP";
	}

	public void SetLatencySimulation(bool _enable, int _minLatency, int _maxLatency)
	{
		if (server != null)
		{
			server.SimulateLatency = _enable;
			server.SimulationMinLatency = _minLatency;
			server.SimulationMaxLatency = _maxLatency;
		}
	}

	public void SetPacketLossSimulation(bool _enable, int _chance)
	{
		if (server != null)
		{
			server.SimulatePacketLoss = _enable;
			server.SimulationPacketLossChance = _chance;
		}
	}

	public void EnableStatistics()
	{
		if (server != null)
		{
			server.EnableStatistics = true;
		}
	}

	public void DisableStatistics()
	{
		if (server != null)
		{
			server.EnableStatistics = false;
		}
	}

	public string PrintNetworkStatistics()
	{
		if (server != null)
		{
			return server.Statistics.ToString();
		}
		return "no server!";
	}

	public void ResetNetworkStatistics()
	{
		if (server != null)
		{
			server.Statistics.Reset();
		}
	}

	public int GetMaximumPacketSize(ClientInfo _cInfo, bool _reliable = false)
	{
		int result = -1;
		NetPeer peerByConnectId = GetPeerByConnectId(_cInfo.litenetPeerConnectId);
		if (peerByConnectId != null)
		{
			result = peerByConnectId.GetMaxSinglePacketSize(_reliable ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable);
		}
		return result;
	}
}
