using UnityEngine.Scripting;

[Preserve]
public class NetPackageWallVolume : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int id;

	[PublicizedFrom(EAccessModifier.Private)]
	public WallVolume wallVolume;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageWallVolume Setup(int _id, WallVolume _wallVolume)
	{
		id = _id;
		wallVolume = _wallVolume;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		id = _reader.ReadInt32();
		wallVolume = WallVolume.Read(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(id);
		wallVolume.Write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && _callbacks.worldInitInfoReceived && SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			_world.AddWallVolumeAt(id, wallVolume);
		}
	}

	public override int GetLength()
	{
		return 29;
	}
}
