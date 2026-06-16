using UnityEngine;

namespace GamePath;

public class PathEntity
{
	public PathPoint[] points;

	public Vector3 toPos;

	public Vector3 rawEndPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentPathIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int pathLength;

	public PathPoint CurrentPoint => points[currentPathIndex];

	public PathPoint NextPoint
	{
		get
		{
			int num = currentPathIndex + 1;
			if (num >= points.Length)
			{
				return null;
			}
			return points[num];
		}
	}

	public void Destruct()
	{
		PathPoint[] array = points;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Release();
		}
		points = null;
	}

	public void SetPoints(PathPoint[] _points)
	{
		points = _points;
		pathLength = _points.Length;
	}

	public bool HasPoints()
	{
		return points != null;
	}

	public bool isFinished()
	{
		return currentPathIndex >= pathLength;
	}

	public int NodeCountRemaining()
	{
		return pathLength - currentPathIndex;
	}

	public Vector3 GetEndPos()
	{
		if (pathLength > 0)
		{
			return points[pathLength - 1].projectedLocation;
		}
		return rawEndPos;
	}

	public PathPoint GetEndPoint()
	{
		if (pathLength > 0)
		{
			return points[pathLength - 1];
		}
		return null;
	}

	public void ShortenEnd(float _distance)
	{
		if (pathLength >= 2)
		{
			PathPoint pathPoint = points[pathLength - 2];
			points[pathLength - 1].projectedLocation = pathPoint.projectedLocation;
		}
	}

	public PathPoint getPathPointFromIndex(int _idx)
	{
		return points[_idx];
	}

	public int getCurrentPathLength()
	{
		return pathLength;
	}

	public void setCurrentPathLength(int _length)
	{
		pathLength = _length;
	}

	public int getCurrentPathIndex()
	{
		return currentPathIndex;
	}

	public void setCurrentPathIndex(int _idx, Entity entity, Vector3 entityPos)
	{
		currentPathIndex = _idx;
	}

	public override bool Equals(object _other)
	{
		if (!(_other is PathEntity) || _other == null)
		{
			return false;
		}
		PathEntity pathEntity = (PathEntity)_other;
		if (pathEntity.points.Length != points.Length)
		{
			return false;
		}
		for (int i = 0; i < points.Length; i++)
		{
			if (!points[i].IsSamePos(pathEntity.points[i]))
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		if (points == null)
		{
			return 0;
		}
		int num = 0;
		PathPoint[] array = points;
		foreach (PathPoint pathPoint in array)
		{
			num += pathPoint.GetHashCode();
		}
		return num;
	}
}
