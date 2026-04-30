using System;

namespace Discord;

internal class TimestampTag
{
	public TimestampTagStyles Style { get; set; } = TimestampTagStyles.ShortDateTime;

	public DateTimeOffset Time { get; set; }

	public override string ToString()
	{
		return $"<t:{Time.ToUnixTimeSeconds()}:{(char)Style}>";
	}

	public static TimestampTag FromDateTime(DateTime time, TimestampTagStyles style = TimestampTagStyles.ShortDateTime)
	{
		return new TimestampTag
		{
			Style = style,
			Time = time
		};
	}

	public static TimestampTag FromDateTimeOffset(DateTimeOffset time, TimestampTagStyles style = TimestampTagStyles.ShortDateTime)
	{
		return new TimestampTag
		{
			Style = style,
			Time = time
		};
	}
}
