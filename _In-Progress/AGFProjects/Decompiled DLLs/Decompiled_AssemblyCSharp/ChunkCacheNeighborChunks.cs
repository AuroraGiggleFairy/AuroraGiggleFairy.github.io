using System;

public class ChunkCacheNeighborChunks
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IChunk[,] chunks = new Chunk[3, 3];

	[PublicizedFrom(EAccessModifier.Private)]
	public IChunkAccess chunkAccess;

	public IChunk this[int x, int y]
	{
		get
		{
			return chunks[x + 1, y + 1];
		}
		set
		{
			chunks[x + 1, y + 1] = value;
		}
	}

	public ChunkCacheNeighborChunks(IChunkAccess _chunkAccess)
	{
		chunkAccess = _chunkAccess;
	}

	public void Init(IChunk _chunk, IChunk[] _chunkArr)
	{
		this[0, 0] = _chunk;
		this[-1, 0] = _chunkArr[1];
		this[1, 0] = _chunkArr[0];
		this[0, -1] = _chunkArr[3];
		this[0, 1] = _chunkArr[2];
		this[-1, -1] = _chunkArr[5];
		this[1, -1] = _chunkArr[7];
		this[-1, 1] = _chunkArr[6];
		this[1, 1] = _chunkArr[4];
	}

	public void Clear()
	{
		Array.Clear(chunks, 0, chunks.GetLength(0) * chunks.GetLength(1));
	}
}
