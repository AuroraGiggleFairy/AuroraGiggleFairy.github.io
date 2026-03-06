using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerEntityStats : EntityStats
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<IEntityBuffsChanged> buffChangedDelegates;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<IEntityUINotificationChanged> notificationChangedDelegates;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityUINotification> m_notifications;

	public float CoreTemp = 70f;

	public float LightInsidePer;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float shadePer;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRaycastMask = 65809;

	public List<EntityUINotification> Notifications => m_notifications;

	public PlayerEntityStats()
	{
		Stamina = new Stat();
		Water = new Stat();
		Food = new Stat();
	}

	public PlayerEntityStats(EntityPlayer _ea)
		: base(_ea)
	{
		localPlayer = _ea as EntityPlayerLocal;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Init()
	{
		int num = (int)EffectManager.GetValue(PassiveEffects.HealthMax, null, 100f, m_entity);
		Health = new Stat(Stat.StatTypes.Health, m_entity, num, num)
		{
			GainPassive = PassiveEffects.HealthGain,
			MaxPassive = PassiveEffects.HealthMax,
			LossPassive = PassiveEffects.HealthLoss
		};
		int num2 = (int)EffectManager.GetValue(PassiveEffects.StaminaMax, null, 100f, m_entity);
		Stamina = new Stat(Stat.StatTypes.Stamina, m_entity, num2, num2)
		{
			GainPassive = PassiveEffects.StaminaGain,
			MaxPassive = PassiveEffects.StaminaMax,
			LossPassive = PassiveEffects.StaminaLoss
		};
		int num3 = (int)EffectManager.GetValue(PassiveEffects.WaterMax, null, 100f, m_entity);
		Water = new Stat(Stat.StatTypes.Water, m_entity, num3, num3)
		{
			GainPassive = PassiveEffects.WaterGain,
			MaxPassive = PassiveEffects.WaterMax,
			LossPassive = PassiveEffects.WaterLoss
		};
		int num4 = (int)EffectManager.GetValue(PassiveEffects.FoodMax, null, 100f, m_entity);
		Food = new Stat(Stat.StatTypes.Food, m_entity, num4, num4)
		{
			GainPassive = PassiveEffects.FoodGain,
			MaxPassive = PassiveEffects.FoodMax,
			LossPassive = PassiveEffects.FoodLoss
		};
		buffChangedDelegates = new List<IEntityBuffsChanged>();
		notificationChangedDelegates = new List<IEntityUINotificationChanged>();
		m_notifications = new List<EntityUINotification>();
	}

	public override void CopyFrom(EntityStats _stats)
	{
		PlayerEntityStats playerEntityStats = (PlayerEntityStats)_stats;
		Health.CopyFrom(playerEntityStats.Health);
		Stamina.CopyFrom(playerEntityStats.Stamina);
		Water.CopyFrom(playerEntityStats.Water);
		Food.CopyFrom(playerEntityStats.Food);
		CoreTemp = playerEntityStats.CoreTemp;
	}

	public override EntityStats SimpleClone()
	{
		PlayerEntityStats playerEntityStats = new PlayerEntityStats();
		playerEntityStats.Health.CopyFrom(Health);
		playerEntityStats.Stamina.CopyFrom(Stamina);
		playerEntityStats.Water.CopyFrom(Water);
		playerEntityStats.Food.CopyFrom(Food);
		playerEntityStats.CoreTemp = CoreTemp;
		return playerEntityStats;
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

	public override void EntityBuffAdded(BuffValue _buff)
	{
		for (int i = 0; i < buffChangedDelegates.Count; i++)
		{
			buffChangedDelegates[i].EntityBuffAdded(_buff);
		}
		BuffClass buffClass = _buff.BuffClass;
		if (!buffClass.Hidden && buffClass.Icon != null)
		{
			BuffEntityUINotification buffEntityUINotification = new BuffEntityUINotification(m_entity, _buff);
			m_notifications.Add(buffEntityUINotification);
			for (int j = 0; j < notificationChangedDelegates.Count; j++)
			{
				notificationChangedDelegates[j].EntityUINotificationAdded(buffEntityUINotification);
			}
		}
	}

	public override void EntityBuffRemoved(BuffValue _buff)
	{
		for (int i = 0; i < buffChangedDelegates.Count; i++)
		{
			buffChangedDelegates[i].EntityBuffRemoved(_buff);
		}
		int num = 0;
		while (num < m_notifications.Count)
		{
			EntityUINotification entityUINotification = m_notifications[num];
			if (entityUINotification.Buff == _buff)
			{
				m_notifications.RemoveAt(num);
				for (int j = 0; j < notificationChangedDelegates.Count; j++)
				{
					notificationChangedDelegates[j].EntityUINotificationRemoved(entityUINotification);
				}
			}
			else
			{
				num++;
			}
		}
	}

	public override void TickWait(ulong worldTime)
	{
		float dt = 0.5f;
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
	public void UpdateWeatherStats(float dt, ulong worldTime, bool godMode)
	{
		m_amountEnclosed = m_entity.GetAmountEnclosed();
		LightInsidePer = m_amountEnclosed;
		shadePer = CalcShadeFromSunPercent();
		EntityAlive entity = m_entity;
		EntityBuffs buffs = entity.Buffs;
		float wetnessRate = entity.GetWetnessRate();
		buffs.SetCustomVar("_wetnessrate", wetnessRate);
		float outsideTemperature = GetOutsideTemperature();
		buffs.SetCustomVar("_outsidetemp", outsideTemperature);
		float value = (localPlayer ? localPlayer.shelterPercent : 0f);
		buffs.SetCustomVar("_sheltered", value);
		if (!EntityStats.WeatherSurvivalEnabled || WeatherManager.inWeatherGracePeriod || entity.biomeStandingOn == null || entity.IsGodMode.Value || buffs.HasBuff("god"))
		{
			buffs.SetCustomVar("_degreesabsorbed", 0f);
			buffs.SetCustomVar("_coretemp", 70f);
			buffs.SetCustomVar("_shaded", shadePer);
			return;
		}
		float value2;
		if (outsideTemperature < 70f)
		{
			value2 = EffectManager.GetValue(PassiveEffects.HypothermalResist, null, 0f, entity);
			outsideTemperature = Utils.FastMin(70f, outsideTemperature + value2);
		}
		else
		{
			value2 = EffectManager.GetValue(PassiveEffects.HyperthermalResist, null, 0f, entity);
			outsideTemperature = Utils.FastMax(70f, outsideTemperature - value2);
		}
		CoreTemp = Utils.FastMoveTowards(CoreTemp, outsideTemperature, 1f);
		buffs.SetCustomVar("_degreesabsorbed", value2);
		buffs.SetCustomVar("_coretemp", CoreTemp);
		buffs.SetCustomVar("_shaded", shadePer);
	}

	public void UpdatePlayerFoodOT(float dt)
	{
		Food.RegenerationAmount += EffectManager.GetValue(PassiveEffects.FoodChangeOT, null, 0f, m_entity, null, m_entity.CurrentMovementTag) * dt;
		Food.MaxModifier = 0f - EffectManager.GetValue(PassiveEffects.FoodMaxBlockage, null, 0f, m_entity);
		Food.Tick(dt);
		Food.RegenerationAmount = 0f;
	}

	public void UpdatePlayerWaterOT(float dt)
	{
		Water.RegenerationAmount += EffectManager.GetValue(PassiveEffects.WaterChangeOT, null, 0f, m_entity) * dt;
		Water.MaxModifier = 0f - EffectManager.GetValue(PassiveEffects.WaterMaxBlockage, null, 0f, m_entity);
		Water.Tick(dt);
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
					BuffValue buff = m_entity.Buffs.GetBuff((string)valuesAndSources[i].Source);
					if (buff != null && buff.BuffClass != null && !buff.Remove)
					{
						BuffClass buffClass = buff.BuffClass;
						num = 0f;
						num2 = 1f;
						buffClass.ModifyValue(m_entity, PassiveEffects.HealthChangeOT, buff, ref num, ref num2, FastTags<TagGroup.Global>.none);
						value = num * num2 * dt;
						if (value < 0f)
						{
							DamageSourceEntity damageSourceEntity = new DamageSourceEntity(buffClass.DamageSource, buffClass.DamageType, buff.InstigatorId);
							damageSourceEntity.BuffClass = buffClass;
							m_entity.DamageEntity(damageSourceEntity, (int)(0f - value + 0.5f), _criticalHit: false, 0f);
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
		Health.Tick(dt);
	}

	public void UpdatePlayerStaminaOT(float _dt)
	{
		float value = EffectManager.GetValue(PassiveEffects.StaminaChangeOT, null, 0f, m_entity);
		if (Stamina.ValuePercent < 1f && value > 0f)
		{
			Stamina.RegenerationAmount = value * _dt;
		}
		else if (value < 0f)
		{
			Stamina.RegenerationAmount = value * _dt;
		}
		float num = EffectManager.GetValue(PassiveEffects.StaminaChangeOT, null, Stamina.RegenerationAmount, m_entity, null, m_entity.CurrentMovementTag | m_entity.CurrentStanceTag);
		if (num > 0f)
		{
			float waterPercent = GetWaterPercent();
			waterPercent = Utils.FastMax(0.2f, waterPercent);
			num *= waterPercent;
		}
		Stamina.RegenerationAmount = num * _dt;
		Stamina.MaxModifier = 0f - EffectManager.GetValue(PassiveEffects.StaminaMaxBlockage, null, 0f, m_entity);
		Stamina.Tick(_dt);
	}

	public float GetOutsideTemperature()
	{
		float temperature = WeatherManager.GetTemperature();
		temperature += Utils.FastLerp(0f, -20f, (m_entity.position.y - 130f) / 100f);
		float sunPercent = SkyManager.GetSunPercent();
		if (sunPercent > 0.25f)
		{
			temperature += shadePer * sunPercent * -8f;
		}
		if (localPlayer.shelterPercent < 0.5f)
		{
			float num = WeatherManager.GetWindSpeed() * 0.01f * -24f;
			temperature += num;
		}
		float b = ((!(temperature < 70f)) ? Utils.FastMax(70f, temperature - 30f) : Utils.FastMin(70f, temperature + 30f));
		return Utils.FastLerpUnclamped(temperature, b, localPlayer.shelterPercent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalcShadeFromSunPercent()
	{
		Vector3 sunLightDirection = SkyManager.GetSunLightDirection();
		if (sunLightDirection.y > -0.25f)
		{
			return 1f;
		}
		if (Physics.SphereCast(new Ray(m_entity.getHeadPosition() - Origin.position + sunLightDirection * 0.15f, -sunLightDirection), 0.15f, out var _, float.PositiveInfinity, 65809))
		{
			return 1f;
		}
		return 0f;
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
	public new void SendStatChangePacket(NetPackageEntityStatChanged.EnumStat enumStat)
	{
		NetPackageEntityStatChanged package = NetPackageManager.GetPackage<NetPackageEntityStatChanged>().Setup(m_entity, localPlayer.entityId, enumStat);
		if (m_entity.world.IsRemote())
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		}
		else
		{
			m_entity.world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(m_entity.entityId, -1, package, enumStat != NetPackageEntityStatChanged.EnumStat.Health);
		}
	}

	public override void Read(BinaryReader stream)
	{
		int num = stream.ReadInt32();
		Health.Read(stream);
		Stamina.Read(stream);
		if (num <= 10)
		{
			new Stat(Stat.StatTypes.Health, null, 0f, 0f).Read(stream);
		}
		Water.Read(stream);
		Food.Read(stream);
		if (num >= 11)
		{
			CoreTemp = stream.ReadSByte() * 2;
		}
	}

	public override void Write(BinaryWriter stream)
	{
		stream.Write(11);
		Health.Write(stream);
		Stamina.Write(stream);
		Water.Write(stream);
		Food.Write(stream);
		stream.Write((sbyte)(CoreTemp / 2f));
	}
}
