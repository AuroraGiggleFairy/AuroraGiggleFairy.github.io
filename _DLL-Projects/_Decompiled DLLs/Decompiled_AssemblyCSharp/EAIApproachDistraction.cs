using GamePath;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIApproachDistraction : EAIBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCloseDist = 1.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cLookTime = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hadPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public int pathRecalculateTicks;

	public EAIApproachDistraction()
	{
		MutexBits = 3;
	}

	public override bool CanExecute()
	{
		EntityItem pendingDistraction = theEntity.pendingDistraction;
		if (!pendingDistraction || pendingDistraction.itemClass == null)
		{
			return false;
		}
		if ((bool)theEntity.GetAttackTarget())
		{
			if (!pendingDistraction.itemClass.IsEatDistraction)
			{
				theEntity.pendingDistraction = null;
			}
			return false;
		}
		if ((theEntity.position - pendingDistraction.position).sqrMagnitude < 2.25f && !pendingDistraction.itemClass.IsEatDistraction)
		{
			theEntity.pendingDistraction = null;
			return false;
		}
		return true;
	}

	public override void Start()
	{
		theEntity.SetAttackTarget(null, 0);
		theEntity.IsEating = false;
		theEntity.distraction = theEntity.pendingDistraction;
		theEntity.pendingDistraction = null;
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
		EntityItem distraction = theEntity.distraction;
		if (distraction == null || distraction.itemClass == null)
		{
			return false;
		}
		if (((theEntity.position - distraction.position).sqrMagnitude <= 2.25f || (path != null && path.isFinished())) && (!distraction.itemClass.IsEatDistraction || !distraction.IsDistractionActive))
		{
			return false;
		}
		return true;
	}

	public override void Update()
	{
		EntityItem distraction = theEntity.distraction;
		if ((bool)distraction)
		{
			PathEntity path = theEntity.getNavigator().getPath();
			if (path != null)
			{
				hadPath = true;
			}
			bool flag = false;
			if (path != null && !path.isFinished() && !theEntity.isCollidedHorizontally)
			{
				flag = true;
			}
			if (theEntity.IsSwimming())
			{
				flag = true;
			}
			if (Mathf.Abs(theEntity.speedForward) > 0.01f || Mathf.Abs(theEntity.speedStrafe) > 0.01f)
			{
				flag = true;
			}
			if (flag)
			{
				theEntity.SetLookPosition(distraction.position);
			}
			if ((theEntity.GetPosition() - distraction.position).sqrMagnitude <= 2.25f)
			{
				theEntity.IsEating = true;
				distraction.distractionEatTicks--;
			}
			else if (--pathRecalculateTicks <= 0)
			{
				updatePath();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePath()
	{
		if (!PathFinderThread.Instance.IsCalculatingPath(theEntity.entityId))
		{
			pathRecalculateTicks = 20 + GetRandom(20);
			theEntity.FindPath(theEntity.distraction.position, theEntity.GetMoveSpeedAggro(), canBreak: true, this);
		}
	}

	public override void Reset()
	{
		theEntity.moveHelper.Stop();
		theEntity.SetLookPosition(Vector3.zero);
		theEntity.IsEating = false;
		theEntity.distraction = null;
		manager.lookTime = 2f;
	}
}
