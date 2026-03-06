using System.Collections.Generic;

namespace Discord;

internal interface IApplicationCommandInteractionDataOption
{
	string Name { get; }

	object Value { get; }

	ApplicationCommandOptionType Type { get; }

	IReadOnlyCollection<IApplicationCommandInteractionDataOption> Options { get; }
}
