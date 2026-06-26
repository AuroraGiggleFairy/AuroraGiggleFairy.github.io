using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntitySpawn : NetPackage
{
	public EntityCreationData es;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageEntitySpawn Setup(EntityCreationData _es)
	{
		es = _es;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
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
		Entity entity = EntityFactory.CreateEntity(es);
		_world.SpawnEntityInWorld(entity);
	}

	public override int GetLength()
	{
		return 20;
	}
}
