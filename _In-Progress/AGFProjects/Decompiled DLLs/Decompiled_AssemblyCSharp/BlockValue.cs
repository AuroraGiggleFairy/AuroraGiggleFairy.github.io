using System;
using System.Runtime.CompilerServices;

[Serializable]
public struct BlockValue : IEquatable<BlockValue>
{
	public const uint TypeMask = 65535u;

	public const uint RotationMax = 31u;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const uint RotationMask = 2031616u;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int RotationShift = 16;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const uint Metadata3Max = 1u;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const uint Metadata3Mask = 2097152u;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int Metadata3Shift = 21;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const uint RotationMeta3Max = 63u;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const uint RotationMeta3Mask = 4128768u;

	public const uint MetadataMax = 15u;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const uint Metadata1Mask = 62914560u;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int Metadata1Shift = 22;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const uint Metadata2Mask = 1006632960u;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int Metadata2Shift = 26;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const uint Metadata12Max = 255u;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const uint ChildMask = 1073741824u;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int ChildShift = 30;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const uint HasDecalMask = 2147483648u;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int HasDecalShift = 31;

	public static BlockValue Air;

	public uint rawData;

	public int damage;

	public Block Block
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Block.list[type];
		}
	}

	public bool isair
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return type == 0;
		}
	}

	public bool isWater
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (type != 240 && type != 241)
			{
				return type == 242;
			}
			return true;
		}
	}

	public int type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (int)(rawData & 0xFFFF);
		}
		set
		{
			rawData = (rawData & 0xFFFF0000u) | (uint)(value & 0xFFFF);
		}
	}

	public byte rotation
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((rawData >> 16) & 0x1F);
		}
		set
		{
			rawData = (rawData & 0xFFE0FFFFu) | (uint)((value & 0x1F) << 16);
		}
	}

	public byte meta
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((rawData >> 22) & 0xF);
		}
		set
		{
			rawData = (rawData & 0xFC3FFFFFu) | (uint)((value & 0xF) << 22);
		}
	}

	public byte meta2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((rawData >> 26) & 0xF);
		}
		set
		{
			rawData = (rawData & 0xC3FFFFFFu) | (uint)((value & 0xF) << 26);
		}
	}

	public byte meta2and1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((rawData >> 22) & 0xFF);
		}
		set
		{
			rawData = (rawData & 0xC03FFFFFu) | (uint)(value << 22);
		}
	}

	public byte meta3
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return (byte)((rawData >> 21) & 1);
		}
	}

	public byte rotationAndMeta3
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return (byte)((rawData >> 16) & 0x3F);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			rawData = (rawData & 0xFFC0FFFFu) | (uint)((value & 0x3F) << 16);
		}
	}

	public bool hasdecal
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (rawData & 0x80000000u) != 0;
		}
		set
		{
			rawData = (rawData & 0x7FFFFFFF) | (uint)(value ? int.MinValue : 0);
		}
	}

	public BlockFaceFlag rotatedWaterFlowMask
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return BlockFaceFlags.RotateFlags(Block.WaterFlowMask, rotation);
		}
	}

	public BlockFace decalface
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (BlockFace)((rawData >> 22) & 0xF);
		}
		set
		{
			rawData = (rawData & 0xFC3FFFFFu) | ((uint)(value & (BlockFace)15) << 22);
		}
	}

	public byte decaltex
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((rawData >> 26) & 0xF);
		}
		set
		{
			rawData = (rawData & 0xC3FFFFFFu) | (uint)((value & 0xF) << 26);
		}
	}

	public bool ischild
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (rawData & 0x40000000) != 0;
		}
		set
		{
			rawData = (rawData & 0xBFFFFFFFu) | (uint)(value ? 1073741824 : 0);
		}
	}

	public int parentx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return meta - 8;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			meta = (byte)(value + 8);
		}
	}

	public int parenty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return rotationAndMeta3 - 32;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			rotationAndMeta3 = (byte)(value + 32);
		}
	}

	public int parentz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return meta2 - 8;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			meta2 = (byte)(value + 8);
		}
	}

	public Vector3i parent
	{
		get
		{
			return new Vector3i(parentx, parenty, parentz);
		}
		set
		{
			parentx = value.x;
			parenty = value.y;
			parentz = value.z;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BlockValue(uint _rawData)
	{
		rawData = _rawData;
		damage = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BlockValue(uint _rawData, int _damage)
	{
		rawData = _rawData;
		damage = _damage;
	}

	public BlockValue set(int _type, byte _meta, byte _damage, byte _rotation)
	{
		type = _type;
		meta = _meta;
		damage = _damage;
		rotation = _rotation;
		return this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint GetTypeMasked(uint _v)
	{
		return _v & 0xFFFF;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetForceToOtherBlock(BlockValue _other)
	{
		return Utils.FastMin(Block.blockMaterial.StabilityGlue, _other.Block.blockMaterial.StabilityGlue);
	}

	public int ToItemType()
	{
		return type;
	}

	public ItemValue ToItemValue()
	{
		return new ItemValue
		{
			type = type
		};
	}

	public override int GetHashCode()
	{
		return type;
	}

	public override bool Equals(object _other)
	{
		if (!(_other is BlockValue blockValue))
		{
			return false;
		}
		return blockValue.type == type;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(BlockValue _other)
	{
		return _other.type == type;
	}

	public bool EqualsExceptRotation(BlockValue _other)
	{
		int num = (int)rawData & -2031617;
		uint num2 = _other.rawData & 0xFFE0FFFFu;
		return num == (int)num2;
	}

	public override string ToString()
	{
		if (!ischild)
		{
			return string.Format("id={0} r={1} d={2} m={3} m2={4} m3={5} name={6}", type, rotation, damage, meta, meta2, meta3, Block?.GetBlockName() ?? "-null-");
		}
		return string.Format("id={0} px={1} py={2} pz={3} name={4}", type, parentx, parenty, parentz, Block?.GetBlockName() ?? "-null-");
	}
}
