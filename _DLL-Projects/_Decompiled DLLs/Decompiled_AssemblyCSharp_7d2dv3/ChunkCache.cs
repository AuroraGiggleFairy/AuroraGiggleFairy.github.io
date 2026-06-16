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

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsChunkValid(Chunk _chunk)
	{
		return _chunk?.IsInitialized ?? false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetChunk(int _cX, int _cY, out Chunk _chunk)
	{
		int num = _cX - chunkX;
		int num2 = _cY - chunkZ;
		Chunk chunk = ((num >= 0 && num < chunkArray.GetLength(0) && num2 >= 0 && num2 < chunkArray.GetLength(1)) ? chunkArray[num, num2] : worldObj.ChunkCache.GetChunkSync(_cX, _cY));
		if (!IsChunkValid(chunk))
		{
			_chunk = null;
			return false;
		}
		_chunk = chunk;
		return true;
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
		if (!TryGetChunk(World.toChunkXZ(_x), World.toChunkXZ(_z), out var _chunk))
		{
			return BlockValue.Air;
		}
		return _chunk.GetBlock(_x & 0xF, _y, _z & 0xF);
	}

	public BlockValue GetBlock(Vector3i pos)
	{
		return IBlockAccess.DefaultGetBlock(this, pos);
	}

	public BlockValue GetBlock(BlockValueRef bvRef)
	{
		return IBlockAccess.DefaultGetBlock(this, bvRef);
	}

	public PropValue GetProp(int chunkX, int chunkZ, int propId)
	{
		if (!TryGetChunk(chunkX, chunkZ, out var _chunk))
		{
			return PropValue.AIR;
		}
		return _chunk.GetProp(chunkX, chunkZ, propId);
	}

	public PropValue GetProp(long chunkKey, int propId)
	{
		return GetProp(WorldChunkCache.extractX(chunkKey), WorldChunkCache.extractZ(chunkKey), propId);
	}

	public PropValue GetProp(Vector2i chunkPos, int propId)
	{
		return IBlockAccess.DefaultGetProp(this, chunkPos, propId);
	}

	public PropValue GetProp(PropRef propRef)
	{
		return IBlockAccess.DefaultGetProp(this, propRef);
	}
}
