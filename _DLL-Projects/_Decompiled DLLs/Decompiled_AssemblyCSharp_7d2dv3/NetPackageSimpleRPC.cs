using UnityEngine.Scripting;

[Preserve]
public class NetPackageSimpleRPC : NetPackage
{
	public int entityId;

	public SimpleRPCType type;

	public NetPackageSimpleRPC Setup(int _entityId, SimpleRPCType _type)
	{
		entityId = _entityId;
		type = _type;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		type = (SimpleRPCType)_reader.ReadByte();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		_writer.Write((byte)type);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (ValidEntityIdForSender(entityId))
		{
			_callbacks.SimpleRPC(entityId, type, _bExeLocal: true, _world.IsRemote());
		}
	}

	public override int GetLength()
	{
		return 10;
	}
}
