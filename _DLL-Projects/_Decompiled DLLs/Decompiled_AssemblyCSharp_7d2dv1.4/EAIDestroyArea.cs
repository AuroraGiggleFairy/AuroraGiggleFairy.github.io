using System.Diagnostics;
using GamePath;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIDestroyArea : EAIBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum eState
	{
		FindPath,
		HasPath,
		EndPath,
		Attack
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDumbDistance = 9f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 seekPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i seekBlockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLookFar;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isAtPathEnd;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delayTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public int attackTimeout;

	[PublicizedFrom(EAccessModifier.Private)]
	public eState state;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldRayHitInfo hitInfo = new WorldRayHitInfo();

	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		MutexBits = 3;
		executeDelay = 1f + base.RandomFloat * 0.9f;
	}

	public override bool CanExecute()
	{
		EntityMoveHelper moveHelper = theEntity.moveHelper;
		if (!moveHelper.CanBreakBlocks)
		{
			return false;
		}
		EntityAlive attackTarget = theEntity.GetAttackTarget();
		if (!attackTarget)
		{
			return false;
		}
		if (theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
			return false;
		}
		bool flag = isLookFar;
		if (moveHelper.IsDestroyAreaTryUnreachable)
		{
			moveHelper.IsDestroyAreaTryUnreachable = false;
			float num = moveHelper.UnreachablePercent;
			if (num > 0f)
			{
				if (base.RandomFloat < num)
				{
					flag = true;
					num = 0f;
				}
				moveHelper.UnreachablePercent = num * 0.5f;
			}
		}
		if (manager.pathCostScale < 0.65f)
		{
			float num2 = (1f - manager.pathCostScale * 1.5384616f) * 0.6f;
			if (base.RandomFloat < num2)
			{
				PathEntity path = theEntity.navigator.getPath();
				if (path != null && path.NodeCountRemaining() > 18 && (attackTarget.position - theEntity.position).sqrMagnitude <= 81f)
				{
					flag = true;
				}
			}
		}
		if (!flag && !moveHelper.IsUnreachableAbove)
		{
			return false;
		}
		Vector3 destroyPos = theEntity.position;
		Vector3 vector = (moveHelper.IsUnreachableSide ? moveHelper.UnreachablePos : attackTarget.position);
		Vector3 vector2 = destroyPos - vector;
		float sqrMagnitude = vector2.sqrMagnitude;
		if (sqrMagnitude > 25f)
		{
			destroyPos = vector + vector2 * (5f / Mathf.Sqrt(sqrMagnitude));
		}
		destroyPos.x += -3f + base.RandomFloat * 6f;
		destroyPos.z += -3f + base.RandomFloat * 6f;
		if (!moveHelper.FindDestroyPos(ref destroyPos, isLookFar))
		{
			return false;
		}
		seekPos = destroyPos;
		seekBlockPos = World.worldToBlockPos(destroyPos);
		isLookFar = false;
		state = eState.FindPath;
		theEntity.navigator.clearPath();
		theEntity.FindPath(destroyPos, theEntity.GetMoveSpeedAggro(), canBreak: true, this);
		moveHelper.IsDestroyArea = true;
		return true;
	}

	public override void Start()
	{
		isAtPathEnd = false;
		delayTime = 3f;
		attackTimeout = 0;
	}

	public void Stop()
	{
		delayTime = 0f;
	}

	public override bool Continue()
	{
		if (theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
			return false;
		}
		if (delayTime <= 0f)
		{
			return false;
		}
		EntityMoveHelper moveHelper = theEntity.moveHelper;
		if (state == eState.FindPath && theEntity.navigator.HasPath())
		{
			moveHelper.CalcIfUnreachablePos();
			if (moveHelper.IsUnreachableAbove || moveHelper.IsUnreachableSide)
			{
				isLookFar = true;
				return false;
			}
			moveHelper.IsUnreachableAbove = true;
			state = eState.HasPath;
			delayTime = 15f;
			theEntity.navigator.ShortenEnd(0.2f);
		}
		if (state == eState.HasPath)
		{
			PathEntity path = theEntity.navigator.getPath();
			if (path != null && path.NodeCountRemaining() <= 1)
			{
				state = eState.EndPath;
				delayTime = 5f + base.RandomFloat * 5f;
				isAtPathEnd = true;
			}
		}
		if (state == eState.EndPath && !moveHelper.IsBlocked)
		{
			if (!Voxel.BlockHit(hitInfo, seekBlockPos))
			{
				return false;
			}
			state = eState.Attack;
			theEntity.SeekYawToPos(seekPos, 10f);
		}
		if (!isAtPathEnd && theEntity.navigator.noPathAndNotPlanningOne())
		{
			return false;
		}
		return true;
	}

	public override void Update()
	{
		delayTime -= 0.05f;
		if (state == eState.Attack && --attackTimeout <= 0 && theEntity.inventory.holdingItemData.actionData[0] is ItemActionAttackData itemActionAttackData)
		{
			theEntity.SetLookPosition(Vector3.zero);
			if (theEntity.Attack(_bAttackReleased: false))
			{
				attackTimeout = theEntity.GetAttackTimeoutTicks();
				itemActionAttackData.hitDelegate = GetHitInfo;
				theEntity.Attack(_bAttackReleased: true);
				state = eState.EndPath;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldRayHitInfo GetHitInfo(out float damageScale)
	{
		damageScale = 1f;
		return hitInfo;
	}

	public override void Reset()
	{
		EntityMoveHelper moveHelper = theEntity.moveHelper;
		moveHelper.Stop();
		moveHelper.IsUnreachableAbove = false;
		moveHelper.IsDestroyArea = false;
	}

	public override string ToString()
	{
		return string.Format("{0}, {1}, delayTime {2}", base.ToString(), state.ToStringCached(), delayTime.ToCultureInvariantString("0.00"));
	}

	[Conditional("DEBUG_AIDESTROY")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void LogDestroy(string _format = "", params object[] _args)
	{
		_format = $"{GameManager.frameCount} EAIDestroyArea {theEntity.EntityName} {theEntity.entityId}, {_format}";
		Log.Warning(_format, _args);
	}
}
