using System.Collections.Generic;
using System.Threading;

public class BlockingQueue<T>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool closing;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Queue<T> queue = new Queue<T>();

	public void Enqueue(T item)
	{
		lock (queue)
		{
			queue.Enqueue(item);
			Monitor.PulseAll(queue);
		}
	}

	public T Dequeue()
	{
		lock (queue)
		{
			while (queue.Count == 0)
			{
				if (closing)
				{
					return default(T);
				}
				Monitor.Wait(queue);
			}
			return queue.Dequeue();
		}
	}

	public bool HasData()
	{
		lock (queue)
		{
			return queue.Count > 0;
		}
	}

	public void Close()
	{
		lock (queue)
		{
			closing = true;
			Monitor.PulseAll(queue);
		}
	}

	public void Clear()
	{
		lock (queue)
		{
			queue.Clear();
		}
	}
}
