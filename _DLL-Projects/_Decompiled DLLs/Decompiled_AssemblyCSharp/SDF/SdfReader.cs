using System.Collections.Generic;
using System.IO;

namespace SDF;

public static class SdfReader
{
	public static Dictionary<string, SdfTag> Read(Stream fs)
	{
		Dictionary<string, SdfTag> dictionary = new Dictionary<string, SdfTag>();
		using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false))
		{
			pooledBinaryReader.SetBaseStream(fs);
			try
			{
				while (pooledBinaryReader.BaseStream.Position < pooledBinaryReader.BaseStream.Length)
				{
					SdfTagType sdfTagType = (SdfTagType)pooledBinaryReader.ReadByte();
					pooledBinaryReader.ReadInt16();
					string text = pooledBinaryReader.ReadString();
					switch (sdfTagType)
					{
					case SdfTagType.Float:
					{
						SdfFloat value6 = new SdfFloat(text, pooledBinaryReader.ReadSingle());
						dictionary.Add(text, value6);
						break;
					}
					case SdfTagType.Int:
					{
						SdfInt value5 = new SdfInt(text, pooledBinaryReader.ReadInt32());
						dictionary.Add(text, value5);
						break;
					}
					case SdfTagType.String:
					{
						pooledBinaryReader.ReadInt16();
						SdfString value4 = new SdfString(text, Utils.FromBase64(pooledBinaryReader.ReadString()));
						dictionary.Add(text, value4);
						break;
					}
					case SdfTagType.Binary:
					{
						pooledBinaryReader.ReadInt16();
						SdfString value3 = new SdfString(text, pooledBinaryReader.ReadString());
						dictionary.Add(text, value3);
						break;
					}
					case SdfTagType.Bool:
					{
						SdfBool value2 = new SdfBool(text, pooledBinaryReader.ReadBoolean());
						dictionary.Add(text, value2);
						break;
					}
					case SdfTagType.ByteArray:
					{
						short count = pooledBinaryReader.ReadInt16();
						SdfByteArray value = new SdfByteArray(text, pooledBinaryReader.ReadBytes(count));
						dictionary.Add(text, value);
						break;
					}
					}
				}
			}
			catch
			{
			}
		}
		return dictionary;
	}
}
