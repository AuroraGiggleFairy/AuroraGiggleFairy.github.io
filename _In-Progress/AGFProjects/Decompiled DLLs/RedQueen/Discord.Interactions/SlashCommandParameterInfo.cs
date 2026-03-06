using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

internal class SlashCommandParameterInfo : CommandParameterInfo
{
	internal readonly ComplexParameterInitializer _complexParameterInitializer;

	public new SlashCommandInfo Command => base.Command as SlashCommandInfo;

	public string Description { get; }

	public double? MinValue { get; }

	public double? MaxValue { get; }

	public int? MinLength { get; }

	public int? MaxLength { get; }

	public TypeConverter TypeConverter { get; }

	public IAutocompleteHandler AutocompleteHandler { get; }

	public bool IsAutocomplete { get; }

	public bool IsComplexParameter { get; }

	public ApplicationCommandOptionType? DiscordOptionType => TypeConverter?.GetDiscordType();

	public IReadOnlyCollection<ParameterChoice> Choices { get; }

	public IReadOnlyCollection<ChannelType> ChannelTypes { get; }

	public IReadOnlyCollection<SlashCommandParameterInfo> ComplexParameterFields { get; }

	internal SlashCommandParameterInfo(SlashCommandParameterBuilder builder, SlashCommandInfo command)
		: base(builder, command)
	{
		TypeConverter = builder.TypeConverter;
		AutocompleteHandler = builder.AutocompleteHandler;
		Description = builder.Description;
		MaxValue = builder.MaxValue;
		MinValue = builder.MinValue;
		MinLength = builder.MinLength;
		MaxLength = builder.MaxLength;
		IsComplexParameter = builder.IsComplexParameter;
		IsAutocomplete = builder.Autocomplete;
		Choices = builder.Choices.ToImmutableArray();
		ChannelTypes = builder.ChannelTypes.ToImmutableArray();
		ComplexParameterFields = builder.ComplexParameterFields?.Select((SlashCommandParameterBuilder x) => x.Build(command)).ToImmutableArray();
		_complexParameterInitializer = builder.ComplexParameterInitializer;
	}
}
