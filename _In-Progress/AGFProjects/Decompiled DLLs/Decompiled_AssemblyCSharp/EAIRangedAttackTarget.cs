using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIRangedAttackTarget : EAIBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum State
	{
		StartAnim,
		ReleaseAnim,
		Attack,
		Release
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int itemActionType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int startAnimType = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public float releaseDelay = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float baseCooldown;

	[PublicizedFrom(EAccessModifier.Private)]
	public float cooldown;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entityTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public float attackDuration;

	[PublicizedFrom(EAccessModifier.Private)]
	public float elapsedTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float minRange = 4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float maxRange = 25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float unreachableRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public string soundStartName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string soundReleaseName;

	[PublicizedFrom(EAccessModifier.Private)]
	public State state;

	[PublicizedFrom(EAccessModifier.Private)]
	public float stateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float painHitsFelt;

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
		GetData(data, "startAnimType", ref startAnimType);
		GetData(data, "releaseDelay", ref releaseDelay);
		GetData(data, "cooldown", ref baseCooldown);
		GetData(data, "duration", ref attackDuration);
		GetData(data, "minRange", ref minRange);
		GetData(data, "maxRange", ref maxRange);
		GetData(data, "unreachableRange", ref unreachableRange);
		data.TryGetValue("sndStart", out soundStartName);
		data.TryGetValue("sndRelease", out soundReleaseName);
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
		if (!entityTarget || entityTarget.IsDead())
		{
			return false;
		}
		if (theEntity.bodyDamage.IsAnyLegMissing)
		{
			return false;
		}
		if (startAnimType >= 0 && theEntity.bodyDamage.IsAnyArmOrLegMissing)
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
		theEntity.emodel.avatarController.hitWeightMax = 0.5f;
		painHitsFelt = theEntity.painHitsFelt;
		elapsedTime = 0f;
		state = State.Attack;
		stateTime = 0f;
		if (startAnimType >= 0)
		{
			state = State.StartAnim;
			theEntity.StartAnimAction(startAnimType + 3000);
		}
		if (!string.IsNullOrEmpty(soundStartName))
		{
			Manager.BroadcastPlay(theEntity, soundStartName);
		}
	}

	public override bool Continue()
	{
		if (!entityTarget || entityTarget.IsDead() || elapsedTime >= attackDuration)
		{
			return false;
		}
		if (theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None || theEntity.Electrocuted)
		{
			return false;
		}
		if (startAnimType >= 0 && theEntity.bodyDamage.IsAnyArmOrLegMissing)
		{
			return false;
		}
		return theEntity.painHitsFelt - painHitsFelt < 1f;
	}

	public override void Update()
	{
		elapsedTime += 0.05f;
		if (elapsedTime < attackDuration * 0.5f)
		{
			Vector3 headPosition = entityTarget.getHeadPosition();
			if (theEntity.IsInFrontOfMe(headPosition))
			{
				theEntity.SetLookPosition(headPosition);
			}
			theEntity.SeekYawToPos(entityTarget.position, 30f);
		}
		stateTime += 0.05f;
		if (state == State.StartAnim)
		{
			if (theEntity.GetAnimActionState() != AvatarController.ActionState.Ready)
			{
				return;
			}
			theEntity.ContinueAnimAction(startAnimType + 1 + 3000);
			state = State.ReleaseAnim;
			stateTime = 0f;
			if (!string.IsNullOrEmpty(soundReleaseName))
			{
				Manager.BroadcastPlay(theEntity, soundReleaseName);
			}
		}
		if (state == State.ReleaseAnim)
		{
			if (stateTime < releaseDelay)
			{
				return;
			}
			state = State.Attack;
			stateTime = 0f;
		}
		theEntity.UseHoldingItem(itemActionType, _isReleased: false);
		if (!theEntity.IsHoldingItemInUse(itemActionType))
		{
			elapsedTime = float.MaxValue;
		}
	}

	public override void Reset()
	{
		theEntity.ShowHoldingItem(_show: false);
		theEntity.UseHoldingItem(itemActionType, _isReleased: true);
		theEntity.StartAnimAction(9999);
		theEntity.SetLookPosition(Vector3.zero);
		theEntity.emodel.avatarController.hitWeightMax = 1f;
		entityTarget = null;
		cooldown = baseCooldown + baseCooldown * 0.5f * base.RandomFloat;
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
		return string.Format("{0} {1}, inRange{2}, Time {3}", base.ToString(), entityTarget ? entityTarget.EntityName : "", flag, elapsedTime.ToCultureInvariantString("0.00"));
	}
}
