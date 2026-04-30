using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal struct EmbedField
{
	public string Name
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
		internal set; }

	public string Value
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
		internal set; }

	public bool Inline
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
		internal set; }

	private string DebuggerDisplay => Name + " (" + Value;

	internal EmbedField(string name, string value, bool inline)
	{
		Name = name;
		Value = value;
		Inline = inline;
	}

	public override string ToString()
	{
		return Name;
	}

	public static bool operator ==(EmbedField? left, EmbedField? right)
	{
		if (left.HasValue)
		{
			return left.Equals(right);
		}
		return !right.HasValue;
	}

	public static bool operator !=(EmbedField? left, EmbedField? right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is EmbedField value)
		{
			return Equals(value);
		}
		return false;
	}

	public bool Equals(EmbedField? embedField)
	{
		return GetHashCode() == embedField?.GetHashCode();
	}

	public override int GetHashCode()
	{
		return (Name, Value, Inline).GetHashCode();
	}
}
