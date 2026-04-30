using System;

namespace Discord.Net;

internal interface IRequest
{
	DateTimeOffset? TimeoutAt { get; }

	RequestOptions Options { get; }
}
