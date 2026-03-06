using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace Platform.Steam;

public class MasterServerList : IServerListInterface
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int compatVersionInt;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly EServerRelationType source;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRefreshing;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameServerFoundCallback gameServerFoundCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public ISteamMatchmakingServerListResponse matchmakingServerListResponse;

	[PublicizedFrom(EAccessModifier.Private)]
	public HServerListRequest requestHandle = HServerListRequest.Invalid;

	public bool IsPrefiltered => false;

	public bool IsRefreshing => isRefreshing;

	public MasterServerList(EServerRelationType _source)
	{
		if (!GameManager.IsDedicatedServer)
		{
			Application.wantsToQuit += OnApplicationQuit;
			source = _source;
			compatVersionInt = int.Parse(Constants.SteamVersionNr.Replace(".", ""));
		}
	}

	public void Init(IPlatform _owner)
	{
		owner = _owner;
		_owner.Api.ClientApiInitialized += [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (matchmakingServerListResponse == null && !GameManager.IsDedicatedServer)
			{
				matchmakingServerListResponse = new ISteamMatchmakingServerListResponse(ServerResponded, ServerFailedToRespond, RefreshComplete);
			}
		};
	}

	public void RegisterGameServerFoundCallback(GameServerFoundCallback _serverFound, MaxResultsReachedCallback _maxResultsCallback, ServerSearchErrorCallback _errorCallback)
	{
		gameServerFoundCallback = _serverFound;
	}

	public void StartSearch(IList<IServerListInterface.ServerFilter> _activeFilters)
	{
		if (gameServerFoundCallback != null)
		{
			if (requestHandle != HServerListRequest.Invalid)
			{
				SteamMatchmakingServers.ReleaseRequest(requestHandle);
				requestHandle = HServerListRequest.Invalid;
			}
			MatchMakingKeyValuePair_t[] array = new MatchMakingKeyValuePair_t[0];
			requestHandle = source switch
			{
				EServerRelationType.Internet => SteamMatchmakingServers.RequestInternetServerList((AppId_t)251570u, array, (uint)array.Length, matchmakingServerListResponse), 
				EServerRelationType.LAN => SteamMatchmakingServers.RequestLANServerList((AppId_t)251570u, matchmakingServerListResponse), 
				EServerRelationType.Friends => SteamMatchmakingServers.RequestFriendsServerList((AppId_t)251570u, array, (uint)array.Length, matchmakingServerListResponse), 
				EServerRelationType.Favorites => SteamMatchmakingServers.RequestFavoritesServerList((AppId_t)251570u, array, (uint)array.Length, matchmakingServerListResponse), 
				EServerRelationType.History => SteamMatchmakingServers.RequestHistoryServerList((AppId_t)251570u, array, (uint)array.Length, matchmakingServerListResponse), 
				EServerRelationType.Spectator => SteamMatchmakingServers.RequestSpectatorServerList((AppId_t)251570u, array, (uint)array.Length, matchmakingServerListResponse), 
				_ => requestHandle, 
			};
			isRefreshing = true;
		}
	}

	public void StopSearch()
	{
		if (requestHandle != HServerListRequest.Invalid)
		{
			SteamMatchmakingServers.ReleaseRequest(requestHandle);
			requestHandle = HServerListRequest.Invalid;
		}
		isRefreshing = false;
	}

	public void Disconnect()
	{
		StopSearch();
		gameServerFoundCallback = null;
	}

	public void GetSingleServerDetails(GameServerInfo _serverInfo, EServerRelationType _relation, GameServerFoundCallback _callback)
	{
		throw new NotImplementedException();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool OnApplicationQuit()
	{
		StopSearch();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ServerResponded(HServerListRequest _hRequest, int _iServer)
	{
		gameserveritem_t serverDetails = SteamMatchmakingServers.GetServerDetails(_hRequest, _iServer);
		if (serverDetails.m_nServerVersion == compatVersionInt || source == EServerRelationType.Favorites || source == EServerRelationType.History || source == EServerRelationType.LAN)
		{
			GameServerInfo gameServerInfo = new GameServerInfo();
			gameServerInfo.SetValue(GameInfoInt.Ping, serverDetails.m_nPing);
			gameServerInfo.SetValue(GameInfoString.IP, NetworkUtils.ToAddr(serverDetails.m_NetAdr.GetIP()));
			gameServerInfo.SetValue(GameInfoInt.Port, serverDetails.m_NetAdr.GetQueryPort());
			gameServerInfo.SetValue(GameInfoString.SteamID, serverDetails.m_steamID.ToString());
			gameServerInfo.SetValue(GameInfoString.UniqueId, serverDetails.m_steamID.ToString());
			gameServerInfo.SetValue(GameInfoString.LevelName, serverDetails.GetMap());
			gameServerInfo.SetValue(GameInfoInt.CurrentPlayers, serverDetails.m_nPlayers);
			gameServerInfo.SetValue(GameInfoInt.MaxPlayers, serverDetails.m_nMaxPlayers);
			gameServerInfo.SetValue(GameInfoBool.IsPasswordProtected, serverDetails.m_bPassword);
			gameServerInfo.SetValue(GameInfoString.GameHost, serverDetails.GetServerName());
			gameServerInfo.LastPlayedLinux = (int)serverDetails.m_ulTimeLastPlayed;
			switch (source)
			{
			case EServerRelationType.LAN:
				gameServerInfo.IsLAN = true;
				break;
			case EServerRelationType.Friends:
				gameServerInfo.IsFriends = true;
				break;
			case EServerRelationType.Favorites:
				gameServerInfo.IsFavorite = true;
				break;
			}
			if (NetworkUtils.ParseGameTags(serverDetails.GetGameTags(), gameServerInfo))
			{
				gameServerFoundCallback?.Invoke(owner, gameServerInfo, source);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ServerFailedToRespond(HServerListRequest _hRequest, int _iServer)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshComplete(HServerListRequest _hRequest, EMatchMakingServerResponse _response)
	{
		ThreadManager.StartCoroutine(restartRefreshCo());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator restartRefreshCo()
	{
		yield return new WaitForSeconds(4f);
		StartSearch(null);
	}
}
