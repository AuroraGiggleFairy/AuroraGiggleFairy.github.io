using System.IO;

namespace SDF;

public class SdfInt : SdfTag
{
	public SdfInt(string _name, int _value)
	{
		base.TagType = SdfTagType.Int;
		base.Name = _name;
		base.Value = _value;
	}

	public override void WritePayload(BinaryWriter bw)
	{
		bw.Write((int)base.Value);
	}
}
