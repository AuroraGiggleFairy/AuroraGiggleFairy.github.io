using System.IO;
using System.Runtime.CompilerServices;

public struct WaterValue
{
	public const float cTopPerCap = 0.6f;

	public const int MAX_MASS_VALUE = 65535;

	public static readonly WaterValue Empty;

	public static readonly WaterValue Full;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort mass;

	public long RawData => mass;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool HasMass()
	{
		return mass > 195;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetMass()
	{
		return mass;
	}

	public float GetMassPercent()
	{
		if (mass <= 195)
		{
			return 0f;
		}
		if (mass >= 15600)
		{
			return 1f;
		}
		return (float)(mass - 195) / 15405f;
	}

	public void SetMass(int value)
	{
		mass = (ushort)Utils.FastClamp(value, 0, 65535);
	}

	public override string ToString()
	{
		return $"Raw Mass: {mass:d}";
	}

	public static WaterValue FromRawData(long rawData)
	{
		return new WaterValue
		{
			mass = (ushort)rawData
		};
	}

	public static WaterValue FromBlockType(int type)
	{
		if (type == 240 || type == 241 || type == 242)
		{
			return new WaterValue(19500);
		}
		return Empty;
	}

	public static WaterValue FromStream(BinaryReader _reader)
	{
		WaterValue result = default(WaterValue);
		result.Read(_reader);
		return result;
	}

	public WaterValue(BlockValue _bv)
	{
		mass = (ushort)(_bv.isWater ? 19500u : 0u);
	}

	public WaterValue(int mass)
	{
		this.mass = (ushort)Utils.FastClamp(mass, 0, 65535);
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(mass);
	}

	public void Read(BinaryReader reader)
	{
		mass = reader.ReadUInt16();
	}

	public static int SerializedLength()
	{
		return 2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static WaterValue()
	{
		Empty = new WaterValue(0);
		Full = new WaterValue(19500);
	}
}
