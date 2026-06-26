using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageTurretSpawn : NetPackage
{
	public int entityType;

	public Vector3 pos;

	public Vector3 rot;

	public ItemValue itemValue;

	public int entityThatPlaced;

	public NetPackageTurretSpawn Setup(int _entityType, Vector3 _pos, Vector3 _rot, ItemValue _itemValue, int _entityThatPlaced = -1)
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
		bool flag = false;
		if (itemValue.ItemClass.HasAnyTags(FastTags<TagGroup.Global>.Parse("drone")) && DroneManager.CanAddMoreDrones())
		{
			flag = true;
		}
		else if ((itemValue.ItemClass.HasAnyTags(FastTags<TagGroup.Global>.Parse("turretRanged")) || itemValue.ItemClass.HasAnyTags(FastTags<TagGroup.Global>.Parse("turretMelee"))) && TurretTracker.CanAddMoreTurrets())
		{
			flag = true;
		}
		EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(entityThatPlaced) as EntityPlayer;
		if (flag && entityPlayer != null)
		{
			Entity entity = EntityFactory.CreateEntity(entityType, pos, rot);
			entity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
			if (entity as EntityTurret != null)
			{
				EntityTurret entityTurret = entity as EntityTurret;
				entityTurret.factionId = entityPlayer.factionId;
				entityTurret.belongsPlayerId = entityPlayer.entityId;
				entityTurret.factionRank = (byte)(entityPlayer.factionRank - 1);
				entityTurret.OriginalItemValue = itemValue.Clone();
				entityTurret.groundPosition = pos;
				entityTurret.ForceOn = true;
				entityTurret.Spawned = true;
				ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(entityThatPlaced);
				entityTurret.OwnerID = clientInfo.InternalId;
				entityPlayer.AddOwnedEntity(entityTurret);
				_world.SpawnEntityInWorld(entityTurret);
				entityTurret.bPlayerStatsChanged = true;
			}
			else if (entity as EntityDrone != null)
			{
				EntityDrone entityDrone = entity as EntityDrone;
				entityDrone.factionId = entityPlayer.factionId;
				entityDrone.belongsPlayerId = entityPlayer.entityId;
				entityDrone.factionRank = (byte)(entityPlayer.factionRank - 1);
				entityDrone.OriginalItemValue = itemValue.Clone();
				entityDrone.SetItemValueToLoad(entityDrone.OriginalItemValue);
				entityDrone.Spawned = true;
				entityDrone.PlayWakeupAnim = true;
				ClientInfo clientInfo2 = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(entityThatPlaced);
				entityDrone.OwnerID = clientInfo2.InternalId;
				entityPlayer.AddOwnedEntity(entityDrone);
				_world.SpawnEntityInWorld(entityDrone);
				entityDrone.bPlayerStatsChanged = true;
			}
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
