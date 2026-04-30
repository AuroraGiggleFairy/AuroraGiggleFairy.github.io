using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityZombieCop : EntityZombie
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int ticksToStartToExplode;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int ticksToExplode;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float explodeDelay = 5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float explodeHealthThreshold = 0.4f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPrimed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string warnSoundName;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string tickSoundName;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float tickSoundDelayStart = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float tickSoundDelayScale = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float tickSoundDelay;

	public override void Init(int _entityClass)
	{
		base.Init(_entityClass);
	}

	public override void InitFromPrefab(int _entityClass)
	{
		base.InitFromPrefab(_entityClass);
	}

	public override void PostInit()
	{
		inventory.SetItem(0, inventory.GetBareHandItemValue(), 1);
		HandleNavObject();
	}

	public override void CopyPropertiesFromEntityClass()
	{
		DynamicProperties properties = EntityClass.list[entityClass].Properties;
		properties.ParseFloat(EntityClass.PropExplodeDelay, ref explodeDelay);
		properties.ParseFloat(EntityClass.PropExplodeHealthThreshold, ref explodeHealthThreshold);
		properties.ParseString(EntityClass.PropSoundExplodeWarn, ref warnSoundName);
		properties.ParseString(EntityClass.PropSoundTick, ref tickSoundName);
		if (tickSoundName != null)
		{
			string[] array = tickSoundName.Split(',');
			tickSoundName = array[0];
			if (array.Length >= 2)
			{
				tickSoundDelayStart = StringParsers.ParseFloat(array[1]);
				if (array.Length >= 3)
				{
					tickSoundDelayScale = StringParsers.ParseFloat(array[2]);
				}
			}
		}
		base.CopyPropertiesFromEntityClass();
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		if (isEntityRemote)
		{
			return;
		}
		if (!isPrimed && !IsSleeping && !Buffs.HasBuff("buffShocked"))
		{
			float num = Health;
			if (num > 0f && num < (float)GetMaxHealth() * explodeHealthThreshold)
			{
				isPrimed = true;
				ticksToStartToExplode = (int)(explodeDelay * 20f);
				PlayOneShot(warnSoundName);
			}
		}
		if (isPrimed && !IsDead())
		{
			if (ticksToStartToExplode > 0)
			{
				ticksToStartToExplode--;
				if (ticksToStartToExplode == 0)
				{
					SpecialAttack2 = true;
					ticksToExplode = (int)(explodeDelay / 5f * 1.5f * 20f);
				}
			}
			if (ticksToExplode > 0)
			{
				ticksToExplode--;
				if (ticksToExplode == 0)
				{
					NotifySleeperDeath();
					SetModelLayer(2);
					ticksToExplode = -1;
					GameManager.Instance.ExplosionServer(0, GetPosition(), World.worldToBlockPos(GetPosition()), base.transform.rotation, EntityClass.list[entityClass].explosionData, entityId, 0f, _bRemoveBlockAtExplPosition: false);
					timeStayAfterDeath = 0;
					SetDead();
				}
			}
			tickSoundDelay -= 0.05f;
			if (tickSoundDelay <= 0f)
			{
				tickSoundDelayStart *= tickSoundDelayScale;
				tickSoundDelay = tickSoundDelayStart;
				if (tickSoundDelay < 0.2f)
				{
					tickSoundDelay = 0.2f;
				}
				PlayOneShot(tickSoundName);
			}
		}
		if (ticksToExplode < 0)
		{
			motion.x *= 0.7f;
			motion.z *= 0.7f;
		}
	}

	public override float GetMoveSpeed()
	{
		if (ticksToExplode != 0)
		{
			return 0f;
		}
		return base.GetMoveSpeed();
	}

	public override float GetMoveSpeedAggro()
	{
		if (ticksToExplode != 0)
		{
			return 0f;
		}
		if (isPrimed)
		{
			return moveSpeedAggroMax;
		}
		return base.GetMoveSpeedAggro();
	}

	public override bool IsAttackValid()
	{
		if (isPrimed)
		{
			return false;
		}
		return base.IsAttackValid();
	}

	public override void ProcessDamageResponseLocal(DamageResponse _dmResponse)
	{
		if (!isEntityRemote && (_dmResponse.HitBodyPart & EnumBodyPartHit.Special) > EnumBodyPartHit.None)
		{
			bool flag = !isPrimed;
			ItemClass itemClass = _dmResponse.Source.ItemClass;
			if (itemClass != null && itemClass is ItemClassBlock && _dmResponse.Source.CreatorEntityId == -2)
			{
				flag = false;
			}
			if (flag)
			{
				HandlePrimingDetonator();
			}
		}
		base.ProcessDamageResponseLocal(_dmResponse);
	}

	public void PrimeDetonator()
	{
		Detonator componentInChildren = base.gameObject.GetComponentInChildren<Detonator>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.PulseRateScale = 1f;
			componentInChildren.gameObject.GetComponent<Light>().color = Color.red;
			componentInChildren.StartCountdown();
		}
		else
		{
			Log.Out("PrimeDetonator found no Detonator component");
		}
	}

	public void HandlePrimingDetonator(float overrideDelay = -1f)
	{
		PlayOneShot(warnSoundName);
		isPrimed = true;
		ticksToStartToExplode = (int)(((overrideDelay > 0f) ? overrideDelay : explodeDelay) * 20f);
		PrimeDetonator();
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityPrimeDetonator>().Setup(this));
	}
}
