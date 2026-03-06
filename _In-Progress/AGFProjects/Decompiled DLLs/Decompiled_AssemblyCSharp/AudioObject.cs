using System;
using System.Collections.Generic;
using Audio;
using MusicUtils.Enums;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class AudioObject
{
	public enum Trigger
	{
		Rain,
		Snow,
		Thunder,
		TimeOfDay,
		Dusk,
		Dawn,
		Day7Times,
		Day7Dusk,
		Day8Dawn,
		Random,
		Continual,
		Wind
	}

	public enum PlayOrder
	{
		Random,
		FirstToLast,
		ByValue
	}

	public static Dictionary<byte, BiomeDefinition.BiomeType> biomeIdMap;

	public string name;

	public AudioMixerGroup audioMixerGroup;

	public AudioSource masterAudioSource;

	public AudioClip[] audioClips;

	public List<AudioSource> runtimeAudioSrcs = new List<AudioSource>();

	public Trigger trigger;

	public bool indoorOnly;

	public bool outdoorOnly;

	public bool dayOnly;

	public bool nightOnly;

	public float duskOffset;

	public float dawnOffset;

	public float minWind;

	public bool affectedByEnv;

	public PlayOrder playOrder;

	public BiomeDefinition.BiomeType[] validBiomes;

	public AnimationCurve fadeInSec = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public AnimationCurve transitionCurve;

	public Vector2 repeatFreqRange;

	public float loopDuration;

	public bool music;

	public ThreatLevelType[] validThreatLevels;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AudioSource currentAudioSrc;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float loopTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float repeatTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool fadingOut;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float value;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float playTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float biomeVolume = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int currentPlayNum;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 playAtPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float timeEpsilon = 0.01f;

	public void Init()
	{
		AudioClip[] array = audioClips;
		foreach (AudioClip audioClip in array)
		{
			if (audioClip != null)
			{
				AudioSource audioSource = UnityEngine.Object.Instantiate(masterAudioSource);
				if (playOrder == PlayOrder.ByValue)
				{
					audioSource.transform.parent = EnvironmentAudioManager.Instance.transform;
					audioSource.loop = true;
					audioSource.gameObject.SetActive(value: false);
				}
				else
				{
					audioSource.transform.parent = EnvironmentAudioManager.sourceSounds.transform;
				}
				audioSource.clip = audioClip;
				audioSource.name = audioClip.name;
				audioSource.volume = 0f;
				audioSource.outputAudioMixerGroup = audioMixerGroup;
				runtimeAudioSrcs.Add(audioSource);
			}
		}
		audioClips = null;
		if (trigger == Trigger.Random)
		{
			repeatTime = Time.time + Manager.random.RandomRange(repeatFreqRange.x, repeatFreqRange.y);
		}
	}

	public void SetValue(float _value)
	{
		value = _value;
		if (playOrder != PlayOrder.ByValue)
		{
			return;
		}
		float num = transitionCurve.Evaluate(value) * (float)runtimeAudioSrcs.Count;
		int num2 = 0;
		foreach (AudioSource runtimeAudioSrc in runtimeAudioSrcs)
		{
			float num3 = Mathf.Clamp01(num - (float)num2);
			if (num > (float)(num2 + 1))
			{
				num3 = 1f - Mathf.Clamp01(num - (float)(num2 + 1));
			}
			runtimeAudioSrc.volume = num3 * (music ? EnvironmentAudioManager.musicVolume : 1f) * biomeVolume * EnvironmentAudioManager.GlobalEnvironmentVolumeScale;
			if (GameManager.Instance != null && GameManager.Instance.World != null && GameManager.Instance.World.GetPrimaryPlayer() != null && GameManager.Instance.World.GetPrimaryPlayer().Stats != null)
			{
				if (outdoorOnly)
				{
					runtimeAudioSrc.volume *= EnvironmentAudioManager.Instance.invAmountEnclosedPow;
				}
				else if (indoorOnly)
				{
					runtimeAudioSrc.volume *= 1f - EnvironmentAudioManager.Instance.invAmountEnclosedPow;
				}
			}
			runtimeAudioSrc.gameObject.SetActive(runtimeAudioSrc.volume > 0f);
			if (runtimeAudioSrc.volume > 0f && !runtimeAudioSrc.isPlaying)
			{
				if (!runtimeAudioSrc.isActiveAndEnabled)
				{
					runtimeAudioSrc.gameObject.SetActive(value: true);
				}
				runtimeAudioSrc.Play();
			}
			num2++;
		}
	}

	public void SetBiomeVolume(float _volume)
	{
		biomeVolume = _volume;
	}

	public void Pause()
	{
		if (currentAudioSrc != null)
		{
			currentAudioSrc.Pause();
		}
		if (playOrder != PlayOrder.ByValue)
		{
			return;
		}
		foreach (AudioSource runtimeAudioSrc in runtimeAudioSrcs)
		{
			if (runtimeAudioSrc.isPlaying)
			{
				runtimeAudioSrc.Pause();
			}
		}
	}

	public void UnPause()
	{
		if (currentAudioSrc != null)
		{
			currentAudioSrc.UnPause();
		}
		if (playOrder != PlayOrder.ByValue)
		{
			return;
		}
		foreach (AudioSource runtimeAudioSrc in runtimeAudioSrcs)
		{
			if (runtimeAudioSrc.volume > 0f)
			{
				runtimeAudioSrc.UnPause();
			}
		}
	}

	public void TurnOff(bool immediate = false)
	{
		fadingOut = true;
		if (!immediate)
		{
			return;
		}
		if (currentAudioSrc != null)
		{
			currentAudioSrc.Stop();
		}
		if (playOrder == PlayOrder.ByValue)
		{
			foreach (AudioSource runtimeAudioSrc in runtimeAudioSrcs)
			{
				runtimeAudioSrc.Stop();
			}
		}
		DestroySources();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool PlayConditionPasses()
	{
		World world = GameManager.Instance.World;
		EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
		if (primaryPlayer == null || primaryPlayer.Stats == null || primaryPlayer.IsDead())
		{
			return false;
		}
		if (WeatherManager.currentWeather == null)
		{
			return false;
		}
		if (WeatherManager.currentWeather.Wind() < minWind)
		{
			return false;
		}
		float num = (float)world.GetWorldTime() / 24000f;
		float num2 = (num - (float)(int)num) * 24f;
		float num3 = SkyManager.GetDawnTime() + dawnOffset;
		float num4 = SkyManager.GetDuskTime() + duskOffset;
		bool flag = SkyManager.IsBloodMoonVisible();
		if (outdoorOnly && primaryPlayer.Stats.AmountEnclosed >= 1f)
		{
			return false;
		}
		if (indoorOnly && primaryPlayer.Stats.AmountEnclosed <= 0f)
		{
			return false;
		}
		if (dayOnly)
		{
			if (num2 < 12f && num2 < num3 - 0.02f)
			{
				return false;
			}
			if (num2 > 12f && num2 > num4 + 0.02f)
			{
				return false;
			}
		}
		if (nightOnly)
		{
			if (num2 < 12f && num2 > num3 + 0.02f)
			{
				return false;
			}
			if (num2 > 12f && num2 < num4 - 0.02f)
			{
				return false;
			}
		}
		bool flag2 = false;
		ThreatLevelType category = primaryPlayer.ThreatLevel.Category;
		for (int i = 0; i < validThreatLevels.Length; i++)
		{
			if (validThreatLevels[i] == category)
			{
				flag2 = true;
				break;
			}
		}
		if (!flag2)
		{
			return false;
		}
		switch (trigger)
		{
		case Trigger.Dusk:
		{
			bool flag3 = EffectManager.GetValue(PassiveEffects.NoTimeDisplay, null, 0f, primaryPlayer) == 1f;
			if (num4 > num2 + 0.01f || num4 < num2 - 0.01f || flag || flag3)
			{
				return false;
			}
			break;
		}
		case Trigger.Dawn:
		{
			bool flag4 = EffectManager.GetValue(PassiveEffects.NoTimeDisplay, null, 0f, primaryPlayer) == 1f;
			if (num3 > num2 + 0.01f || num3 < num2 - 0.01f || flag4)
			{
				return false;
			}
			break;
		}
		case Trigger.Day7Dusk:
			if (!flag)
			{
				return false;
			}
			if (num4 > num2 + 0.01f || num4 < num2 - 0.01f)
			{
				return false;
			}
			break;
		case Trigger.Day8Dawn:
			if (SkyManager.dayCount - (float)(8 * ((int)SkyManager.dayCount / 8)) >= 1f)
			{
				return false;
			}
			if (num3 > num2 + 0.01f || num3 < num2 - 0.01f)
			{
				return false;
			}
			break;
		case Trigger.Random:
			if ((num2 > num4 - 0.25f && num2 < num4 + 0.25f) || (num2 > num3 - 0.25f && num2 < num3 + 0.25f))
			{
				return false;
			}
			if (world.dmsConductor != null && world.dmsConductor.IsMusicPlaying)
			{
				return false;
			}
			if (Time.time < repeatTime)
			{
				return false;
			}
			break;
		}
		return true;
	}

	public void DestroySources()
	{
		if (playOrder == PlayOrder.ByValue)
		{
			foreach (AudioSource runtimeAudioSrc in runtimeAudioSrcs)
			{
				if (runtimeAudioSrc != null)
				{
					UnityEngine.Object.DestroyImmediate(runtimeAudioSrc.gameObject);
				}
			}
			runtimeAudioSrcs.Clear();
		}
		if (currentAudioSrc != null)
		{
			UnityEngine.Object.DestroyImmediate(currentAudioSrc.gameObject);
			currentAudioSrc = null;
		}
	}

	public void SetVolume(float volume)
	{
		if (currentAudioSrc != null)
		{
			currentAudioSrc.volume = volume;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayAtPoint()
	{
		if (currentAudioSrc != null)
		{
			if (playAtPosition != Vector3.zero)
			{
				currentAudioSrc.gameObject.transform.position = playAtPosition - Origin.position;
			}
			currentAudioSrc.Play();
		}
	}

	public void Play()
	{
		int index = 0;
		switch (playOrder)
		{
		case PlayOrder.Random:
			index = Manager.random.RandomRange(runtimeAudioSrcs.Count);
			break;
		case PlayOrder.FirstToLast:
			currentPlayNum = 1;
			break;
		}
		if ((bool)currentAudioSrc)
		{
			UnityEngine.Object.DestroyImmediate(currentAudioSrc.gameObject);
		}
		AudioSource original = runtimeAudioSrcs[index];
		currentAudioSrc = UnityEngine.Object.Instantiate(original);
		GameObject gameObject = currentAudioSrc.gameObject;
		gameObject.name = name;
		gameObject.transform.SetParent(EnvironmentAudioManager.Instance.transform, worldPositionStays: false);
		gameObject.SetActive(value: true);
		currentAudioSrc.volume = ((trigger == Trigger.Thunder) ? 1 : 0);
		if (!currentAudioSrc.isPlaying)
		{
			PlayAtPoint();
		}
		if (trigger == Trigger.Continual)
		{
			currentAudioSrc.loop = true;
		}
	}

	public bool IsPlaying()
	{
		if (currentAudioSrc == null)
		{
			return false;
		}
		return currentAudioSrc.isPlaying;
	}

	public void SetPosition(Vector3 _position)
	{
		playAtPosition = _position;
	}

	public void Update(float deltaTime)
	{
		if (runtimeAudioSrcs.Count == 0)
		{
			return;
		}
		if (currentAudioSrc != null)
		{
			if (playOrder != PlayOrder.ByValue)
			{
				if (playOrder != PlayOrder.FirstToLast || currentPlayNum == 1 || fadingOut)
				{
					playTime += (fadingOut ? ((0f - deltaTime) * 0.02f) : deltaTime);
					float num = fadeInSec.Evaluate(playTime);
					float time = fadeInSec[fadeInSec.length - 1].time;
					if (playTime >= time)
					{
						playTime = time;
					}
					float num2 = num * biomeVolume * EnvironmentAudioManager.GlobalEnvironmentVolumeScale;
					if (!name.Contains("Stinger"))
					{
						num2 *= (music ? EnvironmentAudioManager.musicVolume : 1f);
					}
					if (!fadingOut)
					{
						if (num2 == 0f)
						{
							num2 = 0.001f;
						}
					}
					else if (num2 < 0.01f)
					{
						num2 = 0f;
					}
					else
					{
						EnvironmentAudioManager.Instance.fadingBiomes = true;
					}
					currentAudioSrc.volume = num2;
				}
				else
				{
					currentAudioSrc.volume = (music ? EnvironmentAudioManager.musicVolume : 1f) * biomeVolume * EnvironmentAudioManager.GlobalEnvironmentVolumeScale;
				}
				currentAudioSrc.gameObject.SetActive(currentAudioSrc.volume > 0f && currentAudioSrc.isPlaying);
				if (playOrder == PlayOrder.FirstToLast && currentPlayNum < runtimeAudioSrcs.Count && (currentAudioSrc.volume == 0f || !currentAudioSrc.isPlaying))
				{
					UnityEngine.Object.DestroyImmediate(currentAudioSrc.gameObject);
					currentAudioSrc = null;
					currentAudioSrc = UnityEngine.Object.Instantiate(runtimeAudioSrcs[currentPlayNum++]);
					currentAudioSrc.transform.parent = EnvironmentAudioManager.Instance.transform;
					currentAudioSrc.gameObject.name = name;
					currentAudioSrc.name = name;
					currentAudioSrc.gameObject.SetActive(value: true);
					currentAudioSrc.volume = EnvironmentAudioManager.GlobalEnvironmentVolumeScale * (music ? EnvironmentAudioManager.musicVolume : 1f) * biomeVolume;
					if (!currentAudioSrc.isPlaying)
					{
						PlayAtPoint();
					}
				}
			}
			if (outdoorOnly)
			{
				currentAudioSrc.volume *= EnvironmentAudioManager.Instance.invAmountEnclosedPow;
			}
			else if (indoorOnly)
			{
				currentAudioSrc.volume *= 1f - EnvironmentAudioManager.Instance.invAmountEnclosedPow;
			}
			if (loopDuration > 0f && Time.time > loopTime + loopDuration)
			{
				TurnOff();
			}
			float num3 = (float)GameManager.Instance.World.GetWorldTime() / 24000f;
			float num4 = (num3 - (float)(int)num3) * 24f;
			float num5 = SkyManager.GetDawnTime() + dawnOffset;
			float num6 = SkyManager.GetDuskTime() + duskOffset;
			if (dayOnly && num4 < 12f && num4 < num5 - 0.02f)
			{
				TurnOff();
			}
			if (dayOnly && num4 > 12f && num4 > num6 + 0.02f)
			{
				TurnOff();
			}
			if (nightOnly && num4 < 12f && num4 > num5 + 0.02f)
			{
				TurnOff();
			}
			if (nightOnly && num4 > 12f && num4 < num6 - 0.02f)
			{
				TurnOff();
			}
			if (playOrder == PlayOrder.ByValue)
			{
				return;
			}
			if (trigger == Trigger.Continual && currentAudioSrc != null && !currentAudioSrc.isPlaying && currentAudioSrc.volume > 0f)
			{
				if (!currentAudioSrc.isActiveAndEnabled)
				{
					currentAudioSrc.gameObject.SetActive(value: true);
				}
				if (currentAudioSrc.isActiveAndEnabled)
				{
					PlayAtPoint();
				}
			}
			if (!(currentAudioSrc != null) || !currentAudioSrc.isPlaying || (fadingOut && !(currentAudioSrc.volume > 0f)))
			{
				UnityEngine.Object.DestroyImmediate(currentAudioSrc.gameObject);
				currentAudioSrc = null;
				if (trigger == Trigger.Random)
				{
					repeatTime = Time.time + Manager.random.RandomRange(repeatFreqRange.x, repeatFreqRange.y);
				}
			}
		}
		else if (PlayConditionPasses())
		{
			DestroySources();
			value = 0f;
			fadingOut = false;
			playTime = 0f;
			loopTime = Time.time;
			Play();
		}
	}
}
