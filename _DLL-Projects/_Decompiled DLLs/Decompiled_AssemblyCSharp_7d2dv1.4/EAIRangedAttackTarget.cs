using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIRangedAttackTarget : EAIBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int itemActionType;

	[PublicizedFrom(EAccessModifier.Private)]
	public float baseCooldown;

	[PublicizedFrom(EAccessModifier.Private)]
	public float cooldown;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entityTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public float attackTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float attackDuration;

	[PublicizedFrom(EAccessModifier.Private)]
	public float minRange = 4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float maxRange = 25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float unreachableRange;

	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		MutexBits = 11;
		cooldown = 3f;
		attackDuration = 20f;
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
		GetData(data, "itemType", ref itemActionType);
		GetData(data, "cooldown", ref baseCooldown);
		GetData(data, "duration", ref attackDuration);
		GetData(data, "minRange", ref minRange);
		GetData(data, "maxRange", ref maxRange);
		GetData(data, "unreachableRange", ref unreachableRange);
	}

	public override bool CanExecute()
	{
		if (theEntity.IsDancing)
		{
			return false;
		}
		if (cooldown > 0f)
		{
			cooldown -= executeWaitTime;
			return false;
		}
		if (!theEntity.IsAttackValid())
		{
			return false;
		}
		entityTarget = theEntity.GetAttackTarget();
		if (entityTarget == null)
		{
			return false;
		}
		if (!InRange())
		{
			return false;
		}
		return theEntity.CanSee(entityTarget);
	}

	public override void Start()
	{
		attackTime = 0f;
	}

	public override bool Continue()
	{
		if ((bool)entityTarget && entityTarget.IsAlive() && attackTime < attackDuration)
		{
			return theEntity.hasBeenAttackedTime <= 0;
		}
		return false;
	}

	public override void Update()
	{
		attackTime += 0.05f;
		if (attackTime < attackDuration * 0.5f)
		{
			Vector3 headPosition = entityTarget.getHeadPosition();
			if (theEntity.IsInFrontOfMe(headPosition))
			{
				theEntity.SetLookPosition(headPosition);
			}
		}
		Attack(isAttackReleased: false);
		if (theEntity.inventory.holdingItemData.actionData[itemActionType] is ItemActionVomit.ItemActionDataVomit { isDone: not false })
		{
			attackTime = attackDuration;
		}
	}

	public override void Reset()
	{
		Attack(isAttackReleased: true);
		theEntity.SetLookPosition(Vector3.zero);
		entityTarget = null;
		cooldown = baseCooldown + baseCooldown * 0.5f * base.RandomFloat;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Attack(bool isAttackReleased)
	{
		if (itemActionType == 0)
		{
			theEntity.Attack(isAttackReleased);
		}
		else
		{
			theEntity.Use(isAttackReleased);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool InRange()
	{
		float distanceSq = entityTarget.GetDistanceSq(theEntity);
		if (unreachableRange > 0f)
		{
			EntityMoveHelper moveHelper = theEntity.moveHelper;
			if (moveHelper.IsUnreachableAbove || moveHelper.IsUnreachableSide)
			{
				return distanceSq <= unreachableRange * unreachableRange;
			}
		}
		if (distanceSq >= minRange * minRange)
		{
			return distanceSq <= maxRange * maxRange;
		}
		return false;
	}

	public override string ToString()
	{
		bool flag = (bool)entityTarget && InRange();
		return string.Format("{0} {1}, inRange{2}, Time {3}", base.ToString(), entityTarget ? entityTarget.EntityName : "", flag, attackTime.ToCultureInvariantString("0.00"));
	}
}
