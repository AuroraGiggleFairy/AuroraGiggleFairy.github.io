public interface IChunkAccess
{
	IChunk GetChunkFromWorldPos(int x, int y, int z);

	IChunk GetChunkFromWorldPos(Vector3i _blockPos);
}
