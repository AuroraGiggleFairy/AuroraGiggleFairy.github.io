using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace Platform.Steam;

public abstract class LobbyListAbs : IServerListInterface
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameServerFoundCallback gameServerFoundCallback;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isRefreshing;

	public bool IsPrefiltered => false;

	public bool IsRefreshing => isRefreshing;

	[PublicizedFrom(EAccessModifier.Protected)]
	public LobbyListAbs()
	{
		if (!GameManager.IsDedicatedServer)
		{
			Application.wantsToQuit += OnApplicationQuit;
		}
	}

	public abstract void Init(IPlatform _owner);

	public void RegisterGameServerFoundCallback(GameServerFoundCallback _serverFound, MaxResultsReachedCallback _maxResultsCallback, ServerSearchErrorCallback _errorCallback)
	{
		gameServerFoundCallback = _serverFound;
	}

	public abstract void StartSearch(IList<IServerListInterface.ServerFilter> _activeFilters);

	public abstract void StopSearch();

	public virtual void Disconnect()
	{
		StopSearch();
		gameServerFoundCallback = null;
	}

	public void GetSingleServerDetails(GameServerInfo _serverInfo, EServerRelationType _relation, GameServerFoundCallback _callback)
	{
		throw new NotImplementedException();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnApplicationQuit()
	{
		Disconnect();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator restartRefreshCo(float _delay)
	{
		yield return new WaitForSeconds(_delay);
		StartSearch(null);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ParseLobbyData(CSteamID _lobbyId, EServerRelationType _source)
	{
		if (gameServerFoundCallback == null)
		{
			return;
		}
		GameServerInfo gameServerInfo = new GameServerInfo
		{
			IsLobby = true
		};
		int lobbyDataCount = SteamMatchmaking.GetLobbyDataCount(_lobbyId);
		for (int i = 0; i < lobbyDataCount; i++)
		{
			if (SteamMatchmaking.GetLobbyDataByIndex(_lobbyId, i, out var pchKey, 100, out var pchValue, 200))
			{
				gameServerInfo.ParseAny(pchKey, pchValue);
			}
		}
		if (PlatformManager.CrossplatformPlatform == null)
		{
			gameServerInfo.SetValue(GameInfoString.UniqueId, gameServerInfo.GetValue(GameInfoString.SteamID));
		}
		gameServerInfo.IsFriends = _source == EServerRelationType.Friends;
		gameServerFoundCallback(owner, gameServerInfo, _source);
	}
}
