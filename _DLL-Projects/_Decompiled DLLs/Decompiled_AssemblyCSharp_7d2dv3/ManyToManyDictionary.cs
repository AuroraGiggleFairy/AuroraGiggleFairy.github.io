using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public sealed class ManyToManyDictionary<TKey, TValue>
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
	public sealed class KeysView : IReadOnlyCollection<TKey>, IEnumerable<TKey>, IEnumerable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<Entry<TKey>> m_set;

		public int Count => m_set.Count;

		public KeysView(HashSet<Entry<TKey>> set)
		{
			m_set = set;
		}

		public IEnumerator<TKey> GetEnumerator()
		{
			foreach (Entry<TKey> item in m_set)
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
	public readonly Dictionary<Entry<TValue>, HashSet<Entry<TKey>>> m_valueToKeys;

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
			foreach (Entry<TValue> key in m_valueToKeys.Keys)
			{
				yield return key.Value;
			}
		}
	}

	public int CountKeys => m_keyToValues.Count;

	public int CountValues => m_valueToKeys.Count;

	public ManyToManyDictionary()
	{
		m_keyToValues = new Dictionary<Entry<TKey>, HashSet<Entry<TValue>>>();
		m_valueToKeys = new Dictionary<Entry<TValue>, HashSet<Entry<TKey>>>();
	}

	public void Clear()
	{
		m_keyToValues.Clear();
		m_valueToKeys.Clear();
	}

	public void Add(TKey key, TValue value)
	{
		Entry<TKey> entry = new Entry<TKey>(key);
		Entry<TValue> entry2 = new Entry<TValue>(value);
		if (!m_keyToValues.TryGetValue(entry, out var value2))
		{
			m_keyToValues.Add(entry, value2 = new HashSet<Entry<TValue>>());
		}
		if (!m_valueToKeys.TryGetValue(entry2, out var value3))
		{
			m_valueToKeys.Add(entry2, value3 = new HashSet<Entry<TKey>>());
		}
		value2.Add(entry2);
		value3.Add(entry);
	}

	public bool Contains(TKey key, TValue value)
	{
		if (!m_keyToValues.TryGetValue(new Entry<TKey>(key), out var value2))
		{
			return false;
		}
		return value2.Contains(new Entry<TValue>(value));
	}

	public bool ContainsKey(TKey key)
	{
		return m_keyToValues.ContainsKey(new Entry<TKey>(key));
	}

	public bool ContainsValue(TValue value)
	{
		return m_valueToKeys.ContainsKey(new Entry<TValue>(value));
	}

	public bool Remove(TKey key, TValue value)
	{
		Entry<TKey> entry = new Entry<TKey>(key);
		Entry<TValue> entry2 = new Entry<TValue>(value);
		if (!m_keyToValues.TryGetValue(entry, out var value2))
		{
			return false;
		}
		if (!m_valueToKeys.TryGetValue(entry2, out var value3))
		{
			return false;
		}
		bool num = value2.Remove(entry2);
		bool flag = value3.Remove(entry);
		if (value2.Count == 0)
		{
			m_keyToValues.Remove(entry);
		}
		if (value3.Count == 0)
		{
			m_valueToKeys.Remove(entry2);
		}
		return num && flag;
	}

	public bool RemoveByKey(TKey key)
	{
		Entry<TKey> entry = new Entry<TKey>(key);
		if (!m_keyToValues.Remove(entry, out var value))
		{
			return false;
		}
		foreach (Entry<TValue> item in value)
		{
			if (m_valueToKeys.TryGetValue(item, out var value2))
			{
				value2.Remove(entry);
				if (value2.Count == 0)
				{
					m_valueToKeys.Remove(item);
				}
			}
		}
		return true;
	}

	public bool RemoveByValue(TValue value)
	{
		Entry<TValue> entry = new Entry<TValue>(value);
		if (!m_valueToKeys.Remove(entry, out var value2))
		{
			return false;
		}
		foreach (Entry<TKey> item in value2)
		{
			if (m_keyToValues.TryGetValue(item, out var value3))
			{
				value3.Remove(entry);
				if (value3.Count == 0)
				{
					m_keyToValues.Remove(item);
				}
			}
		}
		return true;
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

	public bool TryGetByValue(TValue value, out IReadOnlyCollection<TKey> keys)
	{
		if (!m_valueToKeys.TryGetValue(new Entry<TValue>(value), out var value2))
		{
			keys = null;
			return false;
		}
		keys = new KeysView(value2);
		return true;
	}
}
