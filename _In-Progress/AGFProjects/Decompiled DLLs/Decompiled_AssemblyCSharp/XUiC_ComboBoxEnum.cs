using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxEnum<TEnum> : XUiC_ComboBox<TEnum> where TEnum : struct, IConvertible
{
	public string LocalizationPrefix;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool MinSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool MaxSet;

	public List<TEnum> Elements;

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
		if (Elements != null)
		{
			changeIndex(-1);
		}
		else if (MinSet && MaxSet)
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
		if (Elements != null)
		{
			changeIndex(1);
		}
		else if (MinSet && MaxSet)
		{
			currentValue = currentValue.CycleEnum(Min, Max, _reverse: false, Wrap);
		}
		else
		{
			currentValue = currentValue.CycleEnum(_reverse: false, Wrap);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void changeIndex(int _direction)
	{
		int num = Elements.IndexOf(Value) + _direction;
		if (num < 0)
		{
			num = (Wrap ? (Elements.Count - 1) : 0);
		}
		if (num >= Elements.Count)
		{
			num = ((!Wrap) ? (Elements.Count - 1) : 0);
		}
		Value = Elements[num];
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMax()
	{
		if (Elements != null)
		{
			return Elements.IndexOf(Value) == Elements.Count - 1;
		}
		if (MinSet && MaxSet)
		{
			return currentValue.Ordinal() == Max.Ordinal();
		}
		return currentValue.Ordinal() == EnumUtils.MaxValue<TEnum>().Ordinal();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMin()
	{
		if (Elements != null)
		{
			return Elements.IndexOf(Value) == 0;
		}
		if (MinSet && MaxSet)
		{
			return currentValue.Ordinal() == Min.Ordinal();
		}
		return currentValue.Ordinal() == EnumUtils.MinValue<TEnum>().Ordinal();
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
		case "values":
			if (!_value.EqualsCaseInsensitive("@def"))
			{
				Elements = new List<TEnum>();
				string[] array = _value.Split(',');
				foreach (string text in array)
				{
					if (!EnumUtils.TryParse<TEnum>(text, out var _result, _ignoreCase: true))
					{
						Log.Error("[XUi] Value \"" + text + "\" is not a member of the " + typeof(TEnum).FullName + " enum.");
					}
					else
					{
						Elements.Add(_result);
					}
				}
				if (Elements.Count > 0)
				{
					Value = Elements[0];
				}
			}
			return true;
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
