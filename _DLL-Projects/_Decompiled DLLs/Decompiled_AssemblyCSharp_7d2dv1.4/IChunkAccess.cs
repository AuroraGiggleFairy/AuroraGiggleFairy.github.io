public interface IChunkAccess
{
	IChunk GetChunkSync(int chunkX, int chunkY, int chunkZ);

	IChunk GetChunkFromWorldPos(int x, int y, int z);

	IChunk GetChunkFromWorldPos(Vector3i _blockPos);
}
