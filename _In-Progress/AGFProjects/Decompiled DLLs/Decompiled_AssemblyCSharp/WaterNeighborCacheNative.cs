using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

public struct WaterNeighborCacheNative
{
	public static readonly int2 X_NEG = new int2(-1, 0);

	public static readonly int2 X_POS = new int2(1, 0);

	public static readonly int2 Z_NEG = new int2(0, -1);

	public static readonly int2 Z_POS = new int2(0, 1);

	[PublicizedFrom(EAccessModifier.Private)]
	public UnsafeParallelHashMap<ChunkKey, WaterDataHandle> waterDataHandles;

	public ChunkKey chunkKey;

	public int voxelX;

	public int voxelY;

	public int voxelZ;

	public WaterDataHandle center;

	public static WaterNeighborCacheNative InitializeCache(UnsafeParallelHashMap<ChunkKey, WaterDataHandle> _handles)
	{
		return new WaterNeighborCacheNative
		{
			waterDataHandles = _handles
		};
	}

	public void SetChunk(ChunkKey _chunk)
	{
		chunkKey = _chunk;
		center = waterDataHandles[_chunk];
	}

	public void SetVoxel(int _x, int _y, int _z)
	{
		voxelX = _x;
		voxelY = _y;
		voxelZ = _z;
	}

	public bool TryGetNeighbor(int2 _xzOffset, out ChunkKey _chunkKey, out WaterDataHandle _dataHandle, out int _x, out int _y, out int _z)
	{
		_x = voxelX + _xzOffset.x;
		_y = voxelY;
		_z = voxelZ + _xzOffset.y;
		if (!WaterUtils.IsVoxelOutsideChunk(_x, _z))
		{
			_chunkKey = chunkKey;
			_dataHandle = center;
			return true;
		}
		int num = _x & 0xF;
		int num2 = _z & 0xF;
		int x = chunkKey.x + (_x - num) / 16;
		int z = chunkKey.z + (_z - num2) / 16;
		_chunkKey = new ChunkKey(x, z);
		if (waterDataHandles.TryGetValue(_chunkKey, out _dataHandle))
		{
			_x = num;
			_z = num2;
			return true;
		}
		_chunkKey = default(ChunkKey);
		_dataHandle = default(WaterDataHandle);
		return false;
	}
}
