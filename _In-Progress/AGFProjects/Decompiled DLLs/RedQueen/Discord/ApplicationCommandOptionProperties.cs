using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Discord;

internal class ApplicationCommandOptionProperties
{
	private string _name;

	private string _description;

	private IDictionary<string, string> _nameLocalizations = new Dictionary<string, string>();

	private IDictionary<string, string> _descriptionLocalizations = new Dictionary<string, string>();

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			EnsureValidOptionName(value);
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
			EnsureValidOptionDescription(value);
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

	public List<ApplicationCommandOptionProperties> Options { get; set; }

	public List<ChannelType> ChannelTypes { get; set; }

	public IDictionary<string, string> NameLocalizations
	{
		get
		{
			return _nameLocalizations;
		}
		set
		{
			if (value != null)
			{
				foreach (var (text3, name) in value)
				{
					if (!Regex.IsMatch(text3, "^\\w{2}(?:-\\w{2})?$"))
					{
						throw new ArgumentException("Invalid locale: " + text3, "locale");
					}
					EnsureValidOptionName(name);
				}
			}
			_nameLocalizations = value;
		}
	}

	public IDictionary<string, string> DescriptionLocalizations
	{
		get
		{
			return _descriptionLocalizations;
		}
		set
		{
			if (value != null)
			{
				foreach (var (text3, description) in value)
				{
					if (!Regex.IsMatch(text3, "^\\w{2}(?:-\\w{2})?$"))
					{
						throw new ArgumentException("Invalid locale: " + text3, "locale");
					}
					EnsureValidOptionDescription(description);
				}
			}
			_descriptionLocalizations = value;
		}
	}

	private static void EnsureValidOptionName(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name", "Name cannot be null.");
		}
		if (name.Length > 32)
		{
			throw new ArgumentOutOfRangeException("name", "Name length must be less than or equal to 32.");
		}
		if (!Regex.IsMatch(name, "^[-_\\p{L}\\p{N}\\p{IsDevanagari}\\p{IsThai}]{1,32}$"))
		{
			throw new ArgumentException("Name must match the regex ^[-_\\p{L}\\p{N}\\p{IsDevanagari}\\p{IsThai}]{1,32}$", "name");
		}
		if (name.Any(char.IsUpper))
		{
			throw new FormatException("Name cannot contain any uppercase characters.");
		}
	}

	private static void EnsureValidOptionDescription(string description)
	{
		int length = description.Length;
		if (length <= 100)
		{
			if (length != 0)
			{
				return;
			}
			throw new ArgumentOutOfRangeException("description", "Description length must at least 1.");
		}
		throw new ArgumentOutOfRangeException("description", "Description length must be less than or equal to 100.");
	}
}
