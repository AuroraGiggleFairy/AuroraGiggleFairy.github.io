using System;

namespace Discord;

internal class GameTimestamps
{
	public DateTimeOffset? Start { get; }

	public DateTimeOffset? End { get; }

	internal GameTimestamps(DateTimeOffset? start, DateTimeOffset? end)
	{
		Start = start;
		End = end;
	}
}
