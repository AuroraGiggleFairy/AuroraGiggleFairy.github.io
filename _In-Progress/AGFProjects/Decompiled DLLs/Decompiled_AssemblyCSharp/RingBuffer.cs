public class RingBuffer<T>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public T[] data;

	[PublicizedFrom(EAccessModifier.Private)]
	public int idx;

	[PublicizedFrom(EAccessModifier.Private)]
	public int count;

	[PublicizedFrom(EAccessModifier.Private)]
	public int readIdx;

	public int Count => count;

	public RingBuffer(int _count)
	{
		data = new T[_count];
	}

	public void Add(T _el)
	{
		data[idx++] = _el;
		if (idx >= data.Length)
		{
			idx = 0;
		}
		count++;
		if (count > data.Length)
		{
			count = data.Length;
		}
	}

	public void Clear()
	{
		count = 0;
		idx = 0;
	}

	public void SetToLast()
	{
		readIdx = idx - 1;
		if (readIdx < 0)
		{
			readIdx = data.Length - 1;
		}
	}

	public T Peek()
	{
		return data[readIdx];
	}

	public T GetPrev()
	{
		T result = data[readIdx--];
		if (readIdx < 0)
		{
			readIdx = data.Length - 1;
		}
		return result;
	}

	public T GetNext()
	{
		T result = data[readIdx++];
		if (readIdx >= data.Length)
		{
			readIdx = 0;
		}
		return result;
	}
}
