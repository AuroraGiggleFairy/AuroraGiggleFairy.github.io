using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platform.Shared;

public class FavoriteServers : IServerListInterface
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
	public static readonly WaitForSeconds serverCheckInterval = new WaitForSeconds(0.1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRefreshing;

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine detectCoroutine;

	public bool IsPrefiltered => false;

	public bool IsRefreshing => isRefreshing;

	public FavoriteServers()
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
			detectCoroutine = ThreadManager.StartCoroutine(detectFavoriteServers());
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
	public IEnumerator detectFavoriteServers()
	{
		while (isRefreshing)
		{
			Dictionary<ServerInfoCache.FavoritesHistoryKey, ServerInfoCache.FavoritesHistoryValue>.Enumerator dictEnumerator = ServerInfoCache.Instance.GetFavoriteServersEnumerator();
			bool flag = dictEnumerator.MoveNext();
			while (flag && isRefreshing)
			{
				KeyValuePair<ServerInfoCache.FavoritesHistoryKey, ServerInfoCache.FavoritesHistoryValue> current = dictEnumerator.Current;
				GameServerInfo gameServerInfo = new GameServerInfo();
				gameServerInfo.SetValue(GameInfoString.IP, current.Key.Address);
				gameServerInfo.SetValue(GameInfoInt.Port, current.Key.Port);
				gameServerInfo.IsFavorite = current.Value.IsFavorite;
				gameServerInfo.LastPlayedLinux = (int)current.Value.LastPlayedTime;
				ServerInformationTcpClient.RequestRules(gameServerInfo, _ignoreTimeouts: true, callback);
				yield return serverCheckInterval;
				try
				{
					flag = dictEnumerator.MoveNext();
				}
				catch (InvalidOperationException)
				{
					flag = false;
				}
			}
			dictEnumerator.Dispose();
			yield return refreshInterval;
		}
		detectCoroutine = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void callback(bool _success, string _message, GameServerInfo _gsi)
	{
		if (isRefreshing && _success)
		{
			gameServerFoundCallback?.Invoke(owner, _gsi, _gsi.IsFavorite ? EServerRelationType.Favorites : EServerRelationType.History);
		}
	}
}
