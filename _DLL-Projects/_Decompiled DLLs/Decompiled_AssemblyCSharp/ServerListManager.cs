using System.Collections.Generic;
using Platform;
using UnityEngine;

public class ServerListManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static ServerListManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<IServerListInterface> serverLists = new List<IServerListInterface>();

	public static ServerListManager Instance => instance ?? (instance = new ServerListManager());

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsRefreshing
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsPrefilteredSearch { get; }

	public void StartSearch(List<IServerListInterface.ServerFilter> _activeFilters)
	{
		IsRefreshing = true;
		foreach (IServerListInterface serverList in serverLists)
		{
			serverList.StartSearch(_activeFilters);
		}
	}

	public void StopSearch()
	{
		IsRefreshing = false;
		foreach (IServerListInterface serverList in serverLists)
		{
			serverList.StopSearch();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ServerListManager()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		Application.wantsToQuit += [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			Disconnect();
			return true;
		};
		IList<IServerListInterface> serverListInterfaces = PlatformManager.MultiPlatform.ServerListInterfaces;
		if (serverListInterfaces != null)
		{
			serverLists.AddRange(serverListInterfaces);
		}
		foreach (IServerListInterface serverList in serverLists)
		{
			if (serverList.IsPrefiltered)
			{
				IsPrefilteredSearch = true;
			}
		}
	}

	public void Disconnect()
	{
		foreach (IServerListInterface serverList in serverLists)
		{
			serverList.Disconnect();
		}
		IsRefreshing = false;
	}

	public void RegisterGameServerFoundCallback(GameServerFoundCallback _serverFound, MaxResultsReachedCallback _maxResultsCallback, ServerSearchErrorCallback _errorCallback)
	{
		foreach (IServerListInterface serverList in serverLists)
		{
			serverList.RegisterGameServerFoundCallback(_serverFound, _maxResultsCallback, _errorCallback);
		}
	}
}
