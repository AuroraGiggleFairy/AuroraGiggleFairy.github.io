using System.Collections.Generic;
using System.Collections.Immutable;
using Discord.API;

namespace Discord.Rest;

internal class RestApplicationCommandChoice : IApplicationCommandOptionChoice
{
	public string Name { get; }

	public object Value { get; }

	public IReadOnlyDictionary<string, string> NameLocalizations { get; }

	public string NameLocalized { get; }

	internal RestApplicationCommandChoice(ApplicationCommandOptionChoice model)
	{
		Name = model.Name;
		Value = model.Value;
		NameLocalizations = model.NameLocalizations.GetValueOrDefault(null)?.ToImmutableDictionary();
		NameLocalized = model.NameLocalized.GetValueOrDefault(null);
	}
}
