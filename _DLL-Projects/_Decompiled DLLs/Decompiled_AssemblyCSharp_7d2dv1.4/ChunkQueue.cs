using ConcurrentCollections;

public class ChunkQueue
{
	[PublicizedFrom(EAccessModifier.Private)]
	public object _lock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public ConcurrentHashSet<long> KeyQueue = new ConcurrentHashSet<long>();

	public void Add(long item)
	{
		lock (_lock)
		{
			KeyQueue.Add(item);
		}
	}

	public void Clear()
	{
		KeyQueue.Clear();
	}

	public bool Contains(long item)
	{
		return KeyQueue.Contains(item);
	}

	public void Remove(long item)
	{
		KeyQueue.TryRemove(item);
	}
}
