using MusicUtils;
using UnityEngine.Audio;

namespace DynamicMusic;

public class MixerController : IGamePrefsChangedListener
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static MixerController instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public static AudioMixer Master;

	[PublicizedFrom(EAccessModifier.Private)]
	public static LogarithmicCurve DmsAbsoluteVolumeCurve;

	[PublicizedFrom(EAccessModifier.Private)]
	public static LogarithmicCurve AllCombatVolumeCurve;

	public static MixerController Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new MixerController();
			}
			return instance;
		}
	}

	public void Init()
	{
		DmsAbsoluteVolumeCurve = new LogarithmicCurve(2.0, 6.0, -80f, 0f, 0f, 1f);
		AllCombatVolumeCurve = new LogarithmicCurve(2.0, 6.0, -4f, 0f, 0.7f, 1f);
		Master = DataLoader.LoadAsset<AudioMixer>("@:Sound_Mixers/MasterAudioMixer.mixer");
		SetDynamicMusicVolume(GamePrefs.GetFloat(EnumGamePrefs.OptionsMusicVolumeLevel));
		GamePrefs.AddChangeListener(this);
	}

	public void Update()
	{
		SetAllCombatVolume();
	}

	public void OnGamePrefChanged(EnumGamePrefs _enum)
	{
		if (_enum.Equals(EnumGamePrefs.OptionsMusicVolumeLevel))
		{
			SetDynamicMusicVolume(GamePrefs.GetFloat(EnumGamePrefs.OptionsMusicVolumeLevel));
		}
	}

	public void SetDynamicMusicVolume(float _vol)
	{
		if ((bool)Master)
		{
			Master.SetFloat("dmsVol", DmsAbsoluteVolumeCurve.GetMixerValue(_vol));
		}
	}

	public void OnSnapshotTransition()
	{
		float dynamicMusicVolume = GamePrefs.GetFloat(EnumGamePrefs.OptionsMusicVolumeLevel);
		SetDynamicMusicVolume(dynamicMusicVolume);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetAllCombatVolume()
	{
		if ((bool)Master)
		{
			float mixerValue = AllCombatVolumeCurve.GetMixerValue(GameManager.Instance.World.GetPrimaryPlayer().ThreatLevel.Numeric);
			Master?.SetFloat("AllCbtVol", mixerValue);
		}
	}
}
