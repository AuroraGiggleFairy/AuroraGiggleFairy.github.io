using System.IO;
using UnityEngine;

public sealed class Stat
{
	public enum StatTypes
	{
		Health,
		Stamina,
		Food,
		Water,
		CoreTemp,
		SpeedModifier
	}

	public const int kBinaryVersion = 5;

	public StatTypes StatType;

	public PassiveEffects GainPassive;

	public PassiveEffects LossPassive;

	public PassiveEffects ChangeOTPassive;

	public PassiveEffects MaxPassive;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_baseMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_originalBaseMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_maxModifier;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_value;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_originalValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_valueModifier;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_isEntityAlive;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_isEntityPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_changed;

	public EntityAlive Entity;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_gainPassive;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_lossPassive;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_lossMaxMult;

	[PublicizedFrom(EAccessModifier.Private)]
	public float regenAmount;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_lastValue;

	public float RegenerationAmount
	{
		get
		{
			return regenAmount;
		}
		set
		{
			regenAmount = value;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public float RegenerationAmountUI
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public float Max => m_baseMax;

	public float ModifiedMax => m_baseMax + m_maxModifier;

	public float Value
	{
		get
		{
			if (GodModeEntity())
			{
				return ModifiedMax;
			}
			return Mathf.Clamp(m_value, 0f, ModifiedMax);
		}
		set
		{
			if (m_value != value)
			{
				m_value = Mathf.Clamp(value, 0f, ModifiedMax);
				SetChangedFlag(m_value, value);
			}
		}
	}

	public float ValuePercent => Utils.FastClamp01(Value / ModifiedMax);

	public float ValuePercentUI => Utils.FastClamp01(Value / Max);

	public float ModifiedMaxPercent => Utils.FastClamp01(ModifiedMax / Max);

	public float UnclampedValue => m_value;

	public float BaseMax
	{
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

	public float MaxModifier
	{
		get
		{
			return m_maxModifier;
		}
		set
		{
			m_maxModifier = Mathf.Clamp(value, 0f - Max * 0.75f, 0f);
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
		get
		{
			return m_changed;
		}
		set
		{
			m_changed = value;
		}
	}

	public Stat(EntityAlive entity, float value, float baseMax)
	{
		m_baseMax = baseMax;
		m_value = value;
		m_originalBaseMax = m_baseMax;
		m_originalValue = m_value;
		m_maxModifier = 0f;
		m_valueModifier = 0f;
		m_changed = false;
		Entity = entity;
		m_lastValue = m_value;
	}

	public void Tick(float dt, ulong worldTime = 0uL, bool godMode = false)
	{
		if (MaxPassive != PassiveEffects.None)
		{
			BaseMax = EffectManager.GetValue(MaxPassive, null, m_originalBaseMax, Entity);
		}
		m_isEntityAlive = Entity != null;
		m_isEntityPlayer = Entity as EntityPlayer != null;
		if ((StatType == StatTypes.Stamina || StatType == StatTypes.Health) && Mathf.Abs(m_lastValue - m_value) >= 1f)
		{
			if (m_value > m_lastValue && GainPassive != PassiveEffects.None)
			{
				m_value = Mathf.Clamp(m_lastValue + EffectManager.GetValue(GainPassive, null, m_value - m_lastValue, Entity), 0f, m_baseMax);
			}
			else if (m_value < m_lastValue && LossPassive != PassiveEffects.None)
			{
				m_value = Mathf.Clamp(m_lastValue - EffectManager.GetValue(LossPassive, null, m_lastValue - m_value, Entity), 0f, m_baseMax);
			}
		}
		if (m_value + RegenerationAmount > ModifiedMax)
		{
			RegenerationAmount = ModifiedMax - m_value;
		}
		RegenerationAmountUI = m_value - m_lastValue + RegenerationAmount / dt;
		m_value += RegenerationAmount;
		if (RegenerationAmount > 0f)
		{
			if (StatType == StatTypes.Stamina)
			{
				Entity.Stats.Water.RegenerationAmount -= RegenerationAmount * EffectManager.GetValue(PassiveEffects.WaterLossPerStaminaPointGained, null, 1f, Entity);
				Entity.Stats.Food.RegenerationAmount -= RegenerationAmount * EffectManager.GetValue(PassiveEffects.FoodLossPerStaminaPointGained, null, 1f, Entity);
			}
			else if (StatType == StatTypes.Health)
			{
				Entity.Stats.Water.RegenerationAmount -= RegenerationAmount * EffectManager.GetValue(PassiveEffects.WaterLossPerHealthPointGained, null, 1f, Entity);
				Entity.Stats.Food.RegenerationAmount -= RegenerationAmount * EffectManager.GetValue(PassiveEffects.FoodLossPerHealthPointGained, null, 1f, Entity);
			}
		}
		RegenerationAmount = m_value - m_lastValue;
		SetChangedFlag(m_value, m_lastValue);
		m_lastValue = m_value;
	}

	public void ResetAll()
	{
		ResetValue();
	}

	public void ResetValue()
	{
		m_value = m_originalValue;
		m_baseMax = m_originalBaseMax;
		m_maxModifier = 0f;
		m_valueModifier = 0f;
		m_changed = true;
	}

	public void SimpleAssignFrom(Stat stat)
	{
		m_baseMax = stat.m_baseMax;
		m_value = stat.m_value;
		m_maxModifier = stat.m_maxModifier;
		m_valueModifier = stat.m_valueModifier;
		m_originalValue = stat.m_originalValue;
		m_originalBaseMax = stat.m_originalBaseMax;
		m_changed = false;
	}

	public void Write(BinaryWriter stream, ref ushort fileId)
	{
		stream.Write(5);
		stream.Write(m_value);
		stream.Write(m_maxModifier);
		stream.Write(m_valueModifier);
		stream.Write(m_baseMax);
		stream.Write(m_originalBaseMax);
		stream.Write(m_originalValue);
	}

	public void Read(BinaryReader stream)
	{
		stream.ReadInt32();
		m_value = stream.ReadSingle();
		m_maxModifier = stream.ReadSingle();
		m_valueModifier = stream.ReadSingle();
		m_baseMax = stream.ReadSingle();
		m_originalBaseMax = stream.ReadSingle();
		m_originalValue = stream.ReadSingle();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool GodModeEntity()
	{
		if (Entity != null && Entity.entityId == Entity.world.GetPrimaryPlayerId() && !GameStats.GetBool(EnumGameStats.IsPlayerDamageEnabled))
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetChangedFlag(float newValue, float oldValue)
	{
		m_changed = m_changed || Mathf.FloorToInt(newValue) != Mathf.FloorToInt(oldValue);
	}
}
