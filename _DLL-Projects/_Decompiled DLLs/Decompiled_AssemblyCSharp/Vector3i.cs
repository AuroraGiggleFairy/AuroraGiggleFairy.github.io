using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public struct Vector3i : IEquatable<Vector3i>
{
	public static readonly Vector3i invalid;

	public static readonly Vector3i up;

	public static readonly Vector3i down;

	public static readonly Vector3i zero;

	public static readonly Vector3i left;

	public static readonly Vector3i right;

	public static readonly Vector3i forward;

	public static readonly Vector3i back;

	public static readonly Vector3i one;

	public static readonly Vector3i min;

	public static readonly Vector3i max;

	public static readonly Vector3i[] MIDDLE_AND_ADJACENT_DIRECTIONS;

	public static readonly Vector3i[] AllDirections;

	public static readonly Vector3i[] AllDirectionsShuffled;

	public static readonly Vector3i[] MIDDLE_AND_HORIZONTAL_DIRECTIONS;

	public static readonly Vector3i[] HORIZONTAL_DIRECTIONS;

	public static readonly Vector3i[] MIDDLE_AND_HORIZONTAL_DIRECTIONS_DIAGONAL;

	public int x;

	public int y;

	public int z;

	public bool IsValid => this != invalid;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3i(int _x, int _y, int _z)
	{
		x = _x;
		y = _y;
		z = _z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3i(Vector3 _v)
	{
		x = (int)_v.x;
		y = (int)_v.y;
		z = (int)_v.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3i(float _x, float _y, float _z)
	{
		x = (int)_x;
		y = (int)_y;
		z = (int)_z;
	}

	public void FloorToInt(Vector3 _v)
	{
		x = Utils.Fastfloor(_v.x);
		y = Utils.Fastfloor(_v.y);
		z = Utils.Fastfloor(_v.z);
	}

	public void RoundToInt(Vector3 _v)
	{
		x = Mathf.RoundToInt(_v.x);
		y = Mathf.RoundToInt(_v.y);
		z = Mathf.RoundToInt(_v.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(int _x, int _y, int _z)
	{
		if (x == _x && y == _y)
		{
			return z == _z;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Equals(object obj)
	{
		Vector3i other = (Vector3i)obj;
		return Equals(other);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Vector3i other)
	{
		if (other.x == x && other.y == y)
		{
			return other.z == z;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Vector3i one, Vector3i other)
	{
		if (one.x == other.x && one.y == other.y)
		{
			return one.z == other.z;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
	{
		return x * 8976890 + y * 981131 + z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Vector3i one, Vector3i other)
	{
		return !(one == other);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i operator -(Vector3i one, Vector3i other)
	{
		return new Vector3i(one.x - other.x, one.y - other.y, one.z - other.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i operator +(Vector3i one, Vector3i other)
	{
		return new Vector3i(one.x + other.x, one.y + other.y, one.z + other.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i operator *(Vector3i a, float d)
	{
		return new Vector3i((float)a.x * d, (float)a.y * d, (float)a.z * d);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i operator *(float d, Vector3i a)
	{
		return new Vector3i((float)a.x * d, (float)a.y * d, (float)a.z * d);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i operator *(Vector3i a, int i)
	{
		return new Vector3i(a.x * i, a.y * i, a.z * i);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i operator *(int i, Vector3i a)
	{
		return new Vector3i(a.x * i, a.y * i, a.z * i);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i operator /(Vector3i a, int i)
	{
		return new Vector3i(a.x / i, a.y / i, a.z / i);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i operator &(Vector3i a, int i)
	{
		return new Vector3i(a.x & i, a.y & i, a.z & i);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i operator &(int i, Vector3i a)
	{
		return new Vector3i(a.x & i, a.y & i, a.z & i);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Volume()
	{
		return x * y * z;
	}

	public override string ToString()
	{
		return $"{x}, {y}, {z}";
	}

	public string ToStringNoBlanks()
	{
		return $"{x},{y},{z}";
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3 ToVector3()
	{
		return new Vector3(x, y, z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3 ToVector3Center()
	{
		return new Vector3((float)x + 0.5f, (float)y + 0.5f, (float)z + 0.5f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3 ToVector3CenterXZ()
	{
		return new Vector3((float)x + 0.5f, y, (float)z + 0.5f);
	}

	public static Vector3i Parse(string _s)
	{
		string[] array = _s.Split(',');
		if (array.Length != 3)
		{
			return zero;
		}
		return new Vector3i(int.Parse(array[0]), int.Parse(array[1]), int.Parse(array[2]));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i Cross(Vector3i _a, Vector3i _b)
	{
		return new Vector3i(_a.y * _b.z - _a.z * _b.y, _a.z * _b.x - _a.x * _b.z, _a.x * _b.y - _a.y * _b.x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i FromVector3Rounded(Vector3 _v)
	{
		return new Vector3i(Mathf.RoundToInt(_v.x), Mathf.RoundToInt(_v.y), Mathf.RoundToInt(_v.z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i Min(Vector3i v1, Vector3i v2)
	{
		return new Vector3i(Math.Min(v1.x, v2.x), Math.Min(v1.y, v2.y), Math.Min(v1.z, v2.z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i Max(Vector3i v1, Vector3i v2)
	{
		return new Vector3i(Math.Max(v1.x, v2.x), Math.Max(v1.y, v2.y), Math.Max(v1.z, v2.z));
	}

	public static void SortBoundingBoxEdges(ref Vector3i _minEdge, ref Vector3i _maxEdge)
	{
		Vector3i vector3i = _minEdge;
		Vector3i vector3i2 = _maxEdge;
		_minEdge = new Vector3i(Math.Min(vector3i.x, vector3i2.x), Math.Min(vector3i.y, vector3i2.y), Math.Min(vector3i.z, vector3i2.z));
		_maxEdge = new Vector3i(Math.Max(vector3i.x, vector3i2.x), Math.Max(vector3i.y, vector3i2.y), Math.Max(vector3i.z, vector3i2.z));
	}

	public static Vector3i Floor(Vector3 _v)
	{
		Vector3i result = default(Vector3i);
		result.x = Utils.Fastfloor(_v.x);
		result.y = Utils.Fastfloor(_v.y);
		result.z = Utils.Fastfloor(_v.z);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Vector3(Vector3i _v3i)
	{
		return _v3i.ToVector3();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Vector3Int(Vector3i _v3i)
	{
		return new Vector3Int(_v3i.x, _v3i.y, _v3i.z);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static Vector3i()
	{
		invalid = new Vector3i(int.MinValue, int.MinValue, int.MinValue);
		up = new Vector3i(0, 1, 0);
		down = new Vector3i(0, -1, 0);
		zero = new Vector3i(0, 0, 0);
		left = new Vector3i(-1, 0, 0);
		right = new Vector3i(1, 0, 0);
		forward = new Vector3i(0, 0, 1);
		back = new Vector3i(0, 0, -1);
		one = new Vector3i(1, 1, 1);
		min = new Vector3i(int.MinValue, int.MinValue, int.MinValue);
		max = new Vector3i(int.MaxValue, int.MaxValue, int.MaxValue);
		MIDDLE_AND_ADJACENT_DIRECTIONS = new Vector3i[7]
		{
			zero,
			new Vector3i(1, 0, 0),
			new Vector3i(-1, 0, 0),
			new Vector3i(0, 1, 0),
			new Vector3i(0, -1, 0),
			new Vector3i(0, 0, 1),
			new Vector3i(0, 0, -1)
		};
		AllDirections = new Vector3i[6] { right, left, up, down, forward, back };
		AllDirectionsShuffled = new Vector3i[6] { right, left, up, down, forward, back };
		MIDDLE_AND_HORIZONTAL_DIRECTIONS = new Vector3i[5]
		{
			zero,
			new Vector3i(1, 0, 0),
			new Vector3i(-1, 0, 0),
			new Vector3i(0, 0, 1),
			new Vector3i(0, 0, -1)
		};
		HORIZONTAL_DIRECTIONS = new Vector3i[4]
		{
			new Vector3i(1, 0, 0),
			new Vector3i(-1, 0, 0),
			new Vector3i(0, 0, 1),
			new Vector3i(0, 0, -1)
		};
		MIDDLE_AND_HORIZONTAL_DIRECTIONS_DIAGONAL = new Vector3i[9]
		{
			zero,
			new Vector3i(1, 0, 0),
			new Vector3i(1, 0, -1),
			new Vector3i(0, 0, -1),
			new Vector3i(-1, 0, -1),
			new Vector3i(-1, 0, 0),
			new Vector3i(-1, 0, 1),
			new Vector3i(0, 0, 1),
			new Vector3i(1, 0, 1)
		};
	}
}
