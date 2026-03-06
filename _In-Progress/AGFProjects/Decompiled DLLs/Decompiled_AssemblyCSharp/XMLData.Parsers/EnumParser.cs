using System;

namespace XMLData.Parsers;

public static class EnumParser
{
	public static TEnum Parse<TEnum>(string _value) where TEnum : struct, IConvertible
	{
		return EnumUtils.Parse<TEnum>(_value);
	}

	public static string Unparse<TEnum>(TEnum _value) where TEnum : struct, IConvertible
	{
		return _value.ToStringCached();
	}
}
