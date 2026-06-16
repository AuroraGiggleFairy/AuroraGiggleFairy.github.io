using UnityEngine.Scripting;

[Preserve]
public class NetPackageSetAttackTarget : NetPackageEntityTargeted
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int m_targetId;

	public NetPackageSetAttackTarget Setup(int entityId, int targetId)
	{
		Setup(entityId);
		m_targetId = targetId;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		base.read(_reader);
		m_targetId = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(m_targetId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			EntityAlive entityAlive = _world.GetEntity(entityId) as EntityAlive;
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
