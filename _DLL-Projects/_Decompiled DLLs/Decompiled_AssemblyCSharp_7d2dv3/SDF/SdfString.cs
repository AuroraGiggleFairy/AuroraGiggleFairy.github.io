using System.IO;

namespace SDF;

public class SdfString : SdfTag
{
	public SdfString(string _name, string _value)
	{
		base.TagType = SdfTagType.String;
		base.Name = _name;
		base.Value = _value;
	}

	public override void WritePayload(BinaryWriter bw)
	{
		if (base.Value == null)
		{
			Log.Error("Null value: " + base.Name);
		}
		bw.Write((short)Utils.ToBase64(base.Value.ToString()).Length);
		bw.Write(Utils.ToBase64(base.Value.ToString()));
	}
}
