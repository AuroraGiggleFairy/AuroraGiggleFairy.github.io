using System.Collections.Generic;
using System.IO;

namespace SDF;

public static class SdfWriter
{
	public static void Write(Stream fs, Dictionary<string, SdfTag> sdfTags)
	{
		using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
		pooledBinaryWriter.SetBaseStream(fs);
		pooledBinaryWriter.Seek(0, SeekOrigin.Begin);
		foreach (KeyValuePair<string, SdfTag> sdfTag in sdfTags)
		{
			SdfTag value = sdfTag.Value;
			pooledBinaryWriter.Write((byte)value.TagType);
			pooledBinaryWriter.Write((short)value.Name.Length);
			pooledBinaryWriter.Write(value.Name);
			value.WritePayload(pooledBinaryWriter);
		}
	}
}
