using System.Collections.Generic;
using MusicUtils;
using UnityEngine.Audio;

namespace DynamicMusic.Legacy;

public class TransitionManager : IGamePrefsChangedListener
{
	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicMusicManager dynamicMusicManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public static AudioMixer Master;

	[PublicizedFrom(EAccessModifier.Private)]
	public static LogarithmicCurve DmsAbsoluteVolumeCurve;

	[PublicizedFrom(EAccessModifier.Private)]
	public static LogarithmicCurve DmsEventRangeVolumeCurve;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float dawnDuskFadeTime = 1f / 6f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float currentAbsoluteDMSLogVolume;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float currentEventDMSLogVolume;

	public float MasterParam
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!dynamicMusicManager.IsMusicPlayingThisTick)
			{
				return 0f;
			}
			return dynamicMusicManager.ThreatLevelTracker.NumericalThreatLevel;
		}
	}

	public static void Init(DynamicMusicManager _dynamicMusicManager)
	{
		_dynamicMusicManager.TransitionManager = new TransitionManager();
		_dynamicMusicManager.TransitionManager.dynamicMusicManager = _dynamicMusicManager;
		Master = DataLoader.LoadAsset<AudioMixer>("@:Sound_Mixers/MasterAudioMixer.mixer");
		DmsAbsoluteVolumeCurve = new LogarithmicCurve(2.0, 6.0, -80f, 0f, 0f, 1f);
		SetDynamicMusicVolume(GamePrefs.GetFloat(EnumGamePrefs.OptionsMusicVolumeLevel));
		GamePrefs.AddChangeListener(_dynamicMusicManager.TransitionManager);
	}

	public void Tick()
	{
		if (Master == null)
		{
			return;
		}
		foreach (KeyValuePair<string, Curve> dspCurf in SignalProcessing.DspCurves)
		{
			Master.SetFloat(dspCurf.Key, dspCurf.Value.GetMixerValue(MasterParam));
		}
		if (!dynamicMusicManager.IsInDeadWindow)
		{
			if (dynamicMusicManager.DistanceFromDeadWindow > dawnDuskFadeTime)
			{
				Master.SetFloat("dmsVol", currentEventDMSLogVolume = currentAbsoluteDMSLogVolume);
			}
			else
			{
				Master.SetFloat("dmsVol", currentEventDMSLogVolume = DmsEventRangeVolumeCurve.GetMixerValue(dynamicMusicManager.DistanceFromDeadWindow));
			}
		}
		else
		{
			Master.SetFloat("dmsVol", currentEventDMSLogVolume = -80f);
		}
	}

	public static void SetDynamicMusicVolume(float _value)
	{
		if (Master != null)
		{
			currentAbsoluteDMSLogVolume = DmsAbsoluteVolumeCurve.GetMixerValue(_value);
			Master.SetFloat("dmsVol", currentAbsoluteDMSLogVolume);
			DmsEventRangeVolumeCurve = new LogarithmicCurve(2.0, 6.0, currentAbsoluteDMSLogVolume, -80f, dawnDuskFadeTime, 0f);
		}
	}

	public static void ApplyPauseFilter()
	{
		if (Master != null)
		{
			Master.SetFloat("dmsCutOff", 500f);
		}
	}

	public static void RemovePauseFilter()
	{
		if (Master != null)
		{
			Master.SetFloat("dmsCutOff", 22000f);
		}
	}

	public void OnGamePrefChanged(EnumGamePrefs _enum)
	{
		if (_enum == EnumGamePrefs.OptionsMusicVolumeLevel)
		{
			SetDynamicMusicVolume(GamePrefs.GetFloat(EnumGamePrefs.OptionsMusicVolumeLevel));
		}
	}
}
