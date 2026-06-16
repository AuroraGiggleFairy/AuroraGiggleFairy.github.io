using UnityEngine;

namespace GamePath;

public class PathFinder
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public PathInfo pathInfo;

	public bool canClimbWalls;

	public bool canClimbLadders;

	public bool canEntityDrown;

	public PathFinder(PathInfo _pathInfo, bool _bDrn, bool _canClimbLadders, bool _bCanClimbWalls)
	{
		pathInfo = _pathInfo;
		canEntityDrown = _bDrn;
		canClimbWalls = _bCanClimbWalls;
		canClimbLadders = _canClimbLadders;
	}

	public virtual void Calculate(Vector3 _fromPos)
	{
	}

	public virtual void Destruct()
	{
	}
}
