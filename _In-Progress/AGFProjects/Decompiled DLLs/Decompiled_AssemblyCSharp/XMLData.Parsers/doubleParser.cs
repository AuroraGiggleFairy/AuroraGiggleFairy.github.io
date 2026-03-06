using XMLData.Exceptions;

namespace XMLData.Parsers;

public static class doubleParser
{
	public static double Parse(string _value)
	{
		if (StringParsers.TryParseDouble(_value, out var _result))
		{
			return _result;
		}
		throw new InvalidValueException("Expected double value, found \"" + _value + "\"", -1);
	}

	public static string Unparse(double _value)
	{
		return _value.ToCultureInvariantString();
	}
}
