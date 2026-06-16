using System.Collections.Generic;

public class HashSetList<T>
{
	public HashSet<T> hashSet = new HashSet<T>();

	public List<T> list = new List<T>();

	public void Add(T _value)
	{
		if (hashSet.Add(_value))
		{
			list.Add(_value);
		}
	}

	public void Remove(T _value)
	{
		if (hashSet.Remove(_value))
		{
			list.Remove(_value);
		}
	}

	public void Clear()
	{
		list.Clear();
		hashSet.Clear();
	}
}
