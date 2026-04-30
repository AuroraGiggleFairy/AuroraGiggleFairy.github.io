using System;
using System.Collections;
using System.Text;
using System.Threading;
using Steamworks;
using UnityEngine.Networking;

namespace Platform.Steam;

public class NetworkClientSteam : IPlatformNetworkClient, INetworkClient
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IProtocolManagerProtocolInterface protoManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public Callback<P2PSessionConnectFail_t> m_P2PSessionConnectFail;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo handlerThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly AutoResetEvent signalThread = new AutoResetEvent(initialState: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BlockingQueue<NetworkCommonSteam.SendInfo> sendBufs = new BlockingQueue<NetworkCommonSteam.SendInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool flushBuffers;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool packetsPendingSend;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool anyPacketsSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public CSteamID serverId = CSteamID.Nil;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool connecting;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool disconnectEventReceived;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly byte[] recvBuf = new byte[1048576];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] emptyData = new byte[0];

	public bool IsConnected
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (serverId != CSteamID.Nil && handlerThread != null && (protoManager.IsClient || connecting))
			{
				return owner.User.UserStatus == EUserStatus.LoggedIn;
			}
			return false;
		}
	}

	public NetworkClientSteam(IPlatform _owner, IProtocolManagerProtocolInterface _protoManager)
	{
		owner = _owner;
		protoManager = _protoManager;
		owner.Api.ClientApiInitialized += [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (!GameManager.IsDedicatedServer)
			{
				m_P2PSessionConnectFail = Callback<P2PSessionConnectFail_t>.Create(P2PSessionConnectFail);
			}
		};
	}

	public void Connect(GameServerInfo _gsi)
	{
		disconnectEventReceived = false;
		anyPacketsSent = false;
		Log.Out("NET: Steam NW trying to connect to: " + _gsi.GetValue(GameInfoString.IP) + ":" + _gsi.GetValue(GameInfoInt.Port));
		if (string.IsNullOrEmpty(_gsi.GetValue(GameInfoString.SteamID)))
		{
			Log.Out("[Steamworks.NET] NET: Resolving SteamID for IP " + _gsi.GetValue(GameInfoString.IP) + ":" + _gsi.GetValue(GameInfoInt.Port));
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
		if (string.IsNullOrEmpty(_gsi.GetValue(GameInfoString.SteamID)))
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
			Encoding.UTF8.GetBytes(password, 0, password.Length, arrayListMP.Items, 0);
		}
		else
		{
			arrayListMP = new ArrayListMP<byte>(MemoryPools.poolByte, 1)
			{
				Count = 1
			};
		}
		serverId = new CSteamID(ulong.Parse(_gsi.GetValue(GameInfoString.SteamID)));
		CSteamID cSteamID = serverId;
		Log.Out("[Steamworks.NET] NET: Connecting to SteamID " + cSteamID.ToString());
		if (handlerThread == null)
		{
			handlerThread = ThreadManager.StartThread("SteamNetworkingClient", threadHandlerMethod, null, null, true, false);
		}
		connecting = true;
		SendData(50, arrayListMP);
	}

	public void Disconnect()
	{
		connecting = false;
		if (serverId != CSteamID.Nil)
		{
			sendBufs.Clear();
		}
		if (handlerThread != null)
		{
			signalThread.Set();
			handlerThread.WaitForEnd();
			handlerThread = null;
			serverId = CSteamID.Nil;
		}
	}

	public NetworkError SendData(int _channel, ArrayListMP<byte> _data)
	{
		if (IsConnected)
		{
			CSteamID recipient = serverId;
			_data[_data.Count - 1] = (byte)_channel;
			sendBufs.Enqueue(new NetworkCommonSteam.SendInfo(recipient, _data));
			signalThread.Set();
		}
		else
		{
			Log.Warning("[Steamworks.NET] NET: Tried to send a package while not connected to a server");
		}
		return NetworkError.Ok;
	}

	public void Update()
	{
		if (IsConnected && disconnectEventReceived)
		{
			OnDisconnectedFromServer();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator connectionFailedLater(string _message)
	{
		yield return null;
		yield return null;
		protoManager.ConnectionFailedEv(_message);
	}

	public void LateUpdate()
	{
		flushBuffers = true;
		signalThread.Set();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisconnectedFromServer()
	{
		Disconnect();
		protoManager.DisconnectedFromServerEv(Localization.Get("netSteamNetworking_ConnectionClosedByServer"));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void P2PSessionConnectFail(P2PSessionConnectFail_t _par)
	{
		if (connecting)
		{
			Disconnect();
			Log.Out("[Steamworks.NET] NET: P2PSessionConnectFail to: " + _par.m_steamIDRemote.m_SteamID + " - Error: " + ((EP2PSessionError)_par.m_eP2PSessionError).ToStringCached());
			string msg = Localization.Get("netSteamNetworkingSessionError_" + ((EP2PSessionError)_par.m_eP2PSessionError).ToStringCached());
			protoManager.ConnectionFailedEv(msg);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void threadHandlerMethod(ThreadManager.ThreadInfo _threadinfo)
	{
		while (!_threadinfo.TerminationRequested())
		{
			if (!IsConnected)
			{
				signalThread.WaitOne(100);
				continue;
			}
			signalThread.WaitOne(6);
			if (anyPacketsSent)
			{
				CheckConnection();
			}
			ReceivePackets();
			while (sendBufs.HasData())
			{
				NetworkCommonSteam.SendInfo sendInfo = sendBufs.Dequeue();
				if (!SteamNetworking.SendP2PPacket(sendInfo.Recipient, sendInfo.Data.Items, (uint)sendInfo.Data.Count, EP2PSend.k_EP2PSendReliableWithBuffering))
				{
					Log.Error("[Steamworks.NET] NET: Could not send package to server");
					continue;
				}
				packetsPendingSend = true;
				anyPacketsSent = true;
			}
			if (flushBuffers && packetsPendingSend)
			{
				packetsPendingSend = false;
				flushBuffers = false;
				FlushBuffer();
			}
		}
		if (serverId != CSteamID.Nil)
		{
			SteamNetworking.CloseP2PSessionWithUser(serverId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckConnection()
	{
		if (SteamNetworking.GetP2PSessionState(serverId, out var pConnectionState))
		{
			if (pConnectionState.m_bConnectionActive != 0 && connecting)
			{
				connecting = false;
				Log.Out("[Steamworks.NET] NET: Connection established");
			}
			else if (pConnectionState.m_bConnecting == 0 && pConnectionState.m_bConnectionActive == 0 && protoManager.IsClient)
			{
				Log.Out("[Steamworks.NET] NET: Connection closed");
				disconnectEventReceived = true;
			}
		}
		else if (protoManager.IsClient)
		{
			Log.Out("[Steamworks.NET] NET: Connection closed");
			disconnectEventReceived = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReceivePackets()
	{
		uint pcubMsgSize;
		CSteamID psteamIDRemote;
		bool flag = SteamNetworking.ReadP2PPacket(recvBuf, (uint)recvBuf.Length, out pcubMsgSize, out psteamIDRemote);
		while (flag)
		{
			if (pcubMsgSize != 0)
			{
				pcubMsgSize--;
				NetworkCommonSteam.ESteamNetChannels eSteamNetChannels = (NetworkCommonSteam.ESteamNetChannels)recvBuf[pcubMsgSize];
				switch (eSteamNetChannels)
				{
				case NetworkCommonSteam.ESteamNetChannels.Authentication:
					if (connecting)
					{
						connecting = false;
						Log.Out("[Steamworks.NET] NET: Connection established");
					}
					if (recvBuf[0] == 0)
					{
						Log.Out("[Steamworks.NET] NET: Received invalid password package");
						ThreadManager.AddSingleTaskMainThread("SteamNetInvalidPassword", [PublicizedFrom(EAccessModifier.Private)] (object _info) =>
						{
							protoManager.InvalidPasswordEv();
						});
					}
					else
					{
						Log.Out("[Steamworks.NET] NET: Password accepted");
						OnConnectedToServer();
					}
					break;
				case NetworkCommonSteam.ESteamNetChannels.Ping:
				{
					ArrayListMP<byte> arrayListMP = new ArrayListMP<byte>(MemoryPools.poolByte, (int)(pcubMsgSize + 1));
					Array.Copy(recvBuf, arrayListMP.Items, pcubMsgSize);
					arrayListMP.Count = (int)(pcubMsgSize + 1);
					SendData(60, arrayListMP);
					break;
				}
				case NetworkCommonSteam.ESteamNetChannels.NetpackageChannel0:
				case NetworkCommonSteam.ESteamNetChannels.NetpackageChannel1:
					if (pcubMsgSize != 0)
					{
						byte[] array = MemoryPools.poolByte.Alloc((int)pcubMsgSize);
						Array.Copy(recvBuf, array, pcubMsgSize);
						SingletonMonoBehaviour<ConnectionManager>.Instance.Net_DataReceivedClient((int)eSteamNetChannels, array, (int)pcubMsgSize);
					}
					break;
				}
			}
			flag = SteamNetworking.ReadP2PPacket(recvBuf, (uint)recvBuf.Length, out pcubMsgSize, out psteamIDRemote);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FlushBuffer()
	{
		if (!SteamNetworking.SendP2PPacket(serverId, emptyData, 0u, EP2PSend.k_EP2PSendReliable))
		{
			Log.Error("[Steamworks.NET] NET: Could not flush the data buffer");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnConnectedToServer()
	{
		INetConnection[] array = new INetConnection[2];
		for (int i = 0; i < 2; i++)
		{
			array[i] = new NetConnectionSteam(i, null, this, null);
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
