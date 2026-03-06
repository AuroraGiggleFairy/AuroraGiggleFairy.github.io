using System.Net;
using System.Net.Sockets;
using System.Text;
using LiteNetLib;
using UnityEngine.Networking;

public class NetworkClientLiteNetLib : INetworkClient
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class LiteNetLibAuthWrapperClient
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly NetworkClientLiteNetLib owner;

		public LiteNetLibAuthWrapperClient(NetworkClientLiteNetLib _owner)
		{
			owner = _owner;
		}

		public void OnPeerConnectedEvent(NetPeer _peer)
		{
			Log.Out("NET: LiteNetLib: Connected to server");
			owner.OnPeerConnectedEvent(_peer);
		}

		public void OnPeerDisconnectedEvent(NetPeer _peer, DisconnectInfo _info)
		{
			owner.OnDisconnectedFromServer(_peer, _info);
		}

		public void OnNetworkReceiveEvent(NetPeer _peer, NetPacketReader _reader, byte _channel, DeliveryMethod _deliveryMethod)
		{
			if (_reader.PeekByte() == 202)
			{
				int availableBytes = _reader.AvailableBytes;
				byte[] array = MemoryPools.poolByte.Alloc(availableBytes);
				_reader.GetBytes(array, availableBytes);
				if (ConnectionManager.VerboseNetLogging)
				{
					Log.Out("NET: LiteNetLib: Sending challenge reply to server");
				}
				_peer.Send(array, 0, availableBytes, DeliveryMethod.ReliableOrdered);
				MemoryPools.poolByte.Free(array);
			}
			else
			{
				owner.NetworkReceiveEvent(_peer, _reader, _deliveryMethod);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IProtocolManagerProtocolInterface protoManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool connected;

	[PublicizedFrom(EAccessModifier.Private)]
	public NetManager client;

	[PublicizedFrom(EAccessModifier.Private)]
	public LiteNetLibAuthWrapperClient authWrapper;

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPeer serverPeer;

	public NetworkClientLiteNetLib(IProtocolManagerProtocolInterface _protoManager)
	{
		protoManager = _protoManager;
	}

	public void Update()
	{
	}

	public void LateUpdate()
	{
	}

	public void Connect(GameServerInfo _gsi)
	{
		string value = _gsi.GetValue(GameInfoString.IP);
		int value2 = _gsi.GetValue(GameInfoInt.Port);
		string text = ServerInfoCache.Instance.GetPassword(_gsi);
		if (text == null)
		{
			text = "";
		}
		if (string.IsNullOrEmpty(value))
		{
			Log.Out("NET: Skipping LiteNetLib connection attempt, no IP given");
			protoManager.ConnectionFailedEv(Localization.Get("netConnectionFailedNoIp"));
			return;
		}
		if (_gsi.AllowsCrossplay && !PermissionsManager.IsCrossplayAllowed())
		{
			Disconnect();
			protoManager.ConnectionFailedEv(Localization.Get("auth_noCrossplay"));
			return;
		}
		Log.Out("NET: LiteNetLib trying to connect to: " + value + ":" + value2);
		if (client != null)
		{
			Disconnect();
		}
		EventBasedNetListener eventBasedNetListener = new EventBasedNetListener();
		client = new NetManager(eventBasedNetListener);
		authWrapper = new LiteNetLibAuthWrapperClient(this);
		NetworkCommonLiteNetLib.InitConfig(client);
		eventBasedNetListener.PeerConnectedEvent += authWrapper.OnPeerConnectedEvent;
		eventBasedNetListener.PeerDisconnectedEvent += authWrapper.OnPeerDisconnectedEvent;
		eventBasedNetListener.NetworkReceiveEvent += authWrapper.OnNetworkReceiveEvent;
		eventBasedNetListener.NetworkErrorEvent += [PublicizedFrom(EAccessModifier.Internal)] (IPEndPoint _endpoint, SocketError _code) =>
		{
			Log.Error("NET: LiteNetLib: Network error: {0}", _code);
		};
		client.Start();
		client.Connect(value, value2 + 2, text);
	}

	public void Disconnect()
	{
		connected = false;
		if (client != null && client.IsRunning)
		{
			client.Stop();
		}
		client = null;
		serverPeer = null;
		authWrapper = null;
	}

	public NetworkError SendData(int _channel, ArrayListMP<byte> _data)
	{
		if (serverPeer == null)
		{
			Log.Warning("NET: LiteNetLib: SendData requested without active connection");
			return NetworkError.WrongOperation;
		}
		_data[0] = (byte)_channel;
		if (ConnectionManager.VerboseNetLogging)
		{
			Log.Out("Sending data to server: ch={0}, size={1}", _channel, _data.Count);
		}
		serverPeer.Send(_data.Items, 0, _data.Count, DeliveryMethod.ReliableOrdered);
		return NetworkError.Ok;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NetworkReceiveEvent(NetPeer _peer, NetPacketReader _reader, DeliveryMethod _deliveryMethod)
	{
		if (_reader.AvailableBytes == 0)
		{
			Log.Out("NET: LiteNetLib: Received package with zero size from");
			return;
		}
		int availableBytes = _reader.AvailableBytes;
		byte[] array = MemoryPools.poolByte.Alloc(availableBytes);
		_reader.GetBytes(array, availableBytes);
		if (ConnectionManager.VerboseNetLogging)
		{
			Log.Out("Received data from server: ch={0}, size={1}", array[0], availableBytes);
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.Net_DataReceivedClient(array[0], array, availableBytes);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPeerConnectedEvent(NetPeer _peer)
	{
		Log.Out("NET: LiteNetLib: Accepted by server");
		serverPeer = _peer;
		connected = true;
		INetConnection[] array = new INetConnection[2];
		for (int i = 0; i < 2; i++)
		{
			array[i] = new NetConnectionSimple(i, null, this, null, 1);
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SetConnectionToServer(array);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisconnectedFromServer(NetPeer _peer, DisconnectInfo _info)
	{
		NetworkCommonLiteNetLib.EAdditionalDisconnectCause additionalDisconnectCause = NetworkCommonLiteNetLib.EAdditionalDisconnectCause.InvalidPassword;
		string arg = null;
		bool hasDisconnectInfo = !_info.AdditionalData.IsNull && _info.AdditionalData.AvailableBytes != 0;
		if (hasDisconnectInfo)
		{
			int availableBytes = _info.AdditionalData.AvailableBytes;
			byte[] array = MemoryPools.poolByte.Alloc(availableBytes);
			_info.AdditionalData.GetBytes(array, availableBytes);
			additionalDisconnectCause = (NetworkCommonLiteNetLib.EAdditionalDisconnectCause)array[0];
			if (((availableBytes >= 2 && array[1] != 0) ? 1 : 0) > (false ? 1 : 0))
			{
				arg = Encoding.UTF8.GetString(array, 2, array[1]);
			}
			MemoryPools.poolByte.Free(array);
		}
		DisconnectReason reason = _info.Reason;
		string displayMessage = (hasDisconnectInfo ? string.Format(Localization.Get("netLiteNetLibDisconnectReason_" + additionalDisconnectCause.ToStringCached()), arg) : Localization.Get("netLiteNetLibDisconnectReason_" + reason.ToStringCached()));
		ThreadManager.AddSingleTaskMainThread("DisconnectLiteNetLib", [PublicizedFrom(EAccessModifier.Internal)] (object _taskInfo) =>
		{
			if (!connected)
			{
				Log.Out("NET: LiteNetLib: Connection failed: {0}", reason.ToStringCached());
				if (reason == DisconnectReason.ConnectionRejected)
				{
					if (additionalDisconnectCause == NetworkCommonLiteNetLib.EAdditionalDisconnectCause.InvalidPassword)
					{
						protoManager.InvalidPasswordEv();
					}
					else
					{
						Log.Out("NET: LiteNetLib: Reject cause: {0}", additionalDisconnectCause.ToStringCached());
						protoManager.ConnectionFailedEv(displayMessage);
					}
				}
				else
				{
					protoManager.ConnectionFailedEv(displayMessage);
				}
			}
			else
			{
				Log.Out("NET: LiteNetLib: Connection closed: " + reason.ToStringCached());
				if (hasDisconnectInfo && additionalDisconnectCause != NetworkCommonLiteNetLib.EAdditionalDisconnectCause.ClientSideDisconnect)
				{
					Log.Out("NET: LiteNetLib: Cause: {0}", additionalDisconnectCause.ToStringCached());
				}
				if (additionalDisconnectCause != NetworkCommonLiteNetLib.EAdditionalDisconnectCause.ClientSideDisconnect)
				{
					protoManager.DisconnectedFromServerEv(displayMessage);
				}
			}
			Disconnect();
		});
	}

	public void SetLatencySimulation(bool _enable, int _minLatency, int _maxLatency)
	{
		if (client != null)
		{
			client.SimulateLatency = _enable;
			client.SimulationMinLatency = _minLatency;
			client.SimulationMaxLatency = _maxLatency;
		}
	}

	public void SetPacketLossSimulation(bool _enable, int _chance)
	{
		if (client != null)
		{
			client.SimulatePacketLoss = _enable;
			client.SimulationPacketLossChance = _chance;
		}
	}

	public void EnableStatistics()
	{
		if (client != null)
		{
			client.EnableStatistics = true;
		}
	}

	public void DisableStatistics()
	{
		if (client != null)
		{
			client.EnableStatistics = false;
		}
	}

	public string PrintNetworkStatistics()
	{
		if (client != null)
		{
			return client.Statistics.ToString();
		}
		return "No client!";
	}

	public void ResetNetworkStatistics()
	{
		if (client != null)
		{
			client.Statistics.Reset();
		}
	}
}
