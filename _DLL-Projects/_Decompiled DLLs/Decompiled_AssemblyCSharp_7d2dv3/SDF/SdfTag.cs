using System.IO;

namespace SDF;

public abstract class SdfTag
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public SdfTagType TagType { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Name { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public object Value { get; set; }

	public abstract void WritePayload(BinaryWriter bw);

	[PublicizedFrom(EAccessModifier.Protected)]
	public SdfTag()
	{
	}
}
