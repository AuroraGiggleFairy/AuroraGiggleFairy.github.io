using System;

[PublicizedFrom(EAccessModifier.Internal)]
public class AudioBiome
{
	public AudioTrigger[] triggers;

	public AudioBiome()
	{
		int num = Enum.GetNames(typeof(AudioObject.Trigger)).Length;
		triggers = new AudioTrigger[num];
		for (int i = 0; i < num; i++)
		{
			triggers[i] = new AudioTrigger((AudioObject.Trigger)i);
		}
	}

	public void Add(AudioObject _audioObject)
	{
		triggers[(int)_audioObject.trigger].Add(_audioObject);
	}

	public void TransitionFrom(float _biomeTransition)
	{
		for (int i = 3; i < triggers.Length - 1; i++)
		{
			AudioTrigger audioTrigger = triggers[i];
			if (i != 5 && i != 4)
			{
				audioTrigger.TurnOff();
				audioTrigger.SetVolume(1f - _biomeTransition);
			}
			audioTrigger.Update();
		}
	}

	public void TransitionTo(float _biomeTransition)
	{
		for (int i = 3; i < triggers.Length - 1; i++)
		{
			AudioTrigger audioTrigger = triggers[i];
			if (i != 5 && i != 4)
			{
				audioTrigger.SetVolume(_biomeTransition);
			}
			audioTrigger.Update();
		}
	}

	public void Pause()
	{
		for (int i = 0; i < triggers.Length; i++)
		{
			triggers[i].Pause();
		}
	}

	public void UnPause()
	{
		for (int i = 0; i < triggers.Length; i++)
		{
			triggers[i].UnPause();
		}
	}

	public void TurnOff()
	{
		for (int i = 0; i < triggers.Length; i++)
		{
			triggers[i].TurnOff();
		}
	}
}
