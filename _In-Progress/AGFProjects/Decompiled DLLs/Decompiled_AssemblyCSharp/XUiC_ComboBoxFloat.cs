using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxFloat : XUiC_ComboBoxOrdinal<double>
{
	public double IncrementSize = 1.0;

	public XUiC_ComboBoxFloat()
	{
		Max = 1.0;
		Min = 0.0;
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
				Min = StringParsers.ParseDouble(_value);
			}
			return true;
		case "value_max":
			if (!_value.EqualsCaseInsensitive("@def"))
			{
				Max = StringParsers.ParseDouble(_value);
			}
			return true;
		case "value_increment":
			if (!_value.EqualsCaseInsensitive("@def"))
			{
				IncrementSize = StringParsers.ParseDouble(_value);
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
			_value = ((currentValue - Min) / (Max - Min)).ToCultureInvariantString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setRelativeValue(double _value)
	{
		double value = Value;
		Value = (Max - Min) * _value + Min;
		TriggerValueChangedEvent(value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void incrementalChangeValue(double _value)
	{
		double num = (Max - Min) * _value * 0.5;
		if (num > IncrementSize)
		{
			num = IncrementSize;
		}
		else if (num < 0.0 - IncrementSize)
		{
			num = 0.0 - IncrementSize;
		}
		double value = Value;
		double num2 = Value + num;
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool handleSegmentedFillValueBinding(ref string _value, int _index)
	{
		double num = (Max - Min) / (double)base.SegmentedFillCount;
		double num2 = (double)_index * num;
		double num3 = (double)(_index + 1) * num;
		if (currentValue <= num2)
		{
			_value = "0";
		}
		else if (currentValue >= num3)
		{
			_value = "1";
		}
		else
		{
			_value = ((currentValue - num2) / num).ToCultureInvariantString();
		}
		return true;
	}
}
