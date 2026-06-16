using System.Collections.Generic;

public class DictionaryKeyList<T, S>
{
	public Dictionary<T, S> dict = new Dictionary<T, S>();

	public List<T> list = new List<T>();

	public void Add(T _key, S _value)
	{
		dict.Add(_key, _value);
		list.Add(_key);
	}

	public void Remove(T _key)
	{
		list.Remove(_key);
		dict.Remove(_key);
	}

	public void Replace(T _key, S _value)
	{
		if (dict.ContainsKey(_key))
		{
			Remove(_key);
		}
		Add(_key, _value);
	}

	public void Clear()
	{
		list.Clear();
		dict.Clear();
	}
}
