using System;

namespace Discord;

internal class EmbedFieldBuilder
{
	private string _name;

	private string _value;

	public const int MaxFieldNameLength = 256;

	public const int MaxFieldValueLength = 1024;

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				throw new ArgumentException("Field name must not be null, empty or entirely whitespace.", "Name");
			}
			if (value.Length > 256)
			{
				throw new ArgumentException($"Field name length must be less than or equal to {256}.", "Name");
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
			string text = value?.ToString();
			if (string.IsNullOrWhiteSpace(text))
			{
				throw new ArgumentException("Field value must not be null or empty.", "Value");
			}
			if (text.Length > 1024)
			{
				throw new ArgumentException($"Field value length must be less than or equal to {1024}.", "Value");
			}
			_value = text;
		}
	}

	public bool IsInline { get; set; }

	public EmbedFieldBuilder WithName(string name)
	{
		Name = name;
		return this;
	}

	public EmbedFieldBuilder WithValue(object value)
	{
		Value = value;
		return this;
	}

	public EmbedFieldBuilder WithIsInline(bool isInline)
	{
		IsInline = isInline;
		return this;
	}

	public EmbedField Build()
	{
		return new EmbedField(Name, Value.ToString(), IsInline);
	}

	public static bool operator ==(EmbedFieldBuilder left, EmbedFieldBuilder right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public static bool operator !=(EmbedFieldBuilder left, EmbedFieldBuilder right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is EmbedFieldBuilder embedFieldBuilder)
		{
			return Equals(embedFieldBuilder);
		}
		return false;
	}

	public bool Equals(EmbedFieldBuilder embedFieldBuilder)
	{
		if (_name == embedFieldBuilder?._name && _value == embedFieldBuilder?._value)
		{
			return IsInline == embedFieldBuilder?.IsInline;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
