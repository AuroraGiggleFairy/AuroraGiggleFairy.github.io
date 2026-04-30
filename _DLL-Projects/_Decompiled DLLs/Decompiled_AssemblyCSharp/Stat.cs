using System.IO;
using System.Runtime.CompilerServices;

public sealed class Stat
{
	public enum StatTypes
	{
		Health,
		Stamina,
		Food,
		Water,
		CoreTemp
	}

	public const int cBinaryVersion = 6;

	public StatTypes StatType;

	public PassiveEffects GainPassive;

	public PassiveEffects LossPassive;

	public PassiveEffects MaxPassive;

	public EntityAlive Entity;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_value;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_originalValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_lastValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_baseMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_originalBaseMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_maxModifier;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_changed;

	[PublicizedFrom(EAccessModifier.Private)]
	public float regenAmount;

	public float RegenerationAmount
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return regenAmount;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			regenAmount = value;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public float RegenerationAmountUI
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public float Max
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_baseMax;
		}
	}

	public float BaseMax
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_baseMax;
		}
		set
		{
			if (m_baseMax != value)
			{
				SetChangedFlag(m_baseMax, value);
				m_baseMax = value;
			}
		}
	}

	public float ModifiedMax
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_baseMax + m_maxModifier;
		}
	}

	public float Value
	{
		get
		{
			if (GodModeEntity())
			{
				return ModifiedMax;
			}
			return Utils.FastClamp(m_value, 0f, ModifiedMax);
		}
		set
		{
			if (m_value != value)
			{
				float value2 = m_value;
				m_value = Utils.FastClamp(value, 0f, ModifiedMax);
				SetChangedFlag(value2, value);
			}
		}
	}

	public float ValuePercent => Utils.FastClamp01(Value / ModifiedMax);

	public float ValuePercentUI => Utils.FastClamp01(Value / Max);

	public float ModifiedMaxPercent => Utils.FastClamp01(ModifiedMax / Max);

	public float MaxModifier
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_maxModifier;
		}
		set
		{
			m_maxModifier = Utils.FastClamp(value, 0f - Max * 0.75f, 0f);
		}
	}

	public float OriginalValue
	{
		get
		{
			return m_originalValue;
		}
		set
		{
			m_originalValue = value;
		}
	}

	public float OriginalMax
	{
		get
		{
			return m_originalBaseMax;
		}
		set
		{
			m_originalBaseMax = value;
		}
	}

	public bool Changed
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_changed;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_changed = value;
		}
	}

	public Stat()
	{
	}

	public Stat(StatTypes _statType, EntityAlive _entity, float _value, float _baseMax)
	{
		StatType = _statType;
		Entity = _entity;
		m_value = _value;
		m_originalValue = _value;
		m_lastValue = _value;
		m_baseMax = _baseMax;
		m_originalBaseMax = _baseMax;
		m_maxModifier = 0f;
		m_changed = false;
	}

	public void Tick(float dt)
	{
		if (MaxPassive != PassiveEffects.None)
		{
			BaseMax = EffectManager.GetValue(MaxPassive, null, m_originalBaseMax, Entity);
		}
		if ((StatType == StatTypes.Health || StatType == StatTypes.Stamina) && Utils.FastAbs(m_lastValue - m_value) >= 1f)
		{
			if (m_value > m_lastValue && GainPassive != PassiveEffects.None)
			{
				m_value = Utils.FastClamp(m_lastValue + EffectManager.GetValue(GainPassive, null, m_value - m_lastValue, Entity), 0f, m_baseMax);
			}
			else if (m_value < m_lastValue && LossPassive != PassiveEffects.None)
			{
				m_value = Utils.FastClamp(m_lastValue - EffectManager.GetValue(LossPassive, null, m_lastValue - m_value, Entity), 0f, m_baseMax);
			}
		}
		if (m_value + regenAmount > ModifiedMax)
		{
			regenAmount = ModifiedMax - m_value;
		}
		RegenerationAmountUI = m_value - m_lastValue + regenAmount / dt;
		m_value += regenAmount;
		if (regenAmount > 0f)
		{
			if (StatType == StatTypes.Stamina)
			{
				Entity.Stats.Water.RegenerationAmount -= regenAmount * EffectManager.GetValue(PassiveEffects.WaterLossPerStaminaPointGained, null, 1f, Entity);
				Entity.Stats.Food.RegenerationAmount -= regenAmount * EffectManager.GetValue(PassiveEffects.FoodLossPerStaminaPointGained, null, 1f, Entity);
			}
			else if (StatType == StatTypes.Health)
			{
				Entity.Stats.Water.RegenerationAmount -= regenAmount * EffectManager.GetValue(PassiveEffects.WaterLossPerHealthPointGained, null, 1f, Entity);
				Entity.Stats.Food.RegenerationAmount -= regenAmount * EffectManager.GetValue(PassiveEffects.FoodLossPerHealthPointGained, null, 1f, Entity);
			}
		}
		regenAmount = m_value - m_lastValue;
		SetChangedFlag(m_value, m_lastValue);
		m_lastValue = m_value;
	}

	public void ResetValue()
	{
		m_value = m_originalValue;
		m_baseMax = m_originalBaseMax;
		m_maxModifier = 0f;
		m_changed = true;
	}

	public void CopyFrom(Stat _stat)
	{
		m_value = _stat.m_value;
		m_originalValue = _stat.m_originalValue;
		m_baseMax = _stat.m_baseMax;
		m_originalBaseMax = _stat.m_originalBaseMax;
		m_maxModifier = _stat.m_maxModifier;
		m_changed = false;
	}

	public void Write(BinaryWriter stream)
	{
		stream.Write(6);
		stream.Write(m_value);
		stream.Write(m_maxModifier);
		stream.Write(m_baseMax);
		stream.Write(m_originalBaseMax);
		stream.Write(m_originalValue);
	}

	public void Read(BinaryReader stream)
	{
		int num = stream.ReadInt32();
		m_value = stream.ReadSingle();
		m_maxModifier = stream.ReadSingle();
		if (num <= 5)
		{
			stream.ReadSingle();
		}
		m_baseMax = stream.ReadSingle();
		m_originalBaseMax = stream.ReadSingle();
		m_originalValue = stream.ReadSingle();
		m_lastValue = m_value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool GodModeEntity()
	{
		if ((bool)Entity && Entity.entityId == Entity.world.GetPrimaryPlayerId() && !GameStats.GetBool(EnumGameStats.IsPlayerDamageEnabled))
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetChangedFlag(float newValue, float oldValue)
	{
		m_changed = m_changed || Utils.Fastfloor(newValue) != Utils.Fastfloor(oldValue);
	}
}
