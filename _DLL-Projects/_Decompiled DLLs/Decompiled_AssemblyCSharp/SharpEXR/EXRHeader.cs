using System.Collections.Generic;
using SharpEXR.AttributeTypes;

namespace SharpEXR;

public class EXRHeader
{
	public static readonly Chromaticities DefaultChromaticities = new Chromaticities(0.64f, 0.33f, 0.3f, 0.6f, 0.15f, 0.06f, 0.3127f, 0.329f);

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, EXRAttribute> Attributes
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public bool IsEmpty => Attributes.Count == 0;

	public int ChunkCount
	{
		get
		{
			if (!TryGetAttribute<int>("chunkCount", out var result))
			{
				throw new EXRFormatException("Invalid or corrupt EXR header: Missing chunkCount attribute.");
			}
			return result;
		}
	}

	public Box2I DataWindow
	{
		get
		{
			if (!TryGetAttribute<Box2I>("dataWindow", out var result))
			{
				throw new EXRFormatException("Invalid or corrupt EXR header: Missing dataWindow attribute.");
			}
			return result;
		}
	}

	public EXRCompression Compression
	{
		get
		{
			if (!TryGetAttribute<EXRCompression>("compression", out var result))
			{
				throw new EXRFormatException("Invalid or corrupt EXR header: Missing compression attribute.");
			}
			return result;
		}
	}

	public PartType Type
	{
		get
		{
			if (!TryGetAttribute<PartType>("type", out var result))
			{
				throw new EXRFormatException("Invalid or corrupt EXR header: Missing type attribute.");
			}
			return result;
		}
	}

	public ChannelList Channels
	{
		get
		{
			if (!TryGetAttribute<ChannelList>("channels", out var result))
			{
				throw new EXRFormatException("Invalid or corrupt EXR header: Missing channels attribute.");
			}
			return result;
		}
	}

	public Chromaticities Chromaticities
	{
		get
		{
			foreach (EXRAttribute value in Attributes.Values)
			{
				if (value.Type == "chromaticities" && value.Value is Chromaticities)
				{
					return (Chromaticities)value.Value;
				}
			}
			return DefaultChromaticities;
		}
	}

	public EXRHeader()
	{
		Attributes = new Dictionary<string, EXRAttribute>();
	}

	public void Read(EXRFile file, IEXRReader reader)
	{
		EXRAttribute attribute;
		while (EXRAttribute.Read(file, reader, out attribute))
		{
			Attributes[attribute.Name] = attribute;
		}
	}

	public bool TryGetAttribute<T>(string name, out T result)
	{
		if (!Attributes.TryGetValue(name, out var value))
		{
			result = default(T);
			return false;
		}
		if (value.Value == null)
		{
			result = default(T);
			if (!typeof(T).IsClass && !typeof(T).IsInterface)
			{
				return !typeof(T).IsArray;
			}
			return false;
		}
		if (typeof(T).IsAssignableFrom(value.Value.GetType()))
		{
			result = (T)value.Value;
			return true;
		}
		result = default(T);
		return false;
	}
}
