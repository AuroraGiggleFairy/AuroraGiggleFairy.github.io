using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityAddExpServer : NetPackageEntityAddExpClient
{
	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageEntityAddExpServer Setup(int _entityId, int _experience)
	{
		Setup(_entityId, _experience, Progression.XPTypes.Other);
		return this;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			EntityPlayer entityPlayer = (EntityPlayer)_world.GetEntity(entityId);
			if (!(entityPlayer == null) && entityPlayer.isEntityRemote)
			{
				entityPlayer.Progression.AddLevelExp(xp);
			}
		}
	}
}
