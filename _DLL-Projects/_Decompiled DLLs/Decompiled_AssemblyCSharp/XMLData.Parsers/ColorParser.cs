using System.Text.RegularExpressions;
using UnityEngine;
using XMLData.Exceptions;

namespace XMLData.Parsers;

public static class ColorParser
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex decimalMatcher = new Regex("^\\s*(\\d*\\.\\d+)\\s*,\\s*(\\d*\\.\\d+)\\s*,\\s*(\\d*\\.\\d+)\\s*$");

	public static Color Parse(string _value)
	{
		Match match = decimalMatcher.Match(_value);
		if (match.Success)
		{
			if (!StringParsers.TryParseFloat(match.Groups[1].Value, out var _result))
			{
				throw new InvalidValueException("Expected float value as first part of Color field, found \"" + match.Groups[1].Value + "\"", -1);
			}
			if (_result < 0f || _result > 1f)
			{
				throw new InvalidValueException("Expected float between 0 and 1 as first part of Color field, found " + _result.ToCultureInvariantString(), -1);
			}
			if (!StringParsers.TryParseFloat(match.Groups[2].Value, out var _result2))
			{
				throw new InvalidValueException("Expected float value as second part of Color field, found \"" + match.Groups[2].Value + "\"", -1);
			}
			if (_result2 < 0f || _result2 > 1f)
			{
				throw new InvalidValueException("Expected float between 0 and 1 as second part of Color field, found " + _result2.ToCultureInvariantString(), -1);
			}
			if (!StringParsers.TryParseFloat(match.Groups[3].Value, out var _result3))
			{
				throw new InvalidValueException("Expected float value as third part of Color field, found \"" + match.Groups[3].Value + "\"", -1);
			}
			if (_result3 < 0f || _result3 > 1f)
			{
				throw new InvalidValueException("Expected float between 0 and 1 as third part of Color field, found " + _result3.ToCultureInvariantString(), -1);
			}
			return new Color(_result, _result2, _result3, 1f);
		}
		return StringParsers.ParseHexColor(_value);
	}

	public static string Unparse(Color _value)
	{
		return $"{_value.r.ToCultureInvariantString()},{_value.g.ToCultureInvariantString()},{_value.b.ToCultureInvariantString()}";
	}
}
