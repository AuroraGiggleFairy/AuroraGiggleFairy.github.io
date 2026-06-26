using UnityEngine.Scripting;

[Preserve]
public class NetPackageSleeperWakeup : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int m_targetId;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageSleeperWakeup Setup(int targetId)
	{
		m_targetId = targetId;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		m_targetId = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(m_targetId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && _world.IsRemote())
		{
			EntityAlive entityAlive = _world.GetEntity(m_targetId) as EntityAlive;
			if (!(entityAlive == null))
			{
				entityAlive.ConditionalTriggerSleeperWakeUp();
			}
		}
	}

	public override int GetLength()
	{
		return 8;
	}
}
