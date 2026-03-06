using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Audio;

public class Manager : IDisposable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct SequenceGOs
	{
		public GameObject nearStart;

		public GameObject nearLoop;

		public GameObject nearEnd;

		public GameObject farStart;

		public GameObject farLoop;

		public GameObject farEnd;

		public float longestClipLength;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct NearAndFarGO
	{
		public GameObject near;

		public GameObject far;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct Channels
	{
		public int currentMouthPriority;

		public List<AudioSource> mouth;

		public DictionaryList<string, List<AudioSource>> environment;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct DopplerItem
	{
		public AudioSource src;

		public float doppler;
	}

	public struct AudioSourceData
	{
		public float maxVolume;
	}

	public class SequenceStopper
	{
		public List<GameObject> sequenceObjs;

		public float stopTime;

		public SequenceStopper(List<GameObject> audioSourceObjs, float _stopTime)
		{
			sequenceObjs = audioSourceObjs;
			stopTime = _stopTime;
		}
	}

	public static Dictionary<string, AudioClip> audioClipAssetCache = new Dictionary<string, AudioClip>();

	public static Dictionary<string, GameObject> audioSrcObjAssetCache = new Dictionary<string, GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, bool> ignoreDistanceCheckSounds = new Dictionary<string, bool>();

	public static Dictionary<string, SubtitleData> subtitleCache = new Dictionary<string, SubtitleData>();

	public static Dictionary<string, string> subtitleSpeakerColorCache = new Dictionary<string, string>();

	public static GameRandom random;

	public static bool occlusionsOn;

	public const float OccludedVolumeMultiplier = 0.5f;

	public static Server ServerAudio;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool bCameraWasUnderwater;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int underwaterSoundID = -1;

	public static Vector3 currentListenerPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float fadeOutUpdateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject PositionalSoundsPlaying;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform PositionalSoundsPlayingT;

	public bool bUseAltSounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Manager instance;

	public static Dictionary<string, XmlData> audioData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<int, Dictionary<string, NearAndFarGO>> loopingOnEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<int, Dictionary<string, NearAndFarGO>> fadingOutOnEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<Vector3, Dictionary<string, NearAndFarGO>> loopingOnPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<int, Dictionary<string, SequenceGOs>> sequenceOnEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<int, Dictionary<string, SequenceStopper>> stoppedEntitySequences;

	public static List<AudioSource> playingAudioSources;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<int, Channels> playingOnEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int dopplerDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<DopplerItem> playingAudioSourceDopplers;

	public static Dictionary<string, AudioSourceData> audioSourceDatas;

	[PublicizedFrom(EAccessModifier.Private)]
	public static EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDopplerDist = 20f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPlayerPitchShift = 0.05f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static AudioSource uniqueSrc;

	[PublicizedFrom(EAccessModifier.Private)]
	public static char[] convertChars = new char[2] { '*', '#' };

	[PublicizedFrom(EAccessModifier.Private)]
	public const int raycastMask = 65537;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<int> listIntRemove = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<string> listStringRemove = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<string> removeSequenceStopper = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Camera mainCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 cameraPos;

	public static Manager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new Manager();
			}
			return instance;
		}
	}

	public Manager()
	{
		PositionalSoundsPlaying = new GameObject("PositionalSoundsPlaying");
		PositionalSoundsPlayingT = PositionalSoundsPlaying.transform;
		Origin.Add(PositionalSoundsPlayingT, 0);
	}

	public static void Init()
	{
		audioData = new Dictionary<string, XmlData>();
		loopingOnEntity = new Dictionary<int, Dictionary<string, NearAndFarGO>>();
		fadingOutOnEntity = new Dictionary<int, Dictionary<string, NearAndFarGO>>();
		loopingOnPosition = new Dictionary<Vector3, Dictionary<string, NearAndFarGO>>(Vector3ToFixedEqualityComparer.Instance);
		sequenceOnEntity = new Dictionary<int, Dictionary<string, SequenceGOs>>();
		stoppedEntitySequences = new Dictionary<int, Dictionary<string, SequenceStopper>>();
		playingAudioSources = new List<AudioSource>();
		playingAudioSourceDopplers = new List<DopplerItem>();
		audioSourceDatas = new Dictionary<string, AudioSourceData>();
		playingOnEntity = new Dictionary<int, Channels>();
		random = GameRandomManager.Instance.CreateGameRandom();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static EntityPlayerLocal LocalPlayer()
	{
		if (!localPlayer)
		{
			localPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		}
		return localPlayer;
	}

	public static void OriginChanged(Vector3 offset)
	{
		updateCurrentListener();
		Dictionary<Vector3, Dictionary<string, NearAndFarGO>> dictionary = new Dictionary<Vector3, Dictionary<string, NearAndFarGO>>(Vector3ToFixedEqualityComparer.Instance);
		foreach (KeyValuePair<Vector3, Dictionary<string, NearAndFarGO>> item in loopingOnPosition)
		{
			Vector3 vector = item.Key + offset;
			bool flag = false;
			for (int i = 0; i < 10; i++)
			{
				if (!dictionary.ContainsKey(vector))
				{
					dictionary.Add(vector, item.Value);
					flag = true;
					break;
				}
				vector.x += 0.0234f;
				vector.z += 0.09f;
			}
			if (!flag)
			{
				Log.Warning("AudioManager OriginChanged key collision {0}, count {1}", vector, item.Value.Count);
			}
		}
		loopingOnPosition.Clear();
		loopingOnPosition = dictionary;
		DopplerCheckForMove();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void DopplerCheckForMove()
	{
		if (dopplerDelay > 0 || !((mainCamera.transform.position - currentListenerPosition).sqrMagnitude > 400f))
		{
			return;
		}
		DopplerRestore();
		DopplerItem item = default(DopplerItem);
		for (int num = playingAudioSources.Count - 1; num >= 0; num--)
		{
			AudioSource audioSource = playingAudioSources[num];
			if ((bool)audioSource)
			{
				float dopplerLevel = audioSource.dopplerLevel;
				if (dopplerLevel > 0f)
				{
					audioSource.dopplerLevel = 0f;
					item.src = audioSource;
					item.doppler = dopplerLevel;
					playingAudioSourceDopplers.Add(item);
				}
			}
		}
		dopplerDelay = 6;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void DopplerRestore()
	{
		int count = playingAudioSourceDopplers.Count;
		if (count <= 0)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			AudioSource src = playingAudioSourceDopplers[i].src;
			if ((bool)src)
			{
				src.dopplerLevel = playingAudioSourceDopplers[i].doppler;
			}
		}
		playingAudioSourceDopplers.Clear();
	}

	public static void AddSoundToIgnoreDistanceCheckList(string soundGroupName)
	{
		if (ignoreDistanceCheckSounds == null)
		{
			ignoreDistanceCheckSounds = new CaseInsensitiveStringDictionary<bool>();
		}
		if (!ignoreDistanceCheckSounds.ContainsKey(soundGroupName))
		{
			ignoreDistanceCheckSounds.Add(soundGroupName, value: true);
		}
	}

	public static bool IgnoresDistanceCheck(string soundGroupName)
	{
		StripOffDirectories(ref soundGroupName);
		if (ignoreDistanceCheckSounds == null)
		{
			ignoreDistanceCheckSounds = new CaseInsensitiveStringDictionary<bool>();
		}
		return ignoreDistanceCheckSounds.ContainsKey(soundGroupName);
	}

	public static void Reset()
	{
		Init();
	}

	public static void CleanUp()
	{
		if (Instance.PositionalSoundsPlaying != null)
		{
			Origin.Remove(Instance.PositionalSoundsPlayingT);
			UnityEngine.Object.Destroy(Instance.PositionalSoundsPlaying);
		}
		instance = null;
	}

	public static void AddAudioData(XmlData _data)
	{
		StripOffDirectories(ref _data.soundGroupName);
		audioData.Add(_data.soundGroupName, _data);
		if (_data.audioClipMap.Count <= 0 && _data.altAudioClipMap == null)
		{
			return;
		}
		if (_data.noiseData != null)
		{
			float volume = _data.noiseData.volume;
			float time = _data.noiseData.time;
			float crouchMuffle = _data.noiseData.crouchMuffle;
			float heatMapStrength = _data.noiseData.heatMapStrength;
			ulong heatMapWorldTimeToLive = _data.noiseData.heatMapTime * 10;
			AIDirectorData.AddNoisySound(_data.soundGroupName, new AIDirectorData.Noise(_data.soundGroupName, volume, time, crouchMuffle, heatMapStrength, heatMapWorldTimeToLive));
		}
		if (!_data.hasProfanity)
		{
			return;
		}
		_data.cleanClipMap = new List<ClipSourceMap>();
		for (int i = 0; i < _data.audioClipMap.Count; i++)
		{
			if (!_data.audioClipMap[i].profanity)
			{
				_data.cleanClipMap.Add(_data.audioClipMap[i]);
			}
		}
	}

	public static void AddSubtitleData(List<SubtitleData> subtitleDatas, List<SubtitleSpeakerColor> speakerColors)
	{
		subtitleCache = new Dictionary<string, SubtitleData>();
		foreach (SubtitleData subtitleData in subtitleDatas)
		{
			subtitleCache.Add(subtitleData.name, subtitleData);
		}
		subtitleSpeakerColorCache = new Dictionary<string, string>();
		foreach (SubtitleSpeakerColor speakerColor in speakerColors)
		{
			subtitleSpeakerColorCache.Add(speakerColor.name, speakerColor.color);
		}
		Log.Out($"Added {subtitleCache.Count} subtitle data entries and {subtitleSpeakerColorCache.Count} speaker colors");
	}

	public static void PauseGameplayAudio()
	{
		lock (playingAudioSources)
		{
			for (int num = playingAudioSources.Count - 1; num >= 0; num--)
			{
				if (playingAudioSources[num] != null)
				{
					playingAudioSources[num].Pause();
				}
				else
				{
					playingAudioSources.RemoveAt(num);
				}
			}
		}
	}

	public static void UnPauseGameplayAudio()
	{
		lock (playingAudioSources)
		{
			for (int num = playingAudioSources.Count - 1; num >= 0; num--)
			{
				if (playingAudioSources[num] != null)
				{
					playingAudioSources[num].UnPause();
				}
				else
				{
					playingAudioSources.RemoveAt(num);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static AudioSource LoadAudio(bool _forcedLooping, float _listenerDistance, string _clipName, string _audioSourceName)
	{
		if (_clipName == "none")
		{
			return null;
		}
		if (!audioSrcObjAssetCache.TryGetValue(_audioSourceName, out var value))
		{
			value = DataLoader.LoadAsset<GameObject>(_audioSourceName);
			if ((bool)value)
			{
				audioSrcObjAssetCache.Add(_audioSourceName, value);
			}
		}
		if (!value)
		{
			Log.Warning("AudioManager LoadAudio failed to load audio source object for " + _audioSourceName);
			return null;
		}
		AudioSource component = value.GetComponent<AudioSource>();
		if (_listenerDistance >= component.maxDistance && !component.loop && !_forcedLooping)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(value);
		if (!gameObject)
		{
			Log.Warning("AudioManager LoadAudio failed to instantiate audio source object for " + _audioSourceName);
			return null;
		}
		component = gameObject.GetComponent<AudioSource>();
		if (!component)
		{
			Log.Warning("AudioManager LoadAudio failed to load audio source " + _audioSourceName);
			UnityEngine.Object.Destroy(gameObject);
			return null;
		}
		if (!audioClipAssetCache.TryGetValue(_clipName, out var value2))
		{
			value2 = DataLoader.LoadAsset<AudioClip>(_clipName);
			if ((bool)value2)
			{
				audioClipAssetCache.Add(_clipName, value2);
			}
		}
		if (!value2)
		{
			Log.Warning("AudioManager LoadAudio failed to load audio clip " + _clipName);
			return null;
		}
		component.clip = value2;
		string key = _audioSourceName.Replace("Sounds/", "");
		if (!audioSourceDatas.ContainsKey(key))
		{
			AudioSourceData value3 = default(AudioSourceData);
			value3.maxVolume = component.volume;
			audioSourceDatas.Add(key, value3);
		}
		if (component.dopplerLevel > 0f)
		{
			Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
			if (rigidbody == null)
			{
				rigidbody = gameObject.AddComponent<Rigidbody>();
			}
			rigidbody.useGravity = false;
			rigidbody.velocity = Vector3.zero;
			rigidbody.isKinematic = true;
			rigidbody.gameObject.tag = "AudioRigidBody";
		}
		return component;
	}

	public static void BroadcastPlay(string soundGroupName)
	{
		if (soundGroupName == null)
		{
			return;
		}
		if (ServerAudio != null)
		{
			if (!GameManager.IsDedicatedServer)
			{
				Play(LocalPlayer(), soundGroupName);
			}
			ServerAudio.Play(LocalPlayer(), soundGroupName, 0f);
		}
		else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			NetPackageAudio package = NetPackageManager.GetPackage<NetPackageAudio>().Setup(LocalPlayer().entityId, soundGroupName, 0f, _play: true);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		}
	}

	public static void BroadcastPlay(Entity entity, string soundGroupName, bool signalOnly = false)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient && signalOnly)
		{
			return;
		}
		EntityPlayer entityPlayer = entity as EntityPlayer;
		if ((entityPlayer != null && entityPlayer.IsSpectator) || soundGroupName == null)
		{
			return;
		}
		if (ServerAudio != null)
		{
			if (!GameManager.IsDedicatedServer)
			{
				Play(entity, soundGroupName);
			}
			ServerAudio.Play(entity, soundGroupName, entity.CalculateAudioOcclusion(), signalOnly);
		}
		else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			NetPackageAudio package = NetPackageManager.GetPackage<NetPackageAudio>().Setup(entity.entityId, soundGroupName, entity.CalculateAudioOcclusion(), _play: true, signalOnly);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		}
	}

	public static void BroadcastPlayByLocalPlayer(Vector3 position, string soundGroupName)
	{
		if (soundGroupName == null)
		{
			return;
		}
		if (ServerAudio != null)
		{
			if (!GameManager.IsDedicatedServer)
			{
				Play(position, soundGroupName, (LocalPlayer() != null) ? LocalPlayer().entityId : (-1));
			}
			ServerAudio.Play(position, soundGroupName, 0f);
		}
		else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			NetPackageAudio package = NetPackageManager.GetPackage<NetPackageAudio>().Setup(position, soundGroupName, 0f, _play: true, (LocalPlayer() != null) ? LocalPlayer().entityId : (-1));
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		}
	}

	public static void BroadcastPlay(Vector3 position, string soundGroupName, float _occlusion = 0f)
	{
		if (soundGroupName == null)
		{
			return;
		}
		if (ServerAudio != null)
		{
			if (!GameManager.IsDedicatedServer)
			{
				Play(position, soundGroupName);
			}
			ServerAudio.Play(position, soundGroupName, _occlusion);
		}
		else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			NetPackageAudio package = NetPackageManager.GetPackage<NetPackageAudio>().Setup(position, soundGroupName, _occlusion, _play: true);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		}
	}

	public static void BroadcastStop(int entityId, string soundGroupName)
	{
		if (soundGroupName == null)
		{
			return;
		}
		if (ServerAudio != null)
		{
			if (!GameManager.IsDedicatedServer)
			{
				Stop(entityId, soundGroupName);
			}
			ServerAudio.Stop(entityId, soundGroupName);
		}
		else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			NetPackageAudio package = NetPackageManager.GetPackage<NetPackageAudio>().Setup(entityId, soundGroupName, 0f, _play: false);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		}
	}

	public static void BroadcastStop(Vector3 position, string soundGroupName)
	{
		if (soundGroupName == null)
		{
			return;
		}
		if (ServerAudio != null)
		{
			if (!GameManager.IsDedicatedServer)
			{
				Stop(position, soundGroupName);
			}
			ServerAudio.Stop(position, soundGroupName);
		}
		else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			NetPackageAudio package = NetPackageManager.GetPackage<NetPackageAudio>().Setup(position, soundGroupName, 0f, _play: false);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		}
	}

	public static Handle Play(Vector3 position, string soundGroupName, int entityId = -1, bool wantHandle = false)
	{
		Entity entity = null;
		ConvertName(ref soundGroupName);
		if (entityId >= 0)
		{
			entity = GameManager.Instance.World.GetEntity(entityId);
		}
		SignalAI(entity, position, soundGroupName, 1f);
		if (!CheckGlobalPlayRequirements(soundGroupName))
		{
			return null;
		}
		if (!audioData.TryGetValue(soundGroupName, out var value))
		{
			return null;
		}
		if (!value.Update())
		{
			return null;
		}
		position -= Origin.position;
		bool flag = false;
		bool flag2 = true;
		if (value.distantFadeStart >= 0f)
		{
			float magnitude = (position - currentListenerPosition).magnitude;
			flag = magnitude > value.distantFadeStart;
			flag2 = magnitude < value.distantFadeEnd;
		}
		ClipSourceMap randomClip = value.GetRandomClip();
		if (randomClip == null)
		{
			return null;
		}
		GameObject gameObject = null;
		AudioSource audioSource = null;
		if (flag2)
		{
			audioSource = LoadAudio(randomClip.forceLoop, (LocalPlayer().position - Origin.position - position).magnitude, randomClip.clipName, randomClip.audioSourceName);
			if (!audioSource)
			{
				return null;
			}
			gameObject = audioSource.gameObject;
			Transform transform = audioSource.transform;
			transform.SetParent(Instance.PositionalSoundsPlayingT, worldPositionStays: false);
			transform.position = position;
			audioSource.loop = audioSource.loop || randomClip.forceLoop;
			if (entity != null && entity is EntityAlive)
			{
				float volume = audioSource.volume;
				volume = (((EntityAlive)entity).IsCrouching ? (volume * value.localCrouchVolumeScale) : volume);
				audioSource.volume = (((EntityAlive)entity).MovementRunning ? (volume * value.runningVolumeScale) : volume);
			}
		}
		GameObject gameObject2 = null;
		AudioSource audioSource2 = null;
		if (flag)
		{
			audioSource2 = LoadAudio(randomClip.forceLoop, (LocalPlayer().position - Origin.position - position).magnitude, randomClip.clipName_distant, (randomClip.audioSourceName_distant.Length > 0) ? randomClip.audioSourceName_distant : randomClip.audioSourceName);
			if (audioSource2 == null)
			{
				return null;
			}
			gameObject2 = audioSource2.gameObject;
			Transform transform2 = audioSource2.transform;
			transform2.SetParent(Instance.PositionalSoundsPlayingT, worldPositionStays: false);
			transform2.position = position;
			audioSource2.loop = audioSource2.loop || randomClip.forceLoop;
			if (entity != null && entity is EntityAlive)
			{
				float volume2 = audioSource2.volume;
				volume2 = (((EntityAlive)entity).IsCrouching ? (volume2 * value.localCrouchVolumeScale) : volume2);
				audioSource2.volume = (((EntityAlive)entity).MovementRunning ? (volume2 * value.runningVolumeScale) : volume2);
			}
		}
		if (((bool)audioSource && audioSource.loop) || ((bool)audioSource2 && audioSource2.loop))
		{
			if (loopingOnPosition.TryGetValue(position, out var value2) && value2.TryGetValue(soundGroupName, out var value3))
			{
				if (value3.near != null)
				{
					AudioSource component = value3.near.GetComponent<AudioSource>();
					if (component != null)
					{
						RemovePlayingAudioSource(component);
					}
					UnityEngine.Object.Destroy(value3.near);
				}
				if (value3.far != null)
				{
					AudioSource component2 = value3.far.GetComponent<AudioSource>();
					if (component2 != null)
					{
						RemovePlayingAudioSource(component2);
					}
					UnityEngine.Object.Destroy(value3.far);
				}
				value2.Remove(soundGroupName);
				if (value2.Count == 0)
				{
					loopingOnPosition.Remove(position);
				}
			}
			if (audioSource != null)
			{
				audioSource.volume *= 1f - ((entity == null) ? 0f : entity.CalculateAudioOcclusion());
				SetPitch(audioSource, value, 0f);
				PlaySource(audioSource);
				AddPlayingAudioSource(audioSource);
			}
			if (audioSource2 != null)
			{
				SetPitch(audioSource2, value, 0f);
				PlaySource(audioSource2);
				AddPlayingAudioSource(audioSource2);
			}
			NearAndFarGO value4 = new NearAndFarGO
			{
				near = gameObject,
				far = gameObject2
			};
			if (loopingOnPosition.TryGetValue(position, out var value5))
			{
				value5.Add(soundGroupName, value4);
			}
			else
			{
				Dictionary<string, NearAndFarGO> dictionary = new Dictionary<string, NearAndFarGO>();
				dictionary.Add(soundGroupName, value4);
				loopingOnPosition.Add(position, dictionary);
			}
		}
		else
		{
			if (audioSource != null)
			{
				SetPitch(audioSource, value, 0f);
				new PlayAndCleanup(gameObject, audioSource, (entity == null) ? 0f : entity.CalculateAudioOcclusion());
				if (value.vibratesController && entity is EntityPlayerLocal)
				{
					GameManager.Instance.triggerEffectManager.SetAudioRumbleSource(audioSource, value.vibrationStrengthMultiplier, _locationBased: true);
				}
			}
			if (audioSource2 != null)
			{
				SetPitch(audioSource2, value, 0f);
				new PlayAndCleanup(gameObject2, audioSource2, (entity == null) ? 0f : entity.CalculateAudioOcclusion());
			}
		}
		if (wantHandle)
		{
			return new Handle(soundGroupName, audioSource, audioSource2);
		}
		return null;
	}

	public static Handle Play(Entity _entity, string soundGroupName, float volumeScale = 1f, bool wantHandle = false)
	{
		ConvertName(ref soundGroupName, _entity);
		SignalAI(_entity, _entity.position, soundGroupName, volumeScale);
		if (!CheckGlobalPlayRequirements(soundGroupName))
		{
			return null;
		}
		if (!audioData.TryGetValue(soundGroupName, out var value) || value == null)
		{
			return null;
		}
		if (!value.Update())
		{
			return null;
		}
		bool flag = false;
		if (playingOnEntity.TryGetValue(_entity.entityId, out var value2))
		{
			if (value.channel == XmlData.Channel.Environment)
			{
				if (value2.environment != null && value2.environment.dict.TryGetValue(soundGroupName, out var value3))
				{
					for (int num = value3.Count - 1; num >= 0; num--)
					{
						AudioSource audioSource = value3[num];
						if (!audioSource || !audioSource.isPlaying)
						{
							value3.RemoveAt(num);
							RemovePlayingAudioSource(audioSource);
						}
					}
					if (value3.Count == 0)
					{
						value2.environment.Remove(soundGroupName);
						if (value2.environment.Count == 0 && (value2.mouth == null || value2.mouth.Count == 0))
						{
							playingOnEntity.Remove(_entity.entityId);
						}
					}
					else if (value3.Count > value.maxVoicesPerEntity)
					{
						flag = true;
					}
				}
			}
			else if (value2.mouth != null)
			{
				List<int> list = null;
				for (int i = 0; i < value2.mouth.Count; i++)
				{
					if (value2.mouth[i] == null || !value2.mouth[i].isPlaying)
					{
						if (list == null)
						{
							list = new List<int>();
						}
						list.Add(i);
					}
				}
				if (list != null)
				{
					for (int num2 = list.Count - 1; num2 >= 0; num2--)
					{
						if (value2.mouth[list[num2]] != null)
						{
							RemovePlayingAudioSource(value2.mouth[list[num2]]);
						}
						value2.mouth.RemoveAt(list[num2]);
					}
				}
				if (value2.mouth.Count > 0)
				{
					if (value.priority < value2.currentMouthPriority)
					{
						for (int j = 0; j < value2.mouth.Count; j++)
						{
							StopSource(value2.mouth[j]);
							RemovePlayingAudioSource(value2.mouth[j]);
						}
						value2.mouth.Clear();
					}
					else
					{
						flag = true;
					}
				}
				if (value2.mouth.Count == 0 && (value2.environment == null || value2.environment.Count == 0))
				{
					playingOnEntity.Remove(_entity.entityId);
				}
			}
			if (playingOnEntity.ContainsKey(_entity.entityId))
			{
				playingOnEntity[_entity.entityId] = value2;
			}
		}
		if (flag)
		{
			return null;
		}
		bool flag2 = false;
		bool flag3 = true;
		if (value.distantFadeStart >= 0f)
		{
			float magnitude = (_entity.position - Origin.position - currentListenerPosition).magnitude;
			flag2 = magnitude > value.distantFadeStart;
			flag3 = magnitude < value.distantFadeEnd;
		}
		Transform transform = _entity.transform;
		Vector3 position = transform.position;
		ClipSourceMap randomClip = value.GetRandomClip();
		if (randomClip == null)
		{
			return null;
		}
		GameObject gameObject = null;
		AudioSource audioSource2 = null;
		if (flag3)
		{
			audioSource2 = LoadAudio(randomClip.forceLoop, (_entity.position - LocalPlayer().position).magnitude, randomClip.clipName, randomClip.audioSourceName);
			if (!audioSource2)
			{
				return null;
			}
			gameObject = audioSource2.gameObject;
			Transform transform2 = audioSource2.transform;
			Transform transform3 = null;
			if ((bool)_entity.emodel)
			{
				transform3 = _entity.emodel.bipedPelvisTransform;
			}
			if ((bool)transform3)
			{
				transform2.SetParent(transform3, worldPositionStays: false);
				transform2.position = transform3.position;
			}
			else
			{
				transform2.SetParent(transform, worldPositionStays: false);
				transform2.position = transform.position;
			}
			audioSource2.loop = audioSource2.loop || randomClip.forceLoop;
		}
		GameObject gameObject2 = null;
		AudioSource audioSource3 = null;
		if (flag2)
		{
			audioSource3 = LoadAudio(randomClip.forceLoop, (_entity.position - LocalPlayer().position).magnitude, randomClip.clipName_distant, (randomClip.audioSourceName_distant.Length > 0) ? randomClip.audioSourceName_distant : randomClip.audioSourceName);
			if (audioSource3 == null)
			{
				return null;
			}
			gameObject2 = audioSource3.gameObject;
			Transform transform4 = audioSource3.transform;
			transform4.SetParent(transform);
			transform4.position = position;
			audioSource3.loop = audioSource3.loop || randomClip.forceLoop;
		}
		EntityAlive entityAlive = _entity as EntityAlive;
		if ((bool)entityAlive)
		{
			float shift = ((_entity is EntityPlayerLocal) ? 0.05f : entityAlive.OverridePitch);
			if (audioSource2 != null)
			{
				float num3 = audioSource2.volume * volumeScale;
				if (entityAlive.IsCrouching)
				{
					num3 *= value.localCrouchVolumeScale;
				}
				if (entityAlive.MovementRunning)
				{
					num3 *= value.runningVolumeScale;
				}
				audioSource2.volume = num3;
				SetPitch(audioSource2, value, shift);
				if (value.vibratesController)
				{
					if (_entity is EntityPlayerLocal)
					{
						GameManager.Instance.triggerEffectManager.SetAudioRumbleSource(audioSource2, value.vibrationStrengthMultiplier, _locationBased: false);
					}
					else if (entityAlive.GetAttachedPlayerLocal() != null)
					{
						GameManager.Instance.triggerEffectManager.SetAudioRumbleSource(audioSource2, value.vibrationStrengthMultiplier, _locationBased: false);
					}
				}
			}
			if (audioSource3 != null)
			{
				float num4 = audioSource3.volume * volumeScale;
				if (entityAlive.IsCrouching)
				{
					num4 *= value.localCrouchVolumeScale;
				}
				if (entityAlive.MovementRunning)
				{
					num4 *= value.runningVolumeScale;
				}
				audioSource3.volume = num4;
				SetPitch(audioSource3, value, shift);
			}
		}
		if (((bool)audioSource2 && audioSource2.loop) || ((bool)audioSource3 && audioSource3.loop))
		{
			if (loopingOnEntity.TryGetValue(_entity.entityId, out var value4))
			{
				StopGroupLoop(value4, soundGroupName);
			}
			if (audioSource2 != null)
			{
				audioSource2.volume *= 1f - ((_entity == null) ? 0f : _entity.CalculateAudioOcclusion());
				PlaySource(audioSource2);
				AddPlayingAudioSource(audioSource2);
			}
			if (audioSource3 != null)
			{
				PlaySource(audioSource3);
				AddPlayingAudioSource(audioSource3);
			}
			if (value4 == null)
			{
				value4 = new Dictionary<string, NearAndFarGO>();
				loopingOnEntity.Add(_entity.entityId, value4);
			}
			NearAndFarGO value5 = new NearAndFarGO
			{
				near = gameObject,
				far = gameObject2
			};
			value4.Add(soundGroupName, value5);
		}
		else
		{
			if (!playingOnEntity.TryGetValue(_entity.entityId, out var value6))
			{
				value6 = default(Channels);
				playingOnEntity.Add(_entity.entityId, value6);
			}
			if (value.channel == XmlData.Channel.Environment)
			{
				if (value6.environment == null)
				{
					value6.environment = new DictionaryList<string, List<AudioSource>>();
				}
				if (!value6.environment.dict.TryGetValue(soundGroupName, out var value7))
				{
					value7 = new List<AudioSource>();
					value6.environment.Add(soundGroupName, value7);
				}
				if (audioSource2 != null)
				{
					value7.Add(audioSource2);
				}
				if (audioSource3 != null)
				{
					value7.Add(audioSource3);
				}
			}
			else
			{
				if (value6.mouth == null)
				{
					value6.mouth = new List<AudioSource>();
				}
				value6.currentMouthPriority = value.priority;
				if (audioSource2 != null)
				{
					value6.mouth.Add(audioSource2);
				}
				if (audioSource3 != null)
				{
					value6.mouth.Add(audioSource3);
				}
			}
			playingOnEntity[_entity.entityId] = value6;
			if (audioSource2 != null)
			{
				new PlayAndCleanup(gameObject, audioSource2, _entity.CalculateAudioOcclusion());
			}
			if (audioSource3 != null)
			{
				new PlayAndCleanup(gameObject2, audioSource3, _entity.CalculateAudioOcclusion());
			}
		}
		Handle result = null;
		if (wantHandle)
		{
			result = new Handle(soundGroupName, audioSource2, audioSource3);
		}
		if (GamePrefs.GetBool(EnumGamePrefs.OptionsSubtitlesEnabled) && randomClip.hasSubtitle)
		{
			GameManager.ShowSubtitle(LocalPlayerUI.primaryUI.xui, GetFormattedSubtitleSpeaker(randomClip.subtitleID), GetFormattedSubtitle(randomClip.subtitleID), audioSource2.clip.length);
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetPitch(AudioSource _src, XmlData _data, float _shift)
	{
		float num = random.RandomRange(_data.lowestPitch, _data.highestPitch);
		num += _shift;
		if (num < 0.85f)
		{
			num = 0.85f;
		}
		_src.pitch = num;
	}

	public static void StopAllLocal()
	{
		if (playingAudioSources == null)
		{
			return;
		}
		for (int i = 0; i < playingAudioSources.Count; i++)
		{
			if (playingAudioSources[i] != null)
			{
				StopSource(playingAudioSources[i]);
			}
		}
	}

	public static void AddPlayingAudioSource(AudioSource _src)
	{
		lock (playingAudioSources)
		{
			playingAudioSources.Add(_src);
		}
	}

	public static void RemovePlayingAudioSource(AudioSource _src)
	{
		lock (playingAudioSources)
		{
			playingAudioSources.Remove(_src);
		}
	}

	public static void FadeOut(int entityId, string soundGroupName)
	{
		if (!CheckGlobalPlayRequirements(soundGroupName))
		{
			return;
		}
		ConvertName(ref soundGroupName);
		if (!loopingOnEntity.TryGetValue(entityId, out var value) || !value.TryGetValue(soundGroupName, out var value2))
		{
			return;
		}
		if (fadingOutOnEntity.TryGetValue(entityId, out value))
		{
			if (value.ContainsKey(soundGroupName))
			{
				return;
			}
		}
		else
		{
			value = new Dictionary<string, NearAndFarGO>();
			fadingOutOnEntity.Add(entityId, value);
		}
		value.Add(soundGroupName, value2);
	}

	public static void Stop(int entityId, string soundGroupName)
	{
		if (!CheckGlobalPlayRequirements(soundGroupName))
		{
			return;
		}
		ConvertName(ref soundGroupName);
		if (loopingOnEntity.TryGetValue(entityId, out var value))
		{
			StopGroupLoop(value, soundGroupName);
			if (value.Count == 0)
			{
				loopingOnEntity.Remove(entityId);
			}
		}
		if (!playingOnEntity.TryGetValue(entityId, out var value2) || value2.environment == null || !value2.environment.dict.TryGetValue(soundGroupName, out var value3))
		{
			return;
		}
		for (int i = 0; i < value3.Count; i++)
		{
			AudioSource audioSource = value3[i];
			if ((bool)audioSource)
			{
				StopSource(audioSource);
				RemovePlayingAudioSource(audioSource);
				UnityEngine.Object.Destroy(audioSource.gameObject);
			}
		}
		value2.environment.Remove(soundGroupName);
		if (value2.environment.Count == 0)
		{
			value2.environment = null;
			if (value2.mouth == null || value2.mouth.Count == 0)
			{
				playingOnEntity.Remove(entityId);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void StopGroupLoop(Dictionary<string, NearAndFarGO> soundGroup, string soundGroupName)
	{
		if (!soundGroup.TryGetValue(soundGroupName, out var value))
		{
			return;
		}
		if (value.near != null)
		{
			AudioSource component = value.near.GetComponent<AudioSource>();
			if (component != null)
			{
				StopSource(component);
				RemovePlayingAudioSource(component);
			}
			UnityEngine.Object.Destroy(value.near);
		}
		if (value.far != null)
		{
			AudioSource component2 = value.far.GetComponent<AudioSource>();
			if (component2 != null)
			{
				StopSource(component2);
				RemovePlayingAudioSource(component2);
			}
			UnityEngine.Object.Destroy(value.far);
		}
		soundGroup.Remove(soundGroupName);
	}

	public static void Stop(Vector3 position, string soundGroupName)
	{
		if (!CheckGlobalPlayRequirements(soundGroupName))
		{
			return;
		}
		ConvertName(ref soundGroupName);
		position -= Origin.position;
		if (!loopingOnPosition.TryGetValue(position, out var value) || !value.TryGetValue(soundGroupName, out var value2))
		{
			return;
		}
		if (value2.near != null)
		{
			AudioSource component = value2.near.GetComponent<AudioSource>();
			if (component != null)
			{
				StopSource(component);
				RemovePlayingAudioSource(component);
			}
			UnityEngine.Object.Destroy(value2.near);
		}
		if (value2.far != null)
		{
			AudioSource component2 = value2.far.GetComponent<AudioSource>();
			if (component2 != null)
			{
				StopSource(component2);
				RemovePlayingAudioSource(component2);
			}
			UnityEngine.Object.Destroy(value2.far);
		}
		value.Remove(soundGroupName);
		if (value.Count < 1)
		{
			loopingOnPosition.Remove(position);
		}
	}

	public static void PlayInsidePlayerHead(string soundGroupNameBegin, int entityID)
	{
		string soundGroupName = soundGroupNameBegin + "_lp";
		if (!CheckGlobalPlayRequirements(soundGroupNameBegin) || !CheckGlobalPlayRequirements(soundGroupName))
		{
			return;
		}
		ConvertName(ref soundGroupNameBegin);
		ConvertName(ref soundGroupName);
		if (!audioData.TryGetValue(soundGroupNameBegin, out var value) || !value.Update() || !audioData.TryGetValue(soundGroupName, out var value2) || !value2.Update())
		{
			return;
		}
		ClipSourceMap randomClip = value.GetRandomClip();
		if (randomClip == null)
		{
			return;
		}
		AudioSource audioSource = LoadAudio(randomClip.forceLoop, 0f, randomClip.clipName, randomClip.audioSourceName);
		if (audioSource == null)
		{
			return;
		}
		ClipSourceMap randomClip2 = value2.GetRandomClip();
		if (randomClip2 == null)
		{
			return;
		}
		AudioSource audioSource2 = LoadAudio(randomClip2.forceLoop, 0f, randomClip2.clipName, randomClip2.audioSourceName);
		if (!(audioSource2 == null))
		{
			audioSource2.loop = true;
			Transform transform = LocalPlayer().transform;
			Vector3 position = transform.position;
			Transform transform2 = audioSource.transform;
			transform2.position = position;
			transform2.SetParent(transform);
			Transform transform3 = audioSource2.transform;
			transform3.position = position;
			transform3.SetParent(transform);
			GameObject gameObject = audioSource.gameObject;
			GameObject gameObject2 = audioSource2.gameObject;
			LoopingPair lp = new LoopingPair
			{
				sgoBegin = 
				{
					go = gameObject,
					src = audioSource
				},
				sgoLoop = 
				{
					go = gameObject2,
					src = audioSource2
				}
			};
			NearAndFarGO value3 = new NearAndFarGO
			{
				near = gameObject2
			};
			if (!loopingOnEntity.ContainsKey(entityID))
			{
				loopingOnEntity.Add(entityID, new Dictionary<string, NearAndFarGO>());
			}
			loopingOnEntity[entityID].Add(soundGroupName, value3);
			new PlayAndCleanup(lp);
		}
	}

	public static void PlayInsidePlayerHead(string soundGroupName, int entityID = -1, float delay = 0f, bool isLooping = false, bool isUnique = false)
	{
		if (!CheckGlobalPlayRequirements(soundGroupName))
		{
			return;
		}
		Entity entity = ((entityID >= 0) ? GameManager.Instance.World.GetEntity(entityID) : null);
		ConvertName(ref soundGroupName, entity);
		if (!audioData.TryGetValue(soundGroupName, out var value) || !value.Update())
		{
			return;
		}
		ClipSourceMap randomClip = value.GetRandomClip();
		if (randomClip == null)
		{
			return;
		}
		AudioSource audioSource = LoadAudio(randomClip.forceLoop, 0f, randomClip.clipName, randomClip.audioSourceName);
		if (audioSource == null)
		{
			return;
		}
		GameObject gameObject = audioSource.gameObject;
		Transform transform = audioSource.transform;
		EntityAlive entityAlive = LocalPlayer();
		Transform transform2 = entityAlive.transform;
		transform.SetParent(transform2, worldPositionStays: false);
		transform.position = transform2.position;
		if (entityAlive.IsCrouching)
		{
			audioSource.volume *= value.localCrouchVolumeScale;
		}
		SetPitch(audioSource, value, 0f);
		if (value.vibratesController)
		{
			GameManager.Instance.triggerEffectManager.SetAudioRumbleSource(audioSource, value.vibrationStrengthMultiplier, _locationBased: false);
		}
		if (isLooping)
		{
			audioSource.loop = true;
			NearAndFarGO value2 = new NearAndFarGO
			{
				near = gameObject
			};
			if (entityID != -1)
			{
				if (loopingOnEntity.ContainsKey(entityID))
				{
					loopingOnEntity[entityID].Add(soundGroupName, value2);
				}
				else
				{
					loopingOnEntity.Add(entityID, new Dictionary<string, NearAndFarGO>());
					loopingOnEntity[entityID].Add(soundGroupName, value2);
				}
			}
			new PlayAndCleanup(gameObject, audioSource, 0f, delay, isLooping: true);
		}
		else
		{
			if (isUnique)
			{
				if (uniqueSrc != null)
				{
					StopSource(uniqueSrc);
				}
				uniqueSrc = audioSource;
			}
			new PlayAndCleanup(gameObject, audioSource, 0f, delay);
		}
		if (GamePrefs.GetBool(EnumGamePrefs.OptionsSubtitlesEnabled) && randomClip.hasSubtitle)
		{
			GameManager.ShowSubtitle(LocalPlayerUI.primaryUI.xui, GetFormattedSubtitleSpeaker(randomClip.subtitleID), GetFormattedSubtitle(randomClip.subtitleID), audioSource.clip.length);
		}
	}

	public static void StopLoopInsidePlayerHead(string soundGroupName, int entityID = -1)
	{
		if (!CheckGlobalPlayRequirements(soundGroupName))
		{
			return;
		}
		ConvertName(ref soundGroupName);
		if (entityID != -1 && loopingOnEntity.ContainsKey(entityID) && loopingOnEntity[entityID].TryGetValue(soundGroupName, out var value))
		{
			AudioSource component = value.near.GetComponent<AudioSource>();
			if (component != null)
			{
				StopSource(component);
				UnityEngine.Object.Destroy(value.near);
				loopingOnEntity[entityID].Remove(soundGroupName);
			}
		}
	}

	public static void ConvertName(ref string soundGroupName, Entity _entity = null)
	{
		if (soundGroupName == null)
		{
			return;
		}
		if (_entity is EntityPlayer entityPlayer)
		{
			int num = soundGroupName.IndexOfAny(convertChars);
			if (num >= 0)
			{
				if (soundGroupName[num] == '*')
				{
					string newValue = (entityPlayer.IsMale ? "Male" : "Female");
					soundGroupName = soundGroupName.Replace("*", newValue);
				}
				else
				{
					string newValue2 = (entityPlayer.IsMale ? "1" : "2");
					soundGroupName = soundGroupName.Replace("#", newValue2);
				}
			}
		}
		StripOffDirectories(ref soundGroupName);
	}

	public static float CalculateOcclusion(Vector3 positionOfSound, Vector3 positionOfEars)
	{
		if (!occlusionsOn)
		{
			return 1f;
		}
		if (mainCamera == null)
		{
			return 1f;
		}
		Vector3 vector = positionOfSound - positionOfEars;
		float magnitude = vector.magnitude;
		if (magnitude < 1f)
		{
			return 1f;
		}
		Vector3 normalized = vector.normalized;
		if (Physics.Raycast(new Ray(positionOfEars - Origin.position, normalized), out var hitInfo, float.PositiveInfinity, 65537) && magnitude > hitInfo.distance + 0.5f && hitInfo.distance < float.PositiveInfinity && Physics.Raycast(new Ray(positionOfSound - Origin.position, (positionOfEars - positionOfSound).normalized), out var hitInfo2, float.PositiveInfinity, 65537))
		{
			float num = magnitude - hitInfo2.distance - hitInfo.distance;
			return 1f - Mathf.Pow(Mathf.Clamp01(num / 13f), 0.75f) * 0.9f;
		}
		return 1f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool CheckGlobalPlayRequirements(string soundGroupName)
	{
		if (GameManager.IsDedicatedServer)
		{
			return false;
		}
		if (!GameManager.Instance)
		{
			return false;
		}
		World world = GameManager.Instance.World;
		if (world == null || !world.GetPrimaryPlayer())
		{
			return false;
		}
		if (soundGroupName == null)
		{
			return false;
		}
		return true;
	}

	public static void PlaySequence(Entity entity, string soundGroupName)
	{
		ConvertName(ref soundGroupName, entity);
		SignalAI(entity, entity.position, soundGroupName, 1f);
		if (!CheckGlobalPlayRequirements(soundGroupName) || entity == null || !audioData.TryGetValue(soundGroupName, out var value) || !value.Update())
		{
			return;
		}
		if (!sequenceOnEntity.TryGetValue(entity.entityId, out var value2))
		{
			value2 = new Dictionary<string, SequenceGOs>();
			sequenceOnEntity.Add(entity.entityId, value2);
		}
		if (value2.ContainsKey(soundGroupName))
		{
			return;
		}
		SequenceGOs value3 = new SequenceGOs
		{
			longestClipLength = 0f
		};
		int num = 0;
		double num2 = 0.0;
		double num3 = 0.0;
		bool flag = false;
		bool flag2 = true;
		float num4 = 1f;
		Transform transform = entity.transform;
		Vector3 position = transform.position;
		if (value.distantFadeStart >= 0f)
		{
			float magnitude = (position - currentListenerPosition).magnitude;
			flag = magnitude > value.distantFadeStart;
			flag2 = magnitude < value.distantFadeEnd;
			num4 = (flag ? (1f - (magnitude - value.distantFadeStart) / (value.distantFadeEnd - value.distantFadeStart)) : 1f);
		}
		List<ClipSourceMap> clipList = value.GetClipList();
		for (int i = 0; i < clipList.Count; i++)
		{
			ClipSourceMap clipSourceMap = clipList[i];
			if (flag2 || clipSourceMap.clipName_distant.Length == 0)
			{
				AudioSource audioSource = LoadAudio(_forcedLooping: false, 0f, clipSourceMap.clipName, clipSourceMap.audioSourceName);
				if (audioSource != null)
				{
					GameObject gameObject = audioSource.gameObject;
					Transform transform2 = audioSource.transform;
					transform2.SetParent(transform);
					transform2.position = position;
					if (audioSource.clip.length > value3.longestClipLength)
					{
						value3.longestClipLength = audioSource.clip.length;
					}
					switch (num)
					{
					case 0:
					{
						audioSource.volume *= CalculateOcclusion(position, currentListenerPosition);
						audioSource.volume *= num4;
						audioSource.loop = false;
						PlaySource(audioSource);
						AddPlayingAudioSource(audioSource);
						double dspTime = AudioSettings.dspTime;
						double num5 = audioSource.clip.samples / 2;
						num5 /= (double)audioSource.clip.frequency;
						num2 += dspTime + num5;
						value3.nearStart = gameObject;
						break;
					}
					case 1:
						if (value.playImmediate)
						{
							audioSource.loop = false;
							audioSource.volume *= CalculateOcclusion(position, currentListenerPosition);
							audioSource.volume *= num4;
							PlaySource(audioSource);
							AddPlayingAudioSource(audioSource);
						}
						else
						{
							audioSource.loop = true;
							audioSource.volume *= CalculateOcclusion(position, currentListenerPosition);
							audioSource.volume *= num4;
							audioSource.PlayScheduled(num2);
							AddPlayingAudioSource(audioSource);
						}
						value3.nearLoop = gameObject;
						break;
					default:
						if (value.playImmediate && num == 2)
						{
							audioSource.volume *= CalculateOcclusion(position, currentListenerPosition);
							audioSource.volume *= num4;
							PlaySource(audioSource);
							AddPlayingAudioSource(audioSource);
							value3.nearEnd = gameObject;
						}
						else
						{
							value3.nearEnd = gameObject;
						}
						break;
					}
				}
			}
			if (flag && clipSourceMap.clipName_distant.Length > 0)
			{
				AudioSource audioSource2 = LoadAudio(_forcedLooping: false, 0f, clipSourceMap.clipName_distant, (clipSourceMap.audioSourceName_distant.Length > 0) ? clipSourceMap.audioSourceName_distant : clipSourceMap.audioSourceName);
				if (audioSource2 != null)
				{
					GameObject gameObject2 = audioSource2.gameObject;
					Transform transform3 = audioSource2.transform;
					transform3.SetParent(transform);
					transform3.position = position;
					if (audioSource2.clip.length > value3.longestClipLength)
					{
						value3.longestClipLength = audioSource2.clip.length;
					}
					switch (num)
					{
					case 0:
					{
						audioSource2.volume *= CalculateOcclusion(position, currentListenerPosition);
						audioSource2.volume *= 1f - num4;
						audioSource2.loop = false;
						PlaySource(audioSource2);
						AddPlayingAudioSource(audioSource2);
						double dspTime2 = AudioSettings.dspTime;
						double num6 = audioSource2.clip.samples / 2;
						num6 /= (double)audioSource2.clip.frequency;
						num3 += dspTime2 + num6;
						value3.farStart = gameObject2;
						break;
					}
					case 1:
						if (value.playImmediate)
						{
							audioSource2.loop = false;
							audioSource2.volume *= CalculateOcclusion(position, currentListenerPosition);
							audioSource2.volume *= 1f - num4;
							PlaySource(audioSource2);
							AddPlayingAudioSource(audioSource2);
						}
						else
						{
							audioSource2.loop = true;
							audioSource2.volume *= CalculateOcclusion(position, currentListenerPosition);
							audioSource2.volume *= 1f - num4;
							audioSource2.PlayScheduled(num3);
							AddPlayingAudioSource(audioSource2);
						}
						value3.farLoop = gameObject2;
						break;
					default:
						if (value.playImmediate && num == 2)
						{
							audioSource2.volume *= CalculateOcclusion(position, currentListenerPosition);
							audioSource2.volume *= 1f - num4;
							PlaySource(audioSource2);
							AddPlayingAudioSource(audioSource2);
							value3.farEnd = gameObject2;
						}
						else
						{
							value3.farEnd = gameObject2;
						}
						break;
					}
				}
			}
			num++;
		}
		value2.Add(soundGroupName, value3);
	}

	public static bool IsASequence(Entity entity, string soundGroupName)
	{
		if (soundGroupName == null)
		{
			return false;
		}
		ConvertName(ref soundGroupName, entity);
		if (!audioData.TryGetValue(soundGroupName, out var value))
		{
			return false;
		}
		return value.sequence;
	}

	public static void RestartSequence(Entity entity, string soundGroupName)
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		ConvertName(ref soundGroupName, entity);
		if (stoppedEntitySequences.TryGetValue(entity.entityId, out var value) && value.TryGetValue(soundGroupName, out var value2))
		{
			for (int i = 0; i < value2.sequenceObjs.Count; i++)
			{
				GameObject gameObject = value2.sequenceObjs[i];
				if (gameObject != null)
				{
					AudioSource component = gameObject.GetComponent<AudioSource>();
					if (component != null)
					{
						StopSource(component);
						RemovePlayingAudioSource(component);
					}
					UnityEngine.Object.Destroy(gameObject);
				}
			}
			value.Remove(soundGroupName);
			if (value.Count < 1)
			{
				stoppedEntitySequences.Remove(entity.entityId);
			}
		}
		if (sequenceOnEntity.TryGetValue(entity.entityId, out var value3) && value3.TryGetValue(soundGroupName, out var value4))
		{
			if (value4.nearStart != null)
			{
				AudioSource component2 = value4.nearStart.GetComponent<AudioSource>();
				if (component2 != null)
				{
					StopSource(component2);
					RemovePlayingAudioSource(component2);
				}
				UnityEngine.Object.Destroy(value4.nearStart);
			}
			if (value4.nearLoop != null)
			{
				AudioSource component3 = value4.nearLoop.GetComponent<AudioSource>();
				if (component3 != null)
				{
					StopSource(component3);
					RemovePlayingAudioSource(component3);
				}
				UnityEngine.Object.Destroy(value4.nearLoop);
			}
			if (value4.nearEnd != null)
			{
				AudioSource component4 = value4.nearEnd.GetComponent<AudioSource>();
				if (component4 != null)
				{
					StopSource(component4);
					RemovePlayingAudioSource(component4);
				}
				UnityEngine.Object.Destroy(value4.nearEnd);
			}
			if (value4.farStart != null)
			{
				AudioSource component5 = value4.farStart.GetComponent<AudioSource>();
				if (component5 != null)
				{
					StopSource(component5);
					RemovePlayingAudioSource(component5);
				}
				UnityEngine.Object.Destroy(value4.farStart);
			}
			if (value4.farLoop != null)
			{
				AudioSource component6 = value4.farLoop.GetComponent<AudioSource>();
				if (component6 != null)
				{
					StopSource(component6);
					RemovePlayingAudioSource(component6);
				}
				UnityEngine.Object.Destroy(value4.farLoop);
			}
			if (value4.farEnd != null)
			{
				AudioSource component7 = value4.farEnd.GetComponent<AudioSource>();
				if (component7 != null)
				{
					StopSource(component7);
					RemovePlayingAudioSource(component7);
				}
				UnityEngine.Object.Destroy(value4.farEnd);
			}
			value3.Remove(soundGroupName);
			if (value3.Count < 1)
			{
				sequenceOnEntity.Remove(entity.entityId);
			}
		}
		PlaySequence(entity, soundGroupName);
	}

	public static void StopAllSequencesOnEntity(Entity entity)
	{
		if (GameManager.IsDedicatedServer || !sequenceOnEntity.TryGetValue(entity.entityId, out var value))
		{
			return;
		}
		foreach (KeyValuePair<string, SequenceGOs> item in value)
		{
			SequenceGOs value2 = item.Value;
			if (value2.nearLoop != null)
			{
				AudioSource component = value2.nearLoop.GetComponent<AudioSource>();
				if (component != null)
				{
					StopSource(component);
					RemovePlayingAudioSource(component);
				}
			}
			if (value2.farLoop != null)
			{
				AudioSource component2 = value2.farLoop.GetComponent<AudioSource>();
				if (component2 != null)
				{
					StopSource(component2);
					RemovePlayingAudioSource(component2);
				}
			}
			if (!(value2.nearEnd != null) && !(value2.farEnd != null))
			{
				continue;
			}
			AudioSource audioSource = (value2.nearEnd ? value2.nearEnd.GetComponent<AudioSource>() : null);
			AudioSource audioSource2 = (value2.farEnd ? value2.farEnd.GetComponent<AudioSource>() : null);
			if (!(audioSource != null) && !(audioSource2 != null))
			{
				continue;
			}
			if (audioData.TryGetValue(item.Key, out var value3) && !value3.playImmediate)
			{
				if (audioSource != null)
				{
					audioSource.volume *= CalculateOcclusion(entity.position - Origin.position, currentListenerPosition);
					PlaySource(audioSource);
					AddPlayingAudioSource(audioSource);
				}
				if (audioSource2 != null)
				{
					audioSource2.volume *= CalculateOcclusion(entity.position - Origin.position, currentListenerPosition);
					PlaySource(audioSource2);
					AddPlayingAudioSource(audioSource2);
				}
			}
			SequenceStopper value5;
			if (stoppedEntitySequences.TryGetValue(entity.entityId, out var value4))
			{
				if (value4.TryGetValue(item.Key, out value5))
				{
					for (int i = 0; i < value5.sequenceObjs.Count; i++)
					{
						UnityEngine.Object.Destroy(value5.sequenceObjs[i]);
					}
					value4.Remove(item.Key);
				}
			}
			else
			{
				value4 = new Dictionary<string, SequenceStopper>();
				stoppedEntitySequences.Add(entity.entityId, value4);
			}
			if (!value4.TryGetValue(item.Key, out value5))
			{
				List<GameObject> list = new List<GameObject>();
				if (value2.nearStart != null)
				{
					list.Add(value2.nearStart);
				}
				if (value2.nearLoop != null)
				{
					list.Add(value2.nearLoop);
				}
				if (value2.nearEnd != null)
				{
					list.Add(value2.nearEnd);
				}
				if (value2.farStart != null)
				{
					list.Add(value2.farStart);
				}
				if (value2.farLoop != null)
				{
					list.Add(value2.farLoop);
				}
				if (value2.farEnd != null)
				{
					list.Add(value2.farEnd);
				}
				value5 = new SequenceStopper(list, Time.time + value2.longestClipLength);
				value4.Add(item.Key, value5);
			}
		}
		value.Clear();
	}

	public static void StopSequence(Entity entity, string soundGroupName)
	{
		if (GameManager.IsDedicatedServer || soundGroupName == null)
		{
			return;
		}
		ConvertName(ref soundGroupName, entity);
		if (!sequenceOnEntity.TryGetValue(entity.entityId, out var value) || !value.TryGetValue(soundGroupName, out var value2))
		{
			return;
		}
		if (value2.nearLoop != null)
		{
			AudioSource component = value2.nearLoop.GetComponent<AudioSource>();
			if (component != null)
			{
				StopSource(component);
				RemovePlayingAudioSource(component);
			}
		}
		if (value2.farLoop != null)
		{
			AudioSource component2 = value2.farLoop.GetComponent<AudioSource>();
			if (component2 != null)
			{
				StopSource(component2);
				RemovePlayingAudioSource(component2);
			}
		}
		if (value2.nearEnd != null || value2.farEnd != null)
		{
			AudioSource audioSource = (value2.nearEnd ? value2.nearEnd.GetComponent<AudioSource>() : null);
			AudioSource audioSource2 = (value2.farEnd ? value2.farEnd.GetComponent<AudioSource>() : null);
			if (audioSource != null || audioSource2 != null)
			{
				if (audioData.TryGetValue(soundGroupName, out var value3) && !value3.playImmediate)
				{
					if (audioSource != null)
					{
						audioSource.volume *= CalculateOcclusion(entity.position - Origin.position, currentListenerPosition);
						PlaySource(audioSource);
						AddPlayingAudioSource(audioSource);
					}
					if (audioSource2 != null)
					{
						audioSource2.volume *= CalculateOcclusion(entity.position - Origin.position, currentListenerPosition);
						PlaySource(audioSource2);
						AddPlayingAudioSource(audioSource2);
					}
				}
				SequenceStopper value5;
				if (stoppedEntitySequences.TryGetValue(entity.entityId, out var value4))
				{
					if (value4.TryGetValue(soundGroupName, out value5))
					{
						for (int i = 0; i < value5.sequenceObjs.Count; i++)
						{
							UnityEngine.Object.Destroy(value5.sequenceObjs[i]);
						}
						value4.Remove(soundGroupName);
					}
				}
				else
				{
					value4 = new Dictionary<string, SequenceStopper>();
					stoppedEntitySequences.Add(entity.entityId, value4);
				}
				if (!value4.TryGetValue(soundGroupName, out value5))
				{
					List<GameObject> list = new List<GameObject>();
					if (value2.nearStart != null)
					{
						list.Add(value2.nearStart);
					}
					if (value2.nearLoop != null)
					{
						list.Add(value2.nearLoop);
					}
					if (value2.nearEnd != null)
					{
						list.Add(value2.nearEnd);
					}
					if (value2.farStart != null)
					{
						list.Add(value2.farStart);
					}
					if (value2.farLoop != null)
					{
						list.Add(value2.farLoop);
					}
					if (value2.farEnd != null)
					{
						list.Add(value2.farEnd);
					}
					value5 = new SequenceStopper(list, Time.time + value2.longestClipLength);
					value4.Add(soundGroupName, value5);
				}
			}
		}
		value.Remove(soundGroupName);
	}

	public static void DestroySoundsForEntity(int entityId)
	{
		if (playingOnEntity.TryGetValue(entityId, out var value))
		{
			if (value.environment != null)
			{
				for (int i = 0; i < value.environment.list.Count; i++)
				{
					for (int j = 0; j < value.environment.list[i].Count; j++)
					{
						if (value.environment.list[i][j] != null)
						{
							StopSource(value.environment.list[i][j]);
							RemovePlayingAudioSource(value.environment.list[i][j]);
						}
					}
				}
			}
			if (value.mouth != null)
			{
				for (int k = 0; k < value.mouth.Count; k++)
				{
					if (value.mouth[k] != null)
					{
						if (k < value.mouth.Count - 1)
						{
							StopSource(value.mouth[k]);
						}
						RemovePlayingAudioSource(value.mouth[k]);
					}
				}
			}
		}
		playingOnEntity.Remove(entityId);
		if (sequenceOnEntity.TryGetValue(entityId, out var value2))
		{
			foreach (KeyValuePair<string, SequenceGOs> item in value2)
			{
				UnityEngine.Object.Destroy(item.Value.nearStart);
				UnityEngine.Object.Destroy(item.Value.nearLoop);
				UnityEngine.Object.Destroy(item.Value.nearEnd);
				UnityEngine.Object.Destroy(item.Value.farStart);
				UnityEngine.Object.Destroy(item.Value.farLoop);
				UnityEngine.Object.Destroy(item.Value.farEnd);
			}
			sequenceOnEntity.Remove(entityId);
		}
		if (!loopingOnEntity.TryGetValue(entityId, out var value3))
		{
			return;
		}
		foreach (KeyValuePair<string, NearAndFarGO> item2 in value3)
		{
			if (item2.Value.near != null)
			{
				AudioSource component = item2.Value.near.GetComponent<AudioSource>();
				if (component != null)
				{
					RemovePlayingAudioSource(component);
				}
				UnityEngine.Object.Destroy(item2.Value.near);
			}
			if (item2.Value.far != null)
			{
				AudioSource component2 = item2.Value.far.GetComponent<AudioSource>();
				if (component2 != null)
				{
					RemovePlayingAudioSource(component2);
				}
				UnityEngine.Object.Destroy(item2.Value.far);
			}
		}
		loopingOnEntity.Remove(entityId);
	}

	public static void SignalAI(Entity _entity, Vector3 _position, string _soundName, float volumeScale)
	{
		if (_entity is EntityPlayer && GameManager.Instance != null)
		{
			World world = GameManager.Instance.World;
			if (world != null && world.aiDirector != null)
			{
				world.aiDirector.OnSoundPlayedAtPosition(_entity.entityId, _position, _soundName, volumeScale);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void StripOffDirectories(ref string _name)
	{
		_name = Path.GetFileName(_name).ToLower();
	}

	public static void CameraChanged()
	{
		if ((bool)mainCamera)
		{
			DopplerCheckForMove();
		}
	}

	public static void FrameUpdate()
	{
		if (GameManager.IsDedicatedServer || GameManager.Instance == null || GameManager.Instance.World == null)
		{
			return;
		}
		listIntRemove.Clear();
		listStringRemove.Clear();
		removeSequenceStopper.Clear();
		foreach (KeyValuePair<int, Dictionary<string, SequenceStopper>> stoppedEntitySequence in stoppedEntitySequences)
		{
			removeSequenceStopper.Clear();
			foreach (KeyValuePair<string, SequenceStopper> item in stoppedEntitySequence.Value)
			{
				if (item.Value.stopTime < Time.time)
				{
					for (int i = 0; i < item.Value.sequenceObjs.Count; i++)
					{
						UnityEngine.Object.Destroy(item.Value.sequenceObjs[i]);
					}
					removeSequenceStopper.Add(item.Key);
				}
			}
			for (int j = 0; j < removeSequenceStopper.Count; j++)
			{
				string key = removeSequenceStopper[j];
				stoppedEntitySequence.Value.Remove(key);
			}
			if (stoppedEntitySequence.Value.Count < 1)
			{
				listIntRemove.Add(stoppedEntitySequence.Key);
			}
		}
		for (int k = 0; k < listIntRemove.Count; k++)
		{
			int key2 = listIntRemove[k];
			stoppedEntitySequences.Remove(key2);
		}
		listIntRemove.Clear();
		if (Time.time > fadeOutUpdateTime + 0.16f)
		{
			foreach (KeyValuePair<int, Dictionary<string, NearAndFarGO>> item2 in fadingOutOnEntity)
			{
				listStringRemove.Clear();
				foreach (KeyValuePair<string, NearAndFarGO> item3 in item2.Value)
				{
					if (item3.Value.near == null && item3.Value.far == null)
					{
						listStringRemove.Add(item3.Key);
						continue;
					}
					AudioSource component = item3.Value.near.GetComponent<AudioSource>();
					if (component == null)
					{
						component = item3.Value.far.GetComponent<AudioSource>();
					}
					if (component != null)
					{
						component.volume *= 0.99f;
						if (component.volume < 0.01f)
						{
							Stop(item2.Key, item3.Key);
							listStringRemove.Add(item3.Key);
						}
					}
				}
				for (int l = 0; l < listStringRemove.Count; l++)
				{
					string key3 = listStringRemove[l];
					item2.Value.Remove(key3);
				}
				if (item2.Value.Count == 0)
				{
					listIntRemove.Add(item2.Key);
				}
			}
			for (int m = 0; m < listIntRemove.Count; m++)
			{
				int key4 = listIntRemove[m];
				fadingOutOnEntity.Remove(key4);
			}
		}
		EntityPlayerLocal entityPlayerLocal = LocalPlayer();
		if (!entityPlayerLocal)
		{
			return;
		}
		if (!mainCamera)
		{
			mainCamera = entityPlayerLocal.playerCamera;
			if (!mainCamera)
			{
				return;
			}
		}
		updateCurrentListener();
		bool isUnderwaterCamera = entityPlayerLocal.IsUnderwaterCamera;
		if (bCameraWasUnderwater && !isUnderwaterCamera && underwaterSoundID >= 0)
		{
			Stop(entityPlayerLocal.entityId, "underwater_lp");
			underwaterSoundID = -1;
		}
		else if (!bCameraWasUnderwater && isUnderwaterCamera)
		{
			Play(entityPlayerLocal, "underwater_lp");
			underwaterSoundID = 0;
		}
		bCameraWasUnderwater = isUnderwaterCamera;
		if (occlusionsOn)
		{
			foreach (KeyValuePair<int, Dictionary<string, NearAndFarGO>> item4 in loopingOnEntity)
			{
				foreach (KeyValuePair<string, NearAndFarGO> item5 in item4.Value)
				{
					if (item5.Value.near != null)
					{
						AudioSource component2 = item5.Value.near.GetComponent<AudioSource>();
						if (component2 != null && audioSourceDatas.TryGetValue(component2.name.Replace("(Clone)", ""), out var value))
						{
							component2.volume = component2.volume / value.maxVolume * value.maxVolume * CalculateOcclusion(item5.Value.near.transform.position, currentListenerPosition);
						}
					}
					if (item5.Value.far != null)
					{
						AudioSource component3 = item5.Value.far.GetComponent<AudioSource>();
						if (component3 != null && audioSourceDatas.TryGetValue(component3.name.Replace("(Clone)", ""), out var value2))
						{
							component3.volume = component3.volume / value2.maxVolume * value2.maxVolume * CalculateOcclusion(item5.Value.far.transform.position, currentListenerPosition);
						}
					}
				}
			}
			foreach (KeyValuePair<Vector3, Dictionary<string, NearAndFarGO>> item6 in loopingOnPosition)
			{
				foreach (KeyValuePair<string, NearAndFarGO> item7 in item6.Value)
				{
					if (item7.Value.near != null)
					{
						AudioSource component4 = item7.Value.near.GetComponent<AudioSource>();
						if (component4 != null && audioSourceDatas.TryGetValue(component4.name.Replace("(Clone)", ""), out var value3))
						{
							component4.volume = component4.volume / value3.maxVolume * value3.maxVolume * CalculateOcclusion(item7.Value.near.transform.position, currentListenerPosition);
						}
					}
					if (item7.Value.far != null)
					{
						AudioSource component5 = item7.Value.far.GetComponent<AudioSource>();
						if (component5 != null && audioSourceDatas.TryGetValue(component5.name.Replace("(Clone)", ""), out var value4))
						{
							component5.volume = component5.volume / value4.maxVolume * value4.maxVolume * CalculateOcclusion(item7.Value.far.transform.position, currentListenerPosition);
						}
					}
				}
			}
		}
		if (dopplerDelay > 0 && --dopplerDelay == 0)
		{
			DopplerRestore();
		}
	}

	public static void PlayButtonClick()
	{
		if (GameManager.Instance != null && GameManager.Instance.World != null)
		{
			PlayInsidePlayerHead("Sounds/Misc/buttonclick");
		}
	}

	public static void PlayXUiSound(AudioClip _sound, float _volume)
	{
		if (GameManager.Instance.UIAudioSource == null)
		{
			GameManager.Instance.UIAudioSource = GameManager.Instance.transform.gameObject.AddComponent<AudioSource>();
		}
		if (_sound != null && GameManager.Instance.UIAudioSource != null)
		{
			if (GameManager.Instance.World != null && LocalPlayer() != null)
			{
				GameManager.Instance.UIAudioSource.minDistance = 1f;
				GameManager.Instance.UIAudioSource.transform.position = LocalPlayer().transform.position;
			}
			GameManager.Instance.UIAudioSource.PlayOneShot(_sound, _volume);
		}
	}

	public static void PlaySource(AudioSource src)
	{
		if ((bool)src)
		{
			src.Play();
		}
	}

	public static void StopSource(AudioSource src)
	{
		if ((bool)src)
		{
			src.Stop();
		}
	}

	public void StopDistantLoopingPositionalSounds(Vector3 localPlayerPosition)
	{
		AudioSource[] componentsInChildren = PositionalSoundsPlaying.GetComponentsInChildren<AudioSource>();
		foreach (AudioSource audioSource in componentsInChildren)
		{
			if (audioSource.loop && (audioSource.transform.position - localPlayerPosition).magnitude > audioSource.maxDistance)
			{
				StopSource(audioSource);
			}
		}
	}

	public void RestartNearbyLoopingPositionalSounds(Vector3 localPlayerPosition)
	{
		AudioSource[] componentsInChildren = PositionalSoundsPlaying.GetComponentsInChildren<AudioSource>();
		foreach (AudioSource audioSource in componentsInChildren)
		{
			if (audioSource.loop && !audioSource.isPlaying && (audioSource.transform.position - localPlayerPosition).magnitude <= audioSource.maxDistance)
			{
				PlaySource(audioSource);
			}
		}
	}

	public void Dispose()
	{
		if (ServerAudio != null)
		{
			ServerAudio.Dispose();
			ServerAudio = null;
		}
	}

	public void AttachLocalPlayer(EntityPlayerLocal localPlayer, World world)
	{
		if (ServerAudio != null)
		{
			ServerAudio.AttachLocalPlayer(localPlayer);
		}
	}

	public void EntityAddedToWorld(Entity entity, World world)
	{
		if (ServerAudio != null)
		{
			ServerAudio.EntityAddedToWorld(entity, world);
		}
	}

	public void EntityRemovedFromWorld(Entity entity, World world)
	{
		if (!(entity == null) && ServerAudio != null)
		{
			ServerAudio.EntityRemovedFromWorld(entity, world);
		}
	}

	public static void CreateServer()
	{
		if (ServerAudio == null && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			ServerAudio = new Server();
		}
	}

	public static string GetFormattedSubtitleSpeaker(string subtitleId)
	{
		string text = "";
		SubtitleData subtitleData = GetSubtitleData(subtitleId);
		if (subtitleData != null && !string.IsNullOrEmpty(subtitleData.speakerLocId))
		{
			text = Localization.Get(subtitleData.speakerLocId);
			if (!string.IsNullOrEmpty(subtitleData.speakerColorId))
			{
				string subtitleSpeakerColor = GetSubtitleSpeakerColor(subtitleData.speakerColorId);
				text = $"[{subtitleSpeakerColor}]{text}:[-]";
			}
		}
		return text;
	}

	public static string GetFormattedSubtitle(string subtitleId)
	{
		string result = "";
		SubtitleData subtitleData = GetSubtitleData(subtitleId);
		if (subtitleData != null)
		{
			result = Localization.Get(subtitleData.contentLocId);
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static SubtitleData GetSubtitleData(string subtitleId)
	{
		if (subtitleCache.TryGetValue(subtitleId, out var value))
		{
			return value;
		}
		Log.Error("Could not retrieve subtitle data for ID " + subtitleId);
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetSubtitleSpeakerColor(string speaker)
	{
		if (subtitleSpeakerColorCache.TryGetValue(speaker, out var value))
		{
			return value;
		}
		Log.Error("Could not retrieve subtitle speaker color for ID " + speaker);
		return "#FFFFFF";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void updateCurrentListener()
	{
		currentListenerPosition = mainCamera.transform.position;
	}
}
