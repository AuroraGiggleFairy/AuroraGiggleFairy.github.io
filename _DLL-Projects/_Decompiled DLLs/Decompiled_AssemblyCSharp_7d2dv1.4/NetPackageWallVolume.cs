using UnityEngine.Scripting;

[Preserve]
public class NetPackageWallVolume : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public WallVolume wallVolume;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageWallVolume Setup(WallVolume _wallVolume)
	{
		wallVolume = _wallVolume;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		wallVolume = WallVolume.Read(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		wallVolume.Write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			_world.AddWallVolume(wallVolume);
		}
	}

	public override int GetLength()
	{
		return 29;
	}
}
