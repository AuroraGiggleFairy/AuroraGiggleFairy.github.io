using System;
using System.IO;
using UnityEngine;

public struct ActionTarget(ActionTargetType type, Vector3 position, BlockValueRef blockValueReference) : IEquatable<ActionTarget>
{
	public static readonly ActionTarget None = new ActionTarget(ActionTargetType.None, Vector3.zero, BlockValueRef.None);

	public ActionTargetType Type = type;

	public Vector3 Position = position;

	public BlockValueRef BlockValueReference = blockValueReference;

	public ActionTarget(float x, float y, float z)
		: this(new Vector3(x, y, z))
	{
	}

	public ActionTarget(Vector3 pos)
		: this(ActionTargetType.Position, pos, BlockValueRef.None)
	{
	}

	public ActionTarget(int x, int y, int z)
		: this(new BlockValueRef(x, y, z))
	{
	}

	public ActionTarget(Vector3i pos)
		: this(new BlockValueRef(pos))
	{
	}

	public ActionTarget(BlockValueRef bvRef)
		: this(ActionTargetType.BlockValueRef, Vector3.zero, bvRef)
	{
	}

	public static ActionTarget Read(BinaryReader br)
	{
		return (ActionTargetType)br.ReadByte() switch
		{
			ActionTargetType.None => None, 
			ActionTargetType.Position => new ActionTarget(StreamUtils.ReadVector3(br)), 
			ActionTargetType.BlockValueRef => new ActionTarget(BlockValueRef.Read(br)), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public readonly void Write(BinaryWriter bw)
	{
		bw.Write((byte)Type);
		switch (Type)
		{
		case ActionTargetType.Position:
			StreamUtils.Write(bw, Position);
			break;
		case ActionTargetType.BlockValueRef:
			BlockValueReference.Write(bw);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case ActionTargetType.None:
			break;
		}
	}

	public static implicit operator Vector3(ActionTarget target)
	{
		if (target.Type == ActionTargetType.Position)
		{
			return target.Position;
		}
		if (target.Type == ActionTargetType.BlockValueRef && target.BlockValueReference.TryGetBlockPos(out var pos))
		{
			return pos;
		}
		Log.Warning($"[PROPS] ActionTarget implicitly converted to Vector3, but failed: {target}");
		return Vector3.zero;
	}

	public static implicit operator Vector3i(ActionTarget target)
	{
		if (target.Type == ActionTargetType.BlockValueRef && target.BlockValueReference.TryGetBlockPos(out var pos))
		{
			return pos;
		}
		Log.Warning($"[PROPS] ActionTarget implicitly converted to Vector3i, but failed: {target}");
		return Vector3i.zero;
	}

	public static implicit operator BlockValueRef(ActionTarget target)
	{
		if (target.Type == ActionTargetType.BlockValueRef)
		{
			return target.BlockValueReference;
		}
		Log.Warning($"[PROPS] ActionTarget implicitly converted to BlockValueRef, but failed: {target}");
		return BlockValueRef.None;
	}

	public static implicit operator ActionTarget(Vector3 pos)
	{
		return new ActionTarget(pos);
	}

	public static implicit operator ActionTarget(Vector3i pos)
	{
		return new ActionTarget(new BlockValueRef(pos));
	}

	public static implicit operator ActionTarget(PropRef propRef)
	{
		return new ActionTarget(new BlockValueRef(propRef));
	}

	public static implicit operator ActionTarget(BlockValueRef bvRef)
	{
		return new ActionTarget(bvRef);
	}

	public bool Equals(ActionTarget other)
	{
		if (Type == other.Type && Position.Equals(other.Position))
		{
			return BlockValueReference.Equals(other.BlockValueReference);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is ActionTarget other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine((int)Type, Position, BlockValueReference);
	}

	public static bool operator ==(ActionTarget left, ActionTarget right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ActionTarget left, ActionTarget right)
	{
		return !left.Equals(right);
	}
}
