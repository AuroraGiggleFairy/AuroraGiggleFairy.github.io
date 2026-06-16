using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public sealed class OneToManyDictionary<TKey, TValue>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class Entry<T>
	{
		public readonly T Value;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int stableHash;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly bool s_isRefType = !typeof(T).IsValueType;

		public Entry(T value)
		{
			Value = value;
			stableHash = ((!s_isRefType) ? EqualityComparer<T>.Default.GetHashCode(value) : ((value != null) ? RuntimeHelpers.GetHashCode(value) : 0));
		}

		public override int GetHashCode()
		{
			return stableHash;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Entry<T> entry))
			{
				return false;
			}
			if (s_isRefType)
			{
				return (object)Value == (object)entry.Value;
			}
			return EqualityComparer<T>.Default.Equals(Value, entry.Value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class ValuesView : IReadOnlyCollection<TValue>, IEnumerable<TValue>, IEnumerable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<Entry<TValue>> m_set;

		public int Count => m_set.Count;

		public ValuesView(HashSet<Entry<TValue>> set)
		{
			m_set = set;
		}

		public IEnumerator<TValue> GetEnumerator()
		{
			foreach (Entry<TValue> item in m_set)
			{
				yield return item.Value;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Entry<TKey>, HashSet<Entry<TValue>>> m_keyToValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Entry<TValue>, Entry<TKey>> m_valueToKey;

	public IEnumerable<TKey> Keys
	{
		get
		{
			foreach (Entry<TKey> key in m_keyToValues.Keys)
			{
				yield return key.Value;
			}
		}
	}

	public IEnumerable<TValue> Values
	{
		get
		{
			foreach (Entry<TValue> key in m_valueToKey.Keys)
			{
				yield return key.Value;
			}
		}
	}

	public int CountKeys => m_keyToValues.Count;

	public int CountValues => m_valueToKey.Count;

	public OneToManyDictionary()
	{
		m_keyToValues = new Dictionary<Entry<TKey>, HashSet<Entry<TValue>>>();
		m_valueToKey = new Dictionary<Entry<TValue>, Entry<TKey>>();
	}

	public void Add(TKey key, TValue value)
	{
		Entry<TKey> entry = new Entry<TKey>(key);
		Entry<TValue> entry2 = new Entry<TValue>(value);
		if (m_valueToKey.ContainsKey(entry2))
		{
			throw new ArgumentException("Value already in dictionary.", "value");
		}
		if (!m_keyToValues.TryGetValue(entry, out var value2))
		{
			m_keyToValues.Add(entry, value2 = new HashSet<Entry<TValue>>());
		}
		value2.Add(entry2);
		m_valueToKey.Add(entry2, entry);
	}

	public bool ContainsKey(TKey key)
	{
		return m_keyToValues.ContainsKey(new Entry<TKey>(key));
	}

	public bool ContainsValue(TValue value)
	{
		return m_valueToKey.ContainsKey(new Entry<TValue>(value));
	}

	public bool TryGetByKey(TKey key, out IReadOnlyCollection<TValue> values)
	{
		if (!m_keyToValues.TryGetValue(new Entry<TKey>(key), out var value))
		{
			values = null;
			return false;
		}
		values = new ValuesView(value);
		return true;
	}

	public int TryGetByKey(TKey key, ICollection<TValue> valuesOut)
	{
		if (!m_keyToValues.TryGetValue(new Entry<TKey>(key), out var value))
		{
			return 0;
		}
		int num = 0;
		foreach (Entry<TValue> item in value)
		{
			valuesOut.Add(item.Value);
			num++;
		}
		return num;
	}

	public int TryGetByKey(TKey key, Span<TValue> valuesOut)
	{
		if (!m_keyToValues.TryGetValue(new Entry<TKey>(key), out var value))
		{
			return 0;
		}
		int num = 0;
		foreach (Entry<TValue> item in value)
		{
			if (num >= valuesOut.Length)
			{
				break;
			}
			valuesOut[num] = item.Value;
			num++;
		}
		return num;
	}

	public bool TryGetByValue(TValue value, out TKey key)
	{
		if (m_valueToKey.TryGetValue(new Entry<TValue>(value), out var value2))
		{
			key = value2.Value;
			return true;
		}
		key = default(TKey);
		return false;
	}

	public bool RemoveByKey(TKey key)
	{
		Entry<TKey> key2 = new Entry<TKey>(key);
		if (!m_keyToValues.TryGetValue(key2, out var value))
		{
			return false;
		}
		foreach (Entry<TValue> item in value)
		{
			m_valueToKey.Remove(item);
		}
		m_keyToValues.Remove(key2);
		return true;
	}

	public bool RemoveByValue(TValue value)
	{
		Entry<TValue> entry = new Entry<TValue>(value);
		if (!m_valueToKey.TryGetValue(entry, out var value2))
		{
			return false;
		}
		if (!m_keyToValues.TryGetValue(value2, out var value3))
		{
			return false;
		}
		m_valueToKey.Remove(entry);
		value3.Remove(entry);
		if (value3.Count == 0)
		{
			m_keyToValues.Remove(value2);
		}
		return true;
	}

	public void Clear()
	{
		m_keyToValues.Clear();
		m_valueToKey.Clear();
	}
}
