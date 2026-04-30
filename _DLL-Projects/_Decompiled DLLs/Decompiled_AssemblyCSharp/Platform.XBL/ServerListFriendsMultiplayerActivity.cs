using System;
using System.Collections.Generic;
using System.Threading;
using Unity.XGamingRuntime;

namespace Platform.XBL;

public class ServerListFriendsMultiplayerActivity : IServerListInterface
{
	[PublicizedFrom(EAccessModifier.Private)]
	public GameServerFoundCallback gameServerFoundCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public XblUser user;

	[PublicizedFrom(EAccessModifier.Private)]
	public IServerListInterface serverLookup;

	[PublicizedFrom(EAccessModifier.Private)]
	public XblSocialManagerUserGroupHandle userGroupHandle;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLoadingFriendsList;

	[PublicizedFrom(EAccessModifier.Private)]
	public int activitySearchCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sessionSearchCount;

	public bool IsPrefiltered => false;

	public bool IsRefreshing
	{
		get
		{
			if (!isLoadingFriendsList && activitySearchCount <= 0)
			{
				return sessionSearchCount > 0;
			}
			return true;
		}
	}

	public void Init(IPlatform _owner)
	{
		user = (XblUser)_owner.User;
		serverLookup = PlatformManager.CrossplatformPlatform?.ServerLookupInterface;
		if (serverLookup == null)
		{
			Log.Error("[XBL] no crossplatform server lookup interface provided, friends session search is not possible");
		}
	}

	public void RegisterGameServerFoundCallback(GameServerFoundCallback _serverFound, MaxResultsReachedCallback _maxResultsCallback, ServerSearchErrorCallback _sessionSearchErrorCallback)
	{
		if (serverLookup != null)
		{
			gameServerFoundCallback = _serverFound;
		}
	}

	public void StartSearch(IList<IServerListInterface.ServerFilter> _activeFilters)
	{
		if (serverLookup != null)
		{
			if (!user.SocialManager.TryCreateUserGroup(XblPresenceFilter.TitleOnline, XblRelationshipFilter.Friends, OnlineFriendsListUpdated, out var handle))
			{
				Log.Error("[XBL] could not create friends user group, friends session search will fail");
				return;
			}
			Log.Out("[XBL] ServerListFriendsMultiplayerActivity starting search");
			userGroupHandle = handle;
			isLoadingFriendsList = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnlineFriendsListUpdated(ulong[] _users)
	{
		isLoadingFriendsList = false;
		if (_users != null && _users.Length != 0)
		{
			Interlocked.Increment(ref activitySearchCount);
			user.MultiplayerActivityQueryManager.GetActivityAsync(_users, OnActivitiesRetrieved);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnActivitiesRetrieved(ulong[] _searchedXuids, List<XblMultiplayerActivityInfo> _results)
	{
		int num = 0;
		foreach (XblMultiplayerActivityInfo _result in _results)
		{
			if (!string.IsNullOrEmpty(_result.ConnectionString))
			{
				num++;
			}
		}
		if (num == 0 || gameServerFoundCallback == null)
		{
			Interlocked.Decrement(ref activitySearchCount);
			return;
		}
		ThreadManager.AddSingleTaskMainThread("SearchXboxActivitySessions", [PublicizedFrom(EAccessModifier.Internal)] (object param) =>
		{
			foreach (XblMultiplayerActivityInfo _result2 in _results)
			{
				if (_result2.JoinRestriction != XblMultiplayerActivityJoinRestriction.InviteOnly && !string.IsNullOrEmpty(_result2.ConnectionString))
				{
					GameServerInfo gameServerInfo = new GameServerInfo();
					gameServerInfo.SetValue(GameInfoString.UniqueId, _result2.ConnectionString);
					Interlocked.Increment(ref sessionSearchCount);
					serverLookup.GetSingleServerDetails(gameServerInfo, EServerRelationType.Friends, OnServerFound);
				}
			}
			Interlocked.Decrement(ref activitySearchCount);
		});
	}

	public void OnServerFound(IPlatform _sourcePlatform, GameServerInfo _info, EServerRelationType _source)
	{
		if (_info != null)
		{
			_info.IsFriends = true;
			gameServerFoundCallback?.Invoke(_sourcePlatform, _info, _source);
			Interlocked.Decrement(ref sessionSearchCount);
		}
	}

	public void StopSearch()
	{
		if (userGroupHandle != null)
		{
			user.SocialManager.DestroyUserGroup(userGroupHandle);
		}
		userGroupHandle = null;
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
}
