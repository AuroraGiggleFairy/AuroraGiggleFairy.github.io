using System.IO;

namespace SDF;

public class SdfBinary : SdfTag
{
	public SdfBinary(string _name, string _value)
	{
		base.TagType = SdfTagType.Binary;
		base.Name = _name;
		base.Value = _value;
	}

	public override void WritePayload(BinaryWriter bw)
	{
		bw.Write((short)Utils.ToBase64(base.Value.ToString()).Length);
		bw.Write(Utils.ToBase64((string)base.Value));
	}
}
