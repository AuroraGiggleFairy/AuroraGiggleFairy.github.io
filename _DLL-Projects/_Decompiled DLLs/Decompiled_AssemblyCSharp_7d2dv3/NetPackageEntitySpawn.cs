using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntitySpawn : NetPackageEntityTargeted
{
	public EntityCreationData es;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageEntitySpawn Setup(EntityCreationData _es)
	{
		Setup(_es.id);
		es = _es;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		base.read(_reader);
		es = new EntityCreationData();
		es.read(_reader, _bNetworkRead: true);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		es.write(_writer, _bNetworkWrite: true);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && es.clientEntityId != 0)
		{
			List<EntityPlayerLocal> localPlayers = _world.GetLocalPlayers();
			for (int i = 0; i < localPlayers.Count; i++)
			{
				if (localPlayers[i].entityId == es.belongsPlayerId)
				{
					_world.ChangeClientEntityIdToServer(es.clientEntityId, es.id);
					return;
				}
			}
		}
		_world.entityAsyncManager.StartCreateEntity(es, [PublicizedFrom(EAccessModifier.Internal)] (EntityAsyncManager.EntityCreateHandle op) =>
		{
			_world.SpawnEntityInWorld(op.Entity);
		});
	}

	public override int GetLength()
	{
		return 20;
	}
}
