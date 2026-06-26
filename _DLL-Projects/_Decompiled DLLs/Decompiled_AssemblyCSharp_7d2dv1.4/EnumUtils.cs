using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

public static class EnumUtils
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class EnumInfoCache<TEnum> where TEnum : struct, IConvertible
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static EnumInfoCache<TEnum> instance;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<TEnum> enumValues;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<string> enumNames;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<TEnum, string> enumToName;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, TEnum> nameToEnumCaseSensitive;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, TEnum> nameToEnumCaseInsensitive;

		public readonly ReadOnlyCollection<TEnum> EnumValues;

		public readonly ReadOnlyCollection<string> EnumNames;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool isFlags;

		public static EnumInfoCache<TEnum> Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new EnumInfoCache<TEnum>();
				}
				return instance;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public EnumInfoCache()
		{
			if (!typeof(TEnum).IsEnum)
			{
				throw new NotSupportedException(typeof(TEnum).FullName + " is not an enum type.");
			}
			object[] customAttributes = typeof(TEnum).GetCustomAttributes(inherit: false);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				if (((Attribute)customAttributes[i]).GetType().Name == "FlagsAttribute")
				{
					isFlags = true;
					break;
				}
			}
			Array values = Enum.GetValues(typeof(TEnum));
			enumValues = new List<TEnum>(values.Length);
			enumToName = new EnumDictionary<TEnum, string>(values.Length);
			foreach (TEnum item in values)
			{
				string value = item.ToString(CultureInfo.InvariantCulture);
				if (!enumValues.Contains(item))
				{
					enumValues.Add(item);
				}
				if (!enumToName.ContainsKey(item))
				{
					enumToName.Add(item, value);
				}
			}
			enumValues.Sort([PublicizedFrom(EAccessModifier.Internal)] (TEnum _enumA, TEnum _enumB) => _enumA.Ordinal().CompareTo(_enumB.Ordinal()));
			string[] names = Enum.GetNames(typeof(TEnum));
			enumNames = new List<string>(names.Length);
			nameToEnumCaseSensitive = new Dictionary<string, TEnum>(names.Length, StringComparer.Ordinal);
			nameToEnumCaseInsensitive = new CaseInsensitiveStringDictionary<TEnum>(names.Length);
			string[] array = names;
			foreach (string text in array)
			{
				TEnum value2 = (TEnum)Enum.Parse(typeof(TEnum), text);
				if (!enumNames.Contains(text))
				{
					enumNames.Add(text);
				}
				if (!nameToEnumCaseSensitive.ContainsKey(text))
				{
					nameToEnumCaseSensitive.Add(text, value2);
				}
				if (!nameToEnumCaseInsensitive.ContainsKey(text))
				{
					nameToEnumCaseInsensitive.Add(text, value2);
				}
			}
			EnumValues = new ReadOnlyCollection<TEnum>(enumValues);
			EnumNames = new ReadOnlyCollection<string>(enumNames);
		}

		public string GetName(TEnum _enumValue)
		{
			if (!isFlags)
			{
				if (enumToName.ContainsKey(_enumValue))
				{
					return enumToName[_enumValue];
				}
				return _enumValue.ToString(CultureInfo.InvariantCulture);
			}
			if (!enumToName.ContainsKey(_enumValue))
			{
				enumToName.Add(_enumValue, _enumValue.ToString(CultureInfo.InvariantCulture));
			}
			return enumToName[_enumValue];
		}

		public TEnum Parse(string _name, bool _ignoreCase)
		{
			if (string.IsNullOrEmpty(_name))
			{
				throw new ArgumentException("Value null or empty", "_name");
			}
			if ((_ignoreCase ? nameToEnumCaseInsensitive : nameToEnumCaseSensitive).TryGetValue(_name, out var value))
			{
				return value;
			}
			TEnum val = (TEnum)Enum.Parse(typeof(TEnum), _name, _ignoreCase);
			nameToEnumCaseSensitive.Add(_name, val);
			nameToEnumCaseInsensitive.Add(_name, val);
			return val;
		}

		public bool TryParse(string _name, out TEnum _result, bool _ignoreCase)
		{
			_result = default(TEnum);
			if (string.IsNullOrEmpty(_name))
			{
				return false;
			}
			if ((_ignoreCase ? nameToEnumCaseInsensitive : nameToEnumCaseSensitive).TryGetValue(_name, out _result))
			{
				return true;
			}
			try
			{
				_result = (TEnum)Enum.Parse(typeof(TEnum), _name, _ignoreCase);
				nameToEnumCaseSensitive.Add(_name, _result);
				nameToEnumCaseInsensitive.Add(_name, _result);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public bool HasName(string _name, bool _ignoreCase)
		{
			if (string.IsNullOrEmpty(_name))
			{
				throw new ArgumentException("Value null or empty", "_name");
			}
			return (_ignoreCase ? nameToEnumCaseInsensitive : nameToEnumCaseSensitive).ContainsKey(_name);
		}
	}

	public static string ToStringCached<TEnum>(this TEnum _enumValue) where TEnum : struct, IConvertible
	{
		return EnumInfoCache<TEnum>.Instance.GetName(_enumValue);
	}

	public static int Ordinal<TEnum>(this TEnum _enumValue) where TEnum : struct, IConvertible
	{
		return EnumInt32ToInt.Convert(_enumValue);
	}

	public static TEnum Parse<TEnum>(string _name, TEnum _default, bool _ignoreCase = false) where TEnum : struct, IConvertible
	{
		if (!TryParse<TEnum>(_name, out var _result, _ignoreCase))
		{
			return _default;
		}
		return _result;
	}

	public static TEnum Parse<TEnum>(string _name, bool _ignoreCase = false) where TEnum : struct, IConvertible
	{
		return EnumInfoCache<TEnum>.Instance.Parse(_name, _ignoreCase);
	}

	public static bool TryParse<TEnum>(string _name, out TEnum _result, bool _ignoreCase = false) where TEnum : struct, IConvertible
	{
		return EnumInfoCache<TEnum>.Instance.TryParse(_name, out _result, _ignoreCase);
	}

	public static bool HasName<TEnum>(string _name, bool _ignoreCase = false) where TEnum : struct, IConvertible
	{
		return EnumInfoCache<TEnum>.Instance.HasName(_name, _ignoreCase);
	}

	public static IList<TEnum> Values<TEnum>() where TEnum : struct, IConvertible
	{
		return EnumInfoCache<TEnum>.Instance.EnumValues;
	}

	public static IList<string> Names<TEnum>() where TEnum : struct, IConvertible
	{
		return EnumInfoCache<TEnum>.Instance.EnumNames;
	}

	public static TEnum CycleEnum<TEnum>(this TEnum _enumVal, bool _reverse = false, bool _wrap = true) where TEnum : struct, IConvertible
	{
		if (!typeof(TEnum).IsEnum)
		{
			throw new ArgumentException("Argument " + typeof(TEnum).FullName + " is not an Enum");
		}
		IList<TEnum> enumValues = EnumInfoCache<TEnum>.Instance.EnumValues;
		int num = enumValues.IndexOf(_enumVal) + ((!_reverse) ? 1 : (-1));
		if (num >= enumValues.Count)
		{
			num = ((!_wrap) ? (enumValues.Count - 1) : 0);
		}
		else if (num < 0)
		{
			num = (_wrap ? (enumValues.Count - 1) : 0);
		}
		return enumValues[num];
	}

	public static TEnum CycleEnum<TEnum>(this TEnum _enumVal, TEnum _minVal, TEnum _maxVal, bool _reverse = false, bool _wrap = true) where TEnum : struct, IConvertible
	{
		if (!typeof(TEnum).IsEnum)
		{
			throw new ArgumentException("Argument " + typeof(TEnum).FullName + " is not an Enum");
		}
		IList<TEnum> enumValues = EnumInfoCache<TEnum>.Instance.EnumValues;
		int num = enumValues.IndexOf(_minVal);
		if (num < 0)
		{
			throw new ArgumentException($"Could not find index of {_minVal}", "_minVal");
		}
		int num2 = enumValues.IndexOf(_maxVal);
		if (num2 < 0)
		{
			throw new ArgumentException($"Could not find index of {_maxVal}", "_maxVal");
		}
		if (num2 < num)
		{
			throw new ArgumentException($"Max of {_maxVal} with index {num2} is less than min of {_minVal} with index {num}");
		}
		int num3 = enumValues.IndexOf(_enumVal);
		if (num3 < 0)
		{
			Log.Warning(string.Format("Could not find index of {0}: {1} (using min)", "_enumVal", _enumVal));
			return enumValues[num];
		}
		int num4 = num2 - num + 1;
		if (num4 <= 1)
		{
			return enumValues[num];
		}
		int num5 = num3 - num + ((!_reverse) ? 1 : (-1));
		if (_wrap)
		{
			num5 %= num4;
			if (num5 < 0)
			{
				num5 += num4;
			}
		}
		else if (num5 < 0)
		{
			num5 = 0;
		}
		else if (num5 >= num4)
		{
			num5 = num4 - 1;
		}
		int index = num + num5;
		return enumValues[index];
	}

	public static TEnum MaxValue<TEnum>() where TEnum : struct, IConvertible
	{
		return EnumInfoCache<TEnum>.Instance.EnumValues[EnumInfoCache<TEnum>.Instance.EnumValues.Count - 1];
	}

	public static TEnum MinValue<TEnum>() where TEnum : struct, IConvertible
	{
		return EnumInfoCache<TEnum>.Instance.EnumValues[0];
	}
}
