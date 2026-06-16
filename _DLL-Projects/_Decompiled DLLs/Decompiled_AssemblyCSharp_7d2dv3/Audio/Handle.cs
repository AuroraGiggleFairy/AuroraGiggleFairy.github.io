using UnityEngine;

namespace Audio;

public class Handle
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioSource nearSource;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioSource farSource;

	[PublicizedFrom(EAccessModifier.Private)]
	public float basePitch;

	[PublicizedFrom(EAccessModifier.Private)]
	public float baseVolume;

	public Handle(string soundGroupName, AudioSource near, AudioSource far)
	{
		name = soundGroupName;
		nearSource = near;
		farSource = far;
		if ((bool)nearSource)
		{
			basePitch = nearSource.pitch;
			baseVolume = nearSource.volume;
		}
	}

	public void SetPitch(float pitch)
	{
		if ((bool)nearSource)
		{
			nearSource.pitch = pitch + basePitch;
		}
		if ((bool)farSource)
		{
			farSource.pitch = pitch + basePitch;
		}
	}

	public void SetVolume(float volume)
	{
		if ((bool)nearSource)
		{
			nearSource.volume = volume * baseVolume;
		}
		if ((bool)farSource)
		{
			farSource.volume = volume * baseVolume;
		}
	}

	public void Stop(int entityId)
	{
		Manager.Stop(entityId, name);
	}

	public float ClipLength()
	{
		if ((bool)nearSource)
		{
			return nearSource.clip.length;
		}
		if ((bool)farSource)
		{
			return farSource.clip.length;
		}
		return 0f;
	}

	public bool IsPlaying()
	{
		bool flag = false;
		if (nearSource != null)
		{
			flag = nearSource.isPlaying;
		}
		if (farSource != null)
		{
			flag |= farSource.isPlaying;
		}
		return flag;
	}
}
