public interface IRegionFileChunkSnapshotUtil
{
	IRegionFileChunkSnapshot TakeSnapshot(Chunk chunk, bool saveIfUnchanged);

	void WriteSnapshot(IRegionFileChunkSnapshot snapshot, string dir, int chunkX, int chunkZ);

	Chunk LoadChunk(string dir, long key);

	void Free(IRegionFileChunkSnapshot snapshot);

	void Cleanup();
}
