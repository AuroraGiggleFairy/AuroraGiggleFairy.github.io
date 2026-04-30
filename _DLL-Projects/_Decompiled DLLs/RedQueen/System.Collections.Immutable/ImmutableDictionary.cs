using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Collections.Immutable;

[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
internal static class ImmutableDictionary
{
	public static ImmutableDictionary<TKey, TValue> Create<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>()
	{
		return ImmutableDictionary<TKey, TValue>.Empty;
	}

	public static ImmutableDictionary<TKey, TValue> Create<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> keyComparer)
	{
		return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer);
	}

	public static ImmutableDictionary<TKey, TValue> Create<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> keyComparer, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TValue> valueComparer)
	{
		return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer);
	}

	public static ImmutableDictionary<TKey, TValue> CreateRange<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })] IEnumerable<KeyValuePair<TKey, TValue>> items)
	{
		return ImmutableDictionary<TKey, TValue>.Empty.AddRange(items);
	}

	public static ImmutableDictionary<TKey, TValue> CreateRange<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> keyComparer, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })] IEnumerable<KeyValuePair<TKey, TValue>> items)
	{
		return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer).AddRange(items);
	}

	public static ImmutableDictionary<TKey, TValue> CreateRange<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> keyComparer, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TValue> valueComparer, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })] IEnumerable<KeyValuePair<TKey, TValue>> items)
	{
		return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(items);
	}

	public static ImmutableDictionary<TKey, TValue>.Builder CreateBuilder<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>()
	{
		return Create<TKey, TValue>().ToBuilder();
	}

	public static ImmutableDictionary<TKey, TValue>.Builder CreateBuilder<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> keyComparer)
	{
		return Create<TKey, TValue>(keyComparer).ToBuilder();
	}

	public static ImmutableDictionary<TKey, TValue>.Builder CreateBuilder<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> keyComparer, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TValue> valueComparer)
	{
		return Create(keyComparer, valueComparer).ToBuilder();
	}

	public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TSource, TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> keyComparer, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TValue> valueComparer)
	{
		Requires.NotNull(source, "source");
		Requires.NotNull(keySelector, "keySelector");
		Requires.NotNull(elementSelector, "elementSelector");
		return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(source.Select((TSource element) => new KeyValuePair<TKey, TValue>(keySelector(element), elementSelector(element))));
	}

	public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>(this ImmutableDictionary<TKey, TValue>.Builder builder)
	{
		Requires.NotNull(builder, "builder");
		return builder.ToImmutable();
	}

	public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TSource, TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> keyComparer)
	{
		return source.ToImmutableDictionary(keySelector, elementSelector, keyComparer, null);
	}

	public static ImmutableDictionary<TKey, TSource> ToImmutableDictionary<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return source.ToImmutableDictionary(keySelector, (TSource v) => v, null, null);
	}

	public static ImmutableDictionary<TKey, TSource> ToImmutableDictionary<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> keyComparer)
	{
		return source.ToImmutableDictionary(keySelector, (TSource v) => v, keyComparer, null);
	}

	public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TSource, TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector)
	{
		return source.ToImmutableDictionary(keySelector, elementSelector, null, null);
	}

	public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })] this IEnumerable<KeyValuePair<TKey, TValue>> source, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> keyComparer, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TValue> valueComparer)
	{
		Requires.NotNull(source, "source");
		if (source is ImmutableDictionary<TKey, TValue> immutableDictionary)
		{
			return immutableDictionary.WithComparers(keyComparer, valueComparer);
		}
		return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(source);
	}

	public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })] this IEnumerable<KeyValuePair<TKey, TValue>> source, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> keyComparer)
	{
		return source.ToImmutableDictionary(keyComparer, null);
	}

	public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })] this IEnumerable<KeyValuePair<TKey, TValue>> source)
	{
		return source.ToImmutableDictionary(null, null);
	}

	public static bool Contains<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>(this IImmutableDictionary<TKey, TValue> map, TKey key, TValue value)
	{
		Requires.NotNull(map, "map");
		Requires.NotNullAllowStructs(key, "key");
		return map.Contains(new KeyValuePair<TKey, TValue>(key, value));
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)]
	public static TValue GetValueOrDefault<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>(this IImmutableDictionary<TKey, TValue> dictionary, TKey key)
	{
		return dictionary.GetValueOrDefault(key, default(TValue));
	}

	public static TValue GetValueOrDefault<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>(this IImmutableDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
	{
		Requires.NotNull(dictionary, "dictionary");
		Requires.NotNullAllowStructs(key, "key");
		if (dictionary.TryGetValue(key, out var value))
		{
			return value;
		}
		return defaultValue;
	}
}
[DebuggerTypeProxy(typeof(ImmutableDictionaryDebuggerProxy<, >))]
[DebuggerDisplay("Count = {Count}")]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
internal sealed class ImmutableDictionary<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue> : IImmutableDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IImmutableDictionaryInternal<TKey, TValue>, IHashKeyCollection<TKey>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IDictionary, ICollection
{
	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
	[DebuggerTypeProxy(typeof(ImmutableDictionaryBuilderDebuggerProxy<, >))]
	[DebuggerDisplay("Count = {Count}")]
	public sealed class Builder : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IDictionary, ICollection
	{
		private SortedInt32KeyNode<HashBucket> _root = SortedInt32KeyNode<HashBucket>.EmptyNode;

		private Comparers _comparers;

		private int _count;

		private ImmutableDictionary<TKey, TValue> _immutable;

		private int _version;

		private object _syncRoot;

		public IEqualityComparer<TKey> KeyComparer
		{
			get
			{
				return _comparers.KeyComparer;
			}
			set
			{
				Requires.NotNull(value, "value");
				if (value != KeyComparer)
				{
					Comparers comparers = Comparers.Get(value, ValueComparer);
					MutationInput origin = new MutationInput(SortedInt32KeyNode<HashBucket>.EmptyNode, comparers);
					MutationResult mutationResult = ImmutableDictionary<TKey, TValue>.AddRange((IEnumerable<KeyValuePair<TKey, TValue>>)this, origin, KeyCollisionBehavior.ThrowIfValueDifferent);
					_immutable = null;
					_comparers = comparers;
					_count = mutationResult.CountAdjustment;
					Root = mutationResult.Root;
				}
			}
		}

		public IEqualityComparer<TValue> ValueComparer
		{
			get
			{
				return _comparers.ValueComparer;
			}
			set
			{
				Requires.NotNull(value, "value");
				if (value != ValueComparer)
				{
					_comparers = _comparers.WithValueComparer(value);
					_immutable = null;
				}
			}
		}

		public int Count => _count;

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

		public IEnumerable<TKey> Keys
		{
			get
			{
				using Enumerator enumerator = GetEnumerator();
				while (enumerator.MoveNext())
				{
					yield return enumerator.Current.Key;
				}
			}
		}

		ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys.ToArray(Count);

		public IEnumerable<TValue> Values
		{
			get
			{
				using Enumerator enumerator = GetEnumerator();
				while (enumerator.MoveNext())
				{
					yield return enumerator.Current.Value;
				}
			}
		}

		ICollection<TValue> IDictionary<TKey, TValue>.Values => Values.ToArray(Count);

		bool IDictionary.IsFixedSize => false;

		bool IDictionary.IsReadOnly => false;

		ICollection IDictionary.Keys => Keys.ToArray(Count);

		ICollection IDictionary.Values => Values.ToArray(Count);

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null)
				{
					Interlocked.CompareExchange<object>(ref _syncRoot, new object(), (object)null);
				}
				return _syncRoot;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool ICollection.IsSynchronized => false;

		[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)]
		object IDictionary.this[object key]
		{
			get
			{
				return this[(TKey)key];
			}
			set
			{
				this[(TKey)key] = (TValue)value;
			}
		}

		internal int Version => _version;

		[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
		private MutationInput Origin => new MutationInput(Root, _comparers);

		[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 0, 0 })]
		private SortedInt32KeyNode<HashBucket> Root
		{
			get
			{
				return _root;
			}
			set
			{
				_version++;
				if (_root != value)
				{
					_root = value;
					_immutable = null;
				}
			}
		}

		public TValue this[TKey key]
		{
			get
			{
				if (TryGetValue(key, out var value))
				{
					return value;
				}
				throw new KeyNotFoundException(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.Format(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.Arg_KeyNotFoundWithKey, key.ToString()));
			}
			set
			{
				MutationResult result = ImmutableDictionary<TKey, TValue>.Add(key, value, KeyCollisionBehavior.SetValue, Origin);
				Apply(result);
			}
		}

		internal Builder(ImmutableDictionary<TKey, TValue> map)
		{
			Requires.NotNull(map, "map");
			_root = map._root;
			_count = map._count;
			_comparers = map._comparers;
			_immutable = map;
		}

		void IDictionary.Add(object key, object value)
		{
			Add((TKey)key, (TValue)value);
		}

		bool IDictionary.Contains(object key)
		{
			return ContainsKey((TKey)key);
		}

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return new DictionaryEnumerator<TKey, TValue>(GetEnumerator());
		}

		void IDictionary.Remove(object key)
		{
			Remove((TKey)key);
		}

		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			Requires.NotNull(array, "array");
			Requires.Range(arrayIndex >= 0, "arrayIndex");
			Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<TKey, TValue> current = enumerator.Current;
				array.SetValue(new DictionaryEntry(current.Key, current.Value), arrayIndex++);
			}
		}

		public void AddRange([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })] IEnumerable<KeyValuePair<TKey, TValue>> items)
		{
			MutationResult result = ImmutableDictionary<TKey, TValue>.AddRange(items, Origin, KeyCollisionBehavior.ThrowIfValueDifferent);
			Apply(result);
		}

		public void RemoveRange(IEnumerable<TKey> keys)
		{
			Requires.NotNull(keys, "keys");
			foreach (TKey key in keys)
			{
				Remove(key);
			}
		}

		[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
		public Enumerator GetEnumerator()
		{
			return new Enumerator(_root, this);
		}

		[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)]
		public TValue GetValueOrDefault(TKey key)
		{
			return GetValueOrDefault(key, default(TValue));
		}

		public TValue GetValueOrDefault(TKey key, TValue defaultValue)
		{
			Requires.NotNullAllowStructs(key, "key");
			if (TryGetValue(key, out var value))
			{
				return value;
			}
			return defaultValue;
		}

		public ImmutableDictionary<TKey, TValue> ToImmutable()
		{
			if (_immutable == null)
			{
				_immutable = ImmutableDictionary<TKey, TValue>.Wrap(_root, _comparers, _count);
			}
			return _immutable;
		}

		public void Add(TKey key, TValue value)
		{
			MutationResult result = ImmutableDictionary<TKey, TValue>.Add(key, value, KeyCollisionBehavior.ThrowIfValueDifferent, Origin);
			Apply(result);
		}

		public bool ContainsKey(TKey key)
		{
			return ImmutableDictionary<TKey, TValue>.ContainsKey(key, Origin);
		}

		public bool ContainsValue(TValue value)
		{
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<TKey, TValue> current = enumerator.Current;
					if (ValueComparer.Equals(value, current.Value))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool Remove(TKey key)
		{
			MutationResult result = ImmutableDictionary<TKey, TValue>.Remove(key, Origin);
			return Apply(result);
		}

		public bool TryGetValue(TKey key, [_003C6723b510_002D2ae0_002D4796_002Dbe1b_002D098bdaf7a574_003EMaybeNullWhen(false)] out TValue value)
		{
			return ImmutableDictionary<TKey, TValue>.TryGetValue(key, Origin, out value);
		}

		public bool TryGetKey(TKey equalKey, out TKey actualKey)
		{
			return ImmutableDictionary<TKey, TValue>.TryGetKey(equalKey, Origin, out actualKey);
		}

		public void Add([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1, 1 })] KeyValuePair<TKey, TValue> item)
		{
			Add(item.Key, item.Value);
		}

		public void Clear()
		{
			Root = SortedInt32KeyNode<HashBucket>.EmptyNode;
			_count = 0;
		}

		public bool Contains([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1, 1 })] KeyValuePair<TKey, TValue> item)
		{
			return ImmutableDictionary<TKey, TValue>.Contains(item, Origin);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			Requires.NotNull(array, "array");
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<TKey, TValue> current = enumerator.Current;
				array[arrayIndex++] = current;
			}
		}

		public bool Remove([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1, 1 })] KeyValuePair<TKey, TValue> item)
		{
			if (Contains(item))
			{
				return Remove(item.Key);
			}
			return false;
		}

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private bool Apply(MutationResult result)
		{
			Root = result.Root;
			_count += result.CountAdjustment;
			return result.CountAdjustment != 0;
		}
	}

	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
	internal sealed class Comparers : IEqualityComparer<HashBucket>, IEqualityComparer<KeyValuePair<TKey, TValue>>
	{
		[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 0 })]
		internal static readonly Comparers Default = new Comparers(EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default);

		private readonly IEqualityComparer<TKey> _keyComparer;

		private readonly IEqualityComparer<TValue> _valueComparer;

		internal IEqualityComparer<TKey> KeyComparer => _keyComparer;

		[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })]
		internal IEqualityComparer<KeyValuePair<TKey, TValue>> KeyOnlyComparer
		{
			[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })]
			get
			{
				return this;
			}
		}

		internal IEqualityComparer<TValue> ValueComparer => _valueComparer;

		[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 0, 0 })]
		internal IEqualityComparer<HashBucket> HashBucketEqualityComparer
		{
			[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 0, 0 })]
			get
			{
				return this;
			}
		}

		internal Comparers(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
		{
			Requires.NotNull(keyComparer, "keyComparer");
			Requires.NotNull(valueComparer, "valueComparer");
			_keyComparer = keyComparer;
			_valueComparer = valueComparer;
		}

		[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
		public bool Equals(HashBucket x, HashBucket y)
		{
			if (x.AdditionalElements == y.AdditionalElements && KeyComparer.Equals(x.FirstValue.Key, y.FirstValue.Key))
			{
				return ValueComparer.Equals(x.FirstValue.Value, y.FirstValue.Value);
			}
			return false;
		}

		[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
		public int GetHashCode(HashBucket obj)
		{
			return KeyComparer.GetHashCode(obj.FirstValue.Key);
		}

		bool IEqualityComparer<KeyValuePair<TKey, TValue>>.Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
		{
			return _keyComparer.Equals(x.Key, y.Key);
		}

		int IEqualityComparer<KeyValuePair<TKey, TValue>>.GetHashCode(KeyValuePair<TKey, TValue> obj)
		{
			return _keyComparer.GetHashCode(obj.Key);
		}

		[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 0 })]
		internal static Comparers Get(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
		{
			Requires.NotNull(keyComparer, "keyComparer");
			Requires.NotNull(valueComparer, "valueComparer");
			if (keyComparer != Default.KeyComparer || valueComparer != Default.ValueComparer)
			{
				return new Comparers(keyComparer, valueComparer);
			}
			return Default;
		}

		[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 0 })]
		internal Comparers WithValueComparer(IEqualityComparer<TValue> valueComparer)
		{
			Requires.NotNull(valueComparer, "valueComparer");
			if (_valueComparer != valueComparer)
			{
				return Get(KeyComparer, valueComparer);
			}
			return this;
		}
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
	public struct Enumerator([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 0, 0 })] SortedInt32KeyNode<HashBucket> root, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 0, 0 })] Builder builder = null) : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IEnumerator
	{
		private readonly Builder _builder = builder;

		private SortedInt32KeyNode<HashBucket>.Enumerator _mapEnumerator = new SortedInt32KeyNode<HashBucket>.Enumerator(root);

		private HashBucket.Enumerator _bucketEnumerator = default(HashBucket.Enumerator);

		private int _enumeratingBuilderVersion = builder?.Version ?? (-1);

		[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1, 1 })]
		public KeyValuePair<TKey, TValue> Current
		{
			[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1, 1 })]
			get
			{
				_mapEnumerator.ThrowIfDisposed();
				return _bucketEnumerator.Current;
			}
		}

		[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)]
		object IEnumerator.Current => Current;

		public bool MoveNext()
		{
			ThrowIfChanged();
			if (_bucketEnumerator.MoveNext())
			{
				return true;
			}
			if (_mapEnumerator.MoveNext())
			{
				_bucketEnumerator = new HashBucket.Enumerator(_mapEnumerator.Current.Value);
				return _bucketEnumerator.MoveNext();
			}
			return false;
		}

		public void Reset()
		{
			_enumeratingBuilderVersion = ((_builder != null) ? _builder.Version : (-1));
			_mapEnumerator.Reset();
			_bucketEnumerator.Dispose();
			_bucketEnumerator = default(HashBucket.Enumerator);
		}

		public void Dispose()
		{
			_mapEnumerator.Dispose();
			_bucketEnumerator.Dispose();
		}

		private void ThrowIfChanged()
		{
			if (_builder != null && _builder.Version != _enumeratingBuilderVersion)
			{
				throw new InvalidOperationException(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.CollectionModifiedDuringEnumeration);
			}
		}
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
	[_003C213e6825_002D2666_002D4c32_002Dad9b_002D2809d47857fd_003EIsReadOnly]
	internal struct HashBucket(KeyValuePair<TKey, TValue> firstElement, ImmutableList<KeyValuePair<TKey, TValue>>.Node additionalElements = null) : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
	{
		internal struct Enumerator(HashBucket bucket) : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IEnumerator
		{
			private enum Position
			{
				BeforeFirst,
				First,
				Additional,
				End
			}

			private readonly HashBucket _bucket = bucket;

			private Position _currentPosition = Position.BeforeFirst;

			private ImmutableList<KeyValuePair<TKey, TValue>>.Enumerator _additionalEnumerator = default(ImmutableList<KeyValuePair<TKey, TValue>>.Enumerator);

			[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)]
			object IEnumerator.Current => Current;

			[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1, 1 })]
			public KeyValuePair<TKey, TValue> Current
			{
				[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1, 1 })]
				get
				{
					return _currentPosition switch
					{
						Position.First => _bucket._firstValue, 
						Position.Additional => _additionalEnumerator.Current, 
						_ => throw new InvalidOperationException(), 
					};
				}
			}

			public bool MoveNext()
			{
				if (_bucket.IsEmpty)
				{
					_currentPosition = Position.End;
					return false;
				}
				switch (_currentPosition)
				{
				case Position.BeforeFirst:
					_currentPosition = Position.First;
					return true;
				case Position.First:
					if (_bucket._additionalElements.IsEmpty)
					{
						_currentPosition = Position.End;
						return false;
					}
					_currentPosition = Position.Additional;
					_additionalEnumerator = new ImmutableList<KeyValuePair<TKey, TValue>>.Enumerator(_bucket._additionalElements);
					return _additionalEnumerator.MoveNext();
				case Position.Additional:
					return _additionalEnumerator.MoveNext();
				case Position.End:
					return false;
				default:
					throw new InvalidOperationException();
				}
			}

			public void Reset()
			{
				_additionalEnumerator.Dispose();
				_currentPosition = Position.BeforeFirst;
			}

			public void Dispose()
			{
				_additionalEnumerator.Dispose();
			}
		}

		private readonly KeyValuePair<TKey, TValue> _firstValue = firstElement;

		private readonly ImmutableList<KeyValuePair<TKey, TValue>>.Node _additionalElements = additionalElements ?? ImmutableList<KeyValuePair<TKey, TValue>>.Node.EmptyNode;

		internal bool IsEmpty => _additionalElements == null;

		[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1, 1 })]
		internal KeyValuePair<TKey, TValue> FirstValue
		{
			[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1, 1 })]
			get
			{
				if (IsEmpty)
				{
					throw new InvalidOperationException();
				}
				return _firstValue;
			}
		}

		[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })]
		internal ImmutableList<KeyValuePair<TKey, TValue>>.Node AdditionalElements
		{
			[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })]
			get
			{
				return _additionalElements;
			}
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
		public override bool Equals(object obj)
		{
			throw new NotSupportedException();
		}

		public override int GetHashCode()
		{
			throw new NotSupportedException();
		}

		internal HashBucket Add([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] TKey key, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] TValue value, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })] IEqualityComparer<KeyValuePair<TKey, TValue>> keyOnlyComparer, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] IEqualityComparer<TValue> valueComparer, KeyCollisionBehavior behavior, out OperationResult result)
		{
			KeyValuePair<TKey, TValue> keyValuePair = new KeyValuePair<TKey, TValue>(key, value);
			if (IsEmpty)
			{
				result = OperationResult.SizeChanged;
				return new HashBucket(keyValuePair);
			}
			if (keyOnlyComparer.Equals(keyValuePair, _firstValue))
			{
				switch (behavior)
				{
				case KeyCollisionBehavior.SetValue:
					result = OperationResult.AppliedWithoutSizeChange;
					return new HashBucket(keyValuePair, _additionalElements);
				case KeyCollisionBehavior.Skip:
					result = OperationResult.NoChangeRequired;
					return this;
				case KeyCollisionBehavior.ThrowIfValueDifferent:
					if (!valueComparer.Equals(_firstValue.Value, value))
					{
						throw new ArgumentException(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.Format(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.DuplicateKey, key));
					}
					result = OperationResult.NoChangeRequired;
					return this;
				case KeyCollisionBehavior.ThrowAlways:
					throw new ArgumentException(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.Format(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.DuplicateKey, key));
				default:
					throw new InvalidOperationException();
				}
			}
			int num = _additionalElements.IndexOf(keyValuePair, keyOnlyComparer);
			if (num < 0)
			{
				result = OperationResult.SizeChanged;
				return new HashBucket(_firstValue, _additionalElements.Add(keyValuePair));
			}
			switch (behavior)
			{
			case KeyCollisionBehavior.SetValue:
				result = OperationResult.AppliedWithoutSizeChange;
				return new HashBucket(_firstValue, _additionalElements.ReplaceAt(num, keyValuePair));
			case KeyCollisionBehavior.Skip:
				result = OperationResult.NoChangeRequired;
				return this;
			case KeyCollisionBehavior.ThrowIfValueDifferent:
			{
				KeyValuePair<TKey, TValue> keyValuePair2 = _additionalElements.ItemRef(num);
				if (!valueComparer.Equals(keyValuePair2.Value, value))
				{
					throw new ArgumentException(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.Format(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.DuplicateKey, key));
				}
				result = OperationResult.NoChangeRequired;
				return this;
			}
			case KeyCollisionBehavior.ThrowAlways:
				throw new ArgumentException(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.Format(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.DuplicateKey, key));
			default:
				throw new InvalidOperationException();
			}
		}

		internal HashBucket Remove([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] TKey key, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })] IEqualityComparer<KeyValuePair<TKey, TValue>> keyOnlyComparer, out OperationResult result)
		{
			if (IsEmpty)
			{
				result = OperationResult.NoChangeRequired;
				return this;
			}
			KeyValuePair<TKey, TValue> keyValuePair = new KeyValuePair<TKey, TValue>(key, default(TValue));
			if (keyOnlyComparer.Equals(_firstValue, keyValuePair))
			{
				if (_additionalElements.IsEmpty)
				{
					result = OperationResult.SizeChanged;
					return default(HashBucket);
				}
				int count = _additionalElements.Left.Count;
				result = OperationResult.SizeChanged;
				return new HashBucket(_additionalElements.Key, _additionalElements.RemoveAt(count));
			}
			int num = _additionalElements.IndexOf(keyValuePair, keyOnlyComparer);
			if (num < 0)
			{
				result = OperationResult.NoChangeRequired;
				return this;
			}
			result = OperationResult.SizeChanged;
			return new HashBucket(_firstValue, _additionalElements.RemoveAt(num));
		}

		[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
		internal bool TryGetValue(TKey key, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 0 })] Comparers comparers, [_003C6723b510_002D2ae0_002D4796_002Dbe1b_002D098bdaf7a574_003EMaybeNullWhen(false)] out TValue value)
		{
			if (IsEmpty)
			{
				value = default(TValue);
				return false;
			}
			if (comparers.KeyComparer.Equals(_firstValue.Key, key))
			{
				value = _firstValue.Value;
				return true;
			}
			KeyValuePair<TKey, TValue> item = new KeyValuePair<TKey, TValue>(key, default(TValue));
			int num = _additionalElements.IndexOf(item, comparers.KeyOnlyComparer);
			if (num < 0)
			{
				value = default(TValue);
				return false;
			}
			KeyValuePair<TKey, TValue> keyValuePair = _additionalElements.ItemRef(num);
			value = keyValuePair.Value;
			return true;
		}

		[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
		internal bool TryGetKey(TKey equalKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 0 })] Comparers comparers, out TKey actualKey)
		{
			if (IsEmpty)
			{
				actualKey = equalKey;
				return false;
			}
			if (comparers.KeyComparer.Equals(_firstValue.Key, equalKey))
			{
				actualKey = _firstValue.Key;
				return true;
			}
			KeyValuePair<TKey, TValue> item = new KeyValuePair<TKey, TValue>(equalKey, default(TValue));
			int num = _additionalElements.IndexOf(item, comparers.KeyOnlyComparer);
			if (num < 0)
			{
				actualKey = equalKey;
				return false;
			}
			KeyValuePair<TKey, TValue> keyValuePair = _additionalElements.ItemRef(num);
			actualKey = keyValuePair.Key;
			return true;
		}

		internal void Freeze()
		{
			if (_additionalElements != null)
			{
				_additionalElements.Freeze();
			}
		}
	}

	[_003C213e6825_002D2666_002D4c32_002Dad9b_002D2809d47857fd_003EIsReadOnly]
	private struct MutationInput
	{
		private readonly SortedInt32KeyNode<HashBucket> _root;

		private readonly Comparers _comparers;

		internal SortedInt32KeyNode<HashBucket> Root => _root;

		internal Comparers Comparers => _comparers;

		internal IEqualityComparer<TKey> KeyComparer => _comparers.KeyComparer;

		internal IEqualityComparer<KeyValuePair<TKey, TValue>> KeyOnlyComparer => _comparers.KeyOnlyComparer;

		internal IEqualityComparer<TValue> ValueComparer => _comparers.ValueComparer;

		internal IEqualityComparer<HashBucket> HashBucketComparer => _comparers.HashBucketEqualityComparer;

		internal MutationInput(SortedInt32KeyNode<HashBucket> root, Comparers comparers)
		{
			_root = root;
			_comparers = comparers;
		}

		internal MutationInput(ImmutableDictionary<TKey, TValue> map)
		{
			_root = map._root;
			_comparers = map._comparers;
		}
	}

	[_003C213e6825_002D2666_002D4c32_002Dad9b_002D2809d47857fd_003EIsReadOnly]
	private struct MutationResult
	{
		private readonly SortedInt32KeyNode<HashBucket> _root;

		private readonly int _countAdjustment;

		internal SortedInt32KeyNode<HashBucket> Root => _root;

		internal int CountAdjustment => _countAdjustment;

		internal MutationResult(MutationInput unchangedInput)
		{
			_root = unchangedInput.Root;
			_countAdjustment = 0;
		}

		internal MutationResult(SortedInt32KeyNode<HashBucket> root, int countAdjustment)
		{
			Requires.NotNull(root, "root");
			_root = root;
			_countAdjustment = countAdjustment;
		}

		internal ImmutableDictionary<TKey, TValue> Finalize(ImmutableDictionary<TKey, TValue> priorMap)
		{
			Requires.NotNull(priorMap, "priorMap");
			return priorMap.Wrap(Root, priorMap._count + CountAdjustment);
		}
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
	internal enum KeyCollisionBehavior
	{
		SetValue,
		Skip,
		ThrowIfValueDifferent,
		ThrowAlways
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
	internal enum OperationResult
	{
		AppliedWithoutSizeChange,
		SizeChanged,
		NoChangeRequired
	}

	public static readonly ImmutableDictionary<TKey, TValue> Empty = new ImmutableDictionary<TKey, TValue>();

	private static readonly Action<KeyValuePair<int, HashBucket>> s_FreezeBucketAction = delegate(KeyValuePair<int, HashBucket> kv)
	{
		kv.Value.Freeze();
	};

	private readonly int _count;

	private readonly SortedInt32KeyNode<HashBucket> _root;

	private readonly Comparers _comparers;

	public int Count => _count;

	public bool IsEmpty => Count == 0;

	public IEqualityComparer<TKey> KeyComparer => _comparers.KeyComparer;

	public IEqualityComparer<TValue> ValueComparer => _comparers.ValueComparer;

	public IEnumerable<TKey> Keys
	{
		get
		{
			foreach (KeyValuePair<int, HashBucket> item in _root)
			{
				foreach (KeyValuePair<TKey, TValue> item2 in item.Value)
				{
					yield return item2.Key;
				}
			}
		}
	}

	public IEnumerable<TValue> Values
	{
		get
		{
			foreach (KeyValuePair<int, HashBucket> item in _root)
			{
				foreach (KeyValuePair<TKey, TValue> item2 in item.Value)
				{
					yield return item2.Value;
				}
			}
		}
	}

	ICollection<TKey> IDictionary<TKey, TValue>.Keys => new KeysCollectionAccessor<TKey, TValue>(this);

	ICollection<TValue> IDictionary<TKey, TValue>.Values => new ValuesCollectionAccessor<TKey, TValue>(this);

	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
	private MutationInput Origin => new MutationInput(this);

	public TValue this[TKey key]
	{
		get
		{
			Requires.NotNullAllowStructs(key, "key");
			if (TryGetValue(key, out var value))
			{
				return value;
			}
			throw new KeyNotFoundException(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.Format(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.Arg_KeyNotFoundWithKey, key.ToString()));
		}
	}

	TValue IDictionary<TKey, TValue>.this[TKey key]
	{
		get
		{
			return this[key];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => true;

	bool IDictionary.IsFixedSize => true;

	bool IDictionary.IsReadOnly => true;

	ICollection IDictionary.Keys => new KeysCollectionAccessor<TKey, TValue>(this);

	ICollection IDictionary.Values => new ValuesCollectionAccessor<TKey, TValue>(this);

	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 0, 0 })]
	internal SortedInt32KeyNode<HashBucket> Root
	{
		[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 0, 0 })]
		get
		{
			return _root;
		}
	}

	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)]
	object IDictionary.this[object key]
	{
		get
		{
			return this[(TKey)key];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	object ICollection.SyncRoot => this;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	bool ICollection.IsSynchronized => true;

	private ImmutableDictionary(SortedInt32KeyNode<HashBucket> root, Comparers comparers, int count)
		: this(Requires.NotNullPassthrough(comparers, "comparers"))
	{
		Requires.NotNull(root, "root");
		root.Freeze(s_FreezeBucketAction);
		_root = root;
		_count = count;
	}

	private ImmutableDictionary(Comparers comparers = null)
	{
		_comparers = comparers ?? Comparers.Get(EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default);
		_root = SortedInt32KeyNode<HashBucket>.EmptyNode;
	}

	public ImmutableDictionary<TKey, TValue> Clear()
	{
		if (!IsEmpty)
		{
			return EmptyWithComparers(_comparers);
		}
		return this;
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Clear()
	{
		return Clear();
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 0 })]
	public Builder ToBuilder()
	{
		return new Builder(this);
	}

	public ImmutableDictionary<TKey, TValue> Add(TKey key, TValue value)
	{
		Requires.NotNullAllowStructs(key, "key");
		return Add(key, value, KeyCollisionBehavior.ThrowIfValueDifferent, Origin).Finalize(this);
	}

	public ImmutableDictionary<TKey, TValue> AddRange([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })] IEnumerable<KeyValuePair<TKey, TValue>> pairs)
	{
		Requires.NotNull(pairs, "pairs");
		return AddRange(pairs, avoidToHashMap: false);
	}

	public ImmutableDictionary<TKey, TValue> SetItem(TKey key, TValue value)
	{
		Requires.NotNullAllowStructs(key, "key");
		return Add(key, value, KeyCollisionBehavior.SetValue, Origin).Finalize(this);
	}

	public ImmutableDictionary<TKey, TValue> SetItems([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })] IEnumerable<KeyValuePair<TKey, TValue>> items)
	{
		Requires.NotNull(items, "items");
		return AddRange(items, Origin, KeyCollisionBehavior.SetValue).Finalize(this);
	}

	public ImmutableDictionary<TKey, TValue> Remove(TKey key)
	{
		Requires.NotNullAllowStructs(key, "key");
		return Remove(key, Origin).Finalize(this);
	}

	public ImmutableDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys)
	{
		Requires.NotNull(keys, "keys");
		int num = _count;
		SortedInt32KeyNode<HashBucket> sortedInt32KeyNode = _root;
		foreach (TKey key in keys)
		{
			int hashCode = KeyComparer.GetHashCode(key);
			if (sortedInt32KeyNode.TryGetValue(hashCode, out var value))
			{
				OperationResult result;
				HashBucket newBucket = value.Remove(key, _comparers.KeyOnlyComparer, out result);
				sortedInt32KeyNode = UpdateRoot(sortedInt32KeyNode, hashCode, newBucket, _comparers.HashBucketEqualityComparer);
				if (result == OperationResult.SizeChanged)
				{
					num--;
				}
			}
		}
		return Wrap(sortedInt32KeyNode, num);
	}

	public bool ContainsKey(TKey key)
	{
		Requires.NotNullAllowStructs(key, "key");
		return ContainsKey(key, Origin);
	}

	public bool Contains([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1, 1 })] KeyValuePair<TKey, TValue> pair)
	{
		return Contains(pair, Origin);
	}

	public bool TryGetValue(TKey key, [_003C6723b510_002D2ae0_002D4796_002Dbe1b_002D098bdaf7a574_003EMaybeNullWhen(false)] out TValue value)
	{
		Requires.NotNullAllowStructs(key, "key");
		return TryGetValue(key, Origin, out value);
	}

	public bool TryGetKey(TKey equalKey, out TKey actualKey)
	{
		Requires.NotNullAllowStructs(equalKey, "equalKey");
		return TryGetKey(equalKey, Origin, out actualKey);
	}

	public ImmutableDictionary<TKey, TValue> WithComparers([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> keyComparer, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TValue> valueComparer)
	{
		if (keyComparer == null)
		{
			keyComparer = EqualityComparer<TKey>.Default;
		}
		if (valueComparer == null)
		{
			valueComparer = EqualityComparer<TValue>.Default;
		}
		if (KeyComparer == keyComparer)
		{
			if (ValueComparer == valueComparer)
			{
				return this;
			}
			Comparers comparers = _comparers.WithValueComparer(valueComparer);
			return new ImmutableDictionary<TKey, TValue>(_root, comparers, _count);
		}
		Comparers comparers2 = Comparers.Get(keyComparer, valueComparer);
		ImmutableDictionary<TKey, TValue> immutableDictionary = new ImmutableDictionary<TKey, TValue>(comparers2);
		return immutableDictionary.AddRange(this, avoidToHashMap: true);
	}

	public ImmutableDictionary<TKey, TValue> WithComparers([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> keyComparer)
	{
		return WithComparers(keyComparer, _comparers.ValueComparer);
	}

	public bool ContainsValue(TValue value)
	{
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<TKey, TValue> current = enumerator.Current;
				if (ValueComparer.Equals(value, current.Value))
				{
					return true;
				}
			}
		}
		return false;
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
	public Enumerator GetEnumerator()
	{
		return new Enumerator(_root);
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Add(TKey key, TValue value)
	{
		return Add(key, value);
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItem(TKey key, TValue value)
	{
		return SetItem(key, value);
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
	{
		return SetItems(items);
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
	{
		return AddRange(pairs);
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.RemoveRange(IEnumerable<TKey> keys)
	{
		return RemoveRange(keys);
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Remove(TKey key)
	{
		return Remove(key);
	}

	void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
	{
		throw new NotSupportedException();
	}

	bool IDictionary<TKey, TValue>.Remove(TKey key)
	{
		throw new NotSupportedException();
	}

	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
	{
		throw new NotSupportedException();
	}

	void ICollection<KeyValuePair<TKey, TValue>>.Clear()
	{
		throw new NotSupportedException();
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
	{
		throw new NotSupportedException();
	}

	void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		Requires.NotNull(array, "array");
		Requires.Range(arrayIndex >= 0, "arrayIndex");
		Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<TKey, TValue> current = enumerator.Current;
			array[arrayIndex++] = current;
		}
	}

	void IDictionary.Add(object key, object value)
	{
		throw new NotSupportedException();
	}

	bool IDictionary.Contains(object key)
	{
		return ContainsKey((TKey)key);
	}

	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return new DictionaryEnumerator<TKey, TValue>(GetEnumerator());
	}

	void IDictionary.Remove(object key)
	{
		throw new NotSupportedException();
	}

	void IDictionary.Clear()
	{
		throw new NotSupportedException();
	}

	void ICollection.CopyTo(Array array, int arrayIndex)
	{
		Requires.NotNull(array, "array");
		Requires.Range(arrayIndex >= 0, "arrayIndex");
		Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<TKey, TValue> current = enumerator.Current;
			array.SetValue(new DictionaryEntry(current.Key, current.Value), arrayIndex++);
		}
	}

	IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
	{
		if (!IsEmpty)
		{
			return GetEnumerator();
		}
		return Enumerable.Empty<KeyValuePair<TKey, TValue>>().GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private static ImmutableDictionary<TKey, TValue> EmptyWithComparers(Comparers comparers)
	{
		Requires.NotNull(comparers, "comparers");
		if (Empty._comparers != comparers)
		{
			return new ImmutableDictionary<TKey, TValue>(comparers);
		}
		return Empty;
	}

	private static bool TryCastToImmutableMap(IEnumerable<KeyValuePair<TKey, TValue>> sequence, [_003C0e75b368_002Dd12e_002D4aa3_002D887e_002D84376d2be54f_003ENotNullWhen(true)] out ImmutableDictionary<TKey, TValue> other)
	{
		other = sequence as ImmutableDictionary<TKey, TValue>;
		if (other != null)
		{
			return true;
		}
		if (sequence is Builder builder)
		{
			other = builder.ToImmutable();
			return true;
		}
		return false;
	}

	private static bool ContainsKey(TKey key, MutationInput origin)
	{
		int hashCode = origin.KeyComparer.GetHashCode(key);
		TValue value2;
		if (origin.Root.TryGetValue(hashCode, out var value))
		{
			return value.TryGetValue(key, origin.Comparers, out value2);
		}
		return false;
	}

	private static bool Contains(KeyValuePair<TKey, TValue> keyValuePair, MutationInput origin)
	{
		int hashCode = origin.KeyComparer.GetHashCode(keyValuePair.Key);
		if (origin.Root.TryGetValue(hashCode, out var value))
		{
			if (value.TryGetValue(keyValuePair.Key, origin.Comparers, out var value2))
			{
				return origin.ValueComparer.Equals(value2, keyValuePair.Value);
			}
			return false;
		}
		return false;
	}

	private static bool TryGetValue(TKey key, MutationInput origin, [_003C6723b510_002D2ae0_002D4796_002Dbe1b_002D098bdaf7a574_003EMaybeNullWhen(false)] out TValue value)
	{
		int hashCode = origin.KeyComparer.GetHashCode(key);
		if (origin.Root.TryGetValue(hashCode, out var value2))
		{
			return value2.TryGetValue(key, origin.Comparers, out value);
		}
		value = default(TValue);
		return false;
	}

	private static bool TryGetKey(TKey equalKey, MutationInput origin, out TKey actualKey)
	{
		int hashCode = origin.KeyComparer.GetHashCode(equalKey);
		if (origin.Root.TryGetValue(hashCode, out var value))
		{
			return value.TryGetKey(equalKey, origin.Comparers, out actualKey);
		}
		actualKey = equalKey;
		return false;
	}

	private static MutationResult Add(TKey key, TValue value, KeyCollisionBehavior behavior, MutationInput origin)
	{
		Requires.NotNullAllowStructs(key, "key");
		int hashCode = origin.KeyComparer.GetHashCode(key);
		OperationResult result;
		HashBucket newBucket = origin.Root.GetValueOrDefault(hashCode).Add(key, value, origin.KeyOnlyComparer, origin.ValueComparer, behavior, out result);
		if (result == OperationResult.NoChangeRequired)
		{
			return new MutationResult(origin);
		}
		SortedInt32KeyNode<HashBucket> root = UpdateRoot(origin.Root, hashCode, newBucket, origin.HashBucketComparer);
		return new MutationResult(root, (result == OperationResult.SizeChanged) ? 1 : 0);
	}

	private static MutationResult AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items, MutationInput origin, KeyCollisionBehavior collisionBehavior = KeyCollisionBehavior.ThrowIfValueDifferent)
	{
		Requires.NotNull(items, "items");
		int num = 0;
		SortedInt32KeyNode<HashBucket> sortedInt32KeyNode = origin.Root;
		foreach (KeyValuePair<TKey, TValue> item in items)
		{
			int hashCode = origin.KeyComparer.GetHashCode(item.Key);
			OperationResult result;
			HashBucket newBucket = sortedInt32KeyNode.GetValueOrDefault(hashCode).Add(item.Key, item.Value, origin.KeyOnlyComparer, origin.ValueComparer, collisionBehavior, out result);
			sortedInt32KeyNode = UpdateRoot(sortedInt32KeyNode, hashCode, newBucket, origin.HashBucketComparer);
			if (result == OperationResult.SizeChanged)
			{
				num++;
			}
		}
		return new MutationResult(sortedInt32KeyNode, num);
	}

	private static MutationResult Remove(TKey key, MutationInput origin)
	{
		int hashCode = origin.KeyComparer.GetHashCode(key);
		if (origin.Root.TryGetValue(hashCode, out var value))
		{
			OperationResult result;
			SortedInt32KeyNode<HashBucket> root = UpdateRoot(origin.Root, hashCode, value.Remove(key, origin.KeyOnlyComparer, out result), origin.HashBucketComparer);
			return new MutationResult(root, (result == OperationResult.SizeChanged) ? (-1) : 0);
		}
		return new MutationResult(origin);
	}

	private static SortedInt32KeyNode<HashBucket> UpdateRoot(SortedInt32KeyNode<HashBucket> root, int hashCode, HashBucket newBucket, IEqualityComparer<HashBucket> hashBucketComparer)
	{
		bool mutated;
		if (newBucket.IsEmpty)
		{
			return root.Remove(hashCode, out mutated);
		}
		bool replacedExistingValue;
		return root.SetItem(hashCode, newBucket, hashBucketComparer, out replacedExistingValue, out mutated);
	}

	private static ImmutableDictionary<TKey, TValue> Wrap(SortedInt32KeyNode<HashBucket> root, Comparers comparers, int count)
	{
		Requires.NotNull(root, "root");
		Requires.NotNull(comparers, "comparers");
		Requires.Range(count >= 0, "count");
		return new ImmutableDictionary<TKey, TValue>(root, comparers, count);
	}

	private ImmutableDictionary<TKey, TValue> Wrap(SortedInt32KeyNode<HashBucket> root, int adjustedCountIfDifferentRoot)
	{
		if (root == null)
		{
			return Clear();
		}
		if (_root != root)
		{
			if (!root.IsEmpty)
			{
				return new ImmutableDictionary<TKey, TValue>(root, _comparers, adjustedCountIfDifferentRoot);
			}
			return Clear();
		}
		return this;
	}

	private ImmutableDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs, bool avoidToHashMap)
	{
		Requires.NotNull(pairs, "pairs");
		if (IsEmpty && !avoidToHashMap && TryCastToImmutableMap(pairs, out var other))
		{
			return other.WithComparers(KeyComparer, ValueComparer);
		}
		return AddRange(pairs, Origin).Finalize(this);
	}
}
