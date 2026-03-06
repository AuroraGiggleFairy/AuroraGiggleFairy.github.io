using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class RestAutocompleteInteractionData : IAutocompleteInteractionData, IDiscordInteractionData
{
	public string CommandName { get; }

	public ulong CommandId { get; }

	public ApplicationCommandType Type { get; }

	public ulong Version { get; }

	public AutocompleteOption Current { get; }

	public IReadOnlyCollection<AutocompleteOption> Options { get; }

	internal RestAutocompleteInteractionData(AutocompleteInteractionData model)
	{
		IEnumerable<AutocompleteOption> enumerable = model.Options.SelectMany(GetOptions);
		Current = enumerable.FirstOrDefault((AutocompleteOption x) => x.Focused);
		Options = enumerable.ToImmutableArray();
		if (Options.Count == 1 && Current == null)
		{
			Current = Options.FirstOrDefault();
		}
		CommandName = model.Name;
		CommandId = model.Id;
		Type = model.Type;
		Version = model.Version;
	}

	private List<AutocompleteOption> GetOptions(AutocompleteInteractionDataOption model)
	{
		List<AutocompleteOption> list = new List<AutocompleteOption>();
		list.Add(new AutocompleteOption(model.Type, model.Name, model.Value.GetValueOrDefault(null), model.Focused.GetValueOrDefault(defaultValue: false)));
		if (model.Options.IsSpecified)
		{
			list.AddRange(model.Options.Value.SelectMany(GetOptions));
		}
		return list;
	}
}
