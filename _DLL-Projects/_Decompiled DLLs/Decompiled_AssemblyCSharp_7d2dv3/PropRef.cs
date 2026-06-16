using System;
using System.IO;

public struct PropRef : IEquatable<PropRef>
{
	public static readonly PropRef Default = new PropRef
	{
		ChunkPos = Vector2i.zero,
		PropId = 0
	};

	public Vector2i ChunkPos;

	public int PropId;

	public static PropRef Read(BinaryReader br)
	{
		return new PropRef
		{
			ChunkPos = StreamUtils.ReadVector2i(br),
			PropId = br.ReadInt32()
		};
	}

	public readonly void Write(BinaryWriter bw)
	{
		StreamUtils.Write(bw, ChunkPos);
		bw.Write(PropId);
	}

	public override string ToString()
	{
		return $"ChunkPos=({ChunkPos}), PropId={PropId}";
	}

	public bool Equals(PropRef other)
	{
		if (ChunkPos.Equals(other.ChunkPos))
		{
			return PropId == other.PropId;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is PropRef other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(ChunkPos, PropId);
	}

	public static bool operator ==(PropRef left, PropRef right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(PropRef left, PropRef right)
	{
		return !left.Equals(right);
	}
}
