using MusicUtils;
using UnityEngine.Audio;

public class AmbientAudioController : IGamePrefsChangedListener
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static AmbientAudioController instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public static AudioMixer master;

	[PublicizedFrom(EAccessModifier.Private)]
	public static LogarithmicCurve volumeCurve;

	public static AmbientAudioController Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new AmbientAudioController();
			}
			return instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public AmbientAudioController()
	{
		GamePrefs.AddChangeListener(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static AmbientAudioController()
	{
		master = DataLoader.LoadAsset<AudioMixer>("@:Sound_Mixers/MasterAudioMixer.mixer");
		volumeCurve = new LogarithmicCurve(2.0, 6.0, -80f, 0f, 0f, 1f);
	}

	public void SetAmbientVolume(float _val)
	{
		if ((bool)master)
		{
			master.SetFloat("ambVol", volumeCurve.GetMixerValue(_val));
		}
	}

	public void OnGamePrefChanged(EnumGamePrefs _enum)
	{
		if (_enum == EnumGamePrefs.OptionsAmbientVolumeLevel)
		{
			SetAmbientVolume(GamePrefs.GetFloat(EnumGamePrefs.OptionsAmbientVolumeLevel));
		}
	}
}
