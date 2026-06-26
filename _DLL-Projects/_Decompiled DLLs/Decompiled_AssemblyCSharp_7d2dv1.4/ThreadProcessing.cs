using System.Collections.Generic;

public class ThreadProcessing
{
	public bool IsFinished;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsCancelled;

	public ThreadManager.TaskInfo TaskInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public static object LockObjectThread = new object();

	public List<ThreadInfoParam> JobList;

	public ThreadProcessing(List<ThreadInfoParam> _JobList)
	{
		JobList = new List<ThreadInfoParam>(100);
		Init(_JobList);
	}

	public ThreadProcessing()
	{
		JobList = new List<ThreadInfoParam>(100);
		IsCancelled = false;
		IsFinished = false;
	}

	public void Init(List<ThreadInfoParam> _JobList)
	{
		RemoveTreatedElement(_JobList);
		if (JobList.Count == 0)
		{
			IsFinished = true;
			return;
		}
		IsCancelled = false;
		IsFinished = false;
		for (int i = 0; i < JobList.Count; i++)
		{
			for (int j = 0; j < JobList[i].LengthThreadContList; j++)
			{
				DistantChunk dChunk = JobList[i].ThreadContListA[j].DChunk;
				dChunk.CellMeshData = DistantChunk.SMPool.GetObject(dChunk.BaseChunkMap, dChunk.ResLevel);
			}
		}
		TaskInfo = ThreadManager.AddSingleTask(ThreadJob, JobList);
	}

	public void RemoveTreatedElement(List<ThreadInfoParam> _JobList)
	{
		JobList.Clear();
		for (int i = 0; i < _JobList.Count; i++)
		{
			if (!_JobList[i].IsThreadDone)
			{
				JobList.Add(_JobList[i]);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ThreadJob(ThreadManager.TaskInfo _InfoJob)
	{
		List<ThreadInfoParam> list = (List<ThreadInfoParam>)_InfoJob.parameter;
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = 0; j < list[i].LengthThreadContList; j++)
			{
				lock (LockObjectThread)
				{
					if (IsCancelled)
					{
						break;
					}
					list[i].ThreadContListA[j].ThreadExtraWork();
					continue;
				}
			}
			lock (LockObjectThread)
			{
				if (IsCancelled)
				{
					break;
				}
				list[i].IsThreadDone = true;
				continue;
			}
		}
		lock (LockObjectThread)
		{
			IsFinished = true;
		}
	}

	public void CancelThread()
	{
		lock (LockObjectThread)
		{
			IsCancelled = true;
		}
	}

	public long CancelThreadAndWaitFinished()
	{
		lock (LockObjectThread)
		{
			IsCancelled = true;
		}
		if (TaskInfo != null)
		{
			TaskInfo.WaitForEnd();
		}
		return 0L;
	}

	public bool IsThreadFinished()
	{
		return IsFinished;
	}

	public bool IsThreadDone(int ThreadInfoParamId)
	{
		lock (LockObjectThread)
		{
			return JobList[ThreadInfoParamId].IsThreadDone;
		}
	}
}
