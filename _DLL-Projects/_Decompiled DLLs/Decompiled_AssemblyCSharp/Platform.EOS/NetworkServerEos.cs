using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using UnityEngine;
using UnityEngine.Networking;

namespace Platform.EOS;

public class NetworkServerEos : IPlatformNetworkServer, INetworkServer
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

		public UserIdentifierEos UserIdentifier;

		public int LastPingIndex = -1;

		public readonly int[] Pings = new int[50];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string socketName = "Game";

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IProtocolManagerProtocolInterface protoManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public P2PInterface ptpInterface;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool serverStarted;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int PingCount = 50;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<ProductUserId, ConnectionInformation> connections = new Dictionary<ProductUserId, ConnectionInformation>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BlockingQueue<NetworkCommonEos.SendInfo> sendBufs = new BlockingQueue<NetworkCommonEos.SendInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BlockingQueue<NetworkCommonEos.SendInfo> sendBufsUnreliable = new BlockingQueue<NetworkCommonEos.SendInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public string serverPassword;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProductUserId localUser;

	[PublicizedFrom(EAccessModifier.Private)]
	public SocketId socketId;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ArraySegment<byte> receiveBuffer = new ArraySegment<byte>(new byte[1170]);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ArraySegment<byte> passwordValidPacket = new ArraySegment<byte>(new byte[2] { 50, 1 });

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ArraySegment<byte> passwordInvalidPacket = new ArraySegment<byte>(new byte[2] { 50, 0 });

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ArraySegment<byte> timeData = new ArraySegment<byte>(new byte[9] { 60, 0, 0, 0, 0, 0, 0, 0, 0 });

	public bool ServerRunning
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return serverStarted;
		}
	}

	public NetworkServerEos(IPlatform _owner, IProtocolManagerProtocolInterface _protoManager)
	{
		owner = _owner;
		protoManager = _protoManager;
		owner.Api.ClientApiInitialized += [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (!GameManager.IsDedicatedServer)
			{
				EosHelpers.AssertMainThread("P2PS.Init");
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

	public NetworkConnectionError StartServer(int _basePort, string _password)
	{
		if (ServerRunning)
		{
			Log.Error("[EOS-P2PS] Server already running");
			return NetworkConnectionError.AlreadyConnectedToServer;
		}
		serverPassword = (string.IsNullOrEmpty(_password) ? null : _password);
		serverStarted = true;
		Log.Out("[EOS-P2PS] Server started");
		return NetworkConnectionError.NoError;
	}

	public void SetServerPassword(string _password)
	{
		serverPassword = (string.IsNullOrEmpty(_password) ? null : _password);
	}

	public void StopServer()
	{
		if (ServerRunning)
		{
			serverStarted = false;
			connections.Clear();
			EosHelpers.AssertMainThread("P2PS.Stop");
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
				Log.Error("[EOS-P2PS] Failed closing connections: " + result.ToStringCached());
			}
			Log.Out("[EOS-P2PS] Server stopped");
		}
	}

	public void DropClient(ClientInfo _clientInfo, bool _clientDisconnect)
	{
		ProductUserId productUserId = ((UserIdentifierEos)_clientInfo.CrossplatformId).ProductUserId;
		ThreadManager.StartCoroutine(dropLater(productUserId, 0.2f));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator dropLater(ProductUserId _id, float _delay)
	{
		yield return new WaitForSeconds(_delay);
		if (ServerRunning)
		{
			if (connections.TryGetValue(_id, out var value))
			{
				Log.Out("[EOS-P2PS] Dropping client: " + _id);
				value.State = EConnectionState.Disconnected;
				OnPlayerDisconnected(_id);
			}
			CloseConnectionOptions options = new CloseConnectionOptions
			{
				SocketId = socketId,
				LocalUserId = localUser,
				RemoteUserId = _id
			};
			Result result;
			lock (AntiCheatCommon.LockObject)
			{
				result = ptpInterface.CloseConnection(ref options);
			}
			if (result != Result.Success)
			{
				Log.Error($"[EOS-P2PS] Failed closing connection: {_id}: {result.ToStringCached()}");
			}
		}
	}

	public NetworkError SendData(ClientInfo _clientInfo, int _channel, ArrayListMP<byte> _data, bool _reliableDelivery = true)
	{
		if (ServerRunning)
		{
			_data[0] = (byte)_channel;
			sendBufs.Enqueue(new NetworkCommonEos.SendInfo(_clientInfo, _data));
		}
		else
		{
			Log.Warning("[EOS-P2PS] Tried to send a package to a client while not being a server");
		}
		return NetworkError.Ok;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConnectionRequestHandler(ref OnIncomingConnectionRequestInfo _data)
	{
		if (!ServerRunning)
		{
			return;
		}
		ProductUserId remoteUserId = _data.RemoteUserId;
		UserIdentifierEos userIdentifierEos = new UserIdentifierEos(remoteUserId);
		if (_data.SocketId.Value.SocketName != "Game")
		{
			Log.Warning("[EOS-P2PS] P2P session request from " + userIdentifierEos.ProductUserIdString + " with invalid socket name '" + _data.SocketId.Value.SocketName + "'");
			return;
		}
		Log.Out("[EOS-P2PS] P2PSessionRequest from: " + userIdentifierEos.ProductUserIdString);
		AcceptConnectionOptions options = new AcceptConnectionOptions
		{
			SocketId = socketId,
			LocalUserId = localUser,
			RemoteUserId = remoteUserId
		};
		Result result;
		lock (AntiCheatCommon.LockObject)
		{
			result = ptpInterface.AcceptConnection(ref options);
		}
		if (result != Result.Success)
		{
			Log.Error("[EOS-P2PS] Failed accepting session: " + result.ToStringCached());
			return;
		}
		connections[remoteUserId] = new ConnectionInformation
		{
			State = EConnectionState.Authenticating,
			UserIdentifier = userIdentifierEos
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConnectionEstablishedHandler(ref OnPeerConnectionEstablishedInfo _data)
	{
		if (ServerRunning)
		{
			Log.Out($"[EOS-P2PS] Connection established: {_data.RemoteUserId}");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConnectionClosedHandler(ref OnRemoteConnectionClosedInfo _data)
	{
		if (!ServerRunning)
		{
			return;
		}
		ProductUserId remoteUserId = _data.RemoteUserId;
		Log.Out($"[EOS-P2PS] Connection closed by {remoteUserId}: " + _data.Reason.ToStringCached());
		if (connections.TryGetValue(remoteUserId, out var value) && value.State == EConnectionState.Connected)
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
				Log.Error($"[EOS-P2PS] Failed closing connection to {remoteUserId}: {result.ToStringCached()}");
			}
			value.State = EConnectionState.Disconnected;
			OnPlayerDisconnected(remoteUserId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void IncomingPacketQueueFullHandler(ref OnIncomingPacketQueueFullInfo _data)
	{
		if (ServerRunning)
		{
			Log.Error($"[EOS-P2PS] Packet queue full: Chn={_data.OverflowPacketChannel}, IncSize={_data.OverflowPacketSizeBytes}, Used={_data.PacketQueueCurrentSizeBytes}, Max={_data.PacketQueueMaxSizeBytes}");
		}
	}

	public void Update()
	{
		if (!ServerRunning)
		{
			return;
		}
		long curTime = (long)(Time.unscaledTime * 1000f);
		Result result;
		while (true)
		{
			ReceivePacketOptions options = new ReceivePacketOptions
			{
				LocalUserId = localUser,
				MaxDataSizeBytes = 1170u
			};
			ProductUserId outPeerId = new ProductUserId();
			SocketId outSocketId = default(SocketId);
			uint outBytesWritten;
			lock (AntiCheatCommon.LockObject)
			{
				result = ptpInterface.ReceivePacket(ref options, ref outPeerId, ref outSocketId, out var _, receiveBuffer, out outBytesWritten);
			}
			if (result != Result.Success)
			{
				break;
			}
			if (!connections.TryGetValue(outPeerId, out var value) || value.State == EConnectionState.Disconnected)
			{
				Log.Out("[EOS-P2PS] Received package from an unconnected client: " + outPeerId);
			}
			else
			{
				if (outBytesWritten == 0)
				{
					continue;
				}
				ArraySegment<byte> arraySegment = receiveBuffer;
				NetworkCommonEos.ESteamNetChannels eSteamNetChannels = (NetworkCommonEos.ESteamNetChannels)arraySegment.Array[0];
				switch (eSteamNetChannels)
				{
				case NetworkCommonEos.ESteamNetChannels.Authentication:
					if (value.State == EConnectionState.Authenticating)
					{
						Encoding uTF = Encoding.UTF8;
						arraySegment = receiveBuffer;
						string password = uTF.GetString(arraySegment.Array, 1, (int)(outBytesWritten - 1));
						bool flag = Authenticate(outPeerId, password);
						SendPacketOptions options2 = new SendPacketOptions
						{
							SocketId = socketId,
							LocalUserId = localUser,
							RemoteUserId = outPeerId,
							Channel = 0,
							Reliability = PacketReliability.ReliableOrdered,
							AllowDelayedDelivery = true,
							Data = (flag ? passwordValidPacket : passwordInvalidPacket)
						};
						Result result2;
						lock (AntiCheatCommon.LockObject)
						{
							result2 = ptpInterface.SendPacket(ref options2);
						}
						if (result2 != Result.Success)
						{
							Log.Error($"[EOS-P2PS] Could not send package to client {outPeerId}: {result2.ToStringCached()}");
						}
					}
					break;
				case NetworkCommonEos.ESteamNetChannels.Ping:
				{
					ProductUserId sourceId = outPeerId;
					arraySegment = receiveBuffer;
					UpdatePing(sourceId, arraySegment.Array, curTime);
					break;
				}
				case NetworkCommonEos.ESteamNetChannels.NetpackageChannel0:
				case NetworkCommonEos.ESteamNetChannels.NetpackageChannel1:
					if (value.State == EConnectionState.Connected)
					{
						byte[] array = MemoryPools.poolByte.Alloc((int)outBytesWritten);
						arraySegment = receiveBuffer;
						Array.Copy(arraySegment.Array, array, outBytesWritten);
						ClientInfo cInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForUserId(value.UserIdentifier);
						SingletonMonoBehaviour<ConnectionManager>.Instance.Net_DataReceivedServer(cInfo, (int)eSteamNetChannels, array, (int)outBytesWritten);
					}
					else
					{
						Log.Out("[EOS-P2PS] Received package from an unauthenticated client: " + outPeerId);
					}
					break;
				default:
					Log.Out("[EOS-P2PS] Received package on an unknown channel from: " + outPeerId);
					break;
				}
			}
		}
		if (result != Result.NotFound)
		{
			Log.Error("[EOS-P2PS] Error reading packages: " + result.ToStringCached());
		}
		if (result == Result.InvalidAuth)
		{
			StopServer();
		}
	}

	public void LateUpdate()
	{
		if (!ServerRunning)
		{
			return;
		}
		sendBuffers(sendBufs, PacketReliability.ReliableOrdered);
		sendBuffers(sendBufsUnreliable, PacketReliability.UnreliableUnordered);
		long value = (long)(Time.unscaledTime * 1000f);
		ArraySegment<byte> arraySegment = timeData;
		Utils.GetBytes(value, arraySegment.Array, 1);
		foreach (KeyValuePair<ProductUserId, ConnectionInformation> connection in connections)
		{
			if (connection.Value.State == EConnectionState.Connected)
			{
				FlushBuffer(connection.Key);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void sendBuffers(BlockingQueue<NetworkCommonEos.SendInfo> _buffers, PacketReliability _queue)
	{
		while (_buffers.HasData())
		{
			NetworkCommonEos.SendInfo sendInfo = _buffers.Dequeue();
			ProductUserId productUserId = ((UserIdentifierEos)sendInfo.Recipient.CrossplatformId).ProductUserId;
			if (connections.TryGetValue(productUserId, out var value) && value.State == EConnectionState.Connected)
			{
				SendPacketOptions options = new SendPacketOptions
				{
					SocketId = socketId,
					LocalUserId = localUser,
					RemoteUserId = productUserId,
					Channel = 0,
					Reliability = _queue,
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
					Log.Error($"[EOS-P2PS] Could not send package to client {productUserId}: {result.ToStringCached()}");
				}
			}
		}
	}

	public string GetIP(ClientInfo _cInfo)
	{
		if (!connections.TryGetValue(((UserIdentifierEos)_cInfo.CrossplatformId).ProductUserId, out var value))
		{
			return string.Empty;
		}
		return NetworkUtils.ToAddr(value.Ip);
	}

	public int GetPing(ClientInfo _cInfo)
	{
		if (!connections.TryGetValue(((UserIdentifierEos)_cInfo.CrossplatformId).ProductUserId, out var value))
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
	public void OnPlayerDisconnected(ProductUserId _id)
	{
		UserIdentifierEos userIdentifier = new UserIdentifierEos(_id);
		ClientInfo cInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForUserId(userIdentifier);
		SingletonMonoBehaviour<ConnectionManager>.Instance.Net_PlayerDisconnected(cInfo);
	}

	public string GetServerPorts(int _basePort)
	{
		return "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool Authenticate(ProductUserId _id, string _password)
	{
		bool flag = string.IsNullOrEmpty(serverPassword) || _password == serverPassword;
		Log.Out(string.Format("[EOS-P2PS] Received authentication package from {0}: {1} password", _id, flag ? "valid" : "invalid"));
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
	public void OnPlayerConnected(ProductUserId _id)
	{
		ClientInfo clientInfo = new ClientInfo
		{
			CrossplatformId = new UserIdentifierEos(_id),
			network = this,
			netConnection = new INetConnection[2]
		};
		for (int i = 0; i < 2; i++)
		{
			clientInfo.netConnection[i] = new NetConnectionSimple(i, clientInfo, null, clientInfo.InternalId.CombinedString, 1, 1120);
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.AddClient(clientInfo);
		SingletonMonoBehaviour<ConnectionManager>.Instance.Net_PlayerConnected(clientInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FlushBuffer(ProductUserId _id)
	{
		SendPacketOptions options = new SendPacketOptions
		{
			SocketId = socketId,
			LocalUserId = localUser,
			RemoteUserId = _id,
			Channel = 0,
			Reliability = PacketReliability.ReliableOrdered,
			AllowDelayedDelivery = true,
			Data = timeData
		};
		Result result;
		lock (AntiCheatCommon.LockObject)
		{
			result = ptpInterface.SendPacket(ref options);
		}
		if (result != Result.Success)
		{
			Log.Error($"[EOS-P2PS] Could not send ping package to client {_id}: {result.ToStringCached()}");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdatePing(ProductUserId _sourceId, byte[] _data, long _curTime)
	{
		long num = BitConverter.ToInt64(_data, 1);
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

	public int GetMaximumPacketSize(ClientInfo _cInfo, bool _reliable = false)
	{
		return 1170;
	}

	public int GetBadPacketCount(ClientInfo _cInfo)
	{
		return 0;
	}
}
