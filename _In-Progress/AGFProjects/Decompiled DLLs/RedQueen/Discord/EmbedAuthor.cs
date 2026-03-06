using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal struct EmbedAuthor
{
	public string Name
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
		internal set; }

	public string Url
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
		internal set; }

	public string IconUrl
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
		internal set; }

	public string ProxyIconUrl
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
		internal set; }

	private string DebuggerDisplay => Name + " (" + Url + ")";

	internal EmbedAuthor(string name, string url, string iconUrl, string proxyIconUrl)
	{
		Name = name;
		Url = url;
		IconUrl = iconUrl;
		ProxyIconUrl = proxyIconUrl;
	}

	public override string ToString()
	{
		return Name;
	}

	public static bool operator ==(EmbedAuthor? left, EmbedAuthor? right)
	{
		if (left.HasValue)
		{
			return left.Equals(right);
		}
		return !right.HasValue;
	}

	public static bool operator !=(EmbedAuthor? left, EmbedAuthor? right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is EmbedAuthor value)
		{
			return Equals(value);
		}
		return false;
	}

	public bool Equals(EmbedAuthor? embedAuthor)
	{
		return GetHashCode() == embedAuthor?.GetHashCode();
	}

	public override int GetHashCode()
	{
		return (Name, Url, IconUrl).GetHashCode();
	}
}
