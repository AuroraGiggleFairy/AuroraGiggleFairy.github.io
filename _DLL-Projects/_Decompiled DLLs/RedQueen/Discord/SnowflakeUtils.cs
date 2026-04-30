using System;

namespace Discord;

internal static class SnowflakeUtils
{
	public static DateTimeOffset FromSnowflake(ulong value)
	{
		return DateTimeOffset.FromUnixTimeMilliseconds((long)((value >> 22) + 1420070400000L));
	}

	public static ulong ToSnowflake(DateTimeOffset value)
	{
		return (ulong)(value.ToUnixTimeMilliseconds() - 1420070400000L << 22);
	}
}
