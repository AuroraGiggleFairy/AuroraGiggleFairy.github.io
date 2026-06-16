using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageHordeEvent : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirector.HordeEvent m_event;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_pos;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_maxDist;

	public NetPackageHordeEvent Setup(AIDirector.HordeEvent _event, Vector3 pos, float maxDist)
	{
		m_event = _event;
		m_pos = pos;
		m_maxDist = maxDist;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		int num = _reader.ReadByte();
		m_event = (AIDirector.HordeEvent)num;
		m_pos[0] = _reader.ReadSingle();
		m_pos[1] = _reader.ReadSingle();
		m_pos[2] = _reader.ReadSingle();
		m_maxDist = _reader.ReadSingle();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((byte)m_event);
		_writer.Write(m_pos[0]);
		_writer.Write(m_pos[1]);
		_writer.Write(m_pos[2]);
		_writer.Write(m_maxDist);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			EntityPlayerLocal primaryPlayer = _world.GetPrimaryPlayer();
			if (!(primaryPlayer == null) && (primaryPlayer.GetPosition() - m_pos).sqrMagnitude <= m_maxDist * m_maxDist)
			{
				primaryPlayer.HandleHordeEvent(m_event);
			}
		}
	}

	public override int GetLength()
	{
		return 21;
	}
}
