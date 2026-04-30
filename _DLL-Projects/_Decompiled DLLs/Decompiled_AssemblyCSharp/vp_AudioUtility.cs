using System.Collections.Generic;
using UnityEngine;

public static class vp_AudioUtility
{
	public static void PlayRandomSound(AudioSource audioSource, List<AudioClip> sounds, Vector2 pitchRange)
	{
		if (audioSource == null || sounds == null || sounds.Count == 0)
		{
			return;
		}
		AudioClip audioClip = sounds[Random.Range(0, sounds.Count)];
		if (!(audioClip == null))
		{
			if (pitchRange == Vector2.one)
			{
				audioSource.pitch = Time.timeScale;
			}
			else
			{
				audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y) * Time.timeScale;
			}
			audioSource.PlayOneShot(audioClip);
		}
	}

	public static void PlayRandomSound(AudioSource audioSource, List<AudioClip> sounds)
	{
		PlayRandomSound(audioSource, sounds, Vector2.one);
	}
}
