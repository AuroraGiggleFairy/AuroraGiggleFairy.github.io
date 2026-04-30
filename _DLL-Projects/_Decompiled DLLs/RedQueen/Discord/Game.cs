using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class Game : IActivity
{
	public string Name { get; internal set; }

	public ActivityType Type { get; internal set; }

	public ActivityProperties Flags { get; internal set; }

	public string Details { get; internal set; }

	private string DebuggerDisplay => Name;

	internal Game()
	{
	}

	public Game(string name, ActivityType type = ActivityType.Playing, ActivityProperties flags = ActivityProperties.None, string details = null)
	{
		Name = name;
		Type = type;
		Flags = flags;
		Details = details;
	}

	public override string ToString()
	{
		return Name;
	}
}
