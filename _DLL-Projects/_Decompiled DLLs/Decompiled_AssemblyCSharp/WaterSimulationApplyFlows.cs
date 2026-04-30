using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(CompileSynchronously = true)]
public struct WaterSimulationApplyFlows : IJobParallelFor
{
	public NativeArray<ChunkKey> processingChunks;

	public NativeList<ChunkKey>.ParallelWriter nonFlowingChunks;

	public UnsafeParallelHashSet<ChunkKey>.ParallelWriter activeChunkSet;

	public UnsafeParallelHashMap<ChunkKey, WaterDataHandle> waterDataHandles;

	public NativeArray<WaterStats> waterStats;

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterNeighborCacheNative neighborCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterStats stats;

	public void Execute(int chunkIndex)
	{
		neighborCache = WaterNeighborCacheNative.InitializeCache(waterDataHandles);
		ChunkKey chunkKey = processingChunks[chunkIndex];
		stats = waterStats[chunkIndex];
		if (waterDataHandles.TryGetValue(chunkKey, out var item))
		{
			item.ApplyEnqueuedFlows();
			if (item.flowVoxels.IsEmpty)
			{
				nonFlowingChunks.AddNoResize(chunkKey);
			}
			else
			{
				neighborCache.SetChunk(chunkKey);
				UnsafeParallelHashMap<int, int>.Enumerator enumerator = item.flowVoxels.GetEnumerator();
				while (enumerator.MoveNext())
				{
					int key = enumerator.Current.Key;
					int value = enumerator.Current.Value;
					int3 voxelCoords = WaterDataHandle.GetVoxelCoords(key);
					int num = item.voxelData.Get(key);
					if (item.IsInGroundWater(voxelCoords.x, voxelCoords.y, voxelCoords.z))
					{
						num = math.min(value, 19500);
					}
					else
					{
						if (value == 0)
						{
							continue;
						}
						num += value;
					}
					item.voxelData.Set(key, num);
					item.SetVoxelActive(key);
					neighborCache.SetVoxel(voxelCoords.x, voxelCoords.y, voxelCoords.z);
					WakeNeighbor(WaterNeighborCacheNative.X_NEG);
					WakeNeighbor(WaterNeighborCacheNative.X_POS);
					WakeNeighbor(1);
					WakeNeighbor(-1);
					WakeNeighbor(WaterNeighborCacheNative.Z_NEG);
					WakeNeighbor(WaterNeighborCacheNative.Z_POS);
				}
				activeChunkSet.Add(chunkKey);
			}
		}
		waterStats[chunkIndex] = stats;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WakeNeighbor(int _yOffset)
	{
		int num = neighborCache.voxelY + _yOffset;
		if (num >= 0 && num <= 255)
		{
			neighborCache.center.SetVoxelActive(neighborCache.voxelX, num, neighborCache.voxelZ);
			stats.NumVoxelsWokeUp++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WakeNeighbor(int2 _xzOffset)
	{
		if (neighborCache.TryGetNeighbor(_xzOffset, out var _chunkKey, out var _dataHandle, out var _x, out var _y, out var _z))
		{
			if (_chunkKey.Equals(neighborCache.chunkKey))
			{
				_dataHandle.SetVoxelActive(_x, _y, _z);
			}
			else
			{
				_dataHandle.EnqueueVoxelActive(_x, _y, _z);
				activeChunkSet.Add(_chunkKey);
			}
			stats.NumVoxelsWokeUp++;
		}
	}
}
