using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Newtonsoft.Json.Utilities;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class DictionaryWrapper<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] TKey, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IWrappedDictionary, IDictionary, ICollection
{
	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
	[_003C7efad6e0_002D6dbc_002D40f5_002Dac7f_002De8a284fe164b_003EIsReadOnly]
	private struct DictionaryEnumerator<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] TEnumeratorKey, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] TEnumeratorValue> : IDictionaryEnumerator, IEnumerator
	{
		[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 1, 0, 1, 1 })]
		private readonly IEnumerator<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> _e;

		public DictionaryEntry Entry => (DictionaryEntry)Current;

		public object Key => Entry.Key;

		[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
		public object Value
		{
			[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
			get
			{
				return Entry.Value;
			}
		}

		public object Current => new DictionaryEntry(_e.Current.Key, _e.Current.Value);

		public DictionaryEnumerator([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 1, 0, 1, 1 })] IEnumerator<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> e)
		{
			ValidationUtils.ArgumentNotNull(e, "e");
			_e = e;
		}

		public bool MoveNext()
		{
			return _e.MoveNext();
		}

		public void Reset()
		{
			_e.Reset();
		}
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	private readonly IDictionary _dictionary;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1, 1 })]
	private readonly IDictionary<TKey, TValue> _genericDictionary;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1, 1 })]
	private readonly IReadOnlyDictionary<TKey, TValue> _readOnlyDictionary;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	private object _syncRoot;

	internal IDictionary<TKey, TValue> GenericDictionary => _genericDictionary;

	public ICollection<TKey> Keys
	{
		get
		{
			if (_dictionary != null)
			{
				return _dictionary.Keys.Cast<TKey>().ToList();
			}
			if (_readOnlyDictionary != null)
			{
				return _readOnlyDictionary.Keys.ToList();
			}
			return GenericDictionary.Keys;
		}
	}

	public ICollection<TValue> Values
	{
		get
		{
			if (_dictionary != null)
			{
				return _dictionary.Values.Cast<TValue>().ToList();
			}
			if (_readOnlyDictionary != null)
			{
				return _readOnlyDictionary.Values.ToList();
			}
			return GenericDictionary.Values;
		}
	}

	public TValue this[TKey key]
	{
		get
		{
			if (_dictionary != null)
			{
				return (TValue)_dictionary[key];
			}
			if (_readOnlyDictionary != null)
			{
				return _readOnlyDictionary[key];
			}
			return GenericDictionary[key];
		}
		set
		{
			if (_dictionary != null)
			{
				_dictionary[key] = value;
				return;
			}
			if (_readOnlyDictionary != null)
			{
				throw new NotSupportedException();
			}
			GenericDictionary[key] = value;
		}
	}

	public int Count
	{
		get
		{
			if (_dictionary != null)
			{
				return _dictionary.Count;
			}
			if (_readOnlyDictionary != null)
			{
				return _readOnlyDictionary.Count;
			}
			return GenericDictionary.Count;
		}
	}

	public bool IsReadOnly
	{
		get
		{
			if (_dictionary != null)
			{
				return _dictionary.IsReadOnly;
			}
			if (_readOnlyDictionary != null)
			{
				return true;
			}
			return GenericDictionary.IsReadOnly;
		}
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	object IDictionary.this[object key]
	{
		[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
		get
		{
			if (_dictionary != null)
			{
				return _dictionary[key];
			}
			if (_readOnlyDictionary != null)
			{
				return _readOnlyDictionary[(TKey)key];
			}
			return GenericDictionary[(TKey)key];
		}
		[param: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
		set
		{
			if (_dictionary != null)
			{
				_dictionary[key] = value;
				return;
			}
			if (_readOnlyDictionary != null)
			{
				throw new NotSupportedException();
			}
			GenericDictionary[(TKey)key] = (TValue)value;
		}
	}

	bool IDictionary.IsFixedSize
	{
		get
		{
			if (_genericDictionary != null)
			{
				return false;
			}
			if (_readOnlyDictionary != null)
			{
				return true;
			}
			return _dictionary.IsFixedSize;
		}
	}

	ICollection IDictionary.Keys
	{
		get
		{
			if (_genericDictionary != null)
			{
				return _genericDictionary.Keys.ToList();
			}
			if (_readOnlyDictionary != null)
			{
				return _readOnlyDictionary.Keys.ToList();
			}
			return _dictionary.Keys;
		}
	}

	ICollection IDictionary.Values
	{
		get
		{
			if (_genericDictionary != null)
			{
				return _genericDictionary.Values.ToList();
			}
			if (_readOnlyDictionary != null)
			{
				return _readOnlyDictionary.Values.ToList();
			}
			return _dictionary.Values;
		}
	}

	bool ICollection.IsSynchronized
	{
		get
		{
			if (_dictionary != null)
			{
				return _dictionary.IsSynchronized;
			}
			return false;
		}
	}

	object ICollection.SyncRoot
	{
		get
		{
			if (_syncRoot == null)
			{
				Interlocked.CompareExchange(ref _syncRoot, new object(), null);
			}
			return _syncRoot;
		}
	}

	public object UnderlyingDictionary
	{
		get
		{
			if (_dictionary != null)
			{
				return _dictionary;
			}
			if (_readOnlyDictionary != null)
			{
				return _readOnlyDictionary;
			}
			return GenericDictionary;
		}
	}

	public DictionaryWrapper(IDictionary dictionary)
	{
		ValidationUtils.ArgumentNotNull(dictionary, "dictionary");
		_dictionary = dictionary;
	}

	public DictionaryWrapper(IDictionary<TKey, TValue> dictionary)
	{
		ValidationUtils.ArgumentNotNull(dictionary, "dictionary");
		_genericDictionary = dictionary;
	}

	public DictionaryWrapper(IReadOnlyDictionary<TKey, TValue> dictionary)
	{
		ValidationUtils.ArgumentNotNull(dictionary, "dictionary");
		_readOnlyDictionary = dictionary;
	}

	public void Add(TKey key, TValue value)
	{
		if (_dictionary != null)
		{
			_dictionary.Add(key, value);
			return;
		}
		if (_genericDictionary != null)
		{
			_genericDictionary.Add(key, value);
			return;
		}
		throw new NotSupportedException();
	}

	public bool ContainsKey(TKey key)
	{
		if (_dictionary != null)
		{
			return _dictionary.Contains(key);
		}
		if (_readOnlyDictionary != null)
		{
			return _readOnlyDictionary.ContainsKey(key);
		}
		return GenericDictionary.ContainsKey(key);
	}

	public bool Remove(TKey key)
	{
		if (_dictionary != null)
		{
			if (_dictionary.Contains(key))
			{
				_dictionary.Remove(key);
				return true;
			}
			return false;
		}
		if (_readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		return GenericDictionary.Remove(key);
	}

	public bool TryGetValue(TKey key, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] out TValue value)
	{
		if (_dictionary != null)
		{
			if (!_dictionary.Contains(key))
			{
				value = default(TValue);
				return false;
			}
			value = (TValue)_dictionary[key];
			return true;
		}
		if (_readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		return GenericDictionary.TryGetValue(key, out value);
	}

	public void Add([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 0, 1, 1 })] KeyValuePair<TKey, TValue> item)
	{
		if (_dictionary != null)
		{
			((IList)_dictionary).Add(item);
			return;
		}
		if (_readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		_genericDictionary?.Add(item);
	}

	public void Clear()
	{
		if (_dictionary != null)
		{
			_dictionary.Clear();
			return;
		}
		if (_readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		GenericDictionary.Clear();
	}

	public bool Contains([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 0, 1, 1 })] KeyValuePair<TKey, TValue> item)
	{
		if (_dictionary != null)
		{
			return ((IList)_dictionary).Contains(item);
		}
		if (_readOnlyDictionary != null)
		{
			return _readOnlyDictionary.Contains(item);
		}
		return GenericDictionary.Contains(item);
	}

	public void CopyTo([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 1, 0, 1, 1 })] KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		if (_dictionary != null)
		{
			foreach (DictionaryEntry item in _dictionary)
			{
				array[arrayIndex++] = new KeyValuePair<TKey, TValue>((TKey)item.Key, (TValue)item.Value);
			}
			return;
		}
		if (_readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		GenericDictionary.CopyTo(array, arrayIndex);
	}

	public bool Remove([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 0, 1, 1 })] KeyValuePair<TKey, TValue> item)
	{
		if (_dictionary != null)
		{
			if (_dictionary.Contains(item.Key))
			{
				if (object.Equals(_dictionary[item.Key], item.Value))
				{
					_dictionary.Remove(item.Key);
					return true;
				}
				return false;
			}
			return true;
		}
		if (_readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		return GenericDictionary.Remove(item);
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 1, 0, 1, 1 })]
	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		if (_dictionary != null)
		{
			return (from DictionaryEntry de in _dictionary
				select new KeyValuePair<TKey, TValue>((TKey)de.Key, (TValue)de.Value)).GetEnumerator();
		}
		if (_readOnlyDictionary != null)
		{
			return _readOnlyDictionary.GetEnumerator();
		}
		return GenericDictionary.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	void IDictionary.Add(object key, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value)
	{
		if (_dictionary != null)
		{
			_dictionary.Add(key, value);
			return;
		}
		if (_readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		GenericDictionary.Add((TKey)key, (TValue)value);
	}

	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		if (_dictionary != null)
		{
			return _dictionary.GetEnumerator();
		}
		if (_readOnlyDictionary != null)
		{
			return new DictionaryEnumerator<TKey, TValue>(_readOnlyDictionary.GetEnumerator());
		}
		return new DictionaryEnumerator<TKey, TValue>(GenericDictionary.GetEnumerator());
	}

	bool IDictionary.Contains(object key)
	{
		if (_genericDictionary != null)
		{
			return _genericDictionary.ContainsKey((TKey)key);
		}
		if (_readOnlyDictionary != null)
		{
			return _readOnlyDictionary.ContainsKey((TKey)key);
		}
		return _dictionary.Contains(key);
	}

	public void Remove(object key)
	{
		if (_dictionary != null)
		{
			_dictionary.Remove(key);
			return;
		}
		if (_readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		GenericDictionary.Remove((TKey)key);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		if (_dictionary != null)
		{
			_dictionary.CopyTo(array, index);
			return;
		}
		if (_readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		GenericDictionary.CopyTo((KeyValuePair<TKey, TValue>[])array, index);
	}
}
