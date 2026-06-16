using System;
using System.IO;
using UnityEngine;

public struct BlockValueRef(BlockValueRefType type, Vector3i blockPosition, PropRef propRef) : IEquatable<BlockValueRef>
{
	public static readonly BlockValueRef None = new BlockValueRef(BlockValueRefType.None, Vector3i.zero, default(PropRef));

	public readonly BlockValueRefType Type = type;

	public readonly Vector3i BlockPosition = blockPosition;

	public readonly PropRef PropReference = propRef;

	public BlockValueRef(int x, int y, int z)
		: this(new Vector3i(x, y, z))
	{
	}

	public BlockValueRef(Vector3i pos)
		: this(BlockValueRefType.Block, pos, default(PropRef))
	{
	}

	public BlockValueRef(PropRef propRef)
		: this(BlockValueRefType.Prop, Vector3i.zero, propRef)
	{
	}

	public bool TryGetBlockPos(out Vector3i pos)
	{
		if (Type != BlockValueRefType.Block)
		{
			pos = Vector3i.zero;
			return false;
		}
		pos = BlockPosition;
		return true;
	}

	public bool TryGetPropRef(out PropRef propRef)
	{
		if (Type != BlockValueRefType.Prop)
		{
			propRef = PropRef.Default;
			return false;
		}
		propRef = PropReference;
		return true;
	}

	public Vector3 ToVector3(WorldBase world)
	{
		return Type switch
		{
			BlockValueRefType.None => Vector3.zero, 
			BlockValueRefType.Block => BlockPosition.ToVector3(), 
			BlockValueRefType.Prop => (world.GetChunkSync(PropReference)?.GetWorldPos() ?? Vector3i.zero) + world.GetProp(PropReference).position, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public Vector3 ToVector3Center(WorldBase world)
	{
		return Type switch
		{
			BlockValueRefType.None => Vector3.zero, 
			BlockValueRefType.Block => BlockPosition.ToVector3Center(), 
			BlockValueRefType.Prop => (world.GetChunkSync(PropReference)?.GetWorldPos() ?? Vector3i.zero) + world.GetProp(PropReference).position + new Vector3(0.5f, 0.5f, 0.5f), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public Vector3 ToVector3CenterXZ(WorldBase world)
	{
		return Type switch
		{
			BlockValueRefType.None => Vector3.zero, 
			BlockValueRefType.Block => BlockPosition.ToVector3CenterXZ(), 
			BlockValueRefType.Prop => (world.GetChunkSync(PropReference)?.GetWorldPos() ?? Vector3i.zero) + world.GetProp(PropReference).position + new Vector3(0.5f, 0f, 0.5f), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public Vector3i ToBlockPos(WorldBase world)
	{
		return World.worldToBlockPos(ToVector3Center(world));
	}

	public static BlockValueRef Create(WorldRayHitInfo hitInfo)
	{
		if (!hitInfo.hit.propValue.IsAir)
		{
			return new BlockValueRef(hitInfo.hit.propData.PropRef);
		}
		if (!hitInfo.hit.blockValue.isair)
		{
			return new BlockValueRef(hitInfo.hit.blockPos);
		}
		return None;
	}

	public static BlockValueRef Read(BinaryReader br)
	{
		return (BlockValueRefType)br.ReadByte() switch
		{
			BlockValueRefType.None => None, 
			BlockValueRefType.Block => new BlockValueRef(StreamUtils.ReadVector3i(br)), 
			BlockValueRefType.Prop => new BlockValueRef(PropRef.Read(br)), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public readonly void Write(BinaryWriter bw)
	{
		bw.Write((byte)Type);
		switch (Type)
		{
		case BlockValueRefType.Block:
			StreamUtils.Write(bw, BlockPosition);
			break;
		case BlockValueRefType.Prop:
			PropReference.Write(bw);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case BlockValueRefType.None:
			break;
		}
	}

	public override string ToString()
	{
		object arg = Type;
		return string.Format("Type={0}{1}", arg, Type switch
		{
			BlockValueRefType.None => "", 
			BlockValueRefType.Block => $", BlockPos=({BlockPosition})", 
			BlockValueRefType.Prop => $", PropRef=[{PropReference}]", 
			_ => throw new ArgumentOutOfRangeException(), 
		});
	}

	public static implicit operator Vector3i(BlockValueRef bvRef)
	{
		if (bvRef.TryGetBlockPos(out var pos))
		{
			return pos;
		}
		Log.Warning($"[PROPS] BlockValueRef implicitly converted to Vector3i but type was {bvRef.Type}.");
		return Vector3i.zero;
	}

	public static implicit operator PropRef(BlockValueRef bvRef)
	{
		if (bvRef.TryGetPropRef(out var propRef))
		{
			return propRef;
		}
		Log.Warning($"[PROPS] BlockValueRef implicitly converted to PropRef but type was {bvRef.Type}.");
		return PropRef.Default;
	}

	public static implicit operator BlockValueRef(Vector3i pos)
	{
		return new BlockValueRef(pos);
	}

	public static implicit operator BlockValueRef(PropRef propRef)
	{
		return new BlockValueRef(propRef);
	}

	public bool Equals(BlockValueRef other)
	{
		if (Type == other.Type && BlockPosition.Equals(other.BlockPosition))
		{
			return PropReference.Equals(other.PropReference);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is BlockValueRef other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine((int)Type, BlockPosition, PropReference);
	}

	public static bool operator ==(BlockValueRef left, BlockValueRef right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BlockValueRef left, BlockValueRef right)
	{
		return !left.Equals(right);
	}
}
