using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile(CompileSynchronously = true)]
public struct WaterSimulationPreProcess : IJob
{
	public UnsafeParallelHashSet<ChunkKey> activeChunks;

	public UnsafeParallelHashMap<ChunkKey, WaterDataHandle> waterDataHandles;

	public UnsafeParallelHashSet<ChunkKey> modifiedChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterNeighborCacheNative neighborCache;

	public void Execute()
	{
		neighborCache = WaterNeighborCacheNative.InitializeCache(waterDataHandles);
		using UnsafeParallelHashSet<ChunkKey>.Enumerator enumerator = modifiedChunks.GetEnumerator();
		while (enumerator.MoveNext())
		{
			ChunkKey current = enumerator.Current;
			if (!waterDataHandles.TryGetValue(current, out var item))
			{
				continue;
			}
			neighborCache.SetChunk(current);
			int num = 0;
			using UnsafeParallelHashSet<int>.Enumerator enumerator2 = item.voxelsToWakeup.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				if (++num > 65536)
				{
					Debug.LogError($"[WaterSimulationPreProcess] Number of wakeups for chunk ({current.x}, {current.z}) has exceeded the volume of a chunk {65536}.");
					break;
				}
				int3 voxelCoords = WaterDataHandle.GetVoxelCoords(enumerator2.Current);
				item.SetVoxelActive(voxelCoords.x, voxelCoords.y, voxelCoords.z);
				neighborCache.SetVoxel(voxelCoords.x, voxelCoords.y, voxelCoords.z);
				WakeNeighbor(WaterNeighborCacheNative.X_NEG);
				WakeNeighbor(WaterNeighborCacheNative.X_POS);
				WakeNeighbor(1);
				WakeNeighbor(-1);
				WakeNeighbor(WaterNeighborCacheNative.Z_NEG);
				WakeNeighbor(WaterNeighborCacheNative.Z_POS);
			}
			item.voxelsToWakeup.Clear();
			activeChunks.Add(current);
		}
		modifiedChunks.Clear();
		using NativeArray<ChunkKey> nativeArray = activeChunks.ToNativeArray(Allocator.Temp);
		for (int i = 0; i < nativeArray.Length; i++)
		{
			ChunkKey chunkKey = nativeArray[i];
			TryTrackChunk(chunkKey.x + 1, chunkKey.z);
			TryTrackChunk(chunkKey.x - 1, chunkKey.z);
			TryTrackChunk(chunkKey.x, chunkKey.z + 1);
			TryTrackChunk(chunkKey.x, chunkKey.z - 1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WakeNeighbor(int _yOffset)
	{
		int num = neighborCache.voxelY + _yOffset;
		if (num >= 0 && num <= 255)
		{
			neighborCache.center.SetVoxelActive(neighborCache.voxelX, num, neighborCache.voxelZ);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WakeNeighbor(int2 _xzOffset)
	{
		if (neighborCache.TryGetNeighbor(_xzOffset, out var _, out var _dataHandle, out var _x, out var _y, out var _z))
		{
			_dataHandle.SetVoxelActive(_x, _y, _z);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TryTrackChunk(int _chunkX, int _chunkZ)
	{
		ChunkKey chunkKey = new ChunkKey(_chunkX, _chunkZ);
		if (waterDataHandles.ContainsKey(chunkKey))
		{
			activeChunks.Add(chunkKey);
		}
	}
}
