using System.Collections;
using System.Collections.Generic;

public class CollectionDebugWrapper<T> : EnumerableDebugWrapper<T>, ICollection<T>, IEnumerable<T>, IEnumerable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ICollection<T> m_collection;

	public int Count => m_collection.Count;

	public bool IsReadOnly => m_collection.IsReadOnly;

	public CollectionDebugWrapper(ICollection<T> collection)
		: this((DebugWrapper)null, collection)
	{
	}

	public CollectionDebugWrapper(DebugWrapper parent, ICollection<T> collection)
		: base(parent, (IEnumerable<T>)collection)
	{
		m_collection = collection;
	}

	public void Add(T item)
	{
		using (DebugReadWriteScope())
		{
			m_collection.Add(item);
		}
	}

	public void Clear()
	{
		using (DebugReadWriteScope())
		{
			m_collection.Clear();
		}
	}

	public bool Contains(T item)
	{
		using (DebugReadScope())
		{
			return m_collection.Contains(item);
		}
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		using (DebugReadScope())
		{
			m_collection.CopyTo(array, arrayIndex);
		}
	}

	public bool Remove(T item)
	{
		using (DebugReadWriteScope())
		{
			return m_collection.Remove(item);
		}
	}
}
