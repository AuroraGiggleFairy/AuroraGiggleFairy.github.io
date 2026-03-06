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
		public readonly string DisplayName;

		public GameOptionValue(EOptionValueType _type, string _displayName)
		{
			Type = _type;
			IntValue = -1;
			StringValue = null;
			DisplayName = _displayName;
		}

		public GameOptionValue(int _intValue, string _displayName)
		{
			Type = EOptionValueType.Int;
			IntValue = _intValue;
			StringValue = null;
			DisplayName = _displayName;
		}

		public GameOptionValue(string _stringValue, string _displayName)
		{
			Type = EOptionValueType.String;
			IntValue = -1;
			StringValue = _stringValue;
			DisplayName = _displayName;
		}

		public override string ToString()
		{
			return DisplayName;
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
			return ((((((int)Type * 397) ^ IntValue) * 397) ^ ((StringValue != null) ? StringValue.GetHashCode() : 0)) * 397) ^ ((DisplayName != null) ? DisplayName.GetHashCode() : 0);
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
	public bool allowCustom = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool forceCustom;

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameOptionValue defaultOptionValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultString;

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

	public bool isCustomRange
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
				SetupOptions();
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
				SetupOptions();
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
				SetupOptions();
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
		SetupOptions();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (forceCustom)
		{
			controlCombo.ShowButtons = false;
		}
		IsDirty = true;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "allow_any":
			SetAllowAny(StringParsers.ParseBool(_value));
			return true;
		case "allow_custom":
			allowCustom = forceCustom || StringParsers.ParseBool(_value);
			return true;
		case "force_custom":
			forceCustom = StringParsers.ParseBool(_value);
			if (forceCustom)
			{
				allowCustom = true;
			}
			return true;
		case "default":
			defaultValue = _value;
			return true;
		case "default_min":
			defaultMin = _value;
			return true;
		case "default_max":
			defaultMax = _value;
			return true;
		case "default_string":
			defaultString = _value;
			return true;
		case "values":
			if (_value.Length > 0)
			{
				valuesFromXml = _value.Split(',');
				for (int j = 0; j < valuesFromXml.Length; j++)
				{
					valuesFromXml[j] = valuesFromXml[j].Trim();
				}
			}
			return true;
		case "display_names":
			if (_value.Length > 0)
			{
				namesFromXml = _value.Split(',');
				for (int i = 0; i < namesFromXml.Length; i++)
				{
					namesFromXml[i] = namesFromXml[i].Trim();
				}
			}
			return true;
		case "value_localization_prefix":
			if (_value.Length > 0)
			{
				valueLocalizationPrefixFromXml = _value.Trim();
			}
			return true;
		default:
			return base.ParseAttribute(_name, _value, _parent);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetAllowAny(bool _value)
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "iscustomrange":
			_value = (valueType != GamePrefs.EnumType.String && (forceCustom || isCustomRange)).ToString();
			return true;
		case "iscustomstring":
			_value = (valueType == GamePrefs.EnumType.String && (forceCustom || isCustomRange)).ToString();
			return true;
		case "useCombo":
			_value = (!forceCustom).ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
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
	public string GetGameInfoName()
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
	public void SetupOptions()
	{
		controlCombo.Elements.Clear();
		controlCombo.Elements.Add(new GameOptionValue(EOptionValueType.Custom, ""));
		if (allowAny)
		{
			controlCombo.Elements.Add(new GameOptionValue(EOptionValueType.Any, Localization.Get("goAnyValue")));
		}
		if (!forceCustom)
		{
			int[] array = null;
			string[] array2 = null;
			string[] array3;
			switch (valueType)
			{
			case GamePrefs.EnumType.Int:
				if (!string.IsNullOrEmpty(defaultValue))
				{
					defaultOptionValue = new GameOptionValue(StringParsers.ParseSInt32(defaultValue), "");
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
						throw new Exception("Illegal option setup for " + GetGameInfoName() + " (no values and no names specified)");
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
				if (!string.IsNullOrEmpty(defaultValue))
				{
					defaultOptionValue = new GameOptionValue(StringParsers.ParseBool(defaultValue) ? 1 : 0, "");
				}
				array = new int[2] { 0, 1 };
				array3 = new string[2]
				{
					Localization.Get("goOff"),
					Localization.Get("goOn")
				};
				break;
			case GamePrefs.EnumType.String:
				if (!string.IsNullOrEmpty(defaultValue))
				{
					defaultOptionValue = new GameOptionValue(defaultValue, "");
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
				throw new Exception("Illegal option setup for " + GetGameInfoName() + " (names null)");
			}
			if (valueType != GamePrefs.EnumType.String)
			{
				if (array == null)
				{
					throw new Exception("Illegal option setup for " + GetGameInfoName() + " (values still null)");
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
					throw new Exception("Illegal option setup for " + GetGameInfoName() + " (values still null)");
				}
				for (int num3 = 0; num3 < array2.Length; num3++)
				{
					controlCombo.Elements.Add(new GameOptionValue(array2[num3], array3[num3]));
				}
			}
		}
		controlCombo.MinIndex = (((valueType != GamePrefs.EnumType.Int && valueType != GamePrefs.EnumType.String) || !allowCustom) ? 1 : 0);
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
		if (!forceCustom && defaultOptionValue.Type != EOptionValueType.Null)
		{
			controlCombo.SelectedIndex = controlCombo.Elements.IndexOf(defaultOptionValue);
		}
		else
		{
			controlCombo.SelectedIndex = ((!forceCustom) ? 1 : 0);
		}
		txtValueMin.Text = defaultMin ?? "";
		txtValueMax.Text = defaultMax ?? "";
		txtValueString.Text = defaultString ?? "";
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
	public XUiC_ServersList.UiServerFilter GetValueRangeFilter(string filterName, int? filterMin, int? filterMax)
	{
		if (valueRangeMin.HasValue && (!filterMin.HasValue || filterMin < valueRangeMin.Value))
		{
			filterMin = valueRangeMin.Value;
		}
		if (valueRangeMax.HasValue && (!filterMax.HasValue || filterMax > valueRangeMax.Value))
		{
			filterMax = valueRangeMax.Value;
		}
		Func<XUiC_ServersList.ListEntry, bool> func;
		IServerListInterface.ServerFilter.EServerFilterType type;
		if (filterMin.HasValue)
		{
			if (filterMax.HasValue)
			{
				if (filterMin.Value == filterMax.Value)
				{
					func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(GameInfoInt) == filterMin;
					type = IServerListInterface.ServerFilter.EServerFilterType.IntValue;
				}
				else
				{
					func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) =>
					{
						int value = _entry.gameServerInfo.GetValue(GameInfoInt);
						return value >= filterMin && value <= filterMax;
					};
					type = IServerListInterface.ServerFilter.EServerFilterType.IntRange;
				}
			}
			else
			{
				func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(GameInfoInt) >= filterMin;
				type = IServerListInterface.ServerFilter.EServerFilterType.IntMin;
			}
		}
		else if (filterMax.HasValue)
		{
			func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(GameInfoInt) <= filterMax;
			type = IServerListInterface.ServerFilter.EServerFilterType.IntMax;
		}
		else
		{
			func = null;
			type = IServerListInterface.ServerFilter.EServerFilterType.Any;
		}
		return new XUiC_ServersList.UiServerFilter(filterName, XUiC_ServersList.EnumServerLists.Regular, func, type, filterMin.GetValueOrDefault(), filterMax.GetValueOrDefault());
	}

	public XUiC_ServersList.UiServerFilter GetFilter()
	{
		string gameInfoName = GetGameInfoName();
		if (controlCombo.Value.Type == EOptionValueType.Any)
		{
			if (valueType != GamePrefs.EnumType.String)
			{
				return GetValueRangeFilter(gameInfoName, null, null);
			}
			return new XUiC_ServersList.UiServerFilter(gameInfoName, XUiC_ServersList.EnumServerLists.Regular);
		}
		if (!isCustomRange)
		{
			switch (valueType)
			{
			case GamePrefs.EnumType.Bool:
			{
				bool bValue = controlCombo.Value.IntValue == 1;
				Func<XUiC_ServersList.ListEntry, bool> func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(GameInfoBool) == bValue;
				IServerListInterface.ServerFilter.EServerFilterType type = IServerListInterface.ServerFilter.EServerFilterType.BoolValue;
				return new XUiC_ServersList.UiServerFilter(gameInfoName, XUiC_ServersList.EnumServerLists.Regular, func, type, 0, 0, bValue);
			}
			case GamePrefs.EnumType.Int:
			{
				int intValue = controlCombo.Value.IntValue;
				return GetValueRangeFilter(gameInfoName, intValue, intValue);
			}
			case GamePrefs.EnumType.String:
			{
				string sValue = controlCombo.Value.StringValue;
				Func<XUiC_ServersList.ListEntry, bool> func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(GameInfoString).EqualsCaseInsensitive(sValue);
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
			return GetValueRangeFilter(gameInfoName, filterMin, filterMax);
		}
		string sValue2 = txtValueString.Text.Trim();
		if (sValue2.Length == 0)
		{
			return new XUiC_ServersList.UiServerFilter(gameInfoName, XUiC_ServersList.EnumServerLists.Regular);
		}
		Func<XUiC_ServersList.ListEntry, bool> func2 = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(GameInfoString).ContainsCaseInsensitive(sValue2);
		IServerListInterface.ServerFilter.EServerFilterType type2 = IServerListInterface.ServerFilter.EServerFilterType.StringContains;
		return new XUiC_ServersList.UiServerFilter(gameInfoName, XUiC_ServersList.EnumServerLists.Regular, func2, type2, 0, 0, _boolValue: false, txtValueString.Text);
	}
}
