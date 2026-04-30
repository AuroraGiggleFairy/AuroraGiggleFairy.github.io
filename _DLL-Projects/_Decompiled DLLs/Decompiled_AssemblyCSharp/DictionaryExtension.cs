using System;
using System.Collections.Generic;
using System.Linq;

public static class DictionaryExtension
{
	public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dic, Func<TValue, bool> predicate)
	{
		foreach (TKey item in dic.Keys.Where([PublicizedFrom(EAccessModifier.Internal)] (TKey k) => predicate(dic[k])).ToList())
		{
			dic.Remove(item);
		}
	}

	public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dic, Func<TKey, bool> predicate)
	{
		foreach (TKey item in dic.Keys.Where([PublicizedFrom(EAccessModifier.Internal)] (TKey k) => predicate(k)).ToList())
		{
			dic.Remove(item);
		}
	}

	public static void CopyTo<TKey, TValue>(this IDictionary<TKey, TValue> _src, IDictionary<TKey, TValue> _dest, bool _overwriteExisting = false)
	{
		if (_overwriteExisting)
		{
			foreach (KeyValuePair<TKey, TValue> item in _src)
			{
				_dest[item.Key] = item.Value;
			}
			return;
		}
		foreach (KeyValuePair<TKey, TValue> item2 in _src)
		{
			_dest.Add(item2.Key, item2.Value);
		}
	}

	public static void CopyKeysTo<TKey, TValue>(this IDictionary<TKey, TValue> _src, ICollection<TKey> _dest)
	{
		foreach (KeyValuePair<TKey, TValue> item in _src)
		{
			_dest.Add(item.Key);
		}
	}

	public static void CopyKeysTo<TKey, TValue>(this IDictionary<TKey, TValue> _src, TKey[] _dest)
	{
		if (_dest.Length != _src.Count)
		{
			throw new ArgumentOutOfRangeException("_dest", "Target array does not have the same size as the dictionary");
		}
		int num = 0;
		foreach (KeyValuePair<TKey, TValue> item in _src)
		{
			_dest[num++] = item.Key;
		}
	}

	public static void CopyValuesTo<TKey, TValue>(this IDictionary<TKey, TValue> _src, IList<TValue> _dest)
	{
		foreach (KeyValuePair<TKey, TValue> item in _src)
		{
			_dest.Add(item.Value);
		}
	}

	public static void CopyValuesTo<TKey, TValue>(this IDictionary<TKey, TValue> _src, TValue[] _dest)
	{
		if (_dest.Length != _src.Count)
		{
			throw new ArgumentOutOfRangeException("_dest", "Target array does not have the same size as the dictionary");
		}
		int num = 0;
		foreach (KeyValuePair<TKey, TValue> item in _src)
		{
			_dest[num++] = item.Value;
		}
	}
}
