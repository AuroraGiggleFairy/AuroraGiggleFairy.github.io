using UnityEngine.Scripting;

[Preserve]
public class NetPackageIdMapping : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] data;

	public override bool Compress => true;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageIdMapping Setup(string _name, byte[] _data)
	{
		name = _name;
		data = _data;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		name = _reader.ReadString();
		int count = _reader.ReadInt32();
		data = _reader.ReadBytes(count);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(name);
		_writer.Write(data.Length);
		_writer.Write(data);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		GameManager.Instance.IdMappingReceived(name, data);
	}

	public override int GetLength()
	{
		return name.Length * 2 + data.Length;
	}
}
