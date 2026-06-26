using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxEnum<TEnum> : XUiC_ComboBox<TEnum> where TEnum : struct, IConvertible
{
	public string LocalizationPrefix;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool MinSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool MaxSet;

	public override TEnum Value
	{
		get
		{
			return currentValue;
		}
		set
		{
			if (currentValue.Ordinal() != value.Ordinal())
			{
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateLabel()
	{
		base.ValueText = ((!string.IsNullOrEmpty(LocalizationPrefix)) ? Localization.Get(LocalizationPrefix + currentValue.ToStringCached()) : currentValue.ToStringCached());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDifferentValue(TEnum _oldVal, TEnum _currentValue)
	{
		return _oldVal.Ordinal() != _currentValue.Ordinal();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void BackPressed()
	{
		if (MinSet && MaxSet)
		{
			currentValue = currentValue.CycleEnum(Min, Max, _reverse: true, Wrap);
		}
		else
		{
			currentValue = currentValue.CycleEnum(_reverse: true, Wrap);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ForwardPressed()
	{
		if (MinSet && MaxSet)
		{
			currentValue = currentValue.CycleEnum(Min, Max, _reverse: false, Wrap);
		}
		else
		{
			currentValue = currentValue.CycleEnum(_reverse: false, Wrap);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMax()
	{
		if (!MinSet || !MaxSet)
		{
			return currentValue.Ordinal() == EnumUtils.MaxValue<TEnum>().Ordinal();
		}
		return currentValue.Ordinal() == Max.Ordinal();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMin()
	{
		if (!MinSet || !MaxSet)
		{
			return currentValue.Ordinal() == EnumUtils.MinValue<TEnum>().Ordinal();
		}
		return currentValue.Ordinal() == Min.Ordinal();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isEmpty()
	{
		return false;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "value_min":
			if (!_value.EqualsCaseInsensitive("@def"))
			{
				Min = EnumUtils.Parse<TEnum>(_value);
				MinSet = true;
			}
			return true;
		case "value_max":
			if (!_value.EqualsCaseInsensitive("@def"))
			{
				Max = EnumUtils.Parse<TEnum>(_value);
				MaxSet = true;
			}
			return true;
		case "localization_prefix":
			LocalizationPrefix = _value;
			return true;
		default:
			return base.ParseAttribute(_name, _value, _parent);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setRelativeValue(double _value)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void incrementalChangeValue(double _value)
	{
		if (_value > 0.0)
		{
			ForwardButton_OnPress(this, -1);
		}
		else if (_value < 0.0)
		{
			BackButton_OnPress(this, -1);
		}
	}

	public void SetMinMax(TEnum _min, TEnum _max)
	{
		Min = _min;
		Max = _max;
		MinSet = true;
		MaxSet = true;
	}
}
