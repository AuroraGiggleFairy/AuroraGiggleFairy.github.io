using System.IO;

namespace SDF;

public class SdfFloat : SdfTag
{
	public SdfFloat(string _name, float _value)
	{
		base.TagType = SdfTagType.Float;
		base.Name = _name;
		base.Value = _value;
	}

	public override void WritePayload(BinaryWriter bw)
	{
		bw.Write((float)base.Value);
	}
}
