using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

[BurstCompile(CompileSynchronously = true)]
public struct WaterSimulationPostProcess : IJob
{
	public NativeArray<ChunkKey> processingChunks;

	public NativeList<ChunkKey> nonFlowingChunks;

	public UnsafeParallelHashSet<ChunkKey> activeChunks;

	public UnsafeParallelHashMap<ChunkKey, WaterDataHandle> waterDataHandles;

	public void Execute()
	{
		for (int i = 0; i < nonFlowingChunks.Length; i++)
		{
			ChunkKey key = nonFlowingChunks[i];
			if (waterDataHandles.TryGetValue(key, out var _))
			{
				activeChunks.Remove(nonFlowingChunks[i]);
			}
		}
		for (int j = 0; j < processingChunks.Length; j++)
		{
			ChunkKey chunkKey = processingChunks[j];
			if (waterDataHandles.TryGetValue(chunkKey, out var item2) && item2.activationsFromOtherChunks.Count > 0)
			{
				item2.ApplyEnqueuedActivations();
				activeChunks.Add(chunkKey);
			}
		}
	}
}
