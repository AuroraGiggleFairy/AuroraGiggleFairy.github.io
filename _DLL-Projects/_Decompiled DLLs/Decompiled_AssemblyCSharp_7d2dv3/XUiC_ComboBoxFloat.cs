using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxFloat : XUiC_ComboBoxOrdinal<double>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public double incrementSize = 1.0;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public override long ValueGeneric { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public override long ValueMinGeneric { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public override long ValueMaxGeneric { get; set; }

	public override double RelativeValue
	{
		get
		{
			return (Value - Min) / (Max - Min);
		}
		set
		{
			Value = (Max - Min) * value + Min;
		}
	}

	public XUiC_ComboBoxFloat()
	{
		Max = 1.0;
		Min = 0.0;
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
			Min = StringParsers.ParseDouble(_value);
		}
	}

	[XuiXmlAttribute("value_max", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValueMax(string _value)
	{
		if (!_value.EqualsCaseInsensitive("@def"))
		{
			Max = StringParsers.ParseDouble(_value);
		}
	}

	[XuiXmlAttribute("value_increment", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValueIncrement(string _value)
	{
		if (!_value.EqualsCaseInsensitive("@def"))
		{
			incrementSize = StringParsers.ParseDouble(_value);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void incrementalChangeValue(double _value)
	{
		double num = (Max - Min) * _value * 0.5;
		if (num > incrementSize)
		{
			num = incrementSize;
		}
		else if (num < 0.0 - incrementSize)
		{
			num = 0.0 - incrementSize;
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
}
