using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class ChunkManager : IChunkProviderIndicator
{
	public class ChunkObserver
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static int idCnt;

		public int id;

		public Vector3 position;

		public Vector3i curChunkPos;

		public bool bBuildVisualMeshAround;

		public int viewDim;

		public HashSetLong chunksLoaded;

		public BucketHashSetList chunksToLoad;

		public List<long> chunksToReload;

		public HashSetLong chunksToRemove;

		public BucketHashSetList chunksAround;

		public int entityIdToSendChunksTo;

		public IMapChunkDatabase mapDatabase;

		public ChunkObserver(Vector3 _initialPosition, bool _bBuildVisualMeshAround, int _viewDim, int _entityIdToSendChunksTo)
		{
			id = ++idCnt;
			entityIdToSendChunksTo = _entityIdToSendChunksTo;
			position = _initialPosition;
			curChunkPos = new Vector3i(int.MaxValue, 0, int.MaxValue);
			bBuildVisualMeshAround = _bBuildVisualMeshAround;
			viewDim = _viewDim;
			chunksLoaded = new HashSetLong();
			chunksToLoad = new BucketHashSetList(_viewDim + 2);
			chunksToReload = new List<long>();
			chunksToRemove = new HashSetLong();
			chunksAround = new BucketHashSetList(_viewDim + 2);
		}

		public void SetPosition(Vector3 _position)
		{
			position = _position;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class ThreadRegeneratingData
	{
		public List<long> viewingChunkPositionsCopy = new List<long>();

		public List<long> collisionChunkPositionsCopy = new List<long>();
	}

	public class BakeCollider
	{
		public Mesh mesh;

		public int id;

		public MeshCollider meshCollider;

		public bool isBaked;

		public bool isCancelledDestroy;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cReloadPosY = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxChunksSupported = 100000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxChunksAroundPlayers = 15;

	public static bool GenerateCollidersOnlyAroundEntites = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public World m_World;

	[PublicizedFrom(EAccessModifier.Private)]
	public object lockObject = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunksToCopyIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public ArraySegment<long> m_ChunksToCopy;

	[PublicizedFrom(EAccessModifier.Private)]
	public long[] chunksToCopyArr = new long[100000];

	[PublicizedFrom(EAccessModifier.Private)]
	public ArraySegment<long> m_ChunksToFree;

	[PublicizedFrom(EAccessModifier.Private)]
	public long[] chunksToFreeArr = new long[100000];

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunksToLightIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public ArraySegment<long> m_ChunksToLight;

	[PublicizedFrom(EAccessModifier.Private)]
	public long[] chunksToLightArr = new long[100000];

	[PublicizedFrom(EAccessModifier.Private)]
	public long[] allChunkPositionsCopy = new long[100000];

	[PublicizedFrom(EAccessModifier.Private)]
	public BucketHashSetList m_ViewingChunkPositions = new BucketHashSetList(15);

	[PublicizedFrom(EAccessModifier.Private)]
	public BucketHashSetList m_AllChunkPositions = new BucketHashSetList(15);

	[PublicizedFrom(EAccessModifier.Private)]
	public BucketHashSetList m_CollisionChunkPositions = new BucketHashSetList(15);

	[PublicizedFrom(EAccessModifier.Private)]
	public long[] activeChunkSetArr = new long[100000];

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ChunkGameObject> m_FreeChunkGameObjects = new List<ChunkGameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ChunkGameObject> m_UsedChunkGameObjects = new List<ChunkGameObject>();

	public List<ChunkObserver> m_ObservedEntities = new List<ChunkObserver>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo threadInfoRegenerating;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo threadInfoCalc;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetList<Chunk> chunksToUnload = new HashSetList<Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector2i>[] rectanglesAroundPlayers;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<NetPackage> sendToClientPackages = new List<NetPackage>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static ConcurrentDictionary<long, DateTime> chunkGenerationTimestamps = new ConcurrentDictionary<long, DateTime>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static DateTime lastVmlExhaustionLog = DateTime.MinValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public static TimeSpan vmlExhaustionLogInterval = TimeSpan.FromSeconds(30.0);

	[PublicizedFrom(EAccessModifier.Private)]
	public static TimeSpan MaxVmlLogInterval = TimeSpan.FromMinutes(10.0);

	[PublicizedFrom(EAccessModifier.Private)]
	public static DateTime vmlExhaustionStartTime = DateTime.MinValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double MinLogThresholdSeconds = 1.0;

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk currentCopiedChunk;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentClusterIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkGameObject currentCopiedChunkGameObject;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isContinueCopying;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentCopiedChunkLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInternalForceUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isChunkClusterChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public AutoResetEvent calcThreadWaitHandle = new AutoResetEvent(initialState: false);

	public static int MaxQueuedMeshLayers = 1000;

	public Action<Chunk> OnChunkInitialized;

	public Action<Chunk> OnChunkRegenerated;

	public Action<Chunk> OnChunkCopiedToUnity;

	public List<Chunk> ChunksToCopyInOneFrame = new List<Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunksToCopyInOneFrameIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunksToCopyInOneFramePass;

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk[] neighborsLightingThread = new Chunk[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool bLightingDone = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool bCalcPositionsDone = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool isViewingOrCollisionPositionsChanged_threadCalc;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> viewingChunkPositionsCopy = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> collisionChunkPositionsCopy = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> lightingChunkPositionsCopy = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> chunksToCopyTemp = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> chunksToFreeTemp = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> chunksToLightTemp = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk[] neighborsGenerationThread2 = new Chunk[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool isViewingOrCollisionPositionsChanged_threadReg = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk[] regenerateNextChunkNeighbors = new Chunk[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxCGOsToUnloadPerFrame = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ChunkGameObject> tempDisplayedCGOs = new List<ChunkGameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> cgoToRemove = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlockEntityData>[] groundAlignBlockLists = new List<BlockEntityData>[2]
	{
		new List<BlockEntityData>(),
		new List<BlockEntityData>()
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public int groundAlignIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo bakeThreadInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public AutoResetEvent bakeEvent = new AutoResetEvent(initialState: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator bakeCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BakeCollider> bakes = new List<BakeCollider>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int bakeIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile BakeCollider bakeCurrent;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, MicroStopwatch> debugTimers = new Dictionary<string, MicroStopwatch>();

	public void Init(World _world)
	{
		m_World = _world;
		rectanglesAroundPlayers = new List<Vector2i>[15];
		for (int i = 0; i < 15; i++)
		{
			rectanglesAroundPlayers[i] = new List<Vector2i>();
			for (int j = -i; j <= i; j++)
			{
				for (int k = -i; k <= i; k++)
				{
					if (j == -i || j == i || k == -i || k == i)
					{
						rectanglesAroundPlayers[i].Add(new Vector2i(j, k));
					}
				}
			}
		}
		GamePrefs.OnGamePrefChanged += OnGamePrefChanged;
		MaxQueuedMeshLayers = GamePrefs.GetInt(EnumGamePrefs.MaxQueuedMeshLayers);
		threadInfoRegenerating = ThreadManager.StartThread("ChunkRegeneration", thread_RegeneratingInit, thread_Regenerating, null, null, null, _useRealThread: true);
		threadInfoCalc = ThreadManager.StartThread("ChunkCalc", null, thread_Calc, null, null, null, _useRealThread: true);
		BakeInit();
	}

	public void Cleanup()
	{
		threadInfoRegenerating.WaitForEnd();
		threadInfoRegenerating = null;
		calcThreadWaitHandle.Set();
		threadInfoCalc.WaitForEnd();
		threadInfoCalc = null;
		GroundAlignCleanup();
		BakeCleanup();
		for (int num = m_ObservedEntities.Count - 1; num >= 0; num--)
		{
			RemoveChunkObserver(m_ObservedEntities[num]);
		}
		FreePools();
		m_UsedChunkGameObjects.Clear();
		chunkGenerationTimestamps.Clear();
	}

	public void FreePools()
	{
		for (int i = 0; i < m_FreeChunkGameObjects.Count; i++)
		{
			ChunkGameObject chunkGameObject = m_FreeChunkGameObjects[i];
			chunkGameObject.Cleanup();
			UnityEngine.Object.Destroy(chunkGameObject.gameObject);
		}
		m_FreeChunkGameObjects.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGamePrefChanged(EnumGamePrefs _pref)
	{
		if (_pref == EnumGamePrefs.MaxQueuedMeshLayers)
		{
			MaxQueuedMeshLayers = GamePrefs.GetInt(_pref);
		}
	}

	public void OriginChanged(Vector3 _offset)
	{
		for (int i = 0; i < m_UsedChunkGameObjects.Count; i++)
		{
			GameObject gameObject = m_UsedChunkGameObjects[i].gameObject;
			if ((bool)gameObject)
			{
				gameObject.transform.position += _offset;
			}
		}
	}

	public ChunkObserver AddChunkObserver(Vector3 _initialPosition, bool _bBuildVisualMeshAround, int _viewDim, int _entityIdToSendChunksTo)
	{
		ChunkObserver chunkObserver = new ChunkObserver(_initialPosition, _bBuildVisualMeshAround, _viewDim, _entityIdToSendChunksTo);
		m_ObservedEntities.Add(chunkObserver);
		isInternalForceUpdate = true;
		return chunkObserver;
	}

	public void RemoveChunkObserver(ChunkObserver _chunkObserver)
	{
		for (int i = 0; i < m_ObservedEntities.Count; i++)
		{
			if (m_ObservedEntities[i].id == _chunkObserver.id)
			{
				m_ObservedEntities.RemoveAt(i);
				isInternalForceUpdate = true;
				break;
			}
		}
	}

	public void SendChunksToClients()
	{
		ChunkCluster chunkCache = m_World.ChunkCache;
		sendToClientPackages.Clear();
		for (int i = 0; i < m_ObservedEntities.Count; i++)
		{
			ChunkObserver chunkObserver = m_ObservedEntities[i];
			if (chunkObserver.entityIdToSendChunksTo == -1)
			{
				continue;
			}
			foreach (long item in chunkObserver.chunksToRemove)
			{
				sendToClientPackages.Add(NetPackageManager.GetPackage<NetPackageChunkRemove>().Setup(item));
				chunkObserver.chunksLoaded.Remove(item);
				chunkObserver.chunksToReload.Remove(item);
			}
			chunkObserver.chunksToRemove.Clear();
			if (sendToClientPackages.Count > 0)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(sendToClientPackages, _onlyClientsAttachedToAnEntity: false, chunkObserver.entityIdToSendChunksTo);
				sendToClientPackages.Clear();
			}
			int num = 0;
			while (num < chunkObserver.chunksToLoad.list.Count)
			{
				long num2 = chunkObserver.chunksToLoad.list[num];
				Chunk chunkSync;
				if (chunkCache != null && (chunkSync = chunkCache.GetChunkSync(num2)) != null && !chunkSync.NeedsLightCalculation)
				{
					sendToClientPackages.Add(NetPackageManager.GetPackage<NetPackageChunk>().Setup(chunkSync));
					chunkObserver.chunksLoaded.Add(num2);
					chunkObserver.chunksToLoad.Remove(num2);
				}
				else
				{
					num++;
				}
				if (sendToClientPackages.Count >= 3)
				{
					break;
				}
			}
			for (int num3 = chunkObserver.chunksToReload.Count - 1; num3 >= 0; num3--)
			{
				long key = chunkObserver.chunksToReload[num3];
				if (chunkCache != null)
				{
					Chunk chunkSync2 = chunkCache.GetChunkSync(key);
					if (chunkSync2 != null && !chunkSync2.NeedsLightCalculation)
					{
						sendToClientPackages.Add(NetPackageManager.GetPackage<NetPackageChunk>().Setup(chunkSync2, _bOverwriteExisting: true));
						chunkObserver.chunksToReload.RemoveAt(num3);
					}
				}
			}
			if (chunkObserver.mapDatabase != null)
			{
				NetPackage mapChunkPackagesToSend = chunkObserver.mapDatabase.GetMapChunkPackagesToSend();
				if (mapChunkPackagesToSend != null)
				{
					sendToClientPackages.Add(mapChunkPackagesToSend);
				}
			}
			if (sendToClientPackages.Count > 0)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(sendToClientPackages, _onlyClientsAttachedToAnEntity: false, chunkObserver.entityIdToSendChunksTo);
				sendToClientPackages.Clear();
			}
		}
	}

	public void ResendChunksToClients(HashSetLong _chunks)
	{
		List<EntityPlayerLocal> localPlayers = m_World.GetLocalPlayers();
		for (int i = 0; i < m_ObservedEntities.Count; i++)
		{
			ChunkObserver chunkObserver = m_ObservedEntities[i];
			if (chunkObserver.bBuildVisualMeshAround)
			{
				continue;
			}
			bool flag = false;
			int num = 0;
			while (!flag && num < localPlayers.Count)
			{
				if (chunkObserver.entityIdToSendChunksTo == localPlayers[num].entityId)
				{
					flag = true;
				}
				num++;
			}
			if (!flag)
			{
				chunkObserver.chunksToReload.AddRange(_chunks);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk FindNextChunkToCopy()
	{
		if (ChunksToCopyInOneFrame.Count > 0)
		{
			if (chunksToCopyInOneFramePass == 0)
			{
				chunksToCopyInOneFramePass = 1;
				for (int i = 0; i < ChunksToCopyInOneFrame.Count; i++)
				{
					if (!ChunksToCopyInOneFrame[i].NeedsCopying)
					{
						chunksToCopyInOneFramePass = 0;
						break;
					}
				}
			}
			if (chunksToCopyInOneFramePass > 0)
			{
				if (chunksToCopyInOneFrameIndex < ChunksToCopyInOneFrame.Count)
				{
					Chunk chunk = ChunksToCopyInOneFrame[chunksToCopyInOneFrameIndex++];
					if (chunk != null)
					{
						currentClusterIndex = chunk.ClrIdx;
						chunk.InProgressCopying = true;
					}
					return chunk;
				}
				chunksToCopyInOneFramePass = 0;
				ChunksToCopyInOneFrame.Clear();
			}
		}
		bool flag = false;
		long num;
		do
		{
			lock (chunksToCopyArr)
			{
				if (chunksToCopyIdx >= m_ChunksToCopy.Count)
				{
					return null;
				}
				num = m_ChunksToCopy.Array[chunksToCopyIdx++];
			}
			for (int j = 0; j < ChunksToCopyInOneFrame.Count; j++)
			{
				if (num == ChunksToCopyInOneFrame[j].Key)
				{
					flag = true;
					break;
				}
			}
		}
		while (flag);
		currentClusterIndex = WorldChunkCache.extractClrIdx(num);
		ChunkCluster chunkCluster = m_World.ChunkClusters[currentClusterIndex];
		if (chunkCluster == null)
		{
			return null;
		}
		Chunk chunkSync = chunkCluster.GetChunkSync(num);
		if (chunkSync == null)
		{
			return null;
		}
		chunkSync.EnterWriteLock();
		if (chunkSync.IsLocked)
		{
			chunkSync.ExitWriteLock();
			return null;
		}
		chunkSync.InProgressCopying = true;
		chunkSync.ExitWriteLock();
		return chunkSync;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void freeChunkGameObjects()
	{
		lock (chunksToFreeArr)
		{
			for (int i = 0; i < m_ChunksToFree.Count; i++)
			{
				long key = m_ChunksToFree.Array[i];
				ChunkCluster chunkCluster = m_World.ChunkClusters[WorldChunkCache.extractClrIdx(key)];
				if (chunkCluster == null)
				{
					Log.Warning("freeChunkGameObjects: cluster not found " + WorldChunkCache.extractClrIdx(key));
					continue;
				}
				Chunk chunkSync = chunkCluster.GetChunkSync(key);
				if (chunkSync != null && !chunkSync.hasEntities && chunkSync.IsDisplayed)
				{
					FreeChunkGameObject(chunkCluster, chunkSync);
					chunkSync.IsCollisionMeshGenerated = false;
					chunkSync.NeedsRegeneration = true;
				}
			}
			m_ChunksToFree = default(ArraySegment<long>);
		}
	}

	public void ResetChunksToCopyInOneFrame()
	{
		ChunksToCopyInOneFrame.Clear();
		chunksToCopyInOneFrameIndex = 0;
		chunksToCopyInOneFramePass = 0;
	}

	public bool CopyChunksToUnity()
	{
		bool result;
		do
		{
			result = doCopyChunksToUnity();
		}
		while (ChunksToCopyInOneFrame.Count > 0 && currentCopiedChunk != null);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool doCopyChunksToUnity()
	{
		if (m_ChunksToFree.Count > 0)
		{
			freeChunkGameObjects();
		}
		if (!isContinueCopying && currentCopiedChunk != null)
		{
			currentCopiedChunkGameObject.EndCopyMeshLayer();
			if (!currentCopiedChunk.NeedsCopying)
			{
				if (currentCopiedChunk.displayState == Chunk.DisplayState.Start)
				{
					currentCopiedChunk.InProgressCopying = false;
					currentCopiedChunk.IsCollisionMeshGenerated = true;
					ChunkCluster chunkCache = m_World.ChunkCache;
					if (chunkCache != null)
					{
						chunkCache.OnChunkDisplayed(currentCopiedChunk.Key, _isDisplayed: true);
						currentCopiedChunk.OnDisplay(m_World, currentCopiedChunkGameObject.blockEntitiesParentT, chunkCache);
					}
				}
				if (currentCopiedChunk.displayState == Chunk.DisplayState.BlockEntities)
				{
					ChunkCluster chunkCache2 = m_World.ChunkCache;
					if (chunkCache2 != null)
					{
						currentCopiedChunk.OnDisplayBlockEntities(m_World, currentCopiedChunkGameObject.blockEntitiesParentT, chunkCache2);
					}
				}
				if (currentCopiedChunk.displayState != Chunk.DisplayState.Done)
				{
					return true;
				}
				OnChunkCopiedToUnity?.Invoke(currentCopiedChunk);
				currentCopiedChunk = null;
				if (ChunksToCopyInOneFrame.Count == 0)
				{
					return true;
				}
			}
		}
		if (currentCopiedChunk == null)
		{
			isContinueCopying = false;
			currentCopiedChunk = FindNextChunkToCopy();
			if (currentCopiedChunk == null)
			{
				return chunksToCopyIdx < m_ChunksToCopy.Count - 1;
			}
			ChunkCluster chunkCluster = m_World.ChunkClusters[currentClusterIndex];
			if (chunkCluster == null)
			{
				return false;
			}
			currentCopiedChunk.displayState = Chunk.DisplayState.Start;
			long key = currentCopiedChunk.Key;
			if (!chunkCluster.DisplayedChunkGameObjects.TryGetValue(key, out currentCopiedChunkGameObject))
			{
				currentCopiedChunkGameObject = GetNextFreeChunkGameObject();
				currentCopiedChunkGameObject.SetChunk(currentCopiedChunk, chunkCluster);
				currentCopiedChunkGameObject.gameObject.SetActive(value: true);
				chunkCluster.SetDisplayedChunkGameObject(key, currentCopiedChunkGameObject);
			}
			else if (currentCopiedChunkGameObject.chunk != currentCopiedChunk)
			{
				Log.Warning("currentCopiedChunk {0} wrong chunk on obj!", currentCopiedChunk);
				currentCopiedChunkGameObject.SetChunk(currentCopiedChunk, chunkCluster);
			}
		}
		if (!isContinueCopying)
		{
			currentCopiedChunkLayer = currentCopiedChunkGameObject.StartCopyMeshLayer();
			if (currentCopiedChunkLayer < 0)
			{
				return true;
			}
		}
		int _triangles;
		int _colliderTriangles;
		if (chunksToCopyInOneFramePass > 0)
		{
			currentCopiedChunkGameObject.CreateMeshAll(out _triangles, out _colliderTriangles);
			isContinueCopying = false;
		}
		else
		{
			isContinueCopying = currentCopiedChunkGameObject.CreateFromChunkNext(out var _, out var _, out _triangles, out _colliderTriangles);
		}
		return isContinueCopying;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void task_Lighting(ThreadManager.TaskInfo _taskInfo)
	{
		Chunk chunk = null;
		for (int i = 0; i < m_World.Players.list.Count * 2; i++)
		{
			chunksToLightIdx = 0;
			while (true)
			{
				long num;
				lock (chunksToLightArr)
				{
					if (chunksToLightIdx >= m_ChunksToLight.Count)
					{
						chunk = null;
						break;
					}
					num = m_ChunksToLight.Array[chunksToLightIdx++];
					if (num == long.MaxValue)
					{
						continue;
					}
					chunksToLightArr[chunksToLightIdx - 1] = long.MaxValue;
					goto IL_0093;
				}
				IL_0093:
				int idx = WorldChunkCache.extractClrIdx(num);
				ChunkCluster chunkCluster = m_World.ChunkClusters[idx];
				if (chunkCluster == null)
				{
					continue;
				}
				int x = WorldChunkCache.extractX(num);
				int y = WorldChunkCache.extractZ(num);
				chunk = chunkCluster.GetChunkSync(x, y);
				if (chunk == null || chunk.IsLocked)
				{
					continue;
				}
				if (chunk.NeedsLightDecoration && !chunk.IsLocked)
				{
					chunk.EnterWriteLock();
					if (chunk.IsLocked)
					{
						chunk.ExitWriteLock();
						continue;
					}
					chunk.InProgressDecorating = true;
					chunk.ExitWriteLock();
					if (chunkCluster.ChunkProvider is ChunkProviderGenerateWorld chunkProviderGenerateWorld)
					{
						chunkProviderGenerateWorld.UpdateDecorations(chunk);
						if (chunk.InProgressDecorating)
						{
							chunk.InProgressDecorating = false;
							continue;
						}
					}
					if (chunkCluster.ChunkProvider is ChunkProviderGenerateFlat)
					{
						WaterSimulationNative.Instance.InitializeChunk(chunk);
					}
					chunk.NeedsLightDecoration = false;
					chunk.NeedsDecoration = false;
					chunk.InProgressDecorating = false;
				}
				if (!chunk.NeedsLightCalculation || chunk.NeedsDecoration || chunk.IsLocked)
				{
					continue;
				}
				chunk.EnterWriteLock();
				if (chunk.IsLocked)
				{
					chunk.ExitWriteLock();
					continue;
				}
				chunk.InProgressLighting = true;
				chunk.ExitWriteLock();
				if (!chunkCluster.GetNeighborChunks(chunk, neighborsLightingThread))
				{
					chunk.InProgressLighting = false;
					continue;
				}
				if (!Chunk.IsNeighbourChunksDecorated(neighborsLightingThread))
				{
					chunk.InProgressLighting = false;
					continue;
				}
				if (!lockLightingInProgress(neighborsLightingThread))
				{
					chunk.InProgressLighting = false;
					continue;
				}
				chunkCluster.LightChunk(chunk, neighborsLightingThread);
				chunk.InProgressLighting = false;
				chunk.NeedsLightCalculation = false;
				chunk.NeedsRegeneration = true;
				for (int j = 0; j < neighborsLightingThread.Length; j++)
				{
					neighborsLightingThread[j].InProgressLighting = false;
				}
				OnChunkInitialized?.Invoke(chunk);
				break;
			}
			if (chunk == null)
			{
				break;
			}
		}
		Array.Clear(neighborsLightingThread, 0, neighborsLightingThread.Length);
		bLightingDone = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockLightingInProgress(Chunk[] chunks)
	{
		for (int i = 0; i < chunks.Length; i++)
		{
			Chunk chunk = chunks[i];
			chunk.EnterWriteLock();
			if (chunk.IsLocked)
			{
				for (int j = 0; j < i; j++)
				{
					chunks[j].InProgressLighting = false;
				}
				chunk.ExitWriteLock();
				return false;
			}
			chunk.InProgressLighting = true;
			chunk.ExitWriteLock();
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int thread_Calc(ThreadManager.ThreadInfo _threadInfo)
	{
		if (!_threadInfo.TerminationRequested())
		{
			calcThreadWaitHandle.WaitOne();
			task_CalcChunkPositions(_threadInfo);
			task_Lighting(null);
			return 5;
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void task_CalcChunkPositions(ThreadManager.ThreadInfo _threadInfo)
	{
		if (isViewingOrCollisionPositionsChanged_threadCalc)
		{
			viewingChunkPositionsCopy.Clear();
			collisionChunkPositionsCopy.Clear();
			lightingChunkPositionsCopy.Clear();
			lock (lockObject)
			{
				viewingChunkPositionsCopy.AddRange(m_ViewingChunkPositions.list);
				collisionChunkPositionsCopy.AddRange(m_CollisionChunkPositions.list);
				lightingChunkPositionsCopy.AddRange(m_AllChunkPositions.list);
			}
			isViewingOrCollisionPositionsChanged_threadCalc = false;
		}
		chunksToCopyTemp.Clear();
		chunksToFreeTemp.Clear();
		chunksToLightTemp.Clear();
		int num = Utils.FastMax(Utils.FastMax(viewingChunkPositionsCopy.Count, collisionChunkPositionsCopy.Count), lightingChunkPositionsCopy.Count);
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		for (int i = 0; i < num; i++)
		{
			if (chunksToCopyTemp.Count < 100 && num2 < viewingChunkPositionsCopy.Count)
			{
				long num6 = viewingChunkPositionsCopy[num2];
				if (checkChunkNeedsCopying(num6) != null)
				{
					chunksToCopyTemp.Add(num6);
				}
				num2++;
			}
			if (chunksToCopyTemp.Count < 100 && num3 < collisionChunkPositionsCopy.Count)
			{
				long num7 = collisionChunkPositionsCopy[num3];
				if (checkChunkNeedsCopying(num7) != null)
				{
					chunksToCopyTemp.Add(num7);
				}
				num3++;
			}
			if (chunksToLightTemp.Count < 100 && num5 < lightingChunkPositionsCopy.Count)
			{
				long num8 = lightingChunkPositionsCopy[num5];
				int idx = WorldChunkCache.extractClrIdx(num8);
				ChunkCluster chunkCluster = m_World.ChunkClusters[idx];
				if (chunkCluster != null)
				{
					Chunk chunkSync = chunkCluster.GetChunkSync(num8);
					if (chunkSync != null && chunkSync.NeedsLightCalculation && (!chunkSync.NeedsDecoration || chunkSync.NeedsLightDecoration) && !chunkSync.IsLocked && !chunkCluster.IsOnBorder(chunkSync))
					{
						chunksToLightTemp.Add(num8);
					}
				}
				num5++;
			}
			if (!GenerateCollidersOnlyAroundEntites || num4 >= collisionChunkPositionsCopy.Count)
			{
				continue;
			}
			ChunkCluster chunkCluster2 = m_World.ChunkClusters[0];
			if (chunkCluster2 != null)
			{
				long key = chunkCluster2.ToLocalKey(collisionChunkPositionsCopy[num4]);
				Chunk chunkSync2 = chunkCluster2.GetChunkSync(key);
				if (chunkSync2 != null && chunkSync2.IsCollisionMeshGenerated && chunkSync2.NeedsOnlyCollisionMesh)
				{
					if (!chunkSync2.hasEntities && chunkCluster2.GetNeighborChunks(chunkSync2, neighborsGenerationThread2) && !hasAnyChunkEntities(neighborsGenerationThread2))
					{
						chunksToFreeTemp.Add(chunkSync2.Key);
					}
					Array.Clear(neighborsGenerationThread2, 0, neighborsGenerationThread2.Length);
				}
			}
			num4++;
		}
		lock (chunksToCopyArr)
		{
			chunksToCopyIdx = 0;
			chunksToCopyTemp.CopyTo(chunksToCopyArr);
			m_ChunksToCopy = new ArraySegment<long>(chunksToCopyArr, 0, chunksToCopyTemp.Count);
		}
		lock (chunksToFreeArr)
		{
			chunksToFreeTemp.CopyTo(chunksToFreeArr);
			m_ChunksToFree = new ArraySegment<long>(chunksToFreeArr, 0, chunksToFreeTemp.Count);
		}
		lock (chunksToLightArr)
		{
			chunksToLightIdx = 0;
			chunksToLightTemp.CopyTo(chunksToLightArr);
			m_ChunksToLight = new ArraySegment<long>(chunksToLightArr, 0, chunksToLightTemp.Count);
		}
		bCalcPositionsDone = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void thread_RegeneratingInit(ThreadManager.ThreadInfo _threadInfo)
	{
		_threadInfo.threadData = new ThreadRegeneratingData();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int thread_Regenerating(ThreadManager.ThreadInfo _threadInfo)
	{
		if (!_threadInfo.TerminationRequested())
		{
			int poolSize = MemoryPools.poolVML.GetPoolSize();
			int instanceCount = VoxelMeshLayer.InstanceCount;
			int num = instanceCount - poolSize;
			if (num > MaxQueuedMeshLayers)
			{
				int num2 = num - MaxQueuedMeshLayers;
				DateTime utcNow = DateTime.UtcNow;
				if (vmlExhaustionStartTime == DateTime.MinValue)
				{
					vmlExhaustionStartTime = utcNow;
				}
				TimeSpan timeSpan = utcNow - vmlExhaustionStartTime;
				if (timeSpan.TotalSeconds >= 1.0 && utcNow - lastVmlExhaustionLog >= vmlExhaustionLogInterval)
				{
					Log.Warning($"ChunkManager generation thread blocked for {timeSpan.TotalSeconds:F1}s - VML max queued exceeded by {num2}, (Instance: {instanceCount}, Pooled: {poolSize}, MaxQueued: {MaxQueuedMeshLayers})");
					lastVmlExhaustionLog = utcNow;
					vmlExhaustionLogInterval = TimeSpan.FromMilliseconds(Math.Min(vmlExhaustionLogInterval.TotalMilliseconds * 2.0, MaxVmlLogInterval.TotalMilliseconds));
				}
				return 20;
			}
			if (vmlExhaustionStartTime != DateTime.MinValue)
			{
				TimeSpan timeSpan2 = DateTime.UtcNow - vmlExhaustionStartTime;
				if (timeSpan2.TotalSeconds >= 1.0)
				{
					Log.Out($"ChunkManager generation thread resumed after {timeSpan2.TotalSeconds:F1}s blocked");
				}
				vmlExhaustionStartTime = DateTime.MinValue;
				vmlExhaustionLogInterval = TimeSpan.FromSeconds(30.0);
			}
			ThreadRegeneratingData threadRegeneratingData = _threadInfo.threadData as ThreadRegeneratingData;
			if (isViewingOrCollisionPositionsChanged_threadReg)
			{
				isViewingOrCollisionPositionsChanged_threadReg = false;
				threadRegeneratingData.viewingChunkPositionsCopy.Clear();
				threadRegeneratingData.collisionChunkPositionsCopy.Clear();
				lock (lockObject)
				{
					threadRegeneratingData.viewingChunkPositionsCopy.AddRange(m_ViewingChunkPositions.list);
					threadRegeneratingData.collisionChunkPositionsCopy.AddRange(m_CollisionChunkPositions.list);
				}
			}
			RegenerateNextChunk(threadRegeneratingData.viewingChunkPositionsCopy, _isOnlyColliders: false);
			RegenerateNextChunk(threadRegeneratingData.collisionChunkPositionsCopy, _isOnlyColliders: true);
			return 5;
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk RegenerateNextChunk(List<long> _chunkPositions, bool _isOnlyColliders)
	{
		for (int i = 0; i < _chunkPositions.Count; i++)
		{
			long key = _chunkPositions[i];
			int idx = WorldChunkCache.extractClrIdx(key);
			ChunkCluster chunkCluster = m_World.ChunkClusters[idx];
			if (chunkCluster == null)
			{
				continue;
			}
			Chunk chunkSync = chunkCluster.GetChunkSync(key);
			if (chunkSync == null || chunkSync.IsLocked || (!chunkSync.NeedsRegeneration && (!chunkSync.NeedsOnlyCollisionMesh || _isOnlyColliders)) || chunkSync.NeedsLightCalculation || chunkSync.NeedsDecoration || chunkSync.IsLocked)
			{
				continue;
			}
			chunkSync.EnterWriteLock();
			if (chunkSync.IsLocked)
			{
				chunkSync.ExitWriteLock();
				continue;
			}
			chunkSync.InProgressRegeneration = true;
			chunkSync.ExitWriteLock();
			if (!chunkCluster.GetNeighborChunks(chunkSync, regenerateNextChunkNeighbors))
			{
				chunkSync.InProgressRegeneration = false;
				continue;
			}
			if (!Chunk.IsNeighbourChunksLit(regenerateNextChunkNeighbors))
			{
				chunkSync.InProgressRegeneration = false;
				continue;
			}
			if (GenerateCollidersOnlyAroundEntites && _isOnlyColliders && !chunkSync.hasEntities && !hasAnyChunkEntities(regenerateNextChunkNeighbors))
			{
				chunkSync.InProgressRegeneration = false;
				continue;
			}
			if (!lockGenerateInProgress(regenerateNextChunkNeighbors))
			{
				chunkSync.InProgressRegeneration = false;
				continue;
			}
			if (chunkSync.NeedsOnlyCollisionMesh && !_isOnlyColliders)
			{
				chunkSync.NeedsRegeneration = true;
			}
			chunkCluster.RegenerateChunk(chunkSync, regenerateNextChunkNeighbors);
			chunkSync.InProgressRegeneration = false;
			chunkSync.NeedsOnlyCollisionMesh = _isOnlyColliders;
			for (int j = 0; j < regenerateNextChunkNeighbors.Length; j++)
			{
				regenerateNextChunkNeighbors[j].InProgressRegeneration = false;
			}
			OnChunkRegenerated?.Invoke(chunkSync);
			for (int k = 0; k < regenerateNextChunkNeighbors.Length; k++)
			{
				Chunk chunk = regenerateNextChunkNeighbors[k];
				if (!chunk.NeedsRegeneration)
				{
					OnChunkRegenerated?.Invoke(chunk);
				}
			}
			Array.Clear(regenerateNextChunkNeighbors, 0, regenerateNextChunkNeighbors.Length);
			return chunkSync;
		}
		Array.Clear(regenerateNextChunkNeighbors, 0, regenerateNextChunkNeighbors.Length);
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasAnyChunkEntities(Chunk[] chunks)
	{
		if (chunks[0].hasEntities)
		{
			return true;
		}
		if (chunks[1].hasEntities)
		{
			return true;
		}
		if (chunks[2].hasEntities)
		{
			return true;
		}
		if (chunks[3].hasEntities)
		{
			return true;
		}
		if (chunks[4].hasEntities)
		{
			return true;
		}
		if (chunks[5].hasEntities)
		{
			return true;
		}
		if (chunks[6].hasEntities)
		{
			return true;
		}
		if (chunks[7].hasEntities)
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockGenerateInProgress(Chunk[] chunks)
	{
		for (int i = 0; i < chunks.Length; i++)
		{
			Chunk chunk = chunks[i];
			chunk.EnterWriteLock();
			if (chunk.IsLocked)
			{
				for (int j = 0; j < i; j++)
				{
					chunks[j].InProgressRegeneration = false;
				}
				chunk.ExitWriteLock();
				return false;
			}
			chunk.InProgressRegeneration = true;
			chunk.ExitWriteLock();
		}
		return true;
	}

	public void ReloadAllChunks()
	{
		m_ViewingChunkPositions.Clear();
		m_AllChunkPositions.Clear();
		m_CollisionChunkPositions.Clear();
		recalcFreeChunkGameObjects(int.MaxValue, _bIgnoreFixedSizeFlag: true);
		m_World.ChunkClusters[0]?.Clear();
		for (int i = 0; i < m_ObservedEntities.Count; i++)
		{
			m_ObservedEntities[i].curChunkPos.y = -1;
		}
	}

	public ArraySegment<long> GetActiveChunkSet()
	{
		return new ArraySegment<long>(activeChunkSetArr, 0, m_AllChunkPositions.list.Count);
	}

	public bool IsForceUpdate()
	{
		if (!isInternalForceUpdate)
		{
			return isChunkClusterChanged;
		}
		return true;
	}

	public void DetermineChunksToLoad()
	{
		int num = removeChunksToUnload(8);
		bool flag = isInternalForceUpdate || isChunkClusterChanged;
		for (int i = 0; i < m_ObservedEntities.Count; i++)
		{
			ChunkObserver chunkObserver = m_ObservedEntities[i];
			Vector3i vector3i = new Vector3i(World.toChunkXZ(Utils.Fastfloor(chunkObserver.position.x)), 0, World.toChunkXZ(Utils.Fastfloor(chunkObserver.position.z)));
			bool flag2 = vector3i != chunkObserver.curChunkPos;
			chunkObserver.curChunkPos = vector3i;
			if (!isChunkClusterChanged && !flag2)
			{
				continue;
			}
			flag = true;
			int x = vector3i.x;
			int z = vector3i.z;
			chunkObserver.chunksAround.Clear();
			for (int j = 0; j < chunkObserver.viewDim + 2; j++)
			{
				List<Vector2i> list = rectanglesAroundPlayers[j];
				for (int k = 0; k < list.Count; k++)
				{
					chunkObserver.chunksAround.Add(j, WorldChunkCache.MakeChunkKey(list[k].x + x, list[k].y + z));
				}
			}
			int count = m_World.ChunkClusters.Count;
			for (int l = 1; l < count; l++)
			{
				ChunkCluster chunkCluster = m_World.ChunkClusters[l];
				if (chunkCluster != null)
				{
					chunkObserver.chunksAround.Add(2, chunkCluster.GetChunkKeysCopySync());
				}
			}
			chunkObserver.chunksAround.RecalcHashSetList();
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				continue;
			}
			chunkObserver.chunksToLoad.Clear();
			for (int m = 0; m < chunkObserver.chunksToLoad.buckets.Count; m++)
			{
				chunkObserver.chunksToLoad.Add(m, chunkObserver.chunksAround.buckets.array[m]);
				chunkObserver.chunksToLoad.buckets.array[m].ExceptWithHashSetLong(chunkObserver.chunksLoaded);
			}
			chunkObserver.chunksToLoad.RecalcHashSetList();
			chunkObserver.chunksToRemove.Clear();
			chunkObserver.chunksToRemove.UnionWithHashSetLong(chunkObserver.chunksLoaded);
			chunkObserver.chunksAround.ExceptTarget(chunkObserver.chunksToRemove);
			foreach (long item in chunkObserver.chunksToLoad.list)
			{
				chunkGenerationTimestamps[item] = DateTime.UtcNow;
			}
		}
		isInternalForceUpdate = false;
		isChunkClusterChanged = false;
		if (flag)
		{
			lock (lockObject)
			{
				isViewingOrCollisionPositionsChanged_threadCalc = true;
				isViewingOrCollisionPositionsChanged_threadReg = true;
				m_ViewingChunkPositions.Clear();
				m_AllChunkPositions.Clear();
				for (int n = 0; n <= m_AllChunkPositions.buckets.Count; n++)
				{
					for (int num2 = 0; num2 < m_ObservedEntities.Count; num2++)
					{
						ChunkObserver chunkObserver2 = m_ObservedEntities[num2];
						if (n < chunkObserver2.viewDim + 2)
						{
							m_AllChunkPositions.Add(n, chunkObserver2.chunksAround.buckets.array[n]);
							if (chunkObserver2.bBuildVisualMeshAround)
							{
								m_ViewingChunkPositions.buckets.array[n].UnionWithHashSetLong(chunkObserver2.chunksAround.buckets.array[n]);
							}
						}
					}
				}
				m_ViewingChunkPositions.RecalcHashSetList();
				m_AllChunkPositions.RecalcHashSetList();
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					m_AllChunkPositions.list.CopyTo(activeChunkSetArr);
					m_CollisionChunkPositions.Clear();
					for (int num3 = 0; num3 < m_CollisionChunkPositions.buckets.Count; num3++)
					{
						m_CollisionChunkPositions.buckets.array[num3].UnionWithHashSetLong(m_AllChunkPositions.buckets.array[num3]);
						m_CollisionChunkPositions.buckets.array[num3].ExceptWithHashSetLong(m_ViewingChunkPositions.buckets.array[num3]);
					}
					m_CollisionChunkPositions.RecalcHashSetList();
					ChunkCluster chunkCache = m_World.ChunkCache;
					if (chunkCache != null && !chunkCache.IsFixedSize)
					{
						lock (chunksToUnload)
						{
							foreach (long item2 in chunkCache.GetChunkKeysCopySync())
							{
								if (!m_AllChunkPositions.Contains(item2) && !DynamicMeshThread.ChunksToProcess.Contains(item2) && !DynamicMeshThread.ChunksToLoad.Contains(item2))
								{
									Chunk chunkSync = chunkCache.GetChunkSync(item2);
									chunkSync.InProgressUnloading = true;
									chunksToUnload.Add(chunkSync);
								}
							}
							for (int num4 = 0; num4 < chunksToUnload.list.Count; num4++)
							{
								chunkCache.RemoveChunk(chunksToUnload.list[num4]);
							}
						}
					}
				}
			}
			if (8 - num > 0)
			{
				recalcFreeChunkGameObjects(8 - num);
			}
		}
		calcThreadWaitHandle.Set();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int removeChunksToUnload(int _maxCGOsToUnload = int.MaxValue)
	{
		lock (chunksToUnload)
		{
			int num = 0;
			for (int num2 = chunksToUnload.list.Count - 1; num2 >= 0; num2--)
			{
				Chunk chunk = chunksToUnload.list[num2];
				ChunkCluster chunkCluster = m_World.ChunkClusters[WorldChunkCache.extractClrIdx(chunk.Key)];
				if (chunkCluster != null)
				{
					bool flag = false;
					chunk.EnterWriteLock();
					chunk.InProgressUnloading = true;
					if (!chunk.IsLockedExceptUnloading)
					{
						flag = true;
					}
					chunk.ExitWriteLock();
					if (flag)
					{
						chunksToUnload.Remove(chunk);
						if (chunk.IsDisplayed)
						{
							FreeChunkGameObject(chunkCluster, chunk);
							num++;
						}
						chunkCluster.UnloadChunk(chunk);
						if (num >= _maxCGOsToUnload)
						{
							break;
						}
					}
				}
			}
			return num;
		}
	}

	public void ProcessChunksPendingUnload(Action<Chunk> action)
	{
		lock (chunksToUnload)
		{
			foreach (Chunk item in chunksToUnload.list)
			{
				action(item);
			}
		}
	}

	public long GetNextChunkToProvide()
	{
		ChunkCluster chunkCache = m_World.ChunkCache;
		if (chunkCache != null)
		{
			int num = 0;
			lock (lockObject)
			{
				m_AllChunkPositions.list.CopyTo(allChunkPositionsCopy);
				num = m_AllChunkPositions.list.Count;
			}
			for (int i = 0; i < num; i++)
			{
				long num2 = allChunkPositionsCopy[i];
				if (WorldChunkCache.extractClrIdx(num2) == 0 && !chunkCache.ContainsChunkSync(num2))
				{
					return num2;
				}
			}
			IChunkProvider chunkProvider = chunkCache.ChunkProvider;
			if (chunkProvider != null)
			{
				HashSetList<long> requestedChunks = chunkProvider.GetRequestedChunks();
				if (requestedChunks != null)
				{
					lock (requestedChunks.list)
					{
						int count = requestedChunks.list.Count;
						if (count > 0)
						{
							long num3 = requestedChunks.list[count - 1];
							requestedChunks.Remove(num3);
							return num3;
						}
					}
				}
			}
		}
		return long.MaxValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk checkChunkNeedsCopying(long key)
	{
		int num = WorldChunkCache.extractClrIdx(key);
		ChunkCluster chunkCluster = m_World.ChunkClusters[num];
		if (chunkCluster == null)
		{
			return null;
		}
		Chunk chunkSync = chunkCluster.GetChunkSync(key);
		if (chunkSync == null || chunkSync.IsLocked)
		{
			return null;
		}
		bool needsCopying = chunkSync.NeedsCopying;
		if (num != 0 && !needsCopying)
		{
			return null;
		}
		bool flag;
		lock (chunkCluster.DisplayedChunkGameObjects)
		{
			flag = chunkCluster.DisplayedChunkGameObjects.ContainsKey(chunkSync.Key);
		}
		bool flag2 = !chunkSync.NeedsDecoration && !chunkSync.NeedsLightCalculation && !chunkSync.NeedsRegeneration;
		chunkSync.EnterWriteLock();
		if ((flag && !needsCopying) || (!flag && !flag2) || chunkSync.IsLocked)
		{
			chunkSync.ExitWriteLock();
			return null;
		}
		if (chunkSync.HasMeshLayer() || chunkSync.IsEmpty())
		{
			chunkSync.ExitWriteLock();
			return chunkSync;
		}
		chunkSync.NeedsRegeneration = true;
		chunkSync.ExitWriteLock();
		return null;
	}

	public ICollection GetCurrDisplayedChunkGameObjects()
	{
		return tempDisplayedCGOs;
	}

	public IList<ChunkGameObject> GetDisplayedChunkGameObjects()
	{
		tempDisplayedCGOs.Clear();
		for (int i = 0; i < m_World.ChunkClusters.Count; i++)
		{
			if (m_World.ChunkClusters[i] != null)
			{
				tempDisplayedCGOs.AddRange(m_World.ChunkClusters[i].DisplayedChunkGameObjects.Dict.Values);
			}
		}
		return tempDisplayedCGOs;
	}

	public int GetDisplayedChunkGameObjectsCount()
	{
		int num = 0;
		for (int i = 0; i < m_World.ChunkClusters.Count; i++)
		{
			if (m_World.ChunkClusters[i] != null)
			{
				num += m_World.ChunkClusters[i].DisplayedChunkGameObjects.Count;
			}
		}
		return num;
	}

	public List<ChunkGameObject> GetFreeChunkGameObjects()
	{
		return m_FreeChunkGameObjects;
	}

	public List<ChunkGameObject> GetUsedChunkGameObjects()
	{
		return m_UsedChunkGameObjects;
	}

	public void RemoveChunk(long _chunkKey)
	{
		int idx = WorldChunkCache.extractClrIdx(_chunkKey);
		ChunkCluster chunkCluster = m_World.ChunkClusters[idx];
		if (chunkCluster == null)
		{
			Log.Warning("RemoveChunk: cluster not found " + idx);
			return;
		}
		Chunk chunkSync = chunkCluster.GetChunkSync(_chunkKey);
		if (chunkSync == null)
		{
			Log.Warning("RemoveChunk: chunk not found " + WorldChunkCache.extractX(_chunkKey) + "/" + WorldChunkCache.extractZ(_chunkKey));
			return;
		}
		chunkSync.InProgressUnloading = true;
		chunkCluster.RemoveChunk(chunkSync);
		lock (chunksToUnload)
		{
			chunksToUnload.Add(chunkSync);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void recalcFreeChunkGameObjects(int _maxToUnload = int.MaxValue, bool _bIgnoreFixedSizeFlag = false)
	{
		ChunkCluster chunkCache = m_World.ChunkCache;
		if (chunkCache == null || (chunkCache.IsFixedSize && !_bIgnoreFixedSizeFlag))
		{
			return;
		}
		cgoToRemove.Clear();
		foreach (KeyValuePair<long, ChunkGameObject> item in chunkCache.DisplayedChunkGameObjects.Dict)
		{
			if (!m_ViewingChunkPositions.Contains(item.Key) && !m_CollisionChunkPositions.Contains(item.Key))
			{
				cgoToRemove.Add(item.Key);
			}
		}
		for (int i = 0; i < cgoToRemove.Count; i++)
		{
			FreeChunkGameObject(chunkCache, cgoToRemove[i]);
			if (_maxToUnload-- <= 0)
			{
				break;
			}
		}
	}

	public void FreeChunkGameObject(ChunkCluster _cc, Chunk _chunk)
	{
		long key = _chunk.Key;
		ChunkGameObject chunkGameObject = _cc.DisplayedChunkGameObjects[key];
		if (!(chunkGameObject == null) && chunkGameObject.GetChunk() == _chunk)
		{
			FreeChunkGameObject(_cc, key);
		}
	}

	public void FreeChunkGameObject(ChunkCluster _cc, long _key)
	{
		DynamicMeshThread.RemoveChunkGameObject(_key);
		ChunkGameObject chunkGameObject = _cc.RemoveDisplayedChunkGameObject(_key);
		chunkGameObject.gameObject.SetActive(value: false);
		_cc.OnChunkDisplayed(_key, _isDisplayed: false);
		if (currentCopiedChunkGameObject == chunkGameObject && currentCopiedChunk != null)
		{
			currentCopiedChunk.InProgressCopying = false;
			currentCopiedChunk = null;
		}
		chunkGameObject.SetChunk(null, null);
		m_UsedChunkGameObjects.Remove(chunkGameObject);
		m_FreeChunkGameObjects.Add(chunkGameObject);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkGameObject GetNextFreeChunkGameObject()
	{
		ChunkGameObject chunkGameObject;
		if (m_FreeChunkGameObjects.Count == 0)
		{
			chunkGameObject = new GameObject("Chunk (new)").AddComponent<ChunkGameObject>();
		}
		else
		{
			chunkGameObject = m_FreeChunkGameObjects[m_FreeChunkGameObjects.Count - 1];
			m_FreeChunkGameObjects.RemoveAt(m_FreeChunkGameObjects.Count - 1);
		}
		m_UsedChunkGameObjects.Add(chunkGameObject);
		return chunkGameObject;
	}

	public void ClearChunksForAllObservers(ChunkCluster _cc)
	{
		for (int i = 0; i < m_ObservedEntities.Count; i++)
		{
			ChunkObserver chunkObserver = m_ObservedEntities[i];
			ClearChunksForObserver(chunkObserver, _cc);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isKeyLocalCC(long _key)
	{
		return WorldChunkCache.extractClrIdx(_key) == 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearChunksForObserver(ChunkObserver _chunkObserver, ChunkCluster cc)
	{
		for (int i = 0; i < _chunkObserver.chunksAround.buckets.Count; i++)
		{
			_chunkObserver.chunksAround.buckets.array[i].RemoveWhere(isKeyLocalCC);
			_chunkObserver.chunksToLoad.buckets.array[i].RemoveWhere(isKeyLocalCC);
		}
		_chunkObserver.chunksAround.RecalcHashSetList();
		_chunkObserver.chunksToLoad.RecalcHashSetList();
		_chunkObserver.chunksToReload.Clear();
		HashSetLong chunkKeysCopySync = cc.GetChunkKeysCopySync();
		_chunkObserver.chunksLoaded.ExceptWithHashSetLong(chunkKeysCopySync);
		_chunkObserver.chunksToRemove.ExceptWithHashSetLong(chunkKeysCopySync);
	}

	public void DebugOnGUI(float middleX, float middleY, int size)
	{
		for (int i = 0; i < m_ObservedEntities.Count; i++)
		{
			Color col = Color.white;
			if (!m_ObservedEntities[i].bBuildVisualMeshAround && m_ObservedEntities[i].entityIdToSendChunksTo != -1)
			{
				col = Color.cyan;
			}
			Vector3i vector3i = World.worldToBlockPos(m_ObservedEntities[i].position);
			GUIUtils.DrawRect(new Rect(middleX + (float)(World.toChunkXZ(vector3i.x) * size) - (float)(size / 2), middleY - (float)(World.toChunkXZ(vector3i.z) * size) - (float)(size / 2), size, size), col);
		}
	}

	public void RemoveAllChunksOnAllClients()
	{
		HashSetLong chunkKeysCopySync = m_World.ChunkCache.GetChunkKeysCopySync();
		List<EntityPlayerLocal> localPlayers = m_World.GetLocalPlayers();
		for (int i = 0; i < m_ObservedEntities.Count; i++)
		{
			if (m_ObservedEntities[i].bBuildVisualMeshAround)
			{
				continue;
			}
			bool flag = false;
			int num = 0;
			while (!flag && num < localPlayers.Count)
			{
				if (m_ObservedEntities[i].entityIdToSendChunksTo == localPlayers[num].entityId)
				{
					flag = true;
				}
				num++;
			}
			if (flag)
			{
				continue;
			}
			foreach (long item in chunkKeysCopySync)
			{
				m_ObservedEntities[i].chunksLoaded.Remove(item);
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageChunkRemoveAll>(), _onlyClientsAttachedToAnEntity: false, m_ObservedEntities[i].entityIdToSendChunksTo);
		}
		isChunkClusterChanged = true;
	}

	public void RemoveAllChunks()
	{
		ChunkCluster chunkCache = m_World.ChunkCache;
		foreach (long item in chunkCache.GetChunkKeysCopySync())
		{
			Chunk chunkSync = chunkCache.GetChunkSync(item);
			if (chunkSync != null)
			{
				RemoveChunk(chunkSync.Key);
			}
		}
		removeChunksToUnload();
		chunkCache.Clear();
	}

	public void ForceUpdate()
	{
		isInternalForceUpdate = true;
	}

	public void AddGroundAlignBlock(BlockEntityData _data)
	{
		groundAlignBlockLists[groundAlignIndex].Add(_data);
	}

	public void GroundAlignFrameUpdate()
	{
		groundAlignIndex = (groundAlignIndex + 1) & 1;
		List<BlockEntityData> list = groundAlignBlockLists[groundAlignIndex];
		int count = list.Count;
		if (count > 0)
		{
			for (int i = 0; i < count; i++)
			{
				BlockEntityData blockEntityData = list[i];
				blockEntityData.blockValue.Block.GroundAlign(blockEntityData);
			}
			list.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GroundAlignCleanup()
	{
		groundAlignBlockLists[0].Clear();
		groundAlignBlockLists[1].Clear();
	}

	public void BakeInit()
	{
		bakeThreadInfo = ThreadManager.StartThread("ChunkMeshBake", null, BakeThread, null, null, null, _useRealThread: true);
		bakeCoroutine = BakeEndOfFrame();
		GameManager.Instance.StartCoroutine(bakeCoroutine);
	}

	public void BakeCleanup()
	{
		bakeThreadInfo.RequestTermination();
		bakeEvent.Set();
		bakeThreadInfo.WaitForEnd();
		bakeThreadInfo = null;
		bakes.Clear();
		GameManager.Instance.StopCoroutine(bakeCoroutine);
	}

	public void BakeDestroyCancel(MeshCollider _meshCollider)
	{
		lock (bakes)
		{
			Mesh sharedMesh = _meshCollider.sharedMesh;
			if ((bool)sharedMesh)
			{
				_meshCollider.sharedMesh = null;
				UnityEngine.Object.Destroy(sharedMesh);
			}
			for (int i = 0; i < bakes.Count; i++)
			{
				BakeCollider bakeCollider = bakes[i];
				if (bakeCollider.meshCollider == _meshCollider)
				{
					if ((bool)bakeCollider.mesh)
					{
						UnityEngine.Object.Destroy(bakeCollider.mesh);
						bakeCollider.mesh = null;
					}
					bakeCollider.isBaked = true;
				}
			}
		}
	}

	public Mesh BakeCancelAndGetMesh(MeshCollider _meshCollider)
	{
		lock (bakes)
		{
			Mesh mesh = _meshCollider.sharedMesh;
			BakeCollider bakeCollider = null;
			for (int num = bakes.Count - 1; num >= 0; num--)
			{
				BakeCollider bakeCollider2 = bakes[num];
				if (bakeCollider2.meshCollider == _meshCollider && (bool)bakeCollider2.mesh)
				{
					mesh = bakeCollider2.mesh;
					bakeCollider = bakeCollider2;
					break;
				}
			}
			BakeCollider bakeCollider3 = bakeCurrent;
			if (bakeCollider3 != null && (bool)bakeCollider3.mesh && bakeCollider3.mesh == mesh)
			{
				bakeCollider3.isCancelledDestroy = true;
				return null;
			}
			if (bakeCollider != null)
			{
				bakeCollider.mesh = null;
				bakeCollider.isBaked = true;
			}
			return mesh;
		}
	}

	public void BakeAdd(Mesh _mesh, MeshCollider _meshCollider)
	{
		BakeCollider bakeCollider = new BakeCollider();
		bakeCollider.mesh = _mesh;
		bakeCollider.id = _mesh.GetInstanceID();
		bakeCollider.meshCollider = _meshCollider;
		lock (bakes)
		{
			bakes.Add(bakeCollider);
		}
		bakeEvent.Set();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int BakeThread(ThreadManager.ThreadInfo _threadInfo)
	{
		bakeEvent.WaitOne();
		if (_threadInfo.TerminationRequested())
		{
			return -1;
		}
		while (true)
		{
			BakeCollider bakeCollider;
			lock (bakes)
			{
				if (bakeIndex >= bakes.Count)
				{
					break;
				}
				bakeCollider = bakes[bakeIndex++];
				if (bakeCollider.isBaked)
				{
					continue;
				}
				bakeCurrent = bakeCollider;
				goto IL_0078;
			}
			IL_0078:
			Physics.BakeMesh(bakeCollider.id, convex: false);
			bakeCollider.isBaked = true;
			bakeCurrent = null;
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator BakeEndOfFrame()
	{
		WaitForEndOfFrame wait = new WaitForEndOfFrame();
		while (true)
		{
			yield return wait;
			BakeMeshAssign();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BakeMeshAssign()
	{
		lock (bakes)
		{
			bakeIndex = 0;
			for (int i = 0; i < bakes.Count; i++)
			{
				BakeCollider bakeCollider = bakes[i];
				if (!bakeCollider.mesh)
				{
					continue;
				}
				if (bakeCollider.isCancelledDestroy)
				{
					UnityEngine.Object.Destroy(bakeCollider.mesh);
					continue;
				}
				Mesh sharedMesh = bakeCollider.meshCollider.sharedMesh;
				bakeCollider.meshCollider.sharedMesh = bakeCollider.mesh;
				if ((bool)sharedMesh && sharedMesh != bakeCollider.mesh)
				{
					UnityEngine.Object.Destroy(sharedMesh);
				}
			}
			bakes.Clear();
		}
	}

	public int SetBlockEntitiesVisible(bool _on, string _name)
	{
		ChunkCluster chunkCache = m_World.ChunkCache;
		if (chunkCache == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < m_AllChunkPositions.list.Count; i++)
		{
			long key = m_AllChunkPositions.list[i];
			Chunk chunkSync = chunkCache.GetChunkSync(key);
			if (chunkSync != null)
			{
				num += chunkSync.EnableEntityBlocks(_on, _name);
			}
		}
		return num;
	}

	[Conditional("DEBUG_CHUNK")]
	public static void TimerStart(string _name)
	{
		MicroStopwatch value;
		lock (debugTimers)
		{
			if (!debugTimers.TryGetValue(_name, out value))
			{
				value = new MicroStopwatch(_bStart: true);
				debugTimers.Add(_name, value);
			}
		}
		value.Restart();
	}

	[Conditional("DEBUG_CHUNK")]
	public static void TimerLog(string _name, string _format = "", params object[] _args)
	{
		MicroStopwatch value;
		lock (debugTimers)
		{
			debugTimers.TryGetValue(_name, out value);
		}
		if (value != null)
		{
			float num = (float)value.ElapsedMicroseconds * 0.001f;
			int frameCount = GameManager.frameCount;
			if (ThreadManager.IsMainThread())
			{
				frameCount = Time.frameCount;
			}
			_format = $"{frameCount} {num}ms {_name} {_format}";
			Log.Warning(_format, _args);
			value.Restart();
		}
	}

	public static double SecondsSinceChunkSelectedForGeneration(long chunkKey)
	{
		if (chunkGenerationTimestamps.TryGetValue(chunkKey, out var value))
		{
			return (DateTime.UtcNow - value).TotalSeconds;
		}
		return -1.0;
	}

	public static void LogCurrentGenerationState()
	{
		int poolSize = MemoryPools.poolVML.GetPoolSize();
		int instanceCount = VoxelMeshLayer.InstanceCount;
		int num = instanceCount - poolSize;
		if (num > MaxQueuedMeshLayers)
		{
			int num2 = num - MaxQueuedMeshLayers;
			TimeSpan timeSpan = DateTime.UtcNow - vmlExhaustionStartTime;
			Log.Warning($"[FELLTHROUGHWORLD] ChunkManager generation thread blocked for {timeSpan.TotalSeconds:F1}s - VML max queued exceeded by {num2}, (Instance: {instanceCount}, Pooled: {poolSize}, MaxQueued: {MaxQueuedMeshLayers})");
		}
		else
		{
			Log.Out($"[FELLTHROUGHWORLD] ChunkManager generation thread running - VML pool (Instance: {instanceCount}, Pooled: {poolSize}, Available: {poolSize - instanceCount})");
		}
	}
}
