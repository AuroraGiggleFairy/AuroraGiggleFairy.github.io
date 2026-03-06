using Pathfinding;
using UnityEngine;

namespace GamePath;

[PublicizedFrom(EAccessModifier.Internal)]
public class ASPPathNavigate : PathNavigate
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ASPPathFinder pathFinder;

	public ASPPathNavigate(EntityAlive _ea)
		: base(_ea)
	{
	}

	public override void GetPathTo(PathInfo _pathInfo)
	{
		if (pathFinder != null)
		{
			pathFinder.Cancel();
			pathFinder = null;
		}
		pathInfo = _pathInfo;
		if (canNavigate())
		{
			CreatePath();
		}
	}

	public override bool SetPath(PathInfo _pathInfo, float _speed)
	{
		PathEntity pathEntity = _pathInfo?.path;
		if (pathEntity == null)
		{
			if (currentPath != null)
			{
				currentPath.Destruct();
			}
			currentPath = null;
			return false;
		}
		if (currentPath != null)
		{
			currentPath.Destruct();
		}
		currentPath = pathEntity;
		if (currentPath.getCurrentPathLength() == 0)
		{
			return true;
		}
		ImprovePath();
		speed = _speed;
		canBreakBlocks = _pathInfo.canBreakBlocks;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ImprovePath()
	{
		PathPoint[] points = currentPath.points;
		int num = points.Length;
		for (int i = 0; i < num; i++)
		{
			points[i].ProjectToGround(theEntity);
		}
		if (num >= 2)
		{
			Vector3 projectedLocation = points[0].projectedLocation;
			Vector3 projectedLocation2 = points[1].projectedLocation;
			if (projectedLocation2.y - projectedLocation.y < 0.6f)
			{
				points[0].projectedLocation = VectorMath.ClosestPointOnSegment(projectedLocation, projectedLocation2, theEntity.position);
			}
		}
	}

	public override void UpdateNavigation()
	{
		canPathThroughDoorsDecisionTime++;
		if (!noPath())
		{
			pathFollow();
			if (!noPath())
			{
				theEntity.moveHelper.SetMoveTo(currentPath, speed, canBreakBlocks);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void pathFollow()
	{
		Vector3 vector = currentPath.CurrentPoint.ProjectToGround(theEntity);
		Vector3 position = theEntity.position;
		Vector3 vector2 = VectorMath.ClosestPointOnSegment(theEntity.prevPos, position, vector);
		Vector3 vector3 = vector - vector2;
		float num = Utils.FastAbs(vector3.y);
		vector3.y = 0f;
		float v = theEntity.radius * 0.6f;
		float v2 = 0.15f;
		float num2 = 2f;
		int currentPathIndex = currentPath.getCurrentPathIndex();
		int currentPathLength = currentPath.getCurrentPathLength();
		if (currentPathIndex + 1 < currentPathLength)
		{
			v2 = ((theEntity.moveHelper.SideStepAngle != 0f) ? 0.49f : 0.33f);
		}
		if (theEntity.isSwimming)
		{
			v2 = 0.9f;
			num2 = 0.7f;
		}
		if (theEntity.IsInElevator())
		{
			num2 = 0.2f;
		}
		v = Utils.FastMax(v2, v);
		bool flag = false;
		PathPoint nextPoint = currentPath.NextPoint;
		if (nextPoint != null)
		{
			Vector3 vector4 = nextPoint.ProjectToGround(theEntity);
			if ((VectorMath.ClosestPointOnSegment(vector, vector4, position) - position).sqrMagnitude < 0.040000003f)
			{
				flag = true;
			}
			if (vector.y - vector4.y > 2f && new Plane(vector4 - vector, vector).SameSide(position, vector4))
			{
				flag = true;
			}
		}
		if (flag || (vector3.sqrMagnitude <= v * v && num <= num2))
		{
			if (currentPathIndex + 1 < currentPathLength)
			{
				currentPath.setCurrentPathIndex(currentPathIndex + 1, theEntity, position);
			}
			else
			{
				currentPath.setCurrentPathIndex(currentPathLength, theEntity, position);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreatePath()
	{
		EntityAlive entityAlive = theEntity;
		pathFinder = new ASPPathFinder(pathInfo, canDrown, entityAlive.bCanClimbLadders, entityAlive.bCanClimbVertical);
		if (pathInfo.hasStart)
		{
			pathFinder.Calculate(pathInfo.startPos, pathInfo.targetPos);
			return;
		}
		Vector3 fromPos = entityAlive.position + entityAlive.motion * 2.5f;
		pathFinder.Calculate(fromPos, pathInfo.targetPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreatePathToEntity(EntityAlive _fromEntity, EntityAlive _toEntity)
	{
		pathFinder = new ASPPathFinder(pathInfo, canDrown, _fromEntity.bCanClimbLadders, _fromEntity.bCanClimbVertical);
		pathFinder.Calculate(_fromEntity.position, _toEntity.position);
	}
}
