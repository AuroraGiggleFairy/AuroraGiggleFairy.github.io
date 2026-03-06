using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Discord;

internal abstract class ApplicationCommandProperties
{
	private IReadOnlyDictionary<string, string> _nameLocalizations;

	private IReadOnlyDictionary<string, string> _descriptionLocalizations;

	internal abstract ApplicationCommandType Type { get; }

	public Optional<string> Name { get; set; }

	public Optional<bool> IsDefaultPermission { get; set; }

	public IReadOnlyDictionary<string, string> NameLocalizations
	{
		get
		{
			return _nameLocalizations;
		}
		set
		{
			if (value != null)
			{
				foreach (var (text3, text4) in value)
				{
					if (!Regex.IsMatch(text3, "^\\w{2}(?:-\\w{2})?$"))
					{
						throw new ArgumentException("Invalid locale: " + text3, "locale");
					}
					Preconditions.AtLeast(text4.Length, 1, "name");
					Preconditions.AtMost(text4.Length, 32, "name");
					if (Type == ApplicationCommandType.Slash && !Regex.IsMatch(text4, "^[-_\\p{L}\\p{N}\\p{IsDevanagari}\\p{IsThai}]{1,32}$"))
					{
						throw new ArgumentException("Name must match the regex ^[-_\\p{L}\\p{N}\\p{IsDevanagari}\\p{IsThai}]{1,32}$", "name");
					}
				}
			}
			_nameLocalizations = value;
		}
	}

	public IReadOnlyDictionary<string, string> DescriptionLocalizations
	{
		get
		{
			return _descriptionLocalizations;
		}
		set
		{
			if (value != null)
			{
				foreach (var (text3, text4) in value)
				{
					if (!Regex.IsMatch(text3, "^\\w{2}(?:-\\w{2})?$"))
					{
						throw new ArgumentException("Invalid locale: " + text3, "locale");
					}
					Preconditions.AtLeast(text4.Length, 1, "description");
					Preconditions.AtMost(text4.Length, 100, "description");
				}
			}
			_descriptionLocalizations = value;
		}
	}

	public Optional<bool> IsDMEnabled { get; set; }

	public Optional<GuildPermission> DefaultMemberPermissions { get; set; }

	internal ApplicationCommandProperties()
	{
	}
}
