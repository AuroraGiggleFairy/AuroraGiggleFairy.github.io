using UnityEngine.Scripting;

[Preserve]
public class NetPackageWallVolumeRemove : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int index;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageWallVolumeRemove Setup(int _index)
	{
		index = _index;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		index = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(index);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			_world.RemoveWallVolumeAt(index);
		}
	}

	public override int GetLength()
	{
		return 8;
	}
}
