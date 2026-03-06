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
	public int fleeDistance = 20;

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
		timeoutTicks = (30 + GetRandom(20)) * 20;
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
		if (!checkedPath && !PathFinderThread.Instance.IsCalculatingPath(theEntity.entityId))
		{
			checkedPath = true;
			if (path != null)
			{
				Vector3 rawEndPos = path.rawEndPos;
				if (theEntity.GetDistanceSq(rawEndPos) < 1.21f)
				{
					FindRandomPos();
				}
			}
		}
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
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool FindFleePos(Vector3 fleeFromPos)
	{
		Vector3 dirV = theEntity.position - fleeFromPos;
		Vector3 vector = RandomPositionGenerator.CalcPositionInDirection(theEntity, theEntity.position, dirV, fleeDistance, 80f);
		if (vector.y == 0f)
		{
			return false;
		}
		targetPos = vector;
		fleeTicks = 80;
		pathTicks = 0;
		checkedPath = false;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool FindRandomPos()
	{
		Vector3 vector = RandomPositionGenerator.CalcAround(theEntity, fleeDistance, 0);
		if (vector.y == 0f)
		{
			fleeTicks = 0;
			return false;
		}
		targetPos = vector;
		fleeTicks = 80;
		pathTicks = 0;
		checkedPath = false;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract Vector3 GetFleeFromPos();

	public override string ToString()
	{
		return $"{base.ToString()}, flee {fleeTicks}, timeout {timeoutTicks}";
	}
}
