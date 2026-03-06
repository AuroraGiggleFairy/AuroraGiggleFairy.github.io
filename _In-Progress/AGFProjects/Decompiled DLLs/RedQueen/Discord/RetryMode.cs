using System;

namespace Discord;

[Flags]
internal enum RetryMode
{
	AlwaysFail = 0,
	RetryTimeouts = 1,
	RetryRatelimit = 4,
	Retry502 = 8,
	AlwaysRetry = 0xD
}
