using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWorldAreas : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cVersion = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TraderArea> traders;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageWorldAreas Setup(List<TraderArea> _list)
	{
		traders = _list;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		_reader.ReadByte();
		int num = _reader.ReadInt16();
		traders = new List<TraderArea>();
		for (int i = 0; i < num; i++)
		{
			TraderArea item = TraderArea.Read(_reader);
			traders.Add(item);
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((byte)1);
		_writer.Write((short)traders.Count);
		for (int i = 0; i < traders.Count; i++)
		{
			traders[i].Write(_writer);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		World.SetWorldAreas(traders);
	}

	public override int GetLength()
	{
		int num = 2;
		for (int i = 0; i < traders.Count; i++)
		{
			num += traders[i].GetReadWriteSize();
		}
		return num;
	}
}
