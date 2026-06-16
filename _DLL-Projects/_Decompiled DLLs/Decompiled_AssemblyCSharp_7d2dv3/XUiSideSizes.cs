public readonly struct XUiSideSizes
{
	public readonly int Left;

	public readonly int Right;

	public readonly int Top;

	public readonly int Bottom;

	public int SumLeftRight => Left + Right;

	public int SumTopBottom => Top + Bottom;

	public XUiSideSizes(int _left, int _right, int _top, int _bottom)
	{
		Left = _left;
		Right = _right;
		Top = _top;
		Bottom = _bottom;
	}

	public XUiSideSizes SetLeft(int _value)
	{
		return new XUiSideSizes(_value, Right, Top, Bottom);
	}

	public XUiSideSizes SetRight(int _value)
	{
		return new XUiSideSizes(Left, _value, Top, Bottom);
	}

	public XUiSideSizes SetTop(int _value)
	{
		return new XUiSideSizes(Left, Right, _value, Bottom);
	}

	public XUiSideSizes SetBottom(int _value)
	{
		return new XUiSideSizes(Left, Right, Top, _value);
	}

	public static bool TryParse(string _value, out XUiSideSizes _result, string _valueName)
	{
		StringParsers.SeparatorPositions separatorPositions = StringParsers.GetSeparatorPositions(_value, ',', 3);
		if (separatorPositions.TotalFound > 3)
		{
			Log.Warning($"[XUi] Invalid number of values for {_valueName}: {separatorPositions.TotalFound}, max of 4 expected (input string: '{_value}')");
			_result = default(XUiSideSizes);
			return false;
		}
		if (!StringParsers.TryParseSInt32(_value, out var _result2, 0, separatorPositions.Sep1 - 1))
		{
			Log.Warning("[XUi] " + _valueName + " can not be parsed, not an integer as 1st value (input string: '" + _value + "')");
			_result = default(XUiSideSizes);
			return false;
		}
		if (separatorPositions.TotalFound == 0)
		{
			_result = new XUiSideSizes(_result2, _result2, _result2, _result2);
			return true;
		}
		if (!StringParsers.TryParseSInt32(_value, out var _result3, separatorPositions.Sep1 + 1, separatorPositions.Sep2 - 1))
		{
			Log.Warning("[XUi] " + _valueName + " can not be parsed, not an integer as 2nd value (input string: '" + _value + "')");
			_result = default(XUiSideSizes);
			return false;
		}
		if (separatorPositions.TotalFound == 1)
		{
			_result = new XUiSideSizes(_result3, _result3, _result2, _result2);
			return true;
		}
		if (!StringParsers.TryParseSInt32(_value, out var _result4, separatorPositions.Sep2 + 1, separatorPositions.Sep3 - 1))
		{
			Log.Warning("[XUi] " + _valueName + " can not be parsed, not an integer as 3rd value (input string: '" + _value + "')");
			_result = default(XUiSideSizes);
			return false;
		}
		if (separatorPositions.TotalFound == 2)
		{
			_result = new XUiSideSizes(_result3, _result3, _result2, _result4);
			return true;
		}
		if (!StringParsers.TryParseSInt32(_value, out var _result5, separatorPositions.Sep3 + 1, separatorPositions.Sep4 - 1))
		{
			Log.Warning("[XUi] " + _valueName + " can not be parsed, not an integer as 4th value (input string: '" + _value + "')");
			_result = default(XUiSideSizes);
			return false;
		}
		_result = new XUiSideSizes(_result5, _result3, _result2, _result4);
		return true;
	}
}
