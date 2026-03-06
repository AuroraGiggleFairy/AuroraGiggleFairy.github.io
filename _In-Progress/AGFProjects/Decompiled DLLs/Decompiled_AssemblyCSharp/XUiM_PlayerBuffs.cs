using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class XUiM_PlayerBuffs : XUiModel
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblDuration;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblTimeLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblHealth;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblStamina;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblGassiness;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblSickness;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblMovementSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblWellness;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblCoreTemp;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblHydration;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblFullness;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblStatModifierValueOTForSeconds;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblStatModifierValueOT;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblStatModifierSetValueForSecondsInc;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblStatModifierSetValueForSecondsDec;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblStatModifierSetValueInc;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblStatModifierSetValueDec;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblStatModifierMaxForSeconds;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblStatModifierMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblHours;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblDays;

	public static bool HasLocalizationBeenCached;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string minuteAbbrev;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string secondAbbrev;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string hourAbbrev;

	public static List<string> GetInfoFromBuffNotification(EntityUINotification notification, BuffValue overridenBuff, XUi _xui)
	{
		_ = _xui.playerUI.entityPlayer;
		List<string> _infoList = new List<string>();
		string buffDisplayInfo = GetBuffDisplayInfo(notification);
		if (buffDisplayInfo != null)
		{
			_infoList.Add(StringFormatHandler(buffDisplayInfo, lblDuration));
		}
		BuffValue buff = notification.Buff;
		if (buff.BuffClass != null && buff.BuffClass.Effects != null && buff.BuffClass.Effects.EffectGroups != null && buff.BuffClass.Effects.EffectGroups.Count > 0)
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

	public static string GetInfoFromBuff(EntityPlayerLocal _localPlayer, EntityUINotification notification, BuffValue overridenBuff)
	{
		if (!HasLocalizationBeenCached)
		{
			lblDuration = Localization.Get("xuiBuffStatDuration");
			lblTimeLeft = Localization.Get("xuiBuffStatTimeLeft");
			lblHealth = Localization.Get("xuiBuffStatHealth");
			lblStamina = Localization.Get("xuiBuffStatStamina");
			lblGassiness = Localization.Get("xuiBuffStatGassiness");
			lblSickness = Localization.Get("xuiBuffStatSickness");
			lblMovementSpeed = Localization.Get("xuiBuffStatMovementSpeed");
			lblWellness = Localization.Get("xuiBuffStatWellness");
			lblCoreTemp = Localization.Get("xuiBuffStatCoreTemp");
			lblHydration = Localization.Get("lblHydration");
			lblFullness = Localization.Get("lblFullness");
			lblStatModifierValueOTForSeconds = Localization.Get("xuiBuffStatModifierValueOTForSeconds");
			lblStatModifierValueOT = Localization.Get("xuiBuffStatModifierValueOT");
			lblStatModifierSetValueForSecondsInc = Localization.Get("xuiBuffStatModifierSetValueForSecondsInc");
			lblStatModifierSetValueForSecondsDec = Localization.Get("xuiBuffStatModifierSetValueForSecondsDec");
			lblStatModifierSetValueInc = Localization.Get("xuiBuffStatModifierSetValueInc");
			lblStatModifierSetValueDec = Localization.Get("xuiBuffStatModifierSetValueDec");
			lblStatModifierMaxForSeconds = Localization.Get("xuiBuffStatModifierMaxForSeconds");
			lblStatModifierMax = Localization.Get("xuiBuffStatModifierMax");
			lblHours = Localization.Get("xuiBuffStatHours");
			lblDays = Localization.Get("xuiBuffStatDays");
			HasLocalizationBeenCached = true;
		}
		StringBuilder stringBuilder = new StringBuilder();
		BuffValue buff = notification.Buff;
		List<string> _infoList = new List<string>();
		if (buff.BuffClass != null && buff.BuffClass.Effects != null && buff.BuffClass.Effects.EffectGroups != null && buff.BuffClass.Effects.EffectGroups.Count > 0)
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

	public static string GetTimeString(float currentTime)
	{
		int num = (int)Math.Floor(currentTime / 3600f);
		int num2 = (int)Math.Floor((currentTime - (float)(num * 3600)) / 60f);
		int num3 = (int)Math.Floor(currentTime % 60f);
		if (num3 == 0 && num2 == 0 && num == 0)
		{
			return string.Format("<1{0}", Localization.Get("timeAbbreviationMinutes"));
		}
		return string.Format("{0}{1}{2}", (num > 0) ? string.Format("{0}{1} ", num, Localization.Get("timeAbbreviationHours")) : "", (num2 > 0) ? string.Format("{0}{1} ", num2, Localization.Get("timeAbbreviationMinutes")) : "", (num3 > 0) ? string.Format("{0}{1} ", num3, Localization.Get("timeAbbreviationSeconds")) : "");
	}

	public static string GetBuffTimerDurationString(float duration)
	{
		return GetTimeString(Mathf.FloorToInt(duration * 20f));
	}

	public static string GetBuffTimerTimeLeftString(float duration, float maxDuration)
	{
		return GetTimeString((int)((maxDuration - duration) * 20f));
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

	public static string FormatWorldTimeString(int duration)
	{
		int num = duration / 24000;
		duration -= num * 24000;
		if (num > 0)
		{
			if (duration >= 23000)
			{
				return $"{num}.{9} {lblDays}";
			}
			return $"{num}.{(int)Mathf.Floor((float)duration / 24000f * 10f + 0.5f)} {lblDays}";
		}
		int num2 = duration / 1000;
		duration -= num2 * 1000;
		if (duration >= 900)
		{
			return $"{num2}.{9} {lblHours}";
		}
		return $"{num2}.{(int)Mathf.Floor((float)duration / 1000f * 10f + 0.5f)} {lblHours}";
	}

	public static string GetBuffDisplayInfo(EntityUINotification notification, BuffValue overridenBuff = null)
	{
		if (notification.Buff != null)
		{
			BuffValue buff = notification.Buff;
			string buffTimeLeftString = GetBuffTimeLeftString(buff);
			if (buffTimeLeftString != null && buff.BuffClass.DurationMax != 0f)
			{
				return buffTimeLeftString;
			}
			if (notification.DisplayMode == EnumEntityUINotificationDisplayMode.IconPlusCurrentValue)
			{
				switch (notification.Units)
				{
				case "%":
					return (int)(notification.CurrentValue * 100f) + "%";
				case "°":
					return ValueDisplayFormatters.Temperature(notification.CurrentValue);
				case "cvar":
				{
					BuffClass buffClass = notification.Buff.BuffClass;
					if (buffClass.DisplayValueKey != null)
					{
						string format = Localization.Get(buffClass.DisplayValueKey);
						return buffClass.DisplayValueFormat switch
						{
							BuffClass.CVarDisplayFormat.Degrees => string.Format(format, ValueDisplayFormatters.Temperature(notification.CurrentValue)), 
							BuffClass.CVarDisplayFormat.Time => string.Format(format, GetCVarValueAsTimeString(notification.CurrentValue)), 
							_ => string.Format(format, notification.CurrentValue), 
						};
					}
					if (buffClass.DisplayValueFormat == BuffClass.CVarDisplayFormat.Time)
					{
						return GetCVarValueAsTimeString(notification.CurrentValue);
					}
					return ((int)notification.CurrentValue).ToString();
				}
				case "duration":
					return GetCVarValueAsTimeString(notification.Buff.BuffClass.DurationMax - notification.Buff.DurationInSeconds);
				default:
					if (notification.Buff.BuffClass.DisplayValueKey != null)
					{
						if (notification.Buff.BuffClass.DisplayValueFormat == BuffClass.CVarDisplayFormat.Time)
						{
							return string.Format(Localization.Get(notification.Buff.BuffClass.DisplayValueKey), GetCVarValueAsTimeString(notification.CurrentValue));
						}
						return string.Format(Localization.Get(notification.Buff.BuffClass.DisplayValueKey), notification.CurrentValue);
					}
					return ((int)notification.CurrentValue).ToString();
				}
			}
		}
		return "";
	}

	public static string GetCVarValueAsTimeString(float cvarValue)
	{
		if (cvarValue == 0f)
		{
			return "";
		}
		if (hourAbbrev == null)
		{
			hourAbbrev = Localization.Get("timeAbbreviationHours");
			minuteAbbrev = Localization.Get("timeAbbreviationMinutes");
			secondAbbrev = Localization.Get("timeAbbreviationSeconds");
		}
		int num = (int)Math.Floor(cvarValue / 3600f);
		int num2 = (int)Math.Floor((cvarValue - (float)(num * 3600)) / 60f);
		int num3 = (int)Math.Floor(cvarValue % 60f);
		if (num > 0)
		{
			if (num >= 5 || num2 == 0)
			{
				return $"{num}{hourAbbrev}";
			}
			return string.Format("{0}{2} {1}{3}", num, num2, hourAbbrev, minuteAbbrev);
		}
		if (num2 > 0)
		{
			if (num2 >= 5 || num3 == 0)
			{
				return $"{num2}{minuteAbbrev}";
			}
			return string.Format("{0}{2} {1}{3}", num2, num3, minuteAbbrev, secondAbbrev);
		}
		return $"{num3}{secondAbbrev}";
	}

	public static string ConvertToTimeString(float timeSeconds)
	{
		if (timeSeconds == 0f)
		{
			return "";
		}
		if (hourAbbrev == null)
		{
			hourAbbrev = Localization.Get("timeAbbreviationHours");
			minuteAbbrev = Localization.Get("timeAbbreviationMinutes");
			secondAbbrev = Localization.Get("timeAbbreviationSeconds");
		}
		int num = (int)Math.Floor(timeSeconds / 3600f);
		int num2 = (int)Math.Floor((timeSeconds - (float)(num * 3600)) / 60f);
		int num3 = (int)Math.Floor(timeSeconds % 60f);
		if (num > 0)
		{
			if (num2 == 0)
			{
				return $"{num}{hourAbbrev}";
			}
			return string.Format("{0}{2} {1}{3}", num, num2, hourAbbrev, minuteAbbrev);
		}
		if (num2 > 0)
		{
			if (num3 == 0)
			{
				return $"{num2}{minuteAbbrev}";
			}
			return string.Format("{0}{2} {1}{3}", num2, num3, minuteAbbrev, secondAbbrev);
		}
		return $"{num3}{secondAbbrev}";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string StringFormatHandler(string title, object value)
	{
		return $"{title}: [REPLACE_COLOR]{value}[-]\n";
	}
}
