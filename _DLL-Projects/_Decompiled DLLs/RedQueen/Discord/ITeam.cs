using System.Collections.Generic;

namespace Discord;

internal interface ITeam
{
	string IconUrl { get; }

	ulong Id { get; }

	IReadOnlyList<ITeamMember> TeamMembers { get; }

	string Name { get; }

	ulong OwnerUserId { get; }
}
