using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityStatChanged : NetPackage
{
	public enum EnumStat
	{
		Health,
		Stamina,
		Sickness,
		Gassiness,
		SpeedModifier,
		Wellness,
		CoreTempOLD,
		Food,
		Water
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_instigatorId;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumStat m_enumStat;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_value;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_max;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_maxModifier;

	public NetPackageEntityStatChanged Setup(EntityAlive entity, int instigatorId, EnumStat Estat)
	{
		m_entityId = entity.entityId;
		m_instigatorId = instigatorId;
		m_enumStat = Estat;
		Stat stat = GetStat(entity, Estat);
		m_value = stat.Value;
		m_max = stat.BaseMax;
		m_maxModifier = stat.MaxModifier;
		return this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Stat GetStat(EntityAlive entity, EnumStat stat)
	{
		return stat switch
		{
			EnumStat.Health => entity.Stats.Health, 
			EnumStat.Stamina => entity.Stats.Stamina, 
			EnumStat.Food => entity.Stats.Food, 
			EnumStat.Water => entity.Stats.Water, 
			_ => entity.Stats.Health, 
		};
	}

	public override void read(PooledBinaryReader _reader)
	{
		m_entityId = _reader.ReadInt32();
		m_instigatorId = _reader.ReadInt32();
		m_enumStat = (EnumStat)_reader.ReadByte();
		m_value = _reader.ReadSingle();
		m_max = _reader.ReadSingle();
		m_maxModifier = _reader.ReadSingle();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(m_entityId);
		_writer.Write(m_instigatorId);
		_writer.Write((byte)m_enumStat);
		_writer.Write(m_value);
		_writer.Write(m_max);
		_writer.Write(m_maxModifier);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || (m_entityId == _world.GetPrimaryPlayerId() && m_entityId == m_instigatorId) || !ValidEntityIdForSender(m_instigatorId))
		{
			return;
		}
		EntityAlive entityAlive = _world.GetEntity(m_entityId) as EntityAlive;
		if ((bool)entityAlive)
		{
			Stat stat = GetStat(entityAlive, m_enumStat);
			stat.BaseMax = m_max;
			stat.MaxModifier = m_maxModifier;
			stat.Value = m_value;
			stat.Changed = false;
			if (!entityAlive.isEntityRemote && m_enumStat == EnumStat.Health)
			{
				entityAlive.MinEventContext.Other = _world.GetEntity(m_instigatorId) as EntityAlive;
				entityAlive.FireEvent(MinEventTypes.onOtherHealedSelf);
			}
			if (!_world.IsRemote())
			{
				_world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(entityAlive.entityId, m_instigatorId, NetPackageManager.GetPackage<NetPackageEntityStatChanged>().Setup(entityAlive, m_instigatorId, m_enumStat), m_enumStat != EnumStat.Health);
			}
		}
	}

	public override int GetLength()
	{
		return 21;
	}
}
