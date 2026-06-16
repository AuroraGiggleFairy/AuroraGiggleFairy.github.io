using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platform.Shared;

public class LocalServerDetect : IServerListInterface
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool initDone;

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameServerFoundCallback gameServerFoundCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly WaitForSeconds refreshInterval = new WaitForSeconds(3f);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRefreshing;

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine detectCoroutine;

	public bool IsPrefiltered => false;

	public bool IsRefreshing => isRefreshing;

	public LocalServerDetect()
	{
		if (!GameManager.IsDedicatedServer)
		{
			Application.wantsToQuit += [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				Disconnect();
				return true;
			};
		}
	}

	public void Init(IPlatform _owner)
	{
		if (!GameManager.IsDedicatedServer && !initDone)
		{
			owner = _owner;
		}
	}

	public void RegisterGameServerFoundCallback(GameServerFoundCallback _serverFound, MaxResultsReachedCallback _maxResultsCallback, ServerSearchErrorCallback _errorCallback)
	{
		gameServerFoundCallback = _serverFound;
	}

	public void StartSearch(IList<IServerListInterface.ServerFilter> _activeFilters)
	{
		isRefreshing = true;
		if (detectCoroutine == null)
		{
			detectCoroutine = ThreadManager.StartCoroutine(detectLocalServers());
		}
	}

	public void StopSearch()
	{
		isRefreshing = false;
		detectCoroutine = null;
	}

	public void Disconnect()
	{
		isRefreshing = false;
	}

	public void GetSingleServerDetails(GameServerInfo _serverInfo, EServerRelationType _relation, GameServerFoundCallback _callback)
	{
		throw new NotImplementedException();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator detectLocalServers()
	{
		while (isRefreshing)
		{
			GameServerInfo gameServerInfo = new GameServerInfo();
			gameServerInfo.SetValue(GameInfoString.IP, "127.0.0.1");
			gameServerInfo.SetValue(GameInfoInt.Port, 26900);
			ServerInformationTcpClient.RequestRules(gameServerInfo, _ignoreTimeouts: true, callback);
			yield return refreshInterval;
			GameServerInfo gameServerInfo2 = new GameServerInfo();
			gameServerInfo2.SetValue(GameInfoString.IP, "127.0.0.1");
			gameServerInfo2.SetValue(GameInfoInt.Port, 27020);
			ServerInformationTcpClient.RequestRules(gameServerInfo2, _ignoreTimeouts: true, callback);
			yield return refreshInterval;
		}
		detectCoroutine = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void callback(bool _success, string _message, GameServerInfo _gsi)
	{
		if (isRefreshing && _success)
		{
			_gsi.IsLAN = true;
			gameServerFoundCallback?.Invoke(owner, _gsi, EServerRelationType.LAN);
		}
	}
}
