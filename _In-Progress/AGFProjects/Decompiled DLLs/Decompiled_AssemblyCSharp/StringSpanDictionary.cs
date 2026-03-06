using System.Collections;
using System.Collections.Generic;

public class StringSpanDictionary<T> : IDictionary<string, T>, ICollection<KeyValuePair<string, T>>, IEnumerable<KeyValuePair<string, T>>, IEnumerable, IReadOnlyDictionary<string, T>, IReadOnlyCollection<KeyValuePair<string, T>>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IDictionary<string, T> m_dict;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<int, List<string>> m_hashToKeys;

	public T this[StringSpan key]
	{
		get
		{
			if (!TryGetStringFromHashedKeys(key, out var stringKey))
			{
				throw new KeyNotFoundException();
			}
			return this[stringKey];
		}
		set
		{
			if (!TryGetStringFromHashedKeys(key, out var stringKey))
			{
				this[key.ToString()] = value;
			}
			else
			{
				this[stringKey] = value;
			}
		}
	}

	public int Count => m_dict.Count;

	bool ICollection<KeyValuePair<string, T>>.IsReadOnly
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return m_dict.IsReadOnly;
		}
	}

	public T this[string key]
	{
		get
		{
			return m_dict[key];
		}
		set
		{
			m_dict[key] = value;
			AddHash(key);
		}
	}

	public ICollection<string> Keys => m_dict.Keys;

	public ICollection<T> Values => m_dict.Values;

	IEnumerable<string> IReadOnlyDictionary<string, T>.Keys
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return m_dict.Keys;
		}
	}

	IEnumerable<T> IReadOnlyDictionary<string, T>.Values
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return m_dict.Values;
		}
	}

	public StringSpanDictionary()
		: this((IDictionary<string, T>)new Dictionary<string, T>())
	{
	}

	public StringSpanDictionary(IDictionary<string, T> dict)
	{
		m_dict = dict;
		m_hashToKeys = new Dictionary<int, List<string>>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int GenerateHash(StringSpan key)
	{
		return key.GetHashCode();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddHash(string key)
	{
		int key2 = GenerateHash(key);
		if (!m_hashToKeys.TryGetValue(key2, out var value))
		{
			value = new List<string>();
			m_hashToKeys.Add(key2, value);
		}
		foreach (string item in value)
		{
			if (item == key)
			{
				return;
			}
		}
		value.Add(key);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveHash(string key)
	{
		int key2 = GenerateHash(key);
		if (m_hashToKeys.TryGetValue(key2, out var value))
		{
			value.Remove(key);
			if (value.Count <= 0)
			{
				m_hashToKeys.Remove(key2);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetStringFromHashedKeys(StringSpan spanKey, out string stringKey)
	{
		int key = GenerateHash(spanKey);
		if (!m_hashToKeys.TryGetValue(key, out var value))
		{
			stringKey = null;
			return false;
		}
		foreach (string item in value)
		{
			if (!(item != spanKey))
			{
				stringKey = item;
				return true;
			}
		}
		stringKey = null;
		return false;
	}

	public void Add(StringSpan key, T value)
	{
		if (!TryGetStringFromHashedKeys(key, out var stringKey))
		{
			Add(key.ToString(), value);
		}
		else
		{
			Add(stringKey, value);
		}
	}

	public bool ContainsKey(StringSpan key)
	{
		if (TryGetStringFromHashedKeys(key, out var stringKey))
		{
			return ContainsKey(stringKey);
		}
		return false;
	}

	public bool Remove(StringSpan key)
	{
		if (TryGetStringFromHashedKeys(key, out var stringKey))
		{
			return Remove(stringKey);
		}
		return false;
	}

	public bool TryGetValue(StringSpan key, out T value)
	{
		if (!TryGetStringFromHashedKeys(key, out var stringKey))
		{
			value = default(T);
			return false;
		}
		return TryGetValue(stringKey, out value);
	}

	public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
	{
		return m_dict.GetEnumerator();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_dict.GetEnumerator();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	void ICollection<KeyValuePair<string, T>>.Add(KeyValuePair<string, T> item)
	{
		Add(item.Key, item.Value);
	}

	public void Clear()
	{
		m_dict.Clear();
		m_hashToKeys.Clear();
	}

	public bool Contains(KeyValuePair<string, T> item)
	{
		return m_dict.Contains(item);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	void ICollection<KeyValuePair<string, T>>.CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
	{
		m_dict.CopyTo(array, arrayIndex);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	bool ICollection<KeyValuePair<string, T>>.Remove(KeyValuePair<string, T> item)
	{
		bool result = m_dict.Remove(item);
		if (!m_dict.ContainsKey(item.Key))
		{
			RemoveHash(item.Key);
		}
		return result;
	}

	public void Add(string key, T value)
	{
		m_dict.Add(key, value);
		AddHash(key);
	}

	public bool ContainsKey(string key)
	{
		return m_dict.ContainsKey(key);
	}

	public bool Remove(string key)
	{
		bool result = m_dict.Remove(key);
		if (!m_dict.ContainsKey(key))
		{
			RemoveHash(key);
		}
		return result;
	}

	public bool TryGetValue(string key, out T value)
	{
		return m_dict.TryGetValue(key, out value);
	}
}
