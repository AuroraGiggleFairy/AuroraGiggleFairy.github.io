using System.Collections.Generic;
using UnityEngine;

[PublicizedFrom(EAccessModifier.Internal)]
public class AudioTrigger
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly AudioObject.Trigger trigger;

	public List<AudioObject> sound = new List<AudioObject>();

	public AudioTrigger(AudioObject.Trigger _trigger)
	{
		trigger = _trigger;
	}

	public void Add(AudioObject _audioObject)
	{
		sound.Add(_audioObject);
	}

	public void Update()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		for (int num = sound.Count - 1; num >= 0; num--)
		{
			sound[num].Update(fixedDeltaTime);
		}
	}

	public void SetVolume(float _vol)
	{
		for (int num = sound.Count - 1; num >= 0; num--)
		{
			sound[num].SetBiomeVolume(_vol);
		}
	}

	public void Pause()
	{
		for (int num = sound.Count - 1; num >= 0; num--)
		{
			sound[num].Pause();
		}
	}

	public void UnPause()
	{
		for (int num = sound.Count - 1; num >= 0; num--)
		{
			sound[num].UnPause();
		}
	}

	public void TurnOff()
	{
		for (int num = sound.Count - 1; num >= 0; num--)
		{
			sound[num].TurnOff();
		}
	}
}
