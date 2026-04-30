public static class WaterUtils
{
	public static int GetVoxelKey2D(int _x, int _z)
	{
		return _x * 8976890 + _z * 981131;
	}

	public static int GetVoxelKey(int _x, int _y, int _z = 0)
	{
		return _x * 8976890 + _y * 981131 + _z;
	}

	public static bool IsChunkSafeToUpdate(Chunk chunk)
	{
		if (chunk != null && !chunk.NeedsDecoration && !chunk.NeedsCopying)
		{
			return !chunk.IsLocked;
		}
		return false;
	}

	public static bool TryOpenChunkForUpdate(ChunkCluster _chunks, long _key, out Chunk _chunk)
	{
		using ScopedChunkWriteAccess scopedChunkWriteAccess = ScopedChunkAccess.GetChunkWriteAccess(_chunks, _key);
		Chunk chunk = scopedChunkWriteAccess.Chunk;
		if (!IsChunkSafeToUpdate(chunk))
		{
			_chunk = null;
			return false;
		}
		_chunk = chunk;
		_chunk.InProgressWaterSim = true;
		return true;
	}

	public static bool CanWaterFlowThrough(BlockValue _bv)
	{
		Block block = _bv.Block;
		if (block != null)
		{
			return block.WaterFlowMask != BlockFaceFlag.All;
		}
		return false;
	}

	public static bool CanWaterFlowThrough(int _blockId)
	{
		Block block = Block.list[_blockId];
		if (block != null)
		{
			return block.WaterFlowMask != BlockFaceFlag.All;
		}
		return false;
	}

	public static int GetWaterLevel(WaterValue waterValue)
	{
		if (waterValue.GetMass() > 195)
		{
			return 1;
		}
		return 0;
	}

	public static bool IsVoxelOutsideChunk(int _neighborX, int _neighborZ)
	{
		if (_neighborX >= 0 && _neighborX <= 15 && _neighborZ >= 0)
		{
			return _neighborZ > 15;
		}
		return true;
	}
}
