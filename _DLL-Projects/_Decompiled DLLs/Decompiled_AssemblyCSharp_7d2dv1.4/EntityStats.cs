using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EntityStats
{
	public const int kBinaryVersion = 9;

	public static bool WeatherSurvivalEnabled = true;

	public static bool NewWeatherSurvivalEnabled = false;

	public Stat Health;

	public Stat Stamina;

	public Stat CoreTemp;

	public Stat Water;

	public Stat Food;

	public float LightInsidePer;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<IEntityBuffsChanged> buffChangedDelegates;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<IEntityUINotificationChanged> notificationChangedDelegates;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityUINotification> m_notifications;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerTemperatureUINotification m_tempNotification;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] m_immunity = new int[0];

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer m_localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_localPlayerId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_seekWaterLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_isInShade;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_amountEnclosed;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_heightTemperatureOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_isEntityPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float buffDamageRemainder;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int raycastMask = 65809;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxWaitTicks = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public int waitTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MaxNetSyncWaitTicks = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public int netSyncWaitTicks = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityUINotification tmp_uiNotification;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastCoreTemp = 70f;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive m_entity;

	public float AmountEnclosed
	{
		get
		{
			return m_amountEnclosed;
		}
		set
		{
			m_amountEnclosed = value;
		}
	}

	public float HeightTemperatureOffset
	{
		get
		{
			return m_heightTemperatureOffset;
		}
		set
		{
			m_heightTemperatureOffset = value;
		}
	}

	public bool Shaded
	{
		get
		{
			return m_isInShade;
		}
		set
		{
			m_isInShade = value;
		}
	}

	public List<EntityUINotification> Notifications => m_notifications;

	public float WaterLevel => m_seekWaterLevel;

	public int LocalPlayerId
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (m_localPlayer == null)
			{
				m_localPlayer = m_entity.world.GetPrimaryPlayer();
				if (m_localPlayer != null)
				{
					m_localPlayerId = m_localPlayer.entityId;
				}
			}
			return m_localPlayerId;
		}
	}

	public EntityAlive Entity
	{
		get
		{
			return m_entity;
		}
		set
		{
			m_entity = value;
			Health.Entity = value;
			Stamina.Entity = value;
			CoreTemp.Entity = value;
			Water.Entity = value;
			Food.Entity = value;
			CreatePlayerNotifications();
		}
	}

	public EntityStats(EntityAlive entity)
	{
		m_entity = entity;
		m_isEntityPlayer = entity as EntityPlayer != null || entity as EntityPlayerLocal != null;
		int num = (int)EffectManager.GetValue(PassiveEffects.HealthMax, null, 100f, entity);
		int num2 = (int)EffectManager.GetValue(PassiveEffects.StaminaMax, null, 100f, entity);
		int num3 = (int)EffectManager.GetValue(PassiveEffects.FoodMax, null, 100f, entity);
		int num4 = (int)EffectManager.GetValue(PassiveEffects.WaterMax, null, 100f, entity);
		Health = new Stat(entity, num, num)
		{
			StatType = Stat.StatTypes.Health,
			GainPassive = PassiveEffects.HealthGain,
			ChangeOTPassive = PassiveEffects.HealthChangeOT,
			MaxPassive = PassiveEffects.HealthMax,
			LossPassive = PassiveEffects.HealthLoss
		};
		Stamina = new Stat(entity, num2, num2)
		{
			StatType = Stat.StatTypes.Stamina,
			GainPassive = PassiveEffects.StaminaGain,
			ChangeOTPassive = PassiveEffects.StaminaChangeOT,
			MaxPassive = PassiveEffects.StaminaMax,
			LossPassive = PassiveEffects.StaminaLoss
		};
		Water = new Stat(entity, num4, num4)
		{
			StatType = Stat.StatTypes.Water,
			GainPassive = PassiveEffects.WaterGain,
			ChangeOTPassive = PassiveEffects.WaterChangeOT,
			MaxPassive = PassiveEffects.WaterMax,
			LossPassive = PassiveEffects.WaterLoss
		};
		Food = new Stat(entity, num3, num3)
		{
			StatType = Stat.StatTypes.Food,
			GainPassive = PassiveEffects.FoodGain,
			ChangeOTPassive = PassiveEffects.FoodChangeOT,
			MaxPassive = PassiveEffects.FoodMax,
			LossPassive = PassiveEffects.FoodLoss
		};
		CoreTemp = new Stat(entity, -200f, 200f)
		{
			StatType = Stat.StatTypes.CoreTemp
		};
		buffChangedDelegates = new List<IEntityBuffsChanged>();
		notificationChangedDelegates = new List<IEntityUINotificationChanged>();
		m_notifications = new List<EntityUINotification>();
		CreatePlayerNotifications();
	}

	public void AddUINotificationChangedDelegate(IEntityUINotificationChanged _uiChangedDelegate)
	{
		if (!notificationChangedDelegates.Contains(_uiChangedDelegate))
		{
			notificationChangedDelegates.Add(_uiChangedDelegate);
		}
	}

	public void RemoveUINotificationChangedDelegate(IEntityUINotificationChanged _uiChangedDelegate)
	{
		notificationChangedDelegates.Remove(_uiChangedDelegate);
	}

	public void CopyBuffChangedDelegates(EntityStats _from)
	{
		if (_from == null)
		{
			return;
		}
		foreach (IEntityBuffsChanged buffChangedDelegate in _from.buffChangedDelegates)
		{
			AddBuffChangedDelegate(buffChangedDelegate);
		}
	}

	public void AddBuffChangedDelegate(IEntityBuffsChanged _buffChangedDelegate)
	{
		if (!buffChangedDelegates.Contains(_buffChangedDelegate))
		{
			buffChangedDelegates.Add(_buffChangedDelegate);
		}
	}

	public void RemoveBuffChangedDelegate(IEntityBuffsChanged _buffChangedDelegate)
	{
		buffChangedDelegates.Remove(_buffChangedDelegate);
	}

	public void EntityBuffAdded(BuffValue _buff)
	{
		for (int i = 0; i < buffChangedDelegates.Count; i++)
		{
			buffChangedDelegates[i].EntityBuffAdded(_buff);
		}
		if (!_buff.BuffClass.Hidden && _buff.BuffClass.Icon != null)
		{
			BuffEntityUINotification buffEntityUINotification = new BuffEntityUINotification();
			buffEntityUINotification.SetBuff(_buff);
			buffEntityUINotification.SetStats(this);
			NotificationAdded(buffEntityUINotification);
		}
	}

	public void EntityBuffRemoved(BuffValue _buff)
	{
		for (int i = 0; i < buffChangedDelegates.Count; i++)
		{
			buffChangedDelegates[i].EntityBuffRemoved(_buff);
		}
		int num = 0;
		while (num < m_notifications.Count)
		{
			if (m_notifications[num].Buff == _buff)
			{
				m_notifications[num].NotifyBuffRemoved();
				if (m_notifications[num].Expired)
				{
					NotificationRemoved(m_notifications[num]);
					continue;
				}
			}
			num++;
		}
	}

	public void NotificationAdded(EntityUINotification notification)
	{
		m_notifications.Add(notification);
		for (int i = 0; i < notificationChangedDelegates.Count; i++)
		{
			notificationChangedDelegates[i].EntityUINotificationAdded(notification);
		}
	}

	public void NotificationRemoved(EntityUINotification notification)
	{
		m_notifications.Remove(notification);
		for (int i = 0; i < notificationChangedDelegates.Count; i++)
		{
			notificationChangedDelegates[i].EntityUINotificationRemoved(notification);
		}
	}

	public void Update(float dt, ulong worldTime)
	{
		if (m_entity.isEntityRemote || m_entity.IsDead())
		{
			return;
		}
		if (++waitTicks >= 10)
		{
			waitTicks = 0;
		}
		m_isEntityPlayer = m_entity as EntityPlayer != null || m_entity as EntityPlayerLocal != null;
		dt = 0.5f;
		if (m_isEntityPlayer)
		{
			if (waitTicks == 1)
			{
				UpdateWeatherStats(dt, worldTime, m_entity.IsGodMode.Value);
			}
			if (waitTicks == 2)
			{
				UpdatePlayerFoodOT(dt);
				UpdatePlayerWaterOT(dt);
			}
			if (waitTicks == 3)
			{
				UpdatePlayerHealthOT(dt);
			}
			if (waitTicks == 4)
			{
				UpdatePlayerStaminaOT(dt);
			}
		}
		else if (waitTicks == 1)
		{
			UpdateNPCStatsOverTime(dt);
			Health.Tick(dt, 0uL);
			Stamina.Tick(dt, 0uL);
		}
		if (waitTicks == 5)
		{
			if (Health.Changed)
			{
				SendStatChangePacket(NetPackageEntityStatChanged.EnumStat.Health);
				Health.Changed = false;
			}
			if (Stamina.Changed)
			{
				SendStatChangePacket(NetPackageEntityStatChanged.EnumStat.Stamina);
				Stamina.Changed = false;
			}
			if (Water.Changed)
			{
				SendStatChangePacket(NetPackageEntityStatChanged.EnumStat.Water);
				Water.Changed = false;
			}
			if (Food.Changed)
			{
				SendStatChangePacket(NetPackageEntityStatChanged.EnumStat.Food);
				Food.Changed = false;
			}
		}
		if (waitTicks != 6)
		{
			return;
		}
		if (m_isEntityPlayer)
		{
			int num = 0;
			while (num < m_notifications.Count)
			{
				m_notifications[num].Tick(dt);
				if (m_notifications[num].Expired)
				{
					tmp_uiNotification = m_notifications[num];
					m_notifications.RemoveAt(num);
					for (int i = 0; i < notificationChangedDelegates.Count; i++)
					{
						notificationChangedDelegates[i].EntityUINotificationRemoved(tmp_uiNotification);
					}
				}
				else
				{
					num++;
				}
			}
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

	[PublicizedFrom(EAccessModifier.Private)]
	public float AdjustTemperatureFromEnclosure(float _temperature)
	{
		_temperature = ((_temperature < 70f) ? ((!(_temperature + 30f < 70f)) ? (70f * m_amountEnclosed + _temperature * (1f - m_amountEnclosed)) : ((_temperature + 30f) * m_amountEnclosed + _temperature * (1f - m_amountEnclosed))) : ((!(_temperature - 30f > 70f)) ? (70f * m_amountEnclosed + _temperature * (1f - m_amountEnclosed)) : ((_temperature - 30f) * m_amountEnclosed + _temperature * (1f - m_amountEnclosed))));
		return _temperature;
	}

	public float GetOutsideTemperature()
	{
		float num = WeatherManager.Instance.GetCurrentTemperatureValue();
		if (m_isInShade)
		{
			num = ((!(num > 70f)) ? (num + 8f) : (num - 8f));
		}
		else
		{
			if (WeatherManager.Instance.GetCurrentRainfallValue() > 0.25f && num > 70f)
			{
				num -= 10f;
			}
			float currentCloudThicknessPercent = WeatherManager.Instance.GetCurrentCloudThicknessPercent();
			float num2 = Mathf.Lerp(WeatherParams.OutsideTempChangeWhenInSun, WeatherParams.OutsideTempChangeWhenInSun * WeatherParams.OutsideTempChangeWhenInSunCloudScale, currentCloudThicknessPercent);
			num += num2;
		}
		return AdjustTemperatureFromEnclosure(num);
	}

	public void UpdateNPCStatsOverTime(float dt)
	{
		List<EffectManager.ModifierValuesAndSources> valuesAndSources = EffectManager.GetValuesAndSources(PassiveEffects.HealthChangeOT, null, 0f, m_entity);
		for (int i = 0; i < valuesAndSources.Count; i++)
		{
			EffectManager.ModifierValuesAndSources modifierValuesAndSources = valuesAndSources[i];
			if (modifierValuesAndSources.ParentType == MinEffectController.SourceParentType.BuffClass)
			{
				BuffValue buff = Entity.Buffs.GetBuff((string)modifierValuesAndSources.Source);
				if (buff == null || buff.BuffClass == null)
				{
					continue;
				}
				BuffClass buffClass = buff.BuffClass;
				float _base_value = 0f;
				float _perc_value = 1f;
				buffClass.ModifyValue(Entity, PassiveEffects.HealthChangeOT, buff, ref _base_value, ref _perc_value, FastTags<TagGroup.Global>.none);
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
						Entity.DamageEntity(damageSource, num3, _criticalHit: false, 0f);
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
		Stamina.Value += EffectManager.GetValue(PassiveEffects.StaminaChangeOT, null, 0f, m_entity) * dt;
	}

	public void UpdatePlayerFoodOT(float dt)
	{
		Food.RegenerationAmount += EffectManager.GetValue(PassiveEffects.FoodChangeOT, null, 0f, m_entity, null, Entity.CurrentMovementTag) * dt;
		Food.MaxModifier = 0f - EffectManager.GetValue(PassiveEffects.FoodMaxBlockage, null, 0f, m_entity);
		Food.Tick(dt, 0uL);
		Food.RegenerationAmount = 0f;
	}

	public void UpdatePlayerWaterOT(float dt)
	{
		Water.RegenerationAmount += EffectManager.GetValue(PassiveEffects.WaterChangeOT, null, 0f, m_entity) * dt;
		Water.MaxModifier = 0f - EffectManager.GetValue(PassiveEffects.WaterMaxBlockage, null, 0f, m_entity);
		Water.Tick(dt, 0uL);
		Water.RegenerationAmount = 0f;
	}

	public void UpdatePlayerHealthOT(float dt)
	{
		float value = EffectManager.GetValue(PassiveEffects.HealthChangeOT, null, 0f, m_entity);
		if (Health.ValuePercent < 1f && value > 0f)
		{
			float waterPercent = GetWaterPercent();
			Health.RegenerationAmount = value * waterPercent * dt;
		}
		else if (value < 0f)
		{
			List<EffectManager.ModifierValuesAndSources> valuesAndSources = EffectManager.GetValuesAndSources(PassiveEffects.HealthChangeOT, null, 0f, m_entity);
			float num = 0f;
			float num2 = 1f;
			for (int i = 0; i < valuesAndSources.Count; i++)
			{
				if (valuesAndSources[i].ParentType == MinEffectController.SourceParentType.BuffClass)
				{
					BuffValue buff = Entity.Buffs.GetBuff((string)valuesAndSources[i].Source);
					if (buff != null && buff.BuffClass != null && !buff.Remove)
					{
						BuffClass buffClass = buff.BuffClass;
						num = 0f;
						num2 = 1f;
						buffClass.ModifyValue(Entity, PassiveEffects.HealthChangeOT, buff, ref num, ref num2, FastTags<TagGroup.Global>.none);
						value = num * num2 * dt;
						if (value < 0f)
						{
							DamageSourceEntity damageSourceEntity = new DamageSourceEntity(buffClass.DamageSource, buffClass.DamageType, buff.InstigatorId);
							damageSourceEntity.BuffClass = buffClass;
							Entity.DamageEntity(damageSourceEntity, (int)(0f - value + 0.5f), _criticalHit: false, 0f);
						}
					}
				}
				else
				{
					Health.RegenerationAmount = valuesAndSources[i].Value * dt;
				}
			}
		}
		Health.MaxModifier = 0f - EffectManager.GetValue(PassiveEffects.HealthMaxBlockage, null, 0f, m_entity);
		Health.Tick(dt, 0uL);
	}

	public void UpdatePlayerStaminaOT(float dt)
	{
		float value = EffectManager.GetValue(PassiveEffects.StaminaChangeOT, null, 0f, m_entity);
		if (Stamina.ValuePercent < 1f && value > 0f)
		{
			Stamina.RegenerationAmount = value * dt;
		}
		else if (value < 0f)
		{
			Stamina.RegenerationAmount = value * dt;
		}
		Stamina.RegenerationAmount = EffectManager.GetValue(PassiveEffects.StaminaChangeOT, null, Stamina.RegenerationAmount, Entity, null, Entity.CurrentMovementTag | Entity.CurrentStanceTag) * dt;
		if (Stamina.RegenerationAmount > 0f)
		{
			float waterPercent = GetWaterPercent();
			Stamina.RegenerationAmount *= waterPercent;
		}
		Stamina.MaxModifier = 0f - EffectManager.GetValue(PassiveEffects.StaminaMaxBlockage, null, 0f, m_entity);
		Stamina.Tick(dt, 0uL);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetWaterPercent()
	{
		float num = Water.ValuePercentUI * (Water.Max * 0.01f);
		if (num != 0f)
		{
			num = ((num < 0.25f) ? 0.25f : ((!(num < 0.5f)) ? 1f : 0.5f));
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateWeatherStats(float dt, ulong worldTime, bool godMode)
	{
		m_amountEnclosed = GetAmountEnclosure();
		LightInsidePer = m_amountEnclosed;
		EntityAlive entity = Entity;
		EntityBuffs buffs = entity.Buffs;
		float wetnessPercentage = entity.GetWetnessPercentage();
		if (!WeatherSurvivalEnabled || WeatherManager.inWeatherGracePeriod || entity.IsGodMode.Value || entity.biomeStandingOn == null || buffs.HasBuff("god"))
		{
			buffs.SetCustomVar("_sheltered", m_amountEnclosed);
			buffs.SetCustomVar("_shaded", IsShaded() ? 1 : 0);
			buffs.SetCustomVar("_degreesabsorbed", 0f);
			buffs.SetCustomVar("_coretemp", 0f);
			buffs.SetCustomVar("_wetness", wetnessPercentage);
			buffs.SetCustomVar(".bodytemp", 70f);
			return;
		}
		m_isInShade = IsShaded();
		float outsideTemperature = GetOutsideTemperature();
		outsideTemperature -= 10f * wetnessPercentage;
		float value;
		if (outsideTemperature < 70f)
		{
			value = EffectManager.GetValue(PassiveEffects.HypothermalResist, null, 0f, entity);
			outsideTemperature = Mathf.Min(70f, outsideTemperature + value);
		}
		else
		{
			value = EffectManager.GetValue(PassiveEffects.HyperthermalResist, null, 0f, entity);
			outsideTemperature = Mathf.Max(70f, outsideTemperature - value);
		}
		if ((int)lastCoreTemp < (int)outsideTemperature)
		{
			lastCoreTemp += 1f;
		}
		else if ((int)lastCoreTemp > (int)outsideTemperature)
		{
			lastCoreTemp -= 1f;
		}
		buffs.SetCustomVar("_degreesabsorbed", value);
		buffs.SetCustomVar("_coretemp", lastCoreTemp - 70f);
		buffs.SetCustomVar("_sheltered", m_amountEnclosed);
		buffs.SetCustomVar("_shaded", m_isInShade ? 1 : 0);
		buffs.SetCustomVar("_wetness", wetnessPercentage);
		buffs.SetCustomVar(".bodytemp", lastCoreTemp);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetAmountEnclosure()
	{
		float num = 1f;
		Vector3i blockPos = World.worldToBlockPos(m_entity.GetPosition());
		IChunk chunkFromWorldPos = m_entity.world.GetChunkFromWorldPos(blockPos);
		if (chunkFromWorldPos != null && blockPos.y >= 0 && blockPos.y < 255)
		{
			num = Mathf.Max(chunkFromWorldPos.GetLight(blockPos.x, blockPos.y, blockPos.z, Chunk.LIGHT_TYPE.SUN), chunkFromWorldPos.GetLight(blockPos.x, blockPos.y + 1, blockPos.z, Chunk.LIGHT_TYPE.SUN));
			num /= 15f;
		}
		return 1f - num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsShaded()
	{
		Vector3 sunLightDirection = SkyManager.GetSunLightDirection();
		if (sunLightDirection.y > -0.25f)
		{
			return true;
		}
		Ray ray = new Ray(m_entity.getHeadPosition() - Origin.position + sunLightDirection * 0.5f, -sunLightDirection);
		bool result = false;
		if (Physics.SphereCast(ray, 0.5f, out var hitInfo, float.PositiveInfinity, 65809))
		{
			result = hitInfo.distance < float.PositiveInfinity;
		}
		return result;
	}

	public void ResetStats()
	{
		m_seekWaterLevel = 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveAllNotifications()
	{
		while (m_notifications.Count > 0)
		{
			for (int i = 0; i < notificationChangedDelegates.Count; i++)
			{
				notificationChangedDelegates[i].EntityUINotificationRemoved(m_notifications[0]);
			}
			m_notifications.RemoveAt(0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendStatChangePacket(NetPackageEntityStatChanged.EnumStat enumStat)
	{
		if (m_entity.world.IsRemote())
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityStatChanged>().Setup(m_entity, LocalPlayerId, enumStat));
		}
		else
		{
			m_entity.world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(m_entity.entityId, -1, NetPackageManager.GetPackage<NetPackageEntityStatChanged>().Setup(m_entity, LocalPlayerId, enumStat), enumStat != NetPackageEntityStatChanged.EnumStat.Health);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreatePlayerNotifications()
	{
	}

	public void Write(BinaryWriter stream)
	{
		stream.Write(9);
		stream.Write(m_immunity.Length);
		for (int i = 0; i < m_immunity.Length; i++)
		{
			stream.Write(m_immunity[i]);
		}
		ushort fileId = 0;
		Health.Write(stream, ref fileId);
		Stamina.Write(stream, ref fileId);
		CoreTemp.Write(stream, ref fileId);
		Water.Write(stream, ref fileId);
		Food.Write(stream, ref fileId);
		stream.Write(m_seekWaterLevel);
	}

	public void Read(BinaryReader stream)
	{
		int num = stream.ReadInt32();
		if (num > 3)
		{
			int num2 = stream.ReadInt32();
			for (int i = 0; i < num2; i++)
			{
				int num3 = stream.ReadInt32();
				if (i < m_immunity.Length)
				{
					m_immunity[i] = num3;
				}
			}
		}
		Health.Read(stream);
		Stamina.Read(stream);
		if (num < 8)
		{
			new Stat(Entity, 0f, 0f).Read(stream);
		}
		if (num > 4)
		{
			CoreTemp.Read(stream);
			Water.Read(stream);
			if (num > 8)
			{
				Food.Read(stream);
			}
		}
		else
		{
			CoreTemp.ResetAll();
			Water.ResetAll();
			Food.ResetAll();
			CoreTemp.Changed = false;
		}
		if (num > 5)
		{
			m_seekWaterLevel = stream.ReadSingle();
		}
	}

	public void ReadBeforeEmbeddedVersion(BinaryReader stream)
	{
		Health.Read(stream);
		Stamina.Read(stream);
		CoreTemp.ResetAll();
		CoreTemp.Changed = false;
		Water.ResetAll();
		Water.Changed = true;
		Food.ResetAll();
		Food.Changed = true;
	}

	public void InitWithOldFormatData(int health, int stamina, int sickness, int gassiness)
	{
		if (health != int.MinValue)
		{
			Health.Value = health;
		}
		if (stamina != int.MinValue)
		{
			Stamina.Value = stamina;
		}
	}

	public EntityStats SimpleClone()
	{
		EntityStats entityStats = new EntityStats(null);
		entityStats.Health.SimpleAssignFrom(Health);
		entityStats.Stamina.SimpleAssignFrom(Stamina);
		entityStats.CoreTemp.SimpleAssignFrom(CoreTemp);
		entityStats.Water.SimpleAssignFrom(Water);
		entityStats.Food.SimpleAssignFrom(Food);
		return entityStats;
	}
}
