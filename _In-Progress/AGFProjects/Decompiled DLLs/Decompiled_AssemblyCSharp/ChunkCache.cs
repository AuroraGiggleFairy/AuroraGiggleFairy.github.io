using System;

public class ChunkCache : IBlockAccess
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk[,] chunkArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public World worldObj;

	public ChunkCache(int _dim)
	{
		chunkArray = new Chunk[_dim, _dim];
	}

	public void Init(World world, int x, int y, int z, int sx, int sy, int sz)
	{
		worldObj = world;
		chunkX = x >> 4;
		chunkZ = z >> 4;
		int num = sx >> 4;
		int num2 = sz >> 4;
		for (int i = chunkX; i <= num; i++)
		{
			for (int j = chunkZ; j <= num2; j++)
			{
				Chunk chunkSync = world.ChunkCache.GetChunkSync(i, j);
				if (chunkSync != null)
				{
					chunkArray[i - chunkX, j - chunkZ] = chunkSync;
				}
			}
		}
	}

	public void Clear()
	{
		Array.Clear(chunkArray, 0, chunkArray.GetLength(0) * chunkArray.GetLength(1));
	}

	public BlockValue GetBlock(Vector3i _pos)
	{
		return GetBlock(_pos.x, _pos.y, _pos.z);
	}

	public BlockValue GetBlock(int _x, int _y, int _z)
	{
		if (_y < 0)
		{
			return BlockValue.Air;
		}
		if (_y >= 256)
		{
			return BlockValue.Air;
		}
		int num = (_x >> 4) - chunkX;
		int num2 = (_z >> 4) - chunkZ;
		if (num < 0 || num >= chunkArray.GetLength(0) || num2 < 0 || num2 >= chunkArray.GetLength(1))
		{
			Chunk chunkSync = worldObj.ChunkCache.GetChunkSync(World.toChunkXZ(_x), World.toChunkXZ(_z));
			if (chunkSync == null)
			{
				return BlockValue.Air;
			}
			if (!chunkSync.IsInitialized)
			{
				return BlockValue.Air;
			}
			return chunkSync.GetBlock(_x & 0xF, _y, _z & 0xF);
		}
		Chunk chunk = chunkArray[num, num2];
		if (chunk == null)
		{
			return BlockValue.Air;
		}
		if (!chunk.IsInitialized)
		{
			return BlockValue.Air;
		}
		return chunk.GetBlock(_x & 0xF, _y, _z & 0xF);
	}
}
