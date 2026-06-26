using UnityEngine.Scripting;

[Preserve]
public class NetPackageSetAttackTarget : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int m_targetId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_entityId;

	public NetPackageSetAttackTarget Setup(int entityId, int targetId)
	{
		m_entityId = entityId;
		m_targetId = targetId;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		m_entityId = _reader.ReadInt32();
		m_targetId = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(m_entityId);
		_writer.Write(m_targetId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			EntityAlive entityAlive = _world.GetEntity(m_entityId) as EntityAlive;
			if (!(entityAlive == null))
			{
				EntityAlive attackTargetClient = _world.GetEntity(m_targetId) as EntityAlive;
				entityAlive.SetAttackTargetClient(attackTargetClient);
			}
		}
	}

	public override int GetLength()
	{
		return 8;
	}
}
