using XMLData.Exceptions;

namespace XMLData.Parsers;

public static class floatParser
{
	public static float Parse(string _value)
	{
		if (StringParsers.TryParseFloat(_value, out var _result))
		{
			return _result;
		}
		throw new InvalidValueException("Expected float value, found \"" + _value + "\"", -1);
	}

	public static string Unparse(float _value)
	{
		return _value.ToCultureInvariantString();
	}
}
