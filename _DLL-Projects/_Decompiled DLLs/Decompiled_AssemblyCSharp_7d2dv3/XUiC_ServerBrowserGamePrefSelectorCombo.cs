using System;
using System.Globalization;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowserGamePrefSelectorCombo : XUiController, IServerBrowserFilterControl
{
	public enum EOptionValueType
	{
		Null,
		Custom,
		Any,
		Int,
		String
	}

	public readonly struct GameOptionValue : IEquatable<GameOptionValue>
	{
		public readonly EOptionValueType Type;

		public readonly int IntValue;

		public readonly string StringValue;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string displayName;

		public GameOptionValue(EOptionValueType _type, string _displayName)
		{
			Type = _type;
			IntValue = -1;
			StringValue = null;
			displayName = _displayName;
		}

		public GameOptionValue(int _intValue, string _displayName)
		{
			Type = EOptionValueType.Int;
			IntValue = _intValue;
			StringValue = null;
			displayName = _displayName;
		}

		public GameOptionValue(string _stringValue, string _displayName)
		{
			Type = EOptionValueType.String;
			IntValue = -1;
			StringValue = _stringValue;
			displayName = _displayName;
		}

		public override string ToString()
		{
			return displayName;
		}

		public bool Equals(GameOptionValue _other)
		{
			if (Type != _other.Type)
			{
				return false;
			}
			switch (Type)
			{
			case EOptionValueType.Custom:
			case EOptionValueType.Any:
				return true;
			case EOptionValueType.Int:
				return IntValue == _other.IntValue;
			case EOptionValueType.String:
				return StringValue == _other.StringValue;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public override bool Equals(object _obj)
		{
			if (_obj is GameOptionValue other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((((((int)Type * 397) ^ IntValue) * 397) ^ ((StringValue != null) ? StringValue.GetHashCode() : 0)) * 397) ^ ((displayName != null) ? displayName.GetHashCode() : 0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<GameOptionValue> controlCombo;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtValueMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtValueMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtValueString;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool allowAny = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameOptionValue defaultOptionValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] valuesFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] namesFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public string valueLocalizationPrefixFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public int? valueRangeMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public int? valueRangeMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public GamePrefs.EnumType valueType;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<int, int> customValuePreFilterModifierFunc;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<int, int> valuePreDisplayModifierFunc;

	public Action<IServerBrowserFilterControl> OnValueChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled = true;

	[XuiXmlAttribute("allow_custom", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool AllowCustom
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = true;

	[XuiXmlAttribute("force_custom", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool ForceCustom
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("default", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string DefaultValue
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("default_min", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string DefaultMin
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("default_max", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string DefaultMax
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("default_string", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string DefaultString
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsCustomRange
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (controlCombo != null)
			{
				return controlCombo.Value.Type == EOptionValueType.Custom;
			}
			return false;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public GameInfoBool GameInfoBool
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = (GameInfoBool)(-1);

	[field: PublicizedFrom(EAccessModifier.Private)]
	public GameInfoInt GameInfoInt
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = (GameInfoInt)(-1);

	[field: PublicizedFrom(EAccessModifier.Private)]
	public GameInfoString GameInfoString
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = (GameInfoString)(-1);

	public int? ValueRangeMin
	{
		set
		{
			if (valueRangeMin != value)
			{
				valueRangeMin = value;
				setupOptions();
			}
		}
	}

	public int? ValueRangeMax
	{
		set
		{
			if (valueRangeMax != value)
			{
				valueRangeMax = value;
				setupOptions();
			}
		}
	}

	public Func<int, int> CustomValuePreFilterModifierFunc
	{
		set
		{
			customValuePreFilterModifierFunc = value;
		}
	}

	public Func<int, int> ValuePreDisplayModifierFunc
	{
		set
		{
			valuePreDisplayModifierFunc = value;
			if (valuePreDisplayModifierFunc != null)
			{
				setupOptions();
			}
		}
	}

	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			if (enabled != value)
			{
				enabled = value;
				controlCombo.Enabled = value;
			}
		}
	}

	[XuiXmlBinding("iscustomrange")]
	public bool BindingIsCustomRange
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (valueType != GamePrefs.EnumType.String)
			{
				if (!ForceCustom)
				{
					return IsCustomRange;
				}
				return true;
			}
			return false;
		}
	}

	[XuiXmlBinding("iscustomstring")]
	public bool IsCustomString
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (valueType == GamePrefs.EnumType.String)
			{
				if (!ForceCustom)
				{
					return IsCustomRange;
				}
				return true;
			}
			return false;
		}
	}

	[XuiXmlBinding("usecombo")]
	public bool UseCombo
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !ForceCustom;
		}
	}

	public override void Init()
	{
		base.Init();
		if (EnumUtils.TryParse<GameInfoBool>(viewComponent.ID, out var _result))
		{
			GameInfoBool = _result;
			valueType = GamePrefs.EnumType.Bool;
		}
		if (EnumUtils.TryParse<GameInfoInt>(viewComponent.ID, out var _result2))
		{
			GameInfoInt = _result2;
			valueType = GamePrefs.EnumType.Int;
		}
		if (EnumUtils.TryParse<GameInfoString>(viewComponent.ID, out var _result3))
		{
			GameInfoString = _result3;
			valueType = GamePrefs.EnumType.String;
		}
		controlCombo = GetChildById("ControlCombo").GetChildByType<XUiC_ComboBoxList<GameOptionValue>>();
		controlCombo.OnValueChanged += ControlCombo_OnValueChanged;
		txtValueMin = GetChildById("valuemin").GetChildByType<XUiC_TextInput>();
		txtValueMin.OnChangeHandler += ControlValue_OnChangeHandler;
		txtValueMin.UIInputController.OnScroll += controlCombo.ScrollEvent;
		txtValueMax = GetChildById("valuemax").GetChildByType<XUiC_TextInput>();
		txtValueMax.OnChangeHandler += ControlValue_OnChangeHandler;
		txtValueMax.UIInputController.OnScroll += controlCombo.ScrollEvent;
		txtValueString = GetChildById("valuestring").GetChildByType<XUiC_TextInput>();
		txtValueString.OnChangeHandler += ControlText_OnChangeHandler;
		txtValueString.UIInputController.OnScroll += controlCombo.ScrollEvent;
		txtValueMin.SelectOnTab = txtValueMax;
		txtValueMax.SelectOnTab = txtValueMin;
		setupOptions();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (ForceCustom)
		{
			controlCombo.ShowButtons = false;
		}
		IsDirty = true;
	}

	[XuiXmlAttribute("values", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValues(string _value)
	{
		if (_value.Length > 0)
		{
			valuesFromXml = _value.Split(',');
			for (int i = 0; i < valuesFromXml.Length; i++)
			{
				valuesFromXml[i] = valuesFromXml[i].Trim();
			}
		}
	}

	[XuiXmlAttribute("display_names", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeDisplayNames(string _value)
	{
		if (_value.Length > 0)
		{
			namesFromXml = _value.Split(',');
			for (int i = 0; i < namesFromXml.Length; i++)
			{
				namesFromXml[i] = namesFromXml[i].Trim();
			}
		}
	}

	[XuiXmlAttribute("value_localization_prefix", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValueLocalizationPrefix(string _value)
	{
		if (_value.Length > 0)
		{
			valueLocalizationPrefixFromXml = _value.Trim();
		}
	}

	[XuiXmlAttribute("allow_any", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void setAllowAny(bool _value)
	{
		if (_value == allowAny)
		{
			return;
		}
		allowAny = _value;
		if (controlCombo == null || controlCombo.Elements.Count == 0)
		{
			return;
		}
		if (_value)
		{
			if (!controlCombo.Elements.Contains(new GameOptionValue(EOptionValueType.Any, "")))
			{
				controlCombo.Elements.Insert(1, new GameOptionValue(EOptionValueType.Any, Localization.Get("goAnyValue")));
				if (controlCombo.SelectedIndex > 0)
				{
					controlCombo.SelectedIndex++;
				}
			}
		}
		else if (controlCombo.Elements.Remove(new GameOptionValue(EOptionValueType.Any, "")) && controlCombo.SelectedIndex > 1)
		{
			controlCombo.SelectedIndex--;
		}
		controlCombo.SelectedIndex = controlCombo.SelectedIndex;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			IsDirty = false;
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string getGameInfoName()
	{
		return valueType switch
		{
			GamePrefs.EnumType.Bool => GameInfoBool.ToStringCached(), 
			GamePrefs.EnumType.Int => GameInfoInt.ToStringCached(), 
			GamePrefs.EnumType.String => GameInfoString.ToStringCached(), 
			_ => null, 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupOptions()
	{
		controlCombo.Elements.Clear();
		controlCombo.Elements.Add(new GameOptionValue(EOptionValueType.Custom, ""));
		if (allowAny)
		{
			controlCombo.Elements.Add(new GameOptionValue(EOptionValueType.Any, Localization.Get("goAnyValue")));
		}
		if (!ForceCustom)
		{
			int[] array = null;
			string[] array2 = null;
			string[] array3;
			switch (valueType)
			{
			case GamePrefs.EnumType.Int:
				if (!string.IsNullOrEmpty(DefaultValue))
				{
					defaultOptionValue = new GameOptionValue(StringParsers.ParseSInt32(DefaultValue), "");
				}
				if (valuesFromXml != null)
				{
					array = new int[valuesFromXml.Length];
					for (int k = 0; k < valuesFromXml.Length; k++)
					{
						array[k] = StringParsers.ParseSInt32(valuesFromXml[k]);
					}
				}
				if (array == null)
				{
					if (namesFromXml == null)
					{
						throw new Exception("Illegal option setup for " + getGameInfoName() + " (no values and no names specified)");
					}
					array = new int[namesFromXml.Length];
					for (int l = 0; l < array.Length; l++)
					{
						array[l] = l;
					}
				}
				array3 = new string[array.Length];
				if (namesFromXml == null || namesFromXml.Length != array.Length)
				{
					for (int m = 0; m < array.Length; m++)
					{
						if (namesFromXml != null && m < namesFromXml.Length)
						{
							array3[m] = Localization.Get(namesFromXml[m]);
							continue;
						}
						int num = array[m];
						if (valuePreDisplayModifierFunc != null)
						{
							num = valuePreDisplayModifierFunc(num);
						}
						if (string.IsNullOrEmpty(valueLocalizationPrefixFromXml))
						{
							array3[m] = num.ToString();
						}
						else
						{
							array3[m] = string.Format(Localization.Get(valueLocalizationPrefixFromXml + ((num == 1) ? "" : "s")), num);
						}
					}
				}
				else
				{
					for (int n = 0; n < namesFromXml.Length; n++)
					{
						array3[n] = Localization.Get(namesFromXml[n]);
					}
				}
				break;
			case GamePrefs.EnumType.Bool:
				if (!string.IsNullOrEmpty(DefaultValue))
				{
					defaultOptionValue = new GameOptionValue(StringParsers.ParseBool(DefaultValue) ? 1 : 0, "");
				}
				array = new int[2] { 0, 1 };
				array3 = new string[2]
				{
					Localization.Get("goOff"),
					Localization.Get("goOn")
				};
				break;
			case GamePrefs.EnumType.String:
				if (!string.IsNullOrEmpty(DefaultValue))
				{
					defaultOptionValue = new GameOptionValue(DefaultValue, "");
				}
				array2 = valuesFromXml;
				array3 = new string[array2.Length];
				if (namesFromXml == null || namesFromXml.Length != array2.Length)
				{
					for (int i = 0; i < array2.Length; i++)
					{
						if (namesFromXml != null && i < namesFromXml.Length)
						{
							array3[i] = Localization.Get(namesFromXml[i]);
							continue;
						}
						string text = array2[i];
						if (string.IsNullOrEmpty(valueLocalizationPrefixFromXml))
						{
							array3[i] = text;
						}
						else
						{
							array3[i] = Localization.Get(valueLocalizationPrefixFromXml + text);
						}
					}
				}
				else
				{
					for (int j = 0; j < namesFromXml.Length; j++)
					{
						array3[j] = Localization.Get(namesFromXml[j]);
					}
				}
				break;
			default:
				throw new NotSupportedException("Not a valid GameInfoX: " + viewComponent.ID);
			}
			if (array3 == null)
			{
				throw new Exception("Illegal option setup for " + getGameInfoName() + " (names null)");
			}
			if (valueType != GamePrefs.EnumType.String)
			{
				if (array == null)
				{
					throw new Exception("Illegal option setup for " + getGameInfoName() + " (values still null)");
				}
				for (int num2 = 0; num2 < array.Length; num2++)
				{
					if ((!valueRangeMin.HasValue || array[num2] >= valueRangeMin.Value) && (!valueRangeMax.HasValue || array[num2] <= valueRangeMax.Value))
					{
						controlCombo.Elements.Add(new GameOptionValue(array[num2], array3[num2]));
					}
				}
			}
			if (valueType == GamePrefs.EnumType.String)
			{
				if (array2 == null)
				{
					throw new Exception("Illegal option setup for " + getGameInfoName() + " (values still null)");
				}
				for (int num3 = 0; num3 < array2.Length; num3++)
				{
					controlCombo.Elements.Add(new GameOptionValue(array2[num3], array3[num3]));
				}
			}
		}
		controlCombo.MinIndex = (((valueType != GamePrefs.EnumType.Int && valueType != GamePrefs.EnumType.String) || (!ForceCustom && !AllowCustom)) ? 1 : 0);
		setDefaultValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlCombo_OnValueChanged(XUiController _sender, GameOptionValue _oldValue, GameOptionValue _newValue)
	{
		IsDirty = true;
		OnValueChanged?.Invoke(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlValue_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (_sender is XUiC_TextInput xUiC_TextInput && !string.IsNullOrEmpty(_text))
		{
			ReadOnlySpan<char> readOnlySpan = _text.AsSpan().Trim();
			while (readOnlySpan.Length > 1 && readOnlySpan[0] == '0')
			{
				ReadOnlySpan<char> readOnlySpan2 = readOnlySpan;
				readOnlySpan = readOnlySpan2.Slice(1, readOnlySpan2.Length - 1);
			}
			int _result;
			int? num = (StringParsers.TryParseSInt32(new string(readOnlySpan), out _result) ? new int?(_result) : ((int?)null));
			if (num.HasValue)
			{
				if (valueRangeMin.HasValue && num.Value < valueRangeMin.Value)
				{
					num = valueRangeMin;
				}
				if (valueRangeMax.HasValue && num.Value > valueRangeMax.Value)
				{
					num = valueRangeMax;
				}
			}
			string text = (num.HasValue ? num.Value.ToString(CultureInfo.InvariantCulture) : "");
			if (text != _text)
			{
				xUiC_TextInput.Text = text;
			}
		}
		OnValueChanged?.Invoke(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlText_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		OnValueChanged?.Invoke(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setDefaultValues()
	{
		if (!ForceCustom && defaultOptionValue.Type != EOptionValueType.Null)
		{
			controlCombo.SelectedIndex = controlCombo.Elements.IndexOf(defaultOptionValue);
		}
		else
		{
			controlCombo.SelectedIndex = ((!ForceCustom) ? 1 : 0);
		}
		txtValueMin.Text = DefaultMin ?? "";
		txtValueMax.Text = DefaultMax ?? "";
		txtValueString.Text = DefaultString ?? "";
	}

	public void Reset()
	{
		setDefaultValues();
		IsDirty = true;
		OnValueChanged?.Invoke(this);
	}

	public void SelectEntry(GameOptionValue _value)
	{
		int num = controlCombo.Elements.IndexOf(_value);
		if (num >= 0)
		{
			controlCombo.SelectedIndex = num;
			OnValueChanged?.Invoke(this);
		}
	}

	public GameOptionValue GetSelection()
	{
		return controlCombo.Value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServersList.UiServerFilter getValueRangeFilter(string _filterName, int? _filterMin, int? _filterMax)
	{
		if (valueRangeMin.HasValue && (!_filterMin.HasValue || _filterMin < valueRangeMin.Value))
		{
			_filterMin = valueRangeMin.Value;
		}
		if (valueRangeMax.HasValue && (!_filterMax.HasValue || _filterMax > valueRangeMax.Value))
		{
			_filterMax = valueRangeMax.Value;
		}
		Func<XUiC_ServersList.ListEntry, bool> func;
		IServerListInterface.ServerFilter.EServerFilterType type;
		if (_filterMin.HasValue)
		{
			if (_filterMax.HasValue)
			{
				if (_filterMin.Value == _filterMax.Value)
				{
					func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.GameServerInfo.GetValue(GameInfoInt) == _filterMin;
					type = IServerListInterface.ServerFilter.EServerFilterType.IntValue;
				}
				else
				{
					func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) =>
					{
						int value = _entry.GameServerInfo.GetValue(GameInfoInt);
						return value >= _filterMin && value <= _filterMax;
					};
					type = IServerListInterface.ServerFilter.EServerFilterType.IntRange;
				}
			}
			else
			{
				func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.GameServerInfo.GetValue(GameInfoInt) >= _filterMin;
				type = IServerListInterface.ServerFilter.EServerFilterType.IntMin;
			}
		}
		else if (_filterMax.HasValue)
		{
			func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.GameServerInfo.GetValue(GameInfoInt) <= _filterMax;
			type = IServerListInterface.ServerFilter.EServerFilterType.IntMax;
		}
		else
		{
			func = null;
			type = IServerListInterface.ServerFilter.EServerFilterType.Any;
		}
		return new XUiC_ServersList.UiServerFilter(_filterName, XUiC_ServersList.EnumServerLists.Regular, func, type, _filterMin.GetValueOrDefault(), _filterMax.GetValueOrDefault());
	}

	public XUiC_ServersList.UiServerFilter GetFilter()
	{
		string gameInfoName = getGameInfoName();
		if (controlCombo.Value.Type == EOptionValueType.Any)
		{
			if (valueType != GamePrefs.EnumType.String)
			{
				return getValueRangeFilter(gameInfoName, null, null);
			}
			return new XUiC_ServersList.UiServerFilter(gameInfoName, XUiC_ServersList.EnumServerLists.Regular);
		}
		if (!IsCustomRange)
		{
			switch (valueType)
			{
			case GamePrefs.EnumType.Bool:
			{
				bool bValue = controlCombo.Value.IntValue == 1;
				Func<XUiC_ServersList.ListEntry, bool> func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.GameServerInfo.GetValue(GameInfoBool) == bValue;
				IServerListInterface.ServerFilter.EServerFilterType type = IServerListInterface.ServerFilter.EServerFilterType.BoolValue;
				return new XUiC_ServersList.UiServerFilter(gameInfoName, XUiC_ServersList.EnumServerLists.Regular, func, type, 0, 0, bValue);
			}
			case GamePrefs.EnumType.Int:
			{
				int intValue = controlCombo.Value.IntValue;
				return getValueRangeFilter(gameInfoName, intValue, intValue);
			}
			case GamePrefs.EnumType.String:
			{
				string sValue = controlCombo.Value.StringValue;
				Func<XUiC_ServersList.ListEntry, bool> func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.GameServerInfo.GetValue(GameInfoString).EqualsCaseInsensitive(sValue);
				IServerListInterface.ServerFilter.EServerFilterType type = IServerListInterface.ServerFilter.EServerFilterType.StringValue;
				return new XUiC_ServersList.UiServerFilter(gameInfoName, XUiC_ServersList.EnumServerLists.Regular, func, type, 0, 0, _boolValue: false, sValue);
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		if (valueType != GamePrefs.EnumType.String)
		{
			int _result;
			int? filterMin = (StringParsers.TryParseSInt32(txtValueMin.Text, out _result) ? new int?(_result) : ((int?)null));
			int _result2;
			int? filterMax = (StringParsers.TryParseSInt32(txtValueMax.Text, out _result2) ? new int?(_result2) : ((int?)null));
			if (filterMin.HasValue && customValuePreFilterModifierFunc != null)
			{
				filterMin = customValuePreFilterModifierFunc(filterMin.Value);
			}
			if (filterMax.HasValue && customValuePreFilterModifierFunc != null)
			{
				filterMax = customValuePreFilterModifierFunc(filterMax.Value);
			}
			return getValueRangeFilter(gameInfoName, filterMin, filterMax);
		}
		string sValue2 = txtValueString.Text.Trim();
		if (sValue2.Length == 0)
		{
			return new XUiC_ServersList.UiServerFilter(gameInfoName, XUiC_ServersList.EnumServerLists.Regular);
		}
		Func<XUiC_ServersList.ListEntry, bool> func2 = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.GameServerInfo.GetValue(GameInfoString).ContainsCaseInsensitive(sValue2);
		IServerListInterface.ServerFilter.EServerFilterType type2 = IServerListInterface.ServerFilter.EServerFilterType.StringContains;
		return new XUiC_ServersList.UiServerFilter(gameInfoName, XUiC_ServersList.EnumServerLists.Regular, func2, type2, 0, 0, _boolValue: false, txtValueString.Text);
	}
}
