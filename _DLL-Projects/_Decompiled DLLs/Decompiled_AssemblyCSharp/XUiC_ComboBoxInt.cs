using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxInt : XUiC_ComboBoxOrdinal<long>
{
	public long IncrementSize = 1L;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat fillvalueFormatter = new CachedStringFormatterFloat();

	public XUiC_ComboBoxInt()
	{
		Max = long.MaxValue;
		Min = long.MinValue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void BackPressed()
	{
		currentValue -= IncrementSize;
		base.BackPressed();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ForwardPressed()
	{
		currentValue += IncrementSize;
		base.ForwardPressed();
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "value_min":
			if (!_value.EqualsCaseInsensitive("@def"))
			{
				Min = StringParsers.ParseSInt64(_value);
			}
			return true;
		case "value_max":
			if (!_value.EqualsCaseInsensitive("@def"))
			{
				Max = StringParsers.ParseSInt64(_value);
			}
			return true;
		case "value_increment":
			if (!_value.EqualsCaseInsensitive("@def"))
			{
				IncrementSize = StringParsers.ParseSInt64(_value);
			}
			return true;
		default:
			return base.ParseAttribute(_name, _value, _parent);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "fillvalue")
		{
			_value = fillvalueFormatter.Format(((float)currentValue - (float)Min) / (float)(Max - Min));
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setRelativeValue(double _value)
	{
		long value = Value;
		Value = (long)((double)(Max - Min) * _value) + Min;
		TriggerValueChangedEvent(value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void incrementalChangeValue(double _value)
	{
		long num = (long)((double)(Max - Min) * _value * 0.5);
		if (_value > 0.0 && num == 0L)
		{
			num = 1L;
		}
		else if (_value < 0.0 && num == 0L)
		{
			num = -1L;
		}
		if (num > IncrementSize)
		{
			num = IncrementSize;
		}
		else if (num < -IncrementSize)
		{
			num = -IncrementSize;
		}
		long value = Value;
		long num2 = Value + num;
		if (_value < 0.0 && num2 < Min && Wrap)
		{
			num2 = Max;
		}
		else if (_value > 0.0 && num2 > Max && Wrap)
		{
			num2 = Min;
		}
		Value = num2;
		TriggerValueChangedEvent(value);
	}
}
