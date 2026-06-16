using UnityEngine;

namespace GamePath;

public class PathInfoFleeRandom : PathInfo
{
	public int searchLength;

	public Vector3 aimBias = Vector3.zero;

	public float aimStrength;

	public PathInfoFleeRandom(EntityAlive _entity, int _searchLength, Vector3 _aimBias, float _aimStrength, float _speed, EAIBase _aiTask)
		: base(_entity, _canBreakBlocks: false, _speed, _aiTask)
	{
		searchLength = _searchLength;
		aimBias = _aimBias;
		aimStrength = _aimStrength;
	}
}
