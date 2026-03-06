using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal struct EmbedProvider
{
	public string Name
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public string Url
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	private string DebuggerDisplay => Name + " (" + Url + ")";

	internal EmbedProvider(string name, string url)
	{
		Name = name;
		Url = url;
	}

	public override string ToString()
	{
		return Name;
	}

	public static bool operator ==(EmbedProvider? left, EmbedProvider? right)
	{
		if (left.HasValue)
		{
			return left.Equals(right);
		}
		return !right.HasValue;
	}

	public static bool operator !=(EmbedProvider? left, EmbedProvider? right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is EmbedProvider value)
		{
			return Equals(value);
		}
		return false;
	}

	public bool Equals(EmbedProvider? embedProvider)
	{
		return GetHashCode() == embedProvider?.GetHashCode();
	}

	public override int GetHashCode()
	{
		return (Name, Url).GetHashCode();
	}
}
