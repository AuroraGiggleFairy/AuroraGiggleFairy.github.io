using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Profiling;

public sealed class ManualTaskScheduler : TaskScheduler, IDisposable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ProcessTasksMarker = new ProfilerMarker("ManualTaskScheduler.ProcessTasks");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_TryGetTaskMarker = new ProfilerMarker("ManualTaskScheduler.TryGetTask");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ExecuteTaskMarker = new ProfilerMarker("ManualTaskScheduler.ExecuteTask");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly LinkedList<Task> m_tasks = new LinkedList<Task>();

	[PublicizedFrom(EAccessModifier.Private)]
	public CancellationTokenSource m_taskCancellationSource;

	[PublicizedFrom(EAccessModifier.Private)]
	public TaskFactory m_taskFactory;

	public override int MaximumConcurrencyLevel => 1;

	public TaskFactory Factory => m_taskFactory;

	public ManualTaskScheduler()
	{
		m_taskCancellationSource = new CancellationTokenSource();
		m_taskFactory = new TaskFactory(m_taskCancellationSource.Token, TaskCreationOptions.None, TaskContinuationOptions.None, this);
	}

	public void Dispose()
	{
		m_taskFactory = null;
		m_taskCancellationSource?.Cancel();
		m_taskCancellationSource = null;
	}

	public void ProcessTasks()
	{
		try
		{
			while (true)
			{
				Task value;
				try
				{
					lock (m_tasks)
					{
						if (m_tasks.Count == 0)
						{
							break;
						}
						value = m_tasks.First.Value;
						m_tasks.RemoveFirst();
					}
				}
				finally
				{
				}
				try
				{
					TryExecuteTask(value);
				}
				finally
				{
				}
			}
		}
		finally
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
	{
		if (taskWasPreviouslyQueued && !TryDequeue(task))
		{
			return false;
		}
		return TryExecuteTask(task);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void QueueTask(Task task)
	{
		lock (m_tasks)
		{
			m_tasks.AddLast(task);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool TryDequeue(Task task)
	{
		lock (m_tasks)
		{
			return m_tasks.Remove(task);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override IEnumerable<Task> GetScheduledTasks()
	{
		bool lockTaken = false;
		try
		{
			Monitor.TryEnter(m_tasks, ref lockTaken);
			if (lockTaken)
			{
				return m_tasks;
			}
			throw new NotSupportedException();
		}
		finally
		{
			if (lockTaken)
			{
				Monitor.Exit(m_tasks);
			}
		}
	}
}
