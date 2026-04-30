using System.Collections.Generic;

namespace Discord;

internal interface IApplicationCommandOption
{
	ApplicationCommandOptionType Type { get; }

	string Name { get; }

	string Description { get; }

	bool? IsDefault { get; }

	bool? IsRequired { get; }

	bool? IsAutocomplete { get; }

	double? MinValue { get; }

	double? MaxValue { get; }

	int? MinLength { get; }

	int? MaxLength { get; }

	IReadOnlyCollection<IApplicationCommandOptionChoice> Choices { get; }

	IReadOnlyCollection<IApplicationCommandOption> Options { get; }

	IReadOnlyCollection<ChannelType> ChannelTypes { get; }

	IReadOnlyDictionary<string, string> NameLocalizations { get; }

	IReadOnlyDictionary<string, string> DescriptionLocalizations { get; }

	string NameLocalized { get; }

	string DescriptionLocalized { get; }
}
