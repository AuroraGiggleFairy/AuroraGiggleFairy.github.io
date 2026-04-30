using System.Collections.Generic;

namespace Discord;

internal interface IPresence
{
	UserStatus Status { get; }

	IReadOnlyCollection<ClientType> ActiveClients { get; }

	IReadOnlyCollection<IActivity> Activities { get; }
}
