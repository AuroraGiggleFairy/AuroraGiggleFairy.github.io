using UnityEngine;

namespace GamePath;

public class PathInfo
{
	public enum State
	{
		Queued,
		Pathing,
		Done
	}

	public static PathInfo Empty = new PathInfo(null, Vector3.zero, _canBreakBlocks: false, 0f, null);

	public EntityAlive entity;

	public State state;

	public bool hasStart;

	public Vector3 startPos;

	public Vector3 targetPos;

	public bool canBreakBlocks;

	public float speed;

	public EAIBase aiTask;

	public ChunkCache chunkcache;

	public PathEntity path;

	public PathInfo(EntityAlive _entity, Vector3 _targetPos, bool _canBreakBlocks, float _speed, EAIBase _aiTask)
	{
		entity = _entity;
		hasStart = false;
		targetPos = _targetPos;
		canBreakBlocks = _canBreakBlocks;
		speed = _speed;
		aiTask = _aiTask;
		path = null;
	}

	public void SetStartPos(Vector3 _startPos)
	{
		startPos = _startPos;
		hasStart = true;
	}
}
