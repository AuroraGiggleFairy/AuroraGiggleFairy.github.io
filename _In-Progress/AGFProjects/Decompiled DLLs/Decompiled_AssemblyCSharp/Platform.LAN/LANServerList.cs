using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Platform.LAN;

public class LANServerList : IServerListInterface
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int serverBroadcastIntervalSeconds = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TimeSpan rulesRefreshInterval = new TimeSpan(0, 2, 0);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TimeSpan knownServerTimeout = new TimeSpan(0, 1, 0);

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameServerFoundCallback serverFoundCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IPAddress multicastGroupIp;

	[PublicizedFrom(EAccessModifier.Private)]
	public UdpClient udpClient;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldRefresh;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPaused;

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine requestCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine receiveCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public UdpClientSendHandler sendHandler;

	[PublicizedFrom(EAccessModifier.Private)]
	public UdpClientReceiveHandler receiveHandler;

	[PublicizedFrom(EAccessModifier.Private)]
	public LANServerCacheControl cacheControl = new LANServerCacheControl(rulesRefreshInterval, knownServerTimeout);

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] emptyMessage = new byte[0];

	public bool IsPrefiltered => false;

	public bool IsRefreshing => shouldRefresh;

	public void Init(IPlatform _owner)
	{
		owner = _owner;
	}

	public void RegisterGameServerFoundCallback(GameServerFoundCallback _serverFound, MaxResultsReachedCallback _maxResultsCallback, ServerSearchErrorCallback _sessionSearchErrorCallback)
	{
		serverFoundCallback = _serverFound;
	}

	public void GetSingleServerDetails(GameServerInfo _serverInfo, EServerRelationType _relation, GameServerFoundCallback _callback)
	{
		throw new NotImplementedException();
	}

	public void StartSearch(IList<IServerListInterface.ServerFilter> _activeFilters)
	{
		try
		{
			shouldRefresh = true;
			isPaused = false;
			udpClient = new UdpClient(AddressFamily.InterNetwork);
			sendHandler = new UdpClientSendHandler(udpClient);
			receiveHandler = new UdpClientReceiveHandler(udpClient);
			requestCoroutine = ThreadManager.StartCoroutine(LANServerInfoRequestCoroutine());
			receiveCoroutine = ThreadManager.StartCoroutine(LANServerInfoReceiveCoroutine());
		}
		catch (Exception e)
		{
			Log.Error("[LANServerList] Could not start LAN server search");
			Log.Exception(e);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator LANServerInfoRequestCoroutine()
	{
		while (shouldRefresh)
		{
			IPEndPoint endPoint = new IPEndPoint(LANServerSearchConfig.MulticastGroupIp, 11000);
			if (!sendHandler.BeginSend(emptyMessage, 0, endPoint))
			{
				break;
			}
			while (!sendHandler.isComplete)
			{
				yield return null;
			}
			yield return new WaitForSeconds(5f);
			while (isPaused)
			{
				yield return null;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator LANServerInfoReceiveCoroutine()
	{
		while (shouldRefresh && receiveHandler.BeginReceive())
		{
			while (!receiveHandler.isComplete)
			{
				yield return null;
			}
			while (isPaused)
			{
				yield return null;
			}
			IPEndPoint remoteEP = receiveHandler.remoteEP;
			byte[] message = receiveHandler.message;
			int length = receiveHandler.length;
			if (remoteEP != null && message != null && length == 4)
			{
				int offset = 0;
				int port = StreamUtils.ReadInt32(message, ref offset);
				IPEndPoint iPEndPoint = remoteEP;
				iPEndPoint.Port = port;
				OnServerFound(iPEndPoint);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnServerFound(IPEndPoint endpoint)
	{
		string addressString = endpoint.Address.ToString();
		if (cacheControl.IsUpdateRequired(addressString, endpoint.Port))
		{
			GameServerInfo gameServerInfo = new GameServerInfo();
			gameServerInfo.SetValue(GameInfoString.IP, endpoint.Address.ToString());
			gameServerInfo.SetValue(GameInfoInt.Port, endpoint.Port);
			ServerInformationTcpClient.RequestRules(gameServerInfo, _ignoreTimeouts: false, OnRulesRequestDone);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnRulesRequestDone(bool _success, string _message, GameServerInfo _gsi)
	{
		cacheControl.SetUpdated(_gsi.GetValue(GameInfoString.IP), _gsi.GetValue(GameInfoInt.Port));
		_gsi.IsLAN = true;
		serverFoundCallback(owner, _gsi, EServerRelationType.LAN);
	}

	public void StopSearch()
	{
		isPaused = true;
		cacheControl.Clear();
	}

	public void Disconnect()
	{
		StopSearch();
		isPaused = false;
		shouldRefresh = false;
		if (requestCoroutine != null)
		{
			ThreadManager.StopCoroutine(requestCoroutine);
		}
		if (receiveCoroutine != null)
		{
			ThreadManager.StopCoroutine(receiveCoroutine);
		}
		udpClient?.Dispose();
		udpClient = null;
	}
}
