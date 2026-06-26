using System;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_ComboBoxOrdinal<TValue> : XUiC_ComboBox<TValue> where TValue : struct, IEquatable<TValue>, IComparable<TValue>, IFormattable, IConvertible
{
	public string FormatString;

	public override TValue Value
	{
		get
		{
			return currentValue;
		}
		set
		{
			if (!currentValue.Equals(value))
			{
				if (value.CompareTo(Max) > 0)
				{
					value = Max;
				}
				else if (value.CompareTo(Min) < 0)
				{
					value = Min;
				}
				currentValue = value;
				IsDirty = true;
				UpdateLabel();
			}
		}
	}

	public override int IndexElementCount
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return -1;
		}
	}

	public override int IndexMarkerIndex
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return -1;
		}
	}

	public override void OnOpen()
	{
		if (currentValue.CompareTo(Max) > 0)
		{
			Value = Max;
		}
		else if (currentValue.CompareTo(Min) < 0)
		{
			Value = Min;
		}
		base.OnOpen();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateLabel()
	{
		base.ValueText = currentValue.ToString(FormatString, Utils.StandardCulture);
	}

	public void UpdateLabel(string text)
	{
		base.ValueText = text;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDifferentValue(TValue _oldVal, TValue _currentValue)
	{
		return !_oldVal.Equals(_currentValue);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void BackPressed()
	{
		if (currentValue.CompareTo(Min) < 0)
		{
			currentValue = (Wrap ? Max : Min);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ForwardPressed()
	{
		if (currentValue.CompareTo(Max) > 0)
		{
			currentValue = (Wrap ? Min : Max);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMax()
	{
		return currentValue.CompareTo(Max) == 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMin()
	{
		return currentValue.CompareTo(Min) == 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isEmpty()
	{
		return false;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "format_string")
		{
			FormatString = (string.IsNullOrEmpty(_value) ? null : _value);
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "isnumber")
		{
			_value = true.ToString();
			return true;
		}
		return base.GetBindingValue(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_ComboBoxOrdinal()
	{
	}
}
