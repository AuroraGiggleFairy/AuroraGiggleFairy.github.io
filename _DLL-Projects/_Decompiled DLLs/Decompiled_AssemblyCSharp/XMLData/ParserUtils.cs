using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using ICSharpCode.WpfDesign.XamlDom;
using XMLData.Exceptions;

namespace XMLData;

public static class ParserUtils
{
	public delegate bool ConverterDelegate<T>(string _in, out T _out);

	public static readonly CultureInfo ci;

	[PublicizedFrom(EAccessModifier.Private)]
	static ParserUtils()
	{
		ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
		ci.NumberFormat.CurrencyDecimalSeparator = ".";
		ci.NumberFormat.NumberDecimalSeparator = ".";
		ci.NumberFormat.NumberGroupSeparator = ",";
	}

	public static bool TryParseFloat(string _s, out float _result)
	{
		return float.TryParse(_s, NumberStyles.Any, ci, out _result);
	}

	public static bool TryParseDouble(string _s, out double _result)
	{
		return double.TryParse(_s, NumberStyles.Any, ci, out _result);
	}

	public static bool ParseBoolAttribute(PositionXmlElement _elem, string _attrName, bool _mandatory, bool _defaultValue)
	{
		if (_elem.HasAttribute(_attrName))
		{
			string attribute = _elem.GetAttribute(_attrName);
			if (attribute == "true")
			{
				return true;
			}
			if (attribute == "false")
			{
				return false;
			}
			throw new InvalidValueException("Element has invalid value \"" + attribute + "\" for bool attribute \"" + _attrName + "\"", _elem.LineNumber);
		}
		if (_mandatory)
		{
			throw new MissingAttributeException("Element \"\" + _elem.Name + \"\" is missing required attribute \"" + _attrName + "\"", _elem.LineNumber);
		}
		return _defaultValue;
	}

	public static string ParseStringAttribute(PositionXmlElement _elem, string _attrName, bool _mandatory, string _defaultValue = null)
	{
		if (_elem.HasAttribute(_attrName))
		{
			return _elem.GetAttribute(_attrName);
		}
		if (_mandatory)
		{
			throw new MissingAttributeException("Element \"" + _elem.Name + "\" is missing required attribute \"" + _attrName + "\"", _elem.LineNumber);
		}
		return _defaultValue;
	}

	public static string ParseStringAttribute(XElement _elem, string _attrName, bool _mandatory, string _defaultValue = null)
	{
		if (_elem.HasAttribute(_attrName))
		{
			return _elem.GetAttribute(_attrName);
		}
		if (_mandatory)
		{
			throw new MissingAttributeException("Element \"" + _elem.Name?.ToString() + "\" is missing required attribute \"" + _attrName + "\"", ((IXmlLineInfo)_elem).LineNumber);
		}
		return _defaultValue;
	}

	public static int ParseIntAttribute(PositionXmlElement _elem, string _attrName, bool _mandatory, int _defaultValue = 0)
	{
		if (_elem.HasAttribute(_attrName))
		{
			if (int.TryParse(_elem.GetAttribute(_attrName), out var result))
			{
				return result;
			}
			throw new InvalidValueException("Element has invalid value \"" + result + "\" for int attribute \"" + _attrName + "\"", _elem.LineNumber);
		}
		if (_mandatory)
		{
			throw new MissingAttributeException("Element \"\" + _elem.Name + \"\" is missing required attribute \"" + _attrName + "\"", _elem.LineNumber);
		}
		return _defaultValue;
	}

	public static void ParseRangeAttribute<T>(PositionXmlElement _elem, string _attrName, bool _mandatory, ref Range<T> _defaultAndOutput, bool _allowFixedValue, ConverterDelegate<T> _converter)
	{
		if (!_elem.HasAttribute(_attrName))
		{
			if (_mandatory)
			{
				throw new MissingAttributeException("Element \"\" + _elem.Name + \"\" is missing required attribute \"" + _attrName + "\"", _elem.LineNumber);
			}
			return;
		}
		string attribute = _elem.GetAttribute(_attrName);
		string[] array = attribute.Split('-');
		Range<T> range = new Range<T>();
		if (array.Length > 2)
		{
			throw new InvalidValueException("Element has invalid value \"" + attribute + "\" for range attribute \"" + _attrName + "\" with more than one separator \"-\"", _elem.LineNumber);
		}
		if (!_allowFixedValue && array.Length < 2)
		{
			throw new InvalidValueException("Element has invalid value \"" + attribute + "\" for range attribute \"" + _attrName + "\" with less than one separator \"-\"", _elem.LineNumber);
		}
		if (array[0] != "*")
		{
			if (!_converter(array[0], out var _out))
			{
				throw new InvalidValueException("Element has invalid min range value \"" + array[0] + "\" for attribute \"" + _attrName + "\"", _elem.LineNumber);
			}
			range.min = _out;
			range.hasMin = true;
		}
		if (array.Length > 1)
		{
			if (array[1] != "*")
			{
				if (!_converter(array[1], out var _out2))
				{
					throw new InvalidValueException("Element has invalid max range value \"" + array[1] + "\" for attribute \"" + _attrName + "\"", _elem.LineNumber);
				}
				range.max = _out2;
				range.hasMax = true;
			}
		}
		else
		{
			range.hasMax = range.hasMin;
			range.max = range.min;
		}
		_defaultAndOutput = range;
	}
}
