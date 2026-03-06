using System.Collections.Generic;

namespace Discord;

internal interface IApplicationCommandOptionChoice
{
	string Name { get; }

	object Value { get; }

	IReadOnlyDictionary<string, string> NameLocalizations { get; }

	string NameLocalized { get; }
}
