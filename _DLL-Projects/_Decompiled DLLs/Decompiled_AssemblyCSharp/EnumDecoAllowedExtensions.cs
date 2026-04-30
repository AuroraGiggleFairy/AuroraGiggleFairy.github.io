using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class EnumDecoAllowedExtensions
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly EnumDictionary<EnumDecoAllowed, string> s_toStringCache = new EnumDictionary<EnumDecoAllowed, string>();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EnumDecoAllowedSlope GetSlope(this EnumDecoAllowed decoAllowed)
	{
		return (EnumDecoAllowedSlope)((int)(decoAllowed & (EnumDecoAllowed.SlopeLo | EnumDecoAllowed.SlopeHi)) / 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EnumDecoAllowed WithSlope(this EnumDecoAllowed decoAllowed, EnumDecoAllowedSlope slope)
	{
		return (EnumDecoAllowed)(((uint)decoAllowed & 0xFFFFFFFCu) | (uint)slope);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EnumDecoAllowedSize GetSize(this EnumDecoAllowed decoAllowed)
	{
		return (EnumDecoAllowedSize)((int)(decoAllowed & (EnumDecoAllowed.SizeLo | EnumDecoAllowed.SizeHi)) / 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EnumDecoAllowed WithSize(this EnumDecoAllowed decoAllowed, EnumDecoAllowedSize size)
	{
		return (EnumDecoAllowed)(((uint)decoAllowed & 0xFFFFFFF3u) | (uint)((int)size * 4));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool GetStreetOnly(this EnumDecoAllowed decoAllowed)
	{
		return (decoAllowed & EnumDecoAllowed.StreetOnly) == EnumDecoAllowed.StreetOnly;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EnumDecoAllowed WithStreetOnly(this EnumDecoAllowed decoAllowed, bool streetOnly)
	{
		if (streetOnly)
		{
			return decoAllowed | EnumDecoAllowed.StreetOnly;
		}
		return (EnumDecoAllowed)((uint)decoAllowed & 0xFFFFFFEFu);
	}

	public static bool IsNothing(this EnumDecoAllowed decoAllowed)
	{
		if (!decoAllowed.GetSlope().IsNothing())
		{
			return decoAllowed.GetSize().IsNothing();
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNothing(this EnumDecoAllowedSlope decoSlope)
	{
		return (int)decoSlope >= 2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNothing(this EnumDecoAllowedSize decoSize)
	{
		return (int)decoSize >= 2;
	}

	public static string ToStringFriendlyCached(this EnumDecoAllowed decoAllowed)
	{
		if (s_toStringCache.TryGetValue(decoAllowed, out var value))
		{
			return value;
		}
		string text = ToStringInternal(decoAllowed);
		s_toStringCache[decoAllowed] = text;
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string ToStringInternal(EnumDecoAllowed decoAllowed)
	{
		switch (decoAllowed)
		{
		case EnumDecoAllowed.Everything:
			return "Everything";
		case EnumDecoAllowed.Nothing:
			return "Nothing";
		default:
		{
			List<string> list = new List<string>();
			EnumDecoAllowedSlope slope = decoAllowed.GetSlope();
			if ((int)slope > 0)
			{
				list.Add(slope.ToStringCached());
			}
			EnumDecoAllowedSize size = decoAllowed.GetSize();
			if ((int)size > 0)
			{
				list.Add(size.ToStringCached());
			}
			if (decoAllowed.GetStreetOnly())
			{
				list.Add("StreetOnly");
			}
			if (list.Count > 0)
			{
				return string.Join(",", list);
			}
			return "Unknown";
		}
		}
	}
}
