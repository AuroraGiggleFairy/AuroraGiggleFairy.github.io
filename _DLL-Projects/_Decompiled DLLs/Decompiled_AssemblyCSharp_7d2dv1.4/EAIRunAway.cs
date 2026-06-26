using GamePath;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class EAIRunAway : EAIBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 targetPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int timeoutTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fleeTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int pathTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkedPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fleeDistance = 12;

	public EAIRunAway()
	{
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
		GetData(data, "fleeDistance", ref fleeDistance);
	}

	public override bool CanExecute()
	{
		return FindFleePos(GetFleeFromPos());
	}

	public override void Start()
	{
		timeoutTicks = 800;
		fleeTicks = 0;
		pathTicks = 0;
		PathFinderThread.Instance.RemovePathsFor(theEntity.entityId);
	}

	public override bool Continue()
	{
		return timeoutTicks > 0;
	}

	public override void Update()
	{
		timeoutTicks--;
		PathEntity path = theEntity.navigator.getPath();
		if (checkedPath && path != null && path.getCurrentPathLength() >= 2 && path.NodeCountRemaining() <= 2)
		{
			fleeTicks = 0;
		}
		if (--fleeTicks <= 0)
		{
			Vector3 fleeFromPos = GetFleeFromPos();
			FindFleePos(fleeFromPos);
		}
		if (--pathTicks <= 0 && !PathFinderThread.Instance.IsCalculatingPath(theEntity.entityId))
		{
			pathTicks = 60;
			theEntity.FindPath(targetPos, theEntity.GetMoveSpeed(), canBreak: false, this);
			checkedPath = false;
		}
		if (checkedPath || PathFinderThread.Instance.IsCalculatingPath(theEntity.entityId))
		{
			return;
		}
		checkedPath = true;
		if (path != null)
		{
			Vector3 rawEndPos = path.rawEndPos;
			if (theEntity.GetDistanceSq(rawEndPos) < 1.21f && !FindRandomPos())
			{
				checkedPath = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool FindFleePos(Vector3 fleeFromPos)
	{
		Vector3 dirV = theEntity.position - fleeFromPos;
		Vector3 vector = RandomPositionGenerator.CalcPositionInDirection(theEntity, theEntity.position, dirV, fleeDistance, 80f);
		if (vector.Equals(Vector3.zero))
		{
			return false;
		}
		targetPos = vector;
		fleeTicks = 60;
		pathTicks = 0;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool FindRandomPos()
	{
		Vector3 vector = RandomPositionGenerator.Calc(theEntity, fleeDistance, 0);
		if (vector.Equals(Vector3.zero))
		{
			return false;
		}
		targetPos = vector;
		fleeTicks = 60;
		pathTicks = 0;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract Vector3 GetFleeFromPos();

	public override string ToString()
	{
		return $"{base.ToString()}, flee {fleeTicks}, timeout {timeoutTicks}";
	}
}
