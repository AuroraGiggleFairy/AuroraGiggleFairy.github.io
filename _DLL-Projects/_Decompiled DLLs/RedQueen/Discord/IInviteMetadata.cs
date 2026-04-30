using System;

namespace Discord;

internal interface IInviteMetadata : IInvite, IEntity<string>, IDeletable
{
	bool IsTemporary { get; }

	int? MaxAge { get; }

	int? MaxUses { get; }

	int? Uses { get; }

	DateTimeOffset? CreatedAt { get; }
}
