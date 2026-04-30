using UnityEngine;

namespace GamePath;

public class PathPoint
{
	public Vector3 projectedLocation;

	[PublicizedFrom(EAccessModifier.Private)]
	public int x;

	[PublicizedFrom(EAccessModifier.Private)]
	public int y;

	[PublicizedFrom(EAccessModifier.Private)]
	public int z;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hash;

	public static PathPoint Allocate(Vector3 _pos)
	{
		lock (MemoryPools.s_pool)
		{
			PathPoint pathPoint = MemoryPools.s_pool.Allocate();
			pathPoint.x = (int)_pos.x;
			pathPoint.y = (int)_pos.y;
			pathPoint.z = (int)_pos.z;
			pathPoint.projectedLocation = _pos;
			pathPoint.hash = makeHash(pathPoint.x, pathPoint.y, pathPoint.z);
			return pathPoint;
		}
	}

	public static void CompactPool()
	{
		lock (MemoryPools.s_pool)
		{
			MemoryPools.s_pool.Compact();
		}
	}

	public void Release()
	{
		lock (MemoryPools.s_pool)
		{
			MemoryPools.s_pool.Free(this);
		}
	}

	public static int makeHash(int _x, int _y, int _z)
	{
		return (_y & 0xFF) | ((_x & 0x7FFF) << 8) | ((_z & 0x7FFF) << 24) | ((_x < 0) ? int.MinValue : 0) | ((_z < 0) ? 32768 : 0);
	}

	public override bool Equals(object _obj)
	{
		if (_obj is PathPoint pathPoint)
		{
			if (hash == pathPoint.hash)
			{
				return IsSamePos(pathPoint);
			}
			return false;
		}
		return false;
	}

	public bool IsSamePos(PathPoint _p)
	{
		if (_p.x == x && _p.y == y)
		{
			return _p.z == z;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return hash;
	}

	public float GetDistanceSq(int _x, int _y, int _z)
	{
		int num = x - _x;
		int num2 = y - _y;
		int num3 = z - _z;
		return num * num + num2 * num2 + num3 * num3;
	}

	public Vector3 AdjustedPositionForEntity(Entity entity)
	{
		return projectedLocation;
	}

	public Vector3 ProjectToGround(Entity entity)
	{
		return projectedLocation;
	}

	public Vector3i GetBlockPos()
	{
		return World.worldToBlockPos(projectedLocation);
	}

	public string toString()
	{
		return x + ", " + y + ", " + z;
	}
}
