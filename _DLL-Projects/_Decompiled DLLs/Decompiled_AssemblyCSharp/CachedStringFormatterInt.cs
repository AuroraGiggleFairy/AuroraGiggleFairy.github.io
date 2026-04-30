using System;

public class CachedStringFormatterInt : CachedStringFormatter<int>
{
	public CachedStringFormatterInt()
		: base((Func<int, string>)formatterFunc)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string formatterFunc(int _i)
	{
		return _i.ToString();
	}
}
