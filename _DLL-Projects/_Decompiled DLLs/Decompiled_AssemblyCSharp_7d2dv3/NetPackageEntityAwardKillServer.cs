using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityAwardKillServer : NetPackage
{
	public int EntityId;

	public int KilledEntityId;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageEntityAwardKillServer Setup(int _killerEntityId, int _killedEntityId)
	{
		EntityId = _killerEntityId;
		KilledEntityId = _killedEntityId;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		EntityId = _reader.ReadInt32();
		KilledEntityId = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(EntityId);
		_writer.Write(KilledEntityId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && _world.GetEntity(EntityId) is EntityPlayerLocal killedBy && _world.GetEntity(KilledEntityId) is EntityAlive killedEntity)
		{
			QuestEventManager.Current.EntityKilled(killedBy, killedEntity);
		}
	}

	public override int GetLength()
	{
		return 8;
	}
}
