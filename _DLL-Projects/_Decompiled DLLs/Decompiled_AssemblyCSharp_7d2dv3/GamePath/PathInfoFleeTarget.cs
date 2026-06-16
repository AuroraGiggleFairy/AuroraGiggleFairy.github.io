using UnityEngine;

namespace GamePath;

public class PathInfoFleeTarget : PathInfoFleeRandom
{
	public Vector3 fleeTarget;

	public PathInfoFleeTarget(EntityAlive _entity, Vector3 _fleeTarget, int _searchLength, Vector3 _aimBias, float _aimStrength, float _speed, EAIBase _aiTask)
		: base(_entity, _searchLength, _aimBias, _aimStrength, _speed, _aiTask)
	{
		fleeTarget = _fleeTarget;
	}
}
