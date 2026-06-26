using System;

public class ChunkCacheNeighborBlocks : INeighborBlockCache
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IChunk[] chunks = new Chunk[9];

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkCacheNeighborChunks nChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int centerBX = int.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int centerBZ = int.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int curChunkX = int.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int curChunkZ = int.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstException = true;

	public ChunkCacheNeighborBlocks(ChunkCacheNeighborChunks _nChunks)
	{
		nChunks = _nChunks;
	}

	public void Init(int _bX, int _bZ)
	{
		IChunk chunk = nChunks[0, 0];
		if (_bX != centerBX || _bZ != centerBZ || chunk.X != curChunkX || chunk.Z != curChunkZ)
		{
			centerBX = _bX;
			centerBZ = _bZ;
			curChunkX = chunk.X;
			curChunkZ = chunk.Z;
			chunks[0] = ((_bX <= 0) ? ((_bZ > 0) ? nChunks[-1, 0] : nChunks[-1, -1]) : ((_bZ > 0) ? chunk : nChunks[0, -1]));
			chunks[1] = ((_bZ > 0) ? chunk : nChunks[0, -1]);
			chunks[2] = ((_bX >= 15) ? ((_bZ > 0) ? nChunks[1, 0] : nChunks[1, -1]) : ((_bZ > 0) ? chunk : nChunks[0, -1]));
			chunks[3] = ((_bX > 0) ? chunk : nChunks[-1, 0]);
			chunks[4] = chunk;
			chunks[5] = ((_bX < 15) ? chunk : nChunks[1, 0]);
			chunks[6] = ((_bX <= 0) ? ((_bZ < 15) ? nChunks[-1, 0] : nChunks[-1, 1]) : ((_bZ < 15) ? chunk : nChunks[0, 1]));
			chunks[7] = ((_bZ < 15) ? chunk : nChunks[0, 1]);
			chunks[8] = ((_bX >= 15) ? ((_bZ < 15) ? nChunks[1, 0] : nChunks[1, 1]) : ((_bZ < 15) ? chunk : nChunks[0, 1]));
		}
	}

	public void Clear()
	{
		centerBX = int.MaxValue;
		centerBZ = int.MaxValue;
		curChunkX = int.MaxValue;
		curChunkZ = int.MaxValue;
		Array.Clear(chunks, 0, chunks.Length);
	}

	public IChunk GetChunk(int x, int z)
	{
		return chunks[x + 1 + (z + 1) * 3];
	}

	public IChunk GetNeighborChunk(int x, int z)
	{
		return nChunks[x, z];
	}

	public bool IsBlockInCache(int relx, int absy, int relz)
	{
		if (absy < 0 || absy > 255)
		{
			return false;
		}
		int num = relx + 1 + (relz + 1) * 3;
		if (num > 8)
		{
			return false;
		}
		return chunks[num] != null;
	}

	public Vector3i GetChunkPos(int relx, int absy, int relz)
	{
		return new Vector3i((centerBX + relx) & 0xF, absy, (centerBZ + relz) & 0xF);
	}

	public BlockValue Get(int relx, int absy, int relz)
	{
		int num = relx + 1 + (relz + 1) * 3;
		if (absy < 0 || (uint)num >= 9u)
		{
			return BlockValue.Air;
		}
		try
		{
			BlockValue result = chunks[num].GetBlock((centerBX + relx) & 0xF, absy, (centerBZ + relz) & 0xF);
			if (!GameManager.bShowDecorBlocks && result.Block.IsDecoration)
			{
				result = BlockValue.Air;
			}
			if (!GameManager.bShowLootBlocks)
			{
				if (result.Block is BlockLoot)
				{
					result = BlockValue.Air;
				}
				else if (result.Block is BlockCompositeTileEntity blockCompositeTileEntity && blockCompositeTileEntity.CompositeData.HasFeature<ITileEntityLootable>())
				{
					result = BlockValue.Air;
				}
			}
			return result;
		}
		catch (Exception)
		{
			if (firstException)
			{
				Log.Error("ChunkCacheNeighborBlocks.Get: relX=" + relx + " relz=" + relz + " len=" + chunks.Length);
				firstException = false;
			}
			return BlockValue.Air;
		}
	}

	public byte GetStab(int relx, int absy, int relz)
	{
		byte result = 0;
		try
		{
			result = chunks[relx + 1 + (relz + 1) * 3].GetStability((centerBX + relx) & 0xF, absy, (centerBZ + relz) & 0xF);
		}
		catch (Exception ex)
		{
			Log.Out("Bad ChunkCacheNeighborBlocks index (" + relx + ", " + absy + ", " + relz + "), \nException: " + ex.ToString());
		}
		return result;
	}

	public bool IsWater(int relx, int absy, int relz)
	{
		int num = relx + 1 + (relz + 1) * 3;
		if (absy < 0 || (uint)num >= 9u)
		{
			return false;
		}
		bool result = false;
		try
		{
			result = chunks[num].IsWater((centerBX + relx) & 0xF, absy, (centerBZ + relz) & 0xF);
		}
		catch (Exception ex)
		{
			Log.Out("Bad ChunkCacheNeighborBlocks index (" + relx + ", " + absy + ", " + relz + "), \nException: " + ex.ToString());
		}
		return result;
	}

	public bool IsAir(int relx, int absy, int relz)
	{
		int num = relx + 1 + (relz + 1) * 3;
		if (absy < 0 || (uint)num >= 9u)
		{
			return false;
		}
		int x = (centerBX + relx) & 0xF;
		int z = (centerBZ + relz) & 0xF;
		bool result = false;
		try
		{
			IChunk chunk = chunks[num];
			return chunk.GetBlock(x, absy, z).isair && !chunk.IsWater(x, absy, z);
		}
		catch (Exception ex)
		{
			Log.Out("Bad ChunkCacheNeighborBlocks index (" + relx + ", " + absy + ", " + relz + "), \nException: " + ex.ToString());
			return result;
		}
	}

	public override string ToString()
	{
		return $"BlockCache -- Chunk Pos ({curChunkX}, {curChunkZ}) -- Block Pos ({centerBX}, {centerBZ})";
	}
}
