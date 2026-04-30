using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Discord;

internal struct DiscordJsonError
{
	public string Path
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public IReadOnlyCollection<DiscordError> Errors
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	internal DiscordJsonError(string path, DiscordError[] errors)
	{
		Path = path;
		Errors = errors.ToImmutableArray();
	}
}
