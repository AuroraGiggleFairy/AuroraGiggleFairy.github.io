using System;
using System.Collections.Generic;

public sealed class BiMultiDictionary<TKey, TValue>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<TKey, HashSet<TValue>> m_keyToValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<TValue, TKey> m_valueToKey;

	public BiMultiDictionary()
	{
		m_keyToValues = new Dictionary<TKey, HashSet<TValue>>();
		m_valueToKey = new Dictionary<TValue, TKey>();
	}

	public void Add(TKey key, TValue value)
	{
		if (m_valueToKey.ContainsKey(value))
		{
			throw new ArgumentException("Value already in dictionary.", "value");
		}
		if (!m_keyToValues.TryGetValue(key, out var value2))
		{
			m_keyToValues.Add(key, value2 = new HashSet<TValue>());
		}
		value2.Add(value);
		m_valueToKey.Add(value, key);
	}

	public bool ContainsKey(TKey key)
	{
		return m_keyToValues.ContainsKey(key);
	}

	public bool ContainsValue(TValue value)
	{
		return m_valueToKey.ContainsKey(value);
	}

	public bool TryGetByKey(TKey key, out IReadOnlyCollection<TValue> values)
	{
		if (!m_keyToValues.TryGetValue(key, out var value))
		{
			values = null;
			return false;
		}
		values = value;
		return true;
	}

	public int TryGetByKey(TKey key, ICollection<TValue> valuesOut)
	{
		if (!m_keyToValues.TryGetValue(key, out var value))
		{
			return 0;
		}
		int num = 0;
		foreach (TValue item in value)
		{
			valuesOut.Add(item);
			num++;
		}
		return num;
	}

	public int TryGetByKey(TKey key, Span<TValue> valuesOut)
	{
		if (!m_keyToValues.TryGetValue(key, out var value))
		{
			return 0;
		}
		int num = 0;
		foreach (TValue item in value)
		{
			if (num >= valuesOut.Length)
			{
				break;
			}
			valuesOut[num] = item;
			num++;
		}
		return num;
	}

	public bool TryGetByValue(TValue value, out TKey key)
	{
		return m_valueToKey.TryGetValue(value, out key);
	}

	public bool RemoveByValue(TValue value)
	{
		if (!m_valueToKey.TryGetValue(value, out var value2))
		{
			return false;
		}
		if (!m_keyToValues.TryGetValue(value2, out var value3))
		{
			return false;
		}
		m_valueToKey.Remove(value);
		value3.Remove(value);
		if (value3.Count == 0)
		{
			m_keyToValues.Remove(value2);
		}
		return true;
	}
}
