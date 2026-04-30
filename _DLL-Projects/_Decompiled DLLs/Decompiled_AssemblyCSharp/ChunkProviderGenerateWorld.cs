using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public abstract class ChunkProviderGenerateWorld : ChunkProviderAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public const int cCacheChunks = 0;

	[PublicizedFrom(EAccessModifier.Protected)]
	public World world;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string levelName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string gameName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public DynamicPrefabDecorator prefabDecorator;

	[PublicizedFrom(EAccessModifier.Protected)]
	public SpawnPointManager spawnPointManager;

	[PublicizedFrom(EAccessModifier.Protected)]
	public RegionFileManager m_RegionFileManager;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IBiomeProvider m_BiomeProvider;

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<ChunkProviderParameter> m_Parameters = new List<ChunkProviderParameter>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<IWorldDecorator> m_Decorators = new List<IWorldDecorator>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public ITerrainGenerator m_TerrainGenerator;

	public HashSetList<long> m_ChunkQueue = new HashSetList<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public AutoResetEvent m_WaitHandle = new AutoResetEvent(initialState: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo threadInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public Stopwatch chunkGenerationTimer = new Stopwatch();

	[PublicizedFrom(EAccessModifier.Private)]
	public long currentGeneratingChunk;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bClientMode;

	public ReadOnlyCollection<HashSetLong> ChunkGroups => m_RegionFileManager.ChunkGroups;

	public ChunkProviderGenerateWorld(ChunkCluster _cc, string _levelName, bool _bClientMode = false)
	{
		bClientMode = _bClientMode;
		levelName = _levelName;
		bDecorationsEnabled = true;
	}

	public override DynamicPrefabDecorator GetDynamicPrefabDecorator()
	{
		return prefabDecorator;
	}

	public override IEnumerator Init(World _world)
	{
		world = _world;
		PathAbstractions.AbstractedLocation worldLocation = PathAbstractions.WorldsSearchPaths.GetLocation(levelName);
		prefabDecorator = new DynamicPrefabDecorator();
		yield return prefabDecorator.Load(worldLocation.FullPath);
		yield return null;
		spawnPointManager = new SpawnPointManager();
		if (!bClientMode)
		{
			yield return null;
			spawnPointManager.Load(worldLocation.FullPath);
		}
		if (!bClientMode)
		{
			threadInfo = ThreadManager.StartThread("GenerateChunks", null, GenerateChunksThread, null, null, null, _useRealThread: true);
		}
		yield return null;
	}

	public override void SaveAll()
	{
		if (!bClientMode)
		{
			if (world.IsEditor())
			{
				PathAbstractions.AbstractedLocation location = PathAbstractions.WorldsSearchPaths.GetLocation(levelName);
				GetDynamicPrefabDecorator().Save(location.FullPath);
				spawnPointManager.Save(location.FullPath);
			}
			else if (m_RegionFileManager != null)
			{
				m_RegionFileManager.MakePersistent(world.ChunkCache, _bSaveEvenIfUnchanged: false);
				m_RegionFileManager.WaitSaveDone();
			}
		}
	}

	public override void SaveRandomChunks(int count, ulong _curWorldTimeInTicks, ArraySegment<long> _activeChunkSet)
	{
		if (bClientMode)
		{
			return;
		}
		GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
		for (int i = _activeChunkSet.Offset; i < _activeChunkSet.Count; i++)
		{
			Chunk chunkSync = world.ChunkCache.GetChunkSync(_activeChunkSet.Array[i]);
			if (chunkSync != null && chunkSync.NeedsSaving && !chunkSync.NeedsDecoration && !chunkSync.InProgressDecorating && !chunkSync.NeedsLightCalculation && !chunkSync.InProgressLighting && _curWorldTimeInTicks - chunkSync.SavedInWorldTicks > 400 && gameRandom.RandomFloat < 0.3f)
			{
				lock (chunkSync)
				{
					if (chunkSync.IsLocked)
					{
						continue;
					}
					chunkSync.InProgressSaving = true;
					goto IL_00c6;
				}
			}
			goto IL_00e1;
			IL_00c6:
			m_RegionFileManager.SaveChunkSnapshot(chunkSync, _saveIfUnchanged: false);
			count--;
			chunkSync.InProgressSaving = false;
			goto IL_00e1;
			IL_00e1:
			if (count <= 0)
			{
				break;
			}
		}
	}

	public override void ClearCaches()
	{
		if (!bClientMode)
		{
			m_RegionFileManager.ClearCaches();
		}
	}

	public HashSetLong ResetAllChunks(ChunkProtectionLevel excludedProtectionLevels)
	{
		return m_RegionFileManager.ResetAllChunks(excludedProtectionLevels);
	}

	public HashSetLong ResetRegion(int _regionX, int _regionZ, ChunkProtectionLevel excludedProtectionLevels)
	{
		return m_RegionFileManager.ResetRegion(_regionX, _regionZ, excludedProtectionLevels);
	}

	public void RequestChunkReset(long _chunkKey)
	{
		m_RegionFileManager.RequestChunkReset(_chunkKey);
	}

	public void MainThreadCacheProtectedPositions()
	{
		m_RegionFileManager.MainThreadCacheProtectedPositions();
	}

	public void SaveChunkAgeDebugTexture(float rangeInDays)
	{
		m_RegionFileManager.SaveChunkAgeDebugTexture(rangeInDays);
	}

	public void IterateChunkExpiryTimes(Action<long, ulong> action)
	{
		m_RegionFileManager.IterateChunkExpiryTimes(action);
	}

	public override void RequestChunk(int _x, int _y)
	{
		if (bClientMode)
		{
			return;
		}
		lock (((ICollection)m_ChunkQueue.list).SyncRoot)
		{
			long num = WorldChunkCache.MakeChunkKey(_x, _y);
			if (m_ChunkQueue.hashSet.Contains(num))
			{
				return;
			}
			m_ChunkQueue.Add(num);
		}
		m_WaitHandle.Set();
	}

	public override HashSetList<long> GetRequestedChunks()
	{
		return m_ChunkQueue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void generateTerrain(World _world, Chunk _chunk, GameRandom _random)
	{
		m_TerrainGenerator.GenerateTerrain(_world, _chunk, _random, Vector3i.zero, Vector3i.zero, _bFillEmptyBlocks: false, _isReset: false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void generateTerrain(World _world, Chunk _chunk, GameRandom _random, Vector3i _areaStart, Vector3i _areaSize, bool _bFillEmptyBlocks, bool _isReset)
	{
		m_TerrainGenerator.GenerateTerrain(_world, _chunk, _random, _areaStart, _areaSize, _bFillEmptyBlocks, _isReset);
	}

	public bool GenerateSingleChunk(ChunkCluster cc, long key, bool _forceRebuild = false)
	{
		currentGeneratingChunk = key;
		chunkGenerationTimer.Restart();
		if (!_forceRebuild && cc.ContainsChunkSync(key))
		{
			currentGeneratingChunk = 0L;
			return false;
		}
		Chunk chunk = null;
		if (m_RegionFileManager.ContainsChunkSync(key))
		{
			chunk = m_RegionFileManager.GetChunkSync(key);
			m_RegionFileManager.RemoveChunkSync(key);
		}
		if (_forceRebuild)
		{
			chunk = cc.GetChunkSync(key);
			if (chunk != null)
			{
				chunk.RemoveBlockEntityTransforms();
				chunk.Reset();
			}
		}
		if (_forceRebuild || chunk == null)
		{
			int x = WorldChunkCache.extractX(key);
			int num = WorldChunkCache.extractZ(key);
			if (chunk == null)
			{
				chunk = MemoryPools.PoolChunks.AllocSync(_bReset: true);
			}
			if (chunk != null)
			{
				chunk.X = x;
				chunk.Z = num;
				GameRandom gameRandom = Utils.RandomFromSeedOnPos(x, num, world.Seed);
				generateTerrain(world, chunk, gameRandom);
				GameRandomManager.Instance.FreeGameRandom(gameRandom);
				if (!bDecorationsEnabled)
				{
					chunk.NeedsDecoration = false;
					chunk.NeedsLightCalculation = false;
					chunk.NeedsRegeneration = true;
				}
				if (bDecorationsEnabled)
				{
					chunk.NeedsDecoration = true;
					chunk.NeedsLightCalculation = true;
					if (GetDynamicPrefabDecorator() != null)
					{
						GetDynamicPrefabDecorator().DecorateChunk(world, chunk);
					}
				}
			}
		}
		bool flag = false;
		if (chunk != null)
		{
			if (!_forceRebuild)
			{
				flag = cc.AddChunkSync(chunk);
			}
			else
			{
				ReaderWriterLockSlim syncRoot = cc.GetSyncRoot();
				syncRoot.EnterUpgradeableReadLock();
				if (cc.ContainsChunkSync(key))
				{
					cc.RemoveChunkSync(key);
				}
				flag = cc.AddChunkSync(chunk);
				syncRoot.ExitUpgradeableReadLock();
			}
			if (flag)
			{
				if (!chunk.NeedsDecoration)
				{
					OnChunkSyncedAndDecorated(chunk);
				}
				updateDecorationsWherePossible(chunk);
				if (_forceRebuild)
				{
					chunk.isModified = true;
				}
			}
			else
			{
				MemoryPools.PoolChunks.FreeSync(chunk);
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OnChunkSyncedAndDecorated(Chunk chunk)
	{
		WaterSimulationNative.Instance.InitializeChunk(chunk);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GenerateChunksThread(ThreadManager.ThreadInfo _threadInfo)
	{
		if (!_threadInfo.TerminationRequested())
		{
			if (m_RegionFileManager == null)
			{
				return 15;
			}
			long num = world.GetNextChunkToProvide();
			if (num == long.MaxValue)
			{
				num = DynamicMeshThread.GetNextChunkToLoad();
				if (num == long.MaxValue)
				{
					return 15;
				}
			}
			ChunkCluster chunkCache = world.ChunkCache;
			GenerateSingleChunk(chunkCache, num);
			return 0;
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void tryToDecorate(Chunk _chunk)
	{
		if (_chunk != null && _chunk.NeedsDecoration && !_chunk.IsLocked)
		{
			decorate(_chunk);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void decorate(Chunk _chunk)
	{
		int x = _chunk.X;
		int z = _chunk.Z;
		Chunk chunk;
		Chunk chunk2;
		Chunk chunk3;
		if ((chunk = (Chunk)world.GetChunkSync(x + 1, z + 1)) != null && (chunk2 = (Chunk)world.GetChunkSync(x, z + 1)) != null && (chunk3 = (Chunk)world.GetChunkSync(x + 1, z)) != null)
		{
			chunk.InProgressDecorating = true;
			chunk2.InProgressDecorating = true;
			chunk3.InProgressDecorating = true;
			_chunk.InProgressDecorating = true;
			updateDecosAllowedForChunk(_chunk, chunk3, chunk2);
			for (int i = 0; i < m_Decorators.Count; i++)
			{
				m_Decorators[i].DecorateChunkOverlapping(world, _chunk, chunk3, chunk2, chunk, world.Seed);
			}
			_chunk.OnDecorated();
			_chunk.ResetStability();
			_chunk.RefreshSunlight();
			_chunk.NeedsDecoration = false;
			_chunk.NeedsLightCalculation = true;
			chunk.InProgressDecorating = false;
			chunk2.InProgressDecorating = false;
			chunk3.InProgressDecorating = false;
			_chunk.InProgressDecorating = false;
			OnChunkSyncedAndDecorated(_chunk);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDecosAllowedForChunk(Chunk _chunk, Chunk _c10, Chunk _c01)
	{
		Vector3 lhs = new Vector3(0f, 0f, 1f);
		Vector3 rhs = new Vector3(1f, 0f, 0f);
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				int terrainHeight = _chunk.GetTerrainHeight(j, i);
				int num = ((j < 15) ? _chunk.GetTerrainHeight(j + 1, i) : _c10.GetTerrainHeight(0, i));
				int num2 = ((i < 15) ? _chunk.GetTerrainHeight(j, i + 1) : _c01.GetTerrainHeight(j, 0));
				if (terrainHeight >= 253 || num >= 253 || num2 >= 253)
				{
					_chunk.SetDecoAllowedAt(j, i, EnumDecoAllowed.Nothing);
					continue;
				}
				float num3 = (float)_chunk.GetDensity(j, terrainHeight, i) / -128f;
				float num4 = ((j < 15) ? ((float)_chunk.GetDensity(j + 1, num, i) / -128f) : ((float)_c10.GetDensity(0, num, i) / -128f));
				float num5 = ((i < 15) ? ((float)_chunk.GetDensity(j, num2, i + 1) / -128f) : ((float)_c01.GetDensity(j, num2, 0) / -128f));
				float num6 = (float)_chunk.GetDensity(j, terrainHeight + 1, i) / 127f;
				float num7 = ((j < 15) ? ((float)_chunk.GetDensity(j + 1, num + 1, i) / 127f) : ((float)_c10.GetDensity(0, num + 1, i) / 127f));
				float num8 = ((i < 15) ? ((float)_chunk.GetDensity(j, num2 + 1, i + 1) / 127f) : ((float)_c01.GetDensity(j, num2 + 1, 0) / 127f));
				if (num3 > 0.999f && num6 > 0.999f)
				{
					num3 = 0.5f;
				}
				if (num4 > 0.999f && num7 > 0.999f)
				{
					num4 = 0.5f;
				}
				if (num5 > 0.999f && num8 > 0.999f)
				{
					num5 = 0.5f;
				}
				float num9 = (float)terrainHeight + num3;
				float num10 = (float)num + num4;
				float num11 = (float)num2 + num5;
				lhs.y = num11 - num9;
				rhs.y = num10 - num9;
				Vector3 normalized = Vector3.Cross(lhs, rhs).normalized;
				_chunk.SetTerrainNormal(j, i, normalized);
				if (normalized.y < 0.55f)
				{
					_chunk.SetDecoAllowedSlopeAt(j, i, EnumDecoAllowedSlope.Steep);
				}
				else if (normalized.y < 0.65f)
				{
					_chunk.SetDecoAllowedSlopeAt(j, i, EnumDecoAllowedSlope.Sloped);
				}
				if (terrainHeight <= 1 || terrainHeight >= 255 || _chunk.IsWater(j, terrainHeight, i) || _chunk.IsWater(j, terrainHeight + 1, i))
				{
					_chunk.SetDecoAllowedAt(j, i, EnumDecoAllowed.Nothing);
				}
			}
		}
	}

	public void UpdateDecorations(Chunk _chunk)
	{
		decorate(_chunk);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDecorationsWherePossible(Chunk chunk)
	{
		World world = this.world;
		int x = chunk.X;
		int z = chunk.Z;
		tryToDecorate(chunk);
		tryToDecorate((Chunk)world.GetChunkSync(x - 1, z));
		tryToDecorate((Chunk)world.GetChunkSync(x, z - 1));
		tryToDecorate((Chunk)world.GetChunkSync(x - 1, z - 1));
	}

	public override void UnloadChunk(Chunk _chunk)
	{
		if (bClientMode)
		{
			MemoryPools.PoolChunks.FreeSync(_chunk);
		}
		else
		{
			m_RegionFileManager.AddChunkSync(_chunk);
		}
	}

	public override void ReloadAllChunks()
	{
		lock (((ICollection)m_ChunkQueue.list).SyncRoot)
		{
			m_ChunkQueue.Clear();
		}
		m_RegionFileManager.Clear();
	}

	public List<ChunkProviderParameter> GetParameters()
	{
		return m_Parameters;
	}

	public override void Update()
	{
		m_RegionFileManager?.Update();
	}

	public override void StopUpdate()
	{
		if (threadInfo != null)
		{
			threadInfo.WaitForEnd();
			threadInfo = null;
		}
	}

	public override void Cleanup()
	{
		StopUpdate();
		if (spawnPointManager != null)
		{
			spawnPointManager.Cleanup();
		}
		if (m_RegionFileManager != null)
		{
			m_RegionFileManager.Cleanup();
		}
		MultiBlockManager.Instance.Cleanup();
	}

	public override bool GetOverviewMap(Vector2i _startPos, Vector2i _size, Color[] mapColors)
	{
		m_RegionFileManager.SetCacheSize(1000);
		new TerrainMapGenerator().GenerateTerrain(this);
		m_RegionFileManager.SetCacheSize(0);
		return true;
	}

	public override IBiomeProvider GetBiomeProvider()
	{
		return m_BiomeProvider;
	}

	public override ITerrainGenerator GetTerrainGenerator()
	{
		return m_TerrainGenerator;
	}

	public override SpawnPointList GetSpawnPointList()
	{
		return spawnPointManager.spawnPointList;
	}

	public override void SetSpawnPointList(SpawnPointList _spawnPointList)
	{
		spawnPointManager.spawnPointList = _spawnPointList;
	}

	public override void RebuildTerrain(HashSetLong _chunks, Vector3i _areaStart, Vector3i _areaSize, bool _isStopStabilityCalc, bool _isRegenChunk, bool _isFillEmptyBlocks, bool _isReset)
	{
		ChunkCluster chunkCluster = world.ChunkClusters[0];
		foreach (long _chunk in _chunks)
		{
			Chunk chunkSync = chunkCluster.GetChunkSync(_chunk);
			if (chunkSync != null)
			{
				GameRandom gameRandom = Utils.RandomFromSeedOnPos(chunkSync.X, chunkSync.Z, world.Seed);
				chunkSync.StopStabilityCalculation = _isStopStabilityCalc;
				generateTerrain(world, chunkSync, gameRandom, _areaStart, _areaSize, _isFillEmptyBlocks, _isReset);
				GameRandomManager.Instance.FreeGameRandom(gameRandom);
				if (_isRegenChunk)
				{
					chunkSync.NeedsRegeneration = true;
				}
			}
		}
	}

	public void RemoveChunks(HashSetLong _chunks)
	{
		if (m_RegionFileManager != null)
		{
			m_RegionFileManager.RemoveChunks(_chunks);
		}
	}

	public override ChunkProtectionLevel GetChunkProtectionLevel(Vector3i worldPos)
	{
		return m_RegionFileManager.GetChunkProtectionLevelForWorldPos(worldPos);
	}

	public void CheckPersistentData()
	{
		m_RegionFileManager.CheckPersistentData();
	}

	public void LogCurrentChunkGeneration()
	{
		Log.Error($"ChunkProvider Current Generating Chunk - Key: {currentGeneratingChunk}, Pos: {WorldChunkCache.extractX(currentGeneratingChunk) << 4}/{WorldChunkCache.extractZ(currentGeneratingChunk) << 4}, Duration {chunkGenerationTimer.Elapsed.TotalSeconds}");
	}
}
