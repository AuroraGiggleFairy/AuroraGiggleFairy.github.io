using System;
using System.Collections;
using System.Text;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using UnityEngine.Networking;

namespace Platform.EOS;

public class NetworkClientEos : IPlatformNetworkClient, INetworkClient
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string socketName = "Game";

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IProtocolManagerProtocolInterface protoManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public P2PInterface ptpInterface;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProductUserId localUser;

	[PublicizedFrom(EAccessModifier.Private)]
	public SocketId socketId;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BlockingQueue<NetworkCommonEos.SendInfo> sendBufs = new BlockingQueue<NetworkCommonEos.SendInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ProductUserId serverId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool connecting;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool disconnectEventReceived;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ArraySegment<byte> receiveBuffer = new ArraySegment<byte>(new byte[1170]);

	public bool IsConnected
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (serverId != null && (protoManager.IsClient || connecting))
			{
				return owner.User.UserStatus == EUserStatus.LoggedIn;
			}
			return false;
		}
	}

	public NetworkClientEos(IPlatform _owner, IProtocolManagerProtocolInterface _protoManager)
	{
		owner = _owner;
		protoManager = _protoManager;
		owner.Api.ClientApiInitialized += [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (!GameManager.IsDedicatedServer)
			{
				EosHelpers.AssertMainThread("P2P.Init");
				localUser = ((UserIdentifierEos)owner.User.PlatformUserId).ProductUserId;
				socketId = new SocketId
				{
					SocketName = "Game"
				};
				lock (AntiCheatCommon.LockObject)
				{
					ptpInterface = ((Api)owner.Api).PlatformInterface.GetP2PInterface();
				}
				AddNotifyPeerConnectionRequestOptions options = new AddNotifyPeerConnectionRequestOptions
				{
					LocalUserId = localUser,
					SocketId = socketId
				};
				lock (AntiCheatCommon.LockObject)
				{
					ptpInterface.AddNotifyPeerConnectionRequest(ref options, null, ConnectionRequestHandler);
				}
				AddNotifyPeerConnectionEstablishedOptions options2 = new AddNotifyPeerConnectionEstablishedOptions
				{
					LocalUserId = localUser,
					SocketId = socketId
				};
				lock (AntiCheatCommon.LockObject)
				{
					ptpInterface.AddNotifyPeerConnectionEstablished(ref options2, null, ConnectionEstablishedHandler);
				}
				AddNotifyPeerConnectionClosedOptions options3 = new AddNotifyPeerConnectionClosedOptions
				{
					LocalUserId = localUser,
					SocketId = socketId
				};
				lock (AntiCheatCommon.LockObject)
				{
					ptpInterface.AddNotifyPeerConnectionClosed(ref options3, null, ConnectionClosedHandler);
				}
				AddNotifyIncomingPacketQueueFullOptions options4 = default(AddNotifyIncomingPacketQueueFullOptions);
				lock (AntiCheatCommon.LockObject)
				{
					ptpInterface.AddNotifyIncomingPacketQueueFull(ref options4, null, IncomingPacketQueueFullHandler);
				}
			}
		};
	}

	public void Connect(GameServerInfo _gsi)
	{
		disconnectEventReceived = false;
		Log.Out("[EOS-P2PC] Trying to connect to: " + _gsi.GetValue(GameInfoString.IP) + ":" + _gsi.GetValue(GameInfoInt.Port));
		if (string.IsNullOrEmpty(_gsi.GetValue(GameInfoString.CombinedPrimaryId)))
		{
			Log.Out("[EOS-P2PC] Resolving EOS ID for IP " + _gsi.GetValue(GameInfoString.IP) + ":" + _gsi.GetValue(GameInfoInt.Port));
			ServerInformationTcpClient.RequestRules(_gsi, _ignoreTimeouts: false, RulesRequestTcpDone);
			connecting = true;
		}
		else
		{
			ConnectInternal(_gsi);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RulesRequestTcpDone(bool _success, string _message, GameServerInfo _gsi)
	{
		if (_success && connecting)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo = _gsi;
			ConnectInternal(_gsi);
		}
		else
		{
			Disconnect();
			ThreadManager.StartCoroutine(connectionFailedLater(_message));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConnectInternal(GameServerInfo _gsi)
	{
		string value = _gsi.GetValue(GameInfoString.CombinedPrimaryId);
		if (string.IsNullOrEmpty(value))
		{
			Log.Error("Server info does not have a CombinedPrimaryId");
			Disconnect();
			ThreadManager.StartCoroutine(connectionFailedLater(Localization.Get("netSteamNetworking_NoServerID")));
			return;
		}
		if (_gsi.AllowsCrossplay && !PermissionsManager.IsCrossplayAllowed())
		{
			Disconnect();
			protoManager.ConnectionFailedEv(Localization.Get("auth_noCrossplay"));
			return;
		}
		if (!(PlatformUserIdentifierAbs.FromCombinedString(value) is UserIdentifierEos userIdentifierEos))
		{
			Disconnect();
			ThreadManager.StartCoroutine(connectionFailedLater(Localization.Get("netSteamNetworking_NoServerID")));
			return;
		}
		string password = ServerInfoCache.Instance.GetPassword(_gsi);
		ArrayListMP<byte> arrayListMP;
		if (!string.IsNullOrEmpty(password))
		{
			int byteCount = Encoding.UTF8.GetByteCount(password);
			arrayListMP = new ArrayListMP<byte>(MemoryPools.poolByte, byteCount + 1)
			{
				Count = byteCount + 1
			};
			Encoding.UTF8.GetBytes(password, 0, password.Length, arrayListMP.Items, 1);
		}
		else
		{
			arrayListMP = new ArrayListMP<byte>(MemoryPools.poolByte, 1)
			{
				Count = 1
			};
		}
		EosHelpers.AssertMainThread("P2P.ConInt.PUID");
		serverId = userIdentifierEos.ProductUserId;
		Log.Out("[EOS-P2PC] Connecting to EOS ID " + serverId);
		connecting = true;
		SendData(50, arrayListMP);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator connectionFailedLater(string _message)
	{
		yield return null;
		yield return null;
		protoManager.ConnectionFailedEv(_message);
	}

	public void Disconnect()
	{
		connecting = false;
		sendBufs.Clear();
		EosHelpers.AssertMainThread("P2P.CloseCons");
		CloseConnectionsOptions options = new CloseConnectionsOptions
		{
			SocketId = socketId,
			LocalUserId = localUser
		};
		Result result;
		lock (AntiCheatCommon.LockObject)
		{
			result = ptpInterface.CloseConnections(ref options);
		}
		if (result != Result.Success)
		{
			Log.Error("[EOS-P2PC] Failed closing connections: " + result.ToStringCached());
		}
		serverId = null;
	}

	public NetworkError SendData(int _channel, ArrayListMP<byte> _data)
	{
		if (IsConnected)
		{
			_data[0] = (byte)_channel;
			sendBufs.Enqueue(new NetworkCommonEos.SendInfo(null, _data));
		}
		else
		{
			Log.Warning("[EOS-P2PC] Tried to send a package while not connected to a server");
		}
		return NetworkError.Ok;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConnectionRequestHandler(ref OnIncomingConnectionRequestInfo _data)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConnectionEstablishedHandler(ref OnPeerConnectionEstablishedInfo _data)
	{
		Log.Out($"[EOS-P2PC] Connection established: {_data.RemoteUserId}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConnectionClosedHandler(ref OnRemoteConnectionClosedInfo _data)
	{
		ProductUserId remoteUserId = _data.RemoteUserId;
		if (connecting)
		{
			Disconnect();
			Log.Out($"[EOS-P2PC] P2PSessionConnectFail to: {_data.RemoteUserId} - Error: {_data.Reason.ToStringCached()}");
			string msg = Localization.Get("netSteamNetworkingSessionError_" + _data.Reason.ToStringCached());
			protoManager.ConnectionFailedEv(msg);
		}
		else
		{
			if (!IsConnected)
			{
				return;
			}
			Log.Out($"[EOS-P2PC] Connection closed by {remoteUserId}: " + _data.Reason.ToStringCached());
			if (_data.Reason != ConnectionClosedReason.ClosedByLocalUser)
			{
				CloseConnectionOptions options = new CloseConnectionOptions
				{
					SocketId = socketId,
					LocalUserId = localUser,
					RemoteUserId = remoteUserId
				};
				Result result;
				lock (AntiCheatCommon.LockObject)
				{
					result = ptpInterface.CloseConnection(ref options);
				}
				if (result != Result.Success)
				{
					Log.Error($"[EOS-P2PC] Failed closing connection to {remoteUserId}: {result.ToStringCached()}");
				}
				OnDisconnectedFromServer();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void IncomingPacketQueueFullHandler(ref OnIncomingPacketQueueFullInfo _data)
	{
		if (IsConnected)
		{
			Log.Error($"[EOS-P2PC] Packet queue full: Chn={_data.OverflowPacketChannel}, IncSize={_data.OverflowPacketSizeBytes}, Used={_data.PacketQueueCurrentSizeBytes}, Max={_data.PacketQueueMaxSizeBytes}");
		}
	}

	public void Update()
	{
		if (!IsConnected)
		{
			return;
		}
		while (true)
		{
			ReceivePacketOptions options = new ReceivePacketOptions
			{
				LocalUserId = localUser,
				MaxDataSizeBytes = 1170u
			};
			ProductUserId outPeerId = new ProductUserId();
			SocketId outSocketId = default(SocketId);
			Result result;
			uint outBytesWritten;
			lock (AntiCheatCommon.LockObject)
			{
				result = ptpInterface.ReceivePacket(ref options, ref outPeerId, ref outSocketId, out var _, receiveBuffer, out outBytesWritten);
			}
			switch (result)
			{
			default:
				Log.Error("[EOS-P2PS] Error reading packages: " + result.ToStringCached());
				return;
			case Result.Success:
			{
				if (outBytesWritten == 0)
				{
					break;
				}
				ArraySegment<byte> arraySegment = receiveBuffer;
				NetworkCommonEos.ESteamNetChannels eSteamNetChannels = (NetworkCommonEos.ESteamNetChannels)arraySegment.Array[0];
				switch (eSteamNetChannels)
				{
				case NetworkCommonEos.ESteamNetChannels.Authentication:
					if (connecting)
					{
						connecting = false;
						Log.Out("[EOS-P2PC] Connection established");
					}
					arraySegment = receiveBuffer;
					if (arraySegment.Array[1] == 0)
					{
						Log.Out("[EOS-P2PC] Received invalid password package");
						ThreadManager.AddSingleTaskMainThread("SteamNetInvalidPassword", [PublicizedFrom(EAccessModifier.Private)] (object _info) =>
						{
							protoManager.InvalidPasswordEv();
						});
					}
					else
					{
						Log.Out("[EOS-P2PC] Password accepted");
						OnConnectedToServer();
					}
					break;
				case NetworkCommonEos.ESteamNetChannels.Ping:
				{
					SendPacketOptions options2 = new SendPacketOptions
					{
						SocketId = socketId,
						LocalUserId = localUser,
						RemoteUserId = serverId,
						Channel = 0,
						Reliability = PacketReliability.ReliableOrdered,
						AllowDelayedDelivery = true,
						Data = receiveBuffer
					};
					Result result2;
					lock (AntiCheatCommon.LockObject)
					{
						result2 = ptpInterface.SendPacket(ref options2);
					}
					if (result2 != Result.Success)
					{
						Log.Error("[EOS-P2PC] Could not send ping package to server: " + result2.ToStringCached());
					}
					break;
				}
				case NetworkCommonEos.ESteamNetChannels.NetpackageChannel0:
				case NetworkCommonEos.ESteamNetChannels.NetpackageChannel1:
				{
					byte[] array = MemoryPools.poolByte.Alloc((int)outBytesWritten);
					arraySegment = receiveBuffer;
					Array.Copy(arraySegment.Array, array, outBytesWritten);
					SingletonMonoBehaviour<ConnectionManager>.Instance.Net_DataReceivedClient((int)eSteamNetChannels, array, (int)outBytesWritten);
					break;
				}
				}
				break;
			}
			case Result.NotFound:
				return;
			}
		}
	}

	public void LateUpdate()
	{
		if (!IsConnected)
		{
			return;
		}
		while (sendBufs.HasData())
		{
			NetworkCommonEos.SendInfo sendInfo = sendBufs.Dequeue();
			ProductUserId remoteUserId = serverId;
			SendPacketOptions options = new SendPacketOptions
			{
				SocketId = socketId,
				LocalUserId = localUser,
				RemoteUserId = remoteUserId,
				Channel = 0,
				Reliability = PacketReliability.ReliableOrdered,
				AllowDelayedDelivery = true,
				Data = new ArraySegment<byte>(sendInfo.Data.Items, 0, sendInfo.Data.Count)
			};
			Result result;
			lock (AntiCheatCommon.LockObject)
			{
				result = ptpInterface.SendPacket(ref options);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS-P2PC] Could not send package to server: " + result.ToStringCached());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisconnectedFromServer()
	{
		Disconnect();
		protoManager.DisconnectedFromServerEv(Localization.Get("netSteamNetworking_ConnectionClosedByServer"));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnConnectedToServer()
	{
		INetConnection[] array = new INetConnection[2];
		for (int i = 0; i < 2; i++)
		{
			array[i] = new NetConnectionSimple(i, null, this, null, 1, 1120);
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SetConnectionToServer(array);
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
}
