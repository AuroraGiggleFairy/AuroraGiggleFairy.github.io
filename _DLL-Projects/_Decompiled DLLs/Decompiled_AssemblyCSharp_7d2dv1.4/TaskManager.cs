using System;
using System.Threading;

public class TaskManager
{
	public class TaskGroup
	{
		[PublicizedFrom(EAccessModifier.Internal)]
		public TaskGroup parent;

		[PublicizedFrom(EAccessModifier.Internal)]
		public int pending;

		public bool Pending => Interlocked.CompareExchange(ref pending, 0, 0) != 0;

		[PublicizedFrom(EAccessModifier.Internal)]
		public TaskGroup(TaskGroup _parent)
		{
			parent = _parent;
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public class Task
	{
		[PublicizedFrom(EAccessModifier.Internal)]
		public TaskGroup Group;

		[PublicizedFrom(EAccessModifier.Internal)]
		public Action Execute;

		[PublicizedFrom(EAccessModifier.Internal)]
		public Action Complete;

		[PublicizedFrom(EAccessModifier.Internal)]
		public Task(TaskGroup _group, Action _execute, Action _complete)
		{
			Group = _group;
			Execute = _execute;
			Complete = _complete;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TaskGroup rootGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public static WorkBatch<Task> tasks;

	public static bool Pending => rootGroup.Pending;

	public static void Init()
	{
		rootGroup = new TaskGroup(null);
		tasks = new WorkBatch<Task>();
	}

	public static void Destroy()
	{
		WaitOnGroup(rootGroup);
	}

	public static void Update()
	{
		tasks.DoWork(CompleteTask);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CompleteTask(Task _task)
	{
		if (_task.Complete != null)
		{
			_task.Complete();
			OnTaskCompleted(_task);
		}
	}

	public static TaskGroup CreateGroup()
	{
		return new TaskGroup(rootGroup);
	}

	public static TaskGroup CreateGroup(TaskGroup _parent)
	{
		return new TaskGroup(_parent);
	}

	public static void Schedule(Action _execute, Action _complete)
	{
		Task task = new Task(rootGroup, _execute, _complete);
		OnTaskCreated(task);
		ThreadManager.AddSingleTask(Execute, task, null, _endEvent: false);
	}

	public static void Schedule(TaskGroup _group, Action _execute, Action _complete)
	{
		Task task = new Task(_group, _execute, _complete);
		OnTaskCreated(task);
		ThreadManager.AddSingleTask(Execute, task, null, _endEvent: false);
	}

	public static void WaitOnGroup(TaskGroup _group)
	{
		if (!ThreadManager.MainThreadRef.Equals(Thread.CurrentThread))
		{
			throw new Exception("TaskManager.WaitOnGroup should only be called from the main thread.");
		}
		Update();
		while (_group.Pending)
		{
			Thread.Sleep(1);
			Update();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Execute(ThreadManager.TaskInfo _info)
	{
		Task task = _info.parameter as Task;
		task.Execute();
		if (task.Complete != null)
		{
			tasks.Add(task);
		}
		else
		{
			OnTaskCompleted(task);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OnTaskCreated(Task task)
	{
		for (TaskGroup taskGroup = task.Group; taskGroup != null; taskGroup = taskGroup.parent)
		{
			Interlocked.Increment(ref taskGroup.pending);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OnTaskCompleted(Task task)
	{
		for (TaskGroup taskGroup = task.Group; taskGroup != null; taskGroup = taskGroup.parent)
		{
			Interlocked.Decrement(ref taskGroup.pending);
		}
	}
}
