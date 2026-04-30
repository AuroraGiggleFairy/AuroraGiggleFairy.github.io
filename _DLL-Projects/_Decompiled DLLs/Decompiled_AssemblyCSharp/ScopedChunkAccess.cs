public static class ScopedChunkAccess
{
	public static ScopedChunkReadAccess GetChunkReadAccess(ChunkCluster chunks, int x, int z)
	{
		return new ScopedChunkReadAccess(chunks.GetChunkSync(x, z));
	}

	public static ScopedChunkWriteAccess GetChunkWriteAccess(ChunkCluster chunks, int x, int z)
	{
		return new ScopedChunkWriteAccess(chunks.GetChunkSync(x, z));
	}

	public static ScopedChunkWriteAccess GetChunkWriteAccess(ChunkCluster chunks, long key)
	{
		return new ScopedChunkWriteAccess(chunks.GetChunkSync(key));
	}
}
