using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ChunkProviderGenerateFlat : ChunkProviderAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly int defaultWorldAreaChunkDim = 32;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly int prefabChunkBorderSize = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string worldName;

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicPrefabDecorator prefabDecorator;

	[PublicizedFrom(EAccessModifier.Private)]
	public IChunkProviderIndicator chunkManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ChunkCluster cc;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnPointManager spawnPointManager;

	public ChunkProviderGenerateFlat(ChunkCluster _cc, string _worldName)
	{
		cc = _cc;
		worldName = _worldName;
	}

	public override IEnumerator Init(World _world)
	{
		world = _world;
		GamePrefs.OnGamePrefChanged += GamePrefChanged;
		bool flag = false;
		bool createGroundTerrain = false;
		bool createQuestContainerAndTrader = false;
		Prefab prefabToPlace = null;
		Vector3i playerPos = Vector3i.zero;
		if (GameUtils.IsPlaytesting())
		{
			flag = true;
			createGroundTerrain = true;
			createQuestContainerAndTrader = true;
			prefabToPlace = new Prefab();
			prefabToPlace.Load(GamePrefs.GetString(EnumGamePrefs.GameName));
			playerPos = new Vector3i(1f, WorldConstants.WaterLevel, -prefabToPlace.size.z / 2 - 4 - 10);
		}
		MultiBlockManager.Instance.Initialize(null);
		prefabDecorator = new DynamicPrefabDecorator();
		if (flag)
		{
			if (SdDirectory.Exists(GameIO.GetSaveGameRegionDir()))
			{
				SdDirectory.Delete(GameIO.GetSaveGameRegionDir(), recursive: true);
			}
			if (SdFile.Exists(GameIO.GetSaveGameDir() + "/decoration.7dt"))
			{
				SdFile.Delete(GameIO.GetSaveGameDir() + "/decoration.7dt");
			}
		}
		bool num = _world.IsEditor() || !SdDirectory.Exists(GameIO.GetSaveGameRegionDir());
		cc.IsFixedSize = true;
		PathAbstractions.AbstractedLocation worldLocation = PathAbstractions.WorldsSearchPaths.GetLocation(worldName);
		if (num)
		{
			yield return prefabDecorator.Load(worldLocation.FullPath);
			prefabDecorator.CopyAllPrefabsIntoWorld(world);
		}
		spawnPointManager = new SpawnPointManager();
		spawnPointManager.Load(worldLocation.FullPath);
		BoundsInt boundsInt;
		if (prefabToPlace != null)
		{
			int num2 = -1 * prefabToPlace.size.x / 2;
			int num3 = -1 * prefabToPlace.size.z / 2;
			int num4 = num2 + prefabToPlace.size.x;
			int num5 = num3 + prefabToPlace.size.z;
			Vector3Int vector3Int = new Vector3Int((num2 - 1) / 16 - 1, 0, (num3 - 1) / 16 - 1);
			vector3Int -= new Vector3Int(prefabChunkBorderSize, 0, prefabChunkBorderSize + 3);
			Vector3Int vector3Int2 = new Vector3Int((num4 + 1) / 16 + 1, 0, (num5 + 1) / 16 + 1);
			vector3Int2 += new Vector3Int(prefabChunkBorderSize, 0, prefabChunkBorderSize);
			boundsInt = new BoundsInt(vector3Int, vector3Int2 - vector3Int);
		}
		else
		{
			boundsInt = new BoundsInt(new Vector3Int(-defaultWorldAreaChunkDim / 2, 0, -defaultWorldAreaChunkDim / 2), new Vector3Int(defaultWorldAreaChunkDim, 0, defaultWorldAreaChunkDim));
		}
		world.m_ChunkManager.RemoveAllChunks();
		BiomeDefinition.BiomeType biomeType = (BiomeDefinition.BiomeType)GamePrefs.GetInt(EnumGamePrefs.PlaytestBiome);
		if (biomeType <= BiomeDefinition.BiomeType.Any || biomeType > BiomeDefinition.BiomeType.burnt_forest)
		{
			biomeType = BiomeDefinition.BiomeType.PineForest;
		}
		byte biomeId = (byte)biomeType;
		for (int i = boundsInt.xMin; i <= boundsInt.xMax; i++)
		{
			for (int j = boundsInt.zMin; j <= boundsInt.zMax; j++)
			{
				Chunk chunk = MemoryPools.PoolChunks.AllocSync(_bReset: true);
				chunk.X = i;
				chunk.Z = j;
				chunk.FillBiomeId(biomeId);
				if (createGroundTerrain)
				{
					chunk.FillBlockRaw((int)WorldConstants.WaterLevel - 1, Block.GetBlockValue("terrainFiller"));
				}
				chunk.SetFullSunlight();
				chunk.NeedsLightCalculation = false;
				chunk.NeedsDecoration = false;
				chunk.NeedsRegeneration = false;
				cc.AddChunkSync(chunk, _bOmitCallbacks: true);
			}
		}
		List<Chunk> chunkArrayCopySync = world.ChunkClusters[0].GetChunkArrayCopySync();
		if (prefabToPlace != null)
		{
			PrefabInstance pi = new PrefabInstance(_position: new Vector3i(-prefabToPlace.size.x / 2, WorldConstants.WaterLevel + (float)prefabToPlace.yOffset, -prefabToPlace.size.z / 2), _id: 1, _location: prefabToPlace.location, _rotation: 0, _bad: prefabToPlace, _standaloneBlockSize: 0);
			prefabDecorator.AddPrefab(pi, _isPOI: true);
			prefabDecorator.CopyAllPrefabsIntoWorld(world);
			foreach (Chunk item in chunkArrayCopySync)
			{
				WaterSimulationNative.Instance.InitializeChunk(item);
			}
		}
		if (playerPos == Vector3i.zero)
		{
			playerPos = new Vector3i(0f, createGroundTerrain ? (WorldConstants.WaterLevel + 1f) : 16f, -10f);
		}
		if (createQuestContainerAndTrader)
		{
			world.SetBlock(0, playerPos + new Vector3i(1, 0, 3), Block.GetBlockValue("cntQuestTestLoot"), bNotify: false, updateLight: false);
			Vector3i vector3i = playerPos + new Vector3i(-3, 0, 3);
			if (world.GetChunkFromWorldPos(vector3i) != null)
			{
				EntityCreationData entityCreationData = new EntityCreationData();
				entityCreationData.pos = vector3i;
				entityCreationData.rot = new Vector3(0f, 180f, 0f);
				entityCreationData.entityClass = EntityClass.FromString("npcTraderTest");
				_world.SpawnEntityInWorld(EntityFactory.CreateEntity(entityCreationData));
			}
		}
		foreach (Chunk item2 in chunkArrayCopySync)
		{
			item2.ResetStability();
			item2.ResetLights(0);
			item2.RefreshSunlight();
			item2.NeedsLightCalculation = true;
		}
		spawnPointManager.spawnPointList = new SpawnPointList
		{
			new SpawnPoint(playerPos)
		};
		yield return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GamePrefChanged(EnumGamePrefs _pref)
	{
		if (_pref != EnumGamePrefs.PlaytestBiome)
		{
			return;
		}
		BiomeDefinition.BiomeType biomeType = (BiomeDefinition.BiomeType)GamePrefs.GetInt(EnumGamePrefs.PlaytestBiome);
		if (biomeType <= BiomeDefinition.BiomeType.Any || biomeType > BiomeDefinition.BiomeType.burnt_forest)
		{
			biomeType = BiomeDefinition.BiomeType.PineForest;
		}
		byte biomeId = (byte)biomeType;
		foreach (Chunk item in world.ChunkClusters[0].GetChunkArrayCopySync())
		{
			item.FillBiomeId(biomeId);
			item.ResetStability();
			item.ResetLights(0);
			item.RefreshSunlight();
			item.NeedsLightCalculation = true;
		}
	}

	public override DynamicPrefabDecorator GetDynamicPrefabDecorator()
	{
		return prefabDecorator;
	}

	public override SpawnPointList GetSpawnPointList()
	{
		return spawnPointManager.spawnPointList;
	}

	public override void SetSpawnPointList(SpawnPointList _spawnPointList)
	{
		spawnPointManager.spawnPointList = _spawnPointList;
	}

	public override bool GetOverviewMap(Vector2i _startPos, Vector2i _size, Color[] _mapColors)
	{
		return false;
	}

	public override EnumChunkProviderId GetProviderId()
	{
		return EnumChunkProviderId.FlatWorld;
	}

	public override void Cleanup()
	{
		GamePrefs.OnGamePrefChanged -= GamePrefChanged;
		Thread.Sleep(200);
		MultiBlockManager.Instance.Cleanup();
	}

	public override bool GetWorldExtent(out Vector3i _minSize, out Vector3i _maxSize)
	{
		_minSize = new Vector3i(world.ChunkCache.ChunkMinPos.x * 16, 0, world.ChunkCache.ChunkMinPos.y * 16);
		_maxSize = new Vector3i(world.ChunkCache.ChunkMaxPos.x * 16, 255, world.ChunkCache.ChunkMaxPos.y * 16);
		return true;
	}

	public override void RebuildTerrain(HashSetLong _chunks, Vector3i _areaStart, Vector3i _areaSize, bool _bStopStabilityUpdate, bool _bRegenerateChunk, bool _bFillEmptyBlocks, bool _isReset)
	{
		if (_bStopStabilityUpdate)
		{
			foreach (long _chunk in _chunks)
			{
				Chunk chunkSync = cc.GetChunkSync(_chunk);
				if (chunkSync != null)
				{
					chunkSync.StopStabilityCalculation = true;
				}
			}
		}
		PathAbstractions.AbstractedLocation location = PathAbstractions.WorldsSearchPaths.GetLocation(worldName);
		ThreadManager.RunCoroutineSync(prefabDecorator.Load(location.FullPath));
		prefabDecorator.CopyAllPrefabsIntoWorld(world, _bOverwriteExistingBlocks: true);
		Chunk[] neighbours = new Chunk[8];
		foreach (Chunk item in world.ChunkClusters[0].GetChunkArrayCopySync())
		{
			item.ResetStability();
			item.ResetLights(0);
			item.RefreshSunlight();
			if (cc.GetNeighborChunks(item, neighbours))
			{
				cc.LightChunk(item, neighbours);
			}
			List<TileEntity> list = item.GetTileEntities().list;
			for (int i = 0; i < list.Count; i++)
			{
				list[i].Reset(FastTags<TagGroup.Global>.none);
			}
		}
		List<EntityPlayerLocal> localPlayers = world.GetLocalPlayers();
		foreach (long _chunk2 in _chunks)
		{
			Chunk chunkSync2 = cc.GetChunkSync(_chunk2);
			if (chunkSync2 == null)
			{
				continue;
			}
			for (int j = 0; j < world.m_ChunkManager.m_ObservedEntities.Count; j++)
			{
				if (world.m_ChunkManager.m_ObservedEntities[j].bBuildVisualMeshAround)
				{
					continue;
				}
				bool flag = false;
				int num = 0;
				while (!flag && num < localPlayers.Count)
				{
					if (world.m_ChunkManager.m_ObservedEntities[j].entityIdToSendChunksTo == localPlayers[num].entityId)
					{
						flag = true;
					}
					num++;
				}
				if (!flag && world.m_ChunkManager.m_ObservedEntities[j].chunksLoaded.Contains(_chunk2))
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(chunkSync2, _bOverwriteExisting: true), _onlyClientsAttachedToAnEntity: false, world.m_ChunkManager.m_ObservedEntities[j].entityIdToSendChunksTo);
				}
			}
		}
	}
}
