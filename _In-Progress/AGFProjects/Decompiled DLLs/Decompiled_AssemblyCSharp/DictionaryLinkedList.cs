using System.Collections.Generic;

public class DictionaryLinkedList<T, S>
{
	public Dictionary<T, S> dict;

	public LinkedList<S> list = new LinkedList<S>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<T, LinkedListNode<S>> indices = new Dictionary<T, LinkedListNode<S>>();

	public int Count => list.Count;

	public DictionaryLinkedList()
	{
		dict = new Dictionary<T, S>();
	}

	public DictionaryLinkedList(IEqualityComparer<T> _comparer)
	{
		dict = new Dictionary<T, S>(_comparer);
	}

	public void Add(T _key, S _value)
	{
		dict.Add(_key, _value);
		LinkedListNode<S> value = list.AddLast(_value);
		indices.Add(_key, value);
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
		if (dict.ContainsKey(_key))
		{
			_ = dict[_key];
			dict.Remove(_key);
			LinkedListNode<S> node = indices[_key];
			list.Remove(node);
			indices.Remove(_key);
		}
	}

	public void Clear()
	{
		list.Clear();
		dict.Clear();
		indices.Clear();
	}
}
