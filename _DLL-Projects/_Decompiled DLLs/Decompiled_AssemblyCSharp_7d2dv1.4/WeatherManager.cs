using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum CloudTypes
	{
		Whispy,
		Fluffy,
		ThickOvercast
	}

	public class temperatureOffsetHeightsComparer : IComparer<Vector2>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		int IComparer<Vector2>.Compare(Vector2 x, Vector2 y)
		{
			if (!(y.x < x.x))
			{
				if (!(y.x > x.x))
				{
					return 0;
				}
				return -1;
			}
			return 1;
		}
	}

	[Serializable]
	public class Param
	{
		public string name;

		public float value;

		public float target;

		public float step1Time;

		[NonSerialized]
		[PublicizedFrom(EAccessModifier.Private)]
		public float lastTime;

		public void Clamp()
		{
			value = Mathf.Clamp01(value);
			target = Mathf.Clamp01(target);
		}

		public Param(float _value, float _step1Time = 0.25f)
		{
			name = "Param";
			value = _value;
			target = _value;
			step1Time = _step1Time;
		}

		public void FrameUpdate()
		{
			float time = Time.time;
			if (value == target)
			{
				lastTime = time;
				return;
			}
			float num = time - lastTime;
			if (num < 0f)
			{
				lastTime = time;
			}
			if (!(num >= 0.01f))
			{
				return;
			}
			if (num > 1f)
			{
				num = 1f;
			}
			float num2 = num / step1Time;
			if (value > target)
			{
				value -= num2;
				if (value < target)
				{
					value = target;
				}
			}
			else
			{
				value += num2;
				if (value > target)
				{
					value = target;
				}
			}
			lastTime = time;
		}

		public void Reset()
		{
			Set(0f);
			lastTime = Time.time;
		}

		public void Set(float _value)
		{
			value = _value;
			target = _value;
		}

		public void SetTarget(float _target)
		{
			if (target != _target)
			{
				target = _target;
				lastTime = Time.time;
			}
		}
	}

	[Serializable]
	public class BiomeWeather
	{
		[NonSerialized]
		[PublicizedFrom(EAccessModifier.Private)]
		public const float cStep1Time = 0.15f;

		[NonSerialized]
		[PublicizedFrom(EAccessModifier.Private)]
		public const float cPrecipitationPercentVisibleMin = 0.3f;

		public BiomeDefinition biomeDefinition;

		public Param[] parameters = new Param[5];

		public float[] parameterFinals = new float[5];

		public Param rainParam = new Param(0f);

		public Param wetParam = new Param(0f);

		public Param snowCoverParam = new Param(0f);

		public Param snowFallParam = new Param(0f);

		public BiomeWeather(BiomeDefinition _definition = null)
		{
			biomeDefinition = _definition;
			for (int i = 0; i < 5; i++)
			{
				parameters[i] = new Param(0f, 0.3f);
			}
			parameters[0].step1Time = 0.15f;
		}

		public float CloudThickness()
		{
			return parameters[2].value;
		}

		public float FogPercent()
		{
			return parameters[4].value * 0.01f;
		}

		public float Wind()
		{
			return parameters[3].value;
		}

		public void Normalize()
		{
			rainParam.Clamp();
			wetParam.Clamp();
			snowCoverParam.Clamp();
			snowFallParam.Clamp();
		}

		public void ServerFrameUpdate()
		{
			Param[] array = parameters;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].FrameUpdate();
			}
			float _precipitation = parameters[1].value;
			float _cloudThickness = parameters[2].value;
			float _temperature = parameters[0].value;
			Instance.CalcGlobalPrecipCloudsTemperature(ref _precipitation, ref _cloudThickness, ref _temperature);
			_precipitation = biomeDefinition.WeatherClampToPossibleValues(_precipitation, BiomeDefinition.Probabilities.ProbType.Precipitation);
			_cloudThickness = biomeDefinition.WeatherClampToPossibleValues(_cloudThickness, BiomeDefinition.Probabilities.ProbType.CloudThickness);
			_temperature = biomeDefinition.WeatherClampToPossibleValues(_temperature, globalTemperatureOffset, BiomeDefinition.Probabilities.ProbType.Temperature);
			if (forceClouds >= 0f)
			{
				_cloudThickness = forceClouds * 100f;
			}
			if (forceTemperature > -100f)
			{
				_temperature = forceTemperature;
			}
			parameterFinals[1] = _precipitation;
			parameterFinals[2] = _cloudThickness;
			parameterFinals[0] = _temperature;
			parameterFinals[3] = parameters[3].value;
			parameterFinals[4] = parameters[4].value;
			float num = (_precipitation * 0.01f - 0.3f) / 0.7f;
			rainParam.SetTarget((num > 0f && _temperature > 32f) ? num : 0f);
			float num2 = rainParam.value;
			if (forceRain >= 0f)
			{
				num2 = forceRain;
			}
			if (num2 > 0.5f && _cloudThickness >= 70f)
			{
				isThunderWeather = true;
			}
			float sunPercent = SkyManager.GetSunPercent();
			float num3 = Mathf.Clamp01(sunPercent);
			wetParam.SetTarget((num2 > 0f) ? 1 : 0);
			float num4 = 10f;
			wetParam.step1Time = num4;
			wetParam.step1Time -= num4 * 0.5f * num3 * (float)((wetParam.target < wetParam.value) ? 1 : 0);
			snowFallParam.SetTarget((num > 0f && _temperature <= 32f) ? num : 0f);
			float num5 = snowFallParam.value;
			if (forceSnowfall >= 0f)
			{
				num5 = forceSnowfall;
			}
			float num6 = ((_temperature <= 32f) ? ((32f - _temperature) / 32f) : 0f) * 0.15f;
			num6 *= 1f - Mathf.Clamp01(sunPercent * 8f);
			snowCoverParam.SetTarget((num5 > 0f) ? 1f : num6);
			float sSnowAccumulationSpeed = WeatherManager.sSnowAccumulationSpeed;
			snowCoverParam.step1Time = sSnowAccumulationSpeed * 0.5f * num2;
			snowCoverParam.step1Time -= sSnowAccumulationSpeed * 0.25f * num3 * (float)((snowCoverParam.target < snowCoverParam.value) ? 1 : 0);
			rainParam.FrameUpdate();
			wetParam.FrameUpdate();
			snowCoverParam.FrameUpdate();
			snowFallParam.FrameUpdate();
		}

		public void ParamsFrameUpdate()
		{
			Param[] array = parameters;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].FrameUpdate();
			}
			rainParam.FrameUpdate();
			wetParam.FrameUpdate();
			snowCoverParam.FrameUpdate();
			snowFallParam.FrameUpdate();
		}

		public void Reset()
		{
			if (biomeDefinition != null)
			{
				biomeDefinition.WeatherRandomize(0f);
				for (int i = 0; i < 5; i++)
				{
					parameters[i].Set(biomeDefinition.WeatherGetValue((BiomeDefinition.Probabilities.ProbType)i));
				}
				rainParam.Set(0f);
				wetParam.Set(0f);
				snowCoverParam.Set(0f);
				snowFallParam.Set(0f);
			}
		}

		public void Randomize()
		{
			if (biomeDefinition != null)
			{
				float rand = GameManager.Instance.World.GetGameRandom().RandomFloat;
				if (forceSimRandom >= 0f)
				{
					rand = forceSimRandom;
				}
				biomeDefinition.WeatherRandomize(rand);
				for (int i = 0; i < 5; i++)
				{
					BiomeDefinition.Probabilities.ProbType type = (BiomeDefinition.Probabilities.ProbType)i;
					parameters[i].target = biomeDefinition.WeatherGetValue(type);
				}
			}
		}

		public void ForceWeather(string name)
		{
			if (biomeDefinition != null)
			{
				GeneralReset();
				biomeDefinition.WeatherRandomize(name);
				for (int i = 0; i < 5; i++)
				{
					BiomeDefinition.Probabilities.ProbType type = (BiomeDefinition.Probabilities.ProbType)i;
					parameters[i].target = biomeDefinition.WeatherGetValue(type);
				}
			}
		}

		public override string ToString()
		{
			string text = $"{biomeDefinition.m_sBiomeName}: {biomeDefinition.weatherName}, ";
			for (int i = 0; i < 5; i++)
			{
				BiomeDefinition.Probabilities.ProbType probType = (BiomeDefinition.Probabilities.ProbType)i;
				text += $"{probType} {biomeDefinition.WeatherGetValue(probType)}, ";
			}
			text += $"rain {rainParam.value}, ";
			text += $"wet {wetParam.value}, ";
			text += $"snowCover {snowCoverParam.value}, ";
			return text + $"snowFall {snowFallParam.value}";
		}
	}

	[Serializable]
	public class CurrentBiome
	{
		public string name;

		public float intensity;
	}

	public static WeatherManager Instance;

	public const int BaseTemperature = 70;

	public List<BiomeWeather> biomeWeather;

	public static List<WeatherPackage> savedWeather = new List<WeatherPackage>();

	public static ulong worldTime;

	public static float forceClouds = -1f;

	public static float forceRain = -1f;

	public static float forceWet = -1f;

	public static float forceSnow = -1f;

	public static float forceSnowfall = -1f;

	public const float cForceTempDefault = -100f;

	public static float forceTemperature = -100f;

	public static float forceWind = -1f;

	public static bool needToReUpdateWeatherSpectrums;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float forceSimRandom = -1f;

	public static float globalTemperatureOffset;

	public static float globalRainDayStart = -1f;

	public static float globalRainDayPeak;

	public static float globalRainPercent;

	public static float globalRainOLD1;

	public static float globalRainOLD2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Vector2> temperatureOffsetHeights = new List<Vector2>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool hasCreatedSeaLevel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float seaLevel = 0f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isGameModeNormal;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public WeatherPackage[] weatherPackages;

	public static bool inWeatherGracePeriod = true;

	public static BiomeWeather currentWeather;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 thunderFreq = new Vector2(30f, 60f);

	public static ulong sLightningWorldTime;

	public static Vector3 sLightningPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPlayThunder;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float thunderLastTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float thunderDelay = 10f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cWeatherTransitionSeconds = 10f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float sSnowAccumulationSpeed = 150f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cWeatherChangeFrequency = 1500f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int frameCount = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Entity> players = new List<Entity>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] strCloudTypes = new string[3] { "Whispy", "Fluffy", "ThickOvercast" };

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture[] clouds = new Texture[strCloudTypes.Length];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isCurrentWeatherUpdatedFirstTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cWindMax = 100f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cWindScale = 0.01f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject windZoneObj;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public WindZone windZone;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float windSpeed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float windSpeedPrevious;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float windTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float windTimePrevious;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float windGust;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float windGustStep;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float windGustTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float windGustTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Camera mainCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Texture noiseTexture;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Texture snowTexture;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int raycastMask;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject rainParticleObj;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ParticleSystem rainParticleSys;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Material rainParticleMat;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float rainEmissionMaxRate = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject snowParticleObj;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform snowParticleT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Material snowParticleMat;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ParticleSystem snowParticleSys;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float snowEmissionMaxRate = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ParticleSystem snowNearParticleSys;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float snowNearEmissionMaxRate = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ParticleSystem snowTopParticleSys;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float snowTopEmissionMaxRate = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ParticleSystem snowFarParticleSys;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Color snowFarBaseColor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform snowPlayerForceT;

	public float spectrumBlend = 1f;

	public static SpectrumWeatherType forcedSpectrum = SpectrumWeatherType.None;

	public SpectrumWeatherType spectrumSourceType;

	public SpectrumWeatherType spectrumTargetType;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static AtmosphereEffect[] atmosphereSpectrum;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 playerPosition = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float checkPlayerMoveTime;

	public CurrentBiome[] editorNearBiomes = new CurrentBiome[4];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTimeScale = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTemperatureChangeDuration = 5000f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRainStartMin = 0.375f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRainStartMax = 2f / 3f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRainPeakMin = 1f / 48f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRainPeakMax = 0.09583333f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRainFade = 0.029166667f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastWorldTimeWeatherChanged;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float temperatureStart = globalTemperatureOffset;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float temperatureTarget = globalTemperatureOffset;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ulong temperatureStartWorldTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isThunderWeather;

	public string CustomWeatherName = "";

	public float CustomWeatherTime = -1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float cloudThickness;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float cloudThicknessTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float particleFallHitTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 particleFallLastPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 particleFallPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int processingPackageFrame = -1;

	public static void Init(World _world, GameObject _obj)
	{
		Cleanup();
		Instance = _obj.GetComponent<WeatherManager>();
		Instance.Init(_world);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init(World _world)
	{
		world = _world;
		string value = GamePrefs.GetString(EnumGamePrefs.GameMode);
		isGameModeNormal = !GameModeEditWorld.TypeName.Equals(value) && !GameModeCreative.TypeName.Equals(value);
		InitBiomeWeather();
		InitWeatherPackages();
		currentWeather = new BiomeWeather();
		currentWeather.biomeDefinition = world.Biomes.GetBiome(3);
		currentWeather.parameters[2].name = "CloudThickness";
		currentWeather.parameters[4].name = "Fog";
		currentWeather.parameters[1].name = "Precipitation";
		currentWeather.parameters[0].name = "Temperature";
		currentWeather.parameters[3].name = "Wind";
		currentWeather.rainParam.name = "Rain";
		currentWeather.wetParam.name = "Wet";
		currentWeather.snowCoverParam.name = "SnowCover";
		currentWeather.snowFallParam.name = "SnowFall";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitBiomeWeather()
	{
		biomeWeather = new List<BiomeWeather>();
		foreach (KeyValuePair<uint, BiomeDefinition> item2 in world.Biomes.GetBiomeMap())
		{
			BiomeWeather item = new BiomeWeather(item2.Value);
			biomeWeather.Add(item);
		}
		for (int i = 0; i < biomeWeather.Count; i++)
		{
			biomeWeather[i].Reset();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitWeatherPackages()
	{
		int count = world.Biomes.GetBiomeMap().Count;
		Log.Out("WeatherManager: Init {0} weather packages", count);
		weatherPackages = new WeatherPackage[count];
		for (int i = 0; i < count; i++)
		{
			weatherPackages[i] = new WeatherPackage();
		}
	}

	public static void Cleanup()
	{
		if (Instance != null)
		{
			UnityEngine.Object.DestroyImmediate(Instance.gameObject);
			forceClouds = -1f;
			forceRain = -1f;
			forceWet = -1f;
			forceSnow = -1f;
			forceSnowfall = -1f;
			forceTemperature = -100f;
			forceWind = -1f;
		}
		players.Clear();
	}

	public static void ClearTemperatureOffSetHeights()
	{
		temperatureOffsetHeights.Clear();
	}

	public static void AddTemperatureOffSetHeight(float height, float degreesOffset)
	{
		temperatureOffsetHeights.Add(new Vector2(height, degreesOffset));
		IComparer<Vector2> comparer = new temperatureOffsetHeightsComparer();
		temperatureOffsetHeights.Sort(comparer);
	}

	public static float SeaLevel()
	{
		if (!hasCreatedSeaLevel)
		{
			if (temperatureOffsetHeights.Count < 1)
			{
				return 0f;
			}
			hasCreatedSeaLevel = true;
			int num = 0;
			for (int i = 1; i < temperatureOffsetHeights.Count; i++)
			{
				if (Mathf.Abs(temperatureOffsetHeights[i].y) < Mathf.Abs(temperatureOffsetHeights[num].y))
				{
					num = i;
				}
			}
			if (temperatureOffsetHeights[num].y < 0f && num < temperatureOffsetHeights.Count - 1)
			{
				float num2 = Mathf.Abs(temperatureOffsetHeights[num].y) / (temperatureOffsetHeights[num + 1].y + Mathf.Abs(temperatureOffsetHeights[num].y));
				seaLevel = temperatureOffsetHeights[num].x * (1f - num2) + temperatureOffsetHeights[num + 1].x * num2;
			}
			else if (temperatureOffsetHeights[num].y < 0f)
			{
				seaLevel = temperatureOffsetHeights[num].x + Mathf.Abs(temperatureOffsetHeights[num].y);
			}
			else if (temperatureOffsetHeights[num].y >= 0f && num > 0)
			{
				float num3 = Mathf.Abs(temperatureOffsetHeights[num - 1].y) / (Mathf.Abs(temperatureOffsetHeights[num - 1].y) + temperatureOffsetHeights[num].y);
				seaLevel = temperatureOffsetHeights[num - 1].x * (1f - num3) + temperatureOffsetHeights[num].x * num3;
			}
			else if (temperatureOffsetHeights[num].y >= 0f)
			{
				seaLevel = temperatureOffsetHeights[num].x - temperatureOffsetHeights[num].y;
			}
		}
		return seaLevel;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcGlobalPrecipCloudsTemperature(ref float _precipitation, ref float _cloudThickness, ref float _temperature)
	{
		_precipitation += globalRainPercent * 50f;
		_cloudThickness += globalRainPercent * 50f;
		_precipitation = Mathf.Clamp(_precipitation, 0f, 100f);
		_cloudThickness = Mathf.Clamp(_cloudThickness, 0f, 100f);
		_temperature += globalTemperatureOffset;
		_temperature -= _precipitation * 0.1f;
		float num = SkyManager.GetSunPercent();
		if (num > 0f)
		{
			num *= 1f - _cloudThickness * 0.01f;
		}
		_temperature += num * 20f;
	}

	public static float GetCloudThickness()
	{
		if (forceClouds >= 0f)
		{
			return forceClouds * 100f;
		}
		if (currentWeather == null)
		{
			return 0f;
		}
		return currentWeather.parameters[2].value;
	}

	public static float GetTemperature()
	{
		if (forceTemperature > -100f)
		{
			return forceTemperature;
		}
		if (currentWeather == null)
		{
			return 0f;
		}
		return currentWeather.parameters[0].value;
	}

	public static float GetWindSpeed()
	{
		if (forceWind >= 0f)
		{
			return forceWind;
		}
		if (currentWeather == null)
		{
			return 0f;
		}
		return currentWeather.parameters[3].value;
	}

	public static void EntityAddedToWorld(Entity entity)
	{
		if (entity != null)
		{
			players.Add(entity);
		}
	}

	public static void EntityRemovedFromWorld(Entity entity)
	{
		if (entity != null)
		{
			players.Remove(entity);
		}
	}

	public float GetCurrentSnowfallValue()
	{
		if (!(forceSnowfall >= 0f) && currentWeather != null)
		{
			return currentWeather.snowFallParam.value;
		}
		return forceSnowfall;
	}

	public static float GetCurrentSnowValue()
	{
		if (!(forceSnow >= 0f) && currentWeather != null)
		{
			return currentWeather.snowCoverParam.value;
		}
		return forceSnow;
	}

	public float GetCurrentRainfallValue()
	{
		if (!(forceRain >= 0f) && currentWeather != null)
		{
			return currentWeather.rainParam.value;
		}
		return forceRain;
	}

	public float GetCurrentWetSurfaceValue()
	{
		if (!(forceWet >= 0f) && currentWeather != null)
		{
			return currentWeather.wetParam.value;
		}
		return forceWet;
	}

	public float GetCurrentCloudThicknessPercent()
	{
		return GetCloudThickness() * 0.01f;
	}

	public float GetCurrentTemperatureValue()
	{
		return GetTemperature();
	}

	public static void SetSimRandom(float _random)
	{
		forceSimRandom = _random;
		if ((bool)Instance)
		{
			Instance.lastWorldTimeWeatherChanged = 0f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadSpectrums()
	{
		if (atmosphereSpectrum == null)
		{
			atmosphereSpectrum = new AtmosphereEffect[Enum.GetNames(typeof(SpectrumWeatherType)).Length - 1];
			ReloadSpectrums();
		}
	}

	public static void ReloadSpectrums()
	{
		atmosphereSpectrum[1] = AtmosphereEffect.Load("Snowy", null);
		atmosphereSpectrum[2] = AtmosphereEffect.Load("Stormy", null);
		atmosphereSpectrum[3] = AtmosphereEffect.Load("Rainy", null);
		atmosphereSpectrum[4] = AtmosphereEffect.Load("Foggy", null);
		atmosphereSpectrum[5] = AtmosphereEffect.Load("BloodMoon", null);
	}

	public void Start()
	{
		windZoneObj = GameObject.Find("WindZone");
		if ((bool)windZoneObj)
		{
			windZone = windZoneObj.GetComponent<WindZone>();
		}
		LoadSpectrums();
		raycastMask = LayerMask.GetMask("Water", "NoShadow", "Items", "CC Physics", "TerrainCollision", "CC Physics Dead", "CC Local Physics") | 1;
		noiseTexture = Resources.Load<Texture>("Textures/Graphics/StipplingNoise");
		snowTexture = Resources.Load<Texture>("Textures/Graphics/Snow_n");
		string text = "Textures/Environment/Spectrums/Default/";
		for (int i = 0; i < strCloudTypes.Length; i++)
		{
			Texture texture = Resources.Load<Texture>(text + strCloudTypes[i] + "Clouds");
			clouds[i] = texture;
		}
	}

	public void PushTransitions()
	{
		spectrumBlend = 1f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CurrentWeatherFromNearBiomesFrameUpdate()
	{
		if (world.BiomeAtmosphereEffects == null)
		{
			return;
		}
		BiomeDefinition[] nearBiomes = world.BiomeAtmosphereEffects.nearBiomes;
		for (int i = 0; i < currentWeather.parameters.Length; i++)
		{
			currentWeather.parameters[i].target = 0f;
		}
		currentWeather.rainParam.target = 0f;
		currentWeather.snowFallParam.target = 0f;
		currentWeather.wetParam.target = 0f;
		currentWeather.snowCoverParam.target = 0f;
		float num = 0f;
		for (int j = 0; j < nearBiomes.Length; j++)
		{
			BiomeDefinition biomeDefinition = nearBiomes[j];
			CurrentBiome currentBiome = editorNearBiomes[j];
			if (biomeDefinition != null)
			{
				num += biomeDefinition.currentPlayerIntensity;
				currentBiome.name = biomeDefinition.m_sBiomeName;
				currentBiome.intensity = biomeDefinition.currentPlayerIntensity;
			}
			else
			{
				currentBiome.name = "null";
				currentBiome.intensity = 0f;
			}
		}
		inWeatherGracePeriod = (worldTime < 30000 || !isGameModeNormal) && CustomWeatherTime == -1f;
		if (inWeatherGracePeriod)
		{
			currentWeather.rainParam.Set(0f);
			currentWeather.snowFallParam.Set(0f);
			currentWeather.wetParam.Set(0f);
			currentWeather.snowCoverParam.Set(0f);
			int num2 = 0;
			float num3 = 0f;
			foreach (BiomeDefinition biomeDefinition2 in nearBiomes)
			{
				if (biomeDefinition2 != null && biomeDefinition2.currentPlayerIntensity > num3)
				{
					num3 = biomeDefinition2.currentPlayerIntensity;
					num2 = biomeDefinition2.m_Id;
				}
			}
			int num4 = 70;
			switch (num2)
			{
			case 1:
				num4 = 45;
				break;
			case 2:
			case 3:
				num4 = 60;
				break;
			}
			currentWeather.parameters[0].Set(num4);
			currentWeather.parameters[3].Set(8f);
			return;
		}
		currentWeather.biomeDefinition = nearBiomes[0];
		BiomeDefinition[] array = nearBiomes;
		foreach (BiomeDefinition biomeDefinition3 in array)
		{
			if (biomeDefinition3 == null)
			{
				continue;
			}
			if (!WorldBiomes.Instance.TryGetBiome(biomeDefinition3.m_Id, out var _bd))
			{
				_bd = biomeDefinition3;
			}
			float num5 = Mathf.Clamp01(biomeDefinition3.currentPlayerIntensity / num);
			for (int m = 0; m < currentWeather.parameters.Length; m++)
			{
				Param param = currentWeather.parameters[m];
				param.target += _bd.weatherPackage.param[m] * num5;
				if (!isCurrentWeatherUpdatedFirstTime)
				{
					param.value = param.target;
				}
			}
			currentWeather.rainParam.target += _bd.weatherPackage.particleRain * num5;
			currentWeather.wetParam.target += _bd.weatherPackage.surfaceWet * num5;
			currentWeather.snowCoverParam.target += _bd.weatherPackage.surfaceSnow * num5;
			currentWeather.snowFallParam.target += _bd.weatherPackage.particleSnow * num5;
			if (!isCurrentWeatherUpdatedFirstTime)
			{
				currentWeather.rainParam.value = Mathf.Clamp01(currentWeather.rainParam.target);
				currentWeather.wetParam.value = Mathf.Clamp01(currentWeather.wetParam.target);
				currentWeather.snowCoverParam.value = Mathf.Clamp01(currentWeather.snowCoverParam.target);
				currentWeather.snowFallParam.value = Mathf.Clamp01(currentWeather.snowFallParam.target);
			}
			currentWeather.Normalize();
		}
		isCurrentWeatherUpdatedFirstTime = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplySavedPackages()
	{
		Log.Out("WeatherManager: ApplySavedPackages");
		foreach (WeatherPackage item in savedWeather)
		{
			foreach (BiomeWeather item2 in biomeWeather)
			{
				if (item.biomeID == item2.biomeDefinition.m_Id)
				{
					for (int i = 0; i < 5; i++)
					{
						item2.biomeDefinition.WeatherSetValue((BiomeDefinition.Probabilities.ProbType)i, item.param[i]);
					}
					for (int j = 0; j < item.param.Length; j++)
					{
						item2.parameters[j].Set(item.param[j]);
					}
					item2.rainParam.Set(item.particleRain);
					item2.snowFallParam.Set(item.particleSnow);
					item2.wetParam.Set(item.surfaceWet);
					item2.snowCoverParam.Set(item.surfaceSnow);
					if (WorldBiomes.Instance.TryGetBiome(item.biomeID, out var _bd))
					{
						_bd.weatherPackage = item;
					}
					break;
				}
			}
		}
		savedWeather.Clear();
	}

	public static void SetSnowAccumulationSpeed(float _newSpeed)
	{
		sSnowAccumulationSpeed = _newSpeed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GenerateWeatherServerFrameUpdate()
	{
		if (inWeatherGracePeriod)
		{
			GeneralReset();
		}
		else
		{
			float num = (float)(long)(worldTime - temperatureStartWorldTime) / 5000f;
			globalTemperatureOffset = Mathf.Lerp(temperatureStart, temperatureTarget, num);
			if (num < 0f || num > 1f)
			{
				temperatureStartWorldTime = worldTime;
				temperatureStart = globalTemperatureOffset;
				temperatureTarget = world.RandomRange(-5f, 5f);
			}
			if (num < 0f)
			{
				VersionReset();
			}
			float num2 = SkyManager.dayCount - globalRainDayStart;
			if (num2 >= 0f)
			{
				globalRainPercent = num2 / (globalRainDayPeak - globalRainDayStart);
				float num3 = SkyManager.dayCount - globalRainDayPeak;
				if (num3 >= 0f)
				{
					globalRainPercent = 1f - num3 / 0.029166667f;
					if (globalRainPercent <= 0f)
					{
						globalRainPercent = 0f;
						globalRainDayStart = SkyManager.dayCount + world.RandomRange(0.375f, 2f / 3f);
						globalRainDayPeak = globalRainDayStart + world.RandomRange(1f / 48f, 0.09583333f);
					}
				}
			}
			isThunderWeather = SkyManager.IsBloodMoonVisible();
			if (CustomWeatherTime > 0f)
			{
				CustomWeatherTime -= Time.deltaTime;
				if (CustomWeatherTime <= 0f)
				{
					CustomWeatherName = "";
					CustomWeatherTime = -1f;
					GeneralReset();
					for (int i = 0; i < biomeWeather.Count; i++)
					{
						biomeWeather[i].ForceWeather("default");
					}
					lastWorldTimeWeatherChanged = worldTime;
				}
			}
			else if (Utils.FastAbs((float)worldTime - lastWorldTimeWeatherChanged) >= 1500f)
			{
				lastWorldTimeWeatherChanged = worldTime;
				for (int j = 0; j < biomeWeather.Count; j++)
				{
					biomeWeather[j].Randomize();
				}
			}
		}
		for (int k = 0; k < biomeWeather.Count; k++)
		{
			biomeWeather[k].ServerFrameUpdate();
		}
	}

	public void HandleBiomeChanging(EntityPlayer _player, BiomeDefinition _oldBiome, BiomeDefinition _newBiome)
	{
		if (_player == null)
		{
			return;
		}
		string text = ((_oldBiome != null && _oldBiome.currentWeather != null) ? _oldBiome.currentWeather.buffName : "");
		string text2 = ((_newBiome != null && _newBiome.currentWeather != null) ? _newBiome.currentWeather.buffName : "");
		if (text != "" && text2 != "")
		{
			if (text != text2)
			{
				_player.Buffs.RemoveBuff(text);
				_player.Buffs.AddBuff(text2);
			}
		}
		else if (text != "")
		{
			_player.Buffs.RemoveBuff(text);
		}
		else if (text2 != "")
		{
			_player.Buffs.AddBuff(text2);
		}
	}

	public void ForceWeather(string _weatherName, float _duration)
	{
		GeneralReset();
		CustomWeatherName = _weatherName;
		CustomWeatherTime = _duration;
		for (int i = 0; i < biomeWeather.Count; i++)
		{
			biomeWeather[i].ForceWeather(_weatherName);
		}
	}

	public static void VersionReset()
	{
		GeneralReset();
		savedWeather.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void GeneralReset()
	{
		globalTemperatureOffset = 0f;
		globalRainDayStart = -1f;
		globalRainDayPeak = 0f;
		globalRainPercent = 0f;
		isThunderWeather = false;
		Instance.thunderLastTime = Time.time;
	}

	public void FrameUpdate()
	{
		if (GameManager.Instance == null || SkyManager.random == null || world == null)
		{
			return;
		}
		float time = Time.time;
		if (time > checkPlayerMoveTime + 1f)
		{
			checkPlayerMoveTime = time;
			EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
			if (primaryPlayer != null)
			{
				Vector3 vector = primaryPlayer.position - playerPosition;
				if (vector.x * vector.x + vector.y * vector.y + vector.z * vector.z > 400f)
				{
					spectrumBlend = 1f;
				}
				playerPosition = primaryPlayer.position;
			}
		}
		bool flag = GameManager.IsDedicatedServer || SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		if (flag && savedWeather.Count > 0 && biomeWeather != null)
		{
			ApplySavedPackages();
		}
		int num = Time.frameCount;
		if (frameCount == num)
		{
			return;
		}
		frameCount = num;
		ParticlesFrameUpdate();
		if (flag)
		{
			GenerateWeatherServerFrameUpdate();
		}
		if (isThunderWeather && flag && time >= thunderLastTime + thunderDelay)
		{
			thunderLastTime = time;
			Vector3 zero = Vector3.zero;
			int num2 = 0;
			for (int i = 0; i < players.Count; i++)
			{
				Entity entity = players[i];
				if (entity != null)
				{
					zero += entity.GetPosition();
					num2++;
				}
			}
			if (num2 > 0)
			{
				zero.x /= num2;
				zero.y /= num2;
				zero.z /= num2;
				float num3 = world.GetWorldTime();
				float num4 = num3 % 4f;
				float num5 = num3 % 3f;
				float num6 = num3 % 5f;
				Vector3 vector2 = default(Vector3);
				vector2.x = ((num4 == 2f) ? 0.5f : (num4 - 2f + (float)((num4 > 2f) ? 1 : (-1)))) * 200f;
				vector2.y = num5 * 10f + 200f;
				vector2.z = ((num6 == 2f) ? 0.5f : (num6 - 2f + (float)((num6 > 2f) ? 1 : (-1)))) * 200f;
				zero -= vector2;
				sLightningPos = zero;
				sLightningWorldTime = world.GetWorldTime() + 40;
				isPlayThunder = true;
				thunderDelay = SkyManager.random.RandomFloat;
				thunderDelay = Mathf.LerpUnclamped(thunderFreq.x, thunderFreq.y, thunderDelay);
			}
		}
		CurrentWeatherFromNearBiomesFrameUpdate();
		currentWeather.ParamsFrameUpdate();
		if (isPlayThunder)
		{
			isPlayThunder = false;
			if (((((forceRain >= 0f) ? forceRain : currentWeather.rainParam.value) > 0.5f && ((forceClouds >= 0f) ? (forceClouds * 100f) : currentWeather.CloudThickness()) >= 70f) || SkyManager.IsBloodMoonVisible()) && EnvironmentAudioManager.Instance != null)
			{
				EnvironmentAudioManager.Instance.TriggerThunder(sLightningWorldTime, sLightningPos);
			}
		}
		if (flag)
		{
			WeatherPackagesServerFrameUpdate();
		}
		if (currentWeather != null)
		{
			currentWeather.Normalize();
		}
		if (needToReUpdateWeatherSpectrums)
		{
			needToReUpdateWeatherSpectrums = false;
			spectrumBlend = 1f;
		}
		SpectrumsFrameUpdate();
		CloudsFrameUpdate();
		WindFrameUpdate();
		TriggerEffectManager.UpdateDualSenseLightFromWeather(currentWeather);
	}

	public void CloudsFrameUpdateNow()
	{
		CloudsFrameUpdate();
		cloudThickness = cloudThicknessTarget;
	}

	public void CloudsFrameUpdate()
	{
		if (currentWeather == null)
		{
			return;
		}
		float num = currentWeather.CloudThickness();
		if (forceClouds >= 0f)
		{
			num = forceClouds * 100f;
		}
		cloudThicknessTarget = num;
		if (num < 20f)
		{
			cloudThicknessTarget = 0f;
		}
		cloudThickness = Mathf.MoveTowards(cloudThickness, cloudThicknessTarget, 0.05f);
		Texture mainTex;
		Texture blendTex;
		float cloudTransition;
		if (cloudThickness <= 40f)
		{
			mainTex = clouds[0];
			blendTex = clouds[1];
			cloudTransition = cloudThickness / 40f;
		}
		else
		{
			mainTex = clouds[2];
			blendTex = clouds[1];
			cloudTransition = (cloudThickness - 40f) / 50f;
			if (cloudTransition >= 1f)
			{
				cloudTransition = 1f;
			}
			cloudTransition = 1f - cloudTransition;
		}
		SkyManager.SetCloudTextures(mainTex, blendTex);
		SkyManager.SetCloudTransition(cloudTransition);
	}

	public void WindFrameUpdate()
	{
		float deltaTime = Time.deltaTime;
		float num = GetWindSpeed();
		float num2 = num * 0.01f;
		windGust += windGustStep * deltaTime;
		if (windGust <= 0f)
		{
			windGust = 0f;
			windGustTime -= deltaTime;
			if (windGustTime <= 0f)
			{
				GameRandom gameRandom = world.GetGameRandom();
				windGustTarget = (3f + num * 0.33f) * gameRandom.RandomFloat + 5f;
				windGustStep = 0.35f * windGustTarget;
				windGustTime = (1f + 5f * gameRandom.RandomFloat) * (1f - num2) + 0.5f;
			}
		}
		if (windGust > windGustTarget)
		{
			windGust = windGustTarget;
			windGustStep = 0f - windGustStep;
		}
		num += windGust;
		num *= 0.01f;
		windZone.windMain = num * 1.5f;
		windSpeedPrevious = windSpeed;
		windSpeed = num;
		windTimePrevious = windTime;
		windTime += num * deltaTime;
		Shader.SetGlobalVector("_Wind", new Vector4(windSpeed, windTime, windSpeedPrevious, windTimePrevious));
	}

	public void InitParticles()
	{
		GetParticleParts("Rain", out rainParticleObj, out rainParticleMat, out rainParticleSys);
		rainEmissionMaxRate = rainParticleSys.emission.rateOverTime.constant;
		GetParticleParts("Snow", out snowParticleObj, out snowParticleMat, out snowParticleSys);
		snowParticleT = snowParticleObj.transform;
		snowEmissionMaxRate = snowParticleSys.emission.rateOverTime.constant;
		Transform transform = snowParticleT.Find("Near");
		snowNearParticleSys = transform.GetComponent<ParticleSystem>();
		snowNearEmissionMaxRate = snowNearParticleSys.emission.rateOverTime.constant;
		Transform transform2 = snowParticleT.Find("Top");
		snowTopParticleSys = transform2.GetComponent<ParticleSystem>();
		snowTopEmissionMaxRate = snowTopParticleSys.emission.rateOverTime.constant;
		Transform transform3 = snowParticleT.Find("Far");
		snowFarParticleSys = transform3.GetComponent<ParticleSystem>();
		snowFarBaseColor = snowFarParticleSys.main.startColor.color;
		snowPlayerForceT = snowParticleT.Find("PlayerForce");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetParticleParts(string name, out GameObject obj, out Material mat, out ParticleSystem ps)
	{
		Transform transform = SkyManager.skyManager.transform.Find(name);
		obj = transform.gameObject;
		ps = obj.GetComponent<ParticleSystem>();
		Renderer component = ps.GetComponent<Renderer>();
		mat = component.material;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ParticlesFrameUpdate()
	{
		if (mainCamera == null)
		{
			mainCamera = Camera.main;
			if (mainCamera == null)
			{
				return;
			}
		}
		EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			return;
		}
		Vector4 value = new Vector4((forceWet >= 0f) ? forceWet : currentWeather.wetParam.value, (forceSnow >= 0f) ? forceSnow : currentWeather.snowCoverParam.value, 0f, (forceRain >= 0f) ? forceRain : Mathf.Clamp01(currentWeather.rainParam.value * 40f));
		Shader.SetGlobalVector("_WeatherParams0", value);
		if ((bool)noiseTexture)
		{
			Shader.SetGlobalTexture("_NoiseSampler", noiseTexture);
		}
		Shader.SetGlobalTexture("_SnowSampler", snowTexture);
		if (Time.time > particleFallHitTime + 0.05f)
		{
			particleFallHitTime = Time.time;
			Vector3 position = mainCamera.transform.position;
			position.y += 250f;
			if (Physics.SphereCast(new Ray(position, Vector3.down), 9f, out var hitInfo, float.PositiveInfinity, raycastMask))
			{
				particleFallLastPos = hitInfo.point;
				Vector3 velocityPerSecond = primaryPlayer.GetVelocityPerSecond();
				particleFallLastPos.x += velocityPerSecond.x * 2f;
				particleFallLastPos.z += velocityPerSecond.z * 2f;
				if (velocityPerSecond.y < -5f)
				{
					velocityPerSecond.y = -5f;
				}
				particleFallLastPos.y += velocityPerSecond.y;
			}
			particleFallPos = particleFallLastPos;
			particleFallPos.y += 12f;
			rainParticleObj.transform.position = particleFallPos;
			snowParticleT.position = particleFallPos;
		}
		float num = ((forceRain >= 0f) ? forceRain : currentWeather.rainParam.value);
		if (rainParticleSys != null)
		{
			ParticleSystem.EmissionModule emission = rainParticleSys.emission;
			emission.rateOverTime = rainEmissionMaxRate * (num * 0.995f + 0.005f);
		}
		float num2 = 1f;
		SetParticleIntensity(rainParticleObj, rainParticleMat, num * num2);
		if ((bool)snowParticleObj)
		{
			float num3 = ((forceSnowfall >= 0f) ? forceSnowfall : Mathf.Clamp01(currentWeather.snowFallParam.value));
			snowParticleObj.SetActive(num3 > 0f);
			if (num3 > 0f)
			{
				float num4 = num3 * 0.995f + 0.005f;
				ParticleSystem.EmissionModule emission2 = snowParticleSys.emission;
				emission2.rateOverTime = snowEmissionMaxRate * num4;
				emission2 = snowNearParticleSys.emission;
				emission2.rateOverTime = snowNearEmissionMaxRate * num4;
				emission2 = snowTopParticleSys.emission;
				emission2.rateOverTime = snowTopEmissionMaxRate * num4;
				Color color = snowFarBaseColor;
				color.a *= num3 * 0.95f + 0.05f;
				ParticleSystem.MainModule main = snowFarParticleSys.main;
				ParticleSystem.MinMaxGradient startColor = main.startColor;
				startColor.color = color;
				main.startColor = startColor;
				Vector3 position2 = primaryPlayer.position - Origin.position;
				position2.y += 1f;
				snowPlayerForceT.position = position2;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetParticleIntensity(GameObject go, Material mtrl, float intensity)
	{
		if (go != null)
		{
			go.SetActive(intensity > 0f);
			if (mtrl != null && intensity > 0f)
			{
				mtrl.SetFloat("_Intensity", intensity);
			}
		}
	}

	public string GetSpectrumInfo()
	{
		float value = 1f - spectrumBlend;
		float value2 = spectrumBlend;
		string text = spectrumSourceType.ToString();
		string text2 = spectrumTargetType.ToString();
		return $"source {text} {value.ToCultureInvariantString()}, target {text2} {value2.ToCultureInvariantString()}";
	}

	public static void SetForceSpectrum(SpectrumWeatherType type)
	{
		forcedSpectrum = type;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpectrumsFrameUpdate()
	{
		LoadSpectrums();
		float num = SkyManager.BloodMoonVisiblePercent();
		if (num > 0f && spectrumTargetType == SpectrumWeatherType.BloodMoon)
		{
			spectrumBlend = num;
		}
		else if (spectrumBlend < 1f)
		{
			spectrumBlend += Time.deltaTime / 10f;
			if (spectrumBlend > 1f)
			{
				spectrumBlend = 1f;
			}
		}
		if (spectrumSourceType == spectrumTargetType)
		{
			spectrumBlend = 1f;
		}
		if (spectrumBlend >= 1f)
		{
			spectrumSourceType = spectrumTargetType;
			spectrumTargetType = SpectrumWeatherType.Biome;
			if (currentWeather.biomeDefinition != null)
			{
				spectrumTargetType = currentWeather.biomeDefinition.weatherSpectrum;
			}
			if (num > 0f)
			{
				spectrumTargetType = SpectrumWeatherType.BloodMoon;
			}
			if (spectrumSourceType != spectrumTargetType)
			{
				spectrumBlend = 0f;
			}
		}
	}

	public Color GetWeatherSpectrum(Color regularSpectrum, AtmosphereEffect.ESpecIdx type, float dayTimeScalar)
	{
		if (forcedSpectrum != SpectrumWeatherType.None)
		{
			int num = (int)forcedSpectrum;
			AtmosphereEffect atmosphereEffect = atmosphereSpectrum[num];
			if (atmosphereEffect == null)
			{
				return regularSpectrum;
			}
			return atmosphereEffect.spectrums[(int)type]?.GetValue(dayTimeScalar) ?? regularSpectrum;
		}
		Color color = regularSpectrum;
		Color color2 = regularSpectrum;
		if (isGameModeNormal)
		{
			if (spectrumSourceType != SpectrumWeatherType.Biome)
			{
				ColorSpectrum colorSpectrum = atmosphereSpectrum[(int)spectrumSourceType].spectrums[(int)type];
				if (colorSpectrum != null)
				{
					color = colorSpectrum.GetValue(dayTimeScalar);
				}
			}
			if (spectrumTargetType != SpectrumWeatherType.Biome)
			{
				ColorSpectrum colorSpectrum2 = atmosphereSpectrum[(int)spectrumTargetType].spectrums[(int)type];
				if (colorSpectrum2 != null)
				{
					color2 = colorSpectrum2.GetValue(dayTimeScalar);
				}
			}
		}
		return color * (1f - spectrumBlend) + color2 * spectrumBlend;
	}

	public void TriggerThunder(ulong _playWorldTime, Vector3 _pos)
	{
		sLightningWorldTime = _playWorldTime;
		sLightningPos = _pos;
		isPlayThunder = true;
	}

	public void ClientProcessPackages(WeatherPackage[] _packages)
	{
		int num = Time.frameCount;
		if (processingPackageFrame == num)
		{
			return;
		}
		processingPackageFrame = num;
		foreach (WeatherPackage weatherPackage in _packages)
		{
			if (WorldBiomes.Instance.TryGetBiome(weatherPackage.biomeID, out var _bd))
			{
				_bd.weatherPackage.CopyFrom(weatherPackage);
				_bd.weatherSpectrum = (SpectrumWeatherType)weatherPackage.weatherSpectrum;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WeatherPackagesServerFrameUpdate()
	{
		int num = Utils.FastMin(this.biomeWeather.Count, weatherPackages.Length);
		for (int i = 0; i < num; i++)
		{
			BiomeWeather biomeWeather = this.biomeWeather[i];
			WeatherPackage weatherPackage = weatherPackages[i];
			for (int j = 0; j < biomeWeather.parameters.Length && j < weatherPackage.param.Length; j++)
			{
				weatherPackage.param[j] = biomeWeather.parameterFinals[j];
			}
			weatherPackage.particleRain = Mathf.Clamp01(biomeWeather.rainParam.value);
			weatherPackage.particleSnow = Mathf.Clamp01(biomeWeather.snowFallParam.value);
			weatherPackage.surfaceWet = Mathf.Clamp01(biomeWeather.wetParam.value);
			weatherPackage.surfaceSnow = Mathf.Clamp01(biomeWeather.snowCoverParam.value);
			weatherPackage.biomeID = biomeWeather.biomeDefinition.m_Id;
			weatherPackage.weatherSpectrum = (short)biomeWeather.biomeDefinition.weatherSpectrum;
			if (forceRain >= 0f)
			{
				weatherPackage.particleRain = forceRain;
			}
			if (forceWet >= 0f)
			{
				weatherPackage.surfaceWet = forceWet;
			}
			if (forceSnow >= 0f)
			{
				weatherPackage.surfaceSnow = forceSnow;
			}
			if (forceSnowfall >= 0f)
			{
				weatherPackage.particleSnow = forceSnowfall;
			}
			BiomeDefinition biomeDefinition = biomeWeather.biomeDefinition;
			if (WorldBiomes.Instance.TryGetBiome(biomeWeather.biomeDefinition.m_Id, out var _bd))
			{
				biomeDefinition = _bd;
			}
			biomeDefinition.weatherPackage = weatherPackage;
		}
	}

	public void SendPackages()
	{
		NetPackageWeather package = NetPackageManager.GetPackage<NetPackageWeather>();
		package.Setup(weatherPackages, sLightningWorldTime, sLightningPos);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: true);
		sLightningWorldTime = 0uL;
	}

	[Conditional("DEBUG_WEATHERNET")]
	public void LogNet(string _format = "", params object[] _args)
	{
		_format = $"{GameManager.frameCount} WeatherManager net {_format}";
		Log.Warning(_format, _args);
	}
}
