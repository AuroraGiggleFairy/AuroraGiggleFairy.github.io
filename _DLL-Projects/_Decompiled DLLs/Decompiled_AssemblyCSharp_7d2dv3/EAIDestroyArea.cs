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
		FindExistingPos,
		FindPos,
		FindPath,
		WaitForPath,
		HasPath,
		EndPath,
		Attack
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDumbDistance = 9f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTowardsMeMaxDist = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cFindDirOffset = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float[] quadrants = new float[8] { 9f, 9f, 9f, -8f, -8f, -8f, -8f, 9f };

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 targetPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 seekPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i seekBlockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int findDir;

	[PublicizedFrom(EAccessModifier.Private)]
	public int findCount;

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
		executeDelay = 0.9f + base.RandomFloat * 0.6f;
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
		moveHelper.IsDestroyAreaTryUnreachable = false;
		Vector3 position = theEntity.position;
		targetPos = (moveHelper.IsUnreachableSide ? moveHelper.UnreachablePos : attackTarget.position);
		targetPos.y = Utils.FastMoveTowards(position.y, targetPos.y, 2f);
		seekPos = Vector3.MoveTowards(position, targetPos, 5f);
		seekPos.x += base.Random.RandomRange(11) - 5;
		seekPos.z += base.Random.RandomRange(11) - 5;
		findDir = (int)(Mathf.Atan2(targetPos.x - position.x, targetPos.z - position.z) * 0.6366198f + 4f) & 3;
		findCount = ((base.Random.RandomFloat < 0.1f) ? 4 : 5);
		state = eState.FindExistingPos;
		if (base.RandomFloat < 0.18f)
		{
			state = eState.FindPos;
		}
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
		EntityMoveHelper moveHelper = theEntity.moveHelper;
		if (moveHelper != null)
		{
			moveHelper.IsDestroyAreaTryUnreachable = false;
		}
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
		if (state == eState.FindExistingPos)
		{
			if (!moveHelper.FindExistingDestroyPos(ref seekPos))
			{
				state = eState.FindPos;
				return true;
			}
			state = eState.FindPath;
		}
		if (state == eState.FindPos)
		{
			Vector3 destroyPos;
			if (findCount > 4)
			{
				findCount--;
				destroyPos = targetPos;
				int destroyRadius = 4;
				if (base.RandomFloat < 0.65f)
				{
					destroyPos.x += base.Random.RandomRange(5) - 2;
					destroyPos.z += base.Random.RandomRange(5) - 2;
					destroyRadius = 7;
				}
				if (!moveHelper.FindDestroyPos(ref destroyPos, destroyRadius, isLookFar))
				{
					return true;
				}
			}
			else
			{
				destroyPos = seekPos;
				int num = findDir * 2;
				destroyPos.x += quadrants[num];
				destroyPos.z += quadrants[num + 1];
				if (!moveHelper.FindDestroyPos(ref destroyPos, 8, isLookFar))
				{
					findDir = (findDir + 1) & 3;
					if (--findCount <= 0)
					{
						isLookFar = !isLookFar;
						return false;
					}
					return true;
				}
			}
			seekPos = destroyPos;
			state = eState.FindPath;
		}
		if (state == eState.FindPath)
		{
			seekBlockPos = World.worldToBlockPos(seekPos);
			isLookFar = false;
			state = eState.WaitForPath;
			theEntity.navigator.clearPath();
			theEntity.FindPath(seekPos, theEntity.GetMoveSpeedAggro(), canBreak: true, this);
			moveHelper.IsDestroyArea = true;
			return true;
		}
		if (state == eState.WaitForPath && theEntity.navigator.HasPath())
		{
			theEntity.navigator.ShortenEnd(0f);
			moveHelper.IsUnreachableAbove = true;
			state = eState.HasPath;
			delayTime = 15f;
		}
		if (state == eState.HasPath)
		{
			PathEntity path = theEntity.navigator.getPath();
			if (path != null && path.NodeCountRemaining() <= 1)
			{
				theEntity.navigator.clearPath();
				state = eState.EndPath;
				isAtPathEnd = true;
				delayTime = 5f + base.RandomFloat * 6f;
				moveHelper.BlockedFlags = 0;
				seekPos.y = Utils.FastMin(seekPos.y, theEntity.position.y + 2.1f);
				seekBlockPos.y = Utils.Fastfloor(seekPos.y);
				return true;
			}
		}
		if (state == eState.EndPath && moveHelper.BlockedFlags == 0)
		{
			if (!Voxel.BlockHit(hitInfo, seekBlockPos))
			{
				isLookFar = true;
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
			if (theEntity.Attack(_isReleased: false))
			{
				attackTimeout = theEntity.GetAttackTimeoutTicks();
				itemActionAttackData.hitDelegate = GetHitInfo;
				theEntity.Attack(_isReleased: true);
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
		return string.Format("{0}, {1}, delayTime {2}, findC {3}", base.ToString(), state.ToStringCached(), delayTime.ToCultureInvariantString("0.00"), findCount);
	}

	[Conditional("DEBUG_AIDESTROY")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void LogDestroy(string _format = "", params object[] _args)
	{
		_format = $"{GameManager.frameCount} EAIDestroyArea {theEntity.EntityName} {theEntity.entityId}, {_format}";
		Log.Warning(_format, _args);
	}
}
