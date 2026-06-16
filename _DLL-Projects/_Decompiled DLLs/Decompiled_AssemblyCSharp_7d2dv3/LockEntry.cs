using System;

public readonly struct LockEntry(ILockTarget target, ushort channel = 0) : IEquatable<LockEntry>
{
	public readonly ILockTarget Target = target;

	public readonly ushort Channel = channel;

	public bool Equals(LockEntry other)
	{
		if (Target == other.Target)
		{
			return Channel == other.Channel;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Target?.GetHashCode() ?? 0, Channel);
	}

	public override bool Equals(object obj)
	{
		if (obj is LockEntry other)
		{
			return Equals(other);
		}
		return false;
	}

	public static bool operator ==(LockEntry left, LockEntry right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(LockEntry left, LockEntry right)
	{
		return !left.Equals(right);
	}
}
