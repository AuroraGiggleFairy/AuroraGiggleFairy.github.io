using Pathfinding;
using UnityEngine;

namespace GamePath;

[PublicizedFrom(EAccessModifier.Internal)]
public class ASPPathFinder : PathFinder
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cCollisionMask = 1082195968;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entity;

	[PublicizedFrom(EAccessModifier.Private)]
	public float smoothPercent;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 moveToPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 moveToLocalPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public ABPath path;

	public ASPPathFinder(PathInfo _pathInfo, bool _bDrn, bool _canClimbLadders, bool _bCanClimbWalls)
		: base(_pathInfo, _bDrn, _canClimbLadders, _bCanClimbWalls)
	{
		entity = _pathInfo.entity;
		smoothPercent = 0.82f + entity.rand.RandomFloat * 0.07f;
	}

	public override void Calculate(Vector3 _fromPos, Vector3 _toPos)
	{
		if (AstarPath.active == null)
		{
			return;
		}
		Vector3 start = _fromPos - Origin.position;
		moveToPos = _toPos;
		moveToLocalPos = _toPos - Origin.position;
		path = ABPath.Construct(start, moveToLocalPos, OnPathFinished);
		path.calculatePartial = true;
		path.enabledTags = ((!pathInfo.canBreakBlocks) ? 257 : 267);
		if (pathInfo.entity.bCanClimbLadders)
		{
			path.enabledTags |= 16;
		}
		if (pathInfo.entity.height <= 1f)
		{
			path.enabledTags |= 4;
		}
		if (entity is EntityDrone && !pathInfo.canBreakBlocks)
		{
			path.enabledTags |= 8;
		}
		float pathCostScale = entity.aiManager.pathCostScale;
		if (pathCostScale <= 99f)
		{
			float partialPathHeightScale = entity.aiManager.partialPathHeightScale;
			path.traversalProvider = new TraversalProvider();
			path.CostScale = pathCostScale;
			path.PartialPathHeightScale = partialPathHeightScale * 0.3f;
			if (pathCostScale >= 0.28f)
			{
				partialPathHeightScale -= entity.rand.RandomFloat * 0.02f * pathCostScale;
				if (partialPathHeightScale < 0f)
				{
					partialPathHeightScale = 0f;
				}
				entity.aiManager.partialPathHeightScale = partialPathHeightScale;
			}
		}
		else
		{
			path.traversalProvider = new TraversalProviderNoBreak();
			path.CostScale = 1f;
			path.PartialPathHeightScale = 0f;
		}
		AstarPath.StartPath(path);
		pathInfo.state = PathInfo.State.Pathing;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPathFinished(Path p)
	{
		pathInfo.state = PathInfo.State.Done;
		EntityAlive entityAlive = pathInfo.entity;
		if (!entityAlive || entityAlive.navigator == null || pathInfo != entityAlive.navigator.pathInfo)
		{
			return;
		}
		path = p as ABPath;
		int num = path.vectorPath.Count;
		if (num == 0)
		{
			return;
		}
		pathInfo.path = new PathEntity();
		pathInfo.path.toPos = moveToPos;
		for (int i = 0; i < num - 2; i++)
		{
			Vector3 vector = path.vectorPath[i];
			Vector3 vector2 = path.vectorPath[i + 2];
			float num2 = vector2.x - vector.x;
			float num3 = vector2.z - vector.z;
			if ((num2 < -0.1f || num2 > 0.1f) && (num3 < -0.1f || num3 > 0.1f))
			{
				Vector3 vector3 = path.vectorPath[i + 1];
				if (Mathf.Abs(vector.y - vector3.y) <= 0.5f && IsLineClear(vector, vector2, isTall: true))
				{
					path.vectorPath[i + 1] = (vector + vector2) * 0.475f + vector3 * 0.05f;
					i++;
				}
			}
		}
		Vector3 vector4 = entityAlive.position - Origin.position;
		pathInfo.path.rawEndPos = path.vectorPath[num - 1] + Origin.position;
		if (num >= 2)
		{
			path.vectorPath[0] = vector4 * 0.45f + path.vectorPath[0] * 0.55f;
		}
		if (path.CompleteState == PathCompleteState.Complete)
		{
			if (!pathInfo.canBreakBlocks)
			{
				moveToLocalPos.y = path.vectorPath[num - 1].y;
			}
			path.vectorPath[num - 1] = moveToLocalPos;
		}
		else if (path.CompleteState == PathCompleteState.Partial && num == 1)
		{
			path.vectorPath[0] = vector4 * 0.3f + path.vectorPath[0] * 0.7f;
			Vector3 item = Vector3.MoveTowards(path.vectorPath[0], moveToLocalPos, 5f);
			path.vectorPath.Add(item);
			num++;
		}
		if (num >= 3)
		{
			float num4 = smoothPercent;
			float num5 = (1f - num4) * 0.5f;
			for (int num6 = 2; num6 > 0; num6--)
			{
				for (int j = 0; j < num - 2; j++)
				{
					Vector3 vector5 = path.vectorPath[j];
					Vector3 value = path.vectorPath[j + 1];
					if (Mathf.Abs(vector5.y - value.y) <= 0.5f)
					{
						Vector3 vector6 = path.vectorPath[j + 2];
						value.x = value.x * num4 + (vector5.x + vector6.x) * num5;
						value.z = value.z * num4 + (vector5.z + vector6.z) * num5;
						path.vectorPath[j + 1] = value;
					}
				}
			}
		}
		PathPoint[] array = new PathPoint[num];
		for (int k = 0; k < num; k++)
		{
			PathPoint pathPoint = PathPoint.Allocate(path.vectorPath[k] + Origin.position);
			array[k] = pathPoint;
		}
		pathInfo.path.SetPoints(array);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsLineClear(Vector3 pos1, Vector3 pos2, bool isTall)
	{
		if (Mathf.Abs(pos1.y - pos2.y) > 0.5f)
		{
			return false;
		}
		pos1.y += 0.5f;
		pos2.y += 0.5f;
		if (Physics.Linecast(pos1, pos2, 1082195968))
		{
			return false;
		}
		Vector3 direction = pos2 - pos1;
		Ray ray = new Ray(pos1, direction);
		if (Physics.SphereCast(ray, 0.25f, direction.magnitude, 1082195968))
		{
			return false;
		}
		if (isTall)
		{
			pos1.y += 1f;
			pos2.y += 1f;
			if (Physics.Linecast(pos1, pos2, 1082195968))
			{
				return false;
			}
			ray.origin = pos1;
			if (Physics.SphereCast(ray, 0.25f, direction.magnitude, 1082195968))
			{
				return false;
			}
		}
		return true;
	}

	public void Cancel()
	{
		if (path != null)
		{
			path.Error();
		}
	}
}
