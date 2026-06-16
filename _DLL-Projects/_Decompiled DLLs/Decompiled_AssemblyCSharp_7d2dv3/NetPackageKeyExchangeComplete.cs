using UnityEngine.Scripting;

[Preserve]
public class NetPackageKeyExchangeComplete : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasSuccessful;

	public override bool AllowedBeforeAuth => true;

	public NetPackageKeyExchangeComplete Setup(bool _wasSuccessful)
	{
		wasSuccessful = _wasSuccessful;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		wasSuccessful = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(wasSuccessful);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.AntiCheatEncryptionAuthServer.CompleteKeyExchange(base.Sender, wasSuccessful);
	}

	public override int GetLength()
	{
		return 1;
	}
}
