using System.Collections;
using System.Collections.Generic;

namespace SharpEXR;

public class OffsetTable : IEnumerable<uint>, IEnumerable
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public List<uint> Offsets { get; set; }

	public OffsetTable()
	{
		Offsets = new List<uint>();
	}

	public OffsetTable(int capacity)
	{
		Offsets = new List<uint>(capacity);
	}

	public void Read(IEXRReader reader, int count)
	{
		for (int i = 0; i < count; i++)
		{
			Offsets.Add(reader.ReadUInt32());
			reader.ReadUInt32();
		}
	}

	public IEnumerator<uint> GetEnumerator()
	{
		return Offsets.GetEnumerator();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
