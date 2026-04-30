using UnityEngine.Scripting;

[Preserve]
public class NetPackageAddRemoveBuff : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int instigatorId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string buffName;

	[PublicizedFrom(EAccessModifier.Private)]
	public float duration;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool adding;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i instigatorPos;

	public NetPackageAddRemoveBuff Setup(int _entityId, string _buffName, float _duration, bool _adding, int _instigatorId, Vector3i _instigatorPos)
	{
		entityId = _entityId;
		buffName = _buffName;
		adding = _adding;
		instigatorId = _instigatorId;
		duration = _duration;
		instigatorPos = _instigatorPos;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		buffName = _reader.ReadString();
		duration = _reader.ReadSingle();
		adding = _reader.ReadBoolean();
		instigatorId = _reader.ReadInt32();
		instigatorPos = StreamUtils.ReadVector3i(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		_writer.Write(buffName);
		_writer.Write(duration);
		_writer.Write(adding);
		_writer.Write(instigatorId);
		StreamUtils.Write(_writer, instigatorPos);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageAddRemoveBuff>().Setup(entityId, buffName, duration, adding, instigatorId, instigatorPos), _onlyClientsAttachedToAnEntity: false, -1, _entitiesInRangeOfEntity: entityId, _allButAttachedToEntityId: instigatorId);
		}
		EntityAlive entityAlive = _world.GetEntity(entityId) as EntityAlive;
		if (entityAlive != null)
		{
			if (adding)
			{
				entityAlive.Buffs.AddBuff(buffName, instigatorPos, instigatorId, _netSync: false, _fromElectrical: false, duration);
			}
			else
			{
				entityAlive.Buffs.RemoveBuff(buffName, _netSync: false);
			}
		}
	}

	public override int GetLength()
	{
		return 0;
	}
}
