#define MBM_ENABLE_PROFILER_MARKERS
#define MBM_ENABLED_SANITY_CHECKS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Unity.Profiling;
using UnityEngine;

public class MultiBlockManager
{
	[Flags]
	public enum TrackingTypeFlags : byte
	{
		None = 0,
		PoiMultiBlock = 1,
		CrossChunkMultiBlock = 2,
		OversizedBlock = 4,
		TerrainAlignedBlock = 8,
		All = 0xF
	}

	public struct TrackedBlockData(uint rawData, RectInt flatChunkBounds, TrackingTypeFlags trackingTypeFlags)
	{
		public readonly uint rawData = rawData;

		public readonly RectInt flatChunkBounds = flatChunkBounds;

		public readonly TrackingTypeFlags trackingTypeFlags = trackingTypeFlags;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class TrackedDataMap
	{
		public struct SubsetAccessor(Dictionary<Vector3i, TrackedBlockData> trackedData, HashSet<Vector3i> subset) : IEnumerator<KeyValuePair<Vector3i, TrackedBlockData>>, IEnumerator, IDisposable
		{
			[PublicizedFrom(EAccessModifier.Private)]
			public readonly Dictionary<Vector3i, TrackedBlockData> _trackedData = trackedData;

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly HashSet<Vector3i> _subset = subset;

			[PublicizedFrom(EAccessModifier.Private)]
			public HashSet<Vector3i>.Enumerator _subsetEnumerator = subset.GetEnumerator();

			public KeyValuePair<Vector3i, TrackedBlockData> Current => new KeyValuePair<Vector3i, TrackedBlockData>(_subsetEnumerator.Current, _trackedData[_subsetEnumerator.Current]);

			public int Count => _subset.Count;

			object IEnumerator.Current
			{
				[PublicizedFrom(EAccessModifier.Private)]
				get
				{
					return Current;
				}
			}

			public TrackedBlockData this[Vector3i key]
			{
				get
				{
					if (!_subset.Contains(key))
					{
						throw new KeyNotFoundException($"The key \"{key}\" was not found in the subset.");
					}
					return _trackedData[key];
				}
			}

			public bool MoveNext()
			{
				return _subsetEnumerator.MoveNext();
			}

			public void Reset()
			{
				throw new NotSupportedException();
			}

			public void Dispose()
			{
				_subsetEnumerator.Dispose();
			}

			public bool ContainsKey(Vector3i key)
			{
				return _subset.Contains(key);
			}

			public bool TryGetValue(Vector3i key, out TrackedBlockData value)
			{
				if (_subset.Contains(key) && _trackedData.TryGetValue(key, out value))
				{
					return true;
				}
				value = default(TrackedBlockData);
				return false;
			}

			public SubsetAccessor GetEnumerator()
			{
				return this;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<Vector3i, TrackedBlockData> trackedDataByPosition = new Dictionary<Vector3i, TrackedBlockData>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<Vector3i> poiMultiBlocks = new HashSet<Vector3i>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<Vector3i> crossChunkMultiBlocks = new HashSet<Vector3i>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<Vector3i> oversizedBlocks = new HashSet<Vector3i>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<Vector3i> terrainAlignedBlocks = new HashSet<Vector3i>();

		public readonly ReadOnlyDictionary<Vector3i, TrackedBlockData> TrackedDataByPosition;

		public int Count => trackedDataByPosition.Count;

		public SubsetAccessor PoiMultiBlocks => new SubsetAccessor(trackedDataByPosition, poiMultiBlocks);

		public SubsetAccessor CrossChunkMultiBlocks => new SubsetAccessor(trackedDataByPosition, crossChunkMultiBlocks);

		public SubsetAccessor OversizedBlocks => new SubsetAccessor(trackedDataByPosition, oversizedBlocks);

		public SubsetAccessor TerrainAlignedBlocks => new SubsetAccessor(trackedDataByPosition, terrainAlignedBlocks);

		public TrackedDataMap()
		{
			TrackedDataByPosition = new ReadOnlyDictionary<Vector3i, TrackedBlockData>(trackedDataByPosition);
		}

		public bool ContainsKey(Vector3i key)
		{
			return trackedDataByPosition.ContainsKey(key);
		}

		public bool TryGetValue(Vector3i key, out TrackedBlockData value)
		{
			return trackedDataByPosition.TryGetValue(key, out value);
		}

		public void Clear()
		{
			trackedDataByPosition.Clear();
			poiMultiBlocks.Clear();
			crossChunkMultiBlocks.Clear();
			oversizedBlocks.Clear();
			terrainAlignedBlocks.Clear();
		}

		public void AddOrMergeTrackedData(Vector3i worldPos, uint rawData, RectInt flatChunkBounds, TrackingTypeFlags trackingTypeFlags)
		{
			if (trackingTypeFlags == TrackingTypeFlags.None)
			{
				UnityEngine.Debug.LogError($"AddOrMergeTrackedData failed: Cannot add or merge tracked data with no tracking flags set. At position {worldPos}.");
				return;
			}
			TrackingTypeFlags trackingTypeFlags2;
			if (trackedDataByPosition.TryGetValue(worldPos, out var value))
			{
				if (rawData != value.rawData)
				{
					UnityEngine.Debug.LogWarning($"Unexpected condition in AddOrMergeTrackedData: encountered raw data mismatch at position {worldPos}.");
				}
				RectInt rectInt = flatChunkBounds;
				if (!rectInt.Equals(value.flatChunkBounds))
				{
					rectInt.SetMinMax(Vector2Int.Min(rectInt.min, value.flatChunkBounds.min), Vector2Int.Max(rectInt.max, value.flatChunkBounds.max));
					UnityEngine.Debug.Log($"Expanding FlatChunkBounds at position {worldPos}. Old bounds: {value.flatChunkBounds}. New bounds: {flatChunkBounds}. Merged bounds: {rectInt}.");
				}
				trackingTypeFlags2 = (TrackingTypeFlags)((uint)trackingTypeFlags & (uint)(byte)(~(int)value.trackingTypeFlags));
				if (trackingTypeFlags2 != trackingTypeFlags)
				{
					UnityEngine.Debug.LogWarning($"Unexpected condition in AddOrMergeTrackedData: tracked data already has one or more target flag(s) set at position {worldPos}.");
				}
				TrackingTypeFlags trackingTypeFlags3 = trackingTypeFlags | value.trackingTypeFlags;
				trackedDataByPosition[worldPos] = new TrackedBlockData(rawData, rectInt, trackingTypeFlags3);
			}
			else
			{
				trackingTypeFlags2 = trackingTypeFlags;
				trackedDataByPosition[worldPos] = new TrackedBlockData(rawData, flatChunkBounds, trackingTypeFlags);
			}
			if (trackingTypeFlags2 != TrackingTypeFlags.None)
			{
				if ((trackingTypeFlags2 & TrackingTypeFlags.PoiMultiBlock) != TrackingTypeFlags.None)
				{
					poiMultiBlocks.Add(worldPos);
				}
				if ((trackingTypeFlags2 & TrackingTypeFlags.CrossChunkMultiBlock) != TrackingTypeFlags.None)
				{
					crossChunkMultiBlocks.Add(worldPos);
				}
				if ((trackingTypeFlags2 & TrackingTypeFlags.OversizedBlock) != TrackingTypeFlags.None)
				{
					oversizedBlocks.Add(worldPos);
				}
				if ((trackingTypeFlags2 & TrackingTypeFlags.TerrainAlignedBlock) != TrackingTypeFlags.None)
				{
					terrainAlignedBlocks.Add(worldPos);
				}
			}
		}

		public void RemoveTrackedData(Vector3i worldPos, TrackingTypeFlags flagsToRemove)
		{
			if (!trackedDataByPosition.TryGetValue(worldPos, out var value))
			{
				UnityEngine.Debug.LogError($"RemoveTrackedData failed; no tracked data at position {worldPos}.");
				return;
			}
			TrackingTypeFlags trackingTypeFlags = flagsToRemove & value.trackingTypeFlags;
			if (trackingTypeFlags == TrackingTypeFlags.None)
			{
				UnityEngine.Debug.LogError($"RemoveTrackedData failed; tracked data at position {worldPos} does not have the target flag(s).");
				return;
			}
			if ((trackingTypeFlags & TrackingTypeFlags.PoiMultiBlock) != TrackingTypeFlags.None)
			{
				poiMultiBlocks.Remove(worldPos);
			}
			if ((trackingTypeFlags & TrackingTypeFlags.CrossChunkMultiBlock) != TrackingTypeFlags.None)
			{
				crossChunkMultiBlocks.Remove(worldPos);
			}
			if ((trackingTypeFlags & TrackingTypeFlags.OversizedBlock) != TrackingTypeFlags.None)
			{
				oversizedBlocks.Remove(worldPos);
			}
			if ((trackingTypeFlags & TrackingTypeFlags.TerrainAlignedBlock) != TrackingTypeFlags.None)
			{
				terrainAlignedBlocks.Remove(worldPos);
			}
			TrackingTypeFlags trackingTypeFlags2 = (TrackingTypeFlags)((uint)value.trackingTypeFlags & (uint)(byte)(~(int)flagsToRemove));
			if (trackingTypeFlags2 == TrackingTypeFlags.None)
			{
				trackedDataByPosition.Remove(worldPos);
			}
			else
			{
				trackedDataByPosition[worldPos] = new TrackedBlockData(value.rawData, value.flatChunkBounds, trackingTypeFlags2);
			}
		}
	}

	public enum Mode
	{
		Disabled,
		Normal,
		WorldEditor,
		PrefabPlaytest,
		PrefabEditor,
		Client
	}

	[Flags]
	public enum FeatureFlags
	{
		None = 0,
		POIMBTracking = 1,
		Serialization = 2,
		CrossChunkMBTracking = 4,
		OversizedStability = 8,
		TerrainAlignment = 0x10,
		All = 0x1F
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum FeatureRequirement
	{
		AllEnabled,
		OneOrMoreEnabled,
		AllDisabled
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte FILEVERSION = 6;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int c_deregisteredMultiBlockLimit = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public object lockObj = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerSaveAll = new ProfilerMarker("MultiBlockManager.SaveAll");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerUpdateTrackedBlockData = new ProfilerMarker("MultiBlockManager.UpdateTrackedBlockData");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerTryAddPOIMultiBlock = new ProfilerMarker("MultiBlockManager.TryAddPOIMultiBlock");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerCullChunkless = new ProfilerMarker("MultiBlockManager.CullChunkless");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerCullCompletePOIPlacements = new ProfilerMarker("MultiBlockManager.CullCompletePOIPlacements");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerTryRegisterWorld = new ProfilerMarker("MultiBlockManager.TryRegisterWorld");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerTryRegisterOversized = new ProfilerMarker("MultiBlockManager.TryRegisterOversized");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerTryRegisterTerrainAligned = new ProfilerMarker("MultiBlockManager.TryRegisterTerrainAligned");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerDeregistrationCleanup = new ProfilerMarker("MultiBlockManager.DeregistrationCleanup");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerSetStabilityDirty = new ProfilerMarker("MultiBlockManager.SetStabilityDirty");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerUpdateStability = new ProfilerMarker("MultiBlockManager.UpdateStability");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerSetAlignmentDirty = new ProfilerMarker("MultiBlockManager.SetAlignmentDirty");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerUpdateAlignment = new ProfilerMarker("MultiBlockManager.UpdateAlignment");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_POIMultiBlockCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "POI MultiBlocks", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_CrossChunkMultiBlockCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Cross-Chunk MultiBlocks", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_OversizedBlockCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Oversized Blocks", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_TerrainAlignedBlockCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Terrain-Aligned Blocks", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_TrackedDataCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Tracked Data Count", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_OversizedStabilityChecksCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Oversized Stability Checks", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_AlignmentChecksCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Alignment Checks", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_DeregisteredMultiBlockCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Deregistered MultiBlocks", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_MultiBlockManagerSaveCallCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "MultiBlock Save Called", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_MultiBlockManagerSaveProcessedCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "MultiBlock Save Processed", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	[PublicizedFrom(EAccessModifier.Private)]
	public static MultiBlockManager m_Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileManager regionFileManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public TrackedDataMap trackedDataMap = new TrackedDataMap();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<Vector3i> oversizedBlocksWithDirtyStability = new HashSet<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<Vector3i> blocksWithDirtyAlignment = new HashSet<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<Vector3i> keysToRemove = new Queue<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> tempChunksToGroup = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkCluster cc;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkManager chunkManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public string filePath;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public int deregisteredMultiBlockCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public Mode currentMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public FeatureFlags enabledFeatures;

	public static MultiBlockManager Instance
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = new MultiBlockManager();
			}
			return m_Instance;
		}
	}

	public bool NoFeaturesEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return CheckFeatures(FeatureFlags.All, FeatureRequirement.AllDisabled);
		}
	}

	public bool POIMBTrackingEnabled => CheckFeatures(FeatureFlags.POIMBTracking);

	[PublicizedFrom(EAccessModifier.Private)]
	public MultiBlockManager()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckFeatures(FeatureFlags targetFeatures, FeatureRequirement requirement = FeatureRequirement.AllEnabled)
	{
		DoCurrentModeSanityChecks();
		return requirement switch
		{
			FeatureRequirement.AllDisabled => (enabledFeatures & targetFeatures) == 0, 
			FeatureRequirement.OneOrMoreEnabled => (enabledFeatures & targetFeatures) != 0, 
			_ => (enabledFeatures & targetFeatures) == targetFeatures, 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FeatureFlags GetFeaturesForMode(Mode mode)
	{
		return mode switch
		{
			Mode.Normal => FeatureFlags.All, 
			Mode.WorldEditor => FeatureFlags.POIMBTracking | FeatureFlags.CrossChunkMBTracking | FeatureFlags.OversizedStability | FeatureFlags.TerrainAlignment, 
			Mode.PrefabPlaytest => FeatureFlags.POIMBTracking | FeatureFlags.OversizedStability | FeatureFlags.TerrainAlignment, 
			Mode.PrefabEditor => FeatureFlags.POIMBTracking | FeatureFlags.TerrainAlignment, 
			Mode.Client => FeatureFlags.TerrainAlignment, 
			_ => FeatureFlags.None, 
		};
	}

	public void Initialize(RegionFileManager regionFileManager)
	{
		lock (lockObj)
		{
			this.regionFileManager = regionFileManager;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (GameManager.Instance.IsEditMode())
				{
					currentMode = ((regionFileManager == null) ? Mode.PrefabEditor : Mode.WorldEditor);
				}
				else
				{
					currentMode = ((regionFileManager != null) ? Mode.Normal : Mode.PrefabPlaytest);
				}
			}
			else
			{
				currentMode = Mode.Client;
			}
			enabledFeatures = GetFeaturesForMode(currentMode);
			world = GameManager.Instance.World;
			cc = world.ChunkCache;
			chunkManager = world.m_ChunkManager;
			if (CheckFeatures(FeatureFlags.OversizedStability | FeatureFlags.TerrainAlignment, FeatureRequirement.OneOrMoreEnabled))
			{
				ChunkManager obj = chunkManager;
				obj.OnChunkInitialized = (Action<Chunk>)Delegate.Combine(obj.OnChunkInitialized, new Action<Chunk>(OnChunkInitialized));
			}
			ChunkManager obj2 = chunkManager;
			obj2.OnChunkRegenerated = (Action<Chunk>)Delegate.Combine(obj2.OnChunkRegenerated, new Action<Chunk>(OnChunkRegeneratedOrDisplayed));
			ChunkManager obj3 = chunkManager;
			obj3.OnChunkCopiedToUnity = (Action<Chunk>)Delegate.Combine(obj3.OnChunkCopiedToUnity, new Action<Chunk>(OnChunkRegeneratedOrDisplayed));
			filePath = Path.Combine(GameIO.GetSaveGameDir(), "multiblocks.7dt");
			TryLoad();
			UpdateProfilerCounters();
			isDirty = false;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool TryLoad()
		{
			trackedDataMap.Clear();
			deregisteredMultiBlockCount = 0;
			if (!CheckFeatures(FeatureFlags.Serialization))
			{
				return false;
			}
			if (!SdFile.Exists(filePath))
			{
				return false;
			}
			using Stream baseStream = SdFile.OpenRead(filePath);
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader.SetBaseStream(baseStream);
			byte b = pooledBinaryReader.ReadByte();
			if (b != 6)
			{
				Log.Error($"[MultiBlockManager] Saved MultiBlock data is out of date. Saved version is ({b}). Current version is ({(byte)6}). " + "This data is no longer compatible and will be ignored. MultiBlock-related bugs are likely to occur if you continue with this save. Please start a fresh game to avoid these issues.");
				return false;
			}
			int num = pooledBinaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				Vector3i vector3i = StreamUtils.ReadVector3i(pooledBinaryReader);
				uint rawData = pooledBinaryReader.ReadUInt32();
				BlockValue blockValue = new BlockValue(rawData);
				byte num2 = pooledBinaryReader.ReadByte();
				if ((num2 & 1) != 0)
				{
					TryRegisterPOIMultiBlock(vector3i, blockValue);
				}
				if ((num2 & 2) != 0)
				{
					TryRegisterCrossChunkMultiBlock(vector3i, blockValue);
				}
				if ((num2 & 4) != 0)
				{
					TryRegisterOversizedBlock(vector3i, blockValue);
				}
				if ((num2 & 8) != 0)
				{
					TryRegisterTerrainAlignedBlockInternal(vector3i, blockValue);
				}
			}
			return true;
		}
	}

	public void Cleanup()
	{
		if (NoFeaturesEnabled)
		{
			return;
		}
		lock (lockObj)
		{
			ChunkManager obj = chunkManager;
			obj.OnChunkInitialized = (Action<Chunk>)Delegate.Remove(obj.OnChunkInitialized, new Action<Chunk>(OnChunkInitialized));
			ChunkManager obj2 = chunkManager;
			obj2.OnChunkRegenerated = (Action<Chunk>)Delegate.Remove(obj2.OnChunkRegenerated, new Action<Chunk>(OnChunkRegeneratedOrDisplayed));
			ChunkManager obj3 = chunkManager;
			obj3.OnChunkCopiedToUnity = (Action<Chunk>)Delegate.Remove(obj3.OnChunkCopiedToUnity, new Action<Chunk>(OnChunkRegeneratedOrDisplayed));
			SaveIfDirty();
			trackedDataMap.Clear();
			oversizedBlocksWithDirtyStability.Clear();
			blocksWithDirtyAlignment.Clear();
			deregisteredMultiBlockCount = 0;
			UpdateProfilerCounters();
			filePath = null;
			regionFileManager = null;
			cc = null;
			currentMode = Mode.Disabled;
			enabledFeatures = FeatureFlags.None;
		}
	}

	public void SaveIfDirty()
	{
		if (!CheckFeatures(FeatureFlags.Serialization))
		{
			return;
		}
		ProfilerCounterValue<int> profilerCounterValue = s_MultiBlockManagerSaveCallCount;
		profilerCounterValue.Value++;
		using (s_MultiBlockManagerSaveAll.Auto())
		{
			lock (lockObj)
			{
				if (!isDirty)
				{
					return;
				}
				if (string.IsNullOrEmpty(filePath))
				{
					UnityEngine.Debug.LogError("[MultiBlockManager] Failed to save MultiBlock data; MultiBlockManager has not been initialized with a valid filepath.");
					return;
				}
				CullCompletePOIPlacements();
				using (Stream stream = SdFile.OpenWrite(filePath))
				{
					using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
					pooledBinaryWriter.SetBaseStream(stream);
					pooledBinaryWriter.Write((byte)6);
					pooledBinaryWriter.Write(trackedDataMap.TrackedDataByPosition.Count);
					foreach (KeyValuePair<Vector3i, TrackedBlockData> item in trackedDataMap.TrackedDataByPosition)
					{
						StreamUtils.Write(pooledBinaryWriter, item.Key);
						TrackedBlockData value = item.Value;
						pooledBinaryWriter.Write(value.rawData);
						pooledBinaryWriter.Write((byte)value.trackingTypeFlags);
					}
					stream.SetLength(stream.Position);
					pooledBinaryWriter.Flush();
				}
				isDirty = false;
			}
		}
		profilerCounterValue = s_MultiBlockManagerSaveProcessedCount;
		profilerCounterValue.Value++;
	}

	public void CullChunklessData()
	{
		if (!CheckFeatures(FeatureFlags.CrossChunkMBTracking))
		{
			return;
		}
		using (s_MultiBlockManagerCullChunkless.Auto())
		{
			lock (lockObj)
			{
				foreach (KeyValuePair<Vector3i, TrackedBlockData> item in trackedDataMap.TrackedDataByPosition)
				{
					Vector3i key = item.Key;
					RectInt flatChunkBounds = item.Value.flatChunkBounds;
					if (!AnyOverlappedChunkIsSyncedOrInSaveDir(flatChunkBounds))
					{
						keysToRemove.Enqueue(key);
					}
				}
				Vector3i result;
				while (keysToRemove.TryDequeue(out result))
				{
					DeregisterTrackedBlockDataInternal(result);
				}
				ProcessDeregistrationCleanup();
				UpdateProfilerCounters();
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool AnyOverlappedChunkIsSyncedOrInSaveDir(RectInt rectInt)
		{
			for (int i = rectInt.yMin; i <= rectInt.yMax; i++)
			{
				for (int j = rectInt.xMin; j <= rectInt.xMax; j++)
				{
					long key2 = WorldChunkCache.MakeChunkKey(j, i);
					if (regionFileManager.ContainsChunkSync(key2) || cc.ContainsChunkSync(key2))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public void UpdateTrackedBlockData(Vector3i worldPos, BlockValue blockValue, bool poiOwned)
	{
		if (NoFeaturesEnabled)
		{
			return;
		}
		using (s_MultiBlockManagerUpdateTrackedBlockData.Auto())
		{
			lock (lockObj)
			{
				TrackingTypeFlags trackingTypeFlags = TrackingTypeFlags.None;
				if (trackedDataMap.TryGetValue(worldPos, out var value))
				{
					if (value.rawData != blockValue.rawData)
					{
						DeregisterTrackedBlockDataInternal(worldPos);
						ProcessDeregistrationCleanup();
					}
					else
					{
						trackingTypeFlags = value.trackingTypeFlags;
						if (poiOwned)
						{
							if ((value.trackingTypeFlags & TrackingTypeFlags.CrossChunkMultiBlock) != TrackingTypeFlags.None)
							{
								trackedDataMap.RemoveTrackedData(worldPos, TrackingTypeFlags.CrossChunkMultiBlock);
								deregisteredMultiBlockCount++;
								ProcessDeregistrationCleanup();
								trackingTypeFlags &= ~TrackingTypeFlags.CrossChunkMultiBlock;
							}
						}
						else if ((value.trackingTypeFlags & TrackingTypeFlags.PoiMultiBlock) != TrackingTypeFlags.None)
						{
							trackedDataMap.RemoveTrackedData(worldPos, TrackingTypeFlags.PoiMultiBlock);
							trackingTypeFlags &= ~TrackingTypeFlags.PoiMultiBlock;
						}
					}
				}
				if (blockValue.Block.isMultiBlock)
				{
					if (poiOwned)
					{
						if ((trackingTypeFlags & TrackingTypeFlags.PoiMultiBlock) != TrackingTypeFlags.None)
						{
						}
					}
					else if ((trackingTypeFlags & TrackingTypeFlags.CrossChunkMultiBlock) == 0)
					{
						TryRegisterCrossChunkMultiBlock(worldPos, blockValue);
					}
				}
				if (blockValue.Block.isOversized && (trackingTypeFlags & TrackingTypeFlags.OversizedBlock) == 0)
				{
					TryRegisterOversizedBlock(worldPos, blockValue);
				}
				if (blockValue.Block.terrainAlignmentMode != TerrainAlignmentMode.None && (trackingTypeFlags & TrackingTypeFlags.TerrainAlignedBlock) == 0)
				{
					TryRegisterTerrainAlignedBlockInternal(worldPos, blockValue);
				}
				UpdateProfilerCounters();
			}
		}
	}

	public void DeregisterTrackedBlockData(Vector3i worldPos)
	{
		if (NoFeaturesEnabled)
		{
			return;
		}
		lock (lockObj)
		{
			DeregisterTrackedBlockDataInternal(worldPos);
			ProcessDeregistrationCleanup();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DeregisterTrackedBlockDataInternal(Vector3i worldPos)
	{
		if (trackedDataMap.TryGetValue(worldPos, out var value))
		{
			if ((value.trackingTypeFlags & TrackingTypeFlags.CrossChunkMultiBlock) != TrackingTypeFlags.None)
			{
				deregisteredMultiBlockCount++;
			}
			trackedDataMap.RemoveTrackedData(worldPos, TrackingTypeFlags.All);
			blocksWithDirtyAlignment.Remove(worldPos);
			oversizedBlocksWithDirtyStability.Remove(worldPos);
			isDirty = true;
			UpdateProfilerCounters();
		}
	}

	public void DeregisterTrackedBlockDatas(Bounds bounds)
	{
		if (NoFeaturesEnabled)
		{
			return;
		}
		lock (lockObj)
		{
			List<Vector3i> list = null;
			foreach (Vector3i key in trackedDataMap.TrackedDataByPosition.Keys)
			{
				if (bounds.Contains(key))
				{
					if (list == null)
					{
						list = new List<Vector3i>();
					}
					list.Add(key);
				}
			}
			if (list == null)
			{
				return;
			}
			foreach (Vector3i item in list)
			{
				DeregisterTrackedBlockDataInternal(item);
			}
			ProcessDeregistrationCleanup();
		}
	}

	public void MainThreadUpdate()
	{
		UpdateAlignment();
		UpdateOversizedStability();
	}

	public bool TryRegisterTerrainAlignedBlock(Vector3i worldPos, BlockValue blockValue)
	{
		bool result = TryRegisterTerrainAlignedBlockInternal(worldPos, blockValue);
		UpdateProfilerCounters();
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryRegisterTerrainAlignedBlockInternal(Vector3i worldPos, BlockValue blockValue)
	{
		if (!CheckFeatures(FeatureFlags.TerrainAlignment))
		{
			return false;
		}
		using (s_MultiBlockManagerTryRegisterTerrainAligned.Auto())
		{
			if (blockValue.Block.terrainAlignmentMode == TerrainAlignmentMode.None)
			{
				UnityEngine.Debug.LogError($"[MultiBlockManager] TryRegisterTerrainAlignedBlock failed: target block of type {blockValue.Block.GetBlockName()} at {worldPos} is not a terrain-aligned block.");
				return false;
			}
			if (blockValue.ischild)
			{
				UnityEngine.Debug.LogError($"[MultiBlockManager] TryRegisterTerrainAlignedBlock failed: target block is not a parent at position {worldPos}.");
				return false;
			}
			lock (lockObj)
			{
				if (trackedDataMap.TryGetValue(worldPos, out var value))
				{
					if (blockValue.rawData != value.rawData)
					{
						UnityEngine.Debug.LogError($"Unexpected condition in TryRegisterTerrainAlignedBlock: encountered raw data mismatch at position {worldPos}.");
						return false;
					}
					if ((value.trackingTypeFlags & TrackingTypeFlags.TerrainAlignedBlock) != TrackingTypeFlags.None)
					{
						return true;
					}
					RegisterTerrainAlignedBlock(worldPos, blockValue, value.flatChunkBounds);
					return true;
				}
				bool result = false;
				if (blockValue.Block.isMultiBlock)
				{
					GetMinMaxWorldPositions(worldPos, blockValue, out var minPos, out var maxPos);
					Vector2i vector2i = World.toChunkXZ(minPos);
					Vector2i vector2i2 = World.toChunkXZ(maxPos);
					RegisterTerrainAlignedBlock(flatChunkBounds: new RectInt(vector2i, vector2i2 - vector2i), worldPos: worldPos, blockValue: blockValue);
					result = true;
				}
				if (blockValue.Block.isOversized)
				{
					OversizedBlockUtils.GetWorldAlignedBoundsExtents(worldPos, blockValue.Block.shape.GetRotation(blockValue), blockValue.Block.oversizedBounds, out var min, out var max);
					Vector2i vector2i3 = World.toChunkXZ(min);
					Vector2i vector2i4 = World.toChunkXZ(max);
					RegisterTerrainAlignedBlock(flatChunkBounds: new RectInt(vector2i3, vector2i4 - vector2i3), worldPos: worldPos, blockValue: blockValue);
					result = true;
				}
				return result;
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void RegisterTerrainAlignedBlock(Vector3i vector3i, BlockValue blockValue2, RectInt flatChunkBounds)
		{
			trackedDataMap.AddOrMergeTrackedData(vector3i, blockValue2.rawData, flatChunkBounds, TrackingTypeFlags.TerrainAlignedBlock);
			blocksWithDirtyAlignment.Add(vector3i);
			isDirty = true;
		}
	}

	public void SetTerrainAlignmentDirty(Vector3i worldPos)
	{
		if (!CheckFeatures(FeatureFlags.TerrainAlignment))
		{
			return;
		}
		using (s_MultiBlockManagerSetAlignmentDirty.Auto())
		{
			lock (lockObj)
			{
				if (!blocksWithDirtyAlignment.Contains(worldPos))
				{
					if (!trackedDataMap.TerrainAlignedBlocks.ContainsKey(worldPos))
					{
						UnityEngine.Debug.LogWarning($"[MultiBlockManager][Alignment] SetTerrainAlignmentDirty failed; no terrain-aligned block has been registered at the specified world position: {worldPos}");
					}
					else
					{
						blocksWithDirtyAlignment.Add(worldPos);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateAlignment()
	{
		if (!CheckFeatures(FeatureFlags.TerrainAlignment))
		{
			return;
		}
		using (s_MultiBlockManagerUpdateAlignment.Auto())
		{
			lock (lockObj)
			{
				ProfilerCounterValue<int> profilerCounterValue = s_AlignmentChecksCount;
				profilerCounterValue.Value += blocksWithDirtyAlignment.Count;
				foreach (Vector3i item in blocksWithDirtyAlignment)
				{
					if (!trackedDataMap.TerrainAlignedBlocks.TryGetValue(item, out var value))
					{
						UnityEngine.Debug.LogWarning($"[MultiBlockManager][Alignment] Failed to retrieve registered terrain-aligned block at expected location: {item}");
					}
					else
					{
						TryAlignBlock(item, value);
					}
				}
				blocksWithDirtyAlignment.Clear();
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool AllOverlappedChunksAreReady(RectInt flatChunkBounds)
		{
			for (int i = flatChunkBounds.yMin; i <= flatChunkBounds.yMax; i++)
			{
				for (int j = flatChunkBounds.xMin; j <= flatChunkBounds.xMax; j++)
				{
					long key = WorldChunkCache.MakeChunkKey(j, i);
					Chunk chunkSync = cc.GetChunkSync(key);
					if (chunkSync == null || !chunkSync.IsInitialized || chunkSync.NeedsRegeneration || chunkSync.InProgressRegeneration || chunkSync.NeedsCopying || chunkSync.InProgressCopying)
					{
						return false;
					}
				}
			}
			return true;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool TryAlignBlock(Vector3i worldPos, TrackedBlockData blockData)
		{
			RectInt flatChunkBounds = blockData.flatChunkBounds;
			if (!AllOverlappedChunksAreReady(flatChunkBounds))
			{
				return false;
			}
			BlockEntityData blockEntity = ((Chunk)world.GetChunkFromWorldPos(worldPos)).GetBlockEntity(worldPos);
			BlockValue blockValue = new BlockValue(blockData.rawData);
			Block block = blockValue.Block;
			TerrainAlignmentMode terrainAlignmentMode = block.terrainAlignmentMode;
			if (terrainAlignmentMode != TerrainAlignmentMode.None && (uint)(terrainAlignmentMode - 1) <= 1u)
			{
				return TerrainAlignmentUtils.AlignToTerrain(block, worldPos, blockValue, blockEntity, block.terrainAlignmentMode);
			}
			UnityEngine.Debug.LogError($"[MultiBlockManager][Alignment] TryAlignBlock cannot align block with TerrainAlignmentMode \"{block.terrainAlignmentMode}\" of type {block.GetBlockName()} at {worldPos}");
			return false;
		}
	}

	public void CullChunklessDataOnClient(List<long> removedChunks)
	{
		if (currentMode != Mode.Client)
		{
			return;
		}
		using (s_MultiBlockManagerCullChunkless.Auto())
		{
			lock (lockObj)
			{
				foreach (KeyValuePair<Vector3i, TrackedBlockData> item in trackedDataMap.TrackedDataByPosition)
				{
					Vector3i key = item.Key;
					RectInt flatChunkBounds = item.Value.flatChunkBounds;
					if (!AnyOverlappedChunkIsSynced(flatChunkBounds))
					{
						keysToRemove.Enqueue(key);
					}
				}
				Vector3i result;
				while (keysToRemove.TryDequeue(out result))
				{
					DeregisterTrackedBlockDataInternal(result);
				}
				UpdateProfilerCounters();
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool AnyOverlappedChunkIsSynced(RectInt rectInt)
		{
			for (int i = rectInt.yMin; i <= rectInt.yMax; i++)
			{
				for (int j = rectInt.xMin; j <= rectInt.xMax; j++)
				{
					long num = WorldChunkCache.MakeChunkKey(j, i);
					if (!removedChunks.Contains(num) && cc.ContainsChunkSync(num))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public bool TryRegisterPOIMultiBlock(Vector3i parentWorldPos, BlockValue blockValue)
	{
		if (!CheckFeatures(FeatureFlags.POIMBTracking))
		{
			return false;
		}
		using (s_MultiBlockManagerTryAddPOIMultiBlock.Auto())
		{
			lock (lockObj)
			{
				if (trackedDataMap.CrossChunkMultiBlocks.TryGetValue(parentWorldPos, out var value))
				{
					UnityEngine.Debug.LogError($"[MultiBlockManager] Failed to register POI multiblock at {parentWorldPos} due to previously registered CrossChunk data." + $"\nOld value: {value.rawData} " + $"\nNew value: {blockValue.rawData}");
					return false;
				}
				if (trackedDataMap.PoiMultiBlocks.TryGetValue(parentWorldPos, out var value2))
				{
					UnityEngine.Debug.LogError($"[MultiBlockManager] Duplicate multiblock placement at {parentWorldPos}. New value will not be applied." + $"\nOld value: {value2.rawData} " + $"\nNew value: {blockValue.rawData}");
					return false;
				}
				if (blockValue.ischild)
				{
					UnityEngine.Debug.LogError("[MultiBlockManager] TryAddPOIMultiBlock failed: target block is not a parent.");
					return false;
				}
				RectInt flatChunkBounds;
				if (!blockValue.Block.isMultiBlock)
				{
					Vector2i vector2i = World.toChunkXZ(parentWorldPos);
					flatChunkBounds = new RectInt(vector2i, Vector2Int.zero);
				}
				else
				{
					GetMinMaxWorldPositions(parentWorldPos, blockValue, out var minPos, out var maxPos);
					Vector2i vector2i2 = World.toChunkXZ(minPos);
					Vector2i vector2i3 = World.toChunkXZ(maxPos);
					flatChunkBounds = new RectInt(vector2i2, vector2i3 - vector2i2);
				}
				trackedDataMap.AddOrMergeTrackedData(parentWorldPos, blockValue.rawData, flatChunkBounds, TrackingTypeFlags.PoiMultiBlock);
				isDirty = true;
				UpdateProfilerCounters();
				return true;
			}
		}
	}

	public static void GetMinMaxWorldPositions(Vector3i parentWorldPos, BlockValue blockValue, out Vector3i minPos, out Vector3i maxPos)
	{
		minPos = parentWorldPos;
		maxPos = parentWorldPos;
		for (int i = 0; i < blockValue.Block.multiBlockPos.Length; i++)
		{
			Vector3i v = parentWorldPos + blockValue.Block.multiBlockPos.Get(i, blockValue.type, blockValue.rotation);
			minPos = Vector3i.Min(minPos, v);
			maxPos = Vector3i.Max(maxPos, v);
		}
	}

	public bool TryGetPOIMultiBlock(Vector3i parentWorldPos, out TrackedBlockData poiMultiBlock)
	{
		if (!CheckFeatures(FeatureFlags.POIMBTracking))
		{
			poiMultiBlock = default(TrackedBlockData);
			return false;
		}
		lock (lockObj)
		{
			return trackedDataMap.PoiMultiBlocks.TryGetValue(parentWorldPos, out poiMultiBlock);
		}
	}

	public void CullCompletePOIPlacements()
	{
		if (!CheckFeatures(FeatureFlags.POIMBTracking | FeatureFlags.Serialization))
		{
			return;
		}
		using (s_MultiBlockManagerCullCompletePOIPlacements.Auto())
		{
			lock (lockObj)
			{
				foreach (KeyValuePair<Vector3i, TrackedBlockData> poiMultiBlock in trackedDataMap.PoiMultiBlocks)
				{
					Vector3i key = poiMultiBlock.Key;
					RectInt flatChunkBounds = poiMultiBlock.Value.flatChunkBounds;
					if (AllOverlappedChunksAreSavedAndDormant(flatChunkBounds))
					{
						keysToRemove.Enqueue(key);
					}
				}
				Vector3i result;
				while (keysToRemove.TryDequeue(out result))
				{
					trackedDataMap.RemoveTrackedData(result, TrackingTypeFlags.PoiMultiBlock);
					isDirty = true;
				}
				UpdateProfilerCounters();
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool AllOverlappedChunksAreSavedAndDormant(RectInt rectInt)
		{
			for (int i = rectInt.yMin; i <= rectInt.yMax; i++)
			{
				for (int j = rectInt.xMin; j <= rectInt.xMax; j++)
				{
					long key2 = WorldChunkCache.MakeChunkKey(j, i);
					if (cc.ContainsChunkSync(key2) || !regionFileManager.IsChunkSavedAndDormant(key2))
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryRegisterCrossChunkMultiBlock(Vector3i parentWorldPos, BlockValue parentBlockValue)
	{
		if (!CheckFeatures(FeatureFlags.CrossChunkMBTracking))
		{
			return false;
		}
		using (s_MultiBlockManagerTryRegisterWorld.Auto())
		{
			lock (lockObj)
			{
				if (trackedDataMap.PoiMultiBlocks.ContainsKey(parentWorldPos))
				{
					return false;
				}
				if (!parentBlockValue.Block.isMultiBlock)
				{
					UnityEngine.Debug.LogError($"[MultiBlockManager] TryRegisterCrossChunkMultiBlock failed: target block of type {parentBlockValue.Block.GetBlockName()} at {parentWorldPos} is not a MultiBlock.");
					return false;
				}
				if (parentBlockValue.ischild)
				{
					UnityEngine.Debug.LogError("[MultiBlockManager] TryRegisterCrossChunkMultiBlock failed: target block is not a parent.");
					return false;
				}
				Vector3 vector = parentBlockValue.Block.shape.GetRotation(parentBlockValue) * parentBlockValue.Block.multiBlockPos.dim;
				if (Mathf.Approximately(Mathf.Abs(vector.x), 1f) && Mathf.Approximately(Mathf.Abs(vector.z), 1f))
				{
					return false;
				}
				GetMinMaxWorldPositions(parentWorldPos, parentBlockValue, out var minPos, out var maxPos);
				if (minPos.x == maxPos.x && minPos.z == maxPos.z)
				{
					return false;
				}
				Vector2i vector2i = World.toChunkXZ(minPos);
				Vector2i vector2i2 = World.toChunkXZ(maxPos);
				if (vector2i == vector2i2)
				{
					return false;
				}
				RectInt flatChunkBounds = new RectInt(vector2i, vector2i2 - vector2i);
				trackedDataMap.AddOrMergeTrackedData(parentWorldPos, parentBlockValue.rawData, flatChunkBounds, TrackingTypeFlags.CrossChunkMultiBlock);
				isDirty = true;
				tempChunksToGroup.Clear();
				for (int i = flatChunkBounds.yMin; i <= flatChunkBounds.yMax; i++)
				{
					for (int j = flatChunkBounds.xMin; j <= flatChunkBounds.xMax; j++)
					{
						long item = WorldChunkCache.MakeChunkKey(j, i);
						tempChunksToGroup.Add(item);
					}
				}
				regionFileManager.AddGroupedChunks(tempChunksToGroup);
				tempChunksToGroup.Clear();
				return true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessDeregistrationCleanup()
	{
		if (!CheckFeatures(FeatureFlags.CrossChunkMBTracking) || deregisteredMultiBlockCount <= 20)
		{
			return;
		}
		using (s_MultiBlockManagerDeregistrationCleanup.Auto())
		{
			regionFileManager.RebuildChunkGroupsFromPOIs();
			tempChunksToGroup.Clear();
			foreach (KeyValuePair<Vector3i, TrackedBlockData> crossChunkMultiBlock in trackedDataMap.CrossChunkMultiBlocks)
			{
				RectInt flatChunkBounds = crossChunkMultiBlock.Value.flatChunkBounds;
				for (int i = flatChunkBounds.yMin; i <= flatChunkBounds.yMax; i++)
				{
					for (int j = flatChunkBounds.xMin; j <= flatChunkBounds.xMax; j++)
					{
						long item = WorldChunkCache.MakeChunkKey(j, i);
						tempChunksToGroup.Add(item);
					}
				}
				regionFileManager.AddGroupedChunks(tempChunksToGroup);
				tempChunksToGroup.Clear();
			}
			deregisteredMultiBlockCount = 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryRegisterOversizedBlock(Vector3i worldPos, BlockValue blockValue)
	{
		if (!CheckFeatures(FeatureFlags.OversizedStability))
		{
			return false;
		}
		using (s_MultiBlockManagerTryRegisterOversized.Auto())
		{
			lock (lockObj)
			{
				if (!blockValue.Block.isOversized)
				{
					UnityEngine.Debug.LogError($"[MultiBlockManager] TryRegisterOversizedBlock failed: target block of type {blockValue.Block.GetBlockName()} at {worldPos} is not an Oversized Block.");
					return false;
				}
				OversizedBlockUtils.GetWorldAlignedBoundsExtents(worldPos, blockValue.Block.shape.GetRotation(blockValue), blockValue.Block.oversizedBounds, out var min, out var max);
				Vector2i vector2i = World.toChunkXZ(min);
				Vector2i vector2i2 = World.toChunkXZ(max);
				RectInt flatChunkBounds = new RectInt(vector2i, vector2i2 - vector2i);
				trackedDataMap.AddOrMergeTrackedData(worldPos, blockValue.rawData, flatChunkBounds, TrackingTypeFlags.OversizedBlock);
				oversizedBlocksWithDirtyStability.Add(worldPos);
				isDirty = true;
			}
			return true;
		}
	}

	public void SetOversizedStabilityDirty(Vector3i worldPos)
	{
		if (!CheckFeatures(FeatureFlags.OversizedStability))
		{
			return;
		}
		using (s_MultiBlockManagerSetStabilityDirty.Auto())
		{
			lock (lockObj)
			{
				Vector2i vector2i = World.toChunkXZ(worldPos);
				foreach (KeyValuePair<Vector3i, TrackedBlockData> oversizedBlock in trackedDataMap.OversizedBlocks)
				{
					Vector3i key = oversizedBlock.Key;
					if (oversizedBlocksWithDirtyStability.Contains(key))
					{
						continue;
					}
					TrackedBlockData value = oversizedBlock.Value;
					RectInt flatChunkBounds = value.flatChunkBounds;
					flatChunkBounds.max += Vector2Int.one;
					if (flatChunkBounds.Contains(vector2i))
					{
						BlockValue blockValue = new BlockValue(value.rawData);
						Quaternion rotation = blockValue.Block.shape.GetRotation(blockValue);
						Bounds localStabilityBounds = OversizedBlockUtils.GetLocalStabilityBounds(blockValue.Block.oversizedBounds, rotation);
						localStabilityBounds.extents += (Vector3)Vector3Int.one;
						Matrix4x4 blockWorldToLocalMatrix = OversizedBlockUtils.GetBlockWorldToLocalMatrix(key, rotation);
						if (OversizedBlockUtils.IsBlockCenterWithinBounds(worldPos, localStabilityBounds, blockWorldToLocalMatrix))
						{
							oversizedBlocksWithDirtyStability.Add(key);
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnChunkRegeneratedOrDisplayed(Chunk chunk)
	{
		if (!CheckFeatures(FeatureFlags.TerrainAlignment))
		{
			return;
		}
		Vector2i chunkPos = new Vector2i(chunk.X, chunk.Z);
		lock (lockObj)
		{
			using (s_MultiBlockManagerSetAlignmentDirty.Auto())
			{
				AddChunkOverlappingBlocksToSet(chunkPos, trackedDataMap.TerrainAlignedBlocks, blocksWithDirtyAlignment);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnChunkInitialized(Chunk chunk)
	{
		if (!CheckFeatures(FeatureFlags.OversizedStability | FeatureFlags.TerrainAlignment, FeatureRequirement.OneOrMoreEnabled))
		{
			return;
		}
		Vector2i chunkPos = new Vector2i(chunk.X, chunk.Z);
		lock (lockObj)
		{
			if (CheckFeatures(FeatureFlags.OversizedStability))
			{
				using (s_MultiBlockManagerSetStabilityDirty.Auto())
				{
					AddChunkOverlappingBlocksToSet(chunkPos, trackedDataMap.OversizedBlocks, oversizedBlocksWithDirtyStability);
				}
			}
			if (CheckFeatures(FeatureFlags.TerrainAlignment))
			{
				using (s_MultiBlockManagerSetAlignmentDirty.Auto())
				{
					AddChunkOverlappingBlocksToSet(chunkPos, trackedDataMap.TerrainAlignedBlocks, blocksWithDirtyAlignment);
					return;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AddChunkOverlappingBlocksToSet(Vector2i chunkPos, TrackedDataMap.SubsetAccessor blocksMap, HashSet<Vector3i> targetSet)
	{
		foreach (KeyValuePair<Vector3i, TrackedBlockData> item in blocksMap)
		{
			Vector3i key = item.Key;
			if (!targetSet.Contains(key))
			{
				RectInt flatChunkBounds = item.Value.flatChunkBounds;
				flatChunkBounds.max += Vector2Int.one;
				if (flatChunkBounds.Contains(chunkPos))
				{
					targetSet.Add(key);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateOversizedStability()
	{
		if (!CheckFeatures(FeatureFlags.OversizedStability))
		{
			return;
		}
		using (s_MultiBlockManagerUpdateStability.Auto())
		{
			lock (lockObj)
			{
				ProfilerCounterValue<int> profilerCounterValue = s_OversizedStabilityChecksCount;
				profilerCounterValue.Value += oversizedBlocksWithDirtyStability.Count;
				foreach (Vector3i item in oversizedBlocksWithDirtyStability)
				{
					if (!trackedDataMap.OversizedBlocks.TryGetValue(item, out var value))
					{
						UnityEngine.Debug.LogWarning($"[MultiBlockManager][Stability] Failed to retrieve registered Oversized Block at expected location: {item}");
					}
					else if (!IsOversizedBlockStable(item, value))
					{
						GameManager.Instance.World.AddFallingBlock(item, includeOversized: true);
					}
				}
				oversizedBlocksWithDirtyStability.Clear();
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool AllOverlappedChunksAreSyncedAndInitialized(RectInt flatChunkBounds)
		{
			for (int i = flatChunkBounds.yMin; i <= flatChunkBounds.yMax; i++)
			{
				for (int j = flatChunkBounds.xMin; j <= flatChunkBounds.xMax; j++)
				{
					long key = WorldChunkCache.MakeChunkKey(j, i);
					Chunk chunkSync = cc.GetChunkSync(key);
					if (chunkSync == null || !chunkSync.IsInitialized)
					{
						return false;
					}
				}
			}
			return true;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		bool IsOversizedBlockStable(Vector3i worldPos, TrackedBlockData blockData)
		{
			RectInt flatChunkBounds = blockData.flatChunkBounds;
			if (!AllOverlappedChunksAreSyncedAndInitialized(flatChunkBounds))
			{
				return true;
			}
			BlockValue blockValue = new BlockValue(blockData.rawData);
			Quaternion rotation = blockValue.Block.shape.GetRotation(blockValue);
			Bounds localStabilityBounds = OversizedBlockUtils.GetLocalStabilityBounds(blockValue.Block.oversizedBounds, rotation);
			OversizedBlockUtils.GetWorldAlignedBoundsExtents(worldPos, rotation, localStabilityBounds, out var min, out var max);
			Matrix4x4 blockWorldToLocalMatrix = OversizedBlockUtils.GetBlockWorldToLocalMatrix(worldPos, rotation);
			World world = GameManager.Instance.World;
			for (int i = min.x; i <= max.x; i++)
			{
				for (int j = min.y; j <= max.y; j++)
				{
					for (int k = min.z; k <= max.z; k++)
					{
						Vector3i vector3i = new Vector3i(i, j, k);
						if (OversizedBlockUtils.IsBlockCenterWithinBounds(vector3i, localStabilityBounds, blockWorldToLocalMatrix) && world.GetStability(vector3i) > 1)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}

	[Conditional("MBM_ENABLE_GENERAL_LOG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DebugLogGeneral(string message)
	{
		UnityEngine.Debug.Log(message);
	}

	[Conditional("MBM_ENABLE_PLACEMENT_LOG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DebugLogPlacement(string message)
	{
		UnityEngine.Debug.Log(message);
	}

	[Conditional("MBM_ENABLE_REGISTRATION_LOG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DebugLogRegistration(string message)
	{
		UnityEngine.Debug.Log(message);
	}

	[Conditional("MBM_ENABLE_STABILITY_LOG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DebugLogStability(string message)
	{
		UnityEngine.Debug.Log(message);
	}

	[Conditional("MBM_ENABLE_ALIGNMENT_LOG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DebugLogAlignment(string message)
	{
		UnityEngine.Debug.Log(message);
	}

	[Conditional("MBM_ENABLE_PROFILER_MARKERS")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateProfilerCounters()
	{
		s_POIMultiBlockCount.Value = trackedDataMap.PoiMultiBlocks.Count;
		s_CrossChunkMultiBlockCount.Value = trackedDataMap.CrossChunkMultiBlocks.Count;
		s_OversizedBlockCount.Value = trackedDataMap.OversizedBlocks.Count;
		s_TerrainAlignedBlockCount.Value = trackedDataMap.TerrainAlignedBlocks.Count;
		s_DeregisteredMultiBlockCount.Value = deregisteredMultiBlockCount;
		s_TrackedDataCount.Value = trackedDataMap.Count;
	}

	[Conditional("MBM_ENABLED_SANITY_CHECKS")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void DoCurrentModeSanityChecks()
	{
		Mode mode = Mode.Client;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			mode = ((!GameManager.Instance.IsEditMode()) ? ((regionFileManager != null) ? Mode.Normal : Mode.PrefabPlaytest) : ((regionFileManager == null) ? Mode.PrefabEditor : Mode.WorldEditor));
		}
		if (currentMode != mode)
		{
			UnityEngine.Debug.LogError("[MultiBlockManager] Unexpected mode state. \n" + $"Current mode: {currentMode}. Expected mode: {mode}. \n" + $"GameManager.Instance.IsEditMode(): {GameManager.Instance.IsEditMode()}, " + $"ConnectionManager.Instance.IsServer: {SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer}, ");
		}
	}
}
