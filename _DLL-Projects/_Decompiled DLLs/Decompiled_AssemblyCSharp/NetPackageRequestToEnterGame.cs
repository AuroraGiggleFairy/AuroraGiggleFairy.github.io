using UnityEngine.Scripting;

[Preserve]
public class NetPackageRequestToEnterGame : NetPackage
{
	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public override void read(PooledBinaryReader _reader)
	{
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		ThreadManager.StartCoroutine(_callbacks.RequestToEnterGame(base.Sender));
	}

	public override int GetLength()
	{
		return 20;
	}
}
