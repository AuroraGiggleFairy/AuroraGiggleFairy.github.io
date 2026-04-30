using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.Profiling;

public static class ThreadManager
{
	public class ThreadInfo
	{
		public object parameter;

		public ThreadFunctionDelegate threadDelegate;

		public ThreadFunctionDelegate threadInit;

		public ThreadFunctionLoopDelegate threadLoop;

		public ThreadFunctionEndDelegate threadEnd;

		public string name;

		public Thread thread;

		public bool isSilent;

		public readonly ManualResetEvent evRunning = new ManualResetEvent(initialState: false);

		public readonly ManualResetEvent evStopped = new ManualResetEvent(initialState: false);

		public ExitCallbackThread exitCallback;

		public object threadData;

		public void RequestTermination()
		{
			evRunning.Set();
		}

		public bool TerminationRequested()
		{
			return evRunning.WaitOne(0);
		}

		public bool HasTerminated()
		{
			return evStopped.WaitOne(0);
		}

		public void WaitForEnd(int timeout = 30)
		{
			RequestTermination();
			if (!evStopped.WaitOne(timeout * 1000))
			{
				Log.Error("Thread " + name + " did not finish within " + timeout + "s. Request trace: " + StackTraceUtility.ExtractStackTrace());
				thread?.Abort();
			}
		}
	}

	public class TaskInfo
	{
		public string name;

		public TaskFunctionDelegate taskDelegate;

		public object parameter;

		public ExitCallbackTask exitCallback;

		public ManualResetEvent evStopped;

		public TaskInfo(bool _endEvent = true)
		{
			if (_endEvent)
			{
				evStopped = new ManualResetEvent(initialState: false);
			}
		}

		public void WaitForEnd()
		{
			evStopped.WaitOne();
		}
	}

	public struct MainThreadTaskInfo
	{
		public string name;

		public MainThreadTaskFunctionDelegate taskDelegate;

		public object parameter;
	}

	public delegate int ThreadFunctionLoopDelegate(ThreadInfo _threadInfo);

	public delegate void ThreadFunctionEndDelegate(ThreadInfo _threadInfo, bool _exitForException);

	public delegate void ThreadFunctionDelegate(ThreadInfo _threadInfo);

	public delegate void TaskFunctionDelegate(TaskInfo _taskInfo);

	public delegate void MainThreadTaskFunctionDelegate(object _parameter);

	public delegate void ExitCallbackThread(ThreadInfo _ti, Exception _e);

	public delegate void ExitCallbackTask(TaskInfo _ti, Exception _e);

	public const int cEndTime = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int threadTerminationTimeout = 30;

	public static Thread MainThreadRef;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int MainThreadId;

	public static Dictionary<string, ThreadInfo> ActiveThreads;

	public static int QueuedCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object lockObjectQueuedCounter;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object lockObjectMainThreadTasks;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<MainThreadTaskInfo> mainThreadTasks;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<MainThreadTaskInfo> mainThreadTasksCopy;

	[PublicizedFrom(EAccessModifier.Private)]
	public static MonoBehaviour monoBehaviour;

	[PublicizedFrom(EAccessModifier.Private)]
	public static WaitCallback queuedTaskDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int syncCoroutineNestingLevel;

	public static bool IsInSyncCoroutine => syncCoroutineNestingLevel > 0;

	public static event Action UpdateEv;

	public static event Action LateUpdateEv;

	[PublicizedFrom(EAccessModifier.Private)]
	static ThreadManager()
	{
		ActiveThreads = new Dictionary<string, ThreadInfo>();
		lockObjectQueuedCounter = new object();
		lockObjectMainThreadTasks = new object();
		mainThreadTasks = new List<MainThreadTaskInfo>(150);
		mainThreadTasksCopy = new List<MainThreadTaskInfo>(150);
		queuedTaskDelegate = myQueuedTaskInvoke;
	}

	public static void ReleaseTaskInfo(TaskInfo _info)
	{
		if (_info?.evStopped != null)
		{
			_info.evStopped.Close();
			_info.evStopped = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static ThreadInfo startThread(string _name, ThreadFunctionDelegate _threadDelegate, ThreadFunctionDelegate _threadInit, ThreadFunctionLoopDelegate _threadLoop, ThreadFunctionEndDelegate _threadEnd, object _parameter, ExitCallbackThread _exitCallback, bool _useRealThread = false, bool _isSilent = false)
	{
		ThreadInfo threadInfo = new ThreadInfo();
		threadInfo.parameter = _parameter;
		threadInfo.threadDelegate = _threadDelegate;
		threadInfo.threadInit = _threadInit;
		threadInfo.threadLoop = _threadLoop;
		threadInfo.threadEnd = _threadEnd;
		threadInfo.exitCallback = _exitCallback;
		threadInfo.isSilent = _isSilent;
		lock (ActiveThreads)
		{
			if (ActiveThreads.ContainsKey(_name))
			{
				int num = 0;
				string text;
				do
				{
					num++;
					text = _name + num;
				}
				while (ActiveThreads.ContainsKey(text));
				_name = text;
			}
			ActiveThreads.Add(_name, threadInfo);
		}
		threadInfo.name = _name;
		if (_useRealThread)
		{
			Thread thread = new Thread(myThreadInvoke);
			thread.Name = _name;
			thread.Start(threadInfo);
			threadInfo.thread = thread;
		}
		else
		{
			ThreadPool.UnsafeQueueUserWorkItem(myThreadInvoke, threadInfo);
		}
		return threadInfo;
	}

	public static ThreadInfo StartThread(string _name, ThreadFunctionDelegate _threadDelegate, object _parameter = null, ExitCallbackThread _exitCallback = null, bool _useRealThread = false, bool _isSilent = false)
	{
		return startThread(_name, _threadDelegate, null, null, null, _parameter, _exitCallback, _useRealThread, _isSilent);
	}

	public static ThreadInfo StartThread(string _name, ThreadFunctionDelegate _threadInit, ThreadFunctionLoopDelegate _threadLoop, ThreadFunctionEndDelegate _threadEnd, object _parameter = null, ExitCallbackThread _exitCallback = null, bool _useRealThread = false, bool _isSilent = false)
	{
		return startThread(_name, null, _threadInit, _threadLoop, _threadEnd, _parameter, _exitCallback, _useRealThread, _isSilent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void myThreadInvoke(object _threadInfo)
	{
		ThreadInfo threadInfo = (ThreadInfo)_threadInfo;
		CustomSampler.Create("ThreadDelegate");
		if (!threadInfo.isSilent)
		{
			Log.Out("Started thread " + threadInfo.name);
		}
		Exception e = null;
		try
		{
			if (threadInfo.threadDelegate != null)
			{
				threadInfo.threadDelegate(threadInfo);
			}
			else
			{
				if (threadInfo.threadInit != null)
				{
					threadInfo.threadInit(threadInfo);
				}
				bool exitForException = false;
				try
				{
					int num;
					do
					{
						num = threadInfo.threadLoop(threadInfo);
						if (num > 0)
						{
							Thread.Sleep(num);
						}
					}
					while (num >= 0);
				}
				catch (Exception ex)
				{
					Log.Error("Exception in thread {0}:", threadInfo.name);
					Log.Exception(ex);
					e = ex;
					exitForException = true;
				}
				if (threadInfo.threadEnd != null)
				{
					threadInfo.threadEnd(threadInfo, exitForException);
				}
			}
		}
		catch (Exception ex2)
		{
			Log.Error("Exception in thread {0}:", threadInfo.name);
			Log.Exception(ex2);
			e = ex2;
		}
		finally
		{
			if (!threadInfo.isSilent)
			{
				Log.Out("Exited thread " + threadInfo.name);
			}
			lock (ActiveThreads)
			{
				ActiveThreads.Remove(threadInfo.name);
			}
			threadInfo.evStopped.Set();
		}
		if (threadInfo.exitCallback != null)
		{
			threadInfo.exitCallback(threadInfo, e);
		}
		Profiler.EndThreadProfiling();
	}

	public static TaskInfo AddSingleTask(TaskFunctionDelegate _taskDelegate, object _parameter = null, ExitCallbackTask _exitCallback = null, bool _endEvent = true)
	{
		TaskInfo taskInfo = new TaskInfo(_endEvent);
		taskInfo.taskDelegate = _taskDelegate;
		taskInfo.parameter = _parameter;
		taskInfo.exitCallback = _exitCallback;
		taskInfo.name = _taskDelegate.Method.Name;
		ThreadPool.UnsafeQueueUserWorkItem(queuedTaskDelegate, taskInfo);
		return taskInfo;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void myQueuedTaskInvoke(object _taskInfo)
	{
		TaskInfo taskInfo = (TaskInfo)_taskInfo;
		lock (lockObjectQueuedCounter)
		{
			QueuedCount++;
		}
		Exception e = null;
		try
		{
			taskInfo.taskDelegate(taskInfo);
		}
		catch (Exception ex)
		{
			Log.Error("Exception in task");
			Log.Exception(ex);
			e = ex;
		}
		finally
		{
			if (taskInfo.evStopped != null)
			{
				taskInfo.evStopped.Set();
			}
		}
		lock (lockObjectQueuedCounter)
		{
			QueuedCount--;
		}
		if (taskInfo.exitCallback != null)
		{
			taskInfo.exitCallback(taskInfo, e);
		}
		Profiler.EndThreadProfiling();
	}

	public static void AddSingleTaskMainThread(string _name, MainThreadTaskFunctionDelegate _func, object _parameter = null)
	{
		MainThreadTaskInfo item = new MainThreadTaskInfo
		{
			taskDelegate = _func,
			parameter = _parameter,
			name = _name
		};
		lock (lockObjectMainThreadTasks)
		{
			mainThreadTasks.Add(item);
		}
	}

	public static void UpdateMainThreadTasks()
	{
		ThreadManager.UpdateEv?.Invoke();
		if (mainThreadTasks.Count == 0)
		{
			return;
		}
		lock (lockObjectMainThreadTasks)
		{
			List<MainThreadTaskInfo> list = mainThreadTasks;
			mainThreadTasks = mainThreadTasksCopy;
			mainThreadTasksCopy = list;
		}
		int count = mainThreadTasksCopy.Count;
		for (int i = 0; i < count; i++)
		{
			try
			{
				MainThreadTaskInfo mainThreadTaskInfo = mainThreadTasksCopy[i];
				mainThreadTaskInfo.taskDelegate(mainThreadTaskInfo.parameter);
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
		}
		mainThreadTasksCopy.Clear();
	}

	public static void LateUpdate()
	{
		ThreadManager.LateUpdateEv?.Invoke();
	}

	public static void Shutdown()
	{
		Log.Out("Terminating threads");
		foreach (KeyValuePair<string, ThreadInfo> activeThread in ActiveThreads)
		{
			activeThread.Value.RequestTermination();
		}
		while (ActiveThreads.Count > 0)
		{
			using Dictionary<string, ThreadInfo>.Enumerator enumerator = ActiveThreads.GetEnumerator();
			if (enumerator.MoveNext())
			{
				enumerator.Current.Value.WaitForEnd();
			}
		}
	}

	public static void SetMonoBehaviour(MonoBehaviour _monoBehaviour)
	{
		monoBehaviour = _monoBehaviour;
	}

	public static Coroutine StartCoroutine(IEnumerator _e)
	{
		if (IsMainThread())
		{
			if (monoBehaviour != null)
			{
				return monoBehaviour.StartCoroutine(_e);
			}
			return null;
		}
		AddSingleTaskMainThread("Coroutine", [PublicizedFrom(EAccessModifier.Internal)] (object _taskInfo) =>
		{
			StartCoroutine(_e);
		});
		return null;
	}

	public static void StopCoroutine(IEnumerator _e)
	{
		monoBehaviour.StopCoroutine(_e);
	}

	public static void StopCoroutine(Coroutine _coroutine)
	{
		monoBehaviour.StopCoroutine(_coroutine);
	}

	public static void StopCoroutine(string _methodName)
	{
		monoBehaviour.StopCoroutine(_methodName);
	}

	public static void RunCoroutine(IEnumerator _e, Action _iterCallback)
	{
		while (_e.MoveNext())
		{
			if (_e.Current is IEnumerator e)
			{
				RunCoroutine(e, _iterCallback);
			}
			else
			{
				_iterCallback();
			}
		}
	}

	public static void RunCoroutineSync(IEnumerator _func)
	{
		syncCoroutineNestingLevel++;
		try
		{
			while (_func.MoveNext())
			{
				if (_func.Current is IEnumerator func)
				{
					RunCoroutineSync(func);
				}
			}
		}
		finally
		{
			syncCoroutineNestingLevel--;
		}
	}

	public static IEnumerator CoroutineWrapperWithExceptionCallback(IEnumerator _enumerator, Action<Exception> _exceptionHandler)
	{
		Stack<IEnumerator> stack = new Stack<IEnumerator>();
		stack.Push(_enumerator);
		while (stack.Count > 0)
		{
			IEnumerator enumerator = stack.Peek();
			object current;
			try
			{
				if (!enumerator.MoveNext())
				{
					stack.Pop();
					continue;
				}
				current = enumerator.Current;
			}
			catch (Exception obj)
			{
				_exceptionHandler(obj);
				break;
			}
			if (current is IEnumerator item)
			{
				stack.Push(item);
			}
			else
			{
				yield return current;
			}
		}
	}

	public static void SetMainThreadRef(Thread _mainThreadRef)
	{
		MainThreadRef = _mainThreadRef;
		MainThreadId = _mainThreadRef.ManagedThreadId;
	}

	public static bool IsMainThread()
	{
		return Thread.CurrentThread.ManagedThreadId == MainThreadId;
	}

	[Conditional("DEBUG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DebugLog(string _messagePart1, string _messagePart2 = null)
	{
	}
}
