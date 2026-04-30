using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Discord;

internal class ApplicationCommandOptionChoiceProperties
{
	private string _name;

	private object _value;

	private IDictionary<string, string> _nameLocalizations = new Dictionary<string, string>();

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			int? num = value?.Length;
			if (num.HasValue)
			{
				int valueOrDefault = num.GetValueOrDefault();
				if (valueOrDefault > 100)
				{
					throw new ArgumentOutOfRangeException("value", "Name length must be less than or equal to 100.");
				}
				if (valueOrDefault == 0)
				{
					throw new ArgumentOutOfRangeException("value", "Name length must at least 1.");
				}
			}
			_name = value;
		}
	}

	public object Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (value != null && !(value is string) && !value.IsNumericType())
			{
				throw new ArgumentException("The value of a choice must be a string or a numeric type!");
			}
			_value = value;
		}
	}

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
				foreach (var (input, text3) in value)
				{
					if (!Regex.IsMatch(input, "^\\w{2}(?:-\\w{2})?$"))
					{
						throw new ArgumentException("Key values of the dictionary must be valid language codes.");
					}
					int length = text3.Length;
					if (length <= 100)
					{
						if (length != 0)
						{
							continue;
						}
						throw new ArgumentOutOfRangeException("value", "Name length must at least 1.");
					}
					throw new ArgumentOutOfRangeException("value", "Name length must be less than or equal to 100.");
				}
			}
			_nameLocalizations = value;
		}
	}
}
