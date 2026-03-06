using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxList<TElement> : XUiC_ComboBox<TElement>
{
	public string LocalizationPrefix;

	public bool LocalizationKeyCaseInsensitive;

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
	public bool ReverseList;

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
			if (!ReverseList)
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
					currentIndex = minIndex;
					currentValue = Elements[currentIndex];
				}
			}
			else
			{
				if (value >= Elements.Count)
				{
					value = Elements.Count - 1;
				}
				if (value < minIndex)
				{
					value = minIndex;
				}
				else if (value > maxIndex)
				{
					value = maxIndex;
				}
				currentIndex = value;
				currentValue = Elements[currentIndex];
			}
			IsDirty = true;
			UpdateLabel();
		}
	}

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
				if (currentIndex < minIndex)
				{
					SelectedIndex = minIndex;
				}
				UpdateIndexMarkerPositions();
				IsDirty = true;
			}
		}
	}

	public int MaxIndex
	{
		get
		{
			if (maxIndex < Elements.Count)
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
				if (currentIndex > maxIndex)
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
			if (currentIndex < 0)
			{
				SelectedIndex = 0;
			}
			if (currentIndex > Elements.Count)
			{
				SelectedIndex = Elements.Count - 1;
			}
			if (currentIndex < minIndex)
			{
				SelectedIndex = minIndex;
			}
			if (currentIndex > maxIndex)
			{
				SelectedIndex = maxIndex;
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
		}
		else
		{
			base.ValueText = ((!string.IsNullOrEmpty(LocalizationPrefix)) ? Localization.Get(LocalizationPrefix + currentValue.ToString(), LocalizationKeyCaseInsensitive) : currentValue.ToString());
		}
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
		ChangeIndex(ReverseList ? 1 : (-1));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ForwardPressed()
	{
		ChangeIndex((!ReverseList) ? 1 : (-1));
	}

	public void ChangeIndex(int _direction)
	{
		int num = currentIndex + _direction;
		if (num < minIndex)
		{
			num = (Wrap ? Utils.FastMin(Elements.Count - 1, maxIndex) : minIndex);
		}
		if (num > maxIndex)
		{
			num = (Wrap ? Utils.FastMax(0, minIndex) : maxIndex);
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
		if (!ReverseList)
		{
			return isMaxIndex();
		}
		return isMinIndex();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMin()
	{
		if (!ReverseList)
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
		if (currentIndex != maxIndex)
		{
			return currentIndex == Elements.Count - 1;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isMinIndex()
	{
		if (currentIndex != minIndex)
		{
			return currentIndex == 0;
		}
		return true;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "values":
			if (!_value.EqualsCaseInsensitive("@def"))
			{
				string[] array = _value.Split(',');
				Type typeFromHandle = typeof(TElement);
				if (typeFromHandle == typeof(string))
				{
					string[] array2 = array;
					foreach (string text in array2)
					{
						Elements.Add((TElement)(object)text.Trim());
					}
				}
				else if (typeof(IConvertible).IsAssignableFrom(typeFromHandle))
				{
					string[] array2 = array;
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
				}
				else
				{
					ConstructorInfo constructor = typeFromHandle.GetConstructor(new Type[1] { typeof(string) });
					if (constructor == null)
					{
						Log.Error($"[XUi] ComboBox type {typeFromHandle} does not support values from string elements");
						return true;
					}
					string[] array2 = array;
					foreach (string text3 in array2)
					{
						string[] array3 = new string[1];
						try
						{
							array3[0] = text3;
							List<TElement> elements = Elements;
							object[] parameters = array3;
							elements.Add((TElement)constructor.Invoke(parameters));
						}
						catch (Exception e2)
						{
							Log.Error($"[XUi] Value \"{text3}\" not supported for the ComboBox type {typeFromHandle}");
							Log.Exception(e2);
						}
					}
				}
			}
			return true;
		case "reverse_list":
			if (!_value.EqualsCaseInsensitive("@def"))
			{
				ReverseList = StringParsers.ParseBool(_value);
			}
			return true;
		case "localization_prefix":
			LocalizationPrefix = _value;
			return true;
		case "localization_key_caseinsensitive":
			LocalizationKeyCaseInsensitive = StringParsers.ParseBool(_value);
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
}
