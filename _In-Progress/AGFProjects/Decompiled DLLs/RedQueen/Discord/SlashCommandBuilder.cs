using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Discord;

internal class SlashCommandBuilder
{
	public const int MaxNameLength = 32;

	public const int MaxDescriptionLength = 100;

	public const int MaxOptionsCount = 25;

	private string _name;

	private string _description;

	private Dictionary<string, string> _nameLocalizations;

	private Dictionary<string, string> _descriptionLocalizations;

	private List<SlashCommandOptionBuilder> _options;

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			EnsureValidCommandName(value);
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
			EnsureValidCommandDescription(value);
			_description = value;
		}
	}

	public List<SlashCommandOptionBuilder> Options
	{
		get
		{
			return _options;
		}
		set
		{
			Preconditions.AtMost(value?.Count ?? 0, 25, "value");
			_options = value;
		}
	}

	public IReadOnlyDictionary<string, string> NameLocalizations => _nameLocalizations;

	public IReadOnlyDictionary<string, string> DescriptionLocalizations => _descriptionLocalizations;

	public bool IsDefaultPermission { get; set; } = true;

	public bool IsDMEnabled { get; set; } = true;

	public GuildPermission? DefaultMemberPermissions { get; set; }

	public SlashCommandProperties Build()
	{
		SlashCommandProperties slashCommandProperties = new SlashCommandProperties
		{
			Name = Name,
			Description = Description,
			IsDefaultPermission = IsDefaultPermission,
			NameLocalizations = _nameLocalizations,
			DescriptionLocalizations = _descriptionLocalizations,
			IsDMEnabled = IsDMEnabled,
			DefaultMemberPermissions = (((Optional<GuildPermission>?)DefaultMemberPermissions) ?? Optional<GuildPermission>.Unspecified)
		};
		if (Options != null && Options.Any())
		{
			List<ApplicationCommandOptionProperties> options = new List<ApplicationCommandOptionProperties>();
			Options.OrderByDescending((SlashCommandOptionBuilder x) => x.IsRequired == true).ToList().ForEach(delegate(SlashCommandOptionBuilder x)
			{
				options.Add(x.Build());
			});
			slashCommandProperties.Options = options;
		}
		return slashCommandProperties;
	}

	public SlashCommandBuilder WithName(string name)
	{
		Name = name;
		return this;
	}

	public SlashCommandBuilder WithDescription(string description)
	{
		Description = description;
		return this;
	}

	public SlashCommandBuilder WithDefaultPermission(bool value)
	{
		IsDefaultPermission = value;
		return this;
	}

	public SlashCommandBuilder WithDMPermission(bool permission)
	{
		IsDMEnabled = permission;
		return this;
	}

	public SlashCommandBuilder WithDefaultMemberPermissions(GuildPermission? permissions)
	{
		DefaultMemberPermissions = permissions;
		return this;
	}

	public SlashCommandBuilder AddOption(string name, ApplicationCommandOptionType type, string description, bool? isRequired = null, bool? isDefault = null, bool isAutocomplete = false, double? minValue = null, double? maxValue = null, List<SlashCommandOptionBuilder> options = null, List<ChannelType> channelTypes = null, IDictionary<string, string> nameLocalizations = null, IDictionary<string, string> descriptionLocalizations = null, int? minLength = null, int? maxLength = null, params ApplicationCommandOptionChoiceProperties[] choices)
	{
		Preconditions.Options(name, description);
		if (!Regex.IsMatch(name, "^[-_\\p{L}\\p{N}\\p{IsDevanagari}\\p{IsThai}]{1,32}$"))
		{
			throw new ArgumentException("Name must match the regex ^[-_\\p{L}\\p{N}\\p{IsDevanagari}\\p{IsThai}]{1,32}$", "name");
		}
		if (isDefault == true)
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
			Options = options,
			Type = type,
			IsAutocomplete = isAutocomplete,
			Choices = (choices ?? Array.Empty<ApplicationCommandOptionChoiceProperties>()).ToList(),
			ChannelTypes = channelTypes,
			MinValue = minValue,
			MaxValue = maxValue,
			MinLength = minLength,
			MaxLength = maxLength
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

	public SlashCommandBuilder AddOption(SlashCommandOptionBuilder option)
	{
		if (Options == null)
		{
			List<SlashCommandOptionBuilder> list = (Options = new List<SlashCommandOptionBuilder>());
		}
		if (Options.Count >= 25)
		{
			throw new InvalidOperationException($"Cannot have more than {25} options!");
		}
		Preconditions.NotNull(option, "option");
		Preconditions.Options(option.Name, option.Description);
		Options.Add(option);
		return this;
	}

	public SlashCommandBuilder AddOptions(params SlashCommandOptionBuilder[] options)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options", "Options cannot be null!");
		}
		if (Options == null)
		{
			List<SlashCommandOptionBuilder> list = (Options = new List<SlashCommandOptionBuilder>());
		}
		if (Options.Count + options.Length > 25)
		{
			throw new ArgumentOutOfRangeException("options", $"Cannot have more than {25} options!");
		}
		foreach (SlashCommandOptionBuilder slashCommandOptionBuilder in options)
		{
			Preconditions.Options(slashCommandOptionBuilder.Name, slashCommandOptionBuilder.Description);
		}
		Options.AddRange(options);
		return this;
	}

	public SlashCommandBuilder WithNameLocalizations(IDictionary<string, string> nameLocalizations)
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
			EnsureValidCommandName(name);
		}
		_nameLocalizations = new Dictionary<string, string>(nameLocalizations);
		return this;
	}

	public SlashCommandBuilder WithDescriptionLocalizations(IDictionary<string, string> descriptionLocalizations)
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
			EnsureValidCommandDescription(description);
		}
		_descriptionLocalizations = new Dictionary<string, string>(descriptionLocalizations);
		return this;
	}

	public SlashCommandBuilder AddNameLocalization(string locale, string name)
	{
		if (!Regex.IsMatch(locale, "^\\w{2}(?:-\\w{2})?$"))
		{
			throw new ArgumentException("Invalid locale: " + locale, "locale");
		}
		EnsureValidCommandName(name);
		if (_nameLocalizations == null)
		{
			_nameLocalizations = new Dictionary<string, string>();
		}
		_nameLocalizations.Add(locale, name);
		return this;
	}

	public SlashCommandBuilder AddDescriptionLocalization(string locale, string description)
	{
		if (!Regex.IsMatch(locale, "^\\w{2}(?:-\\w{2})?$"))
		{
			throw new ArgumentException("Invalid locale: " + locale, "locale");
		}
		EnsureValidCommandDescription(description);
		if (_descriptionLocalizations == null)
		{
			_descriptionLocalizations = new Dictionary<string, string>();
		}
		_descriptionLocalizations.Add(locale, description);
		return this;
	}

	internal static void EnsureValidCommandName(string name)
	{
		Preconditions.NotNullOrEmpty(name, "name");
		Preconditions.AtLeast(name.Length, 1, "name");
		Preconditions.AtMost(name.Length, 32, "name");
		if (!Regex.IsMatch(name, "^[-_\\p{L}\\p{N}\\p{IsDevanagari}\\p{IsThai}]{1,32}$"))
		{
			throw new ArgumentException("Name must match the regex ^[-_\\p{L}\\p{N}\\p{IsDevanagari}\\p{IsThai}]{1,32}$", "name");
		}
		if (name.Any(char.IsUpper))
		{
			throw new FormatException("Name cannot contain any uppercase characters.");
		}
	}

	internal static void EnsureValidCommandDescription(string description)
	{
		Preconditions.NotNullOrEmpty(description, "description");
		Preconditions.AtLeast(description.Length, 1, "description");
		Preconditions.AtMost(description.Length, 100, "description");
	}
}
