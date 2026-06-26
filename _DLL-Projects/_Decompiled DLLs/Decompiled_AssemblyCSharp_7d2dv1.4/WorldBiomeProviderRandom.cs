using System.Collections;

public class WorldBiomeProviderRandom : IBiomeProvider
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TS_PerlinNoise temperatureNoise;

	[PublicizedFrom(EAccessModifier.Private)]
	public TS_PerlinNoise humidityNoise;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldBiomes m_Biomes;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte BIOMEID_SNOW = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte BIOMEID_FOREST = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte BIOMEID_PLAINS = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte BIOMEID_DESERT = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte BIOMEID_OCEAN = 6;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte BIOMEID_MOUNTAINS = 17;

	public WorldBiomeProviderRandom(int _seed, WorldBiomes _biomes)
	{
		m_Biomes = _biomes;
		temperatureNoise = new TS_PerlinNoise(_seed + 5);
		temperatureNoise.setOctaves(5);
		humidityNoise = new TS_PerlinNoise(_seed + 6);
		humidityNoise.setOctaves(5);
	}

	public IEnumerator InitData()
	{
		yield break;
	}

	public string GetWorldName()
	{
		return string.Empty;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float getNoiseAt(int x, int z)
	{
		return TeraMath.clamp((humidityNoise.fBm((float)x * 0.0015f, 0f, 0.0015f * (float)z) + 1f) / 2f);
	}

	public float GetHumidityAt(int x, int z)
	{
		return TeraMath.clamp((humidityNoise.fBm((float)x * 0.0005f, 0f, 0.0005f * (float)z) + 1f) / 2f);
	}

	public float GetTemperatureAt(int x, int z)
	{
		return TeraMath.clamp((temperatureNoise.fBm((float)x * 0.0005f, 0f, 0.0005f * (float)z) + 1f) / 2f);
	}

	public float GetRadiationAt(int x, int z)
	{
		return 0f;
	}

	public void Init(int _seed, string _worldName, WorldBiomes _biomes, string _params1, string _params2)
	{
	}

	public int GetSubBiomeIdxAt(BiomeDefinition bd, int _x, int _y, int _z)
	{
		return -1;
	}

	public BiomeDefinition GetBiomeAt(int x, int z)
	{
		float _intensity;
		return GetBiomeAt(x, z, out _intensity);
	}

	public BiomeDefinition GetBiomeAt(int _x, int _z, out float _intensity)
	{
		byte b = 0;
		_intensity = 0f;
		float noiseAt = getNoiseAt(_x, _z);
		if (noiseAt < 0.2f)
		{
			_intensity = 1f - Utils.FastAbs((noiseAt - 0.1f) / 0.1f);
			b = 6;
		}
		else if (noiseAt < 0.4f)
		{
			_intensity = 1f - Utils.FastAbs((noiseAt - 0.3f) / 0.1f);
			b = 4;
		}
		else if (noiseAt < 0.6f)
		{
			_intensity = 1f - Utils.FastAbs((noiseAt - 0.5f) / 0.1f);
			b = 5;
		}
		else if (noiseAt < 0.8f)
		{
			_intensity = (noiseAt - 0.6f) / 0.2f;
			b = 17;
		}
		else
		{
			_intensity = 1f;
			_intensity = (noiseAt - 0.6f) / 0.2f;
			b = 1;
		}
		return m_Biomes.GetBiome(b);
	}

	public BiomeIntensity GetBiomeIntensityAt(int _x, int _z)
	{
		return BiomeIntensity.Default;
	}

	public BlockValue GetTopmostBlockValue(int xWorld, int zWorld)
	{
		return BlockValue.Air;
	}
}
