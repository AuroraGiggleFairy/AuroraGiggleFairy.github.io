using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerSpawnedInWorld : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public RespawnType respawnReason;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i position;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	public override NetPackageDirection PackageDirection => NetPackageDirection.Both;

	public NetPackagePlayerSpawnedInWorld Setup(RespawnType _respawnReason, Vector3i _position, int _entityId)
	{
		respawnReason = _respawnReason;
		position = _position;
		entityId = _entityId;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		respawnReason = (RespawnType)_reader.ReadInt32();
		position = StreamUtils.ReadVector3i(_reader);
		entityId = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((int)respawnReason);
		StreamUtils.Write(_writer, position);
		_writer.Write(entityId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && ValidEntityIdForSender(entityId))
		{
			GameManager.Instance.PlayerSpawnedInWorld(base.Sender, respawnReason, position, entityId);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerSpawnedInWorld>().Setup(respawnReason, new Vector3i(position), entityId), _onlyClientsAttachedToAnEntity: false, -1, base.Sender.entityId);
			}
		}
	}

	public override int GetLength()
	{
		return 16;
	}
}
