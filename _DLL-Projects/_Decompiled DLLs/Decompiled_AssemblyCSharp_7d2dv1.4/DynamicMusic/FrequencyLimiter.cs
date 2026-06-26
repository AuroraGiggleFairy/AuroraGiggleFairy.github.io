using System;
using System.Collections.Generic;
using MusicUtils.Enums;
using UnityEngine;

namespace DynamicMusic;

public class FrequencyLimiter : AbstractFilter, IMultiNotifiableFilter, INotifiable, INotifiableFilter<MusicActionType, SectionType>, INotifiable<MusicActionType>, IFilter<SectionType>, IGamePrefsChangedListener
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMinCooldown = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cProbOfFailingToReachDailyAllotted = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPlayTime = 168f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dayLengthInSeconds;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dailyAllottedPlaySeconds;

	[PublicizedFrom(EAccessModifier.Private)]
	public float rollsPerDay;

	[PublicizedFrom(EAccessModifier.Private)]
	public float cooldown;

	[PublicizedFrom(EAccessModifier.Private)]
	public float chanceOfPositiveRoll;

	[PublicizedFrom(EAccessModifier.Private)]
	public double rollTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameRandom rng;

	public FrequencyLimiter()
	{
		rng = GameRandomManager.Instance.CreateGameRandom();
		GamePrefs.AddChangeListener(this);
		UpdateParameters();
		UpdateRollTime();
	}

	public override List<SectionType> Filter(List<SectionType> _sectionTypes)
	{
		if (AudioSettings.dspTime <= rollTime || rng.RandomRange(1f) > chanceOfPositiveRoll)
		{
			for (int num = _sectionTypes.Count - 1; num >= 0; num--)
			{
				if (_sectionTypes[num] != SectionType.None && _sectionTypes[num] != SectionType.Combat)
				{
					_sectionTypes.RemoveAt(num);
				}
			}
		}
		return _sectionTypes;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateRollTime()
	{
		rollTime = AudioSettings.dspTime + (double)cooldown;
	}

	public void Notify(MusicActionType _state)
	{
		if (_state.Equals(MusicActionType.Stop))
		{
			UpdateRollTime();
		}
	}

	public void Notify()
	{
		UpdateRollTime();
	}

	public void OnGamePrefChanged(EnumGamePrefs _enum)
	{
		if (_enum.Equals(EnumGamePrefs.OptionsDynamicMusicDailyTime) || _enum.Equals(EnumGamePrefs.DayNightLength))
		{
			UpdateParameters();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateParameters()
	{
		dayLengthInSeconds = (float)GamePrefs.GetInt(EnumGamePrefs.DayNightLength) * 60f;
		dailyAllottedPlaySeconds = GamePrefs.GetFloat(EnumGamePrefs.OptionsDynamicMusicDailyTime) * dayLengthInSeconds;
		rollsPerDay = Mathf.Ceil(dailyAllottedPlaySeconds / 168f);
		float num = dayLengthInSeconds / rollsPerDay;
		cooldown = Mathf.Max(num - 168f, 30f);
		chanceOfPositiveRoll = (float)Math.Pow(0.8999999761581421, 1.0 / (double)rollsPerDay);
	}
}
