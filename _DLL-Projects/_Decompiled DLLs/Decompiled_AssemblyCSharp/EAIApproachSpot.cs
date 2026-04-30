using GamePath;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIApproachSpot : EAIBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cInvestigateChangeDist = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCloseDist = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cLookTimeMin = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cLookTimeMax = 8f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 investigatePos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 seekPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hadPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public int investigateTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int pathRecalculateTicks;

	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		MutexBits = 3;
		executeDelay = 0.1f;
	}

	public override bool CanExecute()
	{
		if (!theEntity.HasInvestigatePosition)
		{
			return false;
		}
		if (theEntity.IsSleeping)
		{
			return false;
		}
		investigatePos = theEntity.InvestigatePosition;
		seekPos = theEntity.world.FindSupportingBlockPos(investigatePos);
		return true;
	}

	public override void Start()
	{
		hadPath = false;
		updatePath();
	}

	public override bool Continue()
	{
		PathEntity path = theEntity.navigator.getPath();
		if (hadPath && path == null)
		{
			return false;
		}
		if (++investigateTicks > 40)
		{
			investigateTicks = 0;
			if (!theEntity.HasInvestigatePosition)
			{
				return false;
			}
			if ((investigatePos - theEntity.InvestigatePosition).sqrMagnitude >= 4f)
			{
				return false;
			}
		}
		if ((seekPos - theEntity.position).sqrMagnitude <= 4f || (path != null && path.isFinished()))
		{
			theEntity.ClearInvestigatePosition();
			return false;
		}
		return true;
	}

	public override void Update()
	{
		if (theEntity.navigator.getPath() != null)
		{
			hadPath = true;
			theEntity.moveHelper.CalcIfUnreachablePos();
		}
		Vector3 lookPosition = investigatePos;
		lookPosition.y += 0.8f;
		theEntity.SetLookPosition(lookPosition);
		if (--pathRecalculateTicks <= 0)
		{
			updatePath();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePath()
	{
		if (theEntity.IsScoutZombie)
		{
			AstarManager.Instance.AddLocationLine(theEntity.position, seekPos, 32);
		}
		if (!PathFinderThread.Instance.IsCalculatingPath(theEntity.entityId))
		{
			pathRecalculateTicks = 40 + GetRandom(20);
			theEntity.FindPath(seekPos, theEntity.GetMoveSpeedAggro(), canBreak: true, this);
		}
	}

	public override void Reset()
	{
		theEntity.moveHelper.Stop();
		theEntity.SetLookPosition(Vector3.zero);
		manager.lookTime = 5f + base.RandomFloat * 3f;
		manager.interestDistance = 2f;
	}

	public override string ToString()
	{
		return string.Format("{0}, {1} dist{2}", base.ToString(), theEntity.navigator.noPathAndNotPlanningOne() ? "(-path)" : (theEntity.navigator.noPath() ? "(!path)" : ""), (theEntity.position - seekPos).magnitude.ToCultureInvariantString());
	}
}
