using System.IO;
using Noemax.GZip;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageLocalization : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] data;

	public override bool Compress => true;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageLocalization Setup(byte[] _data)
	{
		data = _data;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		int count = _reader.ReadInt32();
		data = _reader.ReadBytes(count);
		using MemoryStream input = new MemoryStream(data);
		using DeflateInputStream source = new DeflateInputStream(input);
		using MemoryStream memoryStream = new MemoryStream();
		StreamUtils.StreamCopy(source, memoryStream);
		data = memoryStream.ToArray();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(data.Length);
		_writer.Write(data);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		Localization.LoadServerPatchDictionary(data);
	}

	public override int GetLength()
	{
		return data.Length;
	}
}
