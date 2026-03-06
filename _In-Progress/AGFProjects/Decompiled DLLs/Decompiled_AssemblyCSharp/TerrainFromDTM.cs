using System;

[PublicizedFrom(EAccessModifier.Internal)]
public class TerrainFromDTM : TerrainGeneratorWithBiomeResource
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ArrayWithOffset<byte> m_DTM;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[,] heightData;

	public void Init(ArrayWithOffset<byte> _dtm, IBiomeProvider _biomeProvider, string levelName, int _seed)
	{
		base.Init(null, _biomeProvider, _seed);
		m_DTM = _dtm;
		heightData = HeightMapUtils.ConvertDTMToHeightData(levelName);
		heightData = HeightMapUtils.SmoothTerrain(5, heightData);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void fillDensityInBlock(Chunk _chunk, int _x, int _y, int _z, BlockValue _bv)
	{
		int num = (_chunk.X << 4) + _x + heightData.GetLength(0) / 2;
		int num2 = (_chunk.Z << 4) + _z + heightData.GetLength(1) / 2;
		if (num >= 0 && num2 >= 0 && num < heightData.GetLength(0) && num2 < heightData.GetLength(1))
		{
			float num3 = heightData[num, num2] + 0.5f;
			int num4 = (int)num3;
			sbyte density;
			if (_y < num4)
			{
				density = MarchingCubes.DensityTerrain;
			}
			else if (_y > num4 + 1)
			{
				density = MarchingCubes.DensityAir;
			}
			else
			{
				float num5 = num3 - (float)num4;
				density = ((num4 != _y) ? ((sbyte)((float)MarchingCubes.DensityAir * (1f - num5))) : ((sbyte)((float)MarchingCubes.DensityTerrain * num5)));
			}
			_chunk.SetDensity(_x, _y, _z, density);
		}
	}

	public override sbyte GetDensityAt(int _xWorld, int _yWorld, int _zWorld, BiomeDefinition _bd, float _biomeIntensity)
	{
		if (m_DTM[_xWorld, _zWorld] > _yWorld)
		{
			return MarchingCubes.DensityAir;
		}
		return MarchingCubes.DensityTerrain;
	}

	public override byte GetTerrainHeightByteAt(int _x, int _z)
	{
		int num = _x + heightData.GetLength(0) / 2;
		int num2 = _z + heightData.GetLength(1) / 2;
		if (num < 0 || num2 < 0 || num >= heightData.GetLength(0) || num2 >= heightData.GetLength(1))
		{
			return 0;
		}
		return (byte)(int)(heightData[num, num2] + 0.5f);
	}

	public override float GetTerrainHeightAt(int x, int z)
	{
		try
		{
			x += heightData.GetLength(0) / 2;
			z += heightData.GetLength(1) / 2;
			return heightData[x, z];
		}
		catch (Exception)
		{
			return 0f;
		}
	}
}
