using UnityEngine;

public abstract class TerrainGeneratorWithBiomeResource : ITerrainGenerator
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IBiomeProvider biomeProvider;

	[PublicizedFrom(EAccessModifier.Private)]
	public PerlinNoise perlinNoise;

	public virtual void Init(World _world, IBiomeProvider _biomeProvider, int _seed)
	{
		biomeProvider = _biomeProvider;
		perlinNoise = new PerlinNoise(_seed);
	}

	public abstract byte GetTerrainHeightByteAt(int _xWorld, int _zWorld);

	public virtual float GetTerrainHeightAt(int x, int z)
	{
		return 0f;
	}

	public virtual Vector3 GetTerrainNormalAt(int _xWorld, int _zWorld, BiomeDefinition _bd, float _biomeIntensity)
	{
		return Vector3.up;
	}

	public abstract sbyte GetDensityAt(int _xWorld, int _yWorld, int _zWorld, BiomeDefinition _bd, float _biomeIntensity);

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void fillDensityInBlock(Chunk _chunk, int _x, int _y, int _z, BlockValue _bv)
	{
		sbyte density = (_bv.Block.shape.IsTerrain() ? MarchingCubes.DensityTerrain : MarchingCubes.DensityAir);
		_chunk.SetDensity(_x, _y, _z, density);
	}

	public virtual void GenerateTerrain(World _world, Chunk _chunk, GameRandom _random)
	{
		GenerateTerrain(_world, _chunk, _random, Vector3i.zero, Vector3i.zero, _bFillEmptyBlocks: false, _isReset: false);
	}

	public virtual void GenerateTerrain(World _world, Chunk _chunk, GameRandom _random, Vector3i _areaStart, Vector3i _areaSize, bool _bFillEmptyBlocks, bool _isReset)
	{
		int num = 0;
		int num2 = 16;
		int num3 = 0;
		int num4 = 16;
		if (_areaSize.x > 0 && _areaSize.z > 0)
		{
			Vector3i vector3i = _chunk.ToWorldPos(Vector3i.zero);
			Vector3i vector3i2 = vector3i + new Vector3i(16, 0, 16);
			if (vector3i2.x <= _areaStart.x || vector3i.x > _areaStart.x + _areaSize.x || vector3i2.z <= _areaStart.z || vector3i.z > _areaStart.z + _areaSize.z)
			{
				return;
			}
			num = _areaStart.x - vector3i.x;
			num2 = vector3i2.x - (_areaStart.x + _areaSize.x);
			num3 = _areaStart.z - vector3i.z;
			num4 = vector3i2.z - (_areaStart.z + _areaSize.z);
			if (num < 0)
			{
				num = 0;
			}
			if (num3 < 0)
			{
				num3 = 0;
			}
			num2 = ((num2 >= 0) ? (16 - num2) : 16);
			num4 = ((num4 >= 0) ? (16 - num4) : 16);
		}
		for (int i = num3; i < num4; i++)
		{
			int blockWorldPosZ = _chunk.GetBlockWorldPosZ(i);
			for (int j = num; j < num2; j++)
			{
				int blockWorldPosX = _chunk.GetBlockWorldPosX(j);
				BiomeDefinition biomeDefinition = biomeProvider.GetBiomeAt(blockWorldPosX, blockWorldPosZ);
				if (biomeDefinition == null)
				{
					continue;
				}
				_chunk.SetBiomeId(j, i, biomeDefinition.m_Id);
				byte terrainHeightByteAt = GetTerrainHeightByteAt(blockWorldPosX, blockWorldPosZ);
				_chunk.SetTerrainHeight(j, i, terrainHeightByteAt);
				_chunk.SetHeight(j, i, terrainHeightByteAt);
				int subBiomeIdxAt = biomeProvider.GetSubBiomeIdxAt(biomeDefinition, blockWorldPosX, terrainHeightByteAt, blockWorldPosZ);
				if (subBiomeIdxAt >= 0 && subBiomeIdxAt < biomeDefinition.subbiomes.Count)
				{
					biomeDefinition = biomeDefinition.subbiomes[subBiomeIdxAt];
				}
				int num5 = (terrainHeightByteAt + 1) & 0xFF;
				if (_bFillEmptyBlocks)
				{
					for (int num6 = 255; num6 >= num5; num6--)
					{
						_chunk.SetBlockRaw(j, num6, i, BlockValue.Air);
						_chunk.SetDensity(j, num6, i, MarchingCubes.DensityAir);
					}
				}
				fillDensityInBlock(_chunk, j, num5, i, BlockValue.Air);
				num5--;
				if (num5 < 0)
				{
					continue;
				}
				int count = biomeDefinition.m_Layers.Count;
				int num7 = count - 1;
				int depth = biomeDefinition.m_Layers[num7].m_Depth;
				for (int k = 0; k < count; k++)
				{
					if (num5 < 0)
					{
						break;
					}
					BiomeLayer biomeLayer = biomeDefinition.m_Layers[k];
					int num8 = biomeLayer.m_Depth;
					if (num8 < 0)
					{
						num8 = terrainHeightByteAt - biomeDefinition.TotalLayerDepth;
					}
					int num9 = 0;
					while (num9 < num8 && num5 >= 0)
					{
						if (num5 < depth && k != num7)
						{
							k = num7 - 1;
							break;
						}
						BlockValue blockValue = BlockValue.Air;
						int count2 = biomeLayer.m_Resources.Count;
						if (count2 > 0 && GameUtils.GetOreNoiseAt(perlinNoise, blockWorldPosX, num5, blockWorldPosZ) > 0f)
						{
							float randomFloat = _random.RandomFloat;
							for (int l = 0; l < count2; l++)
							{
								if (randomFloat < biomeLayer.SumResourceProbs[l])
								{
									blockValue = biomeLayer.m_Resources[l].blockValues[0];
									break;
								}
							}
						}
						if (blockValue.isair)
						{
							int num10 = biomeLayer.m_Block.blockValues.Length - 1;
							if (num10 > 0)
							{
								num10 = _random.RandomRange(0, num10 + 1);
							}
							blockValue = biomeLayer.m_Block.blockValues[num10];
						}
						int blockId = _chunk.GetBlockId(j, num5, i);
						if (blockId >= 256 && Block.list[blockId].shape is BlockShapeModelEntity)
						{
							_chunk.SetBlock(_world, j, num5, i, blockValue, _notifyAddChange: true, _notifyRemove: true, _isReset, _isReset);
						}
						else
						{
							_chunk.SetBlockRaw(j, num5, i, blockValue);
						}
						fillDensityInBlock(_chunk, j, num5, i, blockValue);
						num9++;
						num5--;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public TerrainGeneratorWithBiomeResource()
	{
	}
}
