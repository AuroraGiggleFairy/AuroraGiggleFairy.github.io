using UnityEngine;

public struct Lighting
{
	public static Lighting one;

	public byte sun;

	public byte block;

	public byte stability;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cToPer = 1f / 15f;

	[PublicizedFrom(EAccessModifier.Private)]
	static Lighting()
	{
		one = new Lighting(15, 15, 0);
	}

	public Lighting(byte _sun, byte _block, byte _stability)
	{
		sun = _sun;
		block = _block;
		stability = _stability;
	}

	public Color ToColor()
	{
		return new Color((float)(int)sun * (1f / 15f), 0f, (float)(int)stability * (1f / 15f), (float)(int)block * (1f / 15f));
	}

	public static Color ToColor(int _sunLight, int _blockLight)
	{
		Color result = default(Color);
		result.r = (float)_sunLight * (1f / 15f);
		result.g = 0f;
		result.b = 0f;
		result.a = (float)_blockLight * (1f / 15f);
		return result;
	}

	public static Color ToColor(int _sunLight, int _blockLight, float _sideFactor)
	{
		Color result = default(Color);
		result.r = (float)_sunLight * (1f / 15f) * _sideFactor;
		result.g = 0f;
		result.b = 0f;
		result.a = (float)_blockLight * (1f / 15f);
		return result;
	}
}
