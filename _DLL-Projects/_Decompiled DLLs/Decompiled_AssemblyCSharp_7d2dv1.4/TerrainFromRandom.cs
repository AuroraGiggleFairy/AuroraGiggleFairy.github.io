using UnityEngine;

public class TerrainFromRandom : TerrainGeneratorWithBiomeResource
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class DefaultTGM : TGMAbstract
	{
		public override void SetSeed(int _seed)
		{
		}

		public override float GetValue(float _x, float _z, float _biomeIntens)
		{
			return 1f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float baseHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public float heightAv;

	[PublicizedFrom(EAccessModifier.Private)]
	public float heightXZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public float heightXp1Z;

	[PublicizedFrom(EAccessModifier.Private)]
	public float heightXZp1;

	[PublicizedFrom(EAccessModifier.Private)]
	public float heightXm1Z;

	[PublicizedFrom(EAccessModifier.Private)]
	public float heightXZm1;

	[PublicizedFrom(EAccessModifier.Private)]
	public float minYAround;

	[PublicizedFrom(EAccessModifier.Private)]
	public float maxYAround;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int SAMPLE_RATE_3D_HOR = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int SAMPLE_RATE_3D_VERT = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public TGMAbstract defaultGenerator = new DefaultTGM();

	[PublicizedFrom(EAccessModifier.Private)]
	public const float xzStep = 0.00390625f;

	public override void Init(World _world, IBiomeProvider _biomeProvider, int _seed)
	{
		base.Init(_world, _biomeProvider, _seed);
	}

	public override byte GetTerrainHeightAt(int worldX, int worldZ, BiomeDefinition _bd, float _biomeIntens)
	{
		TGMAbstract tGMAbstract = ((_bd.m_Terrain != null) ? _bd.m_Terrain : defaultGenerator);
		heightXZ = tGMAbstract.GetValue((float)worldX * 0.00390625f, (float)worldZ * 0.00390625f, _biomeIntens);
		heightXp1Z = tGMAbstract.GetValue((float)(worldX + 1) * 0.00390625f, (float)worldZ * 0.00390625f, _biomeIntens);
		heightXZp1 = tGMAbstract.GetValue((float)worldX * 0.00390625f, (float)(worldZ + 1) * 0.00390625f, _biomeIntens);
		heightXm1Z = tGMAbstract.GetValue((float)(worldX - 1) * 0.00390625f, (float)worldZ * 0.00390625f, _biomeIntens);
		heightXZm1 = tGMAbstract.GetValue((float)worldX * 0.00390625f, (float)(worldZ - 1) * 0.00390625f, _biomeIntens);
		minYAround = Utils.FastMin(heightXp1Z, heightXZp1, heightXm1Z, heightXZm1);
		maxYAround = Utils.FastMax(heightXp1Z, heightXZp1, heightXm1Z, heightXZm1);
		baseHeight = tGMAbstract.GetBaseHeight();
		heightAv = (heightXZ + heightXp1Z + heightXZp1 + heightXm1Z + heightXZm1) / 5f;
		return (byte)(heightAv + baseHeight);
	}

	public override sbyte GetDensityAt(int _xWorld, int _yWorld, int _zWorld, BiomeDefinition _bd, float _biomeIntensity)
	{
		return (sbyte)Utils.FastClamp(((float)_yWorld - (heightAv + baseHeight)) * 127f, -128f, 127f);
	}

	public override Vector3 GetTerrainNormalAt(int _xWorld, int _zWorld, BiomeDefinition _bd, float _biomeIntensity)
	{
		return ((_bd.m_Terrain != null) ? _bd.m_Terrain : defaultGenerator).GetNormal((float)_xWorld * 0.00390625f, (float)_zWorld * 0.00390625f, _biomeIntensity);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isTerrainAt(int _x, int _y, int _z)
	{
		return (float)_y < heightAv + baseHeight;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void fillDensityInBlock(Chunk _chunk, int _x, int _y, int _z, BlockValue _bv)
	{
		float num = (float)_y - (heightAv + baseHeight);
		sbyte b;
		if (num >= 0f)
		{
			if ((float)(_y - 1) < heightAv + baseHeight && minYAround + 1f < heightAv && maxYAround - 1f > heightAv)
			{
				float num2 = heightAv + baseHeight - (float)_y;
				float num3 = heightAv - minYAround;
				float num4 = num2 / num3;
				if (num4 > 1f)
				{
					num4 = 1f;
				}
				num = -1f * num4;
				b = (sbyte)Utils.FastClamp(num * 127f, -128f, 127f);
				if (_y < 255)
				{
					int density = _chunk.GetDensity(_x, _y + 1, _z);
					_chunk.SetDensity(_x, _y + 1, _z, (sbyte)((density + b) / 2));
				}
			}
		}
		else if (minYAround + 1f < heightAv && maxYAround - 1f > heightAv)
		{
			float num5 = heightAv + baseHeight - (float)_y;
			float num6 = heightAv - minYAround;
			float num7 = num5 / num6;
			if (num7 > 1f)
			{
				num7 = 1f;
			}
			num = -1f * num7;
		}
		b = (sbyte)Utils.FastClamp(num * 127f, -128f, 127f);
		if (num < 0f && b == 0)
		{
			b = -1;
		}
		_chunk.SetDensity(_x, _y, _z, b);
	}
}
