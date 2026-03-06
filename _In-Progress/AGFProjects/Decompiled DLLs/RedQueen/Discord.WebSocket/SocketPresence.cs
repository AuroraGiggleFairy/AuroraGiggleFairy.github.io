using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Discord.API;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class SocketPresence : IPresence
{
	public UserStatus Status { get; private set; }

	public IReadOnlyCollection<ClientType> ActiveClients { get; private set; }

	public IReadOnlyCollection<IActivity> Activities { get; private set; }

	private string DebuggerDisplay => string.Format("{0}{1}", Status, Activities?.FirstOrDefault()?.Name ?? "");

	internal SocketPresence()
	{
	}

	internal SocketPresence(UserStatus status, IImmutableSet<ClientType> activeClients, IImmutableList<IActivity> activities)
	{
		Status = status;
		ActiveClients = activeClients ?? ImmutableHashSet<ClientType>.Empty;
		Activities = activities ?? ImmutableList<IActivity>.Empty;
	}

	internal static SocketPresence Create(Presence model)
	{
		SocketPresence socketPresence = new SocketPresence();
		socketPresence.Update(model);
		return socketPresence;
	}

	internal void Update(Presence model)
	{
		Status = model.Status;
		ActiveClients = (IReadOnlyCollection<ClientType>)(ConvertClientTypesDict(model.ClientStatus.GetValueOrDefault()) ?? ((object)System.Collections.Immutable.ImmutableArray<ClientType>.Empty));
		Activities = (IReadOnlyCollection<IActivity>)(ConvertActivitiesList(model.Activities) ?? ((object)System.Collections.Immutable.ImmutableArray<IActivity>.Empty));
	}

	private static IReadOnlyCollection<ClientType> ConvertClientTypesDict(IDictionary<string, string> clientTypesDict)
	{
		if (clientTypesDict == null || clientTypesDict.Count == 0)
		{
			return ImmutableHashSet<ClientType>.Empty;
		}
		HashSet<ClientType> hashSet = new HashSet<ClientType>();
		foreach (string key in clientTypesDict.Keys)
		{
			if (Enum.TryParse<ClientType>(key, ignoreCase: true, out var result))
			{
				hashSet.Add(result);
			}
		}
		return hashSet.ToImmutableHashSet();
	}

	private static IImmutableList<IActivity> ConvertActivitiesList(IList<Discord.API.Game> activities)
	{
		if (activities == null || activities.Count == 0)
		{
			return ImmutableList<IActivity>.Empty;
		}
		List<IActivity> list = new List<IActivity>();
		foreach (Discord.API.Game activity in activities)
		{
			list.Add(activity.ToEntity());
		}
		return list.ToImmutableList();
	}

	public override string ToString()
	{
		return Status.ToString();
	}

	internal SocketPresence Clone()
	{
		return MemberwiseClone() as SocketPresence;
	}
}
