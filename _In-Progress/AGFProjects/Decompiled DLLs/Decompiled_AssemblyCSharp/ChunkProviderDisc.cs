using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ChunkProviderDisc : ChunkProviderAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public World world;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string worldName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string gameName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public WorldState m_HeaderInfo;

	[PublicizedFrom(EAccessModifier.Protected)]
	public DynamicPrefabDecorator prefabDecorator;

	[PublicizedFrom(EAccessModifier.Protected)]
	public RegionFileManager m_RegionFileManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public IChunkProviderIndicator chunkManager;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ChunkCluster cc;

	[PublicizedFrom(EAccessModifier.Protected)]
	public SpawnPointManager spawnPointManager;

	public ChunkProviderDisc(ChunkCluster _cc, string _worldName)
	{
		cc = _cc;
		worldName = _worldName;
	}

	public override IEnumerator Init(World _world)
	{
		world = _world;
		prefabDecorator = new DynamicPrefabDecorator();
		if (GameUtils.IsPlaytesting())
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
		bool flag = false;
		if (_world.IsEditor() || !SdDirectory.Exists(GameIO.GetSaveGameRegionDir()))
		{
			flag = true;
		}
		m_RegionFileManager = new RegionFileManager(GameIO.GetSaveGameRegionDirDefault(), (!_world.IsEditor()) ? GameIO.GetSaveGameRegionDir() : GameIO.GetSaveGameRegionDirDefault(), 0, _bSaveOnChunkDrop: false);
		cc.IsFixedSize = true;
		long[] allChunkKeys = m_RegionFileManager.GetAllChunkKeys();
		for (int i = 0; i < allChunkKeys.Length; i++)
		{
			Chunk chunkSync = m_RegionFileManager.GetChunkSync(allChunkKeys[i]);
			if (chunkSync != null)
			{
				chunkSync.FillBiomeId(3);
				chunkSync.NeedsRegeneration = false;
				chunkSync.NeedsLightCalculation = false;
				cc.AddChunkSync(chunkSync);
			}
		}
		PathAbstractions.AbstractedLocation worldLocation = PathAbstractions.WorldsSearchPaths.GetLocation(worldName);
		if (flag)
		{
			yield return prefabDecorator.Load(worldLocation.FullPath);
			prefabDecorator.CopyAllPrefabsIntoWorld(world);
		}
		spawnPointManager = new SpawnPointManager();
		spawnPointManager.Load(worldLocation.FullPath);
		if (GameUtils.IsPlaytesting())
		{
			Prefab prefab = new Prefab();
			prefab.Load(GamePrefs.GetString(EnumGamePrefs.GameName));
			world.m_ChunkManager.RemoveAllChunks();
			int num = -1 * prefab.size.x / 2;
			int num2 = -1 * prefab.size.z / 2;
			int num3 = num + prefab.size.x;
			int num4 = num2 + prefab.size.z;
			cc.ChunkMinPos = new Vector2i((num - 1) / 16 - 1, (num2 - 1) / 16 - 1);
			cc.ChunkMinPos -= new Vector2i(2, 5);
			cc.ChunkMaxPos = new Vector2i(num3 / 16 + 1, num4 / 16 + 1);
			cc.ChunkMaxPos += new Vector2i(2, 2);
			for (int j = cc.ChunkMinPos.x; j <= cc.ChunkMaxPos.x; j++)
			{
				for (int k = cc.ChunkMinPos.y; k <= cc.ChunkMaxPos.y; k++)
				{
					Chunk chunk = MemoryPools.PoolChunks.AllocSync(_bReset: true);
					chunk.X = j;
					chunk.Z = k;
					chunk.FillBiomeId(3);
					chunk.FillBlockRaw((int)WorldConstants.WaterLevel - 1, Block.GetBlockValue("terrainFiller"));
					chunk.SetFullSunlight();
					chunk.NeedsLightCalculation = false;
					chunk.NeedsDecoration = false;
					chunk.NeedsRegeneration = false;
					cc.AddChunkSync(chunk, _bOmitCallbacks: true);
				}
			}
			PrefabInstance pi = new PrefabInstance(_position: new Vector3i(-prefab.size.x / 2, WorldConstants.WaterLevel + (float)prefab.yOffset, -prefab.size.z / 2), _id: 1, _location: prefab.location, _rotation: 0, _bad: prefab, _standaloneBlockSize: 0);
			prefabDecorator.AddPrefab(pi, _isPOI: true);
			prefabDecorator.CopyAllPrefabsIntoWorld(world);
			int num5 = -prefab.size.z / 2 - 11;
			world.SetBlock(0, new Vector3i(2, world.GetHeight(0, num5) + 1, num5), Block.GetBlockValue("cntQuestTestLoot"), bNotify: false, updateLight: false);
			Vector3 vector = new Vector3(-2f, world.GetHeight(0, num5) + 2, num5);
			IChunk chunkFromWorldPos = world.GetChunkFromWorldPos(new Vector3i(vector));
			if (chunkFromWorldPos != null)
			{
				EntityCreationData entityCreationData = new EntityCreationData();
				entityCreationData.pos = vector;
				entityCreationData.rot = new Vector3(0f, 180f, 0f);
				entityCreationData.entityClass = EntityClass.FromString("npcTraderTest");
				((Chunk)chunkFromWorldPos).AddEntityStub(entityCreationData);
			}
			foreach (Chunk item in world.ChunkClusters[0].GetChunkArrayCopySync())
			{
				item.ResetStability();
				item.ResetLights(0);
				item.RefreshSunlight();
				item.NeedsLightCalculation = true;
			}
		}
		if (worldName == "Empty")
		{
			spawnPointManager.spawnPointList = new SpawnPointList
			{
				new SpawnPoint(new Vector3i(0, 10, 0))
			};
		}
		yield return null;
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

	public override bool GetOverviewMap(Vector2i _startPos, Vector2i _size, Color[] mapColors)
	{
		return false;
	}

	public override EnumChunkProviderId GetProviderId()
	{
		return EnumChunkProviderId.Disc;
	}

	public override void Cleanup()
	{
		Thread.Sleep(200);
		m_RegionFileManager.Cleanup();
	}

	public override void SaveAll()
	{
		if (world.IsEditor())
		{
			PathAbstractions.AbstractedLocation location = PathAbstractions.WorldsSearchPaths.GetLocation(worldName);
			if (location.Type == PathAbstractions.EAbstractedLocationType.None)
			{
				SdDirectory.CreateDirectory(GameIO.GetGameDir("Data/Worlds") + "/" + worldName);
				location = PathAbstractions.WorldsSearchPaths.GetLocation(worldName);
			}
			prefabDecorator.Save(location.FullPath);
			prefabDecorator.CleanAllPrefabsFromWorld(world);
			m_RegionFileManager.MakePersistent(cc, _bSaveEvenIfUnchanged: true);
			m_RegionFileManager.WaitSaveDone();
			prefabDecorator.CopyAllPrefabsIntoWorld(world);
			spawnPointManager.Save(location.FullPath);
		}
		else
		{
			m_RegionFileManager.MakePersistent(world.ChunkCache, _bSaveEvenIfUnchanged: false);
			m_RegionFileManager.WaitSaveDone();
		}
	}

	public override void SaveRandomChunks(int count, ulong _curWorldTimeInTicks, ArraySegment<long> _activeChunkSet)
	{
	}

	public override bool GetWorldExtent(out Vector3i _minSize, out Vector3i _maxSize)
	{
		_minSize = new Vector3i(world.ChunkCache.ChunkMinPos.x * 16, 0, world.ChunkCache.ChunkMinPos.y * 16);
		_maxSize = new Vector3i(world.ChunkCache.ChunkMaxPos.x * 16, 255, world.ChunkCache.ChunkMaxPos.y * 16);
		return true;
	}

	public override void RebuildTerrain(HashSetLong _chunks, Vector3i _areaStart, Vector3i _areaSize, bool _bStopStabilityUpdate, bool _bRegenerateChunk, bool _bFillEmptyBlocks, bool _isReset)
	{
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
		foreach (long _chunk in _chunks)
		{
			Chunk chunkSync = cc.GetChunkSync(_chunk);
			if (chunkSync == null)
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
				if (!flag && world.m_ChunkManager.m_ObservedEntities[j].chunksLoaded.Contains(_chunk))
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(chunkSync, _bOverwriteExisting: true), _onlyClientsAttachedToAnEntity: false, world.m_ChunkManager.m_ObservedEntities[j].entityIdToSendChunksTo);
				}
			}
		}
	}

	public override ChunkProtectionLevel GetChunkProtectionLevel(Vector3i worldPos)
	{
		return m_RegionFileManager.GetChunkProtectionLevelForWorldPos(worldPos);
	}
}
