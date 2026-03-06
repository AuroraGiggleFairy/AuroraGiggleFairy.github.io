namespace SharpEXR;

public class Channel
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Name { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PixelType Type { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool Linear { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int XSampling { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int YSampling { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte[] Reserved { get; set; }

	public Channel(string name, PixelType type, bool linear, int xSampling, int ySampling)
		: this(name, type, linear, 0, 0, 0, xSampling, ySampling)
	{
	}

	public Channel(string name, PixelType type, bool linear, byte reserved0, byte reserved1, byte reserved2, int xSampling, int ySampling)
	{
		Name = name;
		Type = type;
		Linear = linear;
		Reserved = new byte[3] { reserved0, reserved1, reserved2 };
	}

	public override string ToString()
	{
		return $"{GetType().Name} {Name} {Type}";
	}
}
