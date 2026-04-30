using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerStats : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive.EntityNetworkStats entityNetworkStats = new EntityAlive.EntityNetworkStats();

	public NetPackagePlayerStats Setup(EntityAlive _entity)
	{
		entityId = _entity.entityId;
		entityNetworkStats.FillFromEntity(_entity);
		return this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~NetPackagePlayerStats()
	{
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		entityNetworkStats.read(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		entityNetworkStats.write(_writer);
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
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && entityAlive is EntityPlayer)
			{
				entityNetworkStats.SetName(base.Sender.playerName);
			}
			entityNetworkStats.ToEntity(entityAlive);
			entityAlive.EnqueueNetworkStats(entityNetworkStats);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(entityAlive), _onlyClientsAttachedToAnEntity: false, -1, base.Sender.entityId);
			}
		}
	}

	public override int GetLength()
	{
		return 60;
	}
}
