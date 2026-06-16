using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxEnum<TEnum> : XUiC_ComboBox<TEnum> where TEnum : struct, IConvertible
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TEnum? min;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEnum? max;

	public readonly List<TEnum> Elements = new List<TEnum>();

	[XuiXmlAttribute("localization_prefix", false)]
	public string LocalizationPrefix { get; set; }

	public TEnum? Min
	{
		get
		{
			return min ?? EnumUtils.MinValue<TEnum>();
		}
		set
		{
			if (value.HasValue != min.HasValue || (value.HasValue && value.Value.Ordinal() != min.Value.Ordinal()))
			{
				min = value;
				if (min.HasValue && Value.Ordinal() < min.Value.Ordinal())
				{
					Value = min.Value;
				}
				UpdateIndexMarkerPositions();
				IsDirty = true;
			}
		}
	}

	public TEnum? Max
	{
		get
		{
			return max ?? EnumUtils.MaxValue<TEnum>();
		}
		set
		{
			if (value.HasValue != max.HasValue || (value.HasValue && value.Value.Ordinal() != max.Value.Ordinal()))
			{
				max = value;
				if (max.HasValue && Value.Ordinal() > max.Value.Ordinal())
				{
					Value = max.Value;
				}
				UpdateIndexMarkerPositions();
				IsDirty = true;
			}
		}
	}

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
				if (max.HasValue && value.Ordinal() > max.Value.Ordinal())
				{
					value = max.Value;
				}
				if (min.HasValue && value.Ordinal() < min.Value.Ordinal())
				{
					value = min.Value;
				}
				currentValue = value;
				IsDirty = true;
				UpdateLabel();
			}
		}
	}

	public int MinIndex
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!min.HasValue)
			{
				return 0;
			}
			return Elements.IndexOf(min.Value);
		}
	}

	public int MaxIndex
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!max.HasValue)
			{
				return Elements.Count - 1;
			}
			return Elements.IndexOf(max.Value);
		}
	}

	public override double RelativeValue
	{
		get
		{
			return ((double)(float)Elements.IndexOf(Value) + 0.5 - (double)MinIndex) / (double)(MaxIndex - MinIndex + 1);
		}
		set
		{
			if (UsesIndexMarkers)
			{
				int index = ((value >= 1.0) ? MaxIndex : ((!(value <= 0.0)) ? Mathf.RoundToInt((float)value * (float)(MaxIndex - MinIndex + 1) + (float)MinIndex - 0.5f) : MinIndex));
				Value = Elements[index];
			}
		}
	}

	public override long ValueGeneric
	{
		get
		{
			return Value.Ordinal();
		}
		set
		{
			if (EnumUtils.TryFromOrdinal<TEnum>((int)value, out var _result))
			{
				Value = _result;
			}
		}
	}

	public override long ValueMinGeneric
	{
		get
		{
			return Min.Value.Ordinal();
		}
		set
		{
			if (EnumUtils.TryFromOrdinal<TEnum>((int)value, out var _result))
			{
				Min = _result;
			}
		}
	}

	public override long ValueMaxGeneric
	{
		get
		{
			return Max.Value.Ordinal();
		}
		set
		{
			if (EnumUtils.TryFromOrdinal<TEnum>((int)value, out var _result))
			{
				Max = _result;
			}
		}
	}

	public override int IndexElementCount
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return Elements.Count;
		}
	}

	public override int IndexMarkerIndex
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return Elements.IndexOf(Value);
		}
	}

	public override void Init()
	{
		base.Init();
		if (Elements.Count == 0)
		{
			Elements.AddRange(EnumUtils.Values<TEnum>());
			if (Elements.Count > 0)
			{
				Value = Elements[0];
			}
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
		changeIndex(-1);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ForwardPressed()
	{
		changeIndex(1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void changeIndex(int _direction)
	{
		int num = Elements.IndexOf(Value);
		int num2 = num + _direction;
		if (num2 < 0 || num2 < MinIndex)
		{
			num2 = ((!Wrap) ? num : Elements.IndexOf(Max.Value));
		}
		if (num2 >= Elements.Count || num2 > MaxIndex)
		{
			num2 = ((!Wrap) ? num : Elements.IndexOf(Min.Value));
		}
		Value = Elements[num2];
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMax()
	{
		return currentValue.Ordinal() == Max.Value.Ordinal();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMin()
	{
		return currentValue.Ordinal() == Min.Value.Ordinal();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isEmpty()
	{
		return false;
	}

	[XuiXmlAttribute("values", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValues(string _value)
	{
		if (_value.EqualsCaseInsensitive("@def"))
		{
			return;
		}
		Elements.Clear();
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

	[XuiXmlAttribute("value_min", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValueMin(string _value)
	{
		if (!_value.EqualsCaseInsensitive("@def"))
		{
			Min = EnumUtils.Parse<TEnum>(_value);
		}
	}

	[XuiXmlAttribute("value_max", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValueMax(string _value)
	{
		if (!_value.EqualsCaseInsensitive("@def"))
		{
			Max = EnumUtils.Parse<TEnum>(_value);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void incrementalChangeValue(double _value)
	{
		if (_value > 0.0)
		{
			TryPageUp();
		}
		else if (_value < 0.0)
		{
			TryPageDown();
		}
	}
}
