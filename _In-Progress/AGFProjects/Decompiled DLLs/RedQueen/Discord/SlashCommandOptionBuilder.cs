using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Discord;

internal class SlashCommandOptionBuilder
{
	public const int ChoiceNameMaxLength = 100;

	public const int MaxChoiceCount = 25;

	private string _name;

	private string _description;

	private Dictionary<string, string> _nameLocalizations;

	private Dictionary<string, string> _descriptionLocalizations;

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (value != null)
			{
				EnsureValidCommandOptionName(value);
			}
			_name = value;
		}
	}

	public string Description
	{
		get
		{
			return _description;
		}
		set
		{
			if (value != null)
			{
				EnsureValidCommandOptionDescription(value);
			}
			_description = value;
		}
	}

	public ApplicationCommandOptionType Type { get; set; }

	public bool? IsDefault { get; set; }

	public bool? IsRequired { get; set; }

	public bool IsAutocomplete { get; set; }

	public double? MinValue { get; set; }

	public double? MaxValue { get; set; }

	public int? MinLength { get; set; }

	public int? MaxLength { get; set; }

	public List<ApplicationCommandOptionChoiceProperties> Choices { get; set; }

	public List<SlashCommandOptionBuilder> Options { get; set; }

	public List<ChannelType> ChannelTypes { get; set; }

	public IReadOnlyDictionary<string, string> NameLocalizations => _nameLocalizations;

	public IReadOnlyDictionary<string, string> DescriptionLocalizations => _descriptionLocalizations;

	public ApplicationCommandOptionProperties Build()
	{
		bool num = Type == ApplicationCommandOptionType.SubCommandGroup;
		bool flag = Type == ApplicationCommandOptionType.Integer;
		bool flag2 = Type == ApplicationCommandOptionType.String;
		if (num && (Options == null || !Options.Any()))
		{
			throw new InvalidOperationException("SubCommands/SubCommandGroups must have at least one option");
		}
		if (!num && Options != null && Options.Any() && Type != ApplicationCommandOptionType.SubCommand)
		{
			throw new InvalidOperationException($"Cannot have options on {Type} type");
		}
		if (flag && MinValue.HasValue && MinValue % 1.0 != 0.0)
		{
			throw new InvalidOperationException("MinValue cannot have decimals on Integer command options.");
		}
		if (flag && MaxValue.HasValue && MaxValue % 1.0 != 0.0)
		{
			throw new InvalidOperationException("MaxValue cannot have decimals on Integer command options.");
		}
		if (flag2 && MinLength.HasValue && MinLength < 0)
		{
			throw new InvalidOperationException("MinLength cannot be smaller than 0.");
		}
		if (flag2 && MaxLength.HasValue && MaxLength < 1)
		{
			throw new InvalidOperationException("MaxLength cannot be smaller than 1.");
		}
		ApplicationCommandOptionProperties obj = new ApplicationCommandOptionProperties
		{
			Name = Name,
			Description = Description,
			IsDefault = IsDefault,
			IsRequired = IsRequired,
			Type = Type
		};
		List<SlashCommandOptionBuilder> options = Options;
		obj.Options = ((options != null && options.Count > 0) ? (from x in Options
			orderby x.IsRequired == true descending
			select x.Build()).ToList() : new List<ApplicationCommandOptionProperties>());
		obj.Choices = Choices;
		obj.IsAutocomplete = IsAutocomplete;
		obj.ChannelTypes = ChannelTypes;
		obj.MinValue = MinValue;
		obj.MaxValue = MaxValue;
		obj.NameLocalizations = _nameLocalizations;
		obj.DescriptionLocalizations = _descriptionLocalizations;
		obj.MinLength = MinLength;
		obj.MaxLength = MaxLength;
		return obj;
	}

	public SlashCommandOptionBuilder AddOption(string name, ApplicationCommandOptionType type, string description, bool? isRequired = null, bool isDefault = false, bool isAutocomplete = false, double? minValue = null, double? maxValue = null, List<SlashCommandOptionBuilder> options = null, List<ChannelType> channelTypes = null, IDictionary<string, string> nameLocalizations = null, IDictionary<string, string> descriptionLocalizations = null, int? minLength = null, int? maxLength = null, params ApplicationCommandOptionChoiceProperties[] choices)
	{
		Preconditions.Options(name, description);
		if (!Regex.IsMatch(name, "^[-_\\p{L}\\p{N}\\p{IsDevanagari}\\p{IsThai}]{1,32}$"))
		{
			throw new ArgumentException("Name must match the regex ^[-_\\p{L}\\p{N}\\p{IsDevanagari}\\p{IsThai}]{1,32}$", "name");
		}
		if (isDefault)
		{
			List<SlashCommandOptionBuilder> options2 = Options;
			if (options2 != null && options2.Any((SlashCommandOptionBuilder x) => x.IsDefault == true))
			{
				throw new ArgumentException("There can only be one command option with default set to true!", "isDefault");
			}
		}
		SlashCommandOptionBuilder slashCommandOptionBuilder = new SlashCommandOptionBuilder
		{
			Name = name,
			Description = description,
			IsRequired = isRequired,
			IsDefault = isDefault,
			IsAutocomplete = isAutocomplete,
			MinValue = minValue,
			MaxValue = maxValue,
			MinLength = minLength,
			MaxLength = maxLength,
			Options = options,
			Type = type,
			Choices = (choices ?? Array.Empty<ApplicationCommandOptionChoiceProperties>()).ToList(),
			ChannelTypes = channelTypes
		};
		if (nameLocalizations != null)
		{
			slashCommandOptionBuilder.WithNameLocalizations(nameLocalizations);
		}
		if (descriptionLocalizations != null)
		{
			slashCommandOptionBuilder.WithDescriptionLocalizations(descriptionLocalizations);
		}
		return AddOption(slashCommandOptionBuilder);
	}

	public SlashCommandOptionBuilder AddOption(SlashCommandOptionBuilder option)
	{
		if (Options == null)
		{
			List<SlashCommandOptionBuilder> list = (Options = new List<SlashCommandOptionBuilder>());
		}
		if (Options.Count >= 25)
		{
			throw new InvalidOperationException($"There can only be {25} options per sub command group!");
		}
		Preconditions.NotNull(option, "option");
		Preconditions.Options(option.Name, option.Description);
		Options.Add(option);
		return this;
	}

	public SlashCommandOptionBuilder AddOptions(params SlashCommandOptionBuilder[] options)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options", "Options cannot be null!");
		}
		if ((Options?.Count ?? 0) + options.Length > 25)
		{
			throw new ArgumentOutOfRangeException("options", $"There can only be {25} options per sub command group!");
		}
		foreach (SlashCommandOptionBuilder slashCommandOptionBuilder in options)
		{
			Preconditions.Options(slashCommandOptionBuilder.Name, slashCommandOptionBuilder.Description);
		}
		Options.AddRange(options);
		return this;
	}

	public SlashCommandOptionBuilder AddChoice(string name, int value, IDictionary<string, string> nameLocalizations = null)
	{
		return AddChoiceInternal(name, value, nameLocalizations);
	}

	public SlashCommandOptionBuilder AddChoice(string name, string value, IDictionary<string, string> nameLocalizations = null)
	{
		return AddChoiceInternal(name, value, nameLocalizations);
	}

	public SlashCommandOptionBuilder AddChoice(string name, double value, IDictionary<string, string> nameLocalizations = null)
	{
		return AddChoiceInternal(name, value, nameLocalizations);
	}

	public SlashCommandOptionBuilder AddChoice(string name, float value, IDictionary<string, string> nameLocalizations = null)
	{
		return AddChoiceInternal(name, value, nameLocalizations);
	}

	public SlashCommandOptionBuilder AddChoice(string name, long value, IDictionary<string, string> nameLocalizations = null)
	{
		return AddChoiceInternal(name, value, nameLocalizations);
	}

	private SlashCommandOptionBuilder AddChoiceInternal(string name, object value, IDictionary<string, string> nameLocalizations = null)
	{
		if (Choices == null)
		{
			List<ApplicationCommandOptionChoiceProperties> list = (Choices = new List<ApplicationCommandOptionChoiceProperties>());
		}
		if (Choices.Count >= 25)
		{
			throw new InvalidOperationException($"Cannot add more than {25} choices!");
		}
		Preconditions.NotNull(name, "name");
		Preconditions.NotNull(value, "value");
		Preconditions.AtLeast(name.Length, 1, "name");
		Preconditions.AtMost(name.Length, 100, "name");
		if (value is string text)
		{
			Preconditions.AtLeast(text.Length, 1, "value");
			Preconditions.AtMost(text.Length, 100, "value");
		}
		Choices.Add(new ApplicationCommandOptionChoiceProperties
		{
			Name = name,
			Value = value,
			NameLocalizations = nameLocalizations
		});
		return this;
	}

	public SlashCommandOptionBuilder AddChannelType(ChannelType channelType)
	{
		if (ChannelTypes == null)
		{
			List<ChannelType> list = (ChannelTypes = new List<ChannelType>());
		}
		ChannelTypes.Add(channelType);
		return this;
	}

	public SlashCommandOptionBuilder WithName(string name)
	{
		Name = name;
		return this;
	}

	public SlashCommandOptionBuilder WithDescription(string description)
	{
		Description = description;
		return this;
	}

	public SlashCommandOptionBuilder WithRequired(bool value)
	{
		IsRequired = value;
		return this;
	}

	public SlashCommandOptionBuilder WithDefault(bool value)
	{
		IsDefault = value;
		return this;
	}

	public SlashCommandOptionBuilder WithAutocomplete(bool value)
	{
		IsAutocomplete = value;
		return this;
	}

	public SlashCommandOptionBuilder WithMinValue(double value)
	{
		MinValue = value;
		return this;
	}

	public SlashCommandOptionBuilder WithMaxValue(double value)
	{
		MaxValue = value;
		return this;
	}

	public SlashCommandOptionBuilder WithMinLength(int length)
	{
		MinLength = length;
		return this;
	}

	public SlashCommandOptionBuilder WithMaxLength(int length)
	{
		MaxLength = length;
		return this;
	}

	public SlashCommandOptionBuilder WithType(ApplicationCommandOptionType type)
	{
		Type = type;
		return this;
	}

	public SlashCommandOptionBuilder WithNameLocalizations(IDictionary<string, string> nameLocalizations)
	{
		if (nameLocalizations == null)
		{
			throw new ArgumentNullException("nameLocalizations");
		}
		foreach (var (text3, name) in nameLocalizations)
		{
			if (!Regex.IsMatch(text3, "^\\w{2}(?:-\\w{2})?$"))
			{
				throw new ArgumentException("Invalid locale: " + text3, "locale");
			}
			EnsureValidCommandOptionName(name);
		}
		_nameLocalizations = new Dictionary<string, string>(nameLocalizations);
		return this;
	}

	public SlashCommandOptionBuilder WithDescriptionLocalizations(IDictionary<string, string> descriptionLocalizations)
	{
		if (descriptionLocalizations == null)
		{
			throw new ArgumentNullException("descriptionLocalizations");
		}
		foreach (var (text3, description) in descriptionLocalizations)
		{
			if (!Regex.IsMatch(text3, "^\\w{2}(?:-\\w{2})?$"))
			{
				throw new ArgumentException("Invalid locale: " + text3, "locale");
			}
			EnsureValidCommandOptionDescription(description);
		}
		_descriptionLocalizations = new Dictionary<string, string>(descriptionLocalizations);
		return this;
	}

	public SlashCommandOptionBuilder AddNameLocalization(string locale, string name)
	{
		if (!Regex.IsMatch(locale, "^\\w{2}(?:-\\w{2})?$"))
		{
			throw new ArgumentException("Invalid locale: " + locale, "locale");
		}
		EnsureValidCommandOptionName(name);
		if (_nameLocalizations == null)
		{
			_nameLocalizations = new Dictionary<string, string>();
		}
		_nameLocalizations.Add(locale, name);
		return this;
	}

	public SlashCommandOptionBuilder AddDescriptionLocalization(string locale, string description)
	{
		if (!Regex.IsMatch(locale, "^\\w{2}(?:-\\w{2})?$"))
		{
			throw new ArgumentException("Invalid locale: " + locale, "locale");
		}
		EnsureValidCommandOptionDescription(description);
		if (_descriptionLocalizations == null)
		{
			_descriptionLocalizations = new Dictionary<string, string>();
		}
		_descriptionLocalizations.Add(locale, description);
		return this;
	}

	private static void EnsureValidCommandOptionName(string name)
	{
		Preconditions.AtLeast(name.Length, 1, "name");
		Preconditions.AtMost(name.Length, 32, "name");
		if (!Regex.IsMatch(name, "^[-_\\p{L}\\p{N}\\p{IsDevanagari}\\p{IsThai}]{1,32}$"))
		{
			throw new ArgumentException("Name must match the regex ^[-_\\p{L}\\p{N}\\p{IsDevanagari}\\p{IsThai}]{1,32}$", "name");
		}
	}

	private static void EnsureValidCommandOptionDescription(string description)
	{
		Preconditions.AtLeast(description.Length, 1, "description");
		Preconditions.AtMost(description.Length, 100, "description");
	}
}
