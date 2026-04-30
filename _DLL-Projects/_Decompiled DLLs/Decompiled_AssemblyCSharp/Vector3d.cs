using System;
using UnityEngine;

public struct Vector3d : IEquatable<Vector3d>
{
	public double x;

	public double y;

	public double z;

	public static readonly Vector3d Zero;

	public Vector3d(double _x, double _y, double _z)
	{
		x = _x;
		y = _y;
		z = _z;
	}

	public Vector3d(Vector3 _v)
	{
		x = _v.x;
		y = _v.y;
		z = _v.z;
	}

	public Vector3d(Vector3i _v)
	{
		x = _v.x;
		y = _v.y;
		z = _v.z;
	}

	public bool Equals(double _x, double _y, double _z)
	{
		if (x == _x && y == _y)
		{
			return z == _z;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		Vector3d other = (Vector3d)obj;
		return Equals(other);
	}

	public bool Equals(Vector3d other)
	{
		if (other.x == x && other.y == y)
		{
			return other.z == z;
		}
		return false;
	}

	public static bool operator ==(Vector3d one, Vector3d other)
	{
		if (one.x == other.x && one.y == other.y)
		{
			return one.z == other.z;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
	}

	public static bool operator !=(Vector3d one, Vector3d other)
	{
		return !(one == other);
	}

	public static Vector3d operator -(Vector3d one, Vector3d other)
	{
		return new Vector3d(one.x - other.x, one.y - other.y, one.z - other.z);
	}

	public static Vector3d operator +(Vector3d one, Vector3d other)
	{
		return new Vector3d(one.x + other.x, one.y + other.y, one.z + other.z);
	}

	public static Vector3d operator *(Vector3d a, double d)
	{
		return new Vector3d(a.x * d, a.y * d, a.z * d);
	}

	public static Vector3d operator *(double d, Vector3d a)
	{
		return new Vector3d(a.x * d, a.y * d, a.z * d);
	}

	public override string ToString()
	{
		return ToCultureInvariantString();
	}

	public string ToCultureInvariantString()
	{
		return "(" + x.ToCultureInvariantString("F1") + ", " + y.ToCultureInvariantString("F1") + ", " + z.ToCultureInvariantString("F1") + ")";
	}

	public static Vector3d Cross(Vector3d _a, Vector3d _b)
	{
		return new Vector3d(_a.y * _b.z - _a.z * _b.y, _a.z * _b.x - _a.x * _b.z, _a.x * _b.y - _a.y * _b.x);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static Vector3d()
	{
		Zero = new Vector3d(0.0, 0.0, 0.0);
	}
}
