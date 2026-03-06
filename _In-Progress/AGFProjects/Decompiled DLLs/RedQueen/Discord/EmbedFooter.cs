using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal struct EmbedFooter
{
	public string Text
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public string IconUrl
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public string ProxyUrl
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	private string DebuggerDisplay => Text + " (" + IconUrl + ")";

	internal EmbedFooter(string text, string iconUrl, string proxyUrl)
	{
		Text = text;
		IconUrl = iconUrl;
		ProxyUrl = proxyUrl;
	}

	public override string ToString()
	{
		return Text;
	}

	public static bool operator ==(EmbedFooter? left, EmbedFooter? right)
	{
		if (left.HasValue)
		{
			return left.Equals(right);
		}
		return !right.HasValue;
	}

	public static bool operator !=(EmbedFooter? left, EmbedFooter? right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is EmbedFooter value)
		{
			return Equals(value);
		}
		return false;
	}

	public bool Equals(EmbedFooter? embedFooter)
	{
		return GetHashCode() == embedFooter?.GetHashCode();
	}

	public override int GetHashCode()
	{
		return (Text, IconUrl, ProxyUrl).GetHashCode();
	}
}
