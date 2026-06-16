using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityRemove : NetPackageEntityTargeted
{
	[PublicizedFrom(EAccessModifier.Private)]
	public byte reason;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageEntityRemove Setup(int _entityId, EnumRemoveEntityReason _reason)
	{
		Setup(_entityId);
		reason = (byte)_reason;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		base.read(_reader);
		reason = _reader.ReadByte();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(reason);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			if (_world.GetEntity(entityId) == null)
			{
				Log.Error($"NetPackageEntityRemove entity {entityId} missing");
			}
			_world.RemoveEntity(entityId, (EnumRemoveEntityReason)reason);
		}
	}

	public override int GetLength()
	{
		return 4;
	}
}
