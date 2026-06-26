using UnityEngine.Scripting;

[Preserve]
public class NetPackageEditorAddSleeperVolume : NetPackage
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3i hitPointBlockPos;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageEditorAddSleeperVolume Setup(Vector3i _hitPointBlockPos)
	{
		hitPointBlockPos = _hitPointBlockPos;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		hitPointBlockPos = StreamUtils.ReadVector3i(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		StreamUtils.Write(_bw, hitPointBlockPos);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && !_world.IsRemote())
		{
			PrefabSleeperVolumeManager.Instance.AddSleeperVolumeServer(hitPointBlockPos);
		}
	}

	public override int GetLength()
	{
		return 16;
	}
}
