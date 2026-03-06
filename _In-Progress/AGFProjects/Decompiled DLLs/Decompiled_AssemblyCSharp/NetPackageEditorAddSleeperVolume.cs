using UnityEngine.Scripting;

[Preserve]
public class NetPackageEditorAddSleeperVolume : NetPackage
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3i startPos;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3i size;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageEditorAddSleeperVolume Setup(Vector3i _startPos, Vector3i _size)
	{
		startPos = _startPos;
		size = _size;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		startPos = StreamUtils.ReadVector3i(_br);
		size = StreamUtils.ReadVector3i(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		StreamUtils.Write(_bw, startPos);
		StreamUtils.Write(_bw, size);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && !_world.IsRemote())
		{
			PrefabSleeperVolumeManager.Instance.AddSleeperVolumeServer(startPos, size);
		}
	}

	public override int GetLength()
	{
		return 28;
	}
}
