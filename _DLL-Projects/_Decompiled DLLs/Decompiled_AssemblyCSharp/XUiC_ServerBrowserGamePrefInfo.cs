using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowserGamePrefInfo : XUiController
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
	public XUiV_Label label;

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
	public GameInfoString gameInfoString;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameOptionValue> values;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<int, int> valuePreDisplayModifierFunc;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<GameServerInfo, int, string> customIntValueFormatter;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<GameServerInfo, string, string> customStringValueFormatter;

	public GamePrefs.EnumType ValueType => valueType;

	public GameInfoBool GameInfoBool => gameInfoBool;

	public GameInfoInt GameInfoInt => gameInfoInt;

	public GameInfoString GameInfoString => gameInfoString;

	public Func<int, int> ValuePreDisplayModifierFunc
	{
		set
		{
			if (valuePreDisplayModifierFunc != value)
			{
				valuePreDisplayModifierFunc = value;
				SetupOptions();
			}
		}
	}

	public Func<GameServerInfo, int, string> CustomIntValueFormatter
	{
		set
		{
			if (customIntValueFormatter != value)
			{
				customIntValueFormatter = value;
				IsDirty = true;
			}
		}
	}

	public Func<GameServerInfo, string, string> CustomStringValueFormatter
	{
		set
		{
			if (customStringValueFormatter != value)
			{
				customStringValueFormatter = value;
				IsDirty = true;
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
		if (EnumUtils.TryParse<GameInfoString>(viewComponent.ID, out gameInfoString))
		{
			valueType = GamePrefs.EnumType.String;
		}
		else
		{
			gameInfoString = (GameInfoString)(-1);
		}
		label = (XUiV_Label)GetChildById("value").ViewComponent;
		SetupOptions();
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (!(_name == "display_names"))
		{
			if (_name == "value_localization_prefix")
			{
				if (_value.Length > 0)
				{
					valueLocalizationPrefixFromXml = _value.Trim();
				}
				return true;
			}
			return base.ParseAttribute(_name, _value, _parent);
		}
		if (_value.Length > 0)
		{
			namesFromXml = _value.Split(',');
			for (int i = 0; i < namesFromXml.Length; i++)
			{
				namesFromXml[i] = namesFromXml[i].Trim();
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupOptions()
	{
		if (valueType == GamePrefs.EnumType.Int)
		{
			if (namesFromXml == null)
			{
				return;
			}
			values = new List<GameOptionValue>();
			for (int i = 0; i < namesFromXml.Length; i++)
			{
				string text = namesFromXml[i];
				int value = i;
				if (text.IndexOf('=') > 0)
				{
					string[] array = text.Split('=');
					text = array[1];
					value = StringParsers.ParseSInt32(array[0]);
				}
				values.Add(new GameOptionValue(value, Localization.Get(text)));
			}
		}
		else if (valueType == GamePrefs.EnumType.Bool)
		{
			values = new List<GameOptionValue>
			{
				new GameOptionValue(0, Localization.Get("goOff")),
				new GameOptionValue(1, Localization.Get("goOn"))
			};
		}
	}

	public void SetCurrentValue(GameServerInfo _gameInfo)
	{
		try
		{
			if (_gameInfo != null)
			{
				if (valueType == GamePrefs.EnumType.Int)
				{
					int value = _gameInfo.GetValue(gameInfoInt);
					bool flag = false;
					if (values != null)
					{
						foreach (GameOptionValue value4 in values)
						{
							if (value4.Value == value)
							{
								label.Text = value4.ToString();
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						if (customIntValueFormatter != null)
						{
							label.Text = customIntValueFormatter(_gameInfo, value);
						}
						else if (valueLocalizationPrefixFromXml != null)
						{
							label.Text = string.Format(Localization.Get(valueLocalizationPrefixFromXml + ((value == 1) ? "" : "s")), value);
						}
						else
						{
							label.Text = value.ToString();
						}
					}
				}
				else if (valueType == GamePrefs.EnumType.Bool)
				{
					bool value2 = _gameInfo.GetValue(gameInfoBool);
					label.Text = (value2 ? values[1].ToString() : values[0].ToString());
				}
				else if (valueType == GamePrefs.EnumType.String)
				{
					string value3 = _gameInfo.GetValue(gameInfoString);
					if (customStringValueFormatter != null)
					{
						label.Text = customStringValueFormatter(_gameInfo, value3);
					}
					else if (valueLocalizationPrefixFromXml != null && !string.IsNullOrEmpty(value3))
					{
						label.Text = Localization.Get(valueLocalizationPrefixFromXml + value3);
					}
					else
					{
						label.Text = value3;
					}
				}
			}
			else
			{
				label.Text = "";
			}
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
	}
}
