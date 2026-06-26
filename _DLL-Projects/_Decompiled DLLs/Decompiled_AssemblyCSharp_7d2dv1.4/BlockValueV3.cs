using System.Runtime.CompilerServices;

public struct BlockValueV3
{
	public const uint TypeMask = 32767u;

	public const uint RotationMax = 31u;

	[PublicizedFrom(EAccessModifier.Private)]
	public const uint RotationMask = 1015808u;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int RotationShift = 15;

	public const uint MetadataMax = 15u;

	[PublicizedFrom(EAccessModifier.Private)]
	public const uint MetadataMask = 15728640u;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MetadataShift = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public const uint Metadata2Mask = 251658240u;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Metadata2Shift = 24;

	[PublicizedFrom(EAccessModifier.Private)]
	public const uint Metadata3Max = 3u;

	[PublicizedFrom(EAccessModifier.Private)]
	public const uint Metadata3Mask = 805306368u;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Metadata3Shift = 28;

	[PublicizedFrom(EAccessModifier.Private)]
	public const uint ChildMask = 1073741824u;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ChildShift = 30;

	[PublicizedFrom(EAccessModifier.Private)]
	public const uint HasDecalMask = 2147483648u;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int HasDecalShift = 31;

	public uint rawData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static BlockValueV3 convertBV3;

	[PublicizedFrom(EAccessModifier.Private)]
	public static BlockValue convertBV;

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
			return (int)(rawData & 0x7FFF);
		}
		set
		{
			rawData = (rawData & 0xFFFF8000u) | (uint)(int)((long)value & 0x7FFFL);
		}
	}

	public byte rotation
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((rawData & 0xF8000) >> 15);
		}
		set
		{
			rawData = (rawData & 0xFFF07FFFu) | (uint)((value & 0x1F) << 15);
		}
	}

	public byte meta
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((rawData & 0xF00000) >> 20);
		}
		set
		{
			rawData = (rawData & 0xFF0FFFFFu) | (uint)((value & 0xF) << 20);
		}
	}

	public byte meta2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((rawData & 0xF000000) >> 24);
		}
		set
		{
			rawData = (rawData & 0xF0FFFFFFu) | (uint)((value & 0xF) << 24);
		}
	}

	public byte meta3
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return (byte)((rawData & 0x30000000) >> 28);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			rawData = (rawData & 0xCFFFFFFFu) | (uint)((value & 3) << 28);
		}
	}

	public byte meta2and1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((meta2 << 4) | meta);
		}
		set
		{
			meta2 = (byte)((value >> 4) & 0xF);
			meta = (byte)(value & 0xF);
		}
	}

	public byte rotationAndMeta3
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return (byte)((rotation << 2) | meta3);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			rotation = (byte)((ulong)(value >> 2) & 0x1FuL);
			meta3 = value;
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
			return (BlockFace)((rawData & 0xF00000) >> 20);
		}
		set
		{
			rawData = (rawData & 0xFF0FFFFFu) | ((uint)value << 20);
		}
	}

	public byte decaltex
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((rawData & 0xF000000) >> 24);
		}
		set
		{
			rawData = (rawData & 0xF0FFFFFFu) | (uint)(value << 24);
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
		get
		{
			int num = (int)((rawData & 0xF000000) >> 24);
			return ((num & 8) != 0) ? (-(num & 7)) : (num & 7);
		}
		set
		{
			int num = ((value < 0) ? (8 | (-value & 7)) : (value & 7));
			rawData = (rawData & 0xF0FFFFFFu) | (uint)(num << 24);
		}
	}

	public int parenty
	{
		get
		{
			int num = rotationAndMeta3;
			return ((num & 0x20) != 0) ? (-(num & 0x1F)) : (num & 0x1F);
		}
		set
		{
			int num = ((value < 0) ? (0x20 | (-value & 0x1F)) : (value & 0x1F));
			rotationAndMeta3 = (byte)num;
		}
	}

	public int parentz
	{
		get
		{
			int num = (int)((rawData & 0xF00000) >> 20);
			return ((num & 8) != 0) ? (-(num & 7)) : (num & 7);
		}
		set
		{
			int num = ((value < 0) ? (8 | (-value & 7)) : (value & 7));
			rawData = (rawData & 0xFF0FFFFFu) | (uint)(num << 20);
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

	public static uint ConvertOldRawData(uint _rawData)
	{
		convertBV3.rawData = _rawData;
		int num = convertBV3.type;
		convertBV.type = num;
		if (!convertBV3.ischild)
		{
			convertBV.rotation = convertBV3.rotation;
			convertBV.meta = convertBV3.meta;
			convertBV.meta2 = convertBV3.meta2;
		}
		else
		{
			convertBV.parent = convertBV3.parent;
			convertBV.ischild = true;
		}
		convertBV.hasdecal = convertBV3.hasdecal;
		return convertBV.rawData;
	}

	public BlockValueV3(uint _rawData)
	{
		rawData = _rawData;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint GetTypeMasked(uint _v)
	{
		return _v & 0x7FFF;
	}
}
