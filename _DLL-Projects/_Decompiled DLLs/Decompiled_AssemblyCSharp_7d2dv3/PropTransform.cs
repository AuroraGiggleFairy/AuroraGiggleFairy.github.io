using System;
using System.IO;
using UnityEngine;

public struct PropTransform : IEquatable<PropTransform>
{
	public static readonly PropTransform identity = new PropTransform
	{
		position = Vector3.zero,
		rotation = Quaternion.identity,
		scale = Vector3.one
	};

	public Vector3 position;

	public Quaternion rotation;

	public Vector3 scale;

	public static PropTransform Read(BinaryReader br)
	{
		return new PropTransform
		{
			position = StreamUtils.ReadVector3(br),
			rotation = StreamUtils.ReadQuaterion(br),
			scale = StreamUtils.ReadVector3(br)
		};
	}

	public readonly void Write(BinaryWriter bw)
	{
		StreamUtils.Write(bw, position);
		StreamUtils.Write(bw, rotation);
		StreamUtils.Write(bw, scale);
	}

	public override string ToString()
	{
		return $"position={position}, rotation={rotation}, scale={scale}";
	}

	public bool Equals(PropTransform other)
	{
		if (position.Equals(other.position) && rotation.Equals(other.rotation))
		{
			return scale.Equals(other.scale);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is PropTransform other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(position, rotation, scale);
	}

	public static bool operator ==(PropTransform left, PropTransform right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(PropTransform left, PropTransform right)
	{
		return !left.Equals(right);
	}
}
