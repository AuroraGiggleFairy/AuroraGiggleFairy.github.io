using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public struct Vector2d : IEquatable<Vector2d>
{
	public double x;

	public double y;

	public static readonly Vector2d Zero;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2d(double _x, double _y)
	{
		x = _x;
		y = _y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2d(Vector2 _v)
	{
		x = _v.x;
		y = _v.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2d(Vector2i _v)
	{
		x = _v.x;
		y = _v.y;
	}

	public bool Equals(double _x, double _y)
	{
		if (x == _x)
		{
			return y == _y;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		Vector2d other = (Vector2d)obj;
		return Equals(other);
	}

	public bool Equals(Vector2d other)
	{
		if (other.x == x)
		{
			return other.y == y;
		}
		return false;
	}

	public static bool operator ==(Vector2d one, Vector2d other)
	{
		if (one.x == other.x)
		{
			return one.y == other.y;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() ^ (y.GetHashCode() << 2);
	}

	public static bool operator !=(Vector2d one, Vector2d other)
	{
		return !(one == other);
	}

	public static Vector2d operator -(Vector2d one, Vector2d other)
	{
		return new Vector2d(one.x - other.x, one.y - other.y);
	}

	public static Vector2d operator +(Vector2d one, Vector2d other)
	{
		return new Vector2d(one.x + other.x, one.y + other.y);
	}

	public static Vector2d operator *(Vector2d a, double d)
	{
		return new Vector2d(a.x * d, a.y * d);
	}

	public static Vector2d operator *(double d, Vector2d a)
	{
		return new Vector2d(a.x * d, a.y * d);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Dot(Vector2d lhs, Vector2d rhs)
	{
		return lhs.x * rhs.x + lhs.y * rhs.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public double Dot(Vector2 rhs)
	{
		return x * (double)rhs.x + y * (double)rhs.y;
	}

	public override string ToString()
	{
		return ToCultureInvariantString();
	}

	public string ToCultureInvariantString()
	{
		return "(" + x.ToCultureInvariantString("F1") + ", " + y.ToCultureInvariantString("F1") + ")";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static Vector2d()
	{
		Zero = new Vector2d(0.0, 0.0);
	}
}
