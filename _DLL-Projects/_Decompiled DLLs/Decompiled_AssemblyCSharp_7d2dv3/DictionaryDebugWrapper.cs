using System.Collections;
using System.Collections.Generic;

public sealed class DictionaryDebugWrapper<TKey, TValue> : CollectionDebugWrapper<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IDictionary<TKey, TValue> m_dictionary;

	public TValue this[TKey key]
	{
		get
		{
			using (DebugReadScope())
			{
				return m_dictionary[key];
			}
		}
		set
		{
			using (DebugReadWriteScope())
			{
				m_dictionary[key] = value;
			}
		}
	}

	public ICollection<TKey> Keys { get; }

	public ICollection<TValue> Values { get; }

	public DictionaryDebugWrapper()
		: this((IDictionary<TKey, TValue>)new Dictionary<TKey, TValue>())
	{
	}

	public DictionaryDebugWrapper(IDictionary<TKey, TValue> dictionary)
		: this((DebugWrapper)null, dictionary)
	{
	}

	public DictionaryDebugWrapper(DebugWrapper parent, IDictionary<TKey, TValue> dictionary)
		: base(parent, (ICollection<KeyValuePair<TKey, TValue>>)dictionary)
	{
		m_dictionary = dictionary;
		Keys = new CollectionDebugWrapper<TKey>(this, m_dictionary.Keys);
		Values = new CollectionDebugWrapper<TValue>(this, m_dictionary.Values);
	}

	public void Add(TKey key, TValue value)
	{
		using (DebugReadWriteScope())
		{
			m_dictionary.Add(key, value);
		}
	}

	public bool ContainsKey(TKey key)
	{
		using (DebugReadScope())
		{
			return m_dictionary.ContainsKey(key);
		}
	}

	public bool Remove(TKey key)
	{
		using (DebugReadWriteScope())
		{
			return m_dictionary.Remove(key);
		}
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		using (DebugReadScope())
		{
			return m_dictionary.TryGetValue(key, out value);
		}
	}
}
