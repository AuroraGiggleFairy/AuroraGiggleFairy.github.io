using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageVehicleSpawn : NetPackage
{
	public int entityType;

	public Vector3 pos;

	public Vector3 rot;

	public ItemValue itemValue;

	public int entityThatPlaced;

	public NetPackageVehicleSpawn Setup(int _entityType, Vector3 _pos, Vector3 _rot, ItemValue _itemValue, int _entityThatPlaced = -1)
	{
		entityType = _entityType;
		pos = _pos;
		rot = _rot;
		itemValue = _itemValue;
		entityThatPlaced = _entityThatPlaced;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityType = _reader.ReadInt32();
		pos = StreamUtils.ReadVector3(_reader);
		rot = StreamUtils.ReadVector3(_reader);
		itemValue = new ItemValue();
		itemValue.Read(_reader);
		entityThatPlaced = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityType);
		StreamUtils.Write(_writer, pos);
		StreamUtils.Write(_writer, rot);
		itemValue.Write(_writer);
		_writer.Write(entityThatPlaced);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || !ValidEntityIdForSender(entityThatPlaced))
		{
			return;
		}
		if (VehicleManager.CanAddMoreVehicles())
		{
			EntityVehicle entityVehicle = (EntityVehicle)EntityFactory.CreateEntity(entityType, pos, rot);
			entityVehicle.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
			entityVehicle.GetVehicle().SetItemValue(itemValue.Clone());
			if (GameManager.Instance.World.GetEntity(entityThatPlaced) as EntityPlayer != null)
			{
				entityVehicle.Spawned = true;
				ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(entityThatPlaced);
				entityVehicle.SetOwner(clientInfo.InternalId);
			}
			_world.SpawnEntityInWorld(entityVehicle);
			entityVehicle.bPlayerStatsChanged = true;
		}
		else
		{
			GameManager.Instance.ItemDropServer(new ItemStack(itemValue, 1), pos, Vector3.zero, entityThatPlaced);
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageVehicleCount>().Setup());
	}

	public override int GetLength()
	{
		return 20;
	}
}
