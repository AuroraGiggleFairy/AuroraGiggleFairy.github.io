using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class RestApplicationCommandOption : IApplicationCommandOption
{
	public ApplicationCommandOptionType Type { get; private set; }

	public string Name { get; private set; }

	public string Description { get; private set; }

	public bool? IsDefault { get; private set; }

	public bool? IsRequired { get; private set; }

	public bool? IsAutocomplete { get; private set; }

	public double? MinValue { get; private set; }

	public double? MaxValue { get; private set; }

	public int? MinLength { get; private set; }

	public int? MaxLength { get; private set; }

	public IReadOnlyCollection<RestApplicationCommandChoice> Choices { get; private set; }

	public IReadOnlyCollection<RestApplicationCommandOption> Options { get; private set; }

	public IReadOnlyCollection<ChannelType> ChannelTypes { get; private set; }

	public IReadOnlyDictionary<string, string> NameLocalizations { get; private set; }

	public IReadOnlyDictionary<string, string> DescriptionLocalizations { get; private set; }

	public string NameLocalized { get; private set; }

	public string DescriptionLocalized { get; private set; }

	IReadOnlyCollection<IApplicationCommandOption> IApplicationCommandOption.Options => Options;

	IReadOnlyCollection<IApplicationCommandOptionChoice> IApplicationCommandOption.Choices => Choices;

	internal RestApplicationCommandOption()
	{
	}

	internal static RestApplicationCommandOption Create(ApplicationCommandOption model)
	{
		RestApplicationCommandOption restApplicationCommandOption = new RestApplicationCommandOption();
		restApplicationCommandOption.Update(model);
		return restApplicationCommandOption;
	}

	internal void Update(ApplicationCommandOption model)
	{
		Type = model.Type;
		Name = model.Name;
		Description = model.Description;
		if (model.Default.IsSpecified)
		{
			IsDefault = model.Default.Value;
		}
		if (model.Required.IsSpecified)
		{
			IsRequired = model.Required.Value;
		}
		if (model.MinValue.IsSpecified)
		{
			MinValue = model.MinValue.Value;
		}
		if (model.MaxValue.IsSpecified)
		{
			MaxValue = model.MaxValue.Value;
		}
		if (model.Autocomplete.IsSpecified)
		{
			IsAutocomplete = model.Autocomplete.Value;
		}
		MinLength = model.MinLength.ToNullable();
		MaxLength = model.MaxLength.ToNullable();
		Options = (model.Options.IsSpecified ? model.Options.Value.Select(Create).ToImmutableArray() : System.Collections.Immutable.ImmutableArray.Create<RestApplicationCommandOption>());
		Choices = (model.Choices.IsSpecified ? model.Choices.Value.Select((ApplicationCommandOptionChoice x) => new RestApplicationCommandChoice(x)).ToImmutableArray() : System.Collections.Immutable.ImmutableArray.Create<RestApplicationCommandChoice>());
		ChannelTypes = (model.ChannelTypes.IsSpecified ? model.ChannelTypes.Value.ToImmutableArray() : System.Collections.Immutable.ImmutableArray.Create<ChannelType>());
		NameLocalizations = model.NameLocalizations.GetValueOrDefault(null)?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty;
		DescriptionLocalizations = model.DescriptionLocalizations.GetValueOrDefault(null)?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty;
		NameLocalized = model.NameLocalized.GetValueOrDefault();
		DescriptionLocalized = model.DescriptionLocalized.GetValueOrDefault();
	}
}
