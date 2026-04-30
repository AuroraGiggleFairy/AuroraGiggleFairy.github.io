using System.Collections;
using System.Collections.Generic;

public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IDictionary<TKey, TValue> _dictionary;

	public TValue this[TKey key]
	{
		get
		{
			return _dictionary[key];
		}
		set
		{
			if (_dictionary.ContainsKey(key))
			{
				_dictionary[key] = value;
				OnEntryUpdated(key, value);
			}
			else
			{
				_dictionary[key] = value;
				OnEntryAdded(key, value);
			}
		}
	}

	public ICollection<TKey> Keys => _dictionary.Keys;

	public ICollection<TValue> Values => _dictionary.Values;

	public int Count => _dictionary.Count;

	public bool IsReadOnly => false;

	public event DictionaryAddEventHandler<TKey, TValue> EntryAdded;

	public event DictionaryRemoveEventHandler<TKey, TValue> EntryRemoved;

	public event DictionaryUpdatedValueEventHandler<TKey, TValue> EntryUpdatedValue;

	public event DictionaryEntryModifiedEventHandler<TKey, TValue> EntryModified;

	public ObservableDictionary()
		: this((IDictionary<TKey, TValue>)new Dictionary<TKey, TValue>())
	{
	}

	public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
	{
		_dictionary = dictionary;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEntryModified(TKey key, TValue value, string action)
	{
		this.EntryModified?.Invoke(this, new DictionaryChangedEventArgs<TKey, TValue>(key, value, action));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEntryAdded(TKey key, TValue value)
	{
		this.EntryAdded?.Invoke(this, new DictionaryChangedEventArgs<TKey, TValue>(key, value, "Added"));
		OnEntryModified(key, value, "Added");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEntryRemoved(TKey key, TValue value)
	{
		this.EntryRemoved?.Invoke(this, new DictionaryChangedEventArgs<TKey, TValue>(key, value, "Removed"));
		OnEntryModified(key, value, "Removed");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEntryUpdated(TKey key, TValue value)
	{
		this.EntryUpdatedValue?.Invoke(this, new DictionaryChangedEventArgs<TKey, TValue>(key, value, "Updated"));
		OnEntryModified(key, value, "Updated");
	}

	public void Add(TKey key, TValue value)
	{
		_dictionary.Add(key, value);
		OnEntryAdded(key, value);
	}

	public bool Remove(TKey key)
	{
		if (_dictionary.TryGetValue(key, out var value) && _dictionary.Remove(key))
		{
			OnEntryRemoved(key, value);
			return true;
		}
		return false;
	}

	public bool ContainsKey(TKey key)
	{
		return _dictionary.ContainsKey(key);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		return _dictionary.TryGetValue(key, out value);
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		Add(item.Key, item.Value);
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		return Remove(item.Key);
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		if (_dictionary.ContainsKey(item.Key))
		{
			return EqualityComparer<TValue>.Default.Equals(_dictionary[item.Key], item.Value);
		}
		return false;
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		_dictionary.CopyTo(array, arrayIndex);
	}

	public void Clear()
	{
		foreach (TKey item in new List<TKey>(_dictionary.Keys))
		{
			Remove(item);
		}
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return _dictionary.GetEnumerator();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return _dictionary.GetEnumerator();
	}
}
