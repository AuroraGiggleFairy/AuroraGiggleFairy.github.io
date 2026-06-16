using Pathfinding;
using UnityEngine;

namespace GamePath;

public abstract class PathInfo
{
	public enum State
	{
		Queued,
		Pathing,
		Done
	}

	public EntityAlive entity;

	public State state;

	public Vector3 startPos;

	public bool hasStart;

	public bool canBreakBlocks;

	public float speed;

	public EAIBase aiTask;

	public bool calculatePartial;

	public ChunkCache chunkcache;

	public OnPathDelegate OnPathResult;

	public PathEntity path;

	public PathInfo(EntityAlive _entity, bool _canBreakBlocks, float _speed, EAIBase _aiTask)
	{
		entity = _entity;
		hasStart = false;
		canBreakBlocks = _canBreakBlocks;
		speed = _speed;
		aiTask = _aiTask;
		calculatePartial = false;
		path = null;
	}

	public void SetStartPos(Vector3 _startPos)
	{
		startPos = _startPos;
		hasStart = true;
	}

	public virtual void ResetPathInfo()
	{
		path = null;
	}
}
