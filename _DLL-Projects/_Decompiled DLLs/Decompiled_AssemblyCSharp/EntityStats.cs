using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

public class EntityStats
{
	public const int cVersion = 11;

	public static bool WeatherSurvivalEnabled;

	public static bool NewWeatherSurvivalEnabled = true;

	public Stat Health;

	public Stat Stamina;

	public Stat Water;

	public Stat Food;

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityAlive m_entity;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_amountEnclosed;

	[PublicizedFrom(EAccessModifier.Private)]
	public float buffDamageRemainder;

	[PublicizedFrom(EAccessModifier.Protected)]
	public const int cWaitTicks = 10;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int waitTicks;

	[PublicizedFrom(EAccessModifier.Protected)]
	public const int cNetSyncWaitTicks = 10;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int netSyncWaitTicks = 10;

	public float AmountEnclosed
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_amountEnclosed;
		}
	}

	public EntityStats()
	{
		Health = new Stat();
	}

	public EntityStats(EntityAlive _ea)
	{
		m_entity = _ea;
		Init();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Init()
	{
		int num = (int)EffectManager.GetValue(PassiveEffects.HealthMax, null, 100f, m_entity);
		Health = new Stat(Stat.StatTypes.Health, m_entity, num, num)
		{
			MaxPassive = PassiveEffects.HealthMax,
			GainPassive = PassiveEffects.HealthGain,
			LossPassive = PassiveEffects.HealthLoss
		};
	}

	public virtual void CopyFrom(EntityStats _newStats)
	{
		Health.CopyFrom(_newStats.Health);
	}

	public virtual EntityStats SimpleClone()
	{
		EntityStats entityStats = new EntityStats();
		entityStats.Health.CopyFrom(Health);
		return entityStats;
	}

	public virtual void EntityBuffAdded(BuffValue _buff)
	{
	}

	public virtual void EntityBuffRemoved(BuffValue _buff)
	{
	}

	public void Tick(ulong worldTime)
	{
		if (!m_entity.isEntityRemote && !m_entity.IsDead())
		{
			if (++waitTicks >= 10)
			{
				waitTicks = 0;
			}
			TickWait(worldTime);
		}
	}

	public virtual void TickWait(ulong worldTime)
	{
		float dt = 0.5f;
		if (waitTicks == 1)
		{
			UpdateNPCStatsOverTime(dt);
			Health.Tick(dt);
		}
		if (waitTicks == 2 && Health.Changed)
		{
			SendStatChangePacket(NetPackageEntityStatChanged.EnumStat.Health);
			Health.Changed = false;
		}
		if (waitTicks != 6)
		{
			return;
		}
		if (netSyncWaitTicks > 0)
		{
			netSyncWaitTicks--;
			return;
		}
		netSyncWaitTicks = 10;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityStatsBuff>().Setup(m_entity));
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityStatsBuff>().Setup(m_entity));
		}
	}

	public void UpdateNPCStatsOverTime(float dt)
	{
		List<EffectManager.ModifierValuesAndSources> valuesAndSources = EffectManager.GetValuesAndSources(PassiveEffects.HealthChangeOT, null, 0f, m_entity);
		for (int i = 0; i < valuesAndSources.Count; i++)
		{
			EffectManager.ModifierValuesAndSources modifierValuesAndSources = valuesAndSources[i];
			if (modifierValuesAndSources.ParentType == MinEffectController.SourceParentType.BuffClass)
			{
				BuffValue buff = m_entity.Buffs.GetBuff((string)modifierValuesAndSources.Source);
				if (buff == null || buff.BuffClass == null)
				{
					continue;
				}
				BuffClass buffClass = buff.BuffClass;
				float _base_value = 0f;
				float _perc_value = 1f;
				buffClass.ModifyValue(m_entity, PassiveEffects.HealthChangeOT, buff, ref _base_value, ref _perc_value, FastTags<TagGroup.Global>.none);
				float num = _base_value * _perc_value * dt;
				if (num < 0f)
				{
					float num2 = 0f - num + buffDamageRemainder;
					int num3 = (int)num2;
					buffDamageRemainder = num2 - (float)num3;
					if (num3 > 0)
					{
						DamageSource damageSource = new DamageSource(buffClass.DamageSource, buffClass.DamageType);
						damageSource.BuffClass = buffClass;
						m_entity.DamageEntity(damageSource, num3, _criticalHit: false, 0f);
					}
				}
				else if (num > 0f)
				{
					Health.Value += num;
				}
			}
			else
			{
				Health.Value += modifierValuesAndSources.Value * dt;
			}
		}
	}

	public void ResetStats()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendStatChangePacket(NetPackageEntityStatChanged.EnumStat enumStat)
	{
		int num = ((!GameManager.IsDedicatedServer) ? m_entity.world.GetPrimaryPlayer().entityId : (-1));
		NetPackageEntityStatChanged package = NetPackageManager.GetPackage<NetPackageEntityStatChanged>().Setup(m_entity, num, enumStat);
		if (m_entity.world.IsRemote())
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		}
		else
		{
			m_entity.world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(m_entity.entityId, num, package, enumStat != NetPackageEntityStatChanged.EnumStat.Health);
		}
	}

	public virtual void Read(BinaryReader stream)
	{
		stream.ReadInt32();
		Health.Read(stream);
	}

	public virtual void Write(BinaryWriter stream)
	{
		stream.Write(11);
		Health.Write(stream);
	}
}
