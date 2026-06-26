using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class EAITaskList
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<EAITaskEntry> allTasks;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EAITaskEntry> executingTasks;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EAITaskEntry> startedTasks = new List<EAITaskEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float executeDelayScale;

	public List<EAITaskEntry> Tasks => allTasks;

	public EAITaskList(EAIManager _manager)
	{
		allTasks = new List<EAITaskEntry>();
		executingTasks = new List<EAITaskEntry>();
		executeDelayScale = 0.85f + _manager.random.RandomFloat * 0.25f;
	}

	public void AddTask(int _priority, EAIBase _eai)
	{
		allTasks.Add(new EAITaskEntry(_priority, _eai));
	}

	public List<EAITaskEntry> GetExecutingTasks()
	{
		return executingTasks;
	}

	public T GetTask<T>() where T : class
	{
		for (int i = 0; i < allTasks.Count; i++)
		{
			if (allTasks[i].action is T result)
			{
				return result;
			}
		}
		return null;
	}

	public void OnUpdateTasks()
	{
		startedTasks.Clear();
		for (int i = 0; i < allTasks.Count; i++)
		{
			EAITaskEntry eAITaskEntry = allTasks[i];
			if (eAITaskEntry.isExecuting)
			{
				if (isBestTask(eAITaskEntry) && eAITaskEntry.action.Continue())
				{
					continue;
				}
				executingTasks.Remove(eAITaskEntry);
				eAITaskEntry.isExecuting = false;
				eAITaskEntry.executeTime = eAITaskEntry.action.executeDelay * executeDelayScale;
				eAITaskEntry.action.Reset();
			}
			eAITaskEntry.executeTime -= 0.05f;
			eAITaskEntry.action.executeWaitTime += 0.05f;
			if (!(eAITaskEntry.executeTime <= 0f))
			{
				continue;
			}
			eAITaskEntry.executeTime = eAITaskEntry.action.executeDelay * executeDelayScale;
			if (isBestTask(eAITaskEntry))
			{
				if (eAITaskEntry.action.CanExecute())
				{
					startedTasks.Add(eAITaskEntry);
					executingTasks.Add(eAITaskEntry);
					eAITaskEntry.isExecuting = true;
				}
				eAITaskEntry.action.executeWaitTime = 0f;
			}
		}
		for (int j = 0; j < startedTasks.Count; j++)
		{
			startedTasks[j].action.Start();
		}
		for (int k = 0; k < executingTasks.Count; k++)
		{
			executingTasks[k].action.Update();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBestTask(EAITaskEntry _task)
	{
		int num = 0;
		while (true)
		{
			if (num >= executingTasks.Count)
			{
				return true;
			}
			EAITaskEntry eAITaskEntry = executingTasks[num++];
			if (eAITaskEntry == _task)
			{
				continue;
			}
			if (eAITaskEntry.priority > _task.priority)
			{
				if (!eAITaskEntry.action.IsContinuous())
				{
					break;
				}
			}
			else if (!areTasksCompatible(_task, eAITaskEntry))
			{
				break;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool areTasksCompatible(EAITaskEntry _task, EAITaskEntry _other)
	{
		return (_task.action.MutexBits & _other.action.MutexBits) == 0;
	}
}
