using InControl;
using UnityEngine;
using UnityEngine.Scripting;

[XuiXmlAttributeConvertersClass]
[UnityEngine.Scripting.Preserve]
public static class ParsingConverters
{
	[XuiXmlAttributeConverter]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool parseColor(string _inputValue, out Color _outputValue)
	{
		if (string.IsNullOrEmpty(_inputValue))
		{
			_outputValue = Color.white;
			return true;
		}
		if (_inputValue.Contains('.'))
		{
			return StringParsers.TryParseColor(_inputValue, out _outputValue);
		}
		if (!StringParsers.TryParseColor32(_inputValue, out var _output))
		{
			_outputValue = default(Color);
			return false;
		}
		_outputValue = _output;
		return true;
	}

	[XuiXmlAttributeConverter]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool parseColor32(string _inputValue, out Color32 _outputValue)
	{
		if (string.IsNullOrEmpty(_inputValue))
		{
			_outputValue = Color.white;
			return true;
		}
		if (!_inputValue.Contains('.'))
		{
			return StringParsers.TryParseColor32(_inputValue, out _outputValue);
		}
		if (!StringParsers.TryParseColor(_inputValue, out var _output))
		{
			_outputValue = default(Color32);
			return false;
		}
		_outputValue = _output;
		return true;
	}

	[XuiXmlAttributeConverter]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool parseStringList(string _inputValue, out string[] _outputValue)
	{
		_outputValue = StringParsers.ParseList(_inputValue, ';', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _startIndex, int _endIndex) => _s.Substring(_startIndex, _endIndex - _startIndex + 1)).ToArray();
		return true;
	}

	[XuiXmlAttributeConverter]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool parseInputAction(string _value, out PlayerAction _outputValue)
	{
		if (_value == "")
		{
			_outputValue = null;
			return true;
		}
		_outputValue = LocalPlayerUI.primaryUI.playerInput.GUIActions.GetPlayerActionByName(_value);
		return _outputValue != null;
	}

	[XuiXmlAttributeConverter]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool parseColorToColor32(Color _inputValue, out Color32 _outputValue)
	{
		_outputValue = _inputValue;
		return true;
	}

	[XuiXmlAttributeConverter]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool parseColor32ToColor(Color32 _inputValue, out Color _outputValue)
	{
		_outputValue = _inputValue;
		return true;
	}

	[XuiXmlAttributeConverter]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool parseDoubleToFloat(double _inputValue, out float _outputValue)
	{
		_outputValue = (float)_inputValue;
		return true;
	}
}
