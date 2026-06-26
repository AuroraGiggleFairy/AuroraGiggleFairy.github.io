using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityAddVelocity : NetPackage
{
	public int entityId;

	public Vector3 addVelocity;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageEntityAddVelocity Setup(int _entityId, Vector3 _addVelocity)
	{
		entityId = _entityId;
		addVelocity = _addVelocity;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		entityId = _br.ReadInt32();
		addVelocity = StreamUtils.ReadVector3(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(entityId);
		StreamUtils.Write(_bw, addVelocity);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_world?.GetGameManager().AddVelocityToEntityServer(entityId, addVelocity);
	}

	public override int GetLength()
	{
		return 16;
	}
}
