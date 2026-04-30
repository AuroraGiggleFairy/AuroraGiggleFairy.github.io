using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Discord.API;

internal class ApplicationCommandOption
{
	[JsonProperty("type")]
	public ApplicationCommandOptionType Type { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }

	[JsonProperty("default")]
	public Optional<bool> Default { get; set; }

	[JsonProperty("required")]
	public Optional<bool> Required { get; set; }

	[JsonProperty("choices")]
	public Optional<ApplicationCommandOptionChoice[]> Choices { get; set; }

	[JsonProperty("options")]
	public Optional<ApplicationCommandOption[]> Options { get; set; }

	[JsonProperty("autocomplete")]
	public Optional<bool> Autocomplete { get; set; }

	[JsonProperty("min_value")]
	public Optional<double> MinValue { get; set; }

	[JsonProperty("max_value")]
	public Optional<double> MaxValue { get; set; }

	[JsonProperty("channel_types")]
	public Optional<ChannelType[]> ChannelTypes { get; set; }

	[JsonProperty("name_localizations")]
	public Optional<Dictionary<string, string>> NameLocalizations { get; set; }

	[JsonProperty("description_localizations")]
	public Optional<Dictionary<string, string>> DescriptionLocalizations { get; set; }

	[JsonProperty("name_localized")]
	public Optional<string> NameLocalized { get; set; }

	[JsonProperty("description_localized")]
	public Optional<string> DescriptionLocalized { get; set; }

	[JsonProperty("min_length")]
	public Optional<int> MinLength { get; set; }

	[JsonProperty("max_length")]
	public Optional<int> MaxLength { get; set; }

	public ApplicationCommandOption()
	{
	}

	public ApplicationCommandOption(IApplicationCommandOption cmd)
	{
		Choices = cmd.Choices.Select((IApplicationCommandOptionChoice x) => new ApplicationCommandOptionChoice
		{
			Name = x.Name,
			Value = x.Value
		}).ToArray();
		Options = cmd.Options.Select((IApplicationCommandOption x) => new ApplicationCommandOption(x)).ToArray();
		ChannelTypes = cmd.ChannelTypes.ToArray();
		Required = ((Optional<bool>?)cmd.IsRequired) ?? Optional<bool>.Unspecified;
		Default = ((Optional<bool>?)cmd.IsDefault) ?? Optional<bool>.Unspecified;
		MinValue = ((Optional<double>?)cmd.MinValue) ?? Optional<double>.Unspecified;
		MaxValue = ((Optional<double>?)cmd.MaxValue) ?? Optional<double>.Unspecified;
		MinLength = ((Optional<int>?)cmd.MinLength) ?? Optional<int>.Unspecified;
		MaxLength = ((Optional<int>?)cmd.MaxLength) ?? Optional<int>.Unspecified;
		Autocomplete = ((Optional<bool>?)cmd.IsAutocomplete) ?? Optional<bool>.Unspecified;
		Name = cmd.Name;
		Type = cmd.Type;
		Description = cmd.Description;
		Dictionary<string, string> dictionary = cmd.NameLocalizations?.ToDictionary();
		NameLocalizations = ((dictionary != null) ? ((Optional<Dictionary<string, string>>)dictionary) : Optional<Dictionary<string, string>>.Unspecified);
		dictionary = cmd.DescriptionLocalizations?.ToDictionary();
		DescriptionLocalizations = ((dictionary != null) ? ((Optional<Dictionary<string, string>>)dictionary) : Optional<Dictionary<string, string>>.Unspecified);
		NameLocalized = cmd.NameLocalized;
		DescriptionLocalized = cmd.DescriptionLocalized;
	}

	public ApplicationCommandOption(ApplicationCommandOptionProperties option)
	{
		ApplicationCommandOptionChoice[] array = option.Choices?.Select((ApplicationCommandOptionChoiceProperties x) => new ApplicationCommandOptionChoice
		{
			Name = x.Name,
			Value = x.Value
		}).ToArray();
		Choices = ((array != null) ? ((Optional<ApplicationCommandOptionChoice[]>)array) : Optional<ApplicationCommandOptionChoice[]>.Unspecified);
		ApplicationCommandOption[] array2 = option.Options?.Select((ApplicationCommandOptionProperties x) => new ApplicationCommandOption(x)).ToArray();
		Options = ((array2 != null) ? ((Optional<ApplicationCommandOption[]>)array2) : Optional<ApplicationCommandOption[]>.Unspecified);
		Required = ((Optional<bool>?)option.IsRequired) ?? Optional<bool>.Unspecified;
		Default = ((Optional<bool>?)option.IsDefault) ?? Optional<bool>.Unspecified;
		MinValue = ((Optional<double>?)option.MinValue) ?? Optional<double>.Unspecified;
		MaxValue = ((Optional<double>?)option.MaxValue) ?? Optional<double>.Unspecified;
		MinLength = ((Optional<int>?)option.MinLength) ?? Optional<int>.Unspecified;
		MaxLength = ((Optional<int>?)option.MaxLength) ?? Optional<int>.Unspecified;
		ChannelType[] array3 = option.ChannelTypes?.ToArray();
		ChannelTypes = ((array3 != null) ? ((Optional<ChannelType[]>)array3) : Optional<ChannelType[]>.Unspecified);
		Name = option.Name;
		Type = option.Type;
		Description = option.Description;
		Autocomplete = option.IsAutocomplete;
		Dictionary<string, string> dictionary = option.NameLocalizations?.ToDictionary();
		NameLocalizations = ((dictionary != null) ? ((Optional<Dictionary<string, string>>)dictionary) : Optional<Dictionary<string, string>>.Unspecified);
		dictionary = option.DescriptionLocalizations?.ToDictionary();
		DescriptionLocalizations = ((dictionary != null) ? ((Optional<Dictionary<string, string>>)dictionary) : Optional<Dictionary<string, string>>.Unspecified);
	}
}
