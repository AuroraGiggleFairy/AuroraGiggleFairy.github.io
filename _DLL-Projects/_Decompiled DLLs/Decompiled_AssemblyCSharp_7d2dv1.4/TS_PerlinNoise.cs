using UnityEngine;

public class TS_PerlinNoise
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float LACUNARITY = 2.1379201f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float H = 0.836281f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] _spectralWeights;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] _noisePermutations;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _recomputeSpectralWeights = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int _octaves = 9;

	public TS_PerlinNoise(int seed)
	{
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(seed);
		_noisePermutations = new int[512];
		int[] array = new int[256];
		for (int i = 0; i < 256; i++)
		{
			array[i] = i;
		}
		for (int j = 0; j < 256; j++)
		{
			int num = gameRandom.RandomRange(255);
			num = ((num < 0) ? (-num) : num);
			int num2 = array[j];
			array[j] = array[num];
			array[num] = num2;
		}
		for (int k = 0; k < 256; k++)
		{
			_noisePermutations[k] = (_noisePermutations[k + 256] = array[k]);
		}
	}

	public float noise(float x, float y, float z)
	{
		int num = (int)TeraMath.fastFloor(x) & 0xFF;
		int num2 = (int)TeraMath.fastFloor(y) & 0xFF;
		int num3 = (int)TeraMath.fastFloor(z) & 0xFF;
		x -= TeraMath.fastFloor(x);
		y -= TeraMath.fastFloor(y);
		z -= TeraMath.fastFloor(z);
		float t = fade(x);
		float t2 = fade(y);
		float t3 = fade(z);
		int num4 = _noisePermutations[num] + num2;
		int num5 = _noisePermutations[num4] + num3;
		int num6 = _noisePermutations[num4 + 1] + num3;
		int num7 = _noisePermutations[num + 1] + num2;
		int num8 = _noisePermutations[num7] + num3;
		int num9 = _noisePermutations[num7 + 1] + num3;
		return lerp(t3, lerp(t2, lerp(t, grad(_noisePermutations[num5], x, y, z), grad(_noisePermutations[num8], x - 1f, y, z)), lerp(t, grad(_noisePermutations[num6], x, y - 1f, z), grad(_noisePermutations[num9], x - 1f, y - 1f, z))), lerp(t2, lerp(t, grad(_noisePermutations[num5 + 1], x, y, z - 1f), grad(_noisePermutations[num8 + 1], x - 1f, y, z - 1f)), lerp(t, grad(_noisePermutations[num6 + 1], x, y - 1f, z - 1f), grad(_noisePermutations[num9 + 1], x - 1f, y - 1f, z - 1f))));
	}

	public float fBm(float x, float y, float z)
	{
		float num = 0f;
		if (_recomputeSpectralWeights)
		{
			_spectralWeights = new float[_octaves];
			for (int i = 0; i < _octaves; i++)
			{
				_spectralWeights[i] = Mathf.Pow(2.1379201f, -0.836281f * (float)i);
			}
			_recomputeSpectralWeights = false;
		}
		for (int j = 0; j < _octaves; j++)
		{
			num += noise(x, y, z) * _spectralWeights[j];
			x *= 2.1379201f;
			y *= 2.1379201f;
			z *= 2.1379201f;
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float fade(float t)
	{
		return t * t * t * (t * (t * 6f - 15f) + 10f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float lerp(float t, float a, float b)
	{
		return a + t * (b - a);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float grad(int hash, float x, float y, float z)
	{
		int num = hash & 0xF;
		float num2 = ((num < 8) ? x : y);
		float num3 = ((num < 4) ? y : ((num == 12 || num == 14) ? x : z));
		return (((num & 1) == 0) ? num2 : (0f - num2)) + (((num & 2) == 0) ? num3 : (0f - num3));
	}

	public void setOctaves(int octaves)
	{
		_octaves = octaves;
		_recomputeSpectralWeights = true;
	}

	public int getOctaves()
	{
		return _octaves;
	}
}
