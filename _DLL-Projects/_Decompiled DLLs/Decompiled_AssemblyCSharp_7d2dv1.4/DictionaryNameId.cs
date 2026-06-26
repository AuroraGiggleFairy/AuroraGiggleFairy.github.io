using System.Collections.Generic;

public class DictionaryNameId<T>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly DictionaryNameIdMapping mapping;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<int, T> idsToValues = new Dictionary<int, T>();

	public int Count => idsToValues.Count;

	public Dictionary<int, T> Dict => idsToValues;

	public DictionaryNameId(DictionaryNameIdMapping _mapping)
	{
		mapping = _mapping;
	}

	public void Add(string _name, T _value)
	{
		int key = mapping.Add(_name);
		idsToValues[key] = _value;
	}

	public bool Contains(string _name)
	{
		int num = mapping.FindId(_name);
		if (num == 0)
		{
			return false;
		}
		return idsToValues.ContainsKey(num);
	}

	public T Get(int _id)
	{
		idsToValues.TryGetValue(_id, out var value);
		return value;
	}

	public T Get(string _name)
	{
		int num = mapping.FindId(_name);
		if (num == 0)
		{
			return default(T);
		}
		idsToValues.TryGetValue(num, out var value);
		return value;
	}
}
