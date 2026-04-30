using System;

namespace Discord;

internal static class DateTimeUtils
{
	public static DateTimeOffset FromTicks(long ticks)
	{
		return new DateTimeOffset(ticks, TimeSpan.Zero);
	}

	public static DateTimeOffset? FromTicks(long? ticks)
	{
		if (!ticks.HasValue)
		{
			return null;
		}
		return new DateTimeOffset(ticks.Value, TimeSpan.Zero);
	}
}
