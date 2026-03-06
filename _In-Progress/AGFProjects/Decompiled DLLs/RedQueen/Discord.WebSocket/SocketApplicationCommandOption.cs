using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;

namespace Discord.WebSocket;

internal class SocketApplicationCommandOption : IApplicationCommandOption
{
	public string Name { get; private set; }

	public ApplicationCommandOptionType Type { get; private set; }

	public string Description { get; private set; }

	public bool? IsDefault { get; private set; }

	public bool? IsRequired { get; private set; }

	public bool? IsAutocomplete { get; private set; }

	public double? MinValue { get; private set; }

	public double? MaxValue { get; private set; }

	public int? MinLength { get; private set; }

	public int? MaxLength { get; private set; }

	public IReadOnlyCollection<SocketApplicationCommandChoice> Choices { get; private set; }

	public IReadOnlyCollection<SocketApplicationCommandOption> Options { get; private set; }

	public IReadOnlyCollection<ChannelType> ChannelTypes { get; private set; }

	public IReadOnlyDictionary<string, string> NameLocalizations { get; private set; }

	public IReadOnlyDictionary<string, string> DescriptionLocalizations { get; private set; }

	public string NameLocalized { get; private set; }

	public string DescriptionLocalized { get; private set; }

	IReadOnlyCollection<IApplicationCommandOptionChoice> IApplicationCommandOption.Choices => Choices;

	IReadOnlyCollection<IApplicationCommandOption> IApplicationCommandOption.Options => Options;

	internal SocketApplicationCommandOption()
	{
	}

	internal static SocketApplicationCommandOption Create(ApplicationCommandOption model)
	{
		SocketApplicationCommandOption socketApplicationCommandOption = new SocketApplicationCommandOption();
		socketApplicationCommandOption.Update(model);
		return socketApplicationCommandOption;
	}

	internal void Update(ApplicationCommandOption model)
	{
		Name = model.Name;
		Type = model.Type;
		Description = model.Description;
		IsDefault = model.Default.ToNullable();
		IsRequired = model.Required.ToNullable();
		MinValue = model.MinValue.ToNullable();
		MaxValue = model.MaxValue.ToNullable();
		IsAutocomplete = model.Autocomplete.ToNullable();
		MinLength = model.MinLength.ToNullable();
		MaxLength = model.MaxLength.ToNullable();
		Choices = (model.Choices.IsSpecified ? model.Choices.Value.Select(SocketApplicationCommandChoice.Create).ToImmutableArray() : System.Collections.Immutable.ImmutableArray.Create<SocketApplicationCommandChoice>());
		Options = (model.Options.IsSpecified ? model.Options.Value.Select(Create).ToImmutableArray() : System.Collections.Immutable.ImmutableArray.Create<SocketApplicationCommandOption>());
		ChannelTypes = (model.ChannelTypes.IsSpecified ? model.ChannelTypes.Value.ToImmutableArray() : System.Collections.Immutable.ImmutableArray.Create<ChannelType>());
		NameLocalizations = model.NameLocalizations.GetValueOrDefault(null)?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty;
		DescriptionLocalizations = model.DescriptionLocalizations.GetValueOrDefault(null)?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty;
		NameLocalized = model.NameLocalized.GetValueOrDefault();
		DescriptionLocalized = model.DescriptionLocalized.GetValueOrDefault();
	}
}
