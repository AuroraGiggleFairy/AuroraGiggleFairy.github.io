using UnityEngine;

public class AudioGamepadRumbleSource
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSampleCount = 64;

	public AudioSource audioSrc;

	public float[] samples;

	public float strengthMultiplier;

	public bool locationBased;

	public float timeAdded;

	public AudioGamepadRumbleSource()
	{
		samples = new float[64];
	}

	public void SetAudioSource(AudioSource _audioSource, float _strengthMultiplier, bool _locationBased)
	{
		audioSrc = _audioSource;
		strengthMultiplier = _strengthMultiplier;
		locationBased = _locationBased;
		timeAdded = Time.time;
	}

	public float GetSample(int channel)
	{
		audioSrc.GetOutputData(samples, channel);
		float num = 0f;
		for (int i = 0; i < 64; i++)
		{
			num += samples[i];
		}
		return num / 64f;
	}

	public void Clear()
	{
		audioSrc = null;
	}
}
