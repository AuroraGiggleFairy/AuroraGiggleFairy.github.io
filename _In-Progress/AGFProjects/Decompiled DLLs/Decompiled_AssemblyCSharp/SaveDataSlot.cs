using System;

public readonly struct SaveDataSlot : IEquatable<SaveDataSlot>, IComparable<SaveDataSlot>, IComparable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string DUMMY_POSTFIX_WITHOUT_SLASH = "d";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string DUMMY_POSTFIX_WITH_SLASH = "/d";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SaveDataManagedPath m_internalPath;

	public SaveDataType Type
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return m_internalPath.Type;
		}
	}

	public StringSpan SlotPath
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return m_internalPath.SlotPath;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveDataSlot(SaveDataType type, StringSpan slotPath)
		: this(new SaveDataManagedPath((slotPath.Length > 0) ? SpanUtils.Concat(type.GetPathRaw(), "/", slotPath, "/d") : SpanUtils.Concat(type.GetPathRaw(), "/d")))
	{
		if (Type != type)
		{
			throw new ArgumentException($"Got type {Type} but expected {type}. Make sure that concatenating the slot path does not match another type.", "type");
		}
		if (SlotPath != slotPath)
		{
			throw new ArgumentException(SpanUtils.Concat("Expected slot path to be '", slotPath, "', but was: ", SlotPath), "slotPath");
		}
	}

	public SaveDataSlot(SaveDataManagedPath managedPath)
	{
		m_internalPath = managedPath;
	}

	public SaveDataSlot GetSimpleSlot()
	{
		if (!(m_internalPath.PathRelativeToSlot == "d"))
		{
			return new SaveDataSlot(Type, SlotPath);
		}
		return this;
	}

	public override string ToString()
	{
		if (SlotPath.Length != 0)
		{
			return SpanUtils.Concat(Type.ToStringCached(), "[", SlotPath, "]");
		}
		return Type.ToStringCached();
	}

	public bool Equals(SaveDataSlot other)
	{
		if (Type == other.Type)
		{
			return SlotPath == other.SlotPath;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is SaveDataSlot other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((int)Type * 397) ^ SlotPath.GetHashCode();
	}

	public static bool operator ==(SaveDataSlot left, SaveDataSlot right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SaveDataSlot left, SaveDataSlot right)
	{
		return !left.Equals(right);
	}

	public int CompareTo(SaveDataSlot other)
	{
		int num = Type.CompareTo(other.Type);
		if (num != 0)
		{
			return num;
		}
		return SlotPath.CompareTo(other.SlotPath);
	}

	public int CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		if (!(obj is SaveDataSlot other))
		{
			throw new ArgumentException("Object must be of type SaveDataSlot");
		}
		return CompareTo(other);
	}

	public static bool operator <(SaveDataSlot left, SaveDataSlot right)
	{
		return left.CompareTo(right) < 0;
	}

	public static bool operator >(SaveDataSlot left, SaveDataSlot right)
	{
		return left.CompareTo(right) > 0;
	}

	public static bool operator <=(SaveDataSlot left, SaveDataSlot right)
	{
		return left.CompareTo(right) <= 0;
	}

	public static bool operator >=(SaveDataSlot left, SaveDataSlot right)
	{
		return left.CompareTo(right) >= 0;
	}
}
