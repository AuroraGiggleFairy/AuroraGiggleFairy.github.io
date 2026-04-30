using System.Collections;
using System.Collections.Generic;

public sealed class ListDebugWrapper<T> : CollectionDebugWrapper<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IList<T> m_list;

	public T this[int index]
	{
		get
		{
			using (DebugReadScope())
			{
				return m_list[index];
			}
		}
		set
		{
			using (DebugReadWriteScope())
			{
				m_list[index] = value;
			}
		}
	}

	public ListDebugWrapper()
		: this((IList<T>)new List<T>())
	{
	}

	public ListDebugWrapper(IList<T> list)
		: this((DebugWrapper)null, list)
	{
	}

	public ListDebugWrapper(DebugWrapper parent, IList<T> list)
		: base(parent, (ICollection<T>)list)
	{
		m_list = list;
	}

	public int IndexOf(T item)
	{
		using (DebugReadScope())
		{
			return m_list.IndexOf(item);
		}
	}

	public void Insert(int index, T item)
	{
		using (DebugReadWriteScope())
		{
			m_list.Insert(index, item);
		}
	}

	public void RemoveAt(int index)
	{
		using (DebugReadWriteScope())
		{
			m_list.RemoveAt(index);
		}
	}
}
