using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

public class WaterSimulationNative
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct HandleInitRequest
	{
		public ChunkKey chunkKey;

		public NativeSafeHandle<WaterDataHandle> safeHandle;
	}

	public struct ChunkHandle
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public WaterSimulationNative sim;

		[PublicizedFrom(EAccessModifier.Private)]
		public ChunkKey chunkKey;

		public bool IsValid => sim != null;

		public ChunkHandle(WaterSimulationNative _sim, Chunk _chunk)
		{
			sim = _sim;
			chunkKey = new ChunkKey(_chunk);
		}

		public void SetWaterMass(int _x, int _y, int _z, int _mass)
		{
			if (IsValid && sim.waterDataHandles.TryGetValue(chunkKey, out var item))
			{
				int voxelIndex = WaterDataHandle.GetVoxelIndex(_x, _y, _z);
				item.SetVoxelMass(voxelIndex, _mass);
				sim.activeHandles.Add(chunkKey);
			}
		}

		public void SetVoxelSolid(int _x, int _y, int _z, BlockFaceFlag _flags)
		{
			if (IsValid && sim.waterDataHandles.TryGetValue(chunkKey, out var item))
			{
				item.SetVoxelSolid(_x, _y, _z, _flags);
				if (_flags != BlockFaceFlag.All)
				{
					WakeNeighbours(_x, _y, _z);
				}
			}
		}

		public void WakeNeighbours(int _x, int _y, int _z)
		{
			if (IsValid)
			{
				if (sim.waterDataHandles.TryGetValue(chunkKey, out var item))
				{
					item.EnqueueVoxelWakeup(_x, _y, _z);
				}
				sim.modifiedChunks.Add(chunkKey);
			}
		}

		public void Reset()
		{
			if (IsValid && sim.IsInitialized)
			{
				sim.handlesToRemove.Enqueue(chunkKey);
			}
			sim = null;
			chunkKey = default(ChunkKey);
		}
	}

	public bool ShouldEnable = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPaused;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<ChunkKey, NativeSafeHandle<WaterDataHandle>> usedHandles = new Dictionary<ChunkKey, NativeSafeHandle<WaterDataHandle>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ConcurrentQueue<NativeSafeHandle<WaterDataHandle>> freeHandles = new ConcurrentQueue<NativeSafeHandle<WaterDataHandle>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ConcurrentQueue<HandleInitRequest> newInitializedHandles = new ConcurrentQueue<HandleInitRequest>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ConcurrentQueue<ChunkKey> handlesToRemove = new ConcurrentQueue<ChunkKey>();

	[PublicizedFrom(EAccessModifier.Private)]
	public UnsafeParallelHashSet<ChunkKey> activeHandles;

	[PublicizedFrom(EAccessModifier.Private)]
	public UnsafeParallelHashMap<ChunkKey, WaterDataHandle> waterDataHandles;

	[PublicizedFrom(EAccessModifier.Private)]
	public UnsafeParallelHashSet<ChunkKey> modifiedChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public GroundWaterHeightMap groundWaterHeightMap;

	public WaterSimulationApplyChanges changeApplier;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static WaterSimulationNative Instance
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = new WaterSimulationNative();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsInitialized
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsPaused => isPaused;

	public void Init(ChunkCluster _cc)
	{
		changeApplier = new WaterSimulationApplyChanges(_cc);
		if (ShouldEnable && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (waterDataHandles.IsCreated || modifiedChunks.IsCreated)
			{
				Debug.LogError("Last water simulation data was disposed of and may have leaked");
			}
			activeHandles = new UnsafeParallelHashSet<ChunkKey>(500, AllocatorManager.Persistent);
			waterDataHandles = new UnsafeParallelHashMap<ChunkKey, WaterDataHandle>(500, AllocatorManager.Persistent);
			modifiedChunks = new UnsafeParallelHashSet<ChunkKey>(500, AllocatorManager.Persistent);
			groundWaterHeightMap = new GroundWaterHeightMap(GameManager.Instance.World);
			IsInitialized = true;
			isPaused = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsChunkInWorldBounds(Chunk _c)
	{
		GameManager.Instance.World.GetWorldExtent(out var _minSize, out var _maxSize);
		if (_c.X * 16 >= _minSize.x && _c.X * 16 < _maxSize.x && _c.Z * 16 >= _minSize.z)
		{
			return _c.Z * 16 < _maxSize.z;
		}
		return false;
	}

	public void InitializeChunk(Chunk _c)
	{
		if (IsInitialized && IsChunkInWorldBounds(_c))
		{
			WaterDataHandle _target;
			if (freeHandles.TryDequeue(out var result))
			{
				_target = result.Target;
				_target.Clear();
			}
			else
			{
				_target = WaterDataHandle.AllocateNew(Allocator.Persistent);
				result = new NativeSafeHandle<WaterDataHandle>(ref _target, Allocator.Persistent);
			}
			_target.InitializeFromChunk(_c, groundWaterHeightMap);
			newInitializedHandles.Enqueue(new HandleInitRequest
			{
				chunkKey = new ChunkKey(_c),
				safeHandle = result
			});
			_c.AssignWaterSimHandle(new ChunkHandle(this, _c));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CopyInitializedChunksToNative()
	{
		HandleInitRequest result;
		while (newInitializedHandles.TryDequeue(out result))
		{
			ChunkKey chunkKey = result.chunkKey;
			NativeSafeHandle<WaterDataHandle> safeHandle = result.safeHandle;
			if (usedHandles.TryGetValue(chunkKey, out var value))
			{
				freeHandles.Enqueue(value);
				usedHandles[chunkKey] = safeHandle;
			}
			else
			{
				usedHandles.Add(chunkKey, safeHandle);
			}
			WaterDataHandle target = safeHandle.Target;
			waterDataHandles[chunkKey] = target;
			if (target.HasActiveWater)
			{
				activeHandles.Add(chunkKey);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessPendingRemoves()
	{
		ChunkKey result;
		while (handlesToRemove.TryDequeue(out result))
		{
			if (usedHandles.TryGetValue(result, out var value))
			{
				usedHandles.Remove(result);
				freeHandles.Enqueue(value);
			}
			waterDataHandles.Remove(result);
			activeHandles.Remove(result);
		}
	}

	public void SetPaused(bool _isPaused)
	{
		isPaused = _isPaused;
	}

	public void Step()
	{
		if (!isPaused)
		{
			SetPaused(_isPaused: true);
			return;
		}
		isPaused = false;
		Update();
		isPaused = true;
	}

	public void Update()
	{
		if (!IsInitialized)
		{
			return;
		}
		ProcessPendingRemoves();
		CopyInitializedChunksToNative();
		if (isPaused || changeApplier.HasNetWorkLimitBeenReached())
		{
			return;
		}
		WaterStats stats = default(WaterStats);
		if (!modifiedChunks.IsEmpty || !activeHandles.IsEmpty)
		{
			new WaterSimulationPreProcess
			{
				activeChunks = activeHandles,
				waterDataHandles = waterDataHandles,
				modifiedChunks = modifiedChunks
			}.Run();
			if (!activeHandles.IsEmpty)
			{
				NativeArray<ChunkKey> processingChunks = activeHandles.ToNativeArray(Allocator.TempJob);
				NativeList<ChunkKey> nonFlowingChunks = new NativeList<ChunkKey>(processingChunks.Length, AllocatorManager.TempJob);
				NativeArray<WaterStats> nativeArray = new NativeArray<WaterStats>(processingChunks.Length, Allocator.TempJob);
				WaterSimulationCalcFlows jobData = new WaterSimulationCalcFlows
				{
					processingChunks = processingChunks,
					waterStats = nativeArray,
					waterDataHandles = waterDataHandles
				};
				WaterSimulationApplyFlows jobData2 = new WaterSimulationApplyFlows
				{
					processingChunks = processingChunks,
					nonFlowingChunks = nonFlowingChunks.AsParallelWriter(),
					waterStats = nativeArray,
					waterDataHandles = waterDataHandles,
					activeChunkSet = activeHandles.AsParallelWriter()
				};
				WaterSimulationPostProcess jobData3 = new WaterSimulationPostProcess
				{
					processingChunks = processingChunks,
					nonFlowingChunks = nonFlowingChunks,
					activeChunks = activeHandles,
					waterDataHandles = waterDataHandles
				};
				int innerloopBatchCount = processingChunks.Length / JobsUtility.JobWorkerCount + 1;
				JobHandle dependsOn = IJobParallelForExtensions.Schedule(dependsOn: IJobParallelForExtensions.Schedule(jobData, processingChunks.Length, innerloopBatchCount), jobData: jobData2, arrayLength: processingChunks.Length, innerloopBatchCount: innerloopBatchCount);
				JobHandle jobHandle = jobData3.Schedule(dependsOn);
				JobHandle.ScheduleBatchedJobs();
				jobHandle.Complete();
				stats += WaterStats.Sum(nativeArray);
				nativeArray.Dispose();
				processingChunks.Dispose();
				nonFlowingChunks.Dispose();
			}
		}
		ProcessPendingRemoves();
		UnsafeParallelHashMap<ChunkKey, WaterDataHandle>.Enumerator enumerator = waterDataHandles.GetEnumerator();
		while (enumerator.MoveNext())
		{
			ChunkKey key = enumerator.Current.Key;
			WaterDataHandle value = enumerator.Current.Value;
			if (!value.HasFlows)
			{
				continue;
			}
			using WaterSimulationApplyChanges.ChangesForChunk.Writer writer = changeApplier.GetChangeWriter(WorldChunkCache.MakeChunkKey(key.x, key.z));
			UnsafeParallelHashMap<int, int>.Enumerator flowVoxels = value.FlowVoxels;
			while (flowVoxels.MoveNext())
			{
				int key2 = flowVoxels.Current.Key;
				int mass = value.voxelData.Get(key2);
				WaterValue waterValue = new WaterValue(mass);
				writer.RecordChange(key2, waterValue);
			}
			value.flowVoxels.Clear();
		}
		WaterStatsProfiler.SampleTick(stats);
	}

	public void Clear()
	{
		if (!IsInitialized)
		{
			return;
		}
		foreach (NativeSafeHandle<WaterDataHandle> value in usedHandles.Values)
		{
			freeHandles.Enqueue(value);
		}
		usedHandles.Clear();
		waterDataHandles.Clear();
		HandleInitRequest result;
		while (newInitializedHandles.TryDequeue(out result))
		{
			freeHandles.Enqueue(result.safeHandle);
		}
		handlesToRemove = new ConcurrentQueue<ChunkKey>();
		activeHandles.Clear();
		modifiedChunks.Clear();
	}

	public void Cleanup()
	{
		changeApplier?.Cleanup();
		changeApplier = null;
		groundWaterHeightMap = null;
		if (!IsInitialized)
		{
			return;
		}
		Clear();
		NativeSafeHandle<WaterDataHandle> result;
		while (freeHandles.TryDequeue(out result))
		{
			result.Dispose();
		}
		foreach (NativeSafeHandle<WaterDataHandle> value in usedHandles.Values)
		{
			value.Dispose();
		}
		usedHandles.Clear();
		if (activeHandles.IsCreated)
		{
			activeHandles.Dispose();
		}
		if (waterDataHandles.IsCreated)
		{
			waterDataHandles.Dispose();
		}
		if (modifiedChunks.IsCreated)
		{
			modifiedChunks.Dispose();
		}
		IsInitialized = false;
	}

	public string GetMemoryStats()
	{
		int count = usedHandles.Count;
		int count2 = freeHandles.Count;
		int count3 = newInitializedHandles.Count;
		int num = 0;
		foreach (NativeSafeHandle<WaterDataHandle> value in usedHandles.Values)
		{
			num += value.Target.CalculateOwnedBytes();
		}
		foreach (NativeSafeHandle<WaterDataHandle> freeHandle in freeHandles)
		{
			num += freeHandle.Target.CalculateOwnedBytes();
		}
		foreach (HandleInitRequest newInitializedHandle in newInitializedHandles)
		{
			int num2 = num;
			NativeSafeHandle<WaterDataHandle> safeHandle = newInitializedHandle.safeHandle;
			num = num2 + safeHandle.Target.CalculateOwnedBytes();
		}
		int num3 = ProfilerUtils.CalculateUnsafeParallelHashSetBytes(activeHandles);
		num3 += ProfilerUtils.CalculateUnsafeParallelHashMapBytes(waterDataHandles);
		return $"Allocated Handles: {count + count2 + count3}, Used Handles: {count}, Free Handles: {count2}, Pending Handles: {count3}, Handle Contents (MB): {(double)num * 9.5367431640625E-07:F2}, Other Memory (MB): {(double)num3 * 9.5367431640625E-07:F2}, Total Memory (MB): {(double)(num + num3) * 9.5367431640625E-07:F2}";
	}

	public string GetMemoryStatsDetailed()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Used Handles:");
		foreach (KeyValuePair<ChunkKey, NativeSafeHandle<WaterDataHandle>> usedHandle in usedHandles)
		{
			stringBuilder.AppendFormat("Chunk ({0},{1}): {2}\n", usedHandle.Key.x, usedHandle.Key.z, usedHandle.Value.Target.GetMemoryStats());
		}
		return stringBuilder.ToString();
	}
}
