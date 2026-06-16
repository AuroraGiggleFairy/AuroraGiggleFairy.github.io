using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BiomeDefinition
{
	public enum BiomeType
	{
		Any,
		Snow,
		Forest,
		PineForest,
		Plains,
		Desert,
		Water,
		Radiated,
		Wasteland,
		burnt_forest,
		city,
		city_wasteland,
		wasteland_hub,
		caveFloor,
		caveCeiling
	}

	public class Probabilities
	{
		public enum ProbType
		{
			Temperature,
			Precipitation,
			CloudThickness,
			Wind,
			Fog,
			Count
		}

		public const int ProbTypeCount = 5;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<Vector3>[] probabilities;

		public Probabilities()
		{
			probabilities = new List<Vector3>[5];
			for (int i = 0; i < 5; i++)
			{
				probabilities[i] = new List<Vector3>();
			}
		}

		public void AddProbability(ProbType _type, Vector2 _range, float _probability)
		{
			probabilities[(int)_type].Add(new Vector3(_range.x, _range.y, _probability));
		}

		public Vector2 CalcMinMaxPossibleValue(ProbType type)
		{
			Vector2 result = new Vector2(float.MaxValue, float.MinValue);
			List<Vector3> list = probabilities[(int)type];
			for (int i = 0; i < list.Count; i++)
			{
				Vector3 vector = list[i];
				if (vector.x < result.x)
				{
					result.x = vector.x;
				}
				if (vector.y > result.y)
				{
					result.y = vector.y;
				}
			}
			return result;
		}

		public float GetRandomValue(ProbType _type)
		{
			GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
			float randomFloat = gameRandom.RandomFloat;
			float num = 0f;
			List<Vector3> list = probabilities[(int)_type];
			for (int i = 0; i < list.Count; i++)
			{
				Vector3 vector = list[i];
				num += vector.z;
				if (randomFloat < num)
				{
					float randomFloat2 = gameRandom.RandomFloat;
					return vector.x * randomFloat2 + vector.y * (1f - randomFloat2);
				}
			}
			return 0f;
		}

		public void Normalize()
		{
			for (int i = 0; i < 5; i++)
			{
				List<Vector3> list = probabilities[i];
				float num = 0f;
				for (int j = 0; j < list.Count; j++)
				{
					num += list[j].z;
				}
				for (int k = 0; k < list.Count; k++)
				{
					Vector3 value = list[k];
					value.z /= num;
					list[k] = value;
				}
			}
		}
	}

	public class WeatherGroup
	{
		public string name;

		public int stormLevel;

		public float prob;

		public int duration;

		public Vector2i delay;

		public string buffName;

		public SpectrumWeatherType spectrum;

		public Probabilities probabilities = new Probabilities();

		public void AddProbability(Probabilities.ProbType _type, Vector2 _range, float _probability)
		{
			probabilities.AddProbability(_type, _range, _probability);
		}
	}

	public float currentPlayerIntensity;

	public const string BiomeNameLocalizationPrefix = "biome_";

	public static string[] BiomeNames = new string[15]
	{
		"any", "snow", "forest", "pine_forest", "plains", "desert", "water", "radiated", "wasteland", "burnt_forest",
		"city", "city_wasteland", "wasteland_hub", "caveFloor", "caveCeiling"
	};

	public static uint[] BiomeColors = new uint[15]
	{
		0u, 16777215u, 0u, 16384u, 0u, 16770167u, 25599u, 0u, 16754688u, 12189951u,
		8421504u, 12632256u, 10526880u, 0u, 0u
	};

	public readonly byte m_Id;

	public readonly BiomeType m_BiomeType;

	public byte subId;

	public readonly string m_sBiomeName;

	public readonly string LocalizedName;

	public uint m_uiColor;

	public string m_SpectrumName;

	public static Dictionary<string, byte> nameToId;

	public int m_RadiationLevel;

	public int Difficulty = 1;

	public List<BiomeLayer> m_Layers;

	public List<BiomeBlockDecoration> m_DecoBlocks;

	public List<BiomeBlockDecoration> m_DistantDecoBlocks;

	public List<BiomePrefabDecoration> m_DecoPrefabs;

	public List<BiomeBluffDecoration> m_DecoBluffs;

	public List<WeatherGroup> weatherGroups = new List<WeatherGroup>();

	public string weatherName;

	public int currentWeatherGroupIndex;

	public WeatherGroup currentWeatherGroup;

	public SpectrumWeatherType weatherSpectrum;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] weatherValues = new float[5];

	public int TotalLayerDepth;

	public List<BiomeDefinition> subbiomes = new List<BiomeDefinition>();

	public float noiseFreq = 0.03f;

	public float noiseMin = 0.2f;

	public float noiseMax = 1f;

	public Vector2 noiseOffset;

	public Dictionary<int, int> Replacements = new Dictionary<int, int>();

	public float GameStageMod;

	public float GameStageBonus;

	public float LootStageMod;

	public float LootStageBonus;

	public int LootStageMin = -1;

	public int LootStageMax = -1;

	public string Buff;

	public static string LocalizedBiomeName(BiomeType _biomeType)
	{
		return Localization.Get("biome_" + _biomeType.ToStringCached());
	}

	public BiomeDefinition(byte _id, byte _subId, string _name, uint _color, int _radiationLevel, string _buff)
	{
		m_Id = _id;
		m_BiomeType = (BiomeType)(Enum.IsDefined(typeof(BiomeType), (int)m_Id) ? m_Id : 0);
		subId = _subId;
		m_sBiomeName = _name;
		LocalizedName = Localization.Get("biome_" + _name);
		m_SpectrumName = m_sBiomeName;
		m_uiColor = _color;
		m_RadiationLevel = _radiationLevel;
		m_Layers = new List<BiomeLayer>();
		m_DecoBlocks = new List<BiomeBlockDecoration>();
		m_DistantDecoBlocks = new List<BiomeBlockDecoration>();
		m_DecoPrefabs = new List<BiomePrefabDecoration>();
		m_DecoBluffs = new List<BiomeBluffDecoration>();
		Buff = _buff;
		InitWeather();
	}

	public void AddLayer(BiomeLayer _layer)
	{
		m_Layers.Add(_layer);
		TotalLayerDepth += _layer.m_Depth;
	}

	public void AddDecoBlock(BiomeBlockDecoration _deco)
	{
		m_DecoBlocks.Add(_deco);
		if (Block.BlocksLoaded)
		{
			Block block = _deco.blockValues[0].Block;
			if (block != null && block.IsDistantDecoration)
			{
				m_DistantDecoBlocks.Add(_deco);
			}
		}
	}

	public void AddDecoPrefab(BiomePrefabDecoration _deco)
	{
		m_DecoPrefabs.Add(_deco);
	}

	public void AddBluff(BiomeBluffDecoration _deco)
	{
		m_DecoBluffs.Add(_deco);
	}

	public void AddReplacement(int _sourceId, int _targetId)
	{
		Replacements[_sourceId] = _targetId;
	}

	public void addSubBiome(BiomeDefinition _subbiome)
	{
		subbiomes.Add(_subbiome);
	}

	public override bool Equals(object obj)
	{
		if (obj is BiomeDefinition)
		{
			return ((BiomeDefinition)obj).m_Id == m_Id;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_Id;
	}

	public override string ToString()
	{
		return m_sBiomeName;
	}

	public static uint GetBiomeColor(BiomeType _type)
	{
		return BiomeColors[(int)_type];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitWeather()
	{
	}

	public WeatherGroup AddWeatherGroup(string _name, float _prob, float _duration, Vector2 _delay, string _buffName)
	{
		WeatherGroup weatherGroup = new WeatherGroup();
		weatherGroup.name = _name;
		weatherGroup.stormLevel = (_name.StartsWith("storm") ? (_name.Contains("build") ? 1 : 2) : 0);
		weatherGroup.prob = _prob;
		weatherGroup.duration = (int)(_duration * 1000f);
		weatherGroup.delay.x = (int)(_delay.x * 1000f);
		weatherGroup.delay.y = (int)(_delay.y * 1000f);
		if (!string.IsNullOrEmpty(_buffName))
		{
			weatherGroup.buffName = _buffName;
		}
		weatherGroups.Add(weatherGroup);
		return weatherGroup;
	}

	public void SetupWeather()
	{
		float num = 0f;
		for (int i = 0; i < weatherGroups.Count; i++)
		{
			WeatherGroup weatherGroup = weatherGroups[i];
			num += weatherGroup.prob;
			weatherGroup.probabilities.Normalize();
		}
		num += 1E-06f;
		for (int j = 0; j < weatherGroups.Count; j++)
		{
			weatherGroups[j].prob /= num;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float WeatherGetValue(Probabilities.ProbType _type)
	{
		return weatherValues[(int)_type];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WeatherSetValue(Probabilities.ProbType _type, float _value)
	{
		weatherValues[(int)_type] = _value;
	}

	public void WeatherRandomize(float _rand)
	{
		float num = 0f;
		for (int i = 0; i < weatherGroups.Count; i++)
		{
			WeatherGroup weatherGroup = weatherGroups[i];
			num += weatherGroup.prob;
			if (_rand < num)
			{
				SelectWeatherGroup(i);
				break;
			}
		}
	}

	public bool WeatherRandomize(string _weatherGroup)
	{
		int num = FindWeatherGroupIndex(_weatherGroup);
		if (num >= 0)
		{
			SelectWeatherGroup(num);
			return true;
		}
		return false;
	}

	public int WeatherGetDuration(string _weatherGroup)
	{
		return FindWeatherGroup(_weatherGroup)?.duration ?? 0;
	}

	public int WeatherGetDuration(string _weatherGroup, out Vector2i _delay)
	{
		WeatherGroup weatherGroup = FindWeatherGroup(_weatherGroup);
		if (weatherGroup != null)
		{
			_delay = weatherGroup.delay;
			return weatherGroup.duration;
		}
		_delay = Vector2i.zero;
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WeatherGroup FindWeatherGroup(string _name)
	{
		for (int i = 0; i < weatherGroups.Count; i++)
		{
			WeatherGroup weatherGroup = weatherGroups[i];
			if (weatherGroup.name == _name)
			{
				return weatherGroup;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int FindWeatherGroupIndex(string _name)
	{
		for (int i = 0; i < weatherGroups.Count; i++)
		{
			if (weatherGroups[i].name == _name)
			{
				return i;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SelectWeatherGroup(int _index)
	{
		WeatherGroup weatherGroup = weatherGroups[_index];
		weatherName = weatherGroup.name;
		weatherSpectrum = weatherGroup.spectrum;
		currentWeatherGroupIndex = _index;
		currentWeatherGroup = weatherGroup;
		for (int i = 0; i < 5; i++)
		{
			float randomValue = weatherGroup.probabilities.GetRandomValue((Probabilities.ProbType)i);
			weatherValues[i] = randomValue;
		}
	}

	public void SetWeatherGroup(int _index)
	{
		currentWeatherGroupIndex = _index;
		weatherSpectrum = (currentWeatherGroup = weatherGroups[_index]).spectrum;
	}
}
