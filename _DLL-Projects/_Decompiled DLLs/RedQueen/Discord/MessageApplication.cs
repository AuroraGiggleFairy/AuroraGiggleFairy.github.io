using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class MessageApplication
{
	public ulong Id { get; internal set; }

	public string CoverImage { get; internal set; }

	public string Description { get; internal set; }

	public string Icon { get; internal set; }

	public string IconUrl => $"https://cdn.discordapp.com/app-icons/{Id}/{Icon}";

	public string Name { get; internal set; }

	private string DebuggerDisplay => $"{Name} ({Id}): {Description}";

	public override string ToString()
	{
		return DebuggerDisplay;
	}
}
