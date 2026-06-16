using UnityEngine.Scripting;

[Preserve]
public class NetPackageSleeperPassiveChange : NetPackageEntityTargeted
{
	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public new NetPackageSleeperPassiveChange Setup(int targetId)
	{
		base.Setup(targetId);
		return this;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && _world.IsRemote())
		{
			EntityAlive entityAlive = _world.GetEntity(entityId) as EntityAlive;
			if (!(entityAlive == null))
			{
				entityAlive.IsSleeperPassive = false;
			}
		}
	}

	public override int GetLength()
	{
		return 8;
	}
}
