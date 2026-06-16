using System;
using UnityEngine;

public class CachedStringFormatterXuiRgbaColor : CachedStringFormatter<Color32>
{
	public CachedStringFormatterXuiRgbaColor()
		: base((Func<Color32, string>)formatterFunc)
	{
		comparer1 = Color32EqualityComparer.Instance;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string formatterFunc(Color32 _color)
	{
		return _color.ToXuiColorString();
	}
}
