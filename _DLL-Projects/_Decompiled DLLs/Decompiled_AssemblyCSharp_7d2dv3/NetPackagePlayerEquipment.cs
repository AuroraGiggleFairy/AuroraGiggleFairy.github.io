using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerEquipment : NetPackageEntityTargeted
{
	public Equipment equipment;

	public NetPackagePlayerEquipment Setup(EntityAlive _entity)
	{
		Setup(_entity.entityId);
		equipment = _entity.equipment;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		base.read(_reader);
		equipment = Equipment.Read(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		equipment.Write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = _world.GetEntity(entityId) as EntityAlive;
		if (!entityAlive)
		{
			Log.Out("Discarding " + GetType().Name + " for entity Id=" + entityId);
		}
		else if (ValidEntityIdForSender(entityId, _allowAttachedToEntity: true))
		{
			entityAlive.equipment.Apply(equipment, isLocal: false);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerEquipment>().Setup(entityAlive), _onlyClientsAttachedToAnEntity: false, -1, base.Sender.entityId);
			}
		}
	}

	public override int GetLength()
	{
		return 60;
	}
}
