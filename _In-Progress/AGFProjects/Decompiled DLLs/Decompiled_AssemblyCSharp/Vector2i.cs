using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public struct Vector2i : IEquatable<Vector2i>
{
	public static readonly Vector2i zero;

	public static readonly Vector2i one;

	public static readonly Vector2i min;

	public static readonly Vector2i max;

	public static readonly Vector2i up;

	public static readonly Vector2i down;

	public static readonly Vector2i right;

	public static readonly Vector2i left;

	public int x;

	public int y;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2i(int _x, int _y)
	{
		x = _x;
		y = _y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2i(Vector2 vector2)
	{
		this = default(Vector2i);
		x = Mathf.FloorToInt(vector2.x);
		y = Mathf.FloorToInt(vector2.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2i(Vector2Int vector2)
	{
		this = default(Vector2i);
		x = vector2.x;
		y = vector2.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Set(int _x, int _y)
	{
		x = _x;
		y = _y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2Int AsVector2Int()
	{
		return new Vector2Int(x, y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2 AsVector2()
	{
		return new Vector2(x, y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Equals(object obj)
	{
		Vector2i other = (Vector2i)obj;
		return Equals(other);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Vector2i other)
	{
		if (other.x == x)
		{
			return other.y == y;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Distance(Vector2i a, Vector2i b)
	{
		double num = a.x - b.x;
		double num2 = a.y - b.y;
		return (float)Math.Sqrt(num * num + num2 * num2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistanceSqr(Vector2i a, Vector2i b)
	{
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		return num * num + num2 * num2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int DistanceSqrInt(Vector2i a, Vector2i b)
	{
		int num = a.x - b.x;
		int num2 = a.y - b.y;
		return num * num + num2 * num2;
	}

	public void Normalize()
	{
		if (x < 0)
		{
			x = -1;
		}
		else if (x > 0)
		{
			x = 1;
		}
		if (y < 0)
		{
			y = -1;
		}
		else if (y > 0)
		{
			y = 1;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Vector2i one, Vector2i other)
	{
		if (one.x == other.x)
		{
			return one.y == other.y;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
	{
		return x * 8976890 + y * 981131;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Vector2i one, Vector2i other)
	{
		return !(one == other);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2i operator +(Vector2i one, Vector2i other)
	{
		return new Vector2i(one.x + other.x, one.y + other.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2i operator -(Vector2i one, Vector2i other)
	{
		return new Vector2i(one.x - other.x, one.y - other.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2i operator /(Vector2i one, int div)
	{
		return new Vector2i(one.x / div, one.y / div);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2i operator *(Vector2i a, int i)
	{
		return new Vector2i(a.x * i, a.y * i);
	}

	public override string ToString()
	{
		return $"{x}, {y}";
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Vector2Int(Vector2i _v2i)
	{
		return new Vector2Int(_v2i.x, _v2i.y);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static Vector2i()
	{
		zero = new Vector2i(0, 0);
		one = new Vector2i(1, 1);
		min = new Vector2i(int.MinValue, int.MinValue);
		max = new Vector2i(int.MaxValue, int.MaxValue);
		up = new Vector2i(0, 1);
		down = new Vector2i(0, -1);
		right = new Vector2i(1, 0);
		left = new Vector2i(-1, 0);
	}
}
