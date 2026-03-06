using GamePath;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAILeap : EAIBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cCollisionMask = 1082195968;

	[PublicizedFrom(EAccessModifier.Private)]
	public int legCount = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 leapV;

	[PublicizedFrom(EAccessModifier.Private)]
	public float leapDist;

	[PublicizedFrom(EAccessModifier.Private)]
	public float leapYaw;

	[PublicizedFrom(EAccessModifier.Private)]
	public float abortTime;

	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		MutexBits = 3;
		executeDelay = 1f + base.RandomFloat;
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
		GetData(data, "legs", ref legCount);
	}

	public override bool CanExecute()
	{
		if (theEntity.IsDancing)
		{
			return false;
		}
		if (!theEntity.GetAttackTarget())
		{
			return false;
		}
		if (theEntity.Jumping)
		{
			return false;
		}
		if ((legCount <= 2) ? theEntity.bodyDamage.IsAnyLegMissing : theEntity.bodyDamage.IsAnyArmOrLegMissing)
		{
			return false;
		}
		if (theEntity.moveHelper.BlockedFlags > 0)
		{
			return false;
		}
		PathEntity path = theEntity.navigator.getPath();
		if (path == null)
		{
			return false;
		}
		float jumpMaxDistance = theEntity.jumpMaxDistance;
		leapV = path.GetEndPos() - theEntity.position;
		if (leapV.y < -5f || leapV.y > 0.5f + jumpMaxDistance * 0.5f)
		{
			return false;
		}
		leapDist = Mathf.Sqrt(leapV.x * leapV.x + leapV.z * leapV.z);
		if (leapDist < 2.8f || leapDist > jumpMaxDistance)
		{
			return false;
		}
		Vector3 position = theEntity.position;
		position.y += 1.5f;
		if (Physics.Raycast(position - Origin.position, leapV, out var _, leapDist - 0.5f, 1082195968))
		{
			return false;
		}
		return true;
	}

	public override void Start()
	{
		abortTime = 5f;
		theEntity.moveHelper.Stop();
		leapYaw = Mathf.Atan2(leapV.x, leapV.z) * 57.29578f;
	}

	public override bool Continue()
	{
		if (theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
			return false;
		}
		if (abortTime <= 0f)
		{
			return false;
		}
		EntityMoveHelper moveHelper = theEntity.moveHelper;
		theEntity.SeekYaw(leapYaw, 0f, 10f);
		if (Utils.FastAbs(Mathf.DeltaAngle(theEntity.rotation.y, leapYaw)) < 1f)
		{
			moveHelper.StartJump(calcYaw: false, leapDist, leapV.y);
			return false;
		}
		return true;
	}

	public override void Update()
	{
		abortTime -= 0.05f;
	}

	public override void Reset()
	{
	}
}
