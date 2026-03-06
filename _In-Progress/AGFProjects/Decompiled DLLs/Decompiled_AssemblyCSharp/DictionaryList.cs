using System.Collections.Generic;

public class DictionaryList<T, S>
{
	public Dictionary<T, S> dict;

	public List<S> list = new List<S>();

	public int Count => list.Count;

	public DictionaryList()
	{
		dict = new Dictionary<T, S>();
	}

	public DictionaryList(IEqualityComparer<T> _comparer)
	{
		dict = new Dictionary<T, S>(_comparer);
	}

	public void Add(T _key, S _value)
	{
		dict.Add(_key, _value);
		list.Add(_value);
	}

	public void Set(T _key, S _value)
	{
		if (dict.ContainsKey(_key))
		{
			Remove(_key);
		}
		Add(_key, _value);
	}

	public bool Remove(T _key)
	{
		if (dict.ContainsKey(_key))
		{
			S item = dict[_key];
			dict.Remove(_key);
			list.Remove(item);
			return true;
		}
		return false;
	}

	public void Clear()
	{
		list.Clear();
		dict.Clear();
	}
}
