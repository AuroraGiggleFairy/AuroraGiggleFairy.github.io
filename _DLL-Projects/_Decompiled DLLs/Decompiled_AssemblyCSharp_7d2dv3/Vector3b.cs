using System;
using System.Runtime.CompilerServices;

public struct Vector3b : IEquatable<Vector3b>
{
	public byte x;

	public byte y;

	public byte z;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3b(byte _x, byte _y, byte _z)
	{
		x = _x;
		y = _y;
		z = _z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3b(int _x, int _y, int _z)
	{
		x = (byte)_x;
		y = (byte)_y;
		z = (byte)_z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3i ToVector3i()
	{
		return new Vector3i(x, y, z);
	}

	public override int GetHashCode()
	{
		return (x << 16) | (y << 8) | z;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is Vector3b)
		{
			return Equals((Vector3b)obj);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Vector3b other)
	{
		if (x == other.x && y == other.y)
		{
			return z == other.z;
		}
		return false;
	}
}
