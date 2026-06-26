using UnityEngine;

namespace GamePath;

public class PathFinder
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public PathInfo pathInfo;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool canClimbWalls;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool canClimbLadders;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool canEntityDrown;

	public PathFinder(PathInfo _pathInfo, bool _bDrn, bool _canClimbLadders, bool _bCanClimbWalls)
	{
		pathInfo = _pathInfo;
		canEntityDrown = _bDrn;
		canClimbWalls = _bCanClimbWalls;
		canClimbLadders = _canClimbLadders;
	}

	public virtual void Calculate(Vector3 _fromPos, Vector3 _toPos)
	{
	}

	public virtual void Destruct()
	{
	}
}
