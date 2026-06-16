using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class XUiM_PlayerBuffs : XUiModel
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string minuteAbbrev;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string secondAbbrev;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string hourAbbrev;

	public static List<string> GetInfoFromBuffNotification(EntityUINotification _notification, BuffValue _overridenBuff, XUi _xui)
	{
		List<string> _infoList = new List<string>();
		string buffDisplayInfo = GetBuffDisplayInfo(_notification);
		if (buffDisplayInfo != null)
		{
			_infoList.Add(StringFormatHandler(buffDisplayInfo, Localization.Get("xuiBuffStatDuration")));
		}
		BuffValue buff = _notification.Buff;
		if (buff.BuffClass?.Effects?.EffectGroups != null && buff.BuffClass.Effects.EffectGroups.Count > 0)
		{
			for (int i = 0; i < buff.BuffClass.Effects.EffectGroups.Count; i++)
			{
				for (int j = 0; j < buff.BuffClass.Effects.EffectGroups[i].PassiveEffects.Count; j++)
				{
					buff.BuffClass.Effects.EffectGroups[i].PassiveEffects[j]?.AddColoredInfoStrings(ref _infoList);
				}
			}
		}
		return _infoList;
	}

	public static string GetInfoFromBuff(EntityPlayerLocal _localPlayer, EntityUINotification _notification, BuffValue _overridenBuff)
	{
		StringBuilder stringBuilder = new StringBuilder();
		BuffValue buff = _notification.Buff;
		List<string> _infoList = new List<string>();
		if (buff.BuffClass?.Effects?.EffectGroups != null && buff.BuffClass.Effects.EffectGroups.Count > 0)
		{
			for (int i = 0; i < buff.BuffClass.Effects.EffectGroups.Count; i++)
			{
				for (int j = 0; j < buff.BuffClass.Effects.EffectGroups[i].PassiveEffects.Count; j++)
				{
					buff.BuffClass.Effects.EffectGroups[i].PassiveEffects[j]?.AddColoredInfoStrings(ref _infoList, buff.DurationInSeconds);
				}
			}
		}
		string newValue = Utils.ColorToHex(new Color32(222, 206, 163, byte.MaxValue));
		for (int k = 0; k < _infoList.Count; k++)
		{
			stringBuilder.Append(_infoList[k]);
		}
		return stringBuilder.ToString().Replace("REPLACE_COLOR", newValue);
	}

	public static string GetTimeString(float _currentTime)
	{
		int num = (int)Math.Floor(_currentTime / 3600f);
		int num2 = (int)Math.Floor((_currentTime - (float)(num * 3600)) / 60f);
		int num3 = (int)Math.Floor(_currentTime % 60f);
		if (num3 == 0 && num2 == 0 && num == 0)
		{
			return "<1" + Localization.Get("timeAbbreviationMinutes");
		}
		return string.Format("{0}{1}{2}", (num > 0) ? string.Format("{0}{1} ", num, Localization.Get("timeAbbreviationHours")) : "", (num2 > 0) ? string.Format("{0}{1} ", num2, Localization.Get("timeAbbreviationMinutes")) : "", (num3 > 0) ? string.Format("{0}{1} ", num3, Localization.Get("timeAbbreviationSeconds")) : "");
	}

	public static string GetBuffTimerDurationString(float _duration)
	{
		return GetTimeString(Mathf.FloorToInt(_duration * 20f));
	}

	public static string GetBuffTimerTimeLeftString(float _duration, float _maxDuration)
	{
		return GetTimeString((int)((_maxDuration - _duration) * 20f));
	}

	public static string GetBuffTimeLeftString(BuffValue _buff)
	{
		if (_buff.BuffClass == null)
		{
			return "";
		}
		if (_buff.BuffClass.DurationMax > 0f)
		{
			int num = (int)(_buff.BuffClass.DurationMax * (float)((_buff.BuffClass.StackType != BuffEffectStackTypes.Duration) ? 1 : _buff.StackEffectMultiplier) - _buff.DurationInSeconds + 0.9f);
			int num2 = num / 60;
			int num3 = num2 / 60;
			if (num3 > 0)
			{
				return $"{num3}H";
			}
			if (num2 > 0)
			{
				return $"{num2}M";
			}
			return $"{num}S";
		}
		return "";
	}

	public static string FormatWorldTimeString(int _duration)
	{
		int num = _duration / 24000;
		_duration -= num * 24000;
		if (num > 0)
		{
			if (_duration >= 23000)
			{
				return string.Format("{0}.9 {1}", num, Localization.Get("xuiBuffStatDays"));
			}
			return string.Format("{0}.{1} {2}", num, (int)Mathf.Floor((float)_duration / 24000f * 10f + 0.5f), Localization.Get("xuiBuffStatDays"));
		}
		int num2 = _duration / 1000;
		_duration -= num2 * 1000;
		if (_duration >= 900)
		{
			return string.Format("{0}.9 {1}", num2, Localization.Get("xuiBuffStatHours"));
		}
		return string.Format("{0}.{1} {2}", num2, (int)Mathf.Floor((float)_duration / 1000f * 10f + 0.5f), Localization.Get("xuiBuffStatHours"));
	}

	public static string GetBuffDisplayInfo(EntityUINotification _notification, BuffValue _overridenBuff = null)
	{
		if (_notification.Buff != null)
		{
			BuffValue buff = _notification.Buff;
			string buffTimeLeftString = GetBuffTimeLeftString(buff);
			if (buffTimeLeftString != null && buff.BuffClass.DurationMax != 0f)
			{
				return buffTimeLeftString;
			}
			if (_notification.DisplayMode == EnumEntityUINotificationDisplayMode.IconPlusCurrentValue)
			{
				switch (_notification.Units)
				{
				case "%":
					return (int)(_notification.CurrentValue * 100f) + "%";
				case "°":
					return ValueDisplayFormatters.Temperature(_notification.CurrentValue);
				case "cvar":
				{
					BuffClass buffClass = _notification.Buff.BuffClass;
					if (buffClass.DisplayValueKey != null)
					{
						string format = Localization.Get(buffClass.DisplayValueKey);
						return buffClass.DisplayValueFormat switch
						{
							BuffClass.CVarDisplayFormat.Degrees => string.Format(format, ValueDisplayFormatters.Temperature(_notification.CurrentValue)), 
							BuffClass.CVarDisplayFormat.Time => string.Format(format, GetCVarValueAsTimeString(_notification.CurrentValue)), 
							_ => string.Format(format, _notification.CurrentValue), 
						};
					}
					if (buffClass.DisplayValueFormat == BuffClass.CVarDisplayFormat.Time)
					{
						return GetCVarValueAsTimeString(_notification.CurrentValue);
					}
					return ((int)_notification.CurrentValue).ToString();
				}
				case "duration":
					return GetCVarValueAsTimeString(_notification.Buff.BuffClass.DurationMax - _notification.Buff.DurationInSeconds);
				default:
					if (_notification.Buff.BuffClass.DisplayValueKey != null)
					{
						if (_notification.Buff.BuffClass.DisplayValueFormat == BuffClass.CVarDisplayFormat.Time)
						{
							return string.Format(Localization.Get(_notification.Buff.BuffClass.DisplayValueKey), GetCVarValueAsTimeString(_notification.CurrentValue));
						}
						return string.Format(Localization.Get(_notification.Buff.BuffClass.DisplayValueKey), _notification.CurrentValue);
					}
					return ((int)_notification.CurrentValue).ToString();
				}
			}
		}
		return "";
	}

	public static string GetCVarValueAsTimeString(float _cvarValue)
	{
		if (_cvarValue == 0f)
		{
			return "";
		}
		if (hourAbbrev == null)
		{
			hourAbbrev = Localization.Get("timeAbbreviationHours");
			minuteAbbrev = Localization.Get("timeAbbreviationMinutes");
			secondAbbrev = Localization.Get("timeAbbreviationSeconds");
		}
		int num = (int)Math.Floor(_cvarValue / 3600f);
		int num2 = (int)Math.Floor((_cvarValue - (float)(num * 3600)) / 60f);
		int num3 = (int)Math.Floor(_cvarValue % 60f);
		if (num > 0)
		{
			if (num >= 5 || num2 == 0)
			{
				return $"{num}{hourAbbrev}";
			}
			return $"{num}{hourAbbrev} {num2}{minuteAbbrev}";
		}
		if (num2 > 0)
		{
			if (num2 >= 5 || num3 == 0)
			{
				return $"{num2}{minuteAbbrev}";
			}
			return $"{num2}{minuteAbbrev} {num3}{secondAbbrev}";
		}
		return $"{num3}{secondAbbrev}";
	}

	public static string ConvertToTimeString(float _timeSeconds)
	{
		if (_timeSeconds == 0f)
		{
			return "";
		}
		if (hourAbbrev == null)
		{
			hourAbbrev = Localization.Get("timeAbbreviationHours");
			minuteAbbrev = Localization.Get("timeAbbreviationMinutes");
			secondAbbrev = Localization.Get("timeAbbreviationSeconds");
		}
		int num = (int)Math.Floor(_timeSeconds / 3600f);
		int num2 = (int)Math.Floor((_timeSeconds - (float)(num * 3600)) / 60f);
		int num3 = (int)Math.Floor(_timeSeconds % 60f);
		if (num > 0)
		{
			if (num2 == 0)
			{
				return $"{num}{hourAbbrev}";
			}
			return $"{num}{hourAbbrev} {num2}{minuteAbbrev}";
		}
		if (num2 > 0)
		{
			if (num3 == 0)
			{
				return $"{num2}{minuteAbbrev}";
			}
			return $"{num2}{minuteAbbrev} {num3}{secondAbbrev}";
		}
		return $"{num3}{secondAbbrev}";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string StringFormatHandler(string _title, object _value)
	{
		return $"{_title}: [REPLACE_COLOR]{_value}[-]\n";
	}
}
