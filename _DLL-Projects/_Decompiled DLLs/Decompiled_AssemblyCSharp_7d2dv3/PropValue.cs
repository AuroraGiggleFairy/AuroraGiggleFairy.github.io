using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public struct PropValue : IEquatable<PropValue>
{
	public static readonly PropValue AIR = new PropValue
	{
		transform = PropTransform.identity,
		blockValue = BlockValue.Air
	};

	public PropTransform transform;

	public BlockValue blockValue;

	public Vector3 position
	{
		get
		{
			return transform.position;
		}
		set
		{
			transform.position = value;
		}
	}

	public Quaternion rotation
	{
		get
		{
			return transform.rotation;
		}
		set
		{
			transform.rotation = value;
		}
	}

	public Vector3 scale
	{
		get
		{
			return transform.scale;
		}
		set
		{
			transform.scale = value;
		}
	}

	public bool IsAir
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return blockValue.isair;
		}
	}

	public static PropValue Read(BinaryReader br)
	{
		return new PropValue
		{
			transform = PropTransform.Read(br),
			blockValue = BlockValue.Read(br)
		};
	}

	public readonly void Write(BinaryWriter bw)
	{
		transform.Write(bw);
		blockValue.Write(bw);
	}

	public override string ToString()
	{
		return $"transform=[{transform}], blockValue=[{blockValue}]";
	}

	public bool Equals(PropValue other)
	{
		if (transform.Equals(other.transform))
		{
			return blockValue.Equals(other.blockValue);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is PropValue other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(transform, blockValue);
	}

	public static bool operator ==(PropValue left, PropValue right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(PropValue left, PropValue right)
	{
		return !left.Equals(right);
	}
}
