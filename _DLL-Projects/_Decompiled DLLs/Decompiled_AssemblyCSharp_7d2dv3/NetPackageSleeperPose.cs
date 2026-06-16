using UnityEngine.Scripting;

[Preserve]
public class NetPackageSleeperPose : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int m_targetId;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte m_pose;

	public NetPackageSleeperPose Setup(int targetId, byte pose)
	{
		m_targetId = targetId;
		m_pose = pose;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		m_targetId = _reader.ReadInt32();
		m_pose = _reader.ReadByte();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(m_targetId);
		_writer.Write(m_pose);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && _world.IsRemote())
		{
			EntityAlive entityAlive = _world.GetEntity(m_targetId) as EntityAlive;
			if (!(entityAlive == null))
			{
				entityAlive.TriggerSleeperPose(m_pose);
			}
		}
	}

	public override int GetLength()
	{
		return 8;
	}
}
