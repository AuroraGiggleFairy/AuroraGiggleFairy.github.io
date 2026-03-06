using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePath;

public class ASPPathFinderThread : PathFinderThread
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine coroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetList<int> entityWaitQueue = new HashSetList<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, PathInfo> finishedPaths = new Dictionary<int, PathInfo>();

	public ASPPathFinderThread()
	{
		PathFinderThread.Instance = this;
	}

	public override int GetFinishedCount()
	{
		return finishedPaths.Count;
	}

	public override int GetQueueCount()
	{
		return entityWaitQueue.list.Count;
	}

	public override void StartWorkerThreads()
	{
		coroutine = GameManager.Instance.StartCoroutine(FindPaths());
	}

	public override void Cleanup()
	{
		GameManager.Instance.StopCoroutine(coroutine);
		entityWaitQueue.Clear();
		finishedPaths.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator FindPaths()
	{
		while (true)
		{
			for (int i = 0; i < 8; i++)
			{
				if (entityWaitQueue.list.Count == 0)
				{
					break;
				}
				int num = entityWaitQueue.list[0];
				entityWaitQueue.Remove(num);
				if (!finishedPaths.TryGetValue(num, out var value))
				{
					Log.Warning("{0} path dup id {1}", GameManager.frameCount, num);
					continue;
				}
				value.entity.navigator.GetPathTo(value);
				if (value.state == PathInfo.State.Queued)
				{
					finishedPaths.Remove(num);
				}
			}
			yield return null;
		}
	}

	public override void FindPath(EntityAlive _entity, Vector3 _targetPos, float _speed, bool _canBreak, EAIBase _aiTask)
	{
		entityWaitQueue.Add(_entity.entityId);
		finishedPaths[_entity.entityId] = new PathInfo(_entity, _targetPos, _canBreak, _speed, _aiTask);
	}

	public override void FindPath(EntityAlive _entity, Vector3 _startPos, Vector3 _targetPos, float _speed, bool _canBreak, EAIBase _aiTask)
	{
		entityWaitQueue.Add(_entity.entityId);
		PathInfo pathInfo = new PathInfo(_entity, _targetPos, _canBreak, _speed, _aiTask);
		pathInfo.SetStartPos(_startPos);
		finishedPaths[_entity.entityId] = pathInfo;
	}

	public override PathInfo GetPath(int _entityId)
	{
		if (finishedPaths.TryGetValue(_entityId, out var value) && value.state == PathInfo.State.Done)
		{
			finishedPaths.Remove(_entityId);
			return value;
		}
		return PathInfo.Empty;
	}

	public override bool IsCalculatingPath(int _entityId)
	{
		return finishedPaths.ContainsKey(_entityId);
	}

	public override void RemovePathsFor(int _entityId)
	{
		finishedPaths.Remove(_entityId);
		entityWaitQueue.Remove(_entityId);
	}
}
