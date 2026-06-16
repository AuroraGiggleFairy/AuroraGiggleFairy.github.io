using System.Collections.Generic;
using Steamworks;

namespace Platform.Steam;

public class LobbyListFriends : LobbyListAbs
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Callback<LobbyDataUpdate_t> m_lobbyDataUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentFriend;

	public override void Init(IPlatform _owner)
	{
		owner = _owner;
		_owner.Api.ClientApiInitialized += [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (m_lobbyDataUpdate == null && !GameManager.IsDedicatedServer)
			{
				m_lobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(Lobby_DataUpdate);
			}
		};
	}

	public override void StopSearch()
	{
		currentFriend = -1;
		isRefreshing = false;
	}

	public override void StartSearch(IList<IServerListInterface.ServerFilter> _activeFilters)
	{
		if (gameServerFoundCallback != null)
		{
			isRefreshing = true;
			currentFriend = 0;
			queryNextFriend();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void queryNextFriend()
	{
		while (currentFriend < SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagAll))
		{
			if (SteamFriends.GetFriendGamePlayed(SteamFriends.GetFriendByIndex(currentFriend, EFriendFlags.k_EFriendFlagAll), out var pFriendGameInfo) && pFriendGameInfo.m_steamIDLobby != CSteamID.Nil)
			{
				SteamMatchmaking.RequestLobbyData(pFriendGameInfo.m_steamIDLobby);
				return;
			}
			currentFriend++;
		}
		ThreadManager.StartCoroutine(restartRefreshCo(2f));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Lobby_DataUpdate(LobbyDataUpdate_t _val)
	{
		CSteamID lobbyId = new CSteamID(_val.m_ulSteamIDLobby);
		if (_val.m_bSuccess != 0)
		{
			ParseLobbyData(lobbyId, EServerRelationType.Friends);
			if (currentFriend >= 0)
			{
				currentFriend++;
				queryNextFriend();
			}
		}
	}
}
