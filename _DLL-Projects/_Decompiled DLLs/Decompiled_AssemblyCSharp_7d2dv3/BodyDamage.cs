using System.IO;
using System.Runtime.CompilerServices;

public struct BodyDamage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static int cBinaryVersion = 4;

	public int StunKnee;

	public int StunProne;

	public float StunDuration;

	public EnumEntityStunType CurrentStun;

	public bool ShouldBeCrawler;

	public const uint cNoHead = 1u;

	public const uint cNoArmLUpper = 2u;

	public const uint cNoArmLLower = 4u;

	public const uint cNoArmRUpper = 8u;

	public const uint cNoArmRLower = 16u;

	public const uint cNoArm = 30u;

	public const uint cNoLegLUpper = 32u;

	public const uint cNoLegLLower = 64u;

	public const uint cNoLegRUpper = 128u;

	public const uint cNoLegRLower = 256u;

	public const uint cNoLeg = 480u;

	public const uint cCrippledLegL = 4096u;

	public const uint cCrippledLegR = 8192u;

	public uint Flags;

	public EnumDamageTypes damageType;

	public EnumBodyPartHit bodyPartHit;

	public bool HasLeftLeg
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (Flags & 0x60) == 0;
		}
	}

	public bool HasRightLeg
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (Flags & 0x180) == 0;
		}
	}

	public bool HasLimbs
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (Flags & 0x14A) != 330;
		}
	}

	public bool IsAnyLegMissing
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (Flags & 0x1E0) != 0;
		}
	}

	public bool IsAnyArmOrLegMissing
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (Flags & 0x1FE) != 0;
		}
	}

	public bool IsCrippled => (Flags & 0x3000) != 0;

	public static BodyDamage Read(BinaryReader _br, int _version)
	{
		if (_version > 21)
		{
			return ReadData(_br, _br.ReadInt32());
		}
		if (_version > 20)
		{
			return ReadData(_br, 0);
		}
		if (_version > 19)
		{
			_br.ReadInt32();
		}
		return default(BodyDamage);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BodyDamage ReadData(BinaryReader br, int version)
	{
		BodyDamage result = default(BodyDamage);
		if (version >= 4)
		{
			result.damageType = (EnumDamageTypes)br.ReadInt32();
		}
		if (version >= 3)
		{
			result.Flags = br.ReadUInt32();
		}
		else
		{
			br.ReadInt16();
			br.ReadInt16();
			br.ReadInt16();
			br.ReadInt16();
			br.ReadInt16();
			br.ReadInt16();
			if (br.ReadBoolean())
			{
				result.Flags |= 2u;
			}
			if (br.ReadBoolean())
			{
				result.Flags |= 8u;
			}
			if (br.ReadBoolean())
			{
				result.Flags |= 1u;
			}
			if (br.ReadBoolean())
			{
				result.Flags |= 128u;
			}
			if (br.ReadBoolean())
			{
				result.Flags |= 8192u;
			}
			if (version >= 1)
			{
				br.ReadInt16();
				br.ReadInt16();
				br.ReadInt16();
				br.ReadInt16();
				if (br.ReadBoolean())
				{
					result.Flags |= 4u;
				}
				if (br.ReadBoolean())
				{
					result.Flags |= 16u;
				}
				if (br.ReadBoolean())
				{
					result.Flags |= 64u;
				}
				if (br.ReadBoolean())
				{
					result.Flags |= 256u;
				}
				if (version >= 2 && br.ReadBoolean())
				{
					result.Flags |= 32u;
				}
				if (br.ReadBoolean())
				{
					result.Flags |= 4096u;
				}
			}
		}
		result.ShouldBeCrawler = !result.HasLeftLeg || !result.HasRightLeg;
		return result;
	}

	public void Write(BinaryWriter bw)
	{
		bw.Write(cBinaryVersion);
		bw.Write((int)damageType);
		bw.Write(Flags);
	}
}
