using UnityEngine.Scripting;

[Preserve]
public class NetPackageRequestToSpawnEntity : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityCreationData ecd;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageRequestToSpawnEntity Setup(EntityCreationData _es)
	{
		ecd = _es;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		ecd = new EntityCreationData();
		ecd.read(_reader, _bNetworkRead: true);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		ecd.write(_writer, _bNetworkWrite: true);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_world?.GetGameManager().RequestToSpawnEntityServer(ecd);
	}

	public override int GetLength()
	{
		return 32;
	}
}
