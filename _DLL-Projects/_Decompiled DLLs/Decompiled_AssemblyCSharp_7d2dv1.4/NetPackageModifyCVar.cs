using UnityEngine.Scripting;

[Preserve]
public class NetPackageModifyCVar : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int m_entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cvarName;

	[PublicizedFrom(EAccessModifier.Private)]
	public float value;

	public NetPackageModifyCVar Setup(EntityAlive entity, string _cvarName, float _value)
	{
		m_entityId = entity.entityId;
		cvarName = _cvarName;
		value = _value;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		m_entityId = _reader.ReadInt32();
		cvarName = _reader.ReadString();
		value = _reader.ReadSingle();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(m_entityId);
		_writer.Write(cvarName);
		_writer.Write(value);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			EntityAlive entityAlive = _world.GetEntity(m_entityId) as EntityAlive;
			if (entityAlive != null)
			{
				entityAlive.Buffs.SetCustomVar(cvarName, value, SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer);
			}
		}
	}

	public override int GetLength()
	{
		return 0;
	}
}
