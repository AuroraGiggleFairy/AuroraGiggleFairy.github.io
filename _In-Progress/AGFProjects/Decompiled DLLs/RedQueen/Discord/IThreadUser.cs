using System;

namespace Discord;

internal interface IThreadUser : IMentionable
{
	IThreadChannel Thread { get; }

	DateTimeOffset ThreadJoinedAt { get; }

	IGuild Guild { get; }
}
