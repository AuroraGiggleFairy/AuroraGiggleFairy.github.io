using System.Collections.Generic;
using UnityEngine;

[PublicizedFrom(EAccessModifier.Internal)]
public class TerrainFromRaw : TerrainGeneratorWithBiomeResource
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int terrainWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int terrainHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public HeightMap heightMap;

	public void Init(HeightMap _heightMap, IBiomeProvider _biomeProvider, string levelName, int _seed)
	{
		base.Init(null, _biomeProvider, _seed);
		heightMap = _heightMap;
		terrainWidth = heightMap.GetWidth() << heightMap.GetScaleShift();
		terrainHeight = heightMap.GetHeight() << heightMap.GetScaleShift();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkCoordinates(ref int _x, ref int _z)
	{
		_x += terrainWidth / 2;
		_z += terrainHeight / 2;
		if (_x < 0 || _z < 0 || _x >= terrainWidth || _z >= terrainHeight)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void fillDensityInBlock(Chunk _chunk, int _x, int _y, int _z, BlockValue _bv)
	{
		int _x2 = (_chunk.X << 4) + _x;
		int _z2 = (_chunk.Z << 4) + _z;
		if (!checkCoordinates(ref _x2, ref _z2))
		{
			return;
		}
		float num = heightMap.GetAt(_x2, _z2) + 0.5f;
		int num2 = (int)num;
		sbyte b;
		if (_y < num2)
		{
			b = MarchingCubes.DensityTerrain;
		}
		else if (_y > num2 + 1)
		{
			b = MarchingCubes.DensityAir;
		}
		else
		{
			float num3 = num - (float)num2;
			b = ((num2 != _y) ? ((sbyte)((float)MarchingCubes.DensityAir * (1f - num3))) : ((sbyte)((float)MarchingCubes.DensityTerrain * num3)));
			if (b == 0)
			{
				b = (sbyte)((!_bv.Block.shape.IsTerrain()) ? 1 : (-1));
			}
		}
		_chunk.SetDensity(_x, _y, _z, b);
	}

	public override sbyte GetDensityAt(int _xWorld, int _yWorld, int _zWorld, BiomeDefinition _bd, float _biomeIntensity)
	{
		if (!checkCoordinates(ref _xWorld, ref _zWorld))
		{
			return MarchingCubes.DensityAir;
		}
		if (!(heightMap.GetAt(_xWorld, _zWorld) <= (float)_yWorld))
		{
			return MarchingCubes.DensityAir;
		}
		return MarchingCubes.DensityTerrain;
	}

	public override byte GetTerrainHeightAt(int _x, int _z, BiomeDefinition _bd, float _biomeIntensity)
	{
		if (!checkCoordinates(ref _x, ref _z))
		{
			return 0;
		}
		return (byte)(int)(heightMap.GetAt(_x, _z) + 0.5f);
	}

	public override float GetTerrainHeightAt(int _x, int _z)
	{
		if (!checkCoordinates(ref _x, ref _z))
		{
			return 0f;
		}
		return heightMap.GetAt(_x, _z);
	}

	public List<float[,]> ConvertToUnityHeightmap(int _sliceAtWidth)
	{
		int width = heightMap.GetWidth();
		int height = heightMap.GetHeight();
		int scaleSteps = heightMap.GetScaleSteps();
		List<float[,]> list = new List<float[,]>();
		int num = width / _sliceAtWidth;
		int num2 = height / _sliceAtWidth;
		_ = new Terrain[num, num2];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				float[,] array = new float[_sliceAtWidth, _sliceAtWidth];
				for (int num3 = _sliceAtWidth - 1; num3 >= 0; num3--)
				{
					for (int num4 = _sliceAtWidth - 1; num4 >= 0; num4--)
					{
						float at = heightMap.GetAt(j * _sliceAtWidth + num3 * scaleSteps, i * _sliceAtWidth + num4 * scaleSteps);
						array[num4, num3] = at / 256f;
					}
				}
				list.Add(array);
			}
		}
		return list;
	}
}
