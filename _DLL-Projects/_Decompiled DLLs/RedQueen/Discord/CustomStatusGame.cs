using System;
using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class CustomStatusGame : Game
{
	public IEmote Emote { get; internal set; }

	public DateTimeOffset CreatedAt { get; internal set; }

	public string State { get; internal set; }

	private string DebuggerDisplay => base.Name ?? "";

	internal CustomStatusGame()
	{
	}

	public override string ToString()
	{
		return $"{Emote} {State}";
	}
}
