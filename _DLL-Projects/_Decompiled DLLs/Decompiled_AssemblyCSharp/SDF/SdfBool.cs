using System.IO;

namespace SDF;

public class SdfBool : SdfTag
{
	public SdfBool(string _name, bool _value)
	{
		base.TagType = SdfTagType.Bool;
		base.Name = _name;
		base.Value = _value;
	}

	public override void WritePayload(BinaryWriter bw)
	{
		bw.Write((bool)base.Value);
	}
}
