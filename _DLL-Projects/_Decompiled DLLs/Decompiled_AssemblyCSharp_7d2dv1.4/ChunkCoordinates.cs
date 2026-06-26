using UnityEngine;

public class ChunkCoordinates
{
	public Vector3i position;

	public ChunkCoordinates(int _x, int _y, int _z)
	{
		position = new Vector3i(_x, _y, _z);
	}

	public ChunkCoordinates(ChunkCoordinates _cc)
	{
		position = _cc.position;
	}

	public override bool Equals(object _obj)
	{
		if (!(_obj is ChunkCoordinates))
		{
			return false;
		}
		return position.Equals(((ChunkCoordinates)_obj).position);
	}

	public override int GetHashCode()
	{
		return position.x + position.z << 8 + position.y << 16;
	}

	public float getDistance(int _x, int _y, int _z)
	{
		int num = position.x - _x;
		int num2 = position.y - _y;
		int num3 = position.z - _z;
		return Mathf.Sqrt(num * num + num2 * num2 + num3 * num3);
	}

	public float getDistanceSquared(int _x, int _y, int _z)
	{
		int num = position.x - _x;
		int num2 = position.y - _y;
		int num3 = position.z - _z;
		return num * num + num2 * num2 + num3 * num3;
	}
}
