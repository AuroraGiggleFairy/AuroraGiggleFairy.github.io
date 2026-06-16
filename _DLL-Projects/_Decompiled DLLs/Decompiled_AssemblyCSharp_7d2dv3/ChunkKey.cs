using System;

public struct ChunkKey : IEquatable<ChunkKey>
{
	public int x;

	public int z;

	public ChunkKey(IChunk _chunk)
	{
		x = _chunk.X;
		z = _chunk.Z;
	}

	public ChunkKey(int _x, int _z)
	{
		x = _x;
		z = _z;
	}

	public override int GetHashCode()
	{
		return WaterUtils.GetVoxelKey2D(x, z);
	}

	public override bool Equals(object obj)
	{
		return base.Equals((object)(ChunkKey)obj);
	}

	public bool Equals(ChunkKey other)
	{
		if (x == other.x)
		{
			return z == other.z;
		}
		return false;
	}
}
