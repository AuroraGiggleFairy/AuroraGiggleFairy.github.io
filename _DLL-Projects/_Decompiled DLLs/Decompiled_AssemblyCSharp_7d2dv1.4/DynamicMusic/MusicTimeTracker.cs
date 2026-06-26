using System;
using System.Collections.Generic;
using MusicUtils.Enums;
using UnityEngine;

namespace DynamicMusic;

public class MusicTimeTracker : AbstractMusicTimeTracker, IMultiNotifiableFilter, INotifiable, INotifiableFilter<MusicActionType, SectionType>, INotifiable<MusicActionType>, IFilter<SectionType>, IGamePrefsChangedListener
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IMultiNotifiableFilter FrequencyLimiter;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumDictionary<MusicActionType, Action> MusicActions;

	public MusicTimeTracker()
	{
		dailyAllottedPlayTime = GamePrefs.GetFloat(EnumGamePrefs.OptionsDynamicMusicDailyTime) * (float)GamePrefs.GetInt(EnumGamePrefs.DayNightLength) * 60f;
		MusicActions = new EnumDictionary<MusicActionType, Action>(4);
		MusicActions.Add(MusicActionType.Play, OnPlay);
		MusicActions.Add(MusicActionType.Pause, OnPause);
		MusicActions.Add(MusicActionType.UnPause, OnUnPause);
		MusicActions.Add(MusicActionType.Stop, OnStop);
		MusicActions.Add(MusicActionType.FadeIn, OnFadeIn);
		FrequencyLimiter = new FrequencyLimiter();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPlay()
	{
		musicStartTime = (float)AudioSettings.dspTime;
		IsMusicPlaying = true;
		pauseStartTime = (pauseDuration = 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPause()
	{
		pauseStartTime = (float)AudioSettings.dspTime;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnUnPause()
	{
		pauseDuration += (float)AudioSettings.dspTime - pauseStartTime;
		pauseStartTime = 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnStop()
	{
		dailyPlayTimeUsed += (float)AudioSettings.dspTime - musicStartTime - pauseDuration;
		musicStartTime = (pauseDuration = 0f);
		IsMusicPlaying = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnFadeIn()
	{
		if (!IsMusicPlaying)
		{
			musicStartTime = (float)AudioSettings.dspTime;
		}
	}

	public void OnGamePrefChanged(EnumGamePrefs _enum)
	{
		if (_enum == EnumGamePrefs.OptionsDynamicMusicDailyTime || _enum == EnumGamePrefs.DayNightLength)
		{
			dailyAllottedPlayTime = GamePrefs.GetFloat(EnumGamePrefs.OptionsDynamicMusicDailyTime) * (float)GamePrefs.GetInt(EnumGamePrefs.DayNightLength);
		}
	}

	public void Cleanup()
	{
	}

	public override List<SectionType> Filter(List<SectionType> _sectionTypes)
	{
		if (!IsMusicPlaying)
		{
			if (dailyPlayTimeUsed < dailyAllottedPlayTime)
			{
				FrequencyLimiter.Filter(_sectionTypes);
			}
			else
			{
				_sectionTypes.Remove(SectionType.HomeDay);
				_sectionTypes.Remove(SectionType.HomeNight);
				_sectionTypes.Remove(SectionType.Exploration);
				_sectionTypes.Remove(SectionType.Suspense);
			}
		}
		return _sectionTypes;
	}

	public void Notify()
	{
		dailyPlayTimeUsed = 0f;
		FrequencyLimiter.Notify();
	}

	public void Notify(MusicActionType _state)
	{
		if (MusicActions.TryGetValue(_state, out var value))
		{
			value();
		}
		FrequencyLimiter.Notify(_state);
	}

	public override string ToString()
	{
		return $"Daily Play Time Allotted: {dailyAllottedPlayTime}\nPlay Time Used: {dailyPlayTimeUsed}\nIs Music Playing: {IsMusicPlaying}\nMusic Start Time: {musicStartTime}\nPause Start Time: {pauseStartTime}\nPause Duration: {pauseDuration}\n";
	}
}
