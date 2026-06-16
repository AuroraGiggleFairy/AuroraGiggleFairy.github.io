using UnityEngine.Scripting;

[Preserve]
public class NetPackageInventoryKeepOpen : NetPackage
{
	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageInventoryKeepOpen Setup()
	{
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		LockManager.Instance.ProcessKeepOpen(base.Sender.entityId);
	}

	public override int GetLength()
	{
		return 0;
	}
}
