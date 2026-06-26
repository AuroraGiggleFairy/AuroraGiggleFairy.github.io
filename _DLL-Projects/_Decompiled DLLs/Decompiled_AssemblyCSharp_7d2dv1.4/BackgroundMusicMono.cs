using System;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicMono : SingletonMonoBehaviour<BackgroundMusicMono>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum MusicTrack
	{
		None,
		BackgroundMusic,
		CreditsSong
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class MusicTrackState
	{
		public readonly AudioSource AudioSource;

		public float CurrentVolume;

		public MusicTrackState(AudioSource audioSource)
		{
			AudioSource = audioSource;
		}
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float secondsToFadeOut = 3f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float secondsToFadeIn = 3f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly EnumDictionary<MusicTrack, MusicTrackState> musicTrackStates = new EnumDictionary<MusicTrack, MusicTrackState>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<MusicTrack> activeTracks = new HashSet<MusicTrack>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MusicTrack currentlyPlaying;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		AudioListener[] array = UnityEngine.Object.FindObjectsOfType<AudioListener>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].enabled)
			{
				base.transform.position = array[i].transform.position;
				break;
			}
		}
		AudioListener.volume = Mathf.Min(GamePrefs.GetFloat(EnumGamePrefs.OptionsOverallAudioVolumeLevel), 1f);
		AddMusicTrack(MusicTrack.None, null);
		AddMusicTrack(MusicTrack.BackgroundMusic, GameManager.Instance.BackgroundMusicClip);
		AddMusicTrack(MusicTrack.CreditsSong, GameManager.Instance.CreditsSongClip);
		Play(MusicTrack.BackgroundMusic);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (GameStats.GetInt(EnumGameStats.GameState) == 1 && !GameManager.Instance.IsPaused())
		{
			Play(MusicTrack.None);
		}
		else if (LocalPlayerUI.primaryUI.windowManager.IsWindowOpen(XUiC_Credits.ID))
		{
			Play(MusicTrack.CreditsSong);
		}
		else
		{
			Play(MusicTrack.BackgroundMusic);
		}
		activeTracks.RemoveWhere([PublicizedFrom(EAccessModifier.Private)] (MusicTrack activeTrack) => !UpdateTrack(activeTrack));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddMusicTrack(MusicTrack musicTrack, AudioClip audioClip)
	{
		if (!audioClip)
		{
			musicTrackStates.Add(musicTrack, new MusicTrackState(null));
			return;
		}
		AudioSource audioSource = base.gameObject.AddComponent<AudioSource>();
		audioSource.volume = 0f;
		audioSource.clip = audioClip;
		audioSource.loop = true;
		musicTrackStates.Add(musicTrack, new MusicTrackState(audioSource));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Play(MusicTrack musicTrack)
	{
		if (currentlyPlaying != musicTrack)
		{
			currentlyPlaying = musicTrack;
			activeTracks.Add(musicTrack);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool UpdateTrack(MusicTrack activeTrack)
	{
		MusicTrackState musicTrackState = musicTrackStates[activeTrack];
		AudioSource audioSource = musicTrackState.AudioSource;
		if (!audioSource)
		{
			return false;
		}
		float currentVolume = musicTrackState.CurrentVolume;
		currentVolume = ((activeTrack != currentlyPlaying) ? (currentVolume - Time.deltaTime / 3f) : (currentVolume + Time.deltaTime / 3f));
		currentVolume = (musicTrackState.CurrentVolume = Mathf.Clamp01(currentVolume));
		bool flag = activeTrack == currentlyPlaying || currentVolume > 0f;
		audioSource.volume = Mathf.Clamp01(GamePrefs.GetFloat(EnumGamePrefs.OptionsMenuMusicVolumeLevel) * currentVolume);
		if (audioSource.isPlaying == flag)
		{
			return flag;
		}
		if (flag)
		{
			audioSource.Play();
		}
		else
		{
			audioSource.Stop();
		}
		return flag;
	}
}
