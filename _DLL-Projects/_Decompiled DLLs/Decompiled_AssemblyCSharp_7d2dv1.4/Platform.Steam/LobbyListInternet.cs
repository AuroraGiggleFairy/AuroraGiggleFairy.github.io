using System.Collections.Generic;
using Steamworks;

namespace Platform.Steam;

public class LobbyListInternet : LobbyListAbs
{
	[PublicizedFrom(EAccessModifier.Private)]
	public CallResult<LobbyMatchList_t> m_RequestLobbies;

	public override void Init(IPlatform _owner)
	{
		owner = _owner;
		_owner.Api.ClientApiInitialized += [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (m_RequestLobbies == null && !GameManager.IsDedicatedServer)
			{
				m_RequestLobbies = CallResult<LobbyMatchList_t>.Create(RequestLobbies_CallResult);
			}
		};
	}

	public override void StopSearch()
	{
		if (m_RequestLobbies != null && m_RequestLobbies.IsActive())
		{
			m_RequestLobbies.Cancel();
		}
		isRefreshing = false;
	}

	public override void StartSearch(IList<IServerListInterface.ServerFilter> _activeFilters)
	{
		if (gameServerFoundCallback != null)
		{
			SteamMatchmaking.AddRequestLobbyListStringFilter("CompatibilityVersion", global::Constants.cVersionInformation.LongStringNoBuild, ELobbyComparison.k_ELobbyComparisonEqual);
			SteamAPICall_t hAPICall = SteamMatchmaking.RequestLobbyList();
			m_RequestLobbies.Set(hAPICall);
			isRefreshing = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RequestLobbies_CallResult(LobbyMatchList_t _val, bool _ioFailure)
	{
		if (_ioFailure)
		{
			Log.Out("[Steamworks.NET] RequestLobbies failed");
		}
		else
		{
			for (int i = 0; i < _val.m_nLobbiesMatching; i++)
			{
				ParseLobbyData(SteamMatchmaking.GetLobbyByIndex(i), EServerRelationType.Internet);
			}
		}
		ThreadManager.StartCoroutine(restartRefreshCo(3f));
	}
}
