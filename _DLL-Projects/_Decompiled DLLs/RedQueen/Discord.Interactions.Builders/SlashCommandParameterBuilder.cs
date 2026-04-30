using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord.Interactions.Builders;

internal sealed class SlashCommandParameterBuilder : ParameterBuilder<SlashCommandParameterInfo, SlashCommandParameterBuilder>
{
	private readonly List<ParameterChoice> _choices = new List<ParameterChoice>();

	private readonly List<ChannelType> _channelTypes = new List<ChannelType>();

	private readonly List<SlashCommandParameterBuilder> _complexParameterFields = new List<SlashCommandParameterBuilder>();

	public string Description { get; set; }

	public double? MaxValue { get; set; }

	public double? MinValue { get; set; }

	public int? MinLength { get; set; }

	public int? MaxLength { get; set; }

	public IReadOnlyCollection<ParameterChoice> Choices => _choices;

	public IReadOnlyCollection<ChannelType> ChannelTypes => _channelTypes;

	public IReadOnlyCollection<SlashCommandParameterBuilder> ComplexParameterFields => _complexParameterFields;

	public bool Autocomplete { get; set; }

	public TypeConverter TypeConverter { get; private set; }

	public bool IsComplexParameter { get; internal set; }

	public ComplexParameterInitializer ComplexParameterInitializer { get; internal set; }

	public IAutocompleteHandler AutocompleteHandler { get; set; }

	protected override SlashCommandParameterBuilder Instance => this;

	internal SlashCommandParameterBuilder(ICommandBuilder command)
		: base(command)
	{
	}

	public SlashCommandParameterBuilder(ICommandBuilder command, string name, Type type, ComplexParameterInitializer complexParameterInitializer = null)
		: base(command, name, type)
	{
		ComplexParameterInitializer = complexParameterInitializer;
		if (complexParameterInitializer != null)
		{
			IsComplexParameter = true;
		}
	}

	public SlashCommandParameterBuilder WithDescription(string description)
	{
		Description = description;
		return this;
	}

	public SlashCommandParameterBuilder WithMinValue(double value)
	{
		MinValue = value;
		return this;
	}

	public SlashCommandParameterBuilder WithMaxValue(double value)
	{
		MaxValue = value;
		return this;
	}

	public SlashCommandParameterBuilder WithMinLength(int length)
	{
		MinLength = length;
		return this;
	}

	public SlashCommandParameterBuilder WithMaxLength(int length)
	{
		MaxLength = length;
		return this;
	}

	public SlashCommandParameterBuilder WithChoices(params ParameterChoice[] options)
	{
		_choices.AddRange(options);
		return this;
	}

	public SlashCommandParameterBuilder WithChannelTypes(params ChannelType[] channelTypes)
	{
		_channelTypes.AddRange(channelTypes);
		return this;
	}

	public SlashCommandParameterBuilder WithChannelTypes(IEnumerable<ChannelType> channelTypes)
	{
		_channelTypes.AddRange(channelTypes);
		return this;
	}

	public SlashCommandParameterBuilder WithAutocompleteHandler(Type autocompleteHandlerType, IServiceProvider services = null)
	{
		AutocompleteHandler = base.Command.Module.InteractionService.GetAutocompleteHandler(autocompleteHandlerType, services);
		return this;
	}

	public override SlashCommandParameterBuilder SetParameterType(Type type)
	{
		return SetParameterType(type);
	}

	public SlashCommandParameterBuilder SetParameterType(Type type, IServiceProvider services = null)
	{
		base.SetParameterType(type);
		if (!IsComplexParameter)
		{
			TypeConverter = base.Command.Module.InteractionService.GetTypeConverter(base.ParameterType, services);
		}
		return this;
	}

	public SlashCommandParameterBuilder AddComplexParameterField(Action<SlashCommandParameterBuilder> configure)
	{
		SlashCommandParameterBuilder slashCommandParameterBuilder = new SlashCommandParameterBuilder(base.Command);
		configure(slashCommandParameterBuilder);
		if (slashCommandParameterBuilder.IsComplexParameter)
		{
			throw new InvalidOperationException("You cannot create nested complex parameters.");
		}
		_complexParameterFields.Add(slashCommandParameterBuilder);
		return this;
	}

	public SlashCommandParameterBuilder AddComplexParameterFields(params SlashCommandParameterBuilder[] fields)
	{
		if (fields.Any((SlashCommandParameterBuilder x) => x.IsComplexParameter))
		{
			throw new InvalidOperationException("You cannot create nested complex parameters.");
		}
		_complexParameterFields.AddRange(fields);
		return this;
	}

	internal override SlashCommandParameterInfo Build(ICommandInfo command)
	{
		return new SlashCommandParameterInfo(this, command as SlashCommandInfo);
	}
}
