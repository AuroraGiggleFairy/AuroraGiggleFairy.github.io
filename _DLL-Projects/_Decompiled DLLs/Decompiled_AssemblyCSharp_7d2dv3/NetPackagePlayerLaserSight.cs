using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerLaserSight : NetPackage
{
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool laserSightActive;

	public Vector3 laserSightPosition;

	public NetPackagePlayerLaserSight Setup(int _entityId, bool _laserSightActive, Vector3 _laserSightPosition)
	{
		entityId = _entityId;
		laserSightActive = _laserSightActive;
		laserSightPosition = _laserSightPosition;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		laserSightActive = _reader.ReadBoolean();
		if (laserSightActive)
		{
			laserSightPosition = StreamUtils.ReadVector3(_reader);
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		_writer.Write(laserSightActive);
		if (laserSightActive)
		{
			StreamUtils.Write(_writer, laserSightPosition);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerLaserSight>().Setup(entityId, laserSightActive, laserSightPosition), _onlyClientsAttachedToAnEntity: false, -1, base.Sender.entityId);
			}
			EntityAlive entityAlive = (EntityAlive)_world.GetEntity(entityId);
			if (entityAlive != null && entityAlive.inventory.holdingItem != null && entityAlive.inventory.holdingItemData.actionData[0] is ItemActionRanged.ItemActionDataRanged itemActionDataRanged && itemActionDataRanged.Laser != null)
			{
				itemActionDataRanged.Laser.gameObject.SetActive(laserSightActive);
				itemActionDataRanged.Laser.position = laserSightPosition - Origin.position;
			}
		}
	}

	public override int GetLength()
	{
		return 8;
	}
}
