using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RichGame : Game
{
	public string State { get; internal set; }

	public ulong ApplicationId { get; internal set; }

	public GameAsset SmallAsset { get; internal set; }

	public GameAsset LargeAsset { get; internal set; }

	public GameParty Party { get; internal set; }

	public GameSecrets Secrets { get; internal set; }

	public GameTimestamps Timestamps { get; internal set; }

	private string DebuggerDisplay => base.Name + " (Rich)";

	internal RichGame()
	{
	}

	public override string ToString()
	{
		return base.Name;
	}
}
