using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIWander : EAIBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float fade = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lookMin = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lookMax = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float executePercent = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Private)]
	public float time;

	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		MutexBits = 1;
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
		GetData(data, "exePer", ref executePercent);
		GetData(data, "fade", ref fade);
		GetData(data, "lookMin", ref lookMin);
		GetData(data, "lookMax", ref lookMax);
	}

	public override bool CanExecute()
	{
		if (theEntity.sleepingOrWakingUp)
		{
			return false;
		}
		if (manager.lookTime > 0f)
		{
			return false;
		}
		if (fade == 1f && theEntity.GetTicksNoPlayerAdjacent() >= 120)
		{
			return false;
		}
		if (theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
			return false;
		}
		bool isAlert = theEntity.IsAlert;
		if (!isAlert && executePercent * executeWaitTime <= base.RandomFloat)
		{
			return false;
		}
		int minXZ = 1;
		int num = (int)manager.interestDistance;
		if (isAlert)
		{
			minXZ = 2;
			num *= 2;
		}
		Vector3 dirV = ((base.RandomFloat < 0.6f) ? theEntity.GetForwardVector() : base.Random.RandomOnUnitCircleXZ);
		Vector3 vector = RandomPositionGenerator.CalcInDir(theEntity, minXZ, num, num, dirV, 90f);
		if (vector.y == 0f)
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
		theEntity.renderFadeMax = fade;
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
		manager.lookTime = base.Random.RandomRange(lookMin, lookMax);
		theEntity.moveHelper.Stop();
		theEntity.renderFadeMax = 1f;
	}
}
