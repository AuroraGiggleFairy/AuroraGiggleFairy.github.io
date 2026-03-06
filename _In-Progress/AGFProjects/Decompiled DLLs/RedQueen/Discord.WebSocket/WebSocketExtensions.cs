using System.Collections.Generic;
using System.Linq;

namespace Discord.WebSocket;

internal static class WebSocketExtensions
{
	public static IList<string> GetCommandKeywords(this IApplicationCommandInteractionData data)
	{
		List<string> list = new List<string> { data.Name };
		IApplicationCommandInteractionDataOption applicationCommandInteractionDataOption = data.Options?.ElementAtOrDefault(0);
		while ((applicationCommandInteractionDataOption != null && applicationCommandInteractionDataOption.Type == ApplicationCommandOptionType.SubCommandGroup) || (applicationCommandInteractionDataOption != null && applicationCommandInteractionDataOption.Type == ApplicationCommandOptionType.SubCommand))
		{
			list.Add(applicationCommandInteractionDataOption.Name);
			applicationCommandInteractionDataOption = applicationCommandInteractionDataOption.Options?.ElementAtOrDefault(0);
		}
		return list;
	}

	public static IList<string> GetCommandKeywords(this IAutocompleteInteractionData data)
	{
		List<string> list = new List<string> { data.CommandName };
		AutocompleteOption autocompleteOption = data.Options?.FirstOrDefault((AutocompleteOption x) => x.Type == ApplicationCommandOptionType.SubCommandGroup);
		if (autocompleteOption != null)
		{
			list.Add(autocompleteOption.Name);
		}
		AutocompleteOption autocompleteOption2 = data.Options?.FirstOrDefault((AutocompleteOption x) => x.Type == ApplicationCommandOptionType.SubCommand);
		if (autocompleteOption2 != null)
		{
			list.Add(autocompleteOption2.Name);
		}
		return list;
	}
}
