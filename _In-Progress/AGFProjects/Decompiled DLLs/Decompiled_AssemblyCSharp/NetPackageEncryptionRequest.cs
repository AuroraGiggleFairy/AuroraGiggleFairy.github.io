using UnityEngine.Scripting;

[Preserve]
public class NetPackageEncryptionRequest : NetPackage
{
	public override bool AllowedBeforeAuth => true;

	public NetPackageEncryptionRequest Setup()
	{
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.AntiCheatEncryptionAuthClient.StartKeyExchange();
	}

	public override int GetLength()
	{
		return 0;
	}
}
