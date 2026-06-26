using System;

public class CachedStringFormatterFloat : CachedStringFormatter<float>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string format;

	public CachedStringFormatterFloat(string _format = null)
		: base((Func<float, string>)null)
	{
		formatter = formatterFunc;
		format = _format;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string formatterFunc(float _f)
	{
		return _f.ToCultureInvariantString(format);
	}
}
