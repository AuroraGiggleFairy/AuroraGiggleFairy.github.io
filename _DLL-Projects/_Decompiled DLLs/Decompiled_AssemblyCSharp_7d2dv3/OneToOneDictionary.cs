using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class OneToOneDictionary<TKey, TValue>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class Entry<T>
	{
		public readonly T Value;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int stableHash;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly bool s_isRefType = !typeof(T).IsValueType;

		public Entry(T value)
		{
			Value = value;
			stableHash = ((!s_isRefType) ? EqualityComparer<T>.Default.GetHashCode(value) : ((value != null) ? RuntimeHelpers.GetHashCode(value) : 0));
		}

		public override int GetHashCode()
		{
			return stableHash;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Entry<T> entry))
			{
				return false;
			}
			if (s_isRefType)
			{
				return (object)Value == (object)entry.Value;
			}
			return EqualityComparer<T>.Default.Equals(Value, entry.Value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Entry<TKey>, Entry<TValue>> _forward = new Dictionary<Entry<TKey>, Entry<TValue>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Entry<TValue>, Entry<TKey>> _reverse = new Dictionary<Entry<TValue>, Entry<TKey>>();

	public int Count => _forward.Count;

	public IEnumerable<TKey> Keys
	{
		get
		{
			foreach (Entry<TKey> key in _forward.Keys)
			{
				yield return key.Value;
			}
		}
	}

	public IEnumerable<TValue> Values
	{
		get
		{
			foreach (Entry<TValue> value in _forward.Values)
			{
				yield return value.Value;
			}
		}
	}

	public void Add(TKey key, TValue value)
	{
		Entry<TKey> entry = new Entry<TKey>(key);
		Entry<TValue> entry2 = new Entry<TValue>(value);
		if (_forward.ContainsKey(entry))
		{
			throw new ArgumentException($"[TwoWayDictionary] Duplicate key: {key}");
		}
		if (_reverse.ContainsKey(entry2))
		{
			throw new ArgumentException($"[TwoWayDictionary] Duplicate value: {value}");
		}
		_forward[entry] = entry2;
		_reverse[entry2] = entry;
	}

	public bool RemoveByKey(TKey key)
	{
		Entry<TKey> key2 = new Entry<TKey>(key);
		if (_forward.Remove(key2, out var value))
		{
			_reverse.Remove(value);
			return true;
		}
		return false;
	}

	public bool RemoveByValue(TValue value)
	{
		Entry<TValue> key = new Entry<TValue>(value);
		if (_reverse.Remove(key, out var value2))
		{
			_forward.Remove(value2);
			return true;
		}
		return false;
	}

	public bool TryGetByKey(TKey key, out TValue value)
	{
		Entry<TKey> key2 = new Entry<TKey>(key);
		if (_forward.TryGetValue(key2, out var value2))
		{
			value = value2.Value;
			return true;
		}
		value = default(TValue);
		return false;
	}

	public bool TryGetByValue(TValue value, out TKey key)
	{
		Entry<TValue> key2 = new Entry<TValue>(value);
		if (_reverse.TryGetValue(key2, out var value2))
		{
			key = value2.Value;
			return true;
		}
		key = default(TKey);
		return false;
	}

	public TValue GetByKey(TKey key)
	{
		return _forward[new Entry<TKey>(key)].Value;
	}

	public TKey GetByValue(TValue value)
	{
		return _reverse[new Entry<TValue>(value)].Value;
	}

	public bool ContainsKey(TKey key)
	{
		return _forward.ContainsKey(new Entry<TKey>(key));
	}

	public bool ContainsValue(TValue value)
	{
		return _reverse.ContainsKey(new Entry<TValue>(value));
	}

	public void Clear()
	{
		_forward.Clear();
		_reverse.Clear();
	}
}
