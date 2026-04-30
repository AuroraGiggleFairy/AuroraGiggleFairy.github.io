using System.Collections.Generic;
using System.Threading;

public class WorldDecoratorBlocksFromBiome : IWorldDecorator
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IBiomeProvider biomeProvider;

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicPrefabDecorator prefabDecorator;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim();

	[PublicizedFrom(EAccessModifier.Private)]
	public PerlinNoise resourceNoise;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BiomeDefinition[] chunkBiomes = new BiomeDefinition[256];

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cBiomeIdMax = 256;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Vector2i>[] biomePositions = new List<Vector2i>[256];

	[PublicizedFrom(EAccessModifier.Private)]
	public int biomePositionsHighId = -1;

	public WorldDecoratorBlocksFromBiome(IBiomeProvider _biomeProvider, DynamicPrefabDecorator _prefabDecorator)
	{
		biomeProvider = _biomeProvider;
		prefabDecorator = _prefabDecorator;
	}

	public void DecorateChunkOverlapping(World _world, Chunk _chunk, Chunk cX1Z0, Chunk cX0Z1, Chunk cX1Z1, int seed)
	{
		rwlock.EnterWriteLock();
		GameRandom gameRandom = Utils.RandomFromSeedOnPos(_chunk.X, _chunk.Z, seed);
		if (resourceNoise == null)
		{
			resourceNoise = new PerlinNoise(seed);
		}
		for (int num = biomePositionsHighId; num >= 0; num--)
		{
			biomePositions[num]?.Clear();
		}
		IChunkProvider chunkProvider = _world.ChunkCache.ChunkProvider;
		BiomeDefinition biome = _world.GetBiome("underwater");
		int num2 = 0;
		int blockWorldPosX = _chunk.GetBlockWorldPosX(0);
		int blockWorldPosZ = _chunk.GetBlockWorldPosZ(0);
		bool flag = prefabDecorator.IsWithinTraderArea(_chunk.worldPosIMin, _chunk.worldPosIMax);
		Vector2i item = default(Vector2i);
		for (int i = 0; i < 16; i++)
		{
			int z = blockWorldPosZ + i;
			item.y = i;
			for (int j = 0; j < 16; j++)
			{
				int x = blockWorldPosX + j;
				if (!flag || prefabDecorator.GetTraderAtPosition(new Vector3i(x, 0, z), 0) == null)
				{
					int pOIBlockIdOverride = chunkProvider.GetPOIBlockIdOverride(x, z);
					BiomeDefinition biomeDefinition = ((pOIBlockIdOverride <= 0 || !Block.list[pOIBlockIdOverride].blockMaterial.IsLiquid) ? biomeProvider.GetBiomeAt(x, z) : biome);
					if (biomeDefinition == null)
					{
						rwlock.ExitWriteLock();
						return;
					}
					List<Vector2i> list = biomePositions[biomeDefinition.m_Id];
					if (list == null)
					{
						list = new List<Vector2i>(256);
						biomePositions[biomeDefinition.m_Id] = list;
						biomePositionsHighId = Utils.FastMax(biomePositionsHighId, biomeDefinition.m_Id);
					}
					item.x = j;
					list.Add(item);
					int terrainHeight = _chunk.GetTerrainHeight(j, i);
					int subBiomeIdxAt = biomeProvider.GetSubBiomeIdxAt(biomeDefinition, x, terrainHeight, z);
					if (subBiomeIdxAt >= 0)
					{
						biomeDefinition = biomeDefinition.subbiomes[subBiomeIdxAt];
					}
					chunkBiomes[num2] = biomeDefinition;
					num2++;
				}
			}
		}
		decoratePrefabs(_world, _chunk, cX1Z0, cX0Z1, cX1Z1, flag, gameRandom);
		decorateSingleBlocks(_world, _chunk, cX1Z0, cX0Z1, cX1Z1, flag, gameRandom);
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
		rwlock.ExitWriteLock();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void decoratePrefabs(World _world, Chunk _chunk, Chunk cX1Z0, Chunk cX0Z1, Chunk cX1Z1, bool _chunkOverlapsTrader, GameRandom _random)
	{
		if (prefabDecorator == null)
		{
			return;
		}
		for (int i = 0; i <= biomePositionsHighId; i++)
		{
			List<Vector2i> list = biomePositions[i];
			if (list == null)
			{
				continue;
			}
			int count = list.Count;
			if (count == 0)
			{
				continue;
			}
			for (int j = 0; j < count; j++)
			{
				int x = list[j].x;
				int y = list[j].y;
				int blockWorldPosX = _chunk.GetBlockWorldPosX(x);
				int blockWorldPosZ = _chunk.GetBlockWorldPosZ(y);
				if (_chunkOverlapsTrader && prefabDecorator.GetTraderAtPosition(new Vector3i(blockWorldPosX, 0, blockWorldPosZ), 0) != null)
				{
					continue;
				}
				BiomeDefinition biomeDefinition = chunkBiomes[x + y * 16];
				if (biomeDefinition.m_DecoPrefabs.Count == 0)
				{
					continue;
				}
				BiomePrefabDecoration biomePrefabDecoration = biomeDefinition.m_DecoPrefabs[_random.RandomRange(biomeDefinition.m_DecoPrefabs.Count)];
				if (biomePrefabDecoration == null || biomePrefabDecoration.prob < _random.RandomFloat)
				{
					continue;
				}
				EnumDecoAllowed decoAllowedAt = _chunk.GetDecoAllowedAt(x, y);
				if (decoAllowedAt.IsNothing() || decoAllowedAt.GetStreetOnly())
				{
					continue;
				}
				EnumDecoAllowedSlope slope = decoAllowedAt.GetSlope();
				if (!biomePrefabDecoration.isDecorateOnSlopes)
				{
					if ((int)slope >= 1)
					{
						continue;
					}
				}
				else if ((int)slope >= 2)
				{
					continue;
				}
				Prefab prefab = prefabDecorator.GetPrefab(biomePrefabDecoration.prefabName);
				if (prefab == null)
				{
					Log.Error("Prefab with name '" + biomePrefabDecoration.prefabName + "' not found!");
					continue;
				}
				Vector3i size = prefab.size;
				if (x + size.x / 2 >= 16 || y + size.z / 2 >= 16)
				{
					continue;
				}
				int num = _chunk.GetTerrainHeight(x + size.x / 2, y + size.z / 2) + 1;
				if ((biomePrefabDecoration.checkResourceOffsetY < int.MaxValue && (num + prefab.yOffset < 0 || !GameUtils.CheckOreNoiseAt(resourceNoise, blockWorldPosX, num + biomePrefabDecoration.checkResourceOffsetY, blockWorldPosZ))) || !_chunk.GetBlock(x, num + 1, y).isair)
				{
					continue;
				}
				BlockValue block = _chunk.GetBlock(x, num - 1, y);
				if (block.isair || block.Equals(Block.GetBlockValue("water")))
				{
					continue;
				}
				bool flag = true;
				bool flag2 = size.x > 1 || size.z > 1;
				int num2 = 0;
				while (flag && num2 < size.x)
				{
					int num3 = x + num2;
					int x2 = World.toBlockXZ(blockWorldPosX + num2);
					int num4 = 0;
					while (flag && num4 < size.z)
					{
						Chunk chunk = _chunk;
						int num5 = y + num4;
						if (num3 >= 16)
						{
							chunk = cX1Z0;
							if (num5 >= 16)
							{
								chunk = cX1Z1;
							}
						}
						else if (num5 >= 16)
						{
							chunk = cX0Z1;
						}
						int z = World.toBlockXZ(blockWorldPosZ + num4);
						EnumDecoAllowed decoAllowedAt2 = chunk.GetDecoAllowedAt(x2, z);
						if (decoAllowedAt2.IsNothing())
						{
							flag = false;
						}
						if (flag && decoAllowedAt2.GetStreetOnly())
						{
							flag = false;
						}
						if (flag && flag2 && (int)decoAllowedAt2.GetSize() >= 1)
						{
							flag = false;
						}
						EnumDecoAllowedSlope slope2 = decoAllowedAt2.GetSlope();
						if (!biomePrefabDecoration.isDecorateOnSlopes)
						{
							if (flag && (int)slope2 >= 1)
							{
								flag = false;
							}
							if (flag && chunk.GetHeight(x2, z) != num - 1)
							{
								flag = false;
							}
						}
						else if (flag && (int)slope2 >= 2)
						{
							flag = false;
						}
						if (chunk.GetHeight(x2, z) + prefab.size.y >= 255)
						{
							flag = false;
						}
						num4++;
					}
					num2++;
				}
				if (flag)
				{
					int num6 = _random.RandomRange(4);
					if (num6 != 0)
					{
						prefab = prefab.Clone();
						prefab.RotateY(_bLeft: true, num6);
					}
					Vector3i destinationPos = new Vector3i(blockWorldPosX, num + prefab.yOffset, blockWorldPosZ);
					prefab.CopyIntoLocal(_world.ChunkClusters[0], destinationPos, _bOverwriteExistingBlocks: false, _bSetChunkToRegenerate: false, FastTags<TagGroup.Global>.none);
					prefab.SnapTerrainToArea(_world.ChunkClusters[0], destinationPos);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void decorateSingleBlocks(World _world, Chunk _chunk, Chunk cX1Z0, Chunk cX0Z1, Chunk cX1Z1, bool _chunkOverlapsTrader, GameRandom _random)
	{
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				int blockWorldPosX = _chunk.GetBlockWorldPosX(j);
				int blockWorldPosZ = _chunk.GetBlockWorldPosZ(i);
				if (!_chunkOverlapsTrader || prefabDecorator.GetTraderAtPosition(new Vector3i(blockWorldPosX, 0, blockWorldPosZ), 0) == null)
				{
					int num = _chunk.GetTerrainHeight(j, i) + 1;
					if (num < 255)
					{
						Vector3i blockPos = new Vector3i(j, num, i);
						decorateSingleBlock(_world, _chunk, cX1Z0, cX0Z1, cX1Z1, _random, blockPos);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void decorateSingleBlock(World _world, Chunk _chunk, Chunk cX1Z0, Chunk cX0Z1, Chunk cX1Z1, GameRandom _random, Vector3i blockPos)
	{
		if (!_chunk.GetBlockNoDamage(blockPos.x, blockPos.y, blockPos.z).isair)
		{
			return;
		}
		bool flag = _chunk.IsWater(blockPos.x, blockPos.y, blockPos.z);
		EnumDecoAllowed decoAllowedAt = _chunk.GetDecoAllowedAt(blockPos.x, blockPos.z);
		if ((!flag && decoAllowedAt.IsNothing()) || decoAllowedAt.GetStreetOnly())
		{
			return;
		}
		BlockValue blockNoDamage = _chunk.GetBlockNoDamage(blockPos.x, blockPos.y + 1, blockPos.z);
		BlockValue blockNoDamage2 = _chunk.GetBlockNoDamage(blockPos.x, blockPos.y - 1, blockPos.z);
		if (!blockNoDamage.isair || blockNoDamage2.isair || (flag && !_chunk.IsWater(blockPos.x, blockPos.y + 1, blockPos.z)))
		{
			return;
		}
		int num = blockPos.x + blockPos.z * 16;
		BiomeDefinition biomeDefinition = chunkBiomes[num];
		Vector3i worldPos = _chunk.GetWorldPos();
		float terrainNormalY = _chunk.GetTerrainNormalY(blockPos.x, blockPos.z);
		for (int i = 0; i < biomeDefinition.m_DecoBlocks.Count; i++)
		{
			BiomeBlockDecoration deco = biomeDefinition.m_DecoBlocks[i];
			if (decorateSingleBlockTryPlaceDeco(_world, _chunk, cX1Z0, cX0Z1, cX1Z1, _random, blockPos, decoAllowedAt, biomeDefinition, deco, worldPos, terrainNormalY))
			{
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool decorateSingleBlockTryPlaceDeco(World _world, Chunk _chunk, Chunk cX1Z0, Chunk cX0Z1, Chunk cX1Z1, GameRandom _random, Vector3i blockPos, EnumDecoAllowed decoAllowed, BiomeDefinition bd, BiomeBlockDecoration deco, Vector3i chunkWorldPos, float normalY)
	{
		BlockValue blockValue = deco.blockValues[0];
		Block block = blockValue.Block;
		if (block.IsDistantDecoration && DecoManager.Instance.IsEnabled)
		{
			return false;
		}
		if (!block.CanDecorateOnSlopes)
		{
			if ((int)decoAllowed.GetSlope() >= 1)
			{
				return false;
			}
		}
		else if (block.SlopeMaxCos > normalY)
		{
			return false;
		}
		if (block.IsPlant() && blockPos.y > 0 && _chunk.GetBlock(blockPos.x, blockPos.y - 1, blockPos.z).Block.blockMaterial.FertileLevel == 0)
		{
			return false;
		}
		if (block.isMultiBlock && block.multiBlockPos.dim.y + blockPos.y > 255)
		{
			return false;
		}
		if (_random.RandomFloat >= deco.prob)
		{
			return false;
		}
		if (deco.checkResourceOffsetY < int.MaxValue)
		{
			Vector3i vector3i = _chunk.ToWorldPos(blockPos);
			vector3i.y += deco.checkResourceOffsetY;
			if (!GameUtils.CheckOreNoiseAt(resourceNoise, vector3i.x, vector3i.y, vector3i.z))
			{
				return false;
			}
		}
		BlockValue blockValue2 = BlockPlaceholderMap.Instance.Replace(blockValue, _random, _chunk, chunkWorldPos.x + blockPos.x, 0, chunkWorldPos.z + blockPos.z, FastTags<TagGroup.Global>.none);
		if (blockValue2.isair)
		{
			return true;
		}
		if (deco.randomRotateMax > 0)
		{
			blockValue2.rotation = BiomeBlockDecoration.GetRandomRotation(_random.RandomFloat, deco.randomRotateMax);
		}
		Block block2 = blockValue2.Block;
		int decoRadius = DecoUtils.GetDecoRadius(blockValue2, block2);
		if (decoRadius > 0)
		{
			blockPos.x += decoRadius;
			blockPos.z += decoRadius;
			if (blockPos.x >= 16 || blockPos.z >= 16)
			{
				return false;
			}
			blockPos.y = _chunk.GetTerrainHeight(blockPos.x, blockPos.z) + 1;
			if (block.IsPlant() && _chunk.GetBlock(blockPos.x, blockPos.y - 1, blockPos.z).Block.blockMaterial.FertileLevel == 0)
			{
				return false;
			}
			if (!_chunk.GetBlock(blockPos).isair)
			{
				return false;
			}
		}
		if (!DecoUtils.CanPlaceDeco(_chunk, cX1Z0, cX0Z1, cX1Z1, chunkWorldPos + blockPos, blockValue2, [PublicizedFrom(EAccessModifier.Internal)] (EnumDecoAllowed da) => !da.GetStreetOnly()))
		{
			return false;
		}
		DecoUtils.ApplyDecoAllowed(_chunk, cX1Z0, cX1Z0, cX1Z1, chunkWorldPos + blockPos, blockValue2);
		blockValue2 = block2.OnBlockPlaced(_world, 0, chunkWorldPos + blockPos, blockValue2, _random);
		_chunk.SetBlock(_world, blockPos.x, blockPos.y, blockPos.z, blockValue2);
		if (!block2.shape.IsOmitTerrainSnappingUp && !block2.IsTerrainDecoration)
		{
			_world.ChunkCache.SnapTerrainToPositionAroundLocal(_chunk.ToWorldPos(blockPos) - Vector3i.up);
		}
		return true;
	}
}
