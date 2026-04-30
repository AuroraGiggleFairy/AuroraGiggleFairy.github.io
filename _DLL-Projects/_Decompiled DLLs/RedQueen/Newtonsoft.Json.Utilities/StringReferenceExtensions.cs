using System;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Utilities;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal static class StringReferenceExtensions
{
	public static int IndexOf(this StringReference s, char c, int startIndex, int length)
	{
		int num = Array.IndexOf(s.Chars, c, s.StartIndex + startIndex, length);
		if (num == -1)
		{
			return -1;
		}
		return num - s.StartIndex;
	}

	public static bool StartsWith(this StringReference s, string text)
	{
		if (text.Length > s.Length)
		{
			return false;
		}
		char[] chars = s.Chars;
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] != chars[i + s.StartIndex])
			{
				return false;
			}
		}
		return true;
	}

	public static bool EndsWith(this StringReference s, string text)
	{
		if (text.Length > s.Length)
		{
			return false;
		}
		char[] chars = s.Chars;
		int num = s.StartIndex + s.Length - text.Length;
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] != chars[i + num])
			{
				return false;
			}
		}
		return true;
	}
}
