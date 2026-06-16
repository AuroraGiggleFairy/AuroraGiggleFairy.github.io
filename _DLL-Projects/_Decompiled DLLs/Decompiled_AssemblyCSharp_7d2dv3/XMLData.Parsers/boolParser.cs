using XMLData.Exceptions;

namespace XMLData.Parsers;

public static class boolParser
{
	public static bool Parse(string _value)
	{
		if (_value == "true")
		{
			return true;
		}
		if (_value == "false")
		{
			return false;
		}
		throw new InvalidValueException("Expected bool value, found \"" + _value + "\"", -1);
	}

	public static string Unparse(bool _value)
	{
		if (!_value)
		{
			return "false";
		}
		return "true";
	}
}
