using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityLookAt : NetPackage
{
	public int entityId;

	public Vector3 lookAtPosition;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageEntityLookAt Setup(int _entityId, Vector3 _lookAtPosition)
	{
		entityId = _entityId;
		lookAtPosition = _lookAtPosition;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		lookAtPosition = new Vector3(_reader.ReadInt32(), _reader.ReadInt32(), _reader.ReadInt32());
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		_writer.Write((int)lookAtPosition.x);
		_writer.Write((int)lookAtPosition.y);
		_writer.Write((int)lookAtPosition.z);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			EntityAlive entityAlive = (EntityAlive)_world.GetEntity(entityId);
			if (entityAlive != null && entityAlive.emodel != null && entityAlive.emodel.avatarController != null)
			{
				entityAlive.emodel.avatarController.SetLookPosition(lookAtPosition);
			}
		}
	}

	public override int GetLength()
	{
		return 8;
	}
}
