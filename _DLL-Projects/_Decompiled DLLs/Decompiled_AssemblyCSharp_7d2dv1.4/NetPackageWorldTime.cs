using UnityEngine.Scripting;

[Preserve]
public class NetPackageWorldTime : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ulong worldTime;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageWorldTime Setup(ulong _worldTime)
	{
		worldTime = _worldTime;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		worldTime = _br.ReadUInt64();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(worldTime);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_world?.gameManager.SetWorldTime(worldTime);
	}

	public override int GetLength()
	{
		return 8;
	}
}
