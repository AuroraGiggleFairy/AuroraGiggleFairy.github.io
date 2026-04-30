using System.Collections.Generic;
using UnityEngine;

namespace Audio;

public class XmlData
{
	public enum Channel
	{
		Mouth,
		Environment
	}

	public string soundGroupName;

	public int maxVoices;

	public List<ClipSourceMap> audioClipMap;

	public List<ClipSourceMap> altAudioClipMap;

	public List<ClipSourceMap> cleanClipMap;

	public NoiseData noiseData;

	public float localCrouchVolumeScale;

	public float runningVolumeScale;

	public float crouchNoiseScale;

	public float noiseScale;

	public float maxRepeatRate;

	public int voicesPlaying;

	public float lastRecordedPlayTime;

	public bool playImmediate;

	public bool sequence;

	public float maxVolume;

	public float lowestPitch;

	public float highestPitch;

	public float distantFadeStart;

	public float distantFadeEnd;

	public int maxVoicesPerEntity;

	public bool hasProfanity;

	public Channel channel;

	public int priority;

	public bool vibratesController = true;

	public float vibrationStrengthMultiplier = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int randomLastIndex;

	public XmlData()
	{
		soundGroupName = "Invalid";
		maxVoices = 1;
		maxVoicesPerEntity = 5;
		audioClipMap = new List<ClipSourceMap>();
		noiseData = new NoiseData();
		localCrouchVolumeScale = 0.5f;
		crouchNoiseScale = 0.5f;
		noiseScale = 1f;
		maxRepeatRate = 0.1f;
		voicesPlaying = 0;
		lastRecordedPlayTime = Time.time;
		maxVolume = 1f;
		sequence = false;
		runningVolumeScale = 1f;
		lowestPitch = 1f;
		highestPitch = 1f;
		distantFadeStart = -1f;
		distantFadeEnd = -1f;
		channel = Channel.Environment;
		priority = 99;
	}

	public bool Update()
	{
		if (maxRepeatRate > 0f)
		{
			float time = Time.time;
			float num = time - lastRecordedPlayTime;
			if (num < maxRepeatRate)
			{
				return false;
			}
			voicesPlaying = Utils.FastClamp(voicesPlaying - (int)(num / maxRepeatRate), 0, 999);
			if (voicesPlaying >= maxVoices)
			{
				return false;
			}
			voicesPlaying++;
			lastRecordedPlayTime = time;
		}
		return true;
	}

	public List<ClipSourceMap> GetClipList()
	{
		if (Manager.Instance.bUseAltSounds && altAudioClipMap != null)
		{
			return altAudioClipMap;
		}
		if (hasProfanity && GamePrefs.GetBool(EnumGamePrefs.OptionsFilterProfanity))
		{
			return cleanClipMap;
		}
		return audioClipMap;
	}

	public ClipSourceMap GetRandomClip()
	{
		List<ClipSourceMap> clipList = GetClipList();
		int num = 0;
		int count = clipList.Count;
		if (count > 1)
		{
			if (count == 2)
			{
				num = randomLastIndex ^ 1;
			}
			else
			{
				num = Manager.random.RandomRange(count - 1);
				if (num >= randomLastIndex)
				{
					num++;
				}
			}
			randomLastIndex = num;
		}
		else if (count == 0)
		{
			Log.Warning("No Clips in Audio ClipSourceMap " + soundGroupName + ", " + ((hasProfanity && GamePrefs.GetBool(EnumGamePrefs.OptionsFilterProfanity)) ? "using 'no profanity' map:" : ""));
			return null;
		}
		return clipList[num];
	}

	public void AddAltClipSourceMap(ClipSourceMap csm)
	{
		if (altAudioClipMap == null)
		{
			altAudioClipMap = new List<ClipSourceMap>();
		}
		altAudioClipMap.Add(csm);
	}
}
