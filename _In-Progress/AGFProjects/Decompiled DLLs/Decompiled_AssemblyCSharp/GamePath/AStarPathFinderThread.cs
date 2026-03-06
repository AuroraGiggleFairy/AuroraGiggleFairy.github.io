using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GamePath;

public class AStarPathFinderThread : PathFinderThread
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo threadInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public AutoResetEvent writerThreadWaitHandle = new AutoResetEvent(initialState: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetList<int> entityWaitQueue = new HashSetList<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, PathInfo> finishedPaths = new Dictionary<int, PathInfo>();

	public AStarPathFinderThread()
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
		threadInfo = ThreadManager.StartThread("Pathfinder", null, thread_Pathfinder, null);
	}

	public override void Cleanup()
	{
		threadInfo.RequestTermination();
		writerThreadWaitHandle.Set();
		threadInfo.WaitForEnd();
		threadInfo = null;
		entityWaitQueue.Clear();
		finishedPaths.Clear();
		writerThreadWaitHandle = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int thread_Pathfinder(ThreadManager.ThreadInfo _threadInfo)
	{
		while (!_threadInfo.TerminationRequested())
		{
			try
			{
				if (entityWaitQueue.list.Count == 0)
				{
					writerThreadWaitHandle.WaitOne();
				}
				PathInfo pathInfo = PathInfo.Empty;
				lock (finishedPaths)
				{
					if (entityWaitQueue.list.Count > 0)
					{
						int num = entityWaitQueue.list[0];
						entityWaitQueue.Remove(num);
						if (finishedPaths.ContainsKey(num))
						{
							pathInfo = finishedPaths[num];
							goto IL_009f;
						}
					}
				}
				goto end_IL_0006;
				IL_009f:
				pathInfo.entity.navigator.GetPathTo(pathInfo);
				lock (finishedPaths)
				{
					if (pathInfo.path == null)
					{
						finishedPaths.Remove(pathInfo.entity.entityId);
					}
					else
					{
						finishedPaths[pathInfo.entity.entityId] = pathInfo;
					}
				}
				end_IL_0006:;
			}
			catch (Exception ex)
			{
				Log.Error("Exception in PathFinder thread: " + ex.Message);
				Log.Error(ex.StackTrace);
			}
		}
		return -1;
	}

	public override bool IsCalculatingPath(int _entityId)
	{
		lock (finishedPaths)
		{
			return finishedPaths.ContainsKey(_entityId);
		}
	}

	public override void FindPath(EntityAlive _entity, Vector3 _target, float _speed, bool _canBreak, EAIBase _aiTask)
	{
		lock (finishedPaths)
		{
			if (!entityWaitQueue.hashSet.Contains(_entity.entityId))
			{
				entityWaitQueue.Add(_entity.entityId);
			}
			finishedPaths[_entity.entityId] = new PathInfo(_entity, _target, _canBreak, _speed, _aiTask);
		}
		writerThreadWaitHandle.Set();
	}

	public override PathInfo GetPath(int _entityId)
	{
		lock (finishedPaths)
		{
			if (finishedPaths.TryGetValue(_entityId, out var value) && value.path != null)
			{
				finishedPaths.Remove(_entityId);
				return value;
			}
		}
		return PathInfo.Empty;
	}

	public override void RemovePathsFor(int _entityId)
	{
		lock (finishedPaths)
		{
			finishedPaths.Remove(_entityId);
			if (entityWaitQueue.hashSet.Contains(_entityId))
			{
				entityWaitQueue.Remove(_entityId);
			}
		}
	}
}
