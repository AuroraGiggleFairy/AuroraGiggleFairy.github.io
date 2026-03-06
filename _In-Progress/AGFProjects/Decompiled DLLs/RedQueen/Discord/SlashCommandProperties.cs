using System.Collections.Generic;

namespace Discord;

internal class SlashCommandProperties : ApplicationCommandProperties
{
	internal override ApplicationCommandType Type => ApplicationCommandType.Slash;

	public Optional<string> Description { get; set; }

	public Optional<List<ApplicationCommandOptionProperties>> Options { get; set; }

	internal SlashCommandProperties()
	{
	}
}
