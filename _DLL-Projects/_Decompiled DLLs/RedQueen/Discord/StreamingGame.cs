using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class StreamingGame : Game
{
	public string Url { get; internal set; }

	private string DebuggerDisplay => base.Name + " (" + Url + ")";

	public StreamingGame(string name, string url)
	{
		base.Name = name;
		Url = url;
		base.Type = ActivityType.Streaming;
	}

	public override string ToString()
	{
		return base.Name;
	}
}
