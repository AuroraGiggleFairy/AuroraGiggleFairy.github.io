using System;

namespace Discord;

internal class TextInputBuilder
{
	public const int MaxPlaceholderLength = 100;

	public const int LargestMaxLength = 4000;

	private string _customId;

	private int? _maxLength;

	private int? _minLength;

	private string _placeholder;

	private string _value;

	public string CustomId
	{
		get
		{
			return _customId;
		}
		set
		{
			int? num = value?.Length;
			if (num.HasValue)
			{
				int valueOrDefault = num.GetValueOrDefault();
				if (valueOrDefault > 100)
				{
					throw new ArgumentOutOfRangeException("value", $"Custom Id length must be less or equal to {100}.");
				}
				if (valueOrDefault == 0)
				{
					throw new ArgumentOutOfRangeException("value", "Custom Id length must be at least 1.");
				}
			}
			_customId = value;
		}
	}

	public TextInputStyle Style { get; set; } = TextInputStyle.Short;

	public string Label { get; set; }

	public string Placeholder
	{
		get
		{
			return _placeholder;
		}
		set
		{
			if ((value?.Length ?? 0) > 100)
			{
				throw new ArgumentException($"Placeholder cannot have more than {100} characters.");
			}
			_placeholder = value;
		}
	}

	public int? MinLength
	{
		get
		{
			return _minLength;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", "MinLength must not be less than 0");
			}
			if (value > 4000)
			{
				throw new ArgumentOutOfRangeException("value", $"MinLength must not be greater than {4000}");
			}
			if (value > (MaxLength ?? 4000))
			{
				throw new ArgumentOutOfRangeException("value", "MinLength must be less than MaxLength");
			}
			_minLength = value;
		}
	}

	public int? MaxLength
	{
		get
		{
			return _maxLength;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", "MaxLength must not be less than 0");
			}
			if (value > 4000)
			{
				throw new ArgumentOutOfRangeException("value", $"MaxLength most not be greater than {4000}");
			}
			if (value < (MinLength ?? (-1)))
			{
				throw new ArgumentOutOfRangeException("value", $"MaxLength must be greater than MinLength ({MinLength})");
			}
			_maxLength = value;
		}
	}

	public bool? Required { get; set; }

	public string Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (value?.Length > (MaxLength ?? 4000))
			{
				throw new ArgumentOutOfRangeException("value", $"Value must not be longer than {MaxLength ?? 4000}.");
			}
			if (value?.Length < MinLength.GetValueOrDefault())
			{
				throw new ArgumentOutOfRangeException("value", $"Value must not be shorter than {MinLength}");
			}
			_value = value;
		}
	}

	public TextInputBuilder(string label, string customId, TextInputStyle style = TextInputStyle.Short, string placeholder = null, int? minLength = null, int? maxLength = null, bool? required = null, string value = null)
	{
		Label = label;
		Style = style;
		CustomId = customId;
		Placeholder = placeholder;
		MinLength = minLength;
		MaxLength = maxLength;
		Required = required;
		Value = value;
	}

	public TextInputBuilder()
	{
	}

	public TextInputBuilder WithLabel(string label)
	{
		Label = label;
		return this;
	}

	public TextInputBuilder WithStyle(TextInputStyle style)
	{
		Style = style;
		return this;
	}

	public TextInputBuilder WithCustomId(string customId)
	{
		CustomId = customId;
		return this;
	}

	public TextInputBuilder WithPlaceholder(string placeholder)
	{
		Placeholder = placeholder;
		return this;
	}

	public TextInputBuilder WithValue(string value)
	{
		Value = value;
		return this;
	}

	public TextInputBuilder WithMinLength(int minLength)
	{
		MinLength = minLength;
		return this;
	}

	public TextInputBuilder WithMaxLength(int maxLength)
	{
		MaxLength = maxLength;
		return this;
	}

	public TextInputBuilder WithRequired(bool required)
	{
		Required = required;
		return this;
	}

	public TextInputComponent Build()
	{
		if (string.IsNullOrEmpty(CustomId))
		{
			throw new ArgumentException("TextInputComponents must have a custom id.", "CustomId");
		}
		if (string.IsNullOrWhiteSpace(Label))
		{
			throw new ArgumentException("TextInputComponents must have a label.", "Label");
		}
		return new TextInputComponent(CustomId, Label, Placeholder, MinLength, MaxLength, Style, Required, Value);
	}
}
