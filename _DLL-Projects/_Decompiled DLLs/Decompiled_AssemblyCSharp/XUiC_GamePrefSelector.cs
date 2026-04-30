using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_GamePrefSelector : XUiController
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
	public XUiV_Label controlLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<GameOptionValue> controlCombo;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput controlText;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color enabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color disabledColor = new Color(0.625f, 0.625f, 0.625f);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool valuesFromGameServerConfig;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] valuesFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] namesFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public string valueLocalizationPrefixFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool valuesEnforced;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTextInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasDefault = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool alwaysShow;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumGamePrefs gamePref = EnumGamePrefs.Last;

	[PublicizedFrom(EAccessModifier.Private)]
	public GamePrefs.EnumType valueType;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameMode currentGameMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<int, int> valuePreDisplayModifierFunc;

	public Action<XUiC_GamePrefSelector, EnumGamePrefs> OnValueChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled = true;

	public EnumGamePrefs GamePref => gamePref;

	public XUiC_TextInput ControlText => controlText;

	public Func<int, int> ValuePreDisplayModifierFunc
	{
		get
		{
			return valuePreDisplayModifierFunc;
		}
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
				controlText.Enabled = value;
				controlText.ActiveTextColor = (value ? enabledColor : disabledColor);
				controlLabel.Color = (value ? enabledColor : disabledColor);
			}
		}
	}

	public override void Init()
	{
		base.Init();
		gamePref = EnumUtils.Parse<EnumGamePrefs>(viewComponent.ID);
		controlLabel = (XUiV_Label)GetChildById("ControlLabel").ViewComponent;
		enabledColor = controlLabel.Color;
		controlCombo = GetChildById("ControlCombo").GetChildByType<XUiC_ComboBoxList<GameOptionValue>>();
		controlCombo.OnValueChanged += ControlCombo_OnValueChanged;
		controlText = GetChildById("ControlText").GetChildByType<XUiC_TextInput>();
		controlText.OnChangeHandler += ControlText_OnChangeHandler;
		if (!isTextInput)
		{
			SetupOptions();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		controlCombo.ViewComponent.IsVisible = !isTextInput;
		controlText.ViewComponent.IsVisible = isTextInput;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "is_textinput":
			isTextInput = StringParsers.ParseBool(_value);
			return true;
		case "value_type":
			valueType = EnumUtils.Parse<GamePrefs.EnumType>(_value, _ignoreCase: true);
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
		case "values_enforced":
			valuesEnforced = StringParsers.ParseBool(_value);
			return true;
		case "has_default":
			hasDefault = StringParsers.ParseBool(_value);
			return true;
		case "always_show":
			alwaysShow = StringParsers.ParseBool(_value);
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
		case "values_from_gameserverconfig":
			valuesFromGameServerConfig = StringParsers.ParseBool(_value);
			return true;
		default:
			return base.ParseAttribute(_name, _value, _parent);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupOptions()
	{
		int[] array = null;
		string[] array2 = null;
		string[] array3;
		switch (valueType)
		{
		case GamePrefs.EnumType.Int:
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
					throw new Exception("Illegal option setup for " + gamePref.ToStringCached() + " (no values and no names specified)");
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
			array = new int[2] { 0, 1 };
			array3 = new string[2];
			if (namesFromXml != null && namesFromXml.Length == 2)
			{
				array3[0] = Localization.Get(namesFromXml[0]);
				array3[1] = Localization.Get(namesFromXml[1]);
			}
			else
			{
				array3[0] = Localization.Get("goOff");
				array3[1] = Localization.Get("goOn");
			}
			break;
		case GamePrefs.EnumType.String:
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
			throw new NotSupportedException("Not a valid GamePref: " + viewComponent.ID);
		}
		controlCombo.Elements.Clear();
		if (array3 == null)
		{
			throw new Exception("Illegal option setup for " + gamePref.ToStringCached() + " (names null)");
		}
		if (valueType != GamePrefs.EnumType.String)
		{
			if (array == null)
			{
				throw new Exception("Illegal option setup for " + gamePref.ToStringCached() + " (values still null)");
			}
			for (int num2 = 0; num2 < array.Length; num2++)
			{
				controlCombo.Elements.Add(new GameOptionValue(array[num2], array3[num2]));
			}
		}
		if (valueType == GamePrefs.EnumType.String)
		{
			if (array2 == null)
			{
				throw new Exception("Illegal option setup for " + gamePref.ToStringCached() + " (values still null)");
			}
			for (int num3 = 0; num3 < array2.Length; num3++)
			{
				controlCombo.Elements.Add(new GameOptionValue(array2[num3], array3[num3]));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlText_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		switch (valueType)
		{
		case GamePrefs.EnumType.Int:
		{
			if (int.TryParse(_text, out var result))
			{
				GamePrefs.Set(gamePref, result);
			}
			break;
		}
		case GamePrefs.EnumType.String:
			GamePrefs.Set(gamePref, _text);
			break;
		default:
			throw new Exception("Illegal option setup for " + gamePref.ToStringCached());
		}
		OnValueChanged?.Invoke(this, gamePref);
		CheckDefaultValue();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlCombo_OnValueChanged(XUiController _sender, GameOptionValue _oldValue, GameOptionValue _newValue)
	{
		switch (valueType)
		{
		case GamePrefs.EnumType.Int:
			GamePrefs.Set(gamePref, _newValue.IntValue);
			break;
		case GamePrefs.EnumType.Bool:
			GamePrefs.Set(gamePref, _newValue.IntValue == 1);
			break;
		case GamePrefs.EnumType.String:
			GamePrefs.Set(gamePref, _newValue.StringValue);
			break;
		default:
			throw new Exception("Illegal option setup for " + gamePref.ToStringCached());
		}
		OnValueChanged?.Invoke(this, gamePref);
		CheckDefaultValue();
	}

	public void SetCurrentValue()
	{
		try
		{
			switch (valueType)
			{
			case GamePrefs.EnumType.Int:
			{
				int num = GamePrefs.GetInt(gamePref);
				if (isTextInput)
				{
					controlText.Text = num.ToString();
					break;
				}
				bool flag2 = false;
				for (int j = 1; j < controlCombo.Elements.Count; j++)
				{
					if (controlCombo.Elements[j].IntValue == num)
					{
						controlCombo.SelectedIndex = j;
						flag2 = true;
						break;
					}
				}
				if (valuesEnforced && !flag2)
				{
					int num2 = -1;
					int num3 = int.MaxValue;
					for (int k = 0; k < controlCombo.Elements.Count; k++)
					{
						int num4 = Math.Abs(controlCombo.Elements[k].IntValue - num);
						if (num2 < 0 || num4 < num3)
						{
							num2 = k;
							num3 = num4;
							if (num3 <= 0)
							{
								break;
							}
						}
					}
					if (num2 >= 0)
					{
						controlCombo.SelectedIndex = num2;
						GamePrefs.Set(gamePref, controlCombo.Value.IntValue);
						flag2 = true;
					}
				}
				if (!flag2)
				{
					if (string.IsNullOrEmpty(valueLocalizationPrefixFromXml))
					{
						GameOptionValue value3 = new GameOptionValue(num, string.Format("{0} {1}", num.ToString(), Localization.Get("goCustomValueSuffix")));
						controlCombo.Value = value3;
					}
					else
					{
						GameOptionValue value4 = new GameOptionValue(num, string.Format(Localization.Get(valueLocalizationPrefixFromXml + ((num == 1) ? "" : "s")), num.ToString()) + " " + Localization.Get("goCustomValueSuffix"));
						controlCombo.Value = value4;
					}
				}
				break;
			}
			case GamePrefs.EnumType.Bool:
				controlCombo.SelectedIndex = (GamePrefs.GetBool(gamePref) ? 1 : 0);
				break;
			case GamePrefs.EnumType.String:
			{
				string text = GamePrefs.GetString(gamePref);
				if (isTextInput)
				{
					controlText.Text = GamePrefs.GetString(gamePref);
					break;
				}
				bool flag = false;
				for (int i = 1; i < controlCombo.Elements.Count; i++)
				{
					if (controlCombo.Elements[i].StringValue == text)
					{
						controlCombo.SelectedIndex = i;
						flag = true;
						break;
					}
				}
				if (valuesEnforced && !flag && controlCombo.Elements.Count > 0)
				{
					controlCombo.SelectedIndex = 0;
					GamePrefs.Set(gamePref, controlCombo.Value.StringValue);
					flag = true;
				}
				if (!flag)
				{
					if (string.IsNullOrEmpty(text))
					{
						controlCombo.SelectedIndex = 0;
						GamePrefs.Set(gamePref, controlCombo.Value.StringValue);
					}
					else if (string.IsNullOrEmpty(valueLocalizationPrefixFromXml))
					{
						GameOptionValue value = new GameOptionValue(text, string.Format("{0} {1}", text, Localization.Get("goCustomValueSuffix")));
						controlCombo.Value = value;
					}
					else
					{
						GameOptionValue value2 = new GameOptionValue(text, string.Format(Localization.Get(valueLocalizationPrefixFromXml ?? ""), text) + " " + Localization.Get("goCustomValueSuffix"));
						controlCombo.Value = value2;
					}
				}
				break;
			}
			default:
				throw new Exception("Illegal option setup for " + gamePref.ToStringCached());
			}
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		OnValueChanged?.Invoke(this, gamePref);
		CheckDefaultValue();
	}

	public void CheckDefaultValue()
	{
		Color color = ((!enabled) ? disabledColor : (IsDefaultValueForGameMode() ? Color.white : Color.yellow));
		controlText.ActiveTextColor = color;
		controlCombo.TextColor = color;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsDefaultValueForGameMode()
	{
		if (!hasDefault)
		{
			return true;
		}
		if (currentGameMode == null)
		{
			return true;
		}
		Dictionary<EnumGamePrefs, GameMode.ModeGamePref> gamePrefs = currentGameMode.GetGamePrefs();
		if (!gamePrefs.ContainsKey(gamePref))
		{
			return false;
		}
		switch (valueType)
		{
		case GamePrefs.EnumType.Int:
		{
			int _result;
			if (isTextInput)
			{
				StringParsers.TryParseSInt32(controlText.Text, out _result);
			}
			else
			{
				_result = controlCombo.Value.IntValue;
			}
			return _result == (int)gamePrefs[gamePref].DefaultValue;
		}
		case GamePrefs.EnumType.String:
			if (isTextInput)
			{
				return controlText.Text == (string)gamePrefs[gamePref].DefaultValue;
			}
			return controlCombo.Value.StringValue == (string)gamePrefs[gamePref].DefaultValue;
		case GamePrefs.EnumType.Bool:
			return controlCombo.Value.IntValue == 1 == (bool)gamePrefs[gamePref].DefaultValue;
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetVisible(bool _visible)
	{
		viewComponent.IsVisible = _visible;
	}

	public void SetCurrentGameMode(GameMode _gameMode)
	{
		currentGameMode = _gameMode;
		SetVisible(alwaysShow || (_gameMode?.GetGamePrefs().ContainsKey(gamePref) ?? false));
		CheckDefaultValue();
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void OverrideValues(List<string> overrideValues)
	{
		valuesFromXml = overrideValues.ToArray();
		SetupOptions();
	}
}
