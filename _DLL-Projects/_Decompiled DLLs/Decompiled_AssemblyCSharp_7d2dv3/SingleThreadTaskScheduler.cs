using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEngine.Profiling;

public sealed class SingleThreadTaskScheduler : TaskScheduler, IDisposable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ProcessTasksMarker = new ProfilerMarker("SingleThreadTaskScheduler.ProcessTasks");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_WaitForTasksMarker = new ProfilerMarker("SingleThreadTaskScheduler.WaitForTasks");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_TryGetTaskMarker = new ProfilerMarker("SingleThreadTaskScheduler.TryGetTask");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ExecuteTaskMarker = new ProfilerMarker("SingleThreadTaskScheduler.ExecuteTask");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string m_threadGroupName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string m_threadName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly LinkedList<Task> m_tasks = new LinkedList<Task>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_running;

	[PublicizedFrom(EAccessModifier.Private)]
	public Thread m_taskThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public AutoResetEvent m_waitHandle = new AutoResetEvent(initialState: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public CancellationTokenSource m_taskCancellationSource;

	[PublicizedFrom(EAccessModifier.Private)]
	public TaskFactory m_taskFactory;

	public Thread Thread => m_taskThread;

	public TaskFactory Factory => m_taskFactory;

	public bool IsCurrentThread => Thread.CurrentThread == m_taskThread;

	public override int MaximumConcurrencyLevel => 1;

	public SingleThreadTaskScheduler(string threadGroupName, string threadName)
	{
		m_threadGroupName = threadGroupName;
		m_threadName = threadName;
		m_taskThread = new Thread(TaskThread)
		{
			Name = m_threadName,
			IsBackground = true
		};
		m_taskCancellationSource = new CancellationTokenSource();
		m_taskFactory = new TaskFactory(m_taskCancellationSource.Token, TaskCreationOptions.None, TaskContinuationOptions.None, this);
		m_running = true;
		m_taskThread.Start();
	}

	public void Dispose()
	{
		m_taskFactory = null;
		m_taskCancellationSource?.Cancel();
		m_taskCancellationSource = null;
		m_running = false;
		m_waitHandle?.Set();
		m_taskThread?.Interrupt();
		m_taskThread?.Join();
		m_taskThread = null;
		m_waitHandle?.Close();
		m_waitHandle = null;
	}

	public Task ExecuteNoWait(Action task)
	{
		if (!IsCurrentThread)
		{
			return m_taskFactory.StartNew(task);
		}
		task();
		return Task.CompletedTask;
	}

	public Task<T> ExecuteNoWait<T>(Func<T> task)
	{
		if (!IsCurrentThread)
		{
			return m_taskFactory.StartNew(task);
		}
		return Task.FromResult(task());
	}

	public void ExecuteAndWait(Action task)
	{
		if (IsCurrentThread)
		{
			task();
		}
		else
		{
			m_taskFactory.StartNew(task).Wait();
		}
	}

	public T ExecuteAndWait<T>(Func<T> task)
	{
		if (!IsCurrentThread)
		{
			return m_taskFactory.StartNew(task).Result;
		}
		return task();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TaskThread()
	{
		Log.Out("[" + m_threadGroupName + "] Started SingleThreadTaskScheduler Thread: " + m_threadName);
		try
		{
			while (m_running)
			{
				try
				{
					ProcessTasks();
				}
				finally
				{
				}
				try
				{
					m_waitHandle.WaitOne();
				}
				finally
				{
				}
			}
		}
		catch (ThreadInterruptedException)
		{
			Log.Out("[" + m_threadGroupName + "] Interrupted SingleThreadTaskScheduler Thread: " + m_threadName);
		}
		finally
		{
			Profiler.EndThreadProfiling();
			Log.Out("[" + m_threadGroupName + "] Stopped SingleThreadTaskScheduler Thread: " + m_threadName);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void ProcessTasks()
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
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
	{
		if (Thread.CurrentThread != m_taskThread)
		{
			return false;
		}
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
			m_waitHandle.Set();
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
