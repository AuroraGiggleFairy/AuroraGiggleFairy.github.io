using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentAudioManager : MonoBehaviour, IGamePrefsChangedListener
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cBiomeTransitionSpeed = 0.1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRainVolumeTransitionSpeed = 0.025f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMaxRainVolume = 0.25f;

	public static float GlobalEnvironmentVolumeScale = 0.2f;

	public static float musicVolume = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lightningPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool thunderPlaying;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int thunderTriggerWorldTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float thunderTimer;

	public bool fadingBiomes;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool enteredBiome;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public byte biomeEntered;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float biomeTransition = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float prevBiomeTransition = 1f;

	public float invAmountEnclosedPow = -1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float invAmountEnclosedTarget = -1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float prevAmountEnclosed = -1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float[] prevTriggerValue;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int numBiomes;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int numTriggers;

	public static EnvironmentAudioManager Instance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AudioBiome[] audioBiomes;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition.BiomeType fromBiome;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition.BiomeType toBiome;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition.BiomeType queuedBiome;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AudioBiome fromBiomeLoops;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AudioBiome toBiomeLoops;

	public static GameObject sourceSounds = null;

	public AudioObject[] mixedBiomeSounds;

	public AudioObject[] forestOnlyBiomeSounds;

	public AudioObject[] snowOnlyBiomeSounds;

	public AudioObject[] desertOnlyBiomeSounds;

	public AudioObject[] wastelandOnlyBiomeSounds;

	public AudioObject[] waterOnlyBiomeSounds;

	public AudioObject[] burnt_forestOnlyBiomeSounds;

	public AudioSource rainMasterAudioSource;

	public AudioClip[] rainClipsLowToHigh;

	public List<AudioSource> rainAudioSources;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float[] currentRainVolume = new float[3];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static UnityEngine.Object loadedPrefab = null;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool soundsInitDone;

	public static void DestroyInstance()
	{
		if (Instance != null && Instance.gameObject != null)
		{
			UnityEngine.Object.DestroyImmediate(Instance.gameObject);
		}
	}

	public static IEnumerator CreateNewInstance()
	{
		if (GameManager.IsDedicatedServer)
		{
			yield break;
		}
		DestroyInstance();
		if (loadedPrefab == null)
		{
			LoadManager.AssetRequestTask<GameObject> requestTask = LoadManager.LoadAsset<GameObject>("@:Sounds/Prefabs/EnvironmentAudioMaster.prefab");
			yield return new WaitUntil([PublicizedFrom(EAccessModifier.Internal)] () => requestTask.IsDone);
			loadedPrefab = requestTask.Asset;
		}
		Instance = (UnityEngine.Object.Instantiate(loadedPrefab) as GameObject).GetComponent<EnvironmentAudioManager>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		if (sourceSounds == null)
		{
			sourceSounds = new GameObject();
			sourceSounds.name = "SourceSounds";
			sourceSounds.transform.parent = base.transform;
		}
		InitRain();
		GamePrefs.AddChangeListener(this);
		AmbientAudioController.Instance.SetAmbientVolume(GamePrefs.GetFloat(EnumGamePrefs.OptionsAmbientVolumeLevel));
		musicVolume = GamePrefs.GetFloat(EnumGamePrefs.OptionsMusicVolumeLevel);
		numBiomes = Enum.GetNames(typeof(BiomeDefinition.BiomeType)).Length;
		numTriggers = Enum.GetNames(typeof(AudioObject.Trigger)).Length;
		prevTriggerValue = new float[numTriggers];
		for (int i = 0; i < numTriggers; i++)
		{
			prevTriggerValue[i] = 0f;
		}
		audioBiomes = new AudioBiome[numBiomes];
		for (int j = 0; j < numBiomes; j++)
		{
			audioBiomes[j] = new AudioBiome();
		}
		InitSounds();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitRain()
	{
		for (int i = 0; i < rainClipsLowToHigh.Length; i++)
		{
			AudioSource audioSource = UnityEngine.Object.Instantiate(rainMasterAudioSource);
			audioSource.clip = rainClipsLowToHigh[i];
			audioSource.transform.parent = base.transform;
			audioSource.loop = true;
			rainAudioSources.Add(audioSource);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitSounds()
	{
		AudioObject[] array = new AudioObject[0 + mixedBiomeSounds.Length + forestOnlyBiomeSounds.Length + snowOnlyBiomeSounds.Length + desertOnlyBiomeSounds.Length + wastelandOnlyBiomeSounds.Length + waterOnlyBiomeSounds.Length + burnt_forestOnlyBiomeSounds.Length];
		int num = 0;
		AudioObject[] array2 = mixedBiomeSounds;
		foreach (AudioObject audioObject in array2)
		{
			array[num++] = audioObject;
		}
		array2 = forestOnlyBiomeSounds;
		foreach (AudioObject audioObject2 in array2)
		{
			array[num++] = audioObject2;
		}
		array2 = snowOnlyBiomeSounds;
		foreach (AudioObject audioObject3 in array2)
		{
			array[num++] = audioObject3;
		}
		array2 = desertOnlyBiomeSounds;
		foreach (AudioObject audioObject4 in array2)
		{
			array[num++] = audioObject4;
		}
		array2 = wastelandOnlyBiomeSounds;
		foreach (AudioObject audioObject5 in array2)
		{
			array[num++] = audioObject5;
		}
		array2 = waterOnlyBiomeSounds;
		foreach (AudioObject audioObject6 in array2)
		{
			array[num++] = audioObject6;
		}
		array2 = burnt_forestOnlyBiomeSounds;
		foreach (AudioObject audioObject7 in array2)
		{
			array[num++] = audioObject7;
		}
		int num2 = 0;
		array2 = array;
		foreach (AudioObject audioObject8 in array2)
		{
			BiomeDefinition.BiomeType[] validBiomes = audioObject8.validBiomes;
			foreach (BiomeDefinition.BiomeType biomeType in validBiomes)
			{
				audioBiomes[(int)biomeType].Add(audioObject8);
				if (audioObject8.trigger == AudioObject.Trigger.Day7Times || audioObject8.trigger == AudioObject.Trigger.TimeOfDay)
				{
					num2++;
				}
			}
			audioObject8.Init();
		}
		fromBiomeLoops = audioBiomes[(int)fromBiome];
		toBiomeLoops = audioBiomes[(int)toBiome];
		soundsInitDone = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FixedUpdate()
	{
		if (GameManager.Instance.IsPaused() || !soundsInitDone)
		{
			return;
		}
		World world = GameManager.Instance.World;
		if (world == null)
		{
			TurnOffSounds();
			UpdateRainAudio();
			return;
		}
		float deltaTime = Time.deltaTime;
		prevBiomeTransition = biomeTransition;
		biomeTransition += 0.1f * deltaTime;
		biomeTransition = Mathf.Clamp01(biomeTransition);
		fadingBiomes = false;
		if (prevBiomeTransition != biomeTransition)
		{
			fromBiomeLoops.TransitionFrom(biomeTransition);
		}
		toBiomeLoops.TransitionTo(biomeTransition);
		invAmountEnclosedPow = Mathf.Lerp(invAmountEnclosedPow, invAmountEnclosedTarget, deltaTime * 1.5f);
		UpdateRainAudio();
		UpdateValueTrigger(WeatherManager.Instance.GetCurrentSnowfallPercent(), AudioObject.Trigger.Snow);
		UpdateValueTrigger(Mathf.Clamp01(WeatherManager.currentWeather.Wind() * 0.01f + 0.12f), AudioObject.Trigger.Wind);
		if (thunderPlaying)
		{
			List<AudioObject> sound = toBiomeLoops.triggers[2].sound;
			if (sound.Count > 0)
			{
				AudioObject audioObject = sound[0];
				audioObject.SetVolume(invAmountEnclosedPow);
				if (!(thunderPlaying = audioObject.IsPlaying()))
				{
					audioObject.DestroySources();
				}
			}
		}
		if (thunderTriggerWorldTime > 0)
		{
			List<AudioObject> sound2 = toBiomeLoops.triggers[2].sound;
			if (sound2.Count > 0 && (int)world.GetWorldTime() > thunderTriggerWorldTime)
			{
				thunderTriggerWorldTime = 0;
				if (world.GetPrimaryPlayer() != null)
				{
					sound2[0].SetPosition(lightningPos);
					SkyManager.TriggerLightning(lightningPos);
					thunderTimer = Time.time + world.RandomRange(200f, 1000f) / 343f;
				}
			}
		}
		if (Time.time > thunderTimer)
		{
			thunderTimer = float.PositiveInfinity;
			List<AudioObject> sound3 = toBiomeLoops.triggers[2].sound;
			for (int i = 0; i < sound3.Count; i++)
			{
				AudioObject audioObject2 = sound3[i];
				thunderPlaying = true;
				audioObject2.Play();
			}
		}
		if (fadingBiomes)
		{
			return;
		}
		if (enteredBiome)
		{
			if (!AudioObject.biomeIdMap.TryGetValue(biomeEntered, out var value))
			{
				return;
			}
			if (biomeTransition != 1f)
			{
				queuedBiome = value;
			}
			else
			{
				SetNewBiome(value);
			}
			enteredBiome = false;
		}
		if (queuedBiome != BiomeDefinition.BiomeType.Any && biomeTransition >= 1f)
		{
			SetNewBiome(queuedBiome);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateRainAudio()
	{
		float currentRainfallPercent = WeatherManager.Instance.GetCurrentRainfallPercent();
		float num = Time.deltaTime * 0.025f;
		if (currentRainfallPercent <= 0f)
		{
			IncrementRainVolumes(0f - num, 0f - num, 0f - num);
		}
		else if (currentRainfallPercent < 0.28f)
		{
			IncrementRainVolumes(num, 0f - num, 0f - num);
		}
		else if (currentRainfallPercent < 0.56f)
		{
			IncrementRainVolumes(0f - num, num, 0f - num);
		}
		else
		{
			IncrementRainVolumes(0f - num, 0f - num, num);
		}
		foreach (AudioSource rainAudioSource in rainAudioSources)
		{
			if (rainAudioSource.volume == 0f)
			{
				rainAudioSource.Stop();
			}
			else if (!rainAudioSource.isPlaying)
			{
				rainAudioSource.Play();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void IncrementRainVolumes(float inc0, float inc1, float inc2)
	{
		float num = 0.25f * invAmountEnclosedPow * GlobalEnvironmentVolumeScale;
		currentRainVolume[0] = Utils.FastClamp01(currentRainVolume[0] + inc0);
		rainAudioSources[0].volume = Utils.FastClamp01(currentRainVolume[0] * num);
		currentRainVolume[1] = Utils.FastClamp01(currentRainVolume[1] + inc1);
		rainAudioSources[1].volume = Utils.FastClamp01(currentRainVolume[1] * num);
		currentRainVolume[2] = Utils.FastClamp01(currentRainVolume[2] + inc2);
		rainAudioSources[2].volume = Utils.FastClamp01(currentRainVolume[2] * num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateValueTrigger(float value, AudioObject.Trigger trigger)
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer == null || primaryPlayer.Stats == null || (biomeTransition == prevBiomeTransition && prevTriggerValue[(int)trigger] == value && prevAmountEnclosed == primaryPlayer.Stats.AmountEnclosed))
		{
			return;
		}
		prevTriggerValue[(int)trigger] = value;
		if (biomeTransition < 1f || prevBiomeTransition < 1f)
		{
			foreach (AudioObject item in fromBiomeLoops.triggers[(int)trigger].sound)
			{
				item.SetBiomeVolume(1f - biomeTransition);
				item.SetValue(value);
			}
		}
		foreach (AudioObject item2 in toBiomeLoops.triggers[(int)trigger].sound)
		{
			item2.SetBiomeVolume(biomeTransition);
			item2.SetValue(value);
		}
		prevAmountEnclosed = primaryPlayer.Stats.AmountEnclosed;
		invAmountEnclosedTarget = Mathf.Pow(1f - prevAmountEnclosed, 2f);
		if (invAmountEnclosedPow < 0f)
		{
			invAmountEnclosedPow = invAmountEnclosedTarget;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetNewBiome(BiomeDefinition.BiomeType newBiome)
	{
		if (audioBiomes != null)
		{
			fromBiome = toBiome;
			toBiome = newBiome;
			queuedBiome = BiomeDefinition.BiomeType.Any;
			biomeTransition = 0f;
			fromBiomeLoops = audioBiomes[(int)fromBiome];
			toBiomeLoops = audioBiomes[(int)toBiome];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		for (int i = 0; i < rainAudioSources.Count; i++)
		{
			if (!(rainAudioSources[i] == null))
			{
				rainAudioSources[i].Stop();
				GameObject gameObject = rainAudioSources[i].transform.gameObject;
				UnityEngine.Object.DestroyImmediate(rainAudioSources[i]);
				if (gameObject != null)
				{
					UnityEngine.Object.DestroyImmediate(gameObject);
				}
			}
		}
		rainAudioSources.Clear();
		TurnOffSounds();
		fromBiomeLoops = null;
		toBiomeLoops = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TurnOffSounds()
	{
		fromBiomeLoops.TurnOff();
		toBiomeLoops.TurnOff();
	}

	public void Pause()
	{
		toBiomeLoops.Pause();
		fromBiomeLoops.Pause();
		for (int i = 0; i < rainAudioSources.Count; i++)
		{
			rainAudioSources[i].Pause();
		}
	}

	public void UnPause()
	{
		toBiomeLoops.UnPause();
		fromBiomeLoops.UnPause();
		for (int i = 0; i < rainAudioSources.Count; i++)
		{
			rainAudioSources[i].UnPause();
		}
	}

	public void TriggerThunder(Vector3 _pos)
	{
		lightningPos = _pos;
		thunderTriggerWorldTime = 1;
		thunderTimer = float.PositiveInfinity;
	}

	public void EnterBiome(BiomeDefinition _biome)
	{
		if (AudioObject.biomeIdMap == null)
		{
			AudioObject.biomeIdMap = new Dictionary<byte, BiomeDefinition.BiomeType>();
			foreach (KeyValuePair<string, byte> item in BiomeDefinition.nameToId)
			{
				for (int i = 0; i < BiomeDefinition.BiomeNames.Length; i++)
				{
					if (item.Key.EqualsCaseInsensitive(BiomeDefinition.BiomeNames[i]))
					{
						AudioObject.biomeIdMap[item.Value] = (BiomeDefinition.BiomeType)i;
						break;
					}
				}
			}
		}
		if (_biome != null)
		{
			enteredBiome = true;
			biomeEntered = _biome.m_Id;
		}
	}

	public void OnGamePrefChanged(EnumGamePrefs _enum)
	{
		if (_enum == EnumGamePrefs.OptionsMusicVolumeLevel)
		{
			musicVolume = GamePrefs.GetFloat(EnumGamePrefs.OptionsMusicVolumeLevel);
		}
	}
}
