using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxInt : XUiC_ComboBoxOrdinal<long>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public long incrementSize = 1L;

	public override long ValueGeneric
	{
		get
		{
			return Value;
		}
		set
		{
			Value = value;
		}
	}

	public override long ValueMinGeneric
	{
		get
		{
			return Min;
		}
		set
		{
			Min = value;
		}
	}

	public override long ValueMaxGeneric
	{
		get
		{
			return Max;
		}
		set
		{
			Max = value;
		}
	}

	public override double RelativeValue
	{
		get
		{
			return (double)(Value - Min) / (double)(Max - Min);
		}
		set
		{
			Value = (long)((double)(Max - Min) * value) + Min;
		}
	}

	public XUiC_ComboBoxInt()
	{
		Max = long.MaxValue;
		Min = long.MinValue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void BackPressed()
	{
		currentValue -= incrementSize;
		base.BackPressed();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ForwardPressed()
	{
		currentValue += incrementSize;
		base.ForwardPressed();
	}

	[XuiXmlAttribute("value_min", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValueMin(string _value)
	{
		if (!_value.EqualsCaseInsensitive("@def"))
		{
			Min = StringParsers.ParseSInt64(_value);
		}
	}

	[XuiXmlAttribute("value_max", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValueMax(string _value)
	{
		if (!_value.EqualsCaseInsensitive("@def"))
		{
			Max = StringParsers.ParseSInt64(_value);
		}
	}

	[XuiXmlAttribute("value_increment", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValueIncrement(string _value)
	{
		if (!_value.EqualsCaseInsensitive("@def"))
		{
			incrementSize = StringParsers.ParseSInt64(_value);
		}
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
		if (num > incrementSize)
		{
			num = incrementSize;
		}
		else if (num < -incrementSize)
		{
			num = -incrementSize;
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
