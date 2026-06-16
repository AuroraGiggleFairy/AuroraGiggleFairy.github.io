using System;
using System.Collections.Generic;

public sealed class BiDictionary<TKey, TValue>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<TKey, TValue> m_keyToValue = new Dictionary<TKey, TValue>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<TValue, TKey> m_valueToKey = new Dictionary<TValue, TKey>();

	public void Add(TKey key, TValue value)
	{
		if (m_keyToValue.ContainsKey(key))
		{
			throw new ArgumentException("Key already in dictionary.", "key");
		}
		if (m_valueToKey.ContainsKey(value))
		{
			throw new ArgumentException("Value already in dictionary.", "value");
		}
		m_keyToValue.Add(key, value);
		m_valueToKey.Add(value, key);
	}

	public bool ContainsKey(TKey key)
	{
		return m_keyToValue.ContainsKey(key);
	}

	public bool ContainsValue(TValue value)
	{
		return m_valueToKey.ContainsKey(value);
	}

	public bool TryGetByKey(TKey key, out TValue value)
	{
		return m_keyToValue.TryGetValue(key, out value);
	}

	public bool TryGetByValue(TValue value, out TKey key)
	{
		return m_valueToKey.TryGetValue(value, out key);
	}

	public bool RemoveByKey(TKey key)
	{
		if (!m_keyToValue.Remove(key, out var value))
		{
			return false;
		}
		m_valueToKey.Remove(value);
		return true;
	}

	public bool RemoveByValue(TValue value)
	{
		if (!m_valueToKey.Remove(value, out var value2))
		{
			return false;
		}
		m_keyToValue.Remove(value2);
		return true;
	}
}
