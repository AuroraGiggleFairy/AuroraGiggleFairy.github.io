using System.Collections.Generic;
using System.Collections.Immutable;
using Discord.API;

namespace Discord.WebSocket;

internal class SocketApplicationCommandChoice : IApplicationCommandOptionChoice
{
	public string Name { get; private set; }

	public object Value { get; private set; }

	public IReadOnlyDictionary<string, string> NameLocalizations { get; private set; }

	public string NameLocalized { get; private set; }

	internal SocketApplicationCommandChoice()
	{
	}

	internal static SocketApplicationCommandChoice Create(ApplicationCommandOptionChoice model)
	{
		SocketApplicationCommandChoice socketApplicationCommandChoice = new SocketApplicationCommandChoice();
		socketApplicationCommandChoice.Update(model);
		return socketApplicationCommandChoice;
	}

	internal void Update(ApplicationCommandOptionChoice model)
	{
		Name = model.Name;
		Value = model.Value;
		NameLocalizations = model.NameLocalizations.GetValueOrDefault(null)?.ToImmutableDictionary();
		NameLocalized = model.NameLocalized.GetValueOrDefault(null);
	}
}
