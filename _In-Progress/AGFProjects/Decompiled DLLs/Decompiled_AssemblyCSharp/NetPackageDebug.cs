using UnityEngine.Scripting;

[Preserve]
public class NetPackageDebug : NetPackage
{
	public enum Type
	{
		AILatency,
		AILatencyClientOff,
		AINameInfo,
		AINameInfoClientOff,
		AINameInfoServerToggle
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Type type;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] data;

	public override NetPackageDirection PackageDirection => NetPackageDirection.Both;

	public NetPackageDebug Setup(Type _type, int _entityId = -1, byte[] _data = null)
	{
		type = _type;
		entityId = _entityId;
		data = _data;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		type = (Type)_reader.ReadInt16();
		entityId = _reader.ReadInt32();
		int num = _reader.ReadInt32();
		if (num > 0)
		{
			data = _reader.ReadBytes(num);
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((short)type);
		_writer.Write(entityId);
		if (data == null || data.Length == 0)
		{
			_writer.Write(0);
			return;
		}
		_writer.Write(data.Length);
		_writer.Write(data);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		switch (type)
		{
		case Type.AILatency:
			AIDirector.DebugReceiveLatency(entityId, data);
			break;
		case Type.AILatencyClientOff:
			AIDirector.DebugLatencyOff();
			break;
		case Type.AINameInfo:
			AIDirector.DebugReceiveNameInfo(entityId, data);
			break;
		case Type.AINameInfoClientOff:
			EntityAlive.SetupAllDebugNameHUDs(_isAdd: false);
			break;
		case Type.AINameInfoServerToggle:
			AIDirector.DebugToggleSendNameInfo(base.Sender.entityId);
			break;
		}
	}

	public override int GetLength()
	{
		return 10 + ((data != null) ? data.Length : 0);
	}
}
