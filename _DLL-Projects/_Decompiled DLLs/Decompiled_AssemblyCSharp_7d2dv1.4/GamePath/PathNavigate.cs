namespace GamePath;

public class PathNavigate
{
	public PathInfo pathInfo;

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityAlive theEntity;

	[PublicizedFrom(EAccessModifier.Protected)]
	public PathEntity currentPath;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float speed;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool canBreakBlocks;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int curNavTicks;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int prevNavTicks;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool inWater;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool canDrown;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool? canPathThroughDoors;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int canPathThroughDoorsDecisionTime;

	public PathNavigate(EntityAlive _ea)
	{
		theEntity = _ea;
		inWater = false;
		canDrown = false;
	}

	public void setMoveSpeed(float _b)
	{
		speed = _b;
	}

	public void setCanDrown(bool _b)
	{
		canDrown = _b;
	}

	public bool noPath()
	{
		if (currentPath != null)
		{
			return currentPath.isFinished();
		}
		return true;
	}

	public bool noPathAndNotPlanningOne()
	{
		if (noPath())
		{
			return !PathFinderThread.Instance.IsCalculatingPath(theEntity.entityId);
		}
		return false;
	}

	public bool HasPath()
	{
		if (currentPath != null)
		{
			return !currentPath.isFinished();
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool canNavigate()
	{
		return theEntity.CanNavigatePath();
	}

	public void clearPath()
	{
		if (currentPath != null)
		{
			currentPath.Destruct();
		}
		currentPath = null;
	}

	public PathEntity getPath()
	{
		return currentPath;
	}

	public void ShortenEnd(float _distance)
	{
		if (currentPath != null)
		{
			currentPath.ShortenEnd(_distance);
		}
	}

	public virtual void GetPathTo(PathInfo _pathInfo)
	{
	}

	public virtual void GetPathToEntity(PathInfo _pathInfo, EntityAlive _entity)
	{
	}

	public virtual bool SetPath(PathInfo _pathInfo, float _speed)
	{
		return false;
	}

	public virtual void UpdateNavigation()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void CreatePath()
	{
	}
}
