using System;

namespace Discord;

internal interface IRateLimitInfo
{
	bool IsGlobal { get; }

	int? Limit { get; }

	int? Remaining { get; }

	int? RetryAfter { get; }

	DateTimeOffset? Reset { get; }

	TimeSpan? ResetAfter { get; }

	string Bucket { get; }

	TimeSpan? Lag { get; }

	string Endpoint { get; }
}
