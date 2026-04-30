using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal struct EmbedImage
{
	public string Url
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public string ProxyUrl
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public int? Height
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public int? Width
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	private string DebuggerDisplay => Url + " (" + ((Width.HasValue && Height.HasValue) ? $"{Width}x{Height}" : "0x0") + ")";

	internal EmbedImage(string url, string proxyUrl, int? height, int? width)
	{
		Url = url;
		ProxyUrl = proxyUrl;
		Height = height;
		Width = width;
	}

	public override string ToString()
	{
		return Url;
	}

	public static bool operator ==(EmbedImage? left, EmbedImage? right)
	{
		if (left.HasValue)
		{
			return left.Equals(right);
		}
		return !right.HasValue;
	}

	public static bool operator !=(EmbedImage? left, EmbedImage? right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is EmbedImage value)
		{
			return Equals(value);
		}
		return false;
	}

	public bool Equals(EmbedImage? embedImage)
	{
		return GetHashCode() == embedImage?.GetHashCode();
	}

	public override int GetHashCode()
	{
		return (Height, Width, Url, ProxyUrl).GetHashCode();
	}
}
