using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityRemove : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte reason;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageEntityRemove Setup(int _entityId, EnumRemoveEntityReason _reason)
	{
		entityId = _entityId;
		reason = (byte)_reason;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		reason = _reader.ReadByte();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		_writer.Write(reason);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_world?.RemoveEntity(entityId, (EnumRemoveEntityReason)reason);
	}

	public override int GetLength()
	{
		return 4;
	}
}
