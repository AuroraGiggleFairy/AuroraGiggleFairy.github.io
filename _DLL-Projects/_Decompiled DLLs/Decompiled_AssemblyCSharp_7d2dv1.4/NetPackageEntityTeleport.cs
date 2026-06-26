using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityTeleport : NetPackageEntityPosAndRot
{
	public new NetPackageEntityTeleport Setup(Entity _entity)
	{
		base.Setup(_entity);
		return this;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && ValidEntityIdForSender(entityId, _allowAttachedToEntity: true))
		{
			Entity entity = _world.GetEntity(entityId);
			if (entity != null)
			{
				entity.serverPos = NetEntityDistributionEntry.EncodePos(pos);
				entity.SetPosAndRotFromNetwork(pos, rot, 0);
				entity.SetPosition(pos);
				entity.SetRotation(rot);
				entity.SetLastTickPos(pos);
				entity.onGround = onGround;
			}
			else
			{
				Log.Out("Discarding " + GetType().Name + " for entity Id=" + entityId);
			}
		}
	}

	public override int GetLength()
	{
		return 20;
	}
}
