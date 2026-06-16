using Pathfinding;
using UnityEngine;

namespace GamePath;

public class PathInfoSingleTarget : PathInfo
{
	public Vector3 targetPos;

	public PathEndingCondition endingCondition;

	public PathInfoSingleTarget(EntityAlive _entity, Vector3 _targetPos, bool _canBreakBlocks, float _speed, EAIBase _aiTask)
		: base(_entity, _canBreakBlocks, _speed, _aiTask)
	{
		entity = _entity;
		hasStart = false;
		targetPos = _targetPos;
		canBreakBlocks = _canBreakBlocks;
		speed = _speed;
		aiTask = _aiTask;
		path = null;
		calculatePartial = true;
	}
}
