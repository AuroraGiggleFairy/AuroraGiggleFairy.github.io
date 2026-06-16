using UnityEngine.Scripting;

[Preserve]
public class NetPackageAuthConfirmation : NetPackage
{
	public override bool FlushQueue => true;

	public override bool AllowedBeforeAuth => true;

	public NetPackageAuthConfirmation Setup()
	{
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			AuthFinalizer.Instance.ReplyReceived(base.Sender);
		}
		else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageAuthConfirmation>().Setup());
		}
	}

	public override int GetLength()
	{
		return 9;
	}
}
