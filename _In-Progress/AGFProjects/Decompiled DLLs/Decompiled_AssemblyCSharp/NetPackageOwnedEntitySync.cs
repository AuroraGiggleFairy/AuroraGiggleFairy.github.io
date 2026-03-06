using UnityEngine.Scripting;

[Preserve]
public class NetPackageOwnedEntitySync : NetPackage
{
	public enum SyncType : byte
	{
		Remove,
		Add
	}

	public int ownerId;

	public int entityId;

	public SyncType syncType;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageOwnedEntitySync Setup(int _ownerId, int _entityId, SyncType _syncType)
	{
		ownerId = _ownerId;
		entityId = _entityId;
		syncType = _syncType;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		ownerId = _reader.ReadInt32();
		entityId = _reader.ReadInt32();
		syncType = (SyncType)_reader.ReadByte();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(ownerId);
		_writer.Write(entityId);
		_writer.Write((byte)syncType);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			EntityAlive entityAlive = GameManager.Instance.World.GetEntity(ownerId) as EntityAlive;
			if ((bool)entityAlive)
			{
				entityAlive.RemoveOwnedEntity(entityId);
			}
		}
	}

	public override int GetLength()
	{
		return 9;
	}
}
