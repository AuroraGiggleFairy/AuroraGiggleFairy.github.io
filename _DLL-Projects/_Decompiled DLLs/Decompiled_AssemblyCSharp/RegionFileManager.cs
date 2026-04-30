using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Unity.Profiling;
using UnityEngine;

public class RegionFileManager : WorldChunkCache
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class ProtectedPositionCache
	{
		public readonly List<Vector3i> bedrolls = new List<Vector3i>();

		public readonly List<Vector3i> lpBlocks = new List<Vector3i>();

		public readonly List<Vector3i> offlinePlayers = new List<Vector3i>();

		public readonly List<Vector3i> backpacks = new List<Vector3i>();

		public readonly List<Vector3i> quests = new List<Vector3i>();

		public readonly List<Vector3i> vendingMachines = new List<Vector3i>();

		public readonly List<Vector3i> vehicles = new List<Vector3i>();

		public readonly List<Vector3i> drones = new List<Vector3i>();

		public readonly List<Vector3i> supplyCrates = new List<Vector3i>();

		public void ClearAll()
		{
			bedrolls.Clear();
			lpBlocks.Clear();
			offlinePlayers.Clear();
			backpacks.Clear();
			quests.Clear();
			vendingMachines.Clear();
			vehicles.Clear();
			drones.Clear();
			supplyCrates.Clear();
		}
	}

	public const int cProtectedLandClaimChunkMargin = 1;

	public const int cProtectedBedrollChunkMargin = 1;

	public const int cProtectedOfflinePlayerChunkMargin = 1;

	public const int cProtectedDroppedBackpackChunkMargin = 1;

	public const int cProtectedVehicleChunkMargin = 1;

	public const int cProtectedQuestObjectiveMargin = 1;

	public const int cProtectedSupplyCrateMargin = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_CullExpiredChunks = new ProfilerMarker("RegionFileManager.CullExpiredChunks");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_RemoveChunk = new ProfilerMarker("RegionFileManager.RemoveChunk");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_UpdateProtectionLevels = new ProfilerMarker("RegionFileManager.UpdateProtectionLevels");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_SortCullingCandidates = new ProfilerMarker("RegionFileManager.SortCullingCandidates");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_CollectBestCandidates = new ProfilerMarker("RegionFileManager.CollectBestCandidates");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ForceCullChunks = new ProfilerMarker("RegionFileManager.ForceCullChunks");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ComputeChunkGroups = new ProfilerMarker("RegionFileManager.ComputeChunkGroups");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ProcessPersistentDataRemovals = new ProfilerMarker("RegionFileManager.ProcessPendingPersistentDataRemovals");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_CheckPersistentData = new ProfilerMarker("RegionFileManager.CheckPersistentData");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<long> s_RegionDataSize = new ProfilerCounterValue<long>(ProfilerCategory.Scripts, "Saved Region Data Size", ProfilerMarkerDataUnit.Bytes, ProfilerCounterOptions.FlushOnEndOfFrame);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_ProtectedChunkCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Protected Chunks", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);

	public const string pendingResetsFileName = "PendingResets.7pr";

	public const string cChunkFileExt = ".ttc";

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxChunksToCull = 10000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMinimumByteAllowance = 20971520;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long cHeadroomBytes = 5242880L;

	[PublicizedFrom(EAccessModifier.Private)]
	public static IRegionFilePlatformFactory platformFactory = RegionFilePlatform.CreateFactory();

	public static readonly IRegionFileDebugUtil DebugUtil = platformFactory.CreateDebugUtil();

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileAccessAbstract regionFileAccess;

	[PublicizedFrom(EAccessModifier.Private)]
	public string saveDirectory;

	[PublicizedFrom(EAccessModifier.Private)]
	public string loadDirectory;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxChunksInCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public long maxChunkAge = -1L;

	[PublicizedFrom(EAccessModifier.Private)]
	public long maxBytes = -1L;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> chunksInLocalCache = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong chunksInLoadDir = new HashSetLong();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, uint> chunksInSaveDir;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> expiredChunks = new List<long>(10000);

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> resetRequestedChunks = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<long> persistentDataRemovalsRequestedChunks = new HashSet<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool protectionLevelsDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, ChunkProtectionLevel> chunkProtectionLevels = new Dictionary<long, ChunkProtectionLevel>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<KeyValuePair<long, ulong>> sortedCullingCandidates = new List<KeyValuePair<long, ulong>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, HashSetLong> chunksByTraderID = new Dictionary<int, HashSetLong>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<HashSetLong> chunkGroups = new List<HashSetLong>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, HashSetLong> groupsByChunkKey = new Dictionary<long, HashSetLong>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<HashSetLong, ChunkProtectionLevel> groupProtectionLevels = new Dictionary<HashSetLong, ChunkProtectionLevel>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<HashSetLong, uint> groupTimestamps = new Dictionary<HashSetLong, uint>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<HashSetLong> processedChunkGroups = new HashSet<HashSetLong>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<NetPackageDeleteChunkData> pendingChunkDeletionPackages = new List<NetPackageDeleteChunkData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, Chunk> chunksToSave = new Dictionary<long, Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, IRegionFileChunkSnapshot> chunkMemoryStreamsToSave = new Dictionary<long, IRegionFileChunkSnapshot>();

	[PublicizedFrom(EAccessModifier.Private)]
	public IRegionFileChunkSnapshotUtil snapshotUtil;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Chunk> chunksToUnloadLater = new List<Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public AutoResetEvent saveThreadWaitHandle = new AutoResetEvent(initialState: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bSaveOnChunkDrop;

	[PublicizedFrom(EAccessModifier.Private)]
	public long chunkKeyCurrentlySaved = long.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo taskInfoThreadSaveChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool bSaveRunning = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProtectedPositionCache ppdPositionCache = new ProtectedPositionCache();

	[PublicizedFrom(EAccessModifier.Private)]
	public int thread_SaveChunks_SleepCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object saveLock = new object();

	public long MaxBytes => maxBytes;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public SaveDataSlot SaveDataSlot { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ReadOnlyCollection<HashSetLong> ChunkGroups
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public RegionFileManager(string _loadDirectory, string _saveDirectory, int _maxChunksInCache, bool _bSaveOnChunkDrop)
	{
		platformFactory = RegionFilePlatform.CreateFactory();
		SaveDataUtils.SaveDataManager.RegisterRegionFileManager(this);
		Vector2i worldSize = GameManager.Instance.World.ChunkCache.ChunkProvider.GetWorldSize();
		int capacity = worldSize.x / 16 * (worldSize.y / 16);
		chunksInSaveDir = new Dictionary<long, uint>(capacity);
		bSaveOnChunkDrop = _bSaveOnChunkDrop;
		groupTimestamps.Clear();
		RebuildChunkGroupsFromPOIs();
		ChunkGroups = new ReadOnlyCollection<HashSetLong>(chunkGroups);
		loadDirectory = ((_loadDirectory != null) ? Path.GetFullPath(_loadDirectory) : null);
		saveDirectory = ((_saveDirectory != null) ? Path.GetFullPath(_saveDirectory) : null);
		if (SaveDataUtils.TryGetManagedPath(saveDirectory, out var managedPath))
		{
			SaveDataSlot = managedPath.Slot;
			Debug.Log($"[RegionFileManager] SaveDataSlot set to: {managedPath.Slot}");
		}
		regionFileAccess = platformFactory.CreateRegionFileAccess();
		regionFileAccess.ReadDirectory(loadDirectory, [PublicizedFrom(EAccessModifier.Private)] (long chunkKey, string ext, uint timeStamp) =>
		{
			chunksInLoadDir.Add(chunkKey);
		});
		regionFileAccess.ReadDirectory(saveDirectory, [PublicizedFrom(EAccessModifier.Private)] (long chunkKey, string ext, uint timeStamp) =>
		{
			SetChunkTimestamp(chunkKey, timeStamp);
		});
		maxChunksInCache = _maxChunksInCache;
		snapshotUtil = platformFactory.CreateSnapshotUtil(regionFileAccess);
		OnGamePrefChanged(EnumGamePrefs.MaxChunkAge);
		OnGamePrefChanged(EnumGamePrefs.SaveDataLimit);
		GamePrefs.OnGamePrefChanged += OnGamePrefChanged;
		LoadResetRequests();
		taskInfoThreadSaveChunks = ThreadManager.StartThread("SaveChunks " + (string.IsNullOrEmpty(saveDirectory) ? loadDirectory : saveDirectory), null, thread_SaveChunks, null, null, null, _useRealThread: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LoadResetRequests()
	{
		if (string.IsNullOrEmpty(saveDirectory))
		{
			return;
		}
		resetRequestedChunks.Clear();
		string path = Path.Combine(saveDirectory, "PendingResets.7pr");
		if (!SdFile.Exists(path))
		{
			return;
		}
		using Stream clientStream = SdFile.OpenRead(path);
		int num = StreamUtils.ReadInt32(clientStream);
		for (int i = 0; i < num; i++)
		{
			resetRequestedChunks.Add(StreamUtils.ReadInt64(clientStream));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveResetRequests()
	{
		if (string.IsNullOrEmpty(saveDirectory))
		{
			return;
		}
		string path = Path.Combine(saveDirectory, "PendingResets.7pr");
		if (resetRequestedChunks.Count == 0)
		{
			if (SdFile.Exists(path))
			{
				SdFile.Delete(path);
			}
			return;
		}
		using Stream clientStream = SdFile.OpenWrite(path);
		StreamUtils.Write(clientStream, resetRequestedChunks.Count);
		foreach (long resetRequestedChunk in resetRequestedChunks)
		{
			StreamUtils.Write(clientStream, resetRequestedChunk);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGamePrefChanged(EnumGamePrefs pref)
	{
		lock (chunksInSaveDir)
		{
			switch (pref)
			{
			case EnumGamePrefs.MaxChunkAge:
			{
				int num = GamePrefs.GetInt(EnumGamePrefs.MaxChunkAge);
				maxChunkAge = num * 24 * 60;
				if (maxChunkAge >= 0)
				{
					(int Days, int Hours, int Minutes) tuple = GameUtils.WorldTimeToElements(GameUtils.TotalMinutesToWorldTime((uint)maxChunkAge));
					int item = tuple.Days;
					int item2 = tuple.Hours;
					int item3 = tuple.Minutes;
					item--;
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Chunk Reset Time (MaxChunkAge) has been set to {item} days, {item2} hours, and {item3} minutes.");
				}
				else
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Chunk Reset Time (MaxChunkAge) has been disabled.");
				}
				break;
			}
			case EnumGamePrefs.SaveDataLimit:
			{
				long limitFromPref = SaveDataLimit.GetLimitFromPref();
				if (limitFromPref >= 0)
				{
					maxBytes = limitFromPref;
					if (maxBytes < 20971520)
					{
						Log.Warning($"Cannot set RegionFileManager storage limit to {maxBytes} bytes as it is below the minimum value of {20971520} bytes. The miniumum value will be used instead.");
						maxBytes = 20971520L;
					}
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Save data limit has been set to {maxBytes} bytes.");
				}
				else
				{
					maxBytes = -1L;
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Save data limit has been disabled.");
				}
				break;
			}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetChunkTimestamp(long chunkKey, uint timeStamp)
	{
		chunksInSaveDir[chunkKey] = timeStamp;
		if (groupsByChunkKey.TryGetValue(chunkKey, out var value) && (!groupTimestamps.TryGetValue(value, out var value2) || value2 < timeStamp))
		{
			groupTimestamps[value] = timeStamp;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public uint GetChunkTimestamp(long chunkKey)
	{
		if (groupsByChunkKey.TryGetValue(chunkKey, out var value) && groupTimestamps.TryGetValue(value, out var value2))
		{
			return value2;
		}
		if (chunksInSaveDir.TryGetValue(chunkKey, out var value3))
		{
			return value3;
		}
		Log.Error($"No timestamp available for chunk with key: {chunkKey}");
		return 0u;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkProtectionLevel GetChunkProtectionLevelWithGrouping(long chunkKey)
	{
		if (groupsByChunkKey.TryGetValue(chunkKey, out var value) && groupProtectionLevels.TryGetValue(value, out var value2))
		{
			return value2;
		}
		if (chunkProtectionLevels.TryGetValue(chunkKey, out var value3))
		{
			return value3;
		}
		return ChunkProtectionLevel.None;
	}

	public ChunkProtectionLevel GetChunkProtectionLevelForWorldPos(Vector3i worldPos)
	{
		return GetChunkProtectionLevel(World.toChunkXZ(worldPos.x), World.toChunkXZ(worldPos.z));
	}

	public ChunkProtectionLevel GetChunkProtectionLevel(int chunkX, int chunkZ)
	{
		lock (saveLock)
		{
			lock (chunksInSaveDir)
			{
				if (protectionLevelsDirty)
				{
					UpdateChunkProtectionLevels();
				}
				long key = WorldChunkCache.MakeChunkKey(chunkX, chunkZ);
				if (chunkProtectionLevels.TryGetValue(key, out var value))
				{
					return value;
				}
				return ChunkProtectionLevel.None;
			}
		}
	}

	public void RebuildChunkGroupsFromPOIs()
	{
		using (s_ComputeChunkGroups.Auto())
		{
			lock (chunksInSaveDir)
			{
				List<PrefabInstance> dynamicPrefabs = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator().GetDynamicPrefabs();
				chunkGroups.Clear();
				groupsByChunkKey.Clear();
				chunksByTraderID.Clear();
				HashSetLong hashSetLong = null;
				foreach (PrefabInstance item in dynamicPrefabs)
				{
					if (item.name.Contains("rwg_tile"))
					{
						continue;
					}
					HashSetLong occupiedChunks = item.GetOccupiedChunks();
					if (occupiedChunks.Count > 1)
					{
						if (item.prefab.bTraderArea)
						{
							chunksByTraderID[item.id] = new HashSetLong(occupiedChunks);
						}
						HashSetLong hashSetLong2 = MergeOrCreateChunkGroup(occupiedChunks);
						if (hashSetLong == null || hashSetLong2.Count > hashSetLong.Count)
						{
							hashSetLong = hashSetLong2;
						}
					}
				}
				Log.Out($"Computed {chunkGroups.Count} chunk groups containing a total of {groupsByChunkKey.Count} chunks. Largest group contains {hashSetLong?.Count} chunks.");
			}
		}
	}

	public void AddGroupedChunks(ICollection<long> chunksToGroup)
	{
		MergeOrCreateChunkGroup(chunksToGroup);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong MergeOrCreateChunkGroup(ICollection<long> chunksToGroup)
	{
		lock (chunksInSaveDir)
		{
			HashSetLong hashSetLong = null;
			foreach (long item in chunksToGroup)
			{
				if (!groupsByChunkKey.TryGetValue(item, out var value))
				{
					continue;
				}
				if (hashSetLong == null)
				{
					hashSetLong = value;
				}
				else
				{
					if (hashSetLong == value)
					{
						continue;
					}
					hashSetLong.UnionWith(value);
					chunkGroups.Remove(value);
					foreach (long item2 in value)
					{
						groupsByChunkKey[item2] = hashSetLong;
					}
				}
			}
			if (hashSetLong == null)
			{
				hashSetLong = new HashSetLong(chunksToGroup);
				chunkGroups.Add(hashSetLong);
			}
			else
			{
				hashSetLong.UnionWith(chunksToGroup);
			}
			foreach (long item3 in chunksToGroup)
			{
				groupsByChunkKey[item3] = hashSetLong;
			}
			return hashSetLong;
		}
	}

	public override void Update()
	{
		base.Update();
		ProcessPendingPersistentDataRemovals();
	}

	public void Cleanup()
	{
		startSavingTask();
		bSaveRunning = false;
		if (taskInfoThreadSaveChunks != null)
		{
			taskInfoThreadSaveChunks.WaitForEnd();
		}
		ProcessChunkDeletionPackages();
		SaveResetRequests();
		SaveDataUtils.SaveDataManager.DeregisterRegionFileManager(this);
		regionFileAccess.Close();
		GamePrefs.OnGamePrefChanged -= OnGamePrefChanged;
		expiredChunks.Clear();
		resetRequestedChunks.Clear();
		persistentDataRemovalsRequestedChunks.Clear();
		chunkProtectionLevels.Clear();
		sortedCullingCandidates.Clear();
		chunksByTraderID.Clear();
		chunkGroups.Clear();
		groupsByChunkKey.Clear();
		groupProtectionLevels.Clear();
		groupTimestamps.Clear();
		processedChunkGroups.Clear();
		pendingChunkDeletionPackages.Clear();
		lock (ppdPositionCache)
		{
			ppdPositionCache.ClearAll();
		}
		snapshotUtil.Cleanup();
	}

	public void SetCacheSize(int _cacheSize)
	{
		maxChunksInCache = _cacheSize;
	}

	public void RequestChunkReset(long _chunkKey)
	{
		lock (chunksInSaveDir)
		{
			if (groupsByChunkKey.TryGetValue(_chunkKey, out var value))
			{
				foreach (long item in value)
				{
					if (!resetRequestedChunks.Contains(item))
					{
						resetRequestedChunks.Add(item);
					}
				}
				return;
			}
			if (!resetRequestedChunks.Contains(_chunkKey))
			{
				resetRequestedChunks.Add(_chunkKey);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CullExpiredChunks()
	{
		if (maxChunkAge < 0 && resetRequestedChunks.Count == 0)
		{
			return;
		}
		using (s_CullExpiredChunks.Auto())
		{
			lock (saveLock)
			{
				lock (chunksInSaveDir)
				{
					if (protectionLevelsDirty)
					{
						UpdateChunkProtectionLevels();
					}
					if (maxChunkAge >= 0)
					{
						uint num = GameUtils.WorldTimeToTotalMinutes(GameManager.Instance.World.worldTime);
						foreach (long key in chunksInSaveDir.Keys)
						{
							if (!resetRequestedChunks.Contains(key))
							{
								uint chunkTimestamp = GetChunkTimestamp(key);
								if (num - chunkTimestamp <= maxChunkAge)
								{
									continue;
								}
							}
							if (chunkProtectionLevels.TryGetValue(key, out var value))
							{
								UpdateResetRequest(key, value);
								continue;
							}
							expiredChunks.Add(key);
							resetRequestedChunks.Remove(key);
							if (expiredChunks.Count < 10000)
							{
								continue;
							}
							break;
						}
					}
					else
					{
						for (int num2 = resetRequestedChunks.Count - 1; num2 >= 0; num2--)
						{
							long num3 = resetRequestedChunks[num2];
							if (chunkProtectionLevels.TryGetValue(num3, out var value2))
							{
								UpdateResetRequest(num3, value2);
							}
							else
							{
								expiredChunks.Add(num3);
								resetRequestedChunks.RemoveAt(num2);
								if (expiredChunks.Count >= 10000)
								{
									break;
								}
							}
						}
					}
					RemoveChunks(expiredChunks);
					expiredChunks.Clear();
				}
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void UpdateResetRequest(long chunkKey, ChunkProtectionLevel protectionLevel)
		{
			if (!protectionLevel.HasFlag(ChunkProtectionLevel.CurrentlySynced) && !protectionLevel.HasFlag(ChunkProtectionLevel.OfflinePlayer) && !protectionLevel.HasFlag(ChunkProtectionLevel.NearOfflinePlayer) && !protectionLevel.HasFlag(ChunkProtectionLevel.QuestObjective) && !protectionLevel.HasFlag(ChunkProtectionLevel.NearQuestObjective))
			{
				resetRequestedChunks.Remove(chunkKey);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MakeRoomForChunk(long chunkSizeInBytes, long chunkKey)
	{
		lock (saveLock)
		{
			lock (chunksInSaveDir)
			{
				if (maxBytes <= 0)
				{
					return;
				}
				long num = GetChunkByteCount(chunkKey);
				long num2 = chunkSizeInBytes - num;
				if (num2 <= 0)
				{
					return;
				}
				long totalByteCount = regionFileAccess.GetTotalByteCount(saveDirectory);
				long num3 = totalByteCount + num2 - maxBytes;
				if (num3 <= 0)
				{
					return;
				}
				if (chunkSizeInBytes > maxBytes)
				{
					throw new Exception($"Requested space ({chunkSizeInBytes} bytes) exceeds maximum available space ({maxBytes} bytes).");
				}
				HashSetLong value = null;
				groupsByChunkKey.TryGetValue(chunkKey, out value);
				if (protectionLevelsDirty)
				{
					UpdateChunkProtectionLevels();
				}
				using (s_SortCullingCandidates.Auto())
				{
					sortedCullingCandidates.Clear();
					foreach (long key in chunksInSaveDir.Keys)
					{
						if (key != chunkKey && (value == null || !value.Contains(chunkKey)))
						{
							ulong num4 = GetChunkTimestamp(key);
							if (chunkProtectionLevels.TryGetValue(key, out var value2))
							{
								num4 += (ulong)((long)value2 << 32);
							}
							sortedCullingCandidates.Add(new KeyValuePair<long, ulong>(key, num4));
						}
					}
					sortedCullingCandidates.Sort([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<long, ulong> a, KeyValuePair<long, ulong> b) => a.Value.CompareTo(b.Value));
				}
				long num5 = 0L;
				using (s_CollectBestCandidates.Auto())
				{
					long num6 = num3 + 5242880;
					expiredChunks.Clear();
					processedChunkGroups.Clear();
					foreach (KeyValuePair<long, ulong> sortedCullingCandidate in sortedCullingCandidates)
					{
						if (num5 >= num6 || (num5 >= num3 && sortedCullingCandidate.Value >= 549755813888L))
						{
							break;
						}
						if (groupsByChunkKey.TryGetValue(sortedCullingCandidate.Key, out var value3))
						{
							if (processedChunkGroups.Contains(value3))
							{
								continue;
							}
							foreach (long item in value3)
							{
								if (chunksInSaveDir.ContainsKey(item))
								{
									expiredChunks.Add(item);
									int chunkByteCount = GetChunkByteCount(item);
									num5 += chunkByteCount;
								}
							}
							processedChunkGroups.Add(value3);
						}
						else
						{
							expiredChunks.Add(sortedCullingCandidate.Key);
							int chunkByteCount2 = GetChunkByteCount(sortedCullingCandidate.Key);
							num5 += chunkByteCount2;
						}
					}
				}
				using (s_ForceCullChunks.Auto())
				{
					RemoveChunks(expiredChunks);
					expiredChunks.Clear();
				}
				if (totalByteCount - regionFileAccess.GetTotalByteCount(saveDirectory) >= num3)
				{
					return;
				}
				throw new Exception("Failed to clear as much space as requested.");
			}
		}
	}

	public bool MakeRoom(long requiredBytesToClear)
	{
		if (requiredBytesToClear <= 0)
		{
			return true;
		}
		lock (saveLock)
		{
			lock (chunksInSaveDir)
			{
				if (protectionLevelsDirty)
				{
					UpdateChunkProtectionLevels();
				}
				long totalByteCount = regionFileAccess.GetTotalByteCount(saveDirectory);
				if (requiredBytesToClear > totalByteCount)
				{
					Debug.LogError("RegionFileManager has been requested to clear more space than is currently occupied by region data. This may imply the save data limit has been set too low, or another system has exceeded its expected save data budget. Region data will be cleared, but the target will not be met.");
				}
				using (s_SortCullingCandidates.Auto())
				{
					sortedCullingCandidates.Clear();
					foreach (long key in chunksInSaveDir.Keys)
					{
						ulong num = GetChunkTimestamp(key);
						if (chunkProtectionLevels.TryGetValue(key, out var value))
						{
							num += (ulong)((long)value << 32);
						}
						sortedCullingCandidates.Add(new KeyValuePair<long, ulong>(key, num));
					}
					sortedCullingCandidates.Sort([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<long, ulong> a, KeyValuePair<long, ulong> b) => a.Value.CompareTo(b.Value));
				}
				long num2 = 0L;
				using (s_CollectBestCandidates.Auto())
				{
					long num3 = requiredBytesToClear + 5242880;
					expiredChunks.Clear();
					processedChunkGroups.Clear();
					foreach (KeyValuePair<long, ulong> sortedCullingCandidate in sortedCullingCandidates)
					{
						if (num2 >= num3 || (num2 >= requiredBytesToClear && sortedCullingCandidate.Value >= 549755813888L))
						{
							break;
						}
						if (groupsByChunkKey.TryGetValue(sortedCullingCandidate.Key, out var value2))
						{
							if (processedChunkGroups.Contains(value2))
							{
								continue;
							}
							foreach (long item in value2)
							{
								if (chunksInSaveDir.ContainsKey(item))
								{
									expiredChunks.Add(item);
									int chunkByteCount = GetChunkByteCount(item);
									num2 += chunkByteCount;
								}
							}
							processedChunkGroups.Add(value2);
						}
						else
						{
							expiredChunks.Add(sortedCullingCandidate.Key);
							int chunkByteCount2 = GetChunkByteCount(sortedCullingCandidate.Key);
							num2 += chunkByteCount2;
						}
					}
				}
				using (s_ForceCullChunks.Auto())
				{
					RemoveChunks(expiredChunks);
					expiredChunks.Clear();
				}
				long num4 = totalByteCount - regionFileAccess.GetTotalByteCount(saveDirectory);
				if (num4 < requiredBytesToClear)
				{
					Debug.LogError("Failed to clear as much space as requested." + $"\nCleared: {num4} ({(float)num4 / 1048576f:0.00} MB)." + $"\nRequested: {requiredBytesToClear} ({(float)requiredBytesToClear / 1048576f:0.00} MB).");
					return false;
				}
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetChunkByteCount(long cullingCandidate)
	{
		int chunkX = WorldChunkCache.extractX(cullingCandidate);
		int chunkZ = WorldChunkCache.extractZ(cullingCandidate);
		return GetChunkByteCount(chunkX, chunkZ);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetChunkByteCount(int chunkX, int chunkZ)
	{
		return regionFileAccess.GetChunkByteCount(saveDirectory, chunkX, chunkZ);
	}

	public void MainThreadCacheProtectedPositions()
	{
		if (!ThreadManager.IsMainThread())
		{
			Debug.LogError("RegionFileManager.MainThreadCacheProtectedPositions called from a secondary thread.");
			return;
		}
		using (s_UpdateProtectionLevels.Auto())
		{
			lock (ppdPositionCache)
			{
				ppdPositionCache.ClearAll();
				double num = (double)GameStats.GetInt(EnumGameStats.LandClaimExpiryTime) * 24.0;
				double num2 = (double)GameStats.GetInt(EnumGameStats.BedrollExpiryTime) * 24.0;
				foreach (PersistentPlayerData value in ((IDictionary<PlatformUserIdentifierAbs, PersistentPlayerData>)GameManager.Instance.GetPersistentPlayerList().Players).Values)
				{
					if (value.HasBedrollPos && value.OfflineHours < num2)
					{
						ppdPositionCache.bedrolls.Add(value.BedrollPos);
					}
					if (value.OfflineHours < num)
					{
						foreach (Vector3i landProtectionBlock in value.GetLandProtectionBlocks())
						{
							ppdPositionCache.lpBlocks.Add(landProtectionBlock);
						}
					}
					if (value.EntityId == -1)
					{
						ppdPositionCache.offlinePlayers.Add(value.Position);
					}
					if (value.OfflineHours < num)
					{
						value.ProcessBackpacks([PublicizedFrom(EAccessModifier.Private)] (PersistentPlayerData.ProtectedBackpack backpack) =>
						{
							ppdPositionCache.backpacks.Add(backpack.Position);
						});
					}
					foreach (QuestPositionData questPosition in value.QuestPositions)
					{
						ppdPositionCache.quests.Add(questPosition.blockPosition);
					}
					foreach (Vector3i ownedVendingMachinePosition in value.OwnedVendingMachinePositions)
					{
						ppdPositionCache.vendingMachines.Add(ownedVendingMachinePosition);
					}
				}
				foreach (var vehiclePositions in VehicleManager.Instance.GetVehiclePositionsList())
				{
					ppdPositionCache.vehicles.Add(World.worldToBlockPos(vehiclePositions.position));
				}
				foreach (var dronePositions in DroneManager.Instance.GetDronePositionsList())
				{
					ppdPositionCache.drones.Add(World.worldToBlockPos(dronePositions.position));
				}
				foreach (AIDirectorAirDropComponent.SupplyCrateCache supplyCrate in GameManager.Instance.World.aiDirector.GetComponent<AIDirectorAirDropComponent>().supplyCrates)
				{
					ppdPositionCache.supplyCrates.Add(supplyCrate.blockPos);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateChunkProtectionLevels()
	{
		using (s_UpdateProtectionLevels.Auto())
		{
			chunkProtectionLevels.Clear();
			groupProtectionLevels.Clear();
			EvaluateSingleChunkProtectionLevels();
			foreach (KeyValuePair<HashSetLong, ChunkProtectionLevel> groupProtectionLevel in groupProtectionLevels)
			{
				foreach (long item in groupProtectionLevel.Key)
				{
					if (chunkProtectionLevels.TryGetValue(item, out var value) && value != (value & groupProtectionLevel.Value))
					{
						Log.Error($"Error in chunk group protection: member has protections not accounted for by the group. chunkKey: {item}, protectionLevel: {value}, groupProtectionLevel: {groupProtectionLevel.Value})");
					}
					chunkProtectionLevels[item] = groupProtectionLevel.Value;
				}
			}
			protectionLevelsDirty = false;
			s_ProtectedChunkCount.Value = chunkProtectionLevels.Count;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void AddChunksInWorldArea(Vector3i centerPos, int halfSize, ChunkProtectionLevel innerProtectionLevel, int margin, ChunkProtectionLevel marginProtectionLevel)
		{
			int num = World.toChunkXZ(centerPos.x - halfSize);
			int num2 = World.toChunkXZ(centerPos.x + halfSize);
			int num3 = World.toChunkXZ(centerPos.z - halfSize);
			int num4 = World.toChunkXZ(centerPos.z + halfSize);
			for (int i = num - margin; i <= num2 + margin; i++)
			{
				for (int j = num3 - margin; j <= num4 + margin; j++)
				{
					ChunkProtectionLevel protectionLevel = ((i < num || i > num2 || j < num3 || j > num4) ? marginProtectionLevel : innerProtectionLevel);
					long chunkKey = WorldChunkCache.MakeChunkKey(i, j);
					AddProtectionLevel(chunkKey, protectionLevel);
				}
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void AddProtectionLevel(long chunkKey, ChunkProtectionLevel protectionLevel)
		{
			if (chunkProtectionLevels.TryGetValue(chunkKey, out var value2))
			{
				if (protectionLevel == (protectionLevel & value2))
				{
					return;
				}
				chunkProtectionLevels[chunkKey] = value2 | protectionLevel;
			}
			else
			{
				chunkProtectionLevels[chunkKey] = protectionLevel;
			}
			if (groupsByChunkKey.TryGetValue(chunkKey, out var value3))
			{
				if (groupProtectionLevels.TryGetValue(value3, out var value4))
				{
					groupProtectionLevels[value3] = value4 | protectionLevel;
				}
				else
				{
					groupProtectionLevels[value3] = protectionLevel;
				}
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void EvaluateSingleChunkProtectionLevels()
		{
			int halfSize = GamePrefs.GetInt(EnumGamePrefs.LandClaimSize) / 2;
			int halfSize2 = GamePrefs.GetInt(EnumGamePrefs.BedrollDeadZoneSize);
			ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
			if (chunkCache != null)
			{
				chunkCache.GetSyncRoot().EnterReadLock();
				foreach (long chunkKey2 in chunkCache.chunkKeys)
				{
					AddProtectionLevel(chunkKey2, ChunkProtectionLevel.CurrentlySynced);
				}
				chunkCache.GetSyncRoot().ExitReadLock();
			}
			GameManager.Instance.World.m_ChunkManager.ProcessChunksPendingUnload([PublicizedFrom(EAccessModifier.Private)] (Chunk chunk) =>
			{
				AddProtectionLevel(chunk.Key, ChunkProtectionLevel.CurrentlySynced);
			});
			lock (chunksToSave)
			{
				foreach (long key in chunksToSave.Keys)
				{
					AddProtectionLevel(key, ChunkProtectionLevel.CurrentlySynced);
				}
			}
			lock (chunkMemoryStreamsToSave)
			{
				foreach (long key2 in chunkMemoryStreamsToSave.Keys)
				{
					AddProtectionLevel(key2, ChunkProtectionLevel.CurrentlySynced);
				}
			}
			foreach (HashSetLong value5 in chunksByTraderID.Values)
			{
				using HashSetLong.Enumerator enumerator3 = value5.GetEnumerator();
				if (enumerator3.MoveNext())
				{
					long current6 = enumerator3.Current;
					AddProtectionLevel(current6, ChunkProtectionLevel.Trader);
				}
			}
			lock (ppdPositionCache)
			{
				ProcessProtectedPositionList(ppdPositionCache.bedrolls, halfSize2, ChunkProtectionLevel.Bedroll, 1, ChunkProtectionLevel.NearBedroll);
				ProcessProtectedPositionList(ppdPositionCache.lpBlocks, halfSize, ChunkProtectionLevel.LandClaim, 1, ChunkProtectionLevel.NearLandClaim);
				ProcessProtectedPositionList(ppdPositionCache.offlinePlayers, 0, ChunkProtectionLevel.OfflinePlayer, 1, ChunkProtectionLevel.NearOfflinePlayer);
				ProcessProtectedPositionList(ppdPositionCache.backpacks, 0, ChunkProtectionLevel.DroppedBackpack, 1, ChunkProtectionLevel.NearDroppedBackpack);
				ProcessProtectedPositionList(ppdPositionCache.quests, halfSize, ChunkProtectionLevel.QuestObjective, 1, ChunkProtectionLevel.NearQuestObjective);
				ProcessProtectedPositionList(ppdPositionCache.vendingMachines, halfSize, ChunkProtectionLevel.Trader, 1, ChunkProtectionLevel.Trader);
				ProcessProtectedPositionList(ppdPositionCache.drones, halfSize, ChunkProtectionLevel.Drone, 1, ChunkProtectionLevel.Drone);
				ProcessProtectedPositionList(ppdPositionCache.vehicles, halfSize, ChunkProtectionLevel.Vehicle, 1, ChunkProtectionLevel.NearVehicle);
				ProcessProtectedPositionList(ppdPositionCache.supplyCrates, halfSize, ChunkProtectionLevel.SupplyCrate, 1, ChunkProtectionLevel.NearSupplyCrate);
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void ProcessProtectedPositionList(List<Vector3i> centerPositions, int halfSize, ChunkProtectionLevel innerProtectionLevel, int margin, ChunkProtectionLevel marginProtectionLevel)
		{
			foreach (Vector3i centerPosition in centerPositions)
			{
				AddChunksInWorldArea(centerPosition, halfSize, innerProtectionLevel, margin, marginProtectionLevel);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool DoSaveChunks()
	{
		IRegionFileChunkSnapshot regionFileChunkSnapshot = null;
		bool result;
		try
		{
			lock (chunksToUnloadLater)
			{
				for (int num = chunksToUnloadLater.Count - 1; num >= 0; num--)
				{
					if (!chunksToUnloadLater[num].IsLockedExceptUnloading)
					{
						MemoryPools.PoolChunks.FreeSync(chunksToUnloadLater[num]);
						chunksToUnloadLater.RemoveAt(num);
					}
				}
			}
			protectionLevelsDirty = true;
			CullExpiredChunks();
			if (chunkMemoryStreamsToSave.Count == 0 && chunksToSave.Count == 0)
			{
				result = false;
				goto IL_0306;
			}
			long num2 = 0L;
			lock (chunkMemoryStreamsToSave)
			{
				if (chunkMemoryStreamsToSave.Count > 0)
				{
					using Dictionary<long, IRegionFileChunkSnapshot>.Enumerator enumerator = chunkMemoryStreamsToSave.GetEnumerator();
					if (enumerator.MoveNext())
					{
						num2 = enumerator.Current.Key;
						regionFileChunkSnapshot = chunkMemoryStreamsToSave[num2];
						chunkMemoryStreamsToSave.Remove(num2);
					}
				}
			}
			if (regionFileChunkSnapshot == null)
			{
				Chunk chunk = null;
				lock (chunksToSave)
				{
					using Dictionary<long, Chunk>.Enumerator enumerator2 = chunksToSave.GetEnumerator();
					if (enumerator2.MoveNext())
					{
						KeyValuePair<long, Chunk> current = enumerator2.Current;
						chunk = chunksToSave[current.Key];
						chunksToSave.Remove(current.Key);
						chunkKeyCurrentlySaved = current.Key;
					}
				}
				if (chunk != null)
				{
					num2 = chunk.Key;
					regionFileChunkSnapshot = snapshotUtil.TakeSnapshot(chunk, saveIfUnchanged: false);
					chunk.InProgressSaving = false;
					if (chunk.IsLockedExceptUnloading)
					{
						lock (chunksToUnloadLater)
						{
							chunksToUnloadLater.Add(chunk);
						}
					}
					else
					{
						MemoryPools.PoolChunks.FreeSync(chunk);
					}
				}
			}
			if (regionFileChunkSnapshot != null)
			{
				int chunkX = WorldChunkCache.extractX(num2);
				int chunkZ = WorldChunkCache.extractZ(num2);
				MakeRoomForChunk(regionFileChunkSnapshot.Size, num2);
				snapshotUtil.WriteSnapshot(regionFileChunkSnapshot, saveDirectory, chunkX, chunkZ);
				lock (chunksInSaveDir)
				{
					uint timeStamp = GameUtils.WorldTimeToTotalMinutes(GameManager.Instance.World.worldTime);
					SetChunkTimestamp(num2, timeStamp);
					chunkKeyCurrentlySaved = long.MaxValue;
				}
				s_RegionDataSize.Value = regionFileAccess.GetTotalByteCount(saveDirectory);
			}
			MultiBlockManager.Instance.SaveIfDirty();
		}
		catch (Exception ex)
		{
			Log.Error("ERROR: " + ex);
			result = false;
			goto IL_0306;
		}
		finally
		{
			if (regionFileChunkSnapshot != null)
			{
				snapshotUtil.Free(regionFileChunkSnapshot);
			}
		}
		return true;
		IL_0306:
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int thread_SaveChunks(ThreadManager.ThreadInfo _taskInfo)
	{
		if (bSaveRunning)
		{
			if (thread_SaveChunks_SleepCount >= 40 || saveThreadWaitHandle.WaitOne(0))
			{
				while (true)
				{
					lock (saveLock)
					{
						if (!DoSaveChunks())
						{
							break;
						}
					}
				}
				thread_SaveChunks_SleepCount = 0;
			}
			return 5;
		}
		return -1;
	}

	public override Chunk GetChunkSync(long _key)
	{
		Chunk chunk = null;
		bool flag = false;
		if (base.ContainsChunkSync(_key))
		{
			chunk = base.GetChunkSync(_key);
			flag = true;
		}
		IRegionFileChunkSnapshot snapshot = null;
		if (chunk == null)
		{
			lock (chunkMemoryStreamsToSave)
			{
				if (chunkMemoryStreamsToSave.ContainsKey(_key))
				{
					snapshot = chunkMemoryStreamsToSave[_key];
					chunkMemoryStreamsToSave.Remove(_key);
				}
			}
			snapshotUtil.Free(snapshot);
		}
		if (chunk == null)
		{
			lock (chunksToSave)
			{
				if (chunksToSave.ContainsKey(_key))
				{
					chunk = chunksToSave[_key];
					chunksToSave.Remove(_key);
					chunk.OnLoadedFromCache();
				}
			}
		}
		if (chunk == null)
		{
			bool flag2 = false;
			do
			{
				lock (chunksToSave)
				{
					flag2 = _key == chunkKeyCurrentlySaved;
				}
				if (flag2)
				{
					Thread.Sleep(5);
				}
			}
			while (flag2);
		}
		if (chunk == null)
		{
			if (isChunkInSaveDir(_key))
			{
				chunk = snapshotUtil.LoadChunk(saveDirectory, _key);
				if (chunk == null)
				{
					lock (chunksInSaveDir)
					{
						chunksInSaveDir.Remove(_key);
					}
				}
			}
			else if (isChunkInLoadDir(_key))
			{
				chunk = snapshotUtil.LoadChunk(loadDirectory, _key);
				if (chunk == null)
				{
					chunksInLoadDir.Remove(_key);
				}
			}
		}
		if (chunk != null && !flag && maxChunksInCache > 0)
		{
			cacheChunk(chunk);
		}
		return chunk;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cacheChunk(Chunk _chunk)
	{
		if (maxChunksInCache > 0)
		{
			base.AddChunkSync(_chunk);
			long key;
			lock (chunksInLocalCache)
			{
				chunksInLocalCache.Add(_chunk.Key);
				if (chunksInLocalCache.Count <= maxChunksInCache)
				{
					return;
				}
				key = chunksInLocalCache[0];
			}
			_chunk = base.GetChunkSync(key);
			RemoveChunkSync(key);
		}
		if (bSaveOnChunkDrop && _chunk.NeedsSaving && !_chunk.InProgressSaving)
		{
			_chunk.InProgressSaving = true;
			lock (chunksToSave)
			{
				if (!chunksToSave.ContainsKey(_chunk.Key))
				{
					chunksToSave.Add(_chunk.Key, _chunk);
					startSavingTask();
				}
				return;
			}
		}
		if (_chunk.IsLockedExceptUnloading)
		{
			lock (chunksToUnloadLater)
			{
				chunksToUnloadLater.Add(_chunk);
				return;
			}
		}
		MemoryPools.PoolChunks.FreeSync(_chunk);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startSavingTask()
	{
		saveThreadWaitHandle.Set();
	}

	public override bool AddChunkSync(Chunk _chunk, bool _bOmitCallbacks = false)
	{
		if (!GameManager.bSavingActive || _chunk.NeedsDecoration)
		{
			MemoryPools.PoolChunks.FreeSync(_chunk);
			return false;
		}
		if (!base.ContainsChunkSync(_chunk.Key))
		{
			cacheChunk(_chunk);
		}
		return true;
	}

	public override bool ContainsChunkSync(long key)
	{
		if (base.ContainsChunkSync(key))
		{
			return true;
		}
		lock (chunksToSave)
		{
			if (chunksToSave.ContainsKey(key))
			{
				return true;
			}
			if (chunkKeyCurrentlySaved == key)
			{
				return true;
			}
		}
		if (!isChunkInSaveDir(key))
		{
			return isChunkInLoadDir(key);
		}
		return true;
	}

	public bool IsChunkSavedAndDormant(long key)
	{
		lock (chunksInSaveDir)
		{
			if (!chunksInSaveDir.ContainsKey(key))
			{
				return false;
			}
			if (base.ContainsChunkSync(key))
			{
				return false;
			}
			lock (chunksToSave)
			{
				if (chunksToSave.ContainsKey(key))
				{
					return false;
				}
				if (chunkKeyCurrentlySaved == key)
				{
					return false;
				}
			}
			return true;
		}
	}

	public override void RemoveChunkSync(long key)
	{
		if (base.ContainsChunkSync(key))
		{
			base.RemoveChunkSync(key);
		}
		lock (chunksInLocalCache)
		{
			chunksInLocalCache.Remove(key);
		}
	}

	public int MakePersistent(ChunkCluster _mainCache, bool _bSaveEvenIfUnchanged)
	{
		List<Chunk> list = new List<Chunk>();
		List<Chunk> chunkArrayCopySync = GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk chunk = chunkArrayCopySync[i];
			if (chunk.NeedsDecoration || (!chunk.NeedsSaving && !_bSaveEvenIfUnchanged))
			{
				continue;
			}
			lock (chunksToSave)
			{
				if (chunksToSave.ContainsKey(chunk.Key))
				{
					continue;
				}
			}
			chunk.InProgressSaving = true;
			list.Add(chunk);
		}
		if (_mainCache != null)
		{
			List<Chunk> chunkArrayCopySync2 = _mainCache.GetChunkArrayCopySync();
			for (int j = 0; j < chunkArrayCopySync2.Count; j++)
			{
				Chunk chunk2 = chunkArrayCopySync2[j];
				lock (chunk2)
				{
					if (chunk2.InProgressUnloading)
					{
						continue;
					}
					chunk2.InProgressSaving = true;
					goto IL_00d5;
				}
				IL_00d5:
				_mainCache.NotifyOnChunkBeforeSave(chunk2);
				if (!chunk2.NeedsDecoration && (chunk2.NeedsSaving || _bSaveEvenIfUnchanged))
				{
					list.Add(chunk2);
				}
				else
				{
					chunk2.InProgressSaving = false;
				}
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			SaveChunkSnapshot(list[k], _bSaveEvenIfUnchanged);
			list[k].InProgressSaving = false;
		}
		startSavingTask();
		return list.Count;
	}

	public void WaitSaveDone()
	{
		MicroStopwatch microStopwatch = new MicroStopwatch();
		int count = chunkMemoryStreamsToSave.Count;
		int num = 0;
		do
		{
			lock (chunkMemoryStreamsToSave)
			{
				num = chunkMemoryStreamsToSave.Count;
			}
			lock (chunksToSave)
			{
				num += chunksToSave.Count;
			}
			Thread.Sleep(20);
		}
		while (num > 0);
		ProcessPendingPersistentDataRemovals();
		Log.Out("Saving " + count + " of chunks took " + microStopwatch.ElapsedMilliseconds + "ms");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isChunkInLoadDir(long _key)
	{
		return chunksInLoadDir.Contains(_key);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isChunkInSaveDir(long _key)
	{
		lock (chunksInSaveDir)
		{
			return chunksInSaveDir.ContainsKey(_key);
		}
	}

	public virtual void SaveChunkSnapshot(Chunk _chunk, bool _saveIfUnchanged)
	{
		if (saveDirectory == null)
		{
			return;
		}
		lock (chunkMemoryStreamsToSave)
		{
			if (chunkMemoryStreamsToSave.TryGetValue(_chunk.Key, out var value))
			{
				value.Update(_chunk, _saveIfUnchanged);
			}
			else
			{
				chunkMemoryStreamsToSave.Add(_chunk.Key, snapshotUtil.TakeSnapshot(_chunk, _saveIfUnchanged));
			}
		}
		startSavingTask();
	}

	public long[] GetAllChunkKeys()
	{
		HashSetLong hashSetLong = new HashSetLong();
		foreach (long item in chunksInLoadDir)
		{
			hashSetLong.Add(item);
		}
		lock (chunksInSaveDir)
		{
			foreach (long key in chunksInSaveDir.Keys)
			{
				hashSetLong.Add(key);
			}
		}
		long[] array = new long[hashSetLong.Count];
		hashSetLong.CopyTo(array);
		return array;
	}

	public HashSetLong GetUniqueChunkKeys(ChunkProtectionLevel excludedProtectionLevels)
	{
		HashSetLong hashSetLong = new HashSetLong();
		foreach (long item in chunksInLoadDir)
		{
			if (!chunkProtectionLevels.TryGetValue(item, out var value) || (value & excludedProtectionLevels) == 0)
			{
				hashSetLong.Add(item);
			}
		}
		lock (chunksInSaveDir)
		{
			foreach (long key in chunksInSaveDir.Keys)
			{
				if (!chunkProtectionLevels.TryGetValue(key, out var value2) || (value2 & excludedProtectionLevels) == 0)
				{
					hashSetLong.Add(key);
				}
			}
			return hashSetLong;
		}
	}

	public override void Clear()
	{
		base.Clear();
	}

	public void ClearCaches()
	{
		regionFileAccess.ClearCache();
	}

	public void RemoveChunks(ICollection<long> _chunks, bool _resetDecos = true)
	{
		if (_chunks.Count == 0)
		{
			return;
		}
		if (Monitor.IsEntered(chunksInSaveDir) && !Monitor.IsEntered(saveLock))
		{
			Debug.LogError("RemoveChunks failed. Thread safety violation: the lock on \"saveLock\" must be taken before the lock on \"chunksInSaveDir\" to avoid a possible deadlock.");
			return;
		}
		lock (saveLock)
		{
			lock (chunksInSaveDir)
			{
				RemovePersistentDataForChunks(_chunks);
				resetVolumeDataForChunks(_chunks);
				foreach (long _chunk in _chunks)
				{
					RemoveChunk(_chunk, _resetDecos);
				}
				MultiBlockManager.Instance.CullChunklessData();
				lock (pendingChunkDeletionPackages)
				{
					NetPackageDeleteChunkData item = NetPackageManager.GetPackage<NetPackageDeleteChunkData>().Setup(_chunks);
					pendingChunkDeletionPackages.Add(item);
				}
				ThreadManager.AddSingleTaskMainThread("RemoveChunks.ProcessChunkDeletionPackages", [PublicizedFrom(EAccessModifier.Private)] (object _003Cp0_003E) =>
				{
					ProcessChunkDeletionPackages();
				});
				regionFileAccess.OptimizeLayouts();
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void RemoveChunk(long _key, bool flag)
		{
			using (s_RemoveChunk.Auto())
			{
				lock (chunkMemoryStreamsToSave)
				{
					chunkMemoryStreamsToSave.Remove(_key);
				}
				lock (chunksToSave)
				{
					chunksToSave.Remove(_key);
				}
				int chunkX = WorldChunkCache.extractX(_key);
				int chunkZ = WorldChunkCache.extractZ(_key);
				regionFileAccess.Remove(saveDirectory, chunkX, chunkZ);
				RemoveChunkSync(_key);
				chunksInLoadDir.Remove(_key);
				chunksInSaveDir.Remove(_key);
				if (flag)
				{
					DecoManager.Instance.ResetDecosForWorldChunk(_key);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessChunkDeletionPackages()
	{
		lock (pendingChunkDeletionPackages)
		{
			foreach (NetPackageDeleteChunkData pendingChunkDeletionPackage in pendingChunkDeletionPackages)
			{
				DynamicMeshUnity.DeleteDynamicMeshData(pendingChunkDeletionPackage.chunkKeys);
				WaterSimulationNative.Instance.changeApplier.DiscardChangesForChunks(pendingChunkDeletionPackage.chunkKeys);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(pendingChunkDeletionPackage);
			}
			pendingChunkDeletionPackages.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemovePersistentDataForChunks(ICollection<long> _chunks)
	{
		if (_chunks.Count == 0)
		{
			return;
		}
		if (!ThreadManager.IsMainThread())
		{
			lock (persistentDataRemovalsRequestedChunks)
			{
				foreach (long _chunk in _chunks)
				{
					persistentDataRemovalsRequestedChunks.Add(_chunk);
				}
				return;
			}
		}
		if (GameManager.Instance.World == null)
		{
			Log.Error("RemovePersistentDataForChunks world is null");
			return;
		}
		foreach (PersistentPlayerData value in GameManager.Instance.persistentPlayers.Players.Values)
		{
			bool backpacksChanged = false;
			value.RemoveBackpacks([PublicizedFrom(EAccessModifier.Internal)] (PersistentPlayerData.ProtectedBackpack backpack) =>
			{
				long num3 = WorldChunkCache.MakeChunkKey(World.toChunkXZ(backpack.Position.x), World.toChunkXZ(backpack.Position.z));
				foreach (long _chunk2 in _chunks)
				{
					if (_chunk2 == num3)
					{
						backpacksChanged = true;
						return true;
					}
				}
				return false;
			});
			if (!backpacksChanged || value.EntityId == -1)
			{
				continue;
			}
			EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(value.EntityId) as EntityPlayer;
			if (entityPlayer != null)
			{
				if (!entityPlayer.isEntityRemote)
				{
					entityPlayer.SetDroppedBackpackPositions(value.GetDroppedBackpackPositions());
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerSetBackpackPosition>().Setup(value.EntityId, value.GetDroppedBackpackPositions()), _onlyClientsAttachedToAnEntity: false, value.EntityId);
				}
			}
		}
		foreach (var vehiclePositions in VehicleManager.Instance.GetVehiclePositionsList())
		{
			Vector3i vector3i = World.worldToBlockPos(vehiclePositions.position);
			long num = WorldChunkCache.MakeChunkKey(World.toChunkXZ(vector3i.x), World.toChunkXZ(vector3i.z));
			foreach (long _chunk3 in _chunks)
			{
				if (_chunk3 == num)
				{
					VehicleManager.Instance.RemoveUnloadedVehicle(vehiclePositions.entityId);
				}
			}
		}
		AIDirectorAirDropComponent component = GameManager.Instance.World.aiDirector.GetComponent<AIDirectorAirDropComponent>();
		List<int> list = new List<int>();
		foreach (AIDirectorAirDropComponent.SupplyCrateCache supplyCrate in component.supplyCrates)
		{
			long num2 = WorldChunkCache.MakeChunkKey(World.toChunkXZ(supplyCrate.blockPos.x), World.toChunkXZ(supplyCrate.blockPos.z));
			foreach (long _chunk4 in _chunks)
			{
				if (_chunk4 == num2)
				{
					list.Add(supplyCrate.entityId);
				}
			}
		}
		foreach (int item3 in list)
		{
			component.RemoveSupplyCrate(item3);
		}
		List<PowerItem> list2 = new List<PowerItem>();
		PowerManager.Instance.FindPowerItems([PublicizedFrom(EAccessModifier.Internal)] (PowerItem item) =>
		{
			long item2 = WorldChunkCache.MakeChunkKey(World.toChunkXZ(item.Position.x), World.toChunkXZ(item.Position.z));
			return _chunks.Contains(item2);
		}, list2);
		foreach (PowerItem item4 in list2)
		{
			PowerManager.Instance.RemovePowerNode(item4);
		}
	}

	public void CheckPersistentData()
	{
		using (s_CheckPersistentData.Auto())
		{
			foreach (PersistentPlayerData value in GameManager.Instance.persistentPlayers.Players.Values)
			{
				value.RemoveBackpacks([PublicizedFrom(EAccessModifier.Private)] (PersistentPlayerData.ProtectedBackpack backpack) =>
				{
					long key3 = WorldChunkCache.MakeChunkKey(World.toChunkXZ(backpack.Position.x), World.toChunkXZ(backpack.Position.z));
					if (!ContainsChunkSync(key3))
					{
						Log.Out($"Removing backpack at ({backpack.Position}) as the chunk data no longer exists");
						return true;
					}
					return false;
				});
			}
			foreach (var vehiclePositions in VehicleManager.Instance.GetVehiclePositionsList())
			{
				Vector3i vector3i = World.worldToBlockPos(vehiclePositions.position);
				long key = WorldChunkCache.MakeChunkKey(World.toChunkXZ(vector3i.x), World.toChunkXZ(vector3i.z));
				if (!ContainsChunkSync(key))
				{
					Log.Out($"Removing vehicle at ({vector3i}) as the chunk data no longer exists");
					VehicleManager.Instance.RemoveUnloadedVehicle(vehiclePositions.entityId);
				}
			}
			AIDirectorAirDropComponent component = GameManager.Instance.World.aiDirector.GetComponent<AIDirectorAirDropComponent>();
			List<int> list = new List<int>();
			foreach (AIDirectorAirDropComponent.SupplyCrateCache supplyCrate in component.supplyCrates)
			{
				long key2 = WorldChunkCache.MakeChunkKey(World.toChunkXZ(supplyCrate.blockPos.x), World.toChunkXZ(supplyCrate.blockPos.z));
				if (!ContainsChunkSync(key2))
				{
					Log.Out($"Removing air drop at ({supplyCrate.blockPos}) as the chunk data no longer exists");
					list.Add(supplyCrate.entityId);
				}
			}
			foreach (int item in list)
			{
				component.RemoveSupplyCrate(item);
			}
			List<PowerItem> list2 = new List<PowerItem>();
			PowerManager.Instance.FindPowerItems([PublicizedFrom(EAccessModifier.Private)] (PowerItem item) =>
			{
				long key3 = WorldChunkCache.MakeChunkKey(World.toChunkXZ(item.Position.x), World.toChunkXZ(item.Position.z));
				return !ContainsChunkSync(key3);
			}, list2);
			foreach (PowerItem item2 in list2)
			{
				Log.Out($"Removing power item at ({item2.Position}) as the chunk data no longer exists");
				PowerManager.Instance.RemovePowerNode(item2);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessPendingPersistentDataRemovals()
	{
		using (s_ProcessPersistentDataRemovals.Auto())
		{
			lock (persistentDataRemovalsRequestedChunks)
			{
				if (persistentDataRemovalsRequestedChunks.Count > 0)
				{
					RemovePersistentDataForChunks(persistentDataRemovalsRequestedChunks);
					persistentDataRemovalsRequestedChunks.Clear();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void resetVolumeDataForChunks(ICollection<long> _chunks)
	{
		World world = GameManager.Instance.World;
		foreach (long _chunk in _chunks)
		{
			world.ResetTriggerVolumes(_chunk);
			world.ResetSleeperVolumes(_chunk);
		}
	}

	public HashSetLong ResetAllChunks(ChunkProtectionLevel excludedProtectionLevels)
	{
		lock (saveLock)
		{
			lock (chunksInSaveDir)
			{
				UpdateChunkProtectionLevels();
				HashSetLong uniqueChunkKeys = GetUniqueChunkKeys(excludedProtectionLevels);
				RemoveChunks(uniqueChunkKeys);
				return uniqueChunkKeys;
			}
		}
	}

	public HashSetLong ResetRegion(int _regionX, int _regionZ, ChunkProtectionLevel excludedProtectionLevels)
	{
		lock (saveLock)
		{
			lock (chunksInSaveDir)
			{
				UpdateChunkProtectionLevels();
				int chunksPerRegionPerDimension = regionFileAccess.ChunksPerRegionPerDimension;
				Vector2i vector2i = new Vector2i(_regionX * chunksPerRegionPerDimension, _regionZ * chunksPerRegionPerDimension);
				Vector2i vector2i2 = vector2i + new Vector2i(chunksPerRegionPerDimension - 1, chunksPerRegionPerDimension - 1);
				HashSetLong hashSetLong = new HashSetLong();
				for (int i = vector2i.x; i <= vector2i2.x; i++)
				{
					for (int j = vector2i.y; j <= vector2i2.y; j++)
					{
						long num = WorldChunkCache.MakeChunkKey(i, j);
						if (ContainsChunkSync(num) && (!chunkProtectionLevels.TryGetValue(num, out var value) || (value & excludedProtectionLevels) == 0))
						{
							hashSetLong.Add(num);
						}
					}
				}
				RemoveChunks(hashSetLong);
				return hashSetLong;
			}
		}
	}

	public void IterateChunkExpiryTimes(Action<long, ulong> action)
	{
		if (maxChunkAge < 0)
		{
			return;
		}
		lock (chunksInSaveDir)
		{
			UpdateChunkProtectionLevels();
			foreach (KeyValuePair<long, uint> item in chunksInSaveDir)
			{
				if (!chunkProtectionLevels.TryGetValue(item.Key, out var _))
				{
					action(item.Key, GameUtils.TotalMinutesToWorldTime(item.Value + (uint)(int)maxChunkAge));
				}
			}
		}
	}

	public void SaveChunkAgeDebugTexture(float rangeInDays)
	{
		float num = rangeInDays * 24f * 60f;
		Vector2i worldSize = GameManager.Instance.World.ChunkCache.ChunkProvider.GetWorldSize();
		Vector2i vector2i = new Vector2i(worldSize.x / 16, worldSize.y / 16);
		int num2 = vector2i.x / 2;
		int num3 = vector2i.y / 2;
		uint num4 = GameUtils.WorldTimeToTotalMinutes(GameManager.Instance.World.worldTime);
		Color32[] array = new Color32[vector2i.x * vector2i.y];
		lock (chunksInSaveDir)
		{
			UpdateChunkProtectionLevels();
			float num5 = 1f / (float)OrdinalProtectionLevel(ChunkProtectionLevel.CurrentlySynced);
			for (int i = 0; i < array.Length; i++)
			{
				int x = i % vector2i.x - num2;
				int y = i / vector2i.x - num3;
				long key = WorldChunkCache.MakeChunkKey(x, y);
				(float, float, float, float) tuple = (0f, 0f, 0f, 0f);
				if (chunksInSaveDir.TryGetValue(key, out var value))
				{
					tuple.Item4 = 1f;
					long num6 = num4 - value;
					tuple.Item2 = (float)num6 / num;
				}
				if (groupsByChunkKey.TryGetValue(key, out var value2) && groupTimestamps.TryGetValue(value2, out var value3))
				{
					long num7 = num4 - value3;
					tuple.Item1 = (float)num7 / num;
				}
				else
				{
					tuple.Item1 = tuple.Item2;
				}
				if (chunkProtectionLevels.TryGetValue(key, out var value4))
				{
					tuple.Item3 = (float)OrdinalProtectionLevel(value4) * num5;
				}
				array[i] = new Color(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
			}
		}
		Texture2D texture2D = new Texture2D(vector2i.x, vector2i.y);
		texture2D.SetPixels32(array);
		texture2D.Apply();
		byte[] bytes = texture2D.EncodeToTGA();
		SdFile.WriteAllBytes("ChunkAgeMap.tga", bytes);
		UnityEngine.Object.Destroy(texture2D);
		[PublicizedFrom(EAccessModifier.Internal)]
		static int OrdinalProtectionLevel(ChunkProtectionLevel level)
		{
			if (level != ChunkProtectionLevel.None)
			{
				return 1 + (int)Math.Log(Convert.ToInt32(level), 2.0);
			}
			return 0;
		}
	}
}
