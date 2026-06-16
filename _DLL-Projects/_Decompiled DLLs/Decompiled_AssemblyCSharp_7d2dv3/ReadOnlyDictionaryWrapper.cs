using System.Collections;
using System.Collections.Generic;

public class ReadOnlyDictionaryWrapper<TKey, TValueIn, TValueOut> : IReadOnlyDictionary<TKey, TValueOut>, IEnumerable<KeyValuePair<TKey, TValueOut>>, IEnumerable, IReadOnlyCollection<KeyValuePair<TKey, TValueOut>> where TValueIn : TValueOut
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IReadOnlyDictionary<TKey, TValueIn> m_dict;

	public int Count => m_dict.Count;

	public TValueOut this[TKey key] => (TValueOut)(object)m_dict[key];

	public IEnumerable<TKey> Keys => m_dict.Keys;

	public IEnumerable<TValueOut> Values
	{
		get
		{
			foreach (TValueIn value in m_dict.Values)
			{
				yield return (TValueOut)(object)value;
			}
		}
	}

	public ReadOnlyDictionaryWrapper(IReadOnlyDictionary<TKey, TValueIn> dict)
	{
		m_dict = dict;
	}

	public IEnumerator<KeyValuePair<TKey, TValueOut>> GetEnumerator()
	{
		foreach (var (key, val3) in m_dict)
		{
			yield return new KeyValuePair<TKey, TValueOut>(key, (TValueOut)(object)val3);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)m_dict).GetEnumerator();
	}

	public bool ContainsKey(TKey key)
	{
		return m_dict.ContainsKey(key);
	}

	public bool TryGetValue(TKey key, out TValueOut value)
	{
		TValueIn value2;
		bool result = m_dict.TryGetValue(key, out value2);
		value = (TValueOut)(object)value2;
		return result;
	}
}
