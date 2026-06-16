using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerDisconnect : NetPackagePlayerData
{
	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public new NetPackagePlayerDisconnect Setup(EntityPlayer _player)
	{
		base.Setup(_player);
		return this;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		base.ProcessPackage(_world, _callbacks);
		_callbacks.PlayerDisconnected(base.Sender);
	}
}
