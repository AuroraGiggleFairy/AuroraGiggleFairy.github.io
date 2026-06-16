using UnityEngine.Scripting;

[Preserve]
public class NetPackageConfigFile : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] data;

	public override bool Compress => true;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageConfigFile Setup(string _name, byte[] _data)
	{
		name = _name;
		data = _data;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		name = _reader.ReadString();
		int num = _reader.ReadInt32();
		data = ((num >= 0) ? _reader.ReadBytes(num) : null);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(name);
		if (data != null)
		{
			_writer.Write(data.Length);
			_writer.Write(data);
		}
		else
		{
			_writer.Write(-1);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		WorldStaticData.ReceivedConfigFile(name, data);
	}

	public override int GetLength()
	{
		int num = name.Length * 2;
		byte[] array = data;
		return num + ((array != null) ? array.Length : 0);
	}
}
