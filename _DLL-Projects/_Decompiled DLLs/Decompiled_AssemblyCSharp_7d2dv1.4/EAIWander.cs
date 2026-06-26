using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIWander : EAIBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cLookTimeMax = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Private)]
	public float time;

	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		MutexBits = 1;
	}

	public override bool CanExecute()
	{
		if (theEntity.sleepingOrWakingUp)
		{
			return false;
		}
		if (theEntity.GetTicksNoPlayerAdjacent() >= 120)
		{
			return false;
		}
		if (theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
			return false;
		}
		int num = (int)(200f * executeWaitTime);
		if (GetRandom(1000) >= num)
		{
			return false;
		}
		if (manager.lookTime > 0f)
		{
			return false;
		}
		int num2 = (int)manager.interestDistance;
		Vector3 vector;
		if (theEntity.IsAlert)
		{
			num2 *= 2;
			vector = RandomPositionGenerator.CalcAway(theEntity, 0, num2, num2, theEntity.LastTargetPos);
		}
		else
		{
			vector = RandomPositionGenerator.Calc(theEntity, num2, num2);
		}
		if (vector.Equals(Vector3.zero))
		{
			return false;
		}
		position = vector;
		return true;
	}

	public override void Start()
	{
		time = 0f;
		theEntity.FindPath(position, theEntity.GetMoveSpeed(), canBreak: false, this);
	}

	public override bool Continue()
	{
		if (theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
			return false;
		}
		if (theEntity.moveHelper.BlockedTime > 0.3f)
		{
			return false;
		}
		if (time > 30f)
		{
			return false;
		}
		return !theEntity.navigator.noPathAndNotPlanningOne();
	}

	public override void Update()
	{
		time += 0.05f;
	}

	public override void Reset()
	{
		manager.lookTime = base.RandomFloat * 3f;
		theEntity.moveHelper.Stop();
	}
}
