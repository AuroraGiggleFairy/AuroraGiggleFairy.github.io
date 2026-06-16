using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

namespace GamePath;

public class PathInfoMultiSource : PathInfo
{
	public readonly List<Vector3> sourcePositions;

	public OnPathDelegate[] OnTargetPathFinished;

	public List<Vector3>[] vectorPaths;

	public PathInfoMultiSource(EntityAlive _entity, List<Vector3> _sourcePositions, bool _canBreakBlocks, float _speed, EAIBase _aiTask)
		: base(_entity, _canBreakBlocks, _speed, _aiTask)
	{
		sourcePositions = _sourcePositions ?? new List<Vector3>();
	}
}
