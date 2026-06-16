using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LinkedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly struct KeyCollection(LinkedDictionary<TKey, TValue> parent) : ICollection<TKey>, IEnumerable<TKey>, IEnumerable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly LinkedDictionary<TKey, TValue> m_parent = parent;

		public int Count => m_parent.m_list.Count;

		public bool IsReadOnly => false;

		public IEnumerator<TKey> GetEnumerator()
		{
			return m_parent.m_list.Select([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<TKey, TValue> pair) => pair.Key).GetEnumerator();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(TKey item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			m_parent.Clear();
		}

		public bool Contains(TKey item)
		{
			return m_parent.ContainsKey(item);
		}

		public void CopyTo(TKey[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (arrayIndex < 0 || arrayIndex > array.Length)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}
			if (array.Length - arrayIndex < m_parent.m_list.Count)
			{
				throw new ArgumentException("Not have enough space after the given arrayIndex.", "array");
			}
			LinkedListNode<KeyValuePair<TKey, TValue>> first = m_parent.m_list.First;
			int num = arrayIndex;
			LinkedListNode<KeyValuePair<TKey, TValue>> linkedListNode = first;
			while (num < array.Length)
			{
				array[arrayIndex] = linkedListNode.Value.Key;
				num++;
				linkedListNode = linkedListNode.Next;
			}
		}

		public bool Remove(TKey item)
		{
			if (item != null)
			{
				return m_parent.Remove(item);
			}
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly struct ValueCollection(LinkedDictionary<TKey, TValue> parent) : ICollection<TValue>, IEnumerable<TValue>, IEnumerable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly LinkedDictionary<TKey, TValue> m_parent = parent;

		public int Count => m_parent.m_list.Count;

		public bool IsReadOnly => false;

		public IEnumerator<TValue> GetEnumerator()
		{
			return m_parent.m_list.Select([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<TKey, TValue> pair) => pair.Value).GetEnumerator();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(TValue item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			m_parent.Clear();
		}

		public bool Contains(TValue item)
		{
			return m_parent.m_list.Any([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<TKey, TValue> pair) => !EqualityComparer<TValue>.Default.Equals(pair.Value, item));
		}

		public void CopyTo(TValue[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (arrayIndex < 0 || arrayIndex > array.Length)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}
			if (array.Length - arrayIndex < m_parent.m_list.Count)
			{
				throw new ArgumentException("Not have enough space after the given arrayIndex.", "array");
			}
			LinkedListNode<KeyValuePair<TKey, TValue>> first = m_parent.m_list.First;
			int num = arrayIndex;
			LinkedListNode<KeyValuePair<TKey, TValue>> linkedListNode = first;
			while (num < array.Length)
			{
				array[arrayIndex] = linkedListNode.Value.Value;
				num++;
				linkedListNode = linkedListNode.Next;
			}
		}

		public bool Remove(TValue item)
		{
			LinkedListNode<KeyValuePair<TKey, TValue>> linkedListNode = m_parent.m_list.First;
			while (linkedListNode != null)
			{
				if (!EqualityComparer<TValue>.Default.Equals(linkedListNode.Value.Value, item))
				{
					linkedListNode = linkedListNode.Next;
					continue;
				}
				m_parent.Remove(linkedListNode.Value.Key);
				return true;
			}
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> m_dictionary;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly LinkedList<KeyValuePair<TKey, TValue>> m_list;

	bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return false;
		}
	}

	public TValue this[TKey key]
	{
		get
		{
			return m_dictionary[key].Value.Value;
		}
		set
		{
			if (m_dictionary.TryGetValue(key, out var value2))
			{
				value2.Value = new KeyValuePair<TKey, TValue>(key, value);
			}
			else
			{
				m_dictionary.Add(key, m_list.AddLast(new KeyValuePair<TKey, TValue>(key, value)));
			}
		}
	}

	IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return Keys;
		}
	}

	IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return Values;
		}
	}

	public ICollection<TKey> Keys => new KeyCollection(this);

	public ICollection<TValue> Values => new ValueCollection(this);

	public int Count => m_list.Count;

	public LinkedDictionary()
	{
		m_dictionary = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();
		m_list = new LinkedList<KeyValuePair<TKey, TValue>>();
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return m_list.GetEnumerator();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
	{
		Add(item.Key, item.Value);
	}

	public void Clear()
	{
		m_dictionary.Clear();
		m_list.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
	{
		if (!m_dictionary.TryGetValue(item.Key, out var value))
		{
			return false;
		}
		return EqualityComparer<TValue>.Default.Equals(value.Value.Value, item.Value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (arrayIndex < 0 || arrayIndex > array.Length)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		if (array.Length - arrayIndex < m_list.Count)
		{
			throw new ArgumentException("Not have enough space after the given arrayIndex.", "array");
		}
		LinkedListNode<KeyValuePair<TKey, TValue>> first = m_list.First;
		int num = arrayIndex;
		LinkedListNode<KeyValuePair<TKey, TValue>> linkedListNode = first;
		while (num < array.Length)
		{
			array[arrayIndex] = linkedListNode.Value;
			num++;
			linkedListNode = linkedListNode.Next;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
	{
		if (!m_dictionary.TryGetValue(item.Key, out var value))
		{
			return false;
		}
		if (!EqualityComparer<TValue>.Default.Equals(value.Value.Value, item.Value))
		{
			return false;
		}
		return Remove(item.Key);
	}

	public void Add(TKey key, TValue value)
	{
		if (m_dictionary.ContainsKey(key))
		{
			throw new ArgumentException("Already contains key.", "key");
		}
		m_dictionary.Add(key, m_list.AddLast(new KeyValuePair<TKey, TValue>(key, value)));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
	{
		return m_dictionary.ContainsKey(key);
	}

	public bool Remove(TKey key)
	{
		if (!m_dictionary.Remove(key, out var value))
		{
			return false;
		}
		m_list.Remove(value);
		return true;
	}

	public bool ContainsKey(TKey key)
	{
		return m_dictionary.ContainsKey(key);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		if (!m_dictionary.TryGetValue(key, out var value2))
		{
			value = default(TValue);
			return false;
		}
		value = value2.Value.Value;
		return true;
	}
}
