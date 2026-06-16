using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxList<TElement> : XUiC_ComboBox<TElement>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string localizationPrefix;

	public readonly List<TElement> Elements = new List<TElement>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentIndex = int.MinValue;

	public TElement CustomValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useCustomValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int minIndex = int.MinValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxIndex = int.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool reverseList;

	[XuiXmlAttribute("localization_prefix", false)]
	public string LocalizationPrefix
	{
		get
		{
			return localizationPrefix;
		}
		set
		{
			localizationPrefix = value;
			UpdateLabel();
		}
	}

	[XuiXmlAttribute("localization_key_caseinsensitive", false)]
	public bool LocalizationKeyCaseInsensitive { get; set; }

	public override TElement Value
	{
		get
		{
			return currentValue;
		}
		set
		{
			if (Elements.Contains(value))
			{
				int selectedIndex = Elements.IndexOf(value);
				SelectedIndex = selectedIndex;
				useCustomValue = false;
			}
			else if (value != null)
			{
				CustomValue = value;
				useCustomValue = true;
				SelectedIndex = -1;
			}
		}
	}

	public override double RelativeValue
	{
		get
		{
			if (MaxIndex <= MinIndex)
			{
				return 0.0;
			}
			return ((double)(float)SelectedIndex + 0.5 - (double)MinIndex) / (double)(MaxIndex - MinIndex + 1);
		}
		set
		{
			if (UsesIndexMarkers)
			{
				if (value >= 1.0)
				{
					SelectedIndex = MaxIndex;
				}
				else if (value <= 0.0)
				{
					SelectedIndex = MinIndex;
				}
				else
				{
					SelectedIndex = Mathf.RoundToInt((float)value * (float)(MaxIndex - MinIndex + 1) + (float)MinIndex - 0.5f);
				}
			}
		}
	}

	public override long ValueGeneric
	{
		get
		{
			return SelectedIndex;
		}
		set
		{
			SelectedIndex = (int)value;
		}
	}

	public override long ValueMinGeneric
	{
		get
		{
			return MinIndex;
		}
		set
		{
			MinIndex = (int)value;
		}
	}

	public override long ValueMaxGeneric
	{
		get
		{
			return MaxIndex;
		}
		set
		{
			MaxIndex = (int)value;
		}
	}

	public override int IndexElementCount
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return MaxIndex - MinIndex + 1;
		}
	}

	public override int IndexMarkerIndex
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (!reverseList)
			{
				return SelectedIndex - MinIndex;
			}
			return MaxIndex - SelectedIndex;
		}
	}

	public int SelectedIndex
	{
		get
		{
			return currentIndex;
		}
		set
		{
			if (value < 0)
			{
				if (useCustomValue)
				{
					currentIndex = value;
					currentValue = CustomValue;
				}
				else
				{
					currentIndex = MinIndex;
					currentValue = ((currentIndex < 0 || currentIndex >= Elements.Count) ? default(TElement) : Elements[currentIndex]);
				}
			}
			else
			{
				if (value >= Elements.Count)
				{
					value = Elements.Count - 1;
				}
				if (value < MinIndex)
				{
					value = MinIndex;
				}
				else if (value > MaxIndex)
				{
					value = MaxIndex;
				}
				currentIndex = value;
				currentValue = ((currentIndex < 0 || currentIndex >= Elements.Count) ? default(TElement) : Elements[currentIndex]);
			}
			IsDirty = true;
			UpdateLabel();
		}
	}

	[XuiXmlAttribute("index_min", false)]
	public int MinIndex
	{
		get
		{
			if (minIndex >= 0)
			{
				return minIndex;
			}
			return 0;
		}
		set
		{
			if (value != minIndex)
			{
				minIndex = value;
				if (SelectedIndex < minIndex)
				{
					SelectedIndex = minIndex;
				}
				UpdateIndexMarkerPositions();
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("index_max", false)]
	public int MaxIndex
	{
		get
		{
			if (maxIndex < Elements.Count && maxIndex >= 0)
			{
				return maxIndex;
			}
			return Elements.Count - 1;
		}
		set
		{
			if (value != maxIndex)
			{
				maxIndex = value;
				if (maxIndex >= 0 && SelectedIndex > maxIndex)
				{
					SelectedIndex = maxIndex;
				}
				UpdateIndexMarkerPositions();
				IsDirty = true;
			}
		}
	}

	public override void OnOpen()
	{
		if (Elements.Count > 0 && !useCustomValue)
		{
			if (SelectedIndex < 0 && !useCustomValue)
			{
				SelectedIndex = 0;
			}
			if (SelectedIndex > Elements.Count)
			{
				SelectedIndex = Elements.Count - 1;
			}
			if (SelectedIndex < MinIndex)
			{
				SelectedIndex = MinIndex;
			}
			if (SelectedIndex > MaxIndex)
			{
				SelectedIndex = MaxIndex;
			}
			if (!useCustomValue)
			{
				currentValue = ((SelectedIndex < Elements.Count) ? Elements[SelectedIndex] : default(TElement));
			}
		}
		base.OnOpen();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateLabel()
	{
		if (isEmpty())
		{
			base.ValueText = "";
			return;
		}
		TElement val = currentValue;
		string text = ((val == null) ? "" : val.ToString());
		base.ValueText = ((!string.IsNullOrEmpty(LocalizationPrefix)) ? Localization.Get(LocalizationPrefix + text, LocalizationKeyCaseInsensitive) : text);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDifferentValue(TElement _oldVal, TElement _currentValue)
	{
		if (_oldVal == null && _currentValue == null)
		{
			return false;
		}
		if (_oldVal == null || _currentValue == null)
		{
			return true;
		}
		return !_oldVal.Equals(_currentValue);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void BackPressed()
	{
		ChangeIndex(reverseList ? 1 : (-1));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ForwardPressed()
	{
		ChangeIndex((!reverseList) ? 1 : (-1));
	}

	public void ChangeIndex(int _direction)
	{
		int num = SelectedIndex + _direction;
		if (num < MinIndex)
		{
			num = (Wrap ? Utils.FastMin(Elements.Count - 1, MaxIndex) : MinIndex);
		}
		if (num > MaxIndex)
		{
			num = (Wrap ? Utils.FastMax(0, MinIndex) : MaxIndex);
		}
		if (num < 0)
		{
			num = (Wrap ? (Elements.Count - 1) : 0);
		}
		if (num >= Elements.Count)
		{
			num = ((!Wrap) ? (Elements.Count - 1) : 0);
		}
		SelectedIndex = num;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMax()
	{
		if (!reverseList)
		{
			return isMaxIndex();
		}
		return isMinIndex();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMin()
	{
		if (!reverseList)
		{
			return isMinIndex();
		}
		return isMaxIndex();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isEmpty()
	{
		return Elements.Count == 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isMaxIndex()
	{
		if (SelectedIndex != MaxIndex)
		{
			return SelectedIndex == Elements.Count - 1;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isMinIndex()
	{
		if (SelectedIndex != MinIndex)
		{
			return SelectedIndex == 0;
		}
		return true;
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

	[XuiXmlAttribute("values", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValues(string _value)
	{
		if (_value.EqualsCaseInsensitive("@def"))
		{
			return;
		}
		string[] array = _value.Split(',');
		Type typeFromHandle = typeof(TElement);
		string[] array2;
		if (typeFromHandle == typeof(string))
		{
			array2 = array;
			foreach (string text in array2)
			{
				Elements.Add((TElement)(object)text.Trim());
			}
			return;
		}
		if (typeof(IConvertible).IsAssignableFrom(typeFromHandle))
		{
			array2 = array;
			foreach (string text2 in array2)
			{
				try
				{
					Elements.Add((TElement)Convert.ChangeType(text2, typeFromHandle));
				}
				catch (Exception e)
				{
					Log.Error($"[XUi] Value \"{text2}\" not supported for the ComboBox type {typeFromHandle}");
					Log.Exception(e);
				}
			}
			return;
		}
		ConstructorInfo constructor = typeFromHandle.GetConstructor(new Type[1] { typeof(string) });
		if (constructor == null)
		{
			Log.Error($"[XUi] ComboBox type {typeFromHandle} does not support values from string elements");
			return;
		}
		array2 = array;
		foreach (string text3 in array2)
		{
			object[] array3 = new object[1];
			try
			{
				array3[0] = text3;
				Elements.Add((TElement)constructor.Invoke(array3));
			}
			catch (Exception e2)
			{
				Log.Error($"[XUi] Value \"{text3}\" not supported for the ComboBox type {typeFromHandle}");
				Log.Exception(e2);
			}
		}
	}

	[XuiXmlAttribute("reverse_list", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeReverseList(string _value)
	{
		if (!_value.EqualsCaseInsensitive("@def"))
		{
			reverseList = StringParsers.ParseBool(_value);
		}
	}
}
