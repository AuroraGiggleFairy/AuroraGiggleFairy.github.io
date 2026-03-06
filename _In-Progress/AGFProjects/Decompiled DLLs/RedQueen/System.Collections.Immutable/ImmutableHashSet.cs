using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

[DebuggerTypeProxy(typeof(ImmutableEnumerableDebuggerProxy<>))]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
[DebuggerDisplay("Count = {Count}")]
internal sealed class ImmutableHashSet<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T> : IImmutableSet<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, IHashKeyCollection<T>, ICollection<T>, ISet<T>, ICollection, IStrongEnumerable<T, ImmutableHashSet<T>.Enumerator>
{
	private class HashBucketByValueEqualityComparer : IEqualityComparer<HashBucket>
	{
		private static readonly IEqualityComparer<HashBucket> s_defaultInstance = new HashBucketByValueEqualityComparer(EqualityComparer<T>.Default);

		private readonly IEqualityComparer<T> _valueComparer;

		internal static IEqualityComparer<HashBucket> DefaultInstance => s_defaultInstance;

		internal HashBucketByValueEqualityComparer(IEqualityComparer<T> valueComparer)
		{
			Requires.NotNull(valueComparer, "valueComparer");
			_valueComparer = valueComparer;
		}

		public bool Equals(HashBucket x, HashBucket y)
		{
			return x.EqualsByValue(y, _valueComparer);
		}

		public int GetHashCode(HashBucket obj)
		{
			throw new NotSupportedException();
		}
	}

	private class HashBucketByRefEqualityComparer : IEqualityComparer<HashBucket>
	{
		private static readonly IEqualityComparer<HashBucket> s_defaultInstance = new HashBucketByRefEqualityComparer();

		internal static IEqualityComparer<HashBucket> DefaultInstance => s_defaultInstance;

		private HashBucketByRefEqualityComparer()
		{
		}

		public bool Equals(HashBucket x, HashBucket y)
		{
			return x.EqualsByRef(y);
		}

		public int GetHashCode(HashBucket obj)
		{
			throw new NotSupportedException();
		}
	}

	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
	[DebuggerDisplay("Count = {Count}")]
	public sealed class Builder : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, ISet<T>, ICollection<T>
	{
		private SortedInt32KeyNode<HashBucket> _root = SortedInt32KeyNode<HashBucket>.EmptyNode;

		private IEqualityComparer<T> _equalityComparer;

		private readonly IEqualityComparer<HashBucket> _hashBucketEqualityComparer;

		private int _count;

		private ImmutableHashSet<T> _immutable;

		private int _version;

		public int Count => _count;

		bool ICollection<T>.IsReadOnly => false;

		public IEqualityComparer<T> KeyComparer
		{
			get
			{
				return _equalityComparer;
			}
			set
			{
				Requires.NotNull(value, "value");
				if (value != _equalityComparer)
				{
					MutationResult mutationResult = ImmutableHashSet<T>.Union((IEnumerable<T>)this, new MutationInput(SortedInt32KeyNode<HashBucket>.EmptyNode, value, _hashBucketEqualityComparer, 0));
					_immutable = null;
					_equalityComparer = value;
					Root = mutationResult.Root;
					_count = mutationResult.Count;
				}
			}
		}

		internal int Version => _version;

		[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
		private MutationInput Origin => new MutationInput(Root, _equalityComparer, _hashBucketEqualityComparer, _count);

		[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 0 })]
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

		internal Builder(ImmutableHashSet<T> set)
		{
			Requires.NotNull(set, "set");
			_root = set._root;
			_count = set._count;
			_equalityComparer = set._equalityComparer;
			_hashBucketEqualityComparer = set._hashBucketEqualityComparer;
			_immutable = set;
		}

		[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
		public Enumerator GetEnumerator()
		{
			return new Enumerator(_root, this);
		}

		public ImmutableHashSet<T> ToImmutable()
		{
			if (_immutable == null)
			{
				_immutable = ImmutableHashSet<T>.Wrap(_root, _equalityComparer, _count);
			}
			return _immutable;
		}

		public bool TryGetValue(T equalValue, out T actualValue)
		{
			int key = ((equalValue != null) ? _equalityComparer.GetHashCode(equalValue) : 0);
			if (_root.TryGetValue(key, out var value))
			{
				return value.TryExchange(equalValue, _equalityComparer, out actualValue);
			}
			actualValue = equalValue;
			return false;
		}

		public bool Add(T item)
		{
			MutationResult result = ImmutableHashSet<T>.Add(item, Origin);
			Apply(result);
			return result.Count != 0;
		}

		public bool Remove(T item)
		{
			MutationResult result = ImmutableHashSet<T>.Remove(item, Origin);
			Apply(result);
			return result.Count != 0;
		}

		public bool Contains(T item)
		{
			return ImmutableHashSet<T>.Contains(item, Origin);
		}

		public void Clear()
		{
			_count = 0;
			Root = SortedInt32KeyNode<HashBucket>.EmptyNode;
		}

		public void ExceptWith(IEnumerable<T> other)
		{
			MutationResult result = ImmutableHashSet<T>.Except(other, _equalityComparer, _hashBucketEqualityComparer, _root);
			Apply(result);
		}

		public void IntersectWith(IEnumerable<T> other)
		{
			MutationResult result = ImmutableHashSet<T>.Intersect(other, Origin);
			Apply(result);
		}

		public bool IsProperSubsetOf(IEnumerable<T> other)
		{
			return ImmutableHashSet<T>.IsProperSubsetOf(other, Origin);
		}

		public bool IsProperSupersetOf(IEnumerable<T> other)
		{
			return ImmutableHashSet<T>.IsProperSupersetOf(other, Origin);
		}

		public bool IsSubsetOf(IEnumerable<T> other)
		{
			return ImmutableHashSet<T>.IsSubsetOf(other, Origin);
		}

		public bool IsSupersetOf(IEnumerable<T> other)
		{
			return ImmutableHashSet<T>.IsSupersetOf(other, Origin);
		}

		public bool Overlaps(IEnumerable<T> other)
		{
			return ImmutableHashSet<T>.Overlaps(other, Origin);
		}

		public bool SetEquals(IEnumerable<T> other)
		{
			if (this == other)
			{
				return true;
			}
			return ImmutableHashSet<T>.SetEquals(other, Origin);
		}

		public void SymmetricExceptWith(IEnumerable<T> other)
		{
			MutationResult result = ImmutableHashSet<T>.SymmetricExcept(other, Origin);
			Apply(result);
		}

		public void UnionWith(IEnumerable<T> other)
		{
			MutationResult result = ImmutableHashSet<T>.Union(other, Origin);
			Apply(result);
		}

		void ICollection<T>.Add(T item)
		{
			Add(item);
		}

		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			Requires.NotNull(array, "array");
			Requires.Range(arrayIndex >= 0, "arrayIndex");
			Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				array[arrayIndex++] = current;
			}
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private void Apply(MutationResult result)
		{
			Root = result.Root;
			if (result.CountType == CountType.Adjustment)
			{
				_count += result.Count;
			}
			else
			{
				_count = result.Count;
			}
		}
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
	public struct Enumerator([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 0 })] SortedInt32KeyNode<HashBucket> root, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 0 })] Builder builder = null) : IEnumerator<T>, IDisposable, IEnumerator, IStrongEnumerator<T>
	{
		private readonly Builder _builder = builder;

		private SortedInt32KeyNode<HashBucket>.Enumerator _mapEnumerator = new SortedInt32KeyNode<HashBucket>.Enumerator(root);

		private HashBucket.Enumerator _bucketEnumerator = default(HashBucket.Enumerator);

		private int _enumeratingBuilderVersion = builder?.Version ?? (-1);

		[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)]
		public T Current
		{
			[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
			get
			{
				_mapEnumerator.ThrowIfDisposed();
				return _bucketEnumerator.Current;
			}
		}

		[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)]
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
	internal enum OperationResult
	{
		SizeChanged,
		NoChangeRequired
	}

	[_003C213e6825_002D2666_002D4c32_002Dad9b_002D2809d47857fd_003EIsReadOnly]
	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
	internal struct HashBucket(T firstElement, ImmutableList<T>.Node additionalElements = null)
	{
		internal struct Enumerator(HashBucket bucket) : IEnumerator<T>, IDisposable, IEnumerator
		{
			private enum Position
			{
				BeforeFirst,
				First,
				Additional,
				End
			}

			private readonly HashBucket _bucket = bucket;

			private bool _disposed = false;

			private Position _currentPosition = Position.BeforeFirst;

			private ImmutableList<T>.Enumerator _additionalEnumerator = default(ImmutableList<T>.Enumerator);

			[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)]
			object IEnumerator.Current => Current;

			[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)]
			public T Current
			{
				[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
				get
				{
					ThrowIfDisposed();
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
				ThrowIfDisposed();
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
					_additionalEnumerator = new ImmutableList<T>.Enumerator(_bucket._additionalElements);
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
				ThrowIfDisposed();
				_additionalEnumerator.Dispose();
				_currentPosition = Position.BeforeFirst;
			}

			public void Dispose()
			{
				_disposed = true;
				_additionalEnumerator.Dispose();
			}

			private void ThrowIfDisposed()
			{
				if (_disposed)
				{
					Requires.FailObjectDisposed(this);
				}
			}
		}

		private readonly T _firstValue = firstElement;

		private readonly ImmutableList<T>.Node _additionalElements = additionalElements ?? ImmutableList<T>.Node.EmptyNode;

		internal bool IsEmpty => _additionalElements == null;

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
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

		internal bool EqualsByRef(HashBucket other)
		{
			if ((object)_firstValue == (object)other._firstValue)
			{
				return _additionalElements == other._additionalElements;
			}
			return false;
		}

		internal bool EqualsByValue(HashBucket other, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] IEqualityComparer<T> valueComparer)
		{
			if (valueComparer.Equals(_firstValue, other._firstValue))
			{
				return _additionalElements == other._additionalElements;
			}
			return false;
		}

		internal HashBucket Add([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] T value, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] IEqualityComparer<T> valueComparer, out OperationResult result)
		{
			if (IsEmpty)
			{
				result = OperationResult.SizeChanged;
				return new HashBucket(value);
			}
			if (valueComparer.Equals(value, _firstValue) || _additionalElements.IndexOf(value, valueComparer) >= 0)
			{
				result = OperationResult.NoChangeRequired;
				return this;
			}
			result = OperationResult.SizeChanged;
			return new HashBucket(_firstValue, _additionalElements.Add(value));
		}

		[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
		internal bool Contains(T value, IEqualityComparer<T> valueComparer)
		{
			if (IsEmpty)
			{
				return false;
			}
			if (!valueComparer.Equals(value, _firstValue))
			{
				return _additionalElements.IndexOf(value, valueComparer) >= 0;
			}
			return true;
		}

		[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
		internal bool TryExchange(T value, IEqualityComparer<T> valueComparer, out T existingValue)
		{
			if (!IsEmpty)
			{
				if (valueComparer.Equals(value, _firstValue))
				{
					existingValue = _firstValue;
					return true;
				}
				int num = _additionalElements.IndexOf(value, valueComparer);
				if (num >= 0)
				{
					existingValue = _additionalElements.ItemRef(num);
					return true;
				}
			}
			existingValue = value;
			return false;
		}

		internal HashBucket Remove([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] T value, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] IEqualityComparer<T> equalityComparer, out OperationResult result)
		{
			if (IsEmpty)
			{
				result = OperationResult.NoChangeRequired;
				return this;
			}
			if (equalityComparer.Equals(_firstValue, value))
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
			int num = _additionalElements.IndexOf(value, equalityComparer);
			if (num < 0)
			{
				result = OperationResult.NoChangeRequired;
				return this;
			}
			result = OperationResult.SizeChanged;
			return new HashBucket(_firstValue, _additionalElements.RemoveAt(num));
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

		private readonly IEqualityComparer<T> _equalityComparer;

		private readonly int _count;

		private readonly IEqualityComparer<HashBucket> _hashBucketEqualityComparer;

		internal SortedInt32KeyNode<HashBucket> Root => _root;

		internal IEqualityComparer<T> EqualityComparer => _equalityComparer;

		internal int Count => _count;

		internal IEqualityComparer<HashBucket> HashBucketEqualityComparer => _hashBucketEqualityComparer;

		internal MutationInput(ImmutableHashSet<T> set)
		{
			Requires.NotNull(set, "set");
			_root = set._root;
			_equalityComparer = set._equalityComparer;
			_count = set._count;
			_hashBucketEqualityComparer = set._hashBucketEqualityComparer;
		}

		internal MutationInput(SortedInt32KeyNode<HashBucket> root, IEqualityComparer<T> equalityComparer, IEqualityComparer<HashBucket> hashBucketEqualityComparer, int count)
		{
			Requires.NotNull(root, "root");
			Requires.NotNull(equalityComparer, "equalityComparer");
			Requires.Range(count >= 0, "count");
			Requires.NotNull(hashBucketEqualityComparer, "hashBucketEqualityComparer");
			_root = root;
			_equalityComparer = equalityComparer;
			_count = count;
			_hashBucketEqualityComparer = hashBucketEqualityComparer;
		}
	}

	private enum CountType
	{
		Adjustment,
		FinalValue
	}

	[_003C213e6825_002D2666_002D4c32_002Dad9b_002D2809d47857fd_003EIsReadOnly]
	private struct MutationResult
	{
		private readonly SortedInt32KeyNode<HashBucket> _root;

		private readonly int _count;

		private readonly CountType _countType;

		internal SortedInt32KeyNode<HashBucket> Root => _root;

		internal int Count => _count;

		internal CountType CountType => _countType;

		internal MutationResult(SortedInt32KeyNode<HashBucket> root, int count, CountType countType = CountType.Adjustment)
		{
			Requires.NotNull(root, "root");
			_root = root;
			_count = count;
			_countType = countType;
		}

		internal ImmutableHashSet<T> Finalize(ImmutableHashSet<T> priorSet)
		{
			Requires.NotNull(priorSet, "priorSet");
			int num = Count;
			if (CountType == CountType.Adjustment)
			{
				num += priorSet._count;
			}
			return priorSet.Wrap(Root, num);
		}
	}

	[_003C213e6825_002D2666_002D4c32_002Dad9b_002D2809d47857fd_003EIsReadOnly]
	private struct NodeEnumerable : IEnumerable<T>, IEnumerable
	{
		private readonly SortedInt32KeyNode<HashBucket> _root;

		internal NodeEnumerable(SortedInt32KeyNode<HashBucket> root)
		{
			Requires.NotNull(root, "root");
			_root = root;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(_root);
		}

		[ExcludeFromCodeCoverage]
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return GetEnumerator();
		}

		[ExcludeFromCodeCoverage]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public static readonly ImmutableHashSet<T> Empty = new ImmutableHashSet<T>(SortedInt32KeyNode<HashBucket>.EmptyNode, EqualityComparer<T>.Default, 0);

	private static readonly Action<KeyValuePair<int, HashBucket>> s_FreezeBucketAction = delegate(KeyValuePair<int, HashBucket> kv)
	{
		kv.Value.Freeze();
	};

	private readonly IEqualityComparer<T> _equalityComparer;

	private readonly int _count;

	private readonly SortedInt32KeyNode<HashBucket> _root;

	private readonly IEqualityComparer<HashBucket> _hashBucketEqualityComparer;

	public int Count => _count;

	public bool IsEmpty => Count == 0;

	public IEqualityComparer<T> KeyComparer => _equalityComparer;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	object ICollection.SyncRoot => this;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	bool ICollection.IsSynchronized => true;

	internal IBinaryTree Root => _root;

	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
	private MutationInput Origin => new MutationInput(this);

	bool ICollection<T>.IsReadOnly => true;

	internal ImmutableHashSet(IEqualityComparer<T> equalityComparer)
		: this(SortedInt32KeyNode<HashBucket>.EmptyNode, equalityComparer, 0)
	{
	}

	private ImmutableHashSet(SortedInt32KeyNode<HashBucket> root, IEqualityComparer<T> equalityComparer, int count)
	{
		Requires.NotNull(root, "root");
		Requires.NotNull(equalityComparer, "equalityComparer");
		root.Freeze(s_FreezeBucketAction);
		_root = root;
		_count = count;
		_equalityComparer = equalityComparer;
		_hashBucketEqualityComparer = GetHashBucketEqualityComparer(equalityComparer);
	}

	public ImmutableHashSet<T> Clear()
	{
		if (!IsEmpty)
		{
			return Empty.WithComparer(_equalityComparer);
		}
		return this;
	}

	IImmutableSet<T> IImmutableSet<T>.Clear()
	{
		return Clear();
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0 })]
	public Builder ToBuilder()
	{
		return new Builder(this);
	}

	public ImmutableHashSet<T> Add(T item)
	{
		return Add(item, Origin).Finalize(this);
	}

	public ImmutableHashSet<T> Remove(T item)
	{
		return Remove(item, Origin).Finalize(this);
	}

	public bool TryGetValue(T equalValue, out T actualValue)
	{
		int key = ((equalValue != null) ? _equalityComparer.GetHashCode(equalValue) : 0);
		if (_root.TryGetValue(key, out var value))
		{
			return value.TryExchange(equalValue, _equalityComparer, out actualValue);
		}
		actualValue = equalValue;
		return false;
	}

	public ImmutableHashSet<T> Union(IEnumerable<T> other)
	{
		Requires.NotNull(other, "other");
		return Union(other, avoidWithComparer: false);
	}

	public ImmutableHashSet<T> Intersect(IEnumerable<T> other)
	{
		Requires.NotNull(other, "other");
		return Intersect(other, Origin).Finalize(this);
	}

	public ImmutableHashSet<T> Except(IEnumerable<T> other)
	{
		Requires.NotNull(other, "other");
		return Except(other, _equalityComparer, _hashBucketEqualityComparer, _root).Finalize(this);
	}

	public ImmutableHashSet<T> SymmetricExcept(IEnumerable<T> other)
	{
		Requires.NotNull(other, "other");
		return SymmetricExcept(other, Origin).Finalize(this);
	}

	public bool SetEquals(IEnumerable<T> other)
	{
		Requires.NotNull(other, "other");
		if (this == other)
		{
			return true;
		}
		return SetEquals(other, Origin);
	}

	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		Requires.NotNull(other, "other");
		return IsProperSubsetOf(other, Origin);
	}

	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		Requires.NotNull(other, "other");
		return IsProperSupersetOf(other, Origin);
	}

	public bool IsSubsetOf(IEnumerable<T> other)
	{
		Requires.NotNull(other, "other");
		return IsSubsetOf(other, Origin);
	}

	public bool IsSupersetOf(IEnumerable<T> other)
	{
		Requires.NotNull(other, "other");
		return IsSupersetOf(other, Origin);
	}

	public bool Overlaps(IEnumerable<T> other)
	{
		Requires.NotNull(other, "other");
		return Overlaps(other, Origin);
	}

	IImmutableSet<T> IImmutableSet<T>.Add(T item)
	{
		return Add(item);
	}

	IImmutableSet<T> IImmutableSet<T>.Remove(T item)
	{
		return Remove(item);
	}

	IImmutableSet<T> IImmutableSet<T>.Union(IEnumerable<T> other)
	{
		return Union(other);
	}

	IImmutableSet<T> IImmutableSet<T>.Intersect(IEnumerable<T> other)
	{
		return Intersect(other);
	}

	IImmutableSet<T> IImmutableSet<T>.Except(IEnumerable<T> other)
	{
		return Except(other);
	}

	IImmutableSet<T> IImmutableSet<T>.SymmetricExcept(IEnumerable<T> other)
	{
		return SymmetricExcept(other);
	}

	public bool Contains(T item)
	{
		return Contains(item, Origin);
	}

	public ImmutableHashSet<T> WithComparer([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer)
	{
		if (equalityComparer == null)
		{
			equalityComparer = EqualityComparer<T>.Default;
		}
		if (equalityComparer == _equalityComparer)
		{
			return this;
		}
		ImmutableHashSet<T> immutableHashSet = new ImmutableHashSet<T>(equalityComparer);
		return immutableHashSet.Union(this, avoidWithComparer: true);
	}

	bool ISet<T>.Add(T item)
	{
		throw new NotSupportedException();
	}

	void ISet<T>.ExceptWith(IEnumerable<T> other)
	{
		throw new NotSupportedException();
	}

	void ISet<T>.IntersectWith(IEnumerable<T> other)
	{
		throw new NotSupportedException();
	}

	void ISet<T>.SymmetricExceptWith(IEnumerable<T> other)
	{
		throw new NotSupportedException();
	}

	void ISet<T>.UnionWith(IEnumerable<T> other)
	{
		throw new NotSupportedException();
	}

	void ICollection<T>.CopyTo(T[] array, int arrayIndex)
	{
		Requires.NotNull(array, "array");
		Requires.Range(arrayIndex >= 0, "arrayIndex");
		Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			array[arrayIndex++] = current;
		}
	}

	void ICollection<T>.Add(T item)
	{
		throw new NotSupportedException();
	}

	void ICollection<T>.Clear()
	{
		throw new NotSupportedException();
	}

	bool ICollection<T>.Remove(T item)
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
			T current = enumerator.Current;
			array.SetValue(current, arrayIndex++);
		}
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
	public Enumerator GetEnumerator()
	{
		return new Enumerator(_root);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		if (!IsEmpty)
		{
			return GetEnumerator();
		}
		return Enumerable.Empty<T>().GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private static bool IsSupersetOf(IEnumerable<T> other, MutationInput origin)
	{
		Requires.NotNull(other, "other");
		foreach (T item in other.GetEnumerableDisposable<T, Enumerator>())
		{
			if (!Contains(item, origin))
			{
				return false;
			}
		}
		return true;
	}

	private static MutationResult Add(T item, MutationInput origin)
	{
		int num = ((item != null) ? origin.EqualityComparer.GetHashCode(item) : 0);
		OperationResult result;
		HashBucket newBucket = origin.Root.GetValueOrDefault(num).Add(item, origin.EqualityComparer, out result);
		if (result == OperationResult.NoChangeRequired)
		{
			return new MutationResult(origin.Root, 0);
		}
		SortedInt32KeyNode<HashBucket> root = UpdateRoot(origin.Root, num, origin.HashBucketEqualityComparer, newBucket);
		return new MutationResult(root, 1);
	}

	private static MutationResult Remove(T item, MutationInput origin)
	{
		OperationResult result = OperationResult.NoChangeRequired;
		int num = ((item != null) ? origin.EqualityComparer.GetHashCode(item) : 0);
		SortedInt32KeyNode<HashBucket> root = origin.Root;
		if (origin.Root.TryGetValue(num, out var value))
		{
			HashBucket newBucket = value.Remove(item, origin.EqualityComparer, out result);
			if (result == OperationResult.NoChangeRequired)
			{
				return new MutationResult(origin.Root, 0);
			}
			root = UpdateRoot(origin.Root, num, origin.HashBucketEqualityComparer, newBucket);
		}
		return new MutationResult(root, (result == OperationResult.SizeChanged) ? (-1) : 0);
	}

	private static bool Contains(T item, MutationInput origin)
	{
		int key = ((item != null) ? origin.EqualityComparer.GetHashCode(item) : 0);
		if (origin.Root.TryGetValue(key, out var value))
		{
			return value.Contains(item, origin.EqualityComparer);
		}
		return false;
	}

	private static MutationResult Union(IEnumerable<T> other, MutationInput origin)
	{
		Requires.NotNull(other, "other");
		int num = 0;
		SortedInt32KeyNode<HashBucket> sortedInt32KeyNode = origin.Root;
		foreach (T item in other.GetEnumerableDisposable<T, Enumerator>())
		{
			int num2 = ((item != null) ? origin.EqualityComparer.GetHashCode(item) : 0);
			OperationResult result;
			HashBucket newBucket = sortedInt32KeyNode.GetValueOrDefault(num2).Add(item, origin.EqualityComparer, out result);
			if (result == OperationResult.SizeChanged)
			{
				sortedInt32KeyNode = UpdateRoot(sortedInt32KeyNode, num2, origin.HashBucketEqualityComparer, newBucket);
				num++;
			}
		}
		return new MutationResult(sortedInt32KeyNode, num);
	}

	private static bool Overlaps(IEnumerable<T> other, MutationInput origin)
	{
		Requires.NotNull(other, "other");
		if (origin.Root.IsEmpty)
		{
			return false;
		}
		foreach (T item in other.GetEnumerableDisposable<T, Enumerator>())
		{
			if (Contains(item, origin))
			{
				return true;
			}
		}
		return false;
	}

	private static bool SetEquals(IEnumerable<T> other, MutationInput origin)
	{
		Requires.NotNull(other, "other");
		HashSet<T> hashSet = new HashSet<T>(other, origin.EqualityComparer);
		if (origin.Count != hashSet.Count)
		{
			return false;
		}
		foreach (T item in hashSet)
		{
			if (!Contains(item, origin))
			{
				return false;
			}
		}
		return true;
	}

	private static SortedInt32KeyNode<HashBucket> UpdateRoot(SortedInt32KeyNode<HashBucket> root, int hashCode, IEqualityComparer<HashBucket> hashBucketEqualityComparer, HashBucket newBucket)
	{
		bool mutated;
		if (newBucket.IsEmpty)
		{
			return root.Remove(hashCode, out mutated);
		}
		bool replacedExistingValue;
		return root.SetItem(hashCode, newBucket, hashBucketEqualityComparer, out replacedExistingValue, out mutated);
	}

	private static MutationResult Intersect(IEnumerable<T> other, MutationInput origin)
	{
		Requires.NotNull(other, "other");
		SortedInt32KeyNode<HashBucket> root = SortedInt32KeyNode<HashBucket>.EmptyNode;
		int num = 0;
		foreach (T item in other.GetEnumerableDisposable<T, Enumerator>())
		{
			if (Contains(item, origin))
			{
				MutationResult mutationResult = Add(item, new MutationInput(root, origin.EqualityComparer, origin.HashBucketEqualityComparer, num));
				root = mutationResult.Root;
				num += mutationResult.Count;
			}
		}
		return new MutationResult(root, num, CountType.FinalValue);
	}

	private static MutationResult Except(IEnumerable<T> other, IEqualityComparer<T> equalityComparer, IEqualityComparer<HashBucket> hashBucketEqualityComparer, SortedInt32KeyNode<HashBucket> root)
	{
		Requires.NotNull(other, "other");
		Requires.NotNull(equalityComparer, "equalityComparer");
		Requires.NotNull(root, "root");
		int num = 0;
		SortedInt32KeyNode<HashBucket> sortedInt32KeyNode = root;
		foreach (T item in other.GetEnumerableDisposable<T, Enumerator>())
		{
			int num2 = ((item != null) ? equalityComparer.GetHashCode(item) : 0);
			if (sortedInt32KeyNode.TryGetValue(num2, out var value))
			{
				OperationResult result;
				HashBucket newBucket = value.Remove(item, equalityComparer, out result);
				if (result == OperationResult.SizeChanged)
				{
					num--;
					sortedInt32KeyNode = UpdateRoot(sortedInt32KeyNode, num2, hashBucketEqualityComparer, newBucket);
				}
			}
		}
		return new MutationResult(sortedInt32KeyNode, num);
	}

	private static MutationResult SymmetricExcept(IEnumerable<T> other, MutationInput origin)
	{
		Requires.NotNull(other, "other");
		ImmutableHashSet<T> immutableHashSet = ImmutableHashSet.CreateRange(origin.EqualityComparer, other);
		int num = 0;
		SortedInt32KeyNode<HashBucket> root = SortedInt32KeyNode<HashBucket>.EmptyNode;
		foreach (T item in new NodeEnumerable(origin.Root))
		{
			if (!immutableHashSet.Contains(item))
			{
				MutationResult mutationResult = Add(item, new MutationInput(root, origin.EqualityComparer, origin.HashBucketEqualityComparer, num));
				root = mutationResult.Root;
				num += mutationResult.Count;
			}
		}
		foreach (T item2 in immutableHashSet)
		{
			if (!Contains(item2, origin))
			{
				MutationResult mutationResult2 = Add(item2, new MutationInput(root, origin.EqualityComparer, origin.HashBucketEqualityComparer, num));
				root = mutationResult2.Root;
				num += mutationResult2.Count;
			}
		}
		return new MutationResult(root, num, CountType.FinalValue);
	}

	private static bool IsProperSubsetOf(IEnumerable<T> other, MutationInput origin)
	{
		Requires.NotNull(other, "other");
		if (origin.Root.IsEmpty)
		{
			return other.Any();
		}
		HashSet<T> hashSet = new HashSet<T>(other, origin.EqualityComparer);
		if (origin.Count >= hashSet.Count)
		{
			return false;
		}
		int num = 0;
		bool flag = false;
		foreach (T item in hashSet)
		{
			if (Contains(item, origin))
			{
				num++;
			}
			else
			{
				flag = true;
			}
			if (num == origin.Count && flag)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsProperSupersetOf(IEnumerable<T> other, MutationInput origin)
	{
		Requires.NotNull(other, "other");
		if (origin.Root.IsEmpty)
		{
			return false;
		}
		int num = 0;
		foreach (T item in other.GetEnumerableDisposable<T, Enumerator>())
		{
			num++;
			if (!Contains(item, origin))
			{
				return false;
			}
		}
		return origin.Count > num;
	}

	private static bool IsSubsetOf(IEnumerable<T> other, MutationInput origin)
	{
		Requires.NotNull(other, "other");
		if (origin.Root.IsEmpty)
		{
			return true;
		}
		HashSet<T> hashSet = new HashSet<T>(other, origin.EqualityComparer);
		int num = 0;
		foreach (T item in hashSet)
		{
			if (Contains(item, origin))
			{
				num++;
			}
		}
		return num == origin.Count;
	}

	private static ImmutableHashSet<T> Wrap(SortedInt32KeyNode<HashBucket> root, IEqualityComparer<T> equalityComparer, int count)
	{
		Requires.NotNull(root, "root");
		Requires.NotNull(equalityComparer, "equalityComparer");
		Requires.Range(count >= 0, "count");
		return new ImmutableHashSet<T>(root, equalityComparer, count);
	}

	private static IEqualityComparer<HashBucket> GetHashBucketEqualityComparer(IEqualityComparer<T> valueComparer)
	{
		if (!ImmutableExtensions.IsValueType<T>())
		{
			return HashBucketByRefEqualityComparer.DefaultInstance;
		}
		if (valueComparer == EqualityComparer<T>.Default)
		{
			return HashBucketByValueEqualityComparer.DefaultInstance;
		}
		return new HashBucketByValueEqualityComparer(valueComparer);
	}

	private ImmutableHashSet<T> Wrap(SortedInt32KeyNode<HashBucket> root, int adjustedCountIfDifferentRoot)
	{
		if (root == _root)
		{
			return this;
		}
		return new ImmutableHashSet<T>(root, _equalityComparer, adjustedCountIfDifferentRoot);
	}

	private ImmutableHashSet<T> Union(IEnumerable<T> items, bool avoidWithComparer)
	{
		Requires.NotNull(items, "items");
		if (IsEmpty && !avoidWithComparer && items is ImmutableHashSet<T> immutableHashSet)
		{
			return immutableHashSet.WithComparer(KeyComparer);
		}
		return Union(items, Origin).Finalize(this);
	}
}
[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
internal static class ImmutableHashSet
{
	public static ImmutableHashSet<T> Create<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>()
	{
		return ImmutableHashSet<T>.Empty;
	}

	public static ImmutableHashSet<T> Create<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer)
	{
		return ImmutableHashSet<T>.Empty.WithComparer(equalityComparer);
	}

	public static ImmutableHashSet<T> Create<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(T item)
	{
		return ImmutableHashSet<T>.Empty.Add(item);
	}

	public static ImmutableHashSet<T> Create<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer, T item)
	{
		return ImmutableHashSet<T>.Empty.WithComparer(equalityComparer).Add(item);
	}

	public static ImmutableHashSet<T> CreateRange<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(IEnumerable<T> items)
	{
		return ImmutableHashSet<T>.Empty.Union(items);
	}

	public static ImmutableHashSet<T> CreateRange<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer, IEnumerable<T> items)
	{
		return ImmutableHashSet<T>.Empty.WithComparer(equalityComparer).Union(items);
	}

	public static ImmutableHashSet<T> Create<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(params T[] items)
	{
		return ImmutableHashSet<T>.Empty.Union(items);
	}

	public static ImmutableHashSet<T> Create<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer, params T[] items)
	{
		return ImmutableHashSet<T>.Empty.WithComparer(equalityComparer).Union(items);
	}

	public static ImmutableHashSet<T>.Builder CreateBuilder<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>()
	{
		return Create<T>().ToBuilder();
	}

	public static ImmutableHashSet<T>.Builder CreateBuilder<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer)
	{
		return Create(equalityComparer).ToBuilder();
	}

	public static ImmutableHashSet<TSource> ToImmutableHashSet<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TSource>(this IEnumerable<TSource> source, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TSource> equalityComparer)
	{
		if (source is ImmutableHashSet<TSource> immutableHashSet)
		{
			return immutableHashSet.WithComparer(equalityComparer);
		}
		return ImmutableHashSet<TSource>.Empty.WithComparer(equalityComparer).Union(source);
	}

	public static ImmutableHashSet<TSource> ToImmutableHashSet<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TSource>(this ImmutableHashSet<TSource>.Builder builder)
	{
		Requires.NotNull(builder, "builder");
		return builder.ToImmutable();
	}

	public static ImmutableHashSet<TSource> ToImmutableHashSet<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TSource>(this IEnumerable<TSource> source)
	{
		return source.ToImmutableHashSet(null);
	}
}
