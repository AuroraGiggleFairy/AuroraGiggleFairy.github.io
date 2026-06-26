using UnityEngine;

namespace GamePath;

public class PathFinderThread
{
	public static PathFinderThread Instance;

	public virtual int GetFinishedCount()
	{
		return 0;
	}

	public virtual int GetQueueCount()
	{
		return 0;
	}

	public virtual void StartWorkerThreads()
	{
	}

	public virtual void Cleanup()
	{
	}

	public virtual bool IsCalculatingPath(int _entityId)
	{
		return false;
	}

	public virtual void FindPath(EntityAlive _entity, Vector3 _targetPos, float _speed, bool _canBreak, EAIBase _aiTask)
	{
	}

	public virtual void FindPath(EntityAlive _entity, Vector3 _startPos, Vector3 _targetPos, float _speed, bool _canBreak, EAIBase _aiTask)
	{
	}

	public virtual PathInfo GetPath(int _entityId)
	{
		return PathInfo.Empty;
	}

	public virtual void RemovePathsFor(int _entityId)
	{
	}
}
