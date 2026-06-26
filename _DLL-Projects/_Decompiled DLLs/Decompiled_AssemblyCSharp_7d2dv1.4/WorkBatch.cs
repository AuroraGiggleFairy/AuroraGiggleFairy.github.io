using System;
using System.Collections.Generic;

public class WorkBatch<T>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<T> queuingList;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<T> workingList;

	[PublicizedFrom(EAccessModifier.Private)]
	public object sync;

	public WorkBatch()
	{
		queuingList = new List<T>();
		workingList = new List<T>();
		sync = new object();
	}

	public int Count()
	{
		int count = workingList.Count;
		lock (sync)
		{
			return count + queuingList.Count;
		}
	}

	public void Clear()
	{
		lock (sync)
		{
			queuingList.Clear();
		}
		workingList.Clear();
	}

	public void DoWork(Action<T> _action)
	{
		FlipLists();
		workingList.ForEach(_action);
		workingList.Clear();
	}

	public void Add(T _item)
	{
		lock (sync)
		{
			queuingList.Add(_item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FlipLists()
	{
		lock (sync)
		{
			List<T> list = workingList;
			workingList = queuingList;
			queuingList = list;
		}
	}
}
