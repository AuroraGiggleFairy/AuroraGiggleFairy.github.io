using System.IO;

namespace SDF;

public class SdfByteArray : SdfTag
{
	public SdfByteArray(string _name, byte[] _value)
	{
		base.TagType = SdfTagType.ByteArray;
		base.Name = _name;
		base.Value = _value;
	}

	public override void WritePayload(BinaryWriter bw)
	{
		bw.Write(((byte[])base.Value).Length);
		bw.Write((byte[])base.Value);
	}
}
