using System;

namespace Discord;

internal class AutocompleteResult
{
	private object _value;

	private string _name;

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value", "Name cannot be null.");
			}
			int length = value.Length;
			if (length <= 100)
			{
				if (length == 0)
				{
					throw new ArgumentOutOfRangeException("value", "Name length must be at least 1.");
				}
				_name = value;
				return;
			}
			throw new ArgumentOutOfRangeException("value", "Name length must be less than or equal to 100.");
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
			if (!(value is string) && !value.IsNumericType())
			{
				throw new ArgumentException("value must be a numeric type or a string!");
			}
			_value = value;
		}
	}

	public AutocompleteResult()
	{
	}

	public AutocompleteResult(string name, object value)
	{
		Name = name;
		Value = value;
	}
}
