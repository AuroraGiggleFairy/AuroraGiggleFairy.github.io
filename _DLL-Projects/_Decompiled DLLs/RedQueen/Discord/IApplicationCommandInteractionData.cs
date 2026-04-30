using System.Collections.Generic;

namespace Discord;

internal interface IApplicationCommandInteractionData : IDiscordInteractionData
{
	ulong Id { get; }

	string Name { get; }

	IReadOnlyCollection<IApplicationCommandInteractionDataOption> Options { get; }
}
