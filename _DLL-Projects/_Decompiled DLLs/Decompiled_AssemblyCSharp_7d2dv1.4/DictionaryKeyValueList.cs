using System.Collections.Generic;

public class DictionaryKeyValueList<T, S>
{
	public Dictionary<T, S> dict = new Dictionary<T, S>();

	public List<S> valueList = new List<S>();

	public List<T> keyList = new List<T>();

	public void Add(T _key, S _value)
	{
		dict.Add(_key, _value);
		keyList.Add(_key);
		valueList.Add(_value);
	}

	public void Set(T _key, S _value)
	{
		if (dict.ContainsKey(_key))
		{
			Remove(_key);
		}
		Add(_key, _value);
	}

	public void Remove(T _key)
	{
		int num = keyList.IndexOf(_key);
		if (num >= 0)
		{
			keyList.RemoveAt(num);
			valueList.RemoveAt(num);
			dict.Remove(_key);
		}
	}

	public void Clear()
	{
		keyList.Clear();
		valueList.Clear();
		dict.Clear();
	}
}
