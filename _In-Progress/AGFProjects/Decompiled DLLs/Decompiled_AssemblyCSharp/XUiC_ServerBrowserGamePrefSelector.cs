using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowserGamePrefSelector : XUiController
{
	public struct GameOptionValue(int _value, string _displayName)
	{
		public readonly int Value = _value;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string DisplayName = _displayName;

		public override string ToString()
		{
			return DisplayName;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<GameOptionValue> controlCombo;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] valuesFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] namesFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public string valueLocalizationPrefixFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public GamePrefs.EnumType valueType;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameInfoBool gameInfoBool;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameInfoInt gameInfoInt;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<int, int> valuePreDisplayModifierFunc;

	public Action<XUiC_ServerBrowserGamePrefSelector> OnValueChanged;

	public GamePrefs.EnumType ValueType => valueType;

	public GameInfoBool GameInfoBool => gameInfoBool;

	public GameInfoInt GameInfoInt => gameInfoInt;

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

	public override void Init()
	{
		base.Init();
		if (EnumUtils.TryParse<GameInfoBool>(viewComponent.ID, out gameInfoBool))
		{
			valueType = GamePrefs.EnumType.Bool;
		}
		else
		{
			gameInfoBool = (GameInfoBool)(-1);
		}
		if (EnumUtils.TryParse<GameInfoInt>(viewComponent.ID, out gameInfoInt))
		{
			valueType = GamePrefs.EnumType.Int;
		}
		else
		{
			gameInfoInt = (GameInfoInt)(-1);
		}
		controlCombo = GetChildById("ControlCombo").GetChildByType<XUiC_ComboBoxList<GameOptionValue>>();
		controlCombo.OnValueChanged += ControlCombo_OnValueChanged;
		SetupOptions();
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "values":
			if (_value.Length > 0)
			{
				string[] array = _value.Split(',');
				valuesFromXml = new int[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					valuesFromXml[i] = StringParsers.ParseSInt32(array[i]);
				}
			}
			return true;
		case "display_names":
			if (_value.Length > 0)
			{
				namesFromXml = _value.Split(',');
				for (int j = 0; j < namesFromXml.Length; j++)
				{
					namesFromXml[j] = namesFromXml[j].Trim();
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
	public string GetGameInfoName()
	{
		return valueType switch
		{
			GamePrefs.EnumType.Bool => gameInfoBool.ToStringCached(), 
			GamePrefs.EnumType.Int => gameInfoInt.ToStringCached(), 
			_ => null, 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupOptions()
	{
		string[] array = null;
		if (valueType == GamePrefs.EnumType.Int)
		{
			if (valuesFromXml == null)
			{
				if (namesFromXml == null)
				{
					throw new Exception("Illegal option setup for " + GetGameInfoName() + " (no values and no names specified)");
				}
				valuesFromXml = new int[namesFromXml.Length];
				for (int i = 0; i < valuesFromXml.Length; i++)
				{
					valuesFromXml[i] = i;
				}
			}
			array = new string[valuesFromXml.Length];
			if (namesFromXml == null || namesFromXml.Length != valuesFromXml.Length)
			{
				for (int j = 0; j < valuesFromXml.Length; j++)
				{
					if (namesFromXml != null && j < namesFromXml.Length)
					{
						array[j] = Localization.Get(namesFromXml[j]);
						continue;
					}
					int num = valuesFromXml[j];
					if (valuePreDisplayModifierFunc != null)
					{
						num = valuePreDisplayModifierFunc(num);
					}
					array[j] = string.Format(Localization.Get(valueLocalizationPrefixFromXml + ((num == 1) ? "" : "s")), num);
				}
			}
			else
			{
				for (int k = 0; k < namesFromXml.Length; k++)
				{
					array[k] = Localization.Get(namesFromXml[k]);
				}
			}
		}
		else if (valueType == GamePrefs.EnumType.Bool)
		{
			valuesFromXml = new int[2] { 0, 1 };
			array = new string[2]
			{
				Localization.Get("goOff"),
				Localization.Get("goOn")
			};
		}
		controlCombo.Elements.Clear();
		GameOptionValue item = new GameOptionValue(int.MinValue, Localization.Get("goAnyValue"));
		controlCombo.Elements.Add(item);
		if (valuesFromXml == null)
		{
			throw new Exception("Illegal option setup for " + GetGameInfoName() + " (values still null)");
		}
		if (array == null)
		{
			throw new Exception("Illegal option setup for " + GetGameInfoName() + " (names null)");
		}
		for (int l = 0; l < valuesFromXml.Length; l++)
		{
			item = new GameOptionValue(valuesFromXml[l], array[l]);
			controlCombo.Elements.Add(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlCombo_OnValueChanged(XUiController _sender, GameOptionValue _oldValue, GameOptionValue _newValue)
	{
		if (OnValueChanged != null)
		{
			OnValueChanged(this);
		}
	}

	public void SetCurrentValue(object _value)
	{
		try
		{
			if (valueType == GamePrefs.EnumType.Int)
			{
				int num = (int)_value;
				bool flag = false;
				for (int i = 1; i < controlCombo.Elements.Count; i++)
				{
					if (controlCombo.Elements[i].Value == num)
					{
						controlCombo.SelectedIndex = i;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					controlCombo.SelectedIndex = controlCombo.MinIndex;
				}
			}
			else if (valueType == GamePrefs.EnumType.Bool)
			{
				controlCombo.SelectedIndex = ((!(bool)_value) ? 1 : 2);
			}
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
	}

	public XUiC_ServersList.UiServerFilter GetFilter()
	{
		int value = controlCombo.Value.Value;
		bool flag = value == int.MinValue;
		string name = valueType switch
		{
			GamePrefs.EnumType.Bool => gameInfoBool.ToStringCached(), 
			GamePrefs.EnumType.Int => gameInfoInt.ToStringCached(), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (flag)
		{
			return new XUiC_ServersList.UiServerFilter(name, XUiC_ServersList.EnumServerLists.Regular);
		}
		int intMinValue = 0;
		bool boolValue = false;
		Func<XUiC_ServersList.ListEntry, bool> func;
		IServerListInterface.ServerFilter.EServerFilterType type;
		switch (valueType)
		{
		case GamePrefs.EnumType.Bool:
		{
			bool bValue = value == 1;
			func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(gameInfoBool) == bValue;
			type = IServerListInterface.ServerFilter.EServerFilterType.BoolValue;
			boolValue = bValue;
			break;
		}
		case GamePrefs.EnumType.Int:
		{
			int iValue = value;
			func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(gameInfoInt) == iValue;
			type = IServerListInterface.ServerFilter.EServerFilterType.IntValue;
			intMinValue = iValue;
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
		return new XUiC_ServersList.UiServerFilter(name, XUiC_ServersList.EnumServerLists.Regular, func, type, intMinValue, 0, boolValue);
	}
}
