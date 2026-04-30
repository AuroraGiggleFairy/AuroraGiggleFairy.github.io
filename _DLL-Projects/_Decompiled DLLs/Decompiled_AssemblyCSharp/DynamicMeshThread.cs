using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConcurrentCollections;
using UnityEngine;

public class DynamicMeshThread
{
	public class ThreadRegion
	{
		public int X;

		public int Z;

		public long Key;

		[PublicizedFrom(EAccessModifier.Private)]
		public ConcurrentHashSet<long> LoadedChunks = new ConcurrentHashSet<long>();

		[PublicizedFrom(EAccessModifier.Private)]
		public int xIndex;

		[PublicizedFrom(EAccessModifier.Private)]
		public int zIndex;

		public bool IsRegerating;

		public float UpdateTime = float.MaxValue;

		[PublicizedFrom(EAccessModifier.Private)]
		public object chunkListLock = new object();

		public int LoadedChunkCount => LoadedChunks.Count;

		public void AddLoadedChunk(long key)
		{
			lock (chunkListLock)
			{
				LoadedChunks.Add(key);
			}
		}

		public bool RemoveLoadedChunk(long key)
		{
			lock (chunkListLock)
			{
				return LoadedChunks.TryRemove(key);
			}
		}

		public void CopyLoadedChunks(List<long> chunks)
		{
			chunks.Clear();
			lock (chunkListLock)
			{
				chunks.AddRange(LoadedChunks);
			}
		}

		public ThreadRegion(long key)
		{
			Key = key;
			xIndex = WorldChunkCache.extractX(key);
			zIndex = WorldChunkCache.extractZ(key);
			X = xIndex * 16;
			Z = zIndex * 16;
		}

		public Vector3i ToWorldPosition()
		{
			return new Vector3i(X, 0, Z);
		}

		public string ToDebugLocation()
		{
			return $"R:{X} {Z}";
		}

		public bool IsInItemLoad(float playerX, float playerZ)
		{
			if (GameManager.Instance == null)
			{
				return false;
			}
			if (GameManager.Instance.World == null)
			{
				return false;
			}
			if (GameManager.Instance.World.GetPrimaryPlayer() == null)
			{
				return false;
			}
			return DynamicMeshUnity.IsInBuffer(playerX, playerZ, DynamicMeshRegion.ItemLoadIndex, xIndex, zIndex);
		}
	}

	public static bool Paused = false;

	public static bool NoProcessing = false;

	public static bool LockMeshesAfterGenerating = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string QueuePrimary = "Primary";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string QueueSecondary = "Secondary";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string QueueNone = "None";

	public static string Queue;

	public static string Processed;

	public static ChunkQueue ChunksToLoad = new ChunkQueue();

	public static ConcurrentHashSet<long> ChunksToProcess = new ConcurrentHashSet<long>();

	public static ConcurrentQueue<long> nextChunks = new ConcurrentQueue<long>();

	public static bool QueueUpdateOverride;

	public static bool RequestThreadStop = false;

	public static bool AddRegionChecks = false;

	public static int CachePurgeInterval = 8;

	public static List<List<Vector3i>> RegionsToCheck = null;

	public static DynamicMeshRegionDataStorage RegionStorage = new DynamicMeshRegionDataStorage();

	public static DynamicMeshChunkDataStorage<DynamicMeshItem> ChunkDataQueue = new DynamicMeshChunkDataStorage<DynamicMeshItem>(CachePurgeInterval);

	public static ConcurrentQueue<DynamicMeshItem> ChunkMeshGenRequests = new ConcurrentQueue<DynamicMeshItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static ConcurrentQueue<DynamicMeshItem> TempChunkMeshGenRequests = new ConcurrentQueue<DynamicMeshItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static ConcurrentQueue<DyMeshRegionLoadRequest> RegionFileLoadRequests = new ConcurrentQueue<DyMeshRegionLoadRequest>();

	public static DynamicMeshBuilderManager BuilderManager = DynamicMeshBuilderManager.GetOrCreate();

	public static string RegionUpdatesDebug = "";

	public static ConcurrentDictionary<long, DynamicMeshUpdateData> RegionUpdates = new ConcurrentDictionary<long, DynamicMeshUpdateData>();

	public static ConcurrentQueue<DynamicMeshData> ReadyForCollection = new ConcurrentQueue<DynamicMeshData>();

	public static ConcurrentHashSet<Vector2i> ChunkReadyForCollection = new ConcurrentHashSet<Vector2i>();

	public static float PlayerPositionX;

	public static float PlayerPositionZ;

	public static Queue<DynamicMeshItem> ToGenerate = new Queue<DynamicMeshItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static LinkedList<DynamicMeshItem> NeedObservers = new LinkedList<DynamicMeshItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static LinkedList<DynamicMeshItem> IgnoredChunks = new LinkedList<DynamicMeshItem>();

	public static ConcurrentDictionary<long, DynamicMeshItem> PrimaryQueue = new ConcurrentDictionary<long, DynamicMeshItem>();

	public static ConcurrentDictionary<long, DynamicMeshItem> SecondaryQueue = new ConcurrentDictionary<long, DynamicMeshItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<ChunkGameObject> LoadedGos = new List<ChunkGameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Queue<ChunkGameObject> NewlyLoadedGos = new Queue<ChunkGameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Queue<ChunkGameObject> ToRemoveGos = new Queue<ChunkGameObject>();

	public static ConcurrentDictionary<long, ThreadRegion> threadRegions = new ConcurrentDictionary<long, ThreadRegion>();

	public static Queue<DynamicMeshServerUpdates> ServerUpdates = new Queue<DynamicMeshServerUpdates>(20);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Thread MeshThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<long> Keys = new List<long>(50);

	[PublicizedFrom(EAccessModifier.Private)]
	public static DateTime NextRun = DateTime.Now;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DateTime StartTime = DateTime.Now;

	public static int MeshGenCount => ChunkMeshGenRequests.Count + TempChunkMeshGenRequests.Count;

	public static float time => (float)(DateTime.Now - StartTime).TotalSeconds;

	public static bool IsServer => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;

	public static void AddChunkGameObject(Chunk chunk)
	{
		if (GameManager.IsDedicatedServer || chunk.NeedsOnlyCollisionMesh)
		{
			return;
		}
		DynamicMeshManager.ChunkGameObjects.Add(chunk.Key);
		if (DynamicMeshManager.Instance != null)
		{
			DynamicMeshItem itemOrNull = DynamicMeshManager.Instance.GetItemOrNull(chunk.GetWorldPos());
			if (itemOrNull != null)
			{
				itemOrNull.ForceHide();
				itemOrNull.GetRegion().OnChunkVisible(itemOrNull);
			}
		}
	}

	public static void RemoveChunkGameObject(long key)
	{
		if (!GameManager.IsDedicatedServer && DynamicMeshManager.Instance != null)
		{
			DynamicMeshManager.ChunkGameObjects.Remove(key);
			DynamicMeshManager.Instance.ShowChunk(key);
		}
	}

	public static void CleanUp()
	{
		ChunksToProcess.Clear();
		ChunksToLoad.Clear();
		RequestThreadStop = true;
		BuilderManager.StopThreads(forceStop: true);
		DynamicMeshServer.CleanUp();
		ServerUpdates.Clear();
		RegionUpdates?.Clear();
		ClearData();
		ToGenerate?.Clear();
		NeedObservers?.Clear();
		IgnoredChunks?.Clear();
		PrimaryQueue?.Clear();
		SecondaryQueue?.Clear();
		LoadedGos?.Clear();
		ToRemoveGos?.Clear();
		threadRegions?.Clear();
		nextChunks = new ConcurrentQueue<long>();
	}

	public static bool AddChunkUpdateFromServer(DynamicMeshServerUpdates data)
	{
		ServerUpdates.Enqueue(data);
		DynamicMeshManager.Instance.AddChunkStub(new Vector3i(data.ChunkX, data.StartY, data.ChunkZ), null);
		return true;
	}

	public static void AddRegionChunk(int worldX, int worldZ, long key)
	{
		GetThreadRegion(worldX, worldZ).AddLoadedChunk(key);
	}

	public static void RemoveRegionChunk(int worldX, int worldZ, long key)
	{
		if (!GetThreadRegion(worldX, worldZ).RemoveLoadedChunk(key) && DynamicMeshManager.DoLog)
		{
			Log.Warning("Failed to remove threaded chunk");
		}
	}

	public static bool AddRegionUpdateData(int worldX, int worldZ, bool isUrgent)
	{
		if (GameManager.IsDedicatedServer)
		{
			return false;
		}
		ThreadRegion threadRegion = GetThreadRegion(worldX, worldZ);
		RegionUpdates.TryGetValue(threadRegion.Key, out var value);
		if (value == null)
		{
			value = new DynamicMeshUpdateData();
			value.ChunkPosition.x = threadRegion.X;
			value.ChunkPosition.z = threadRegion.Z;
			value.Key = threadRegion.Key;
			value.IsUrgent = false;
			RegionUpdates.TryAdd(value.Key, value);
		}
		value.UpdateTime = time + (float)((!isUrgent) ? 3 : 0);
		value.IsUrgent |= isUrgent;
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg("Adding thread region update " + threadRegion.ToDebugLocation() + " Time: " + value.UpdateTime + " urgent: " + isUrgent);
		}
		return true;
	}

	public static ThreadRegion GetThreadRegion(Vector3i worldPos)
	{
		return GetThreadRegionInternal(DynamicMeshUnity.GetRegionKeyFromWorldPosition(worldPos));
	}

	public static ThreadRegion GetThreadRegion(int worldX, int worldZ)
	{
		return GetThreadRegionInternal(DynamicMeshUnity.GetRegionKeyFromWorldPosition(worldX, worldZ));
	}

	public static ThreadRegion GetThreadRegion(long key)
	{
		return GetThreadRegionInternal(DynamicMeshUnity.GetRegionKeyFromItemKey(key));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static ThreadRegion GetThreadRegionInternal(long key)
	{
		if (!threadRegions.TryGetValue(key, out var value))
		{
			value = new ThreadRegion(key);
			if (!threadRegions.TryAdd(key, value))
			{
				threadRegions.TryGetValue(key, out value);
			}
		}
		return value;
	}

	public static void SetNextChunksFromQueues()
	{
		QueueUpdateOverride = true;
		foreach (KeyValuePair<long, DynamicMeshItem> item in PrimaryQueue)
		{
			nextChunks.Enqueue(item.Key);
		}
		foreach (KeyValuePair<long, DynamicMeshItem> item2 in SecondaryQueue)
		{
			nextChunks.Enqueue(item2.Key);
		}
	}

	public static void SetNextChunks(long key)
	{
		if (nextChunks.Count <= 512)
		{
			int num = WorldChunkCache.extractX(key);
			int num2 = WorldChunkCache.extractZ(key);
			long item = WorldChunkCache.MakeChunkKey(num, num2 + 1);
			long item2 = WorldChunkCache.MakeChunkKey(num, num2 - 1);
			long item3 = WorldChunkCache.MakeChunkKey(num + 1, num2);
			long item4 = WorldChunkCache.MakeChunkKey(num + 1, num2 + 1);
			long item5 = WorldChunkCache.MakeChunkKey(num + 1, num2 - 1);
			long item6 = WorldChunkCache.MakeChunkKey(num - 1, num2);
			long item7 = WorldChunkCache.MakeChunkKey(num - 1, num2 + 1);
			long item8 = WorldChunkCache.MakeChunkKey(num - 1, num2 - 1);
			nextChunks.Enqueue(key);
			nextChunks.Enqueue(item);
			nextChunks.Enqueue(item2);
			nextChunks.Enqueue(item3);
			nextChunks.Enqueue(item4);
			nextChunks.Enqueue(item5);
			nextChunks.Enqueue(item6);
			nextChunks.Enqueue(item7);
			nextChunks.Enqueue(item8);
		}
	}

	public static void RequestChunk(long key)
	{
		nextChunks.Enqueue(key);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetNextChunkToLoad()
	{
		if (nextChunks.Count > 0 || (PrimaryQueue.Count == 0 && SecondaryQueue.Count == 0))
		{
			return;
		}
		DynamicMeshManager instance = DynamicMeshManager.Instance;
		if (instance == null)
		{
			return;
		}
		if (instance.NearestRegionWithUnloaded == null)
		{
			if (instance.FindNearestUnloadedItems)
			{
				return;
			}
			instance.FindNearestUnloadedItems = true;
			if (PrimaryQueue.Count > 0)
			{
				instance.PrimaryLocation = (from d in PrimaryQueue
					where d.Value.WorldPosition != d.Value.GetRegionLocation()
					select d.Value).FirstOrDefault()?.WorldPosition;
			}
			else if (!instance.PrimaryLocation.HasValue && SecondaryQueue.Count > 0)
			{
				instance.PrimaryLocation = (from d in SecondaryQueue
					where d.Value.WorldPosition != d.Value.GetRegionLocation()
					select d.Value).FirstOrDefault()?.WorldPosition;
			}
			return;
		}
		DynamicMeshRegion nearestRegionWithUnloaded = instance.NearestRegionWithUnloaded;
		List<DynamicMeshItem> unloadedItems = nearestRegionWithUnloaded.UnloadedItems;
		if (!QueueUpdateOverride && GameManager.Instance.World.ChunkCache.chunks.Count > 600)
		{
			GameManager.Instance.World.m_ChunkManager.ForceUpdate();
		}
		instance.NearestRegionWithUnloaded = null;
		for (int num = 0; num < unloadedItems.Count; num++)
		{
			DynamicMeshItem dynamicMeshItem = unloadedItems[num];
			if (dynamicMeshItem != null)
			{
				long key = dynamicMeshItem.Key;
				if (SecondaryQueue.ContainsKey(dynamicMeshItem.Key))
				{
					RequestPrimaryQueue(dynamicMeshItem);
				}
				SetNextChunks(key);
			}
		}
		unloadedItems = nearestRegionWithUnloaded.LoadedItems;
		for (int num = 0; num < unloadedItems.Count; num++)
		{
			DynamicMeshItem dynamicMeshItem2 = unloadedItems[num];
			long key2 = dynamicMeshItem2.Key;
			if (SecondaryQueue.ContainsKey(dynamicMeshItem2.Key))
			{
				RequestPrimaryQueue(dynamicMeshItem2);
			}
			SetNextChunks(key2);
		}
	}

	public static ConcurrentDictionary<long, DynamicMeshItem> GetQueue(bool isPrimary)
	{
		if (!isPrimary)
		{
			return SecondaryQueue;
		}
		return PrimaryQueue;
	}

	public static long GetNextChunkToLoad()
	{
		if (RequestThreadStop)
		{
			return long.MaxValue;
		}
		if (nextChunks.Count > 0)
		{
			if (!nextChunks.TryDequeue(out var result))
			{
				return long.MaxValue;
			}
			return result;
		}
		return long.MaxValue;
	}

	public static void RequestSecondaryQueue(DynamicMeshItem item)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			long key = item.Key;
			if (!PrimaryQueue.ContainsKey(key) && !SecondaryQueue.ContainsKey(key))
			{
				GetThreadRegion(item.WorldPosition);
				SecondaryQueue.TryAdd(key, item);
				ChunksToLoad.Add(key);
				ChunksToProcess.Add(key);
			}
		}
	}

	public static void RequestPrimaryQueue(DynamicMeshItem item)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			long key = item.Key;
			GetThreadRegion(item.WorldPosition);
			if (SecondaryQueue.ContainsKey(key))
			{
				SecondaryQueue.TryRemove(key, out var _);
			}
			if (DynamicMeshManager.Instance.PrefabCheck != PrefabCheckState.Run)
			{
				Vector3i regionLocation = item.GetRegionLocation();
				CheckSecondaryQueueForRegion(regionLocation, item.WorldPosition);
				CheckSecondaryQueueForRegion(regionLocation + new Vector3i(160, 0, 160), item.WorldPosition);
				CheckSecondaryQueueForRegion(regionLocation + new Vector3i(160, 0, -160), item.WorldPosition);
				CheckSecondaryQueueForRegion(regionLocation + new Vector3i(-160, 0, 160), item.WorldPosition);
				CheckSecondaryQueueForRegion(regionLocation + new Vector3i(-160, 0, -160), item.WorldPosition);
			}
			if (!PrimaryQueue.ContainsKey(key))
			{
				PrimaryQueue.TryAdd(key, item);
				ChunksToLoad.Add(key);
				ChunksToProcess.Add(key);
			}
		}
	}

	public static void CheckSecondaryQueueForRegion(Vector3i regionPos, Vector3i itemPos)
	{
		for (int i = regionPos.x; i < regionPos.x + 160; i += 16)
		{
			for (int j = regionPos.z; j < regionPos.z + 160; j += 16)
			{
				long key = WorldChunkCache.MakeChunkKey(World.toChunkXZ(i), World.toChunkXZ(j));
				if (SecondaryQueue.TryRemove(key, out var value))
				{
					PrimaryQueue.TryAdd(key, value);
				}
			}
		}
	}

	public static void SetDefaultThreads()
	{
		DynamicMeshBuilderManager.MaxBuilderThreads = Math.Min(Math.Min(8, Math.Max(SystemInfo.processorCount - 2, 1)), DynamicMeshSettings.MaxDyMeshData + 1);
	}

	public static void StartThread()
	{
		StopThreadForce();
		ClearData();
		RequestThreadStop = false;
		ChunkDataQueue = new DynamicMeshChunkDataStorage<DynamicMeshItem>(CachePurgeInterval);
		if (ChunkDataQueue.MaxAllowedItems == 0)
		{
			ChunkDataQueue.MaxAllowedItems = 300;
		}
		ChunksToLoad.Clear();
		ChunksToProcess.Clear();
		SetDefaultThreads();
		StartTime = DateTime.Now;
		MeshThread = new Thread([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			while (GameManager.Instance == null || GameManager.Instance.World == null)
			{
				Thread.Sleep(100);
			}
			Log.Out("Dynamic thread starting");
			RequestThreadStop = false;
			if (DynamicMeshManager.IsValidGameMode())
			{
				while (!RequestThreadStop)
				{
					GenerationThread();
				}
			}
			if (!RequestThreadStop)
			{
				Log.Error("Dynamic thread stopped");
			}
			ClearData();
		});
		MeshThread.Start();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ClearData()
	{
		DynamicMeshData result;
		while (ReadyForCollection.TryDequeue(out result))
		{
		}
		Vector2i returnValue;
		while (ChunkReadyForCollection.TryRemoveFirst(out returnValue))
		{
		}
		RegionStorage.ClearQueues();
		while (ToGenerate.Count != 0 && ToGenerate.Dequeue() != null)
		{
		}
		PrimaryQueue.Clear();
		SecondaryQueue.Clear();
		NeedObservers.Clear();
	}

	public static void StopThreadRequest()
	{
		RequestThreadStop = true;
	}

	public static void StopThreadForce()
	{
		if (MeshThread == null)
		{
			return;
		}
		try
		{
			ChunkDataQueue.ClearQueues();
			MeshThread.Abort();
		}
		catch (Exception ex)
		{
			if (DynamicMeshManager.DoLog)
			{
				DynamicMeshManager.LogMsg("Dynamic Mesh Thread abort error " + ex.Message);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void GetKeys(WorldChunkCache cache)
	{
		Keys.Clear();
		ReaderWriterLockSlim syncRoot = cache.GetSyncRoot();
		syncRoot.EnterReadLock();
		try
		{
			foreach (long chunkKey in cache.chunkKeys)
			{
				Keys.Add(chunkKey);
			}
		}
		finally
		{
			syncRoot.ExitReadLock();
		}
	}

	public static void RemoveFromQueues(long key)
	{
		PrimaryQueue.TryRemove(key, out var value);
		SecondaryQueue.TryRemove(key, out value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void GenerationThread()
	{
		try
		{
			if (DateTime.Now < NextRun)
			{
				Thread.Sleep((int)Math.Max(1.0, (NextRun - DateTime.Now).TotalMilliseconds));
				return;
			}
			if (Paused || DynamicMeshManager.Instance == null)
			{
				NextRun = DateTime.Now.AddMilliseconds(500.0);
				return;
			}
			if (ChunkDataQueue.ChunkData.Count < ChunkDataQueue.LiveItems)
			{
				ChunkDataQueue.LiveItems = ChunkDataQueue.ChunkData.Count;
			}
			while (NewlyLoadedGos.Count > 0)
			{
				LoadedGos.Add(NewlyLoadedGos.Dequeue());
			}
			while (ToRemoveGos.Count > 0)
			{
				LoadedGos.Remove(ToRemoveGos.Dequeue());
			}
			HandleRegionLoads();
			BuilderManager.CheckBuilders();
			AddRegionChecks = PrimaryQueue.Count == 0 && SecondaryQueue.Count == 0;
			bool hasThreadAvailable = BuilderManager.HasThreadAvailable;
			if ((ChunkMeshGenRequests.Count == 0 && RegionUpdates.Count == 0 && PrimaryQueue.Count == 0 && SecondaryQueue.Count == 0 && ChunkMeshGenRequests.Count == 0) || !hasThreadAvailable)
			{
				if (DynamicMeshSettings.NewWorldFullRegen && hasThreadAvailable && DynamicMeshManager.Instance.PrefabCheck == PrefabCheckState.WaitingForCompleteCheck)
				{
					WriteChecksComplete();
					DynamicMeshServer.ProcessDelayedPackages();
				}
				NextRun = DateTime.Now.AddMilliseconds(300.0);
				return;
			}
			SetNextChunkToLoad();
			ProcessRegionRegenRequests();
			ProcessMeshGenerationRequests();
			bool flag = false;
			GetKeys(GameManager.Instance.World.ChunkCache);
			Queue = QueuePrimary;
			if (PrimaryQueue.Count > 0)
			{
				flag = ProcessQueue(PrimaryQueue);
			}
			if (!flag)
			{
				if (PrimaryQueue.Count > 0)
				{
					if (DynamicMeshManager.DoLog)
					{
						Log.Out("Setting chunks");
					}
					foreach (long key in PrimaryQueue.Keys)
					{
						if (!Keys.Contains(key) && !nextChunks.Contains(key))
						{
							SetNextChunks(key);
						}
					}
				}
				if (SecondaryQueue.Count > 0)
				{
					Queue = QueueSecondary;
					flag = ProcessQueue(SecondaryQueue);
				}
			}
			Queue = QueueNone;
		}
		catch (Exception ex)
		{
			if (DynamicMeshManager.DoLog)
			{
				DynamicMeshManager.LogMsg("Process requests " + ex.Message + "\n" + ex.StackTrace);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool ProcessQueue(ConcurrentDictionary<long, DynamicMeshItem> queue)
	{
		if (RequestThreadStop)
		{
			return false;
		}
		if (!BuilderManager.HasThreadAvailable)
		{
			return false;
		}
		bool result = false;
		int num = 0;
		int num2 = 0;
		bool flag = queue == PrimaryQueue;
		if (DynamicMeshManager.DoLog)
		{
			Log.Out($"Checking {Keys.Count} keys");
		}
		foreach (long key in Keys)
		{
			if (RequestThreadStop)
			{
				return false;
			}
			if (PrimaryQueue.Count > 0 && !flag)
			{
				break;
			}
			if (!queue.TryGetValue(key, out var value))
			{
				continue;
			}
			Chunk chunkSync = GameManager.Instance.World.ChunkCache.GetChunkSync(key);
			if (chunkSync == null || !DynamicMeshChunkProcessor.IsChunkLoaded(chunkSync))
			{
				if (DynamicMeshManager.DoLog)
				{
					Log.Out(value.ToDebugLocation() + " not in world cache");
					nextChunks.Enqueue(key);
				}
				num++;
				continue;
			}
			if (!GameManager.Instance.World.ChunkCache.HasNeighborChunks(chunkSync))
			{
				num2++;
				SetNextChunks(chunkSync.Key);
				if (DynamicMeshManager.DoLog)
				{
					Log.Out(value.ToDebugLocation() + " no neighbours");
				}
				continue;
			}
			result = true;
			int num3 = BuilderManager.AddItemForExport(value, flag);
			if (num3 == 1)
			{
				queue.TryRemove(key, out var _);
			}
			if (num3 == -1 || BuilderManager.HasThreadAvailable)
			{
				continue;
			}
			break;
		}
		return result;
	}

	public static void WriteChecksComplete()
	{
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg("All chunks checked. Disabling future checks");
		}
		SdFile.WriteAllText(DynamicMeshFile.MeshLocation + "!!ChunksChecked.info", time.ToString());
		DynamicMeshManager.Instance.PrefabCheck = PrefabCheckState.Run;
	}

	public static void AddChunkGenerationRequest(DynamicMeshItem item)
	{
		AddRegionChunk(item.WorldPosition.x, item.WorldPosition.z, item.Key);
		if (!GameManager.IsDedicatedServer)
		{
			ChunkMeshGenRequests.Enqueue(item);
		}
	}

	public static void AddRegionLoadRequest(DyMeshRegionLoadRequest request)
	{
		if (!GameManager.IsDedicatedServer)
		{
			if (DynamicMeshManager.DoLog)
			{
				Log.Out("Loading region go " + DynamicMeshUnity.GetDebugPositionFromKey(request.Key));
			}
			RegionFileLoadRequests.Enqueue(request);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ProcessMeshGenerationRequests()
	{
		if (!ChunkMeshGenRequests.TryDequeue(out var result))
		{
			return;
		}
		TempChunkMeshGenRequests.Enqueue(result);
		DynamicMeshItem dynamicMeshItem = result;
		float num = result.DistanceToPlayer(PlayerPositionX, PlayerPositionZ);
		DynamicMeshItem result2;
		while (ChunkMeshGenRequests.TryDequeue(out result2))
		{
			if ((PlayerPositionX != 0f || PlayerPositionZ != 0f) && !DynamicMeshUnity.IsInBuffer(PlayerPositionX, PlayerPositionZ, DynamicMeshRegion.ItemLoadIndex, result2.WorldPosition.x / 160, result2.WorldPosition.z / 160))
			{
				result2.State = DynamicItemState.Waiting;
				continue;
			}
			TempChunkMeshGenRequests.Enqueue(result2);
			float num2 = result2.DistanceToPlayer(PlayerPositionX, PlayerPositionZ);
			if (num2 < num)
			{
				num = num2;
				dynamicMeshItem = result2;
			}
		}
		DynamicMeshItem result3;
		while (TempChunkMeshGenRequests.TryDequeue(out result3))
		{
			if (result3 != dynamicMeshItem)
			{
				ChunkMeshGenRequests.Enqueue(result3);
			}
		}
		GetThreadRegion(dynamicMeshItem.Key);
		if (!dynamicMeshItem.FileExists())
		{
			dynamicMeshItem.State = DynamicItemState.Empty;
		}
		else if (BuilderManager.AddItemForMeshGeneration(dynamicMeshItem, isPrimary: false) != 1)
		{
			ChunkMeshGenRequests.Enqueue(dynamicMeshItem);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ProcessRegionRegenRequests()
	{
		bool useAllThreads = false;
		foreach (KeyValuePair<long, DynamicMeshUpdateData> regionUpdate in RegionUpdates)
		{
			if (RequestThreadStop)
			{
				break;
			}
			DynamicMeshUpdateData value = regionUpdate.Value;
			ThreadRegion threadRegionInternal = GetThreadRegionInternal(value.Key);
			if (threadRegionInternal.LoadedChunkCount > 0 && value.UpdateTime < time)
			{
				if (BuilderManager.RegenerateRegion(threadRegionInternal, useAllThreads) == 1)
				{
					RegionUpdates.TryRemove(regionUpdate.Key, out value);
				}
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void HandleRegionLoads()
	{
		DyMeshRegionLoadRequest result;
		while (RegionFileLoadRequests.TryDequeue(out result))
		{
			if (DynamicMeshManager.DoLog)
			{
				Log.Out("Loading region from storage " + DynamicMeshUnity.GetDebugPositionFromKey(result.Key));
			}
			RegionStorage.LoadRegion(result);
			DynamicMeshManager.Instance.RegionFileLoadRequests.Enqueue(result);
		}
	}
}
