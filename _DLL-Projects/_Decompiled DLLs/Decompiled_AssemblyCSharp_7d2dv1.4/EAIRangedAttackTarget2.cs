using UnityEngine.Scripting;

[Preserve]
public class EAIRangedAttackTarget2 : EAIBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entityTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public int attackTimeout;

	[PublicizedFrom(EAccessModifier.Private)]
	public int itemActionType;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bCanSee;

	[PublicizedFrom(EAccessModifier.Private)]
	public int curAttackPeriod;

	[PublicizedFrom(EAccessModifier.Private)]
	public int attackPeriodMax;

	public EAIRangedAttackTarget2()
	{
		attackTimeout = 10;
		MutexBits = 3;
		attackPeriodMax = 20;
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
		GetData(data, "itemType", ref itemActionType);
		GetData(data, "attackPeriod", ref attackPeriodMax);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool inRange(float _distanceSq)
	{
		float value = EffectManager.GetValue(PassiveEffects.DamageFalloffRange, theEntity.inventory.holdingItemItemValue, 0f, theEntity);
		return _distanceSq < value * value * 0.25f;
	}

	public override bool CanExecute()
	{
		if (theEntity.inventory.holdingItem.Actions == null)
		{
			return false;
		}
		if (!(theEntity.inventory.holdingItem.Actions[0] is ItemActionRanged itemActionRanged))
		{
			return false;
		}
		if (theEntity.inventory.holdingItemItemValue.Meta <= 0)
		{
			if (itemActionRanged.CanReload(theEntity.inventory.holdingItemData.actionData[0]))
			{
				itemActionRanged.ReloadGun(theEntity.inventory.holdingItemData.actionData[0]);
			}
			return false;
		}
		if (attackTimeout > 0)
		{
			attackTimeout--;
			return false;
		}
		if (!theEntity.Spawned || !theEntity.IsAttackValid())
		{
			return false;
		}
		entityTarget = theEntity.GetAttackTarget();
		if (entityTarget == null)
		{
			return false;
		}
		float distanceSq = entityTarget.GetDistanceSq(theEntity);
		if (!inRange(distanceSq))
		{
			return false;
		}
		bCanSee = theEntity.CanSee(entityTarget);
		return bCanSee;
	}

	public override bool Continue()
	{
		if (curAttackPeriod > 0)
		{
			return theEntity.hasBeenAttackedTime <= 0;
		}
		return false;
	}

	public override void Start()
	{
		float delay = theEntity.inventory.holdingItem.Actions[0].Delay;
		attackTimeout = (int)(delay * 20f);
		curAttackPeriod = attackPeriodMax;
	}

	public override void Update()
	{
		curAttackPeriod--;
		if ((float)curAttackPeriod > (float)attackPeriodMax * 0.5f && theEntity.IsInFrontOfMe(entityTarget.getHeadPosition()))
		{
			theEntity.SetLookPosition(entityTarget.getBellyPosition());
		}
		if (inRange(entityTarget.GetDistanceSq(theEntity)) && theEntity.IsInFrontOfMe(entityTarget.getHeadPosition()))
		{
			if (itemActionType == 0)
			{
				theEntity.Attack((float)curAttackPeriod < (float)attackPeriodMax / 2f);
			}
			else
			{
				theEntity.Use((float)curAttackPeriod < (float)attackPeriodMax / 2f);
			}
		}
	}

	public override void Reset()
	{
		entityTarget = null;
		curAttackPeriod = 0;
		float delay = theEntity.inventory.holdingItem.Actions[0].Delay;
		attackTimeout = (int)(delay * 20f);
		attackTimeout = 5 + GetRandom(5);
		if (itemActionType == 0)
		{
			theEntity.Attack(_bAttackReleased: true);
		}
		else
		{
			theEntity.Use(_bUseReleased: true);
		}
	}

	public override string ToString()
	{
		bool flag = entityTarget != null && inRange(entityTarget.GetDistanceSq(theEntity));
		return base.ToString() + ": " + ((entityTarget != null) ? entityTarget.EntityName : "null") + " see: " + (bCanSee ? "Y" : "N") + " range=" + (flag ? "Y" : "N");
	}
}
