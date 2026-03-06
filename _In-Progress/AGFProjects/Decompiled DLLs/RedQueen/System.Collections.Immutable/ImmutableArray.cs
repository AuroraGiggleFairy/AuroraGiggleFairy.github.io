using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace System.Collections.Immutable;

[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
internal static class ImmutableArray
{
	internal static readonly byte[] TwoElementArray = new byte[2];

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public static System.Collections.Immutable.ImmutableArray<T> Create<T>()
	{
		return System.Collections.Immutable.ImmutableArray<T>.Empty;
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public static System.Collections.Immutable.ImmutableArray<T> Create<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(T item)
	{
		T[] items = new T[1] { item };
		return new System.Collections.Immutable.ImmutableArray<T>(items);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public static System.Collections.Immutable.ImmutableArray<T> Create<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(T item1, T item2)
	{
		T[] items = new T[2] { item1, item2 };
		return new System.Collections.Immutable.ImmutableArray<T>(items);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public static System.Collections.Immutable.ImmutableArray<T> Create<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(T item1, T item2, T item3)
	{
		T[] items = new T[3] { item1, item2, item3 };
		return new System.Collections.Immutable.ImmutableArray<T>(items);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public static System.Collections.Immutable.ImmutableArray<T> Create<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(T item1, T item2, T item3, T item4)
	{
		T[] items = new T[4] { item1, item2, item3, item4 };
		return new System.Collections.Immutable.ImmutableArray<T>(items);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public static System.Collections.Immutable.ImmutableArray<T> CreateRange<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(IEnumerable<T> items)
	{
		Requires.NotNull(items, "items");
		if (items is IImmutableArray { Array: var array })
		{
			if (array == null)
			{
				throw new InvalidOperationException(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.InvalidOperationOnDefaultArray);
			}
			return new System.Collections.Immutable.ImmutableArray<T>((T[])array);
		}
		if (items.TryGetCount(out var count))
		{
			return new System.Collections.Immutable.ImmutableArray<T>(items.ToArray(count));
		}
		return new System.Collections.Immutable.ImmutableArray<T>(items.ToArray());
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public static System.Collections.Immutable.ImmutableArray<T> Create<T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] params T[] items)
	{
		if (items == null || items.Length == 0)
		{
			return System.Collections.Immutable.ImmutableArray<T>.Empty;
		}
		T[] array = new T[items.Length];
		Array.Copy(items, array, items.Length);
		return new System.Collections.Immutable.ImmutableArray<T>(array);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public static System.Collections.Immutable.ImmutableArray<T> Create<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(T[] items, int start, int length)
	{
		Requires.NotNull(items, "items");
		Requires.Range(start >= 0 && start <= items.Length, "start");
		Requires.Range(length >= 0 && start + length <= items.Length, "length");
		if (length == 0)
		{
			return Create<T>();
		}
		T[] array = new T[length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = items[start + i];
		}
		return new System.Collections.Immutable.ImmutableArray<T>(array);
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public static System.Collections.Immutable.ImmutableArray<T> Create<T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T> items, int start, int length)
	{
		Requires.Range(start >= 0 && start <= items.Length, "start");
		Requires.Range(length >= 0 && start + length <= items.Length, "length");
		if (length == 0)
		{
			return Create<T>();
		}
		if (start == 0 && length == items.Length)
		{
			return items;
		}
		T[] array = new T[length];
		Array.Copy(items.array, start, array, 0, length);
		return new System.Collections.Immutable.ImmutableArray<T>(array);
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public static System.Collections.Immutable.ImmutableArray<TResult> CreateRange<TSource, TResult>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<TSource> items, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] Func<TSource, TResult> selector)
	{
		Requires.NotNull(selector, "selector");
		int length = items.Length;
		if (length == 0)
		{
			return Create<TResult>();
		}
		TResult[] array = new TResult[length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = selector(items[i]);
		}
		return new System.Collections.Immutable.ImmutableArray<TResult>(array);
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public static System.Collections.Immutable.ImmutableArray<TResult> CreateRange<TSource, TResult>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<TSource> items, int start, int length, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] Func<TSource, TResult> selector)
	{
		int length2 = items.Length;
		Requires.Range(start >= 0 && start <= length2, "start");
		Requires.Range(length >= 0 && start + length <= length2, "length");
		Requires.NotNull(selector, "selector");
		if (length == 0)
		{
			return Create<TResult>();
		}
		TResult[] array = new TResult[length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = selector(items[i + start]);
		}
		return new System.Collections.Immutable.ImmutableArray<TResult>(array);
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public static System.Collections.Immutable.ImmutableArray<TResult> CreateRange<TSource, TArg, TResult>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<TSource> items, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] Func<TSource, TArg, TResult> selector, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] TArg arg)
	{
		Requires.NotNull(selector, "selector");
		int length = items.Length;
		if (length == 0)
		{
			return Create<TResult>();
		}
		TResult[] array = new TResult[length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = selector(items[i], arg);
		}
		return new System.Collections.Immutable.ImmutableArray<TResult>(array);
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public static System.Collections.Immutable.ImmutableArray<TResult> CreateRange<TSource, TArg, TResult>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<TSource> items, int start, int length, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] Func<TSource, TArg, TResult> selector, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] TArg arg)
	{
		int length2 = items.Length;
		Requires.Range(start >= 0 && start <= length2, "start");
		Requires.Range(length >= 0 && start + length <= length2, "length");
		Requires.NotNull(selector, "selector");
		if (length == 0)
		{
			return Create<TResult>();
		}
		TResult[] array = new TResult[length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = selector(items[i + start], arg);
		}
		return new System.Collections.Immutable.ImmutableArray<TResult>(array);
	}

	public static System.Collections.Immutable.ImmutableArray<T>.Builder CreateBuilder<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>()
	{
		return Create<T>().ToBuilder();
	}

	public static System.Collections.Immutable.ImmutableArray<T>.Builder CreateBuilder<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(int initialCapacity)
	{
		return new System.Collections.Immutable.ImmutableArray<T>.Builder(initialCapacity);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public static System.Collections.Immutable.ImmutableArray<TSource> ToImmutableArray<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TSource>(this IEnumerable<TSource> items)
	{
		if (items is System.Collections.Immutable.ImmutableArray<TSource>)
		{
			return (System.Collections.Immutable.ImmutableArray<TSource>)(object)items;
		}
		return CreateRange(items);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public static System.Collections.Immutable.ImmutableArray<TSource> ToImmutableArray<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TSource>(this System.Collections.Immutable.ImmutableArray<TSource>.Builder builder)
	{
		Requires.NotNull(builder, "builder");
		return builder.ToImmutable();
	}

	public static int BinarySearch<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> array, T value)
	{
		return Array.BinarySearch(array.array, value);
	}

	public static int BinarySearch<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> array, T value, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IComparer<T> comparer)
	{
		return Array.BinarySearch(array.array, value, comparer);
	}

	public static int BinarySearch<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> array, int index, int length, T value)
	{
		return Array.BinarySearch(array.array, index, length, value);
	}

	public static int BinarySearch<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> array, int index, int length, T value, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IComparer<T> comparer)
	{
		return Array.BinarySearch(array.array, index, length, value, comparer);
	}
}
[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
[_003C516ecac9_002Db1f3_002D482d_002Da3ef_002Db04c2a7ccc00_003ENonVersionable]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal struct ImmutableArray<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T> : IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, IList<T>, ICollection<T>, IEquatable<System.Collections.Immutable.ImmutableArray<T>>, IList, ICollection, IImmutableArray, IStructuralComparable, IStructuralEquatable, IImmutableList<T>
{
	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(ImmutableArrayBuilderDebuggerProxy<>))]
	public sealed class Builder : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>
	{
		private T[] _elements;

		private int _count;

		public int Capacity
		{
			get
			{
				return _elements.Length;
			}
			set
			{
				if (value < _count)
				{
					throw new ArgumentException(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.CapacityMustBeGreaterThanOrEqualToCount, "value");
				}
				if (value == _elements.Length)
				{
					return;
				}
				if (value > 0)
				{
					T[] array = new T[value];
					if (_count > 0)
					{
						Array.Copy(_elements, array, _count);
					}
					_elements = array;
				}
				else
				{
					_elements = System.Collections.Immutable.ImmutableArray<T>.Empty.array;
				}
			}
		}

		public int Count
		{
			get
			{
				return _count;
			}
			set
			{
				Requires.Range(value >= 0, "value");
				if (value < _count)
				{
					if (_count - value > 64)
					{
						Array.Clear(_elements, value, _count - value);
					}
					else
					{
						for (int i = value; i < Count; i++)
						{
							_elements[i] = default(T);
						}
					}
				}
				else if (value > _count)
				{
					EnsureCapacity(value);
				}
				_count = value;
			}
		}

		public T this[int index]
		{
			get
			{
				if (index >= Count)
				{
					ThrowIndexOutOfRangeException();
				}
				return _elements[index];
			}
			set
			{
				if (index >= Count)
				{
					ThrowIndexOutOfRangeException();
				}
				_elements[index] = value;
			}
		}

		bool ICollection<T>.IsReadOnly => false;

		internal Builder(int capacity)
		{
			Requires.Range(capacity >= 0, "capacity");
			_elements = new T[capacity];
			_count = 0;
		}

		internal Builder()
			: this(8)
		{
		}

		private static void ThrowIndexOutOfRangeException()
		{
			throw new IndexOutOfRangeException();
		}

		[return: _003C213e6825_002D2666_002D4c32_002Dad9b_002D2809d47857fd_003EIsReadOnly]
		public ref T ItemRef(int index)
		{
			if (index >= Count)
			{
				ThrowIndexOutOfRangeException();
			}
			return ref _elements[index];
		}

		[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
		public System.Collections.Immutable.ImmutableArray<T> ToImmutable()
		{
			return new System.Collections.Immutable.ImmutableArray<T>(ToArray());
		}

		[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
		public System.Collections.Immutable.ImmutableArray<T> MoveToImmutable()
		{
			if (Capacity != Count)
			{
				throw new InvalidOperationException(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.CapacityMustEqualCountOnMove);
			}
			T[] elements = _elements;
			_elements = System.Collections.Immutable.ImmutableArray<T>.Empty.array;
			_count = 0;
			return new System.Collections.Immutable.ImmutableArray<T>(elements);
		}

		public void Clear()
		{
			Count = 0;
		}

		public void Insert(int index, T item)
		{
			Requires.Range(index >= 0 && index <= Count, "index");
			EnsureCapacity(Count + 1);
			if (index < Count)
			{
				Array.Copy(_elements, index, _elements, index + 1, Count - index);
			}
			_count++;
			_elements[index] = item;
		}

		public void Add(T item)
		{
			int num = _count + 1;
			EnsureCapacity(num);
			_elements[_count] = item;
			_count = num;
		}

		public void AddRange(IEnumerable<T> items)
		{
			Requires.NotNull(items, "items");
			if (items.TryGetCount(out var count))
			{
				EnsureCapacity(Count + count);
				if (items.TryCopyTo(_elements, _count))
				{
					_count += count;
					return;
				}
			}
			foreach (T item in items)
			{
				Add(item);
			}
		}

		public void AddRange(params T[] items)
		{
			Requires.NotNull(items, "items");
			int count = Count;
			Count += items.Length;
			Array.Copy(items, 0, _elements, count, items.Length);
		}

		public void AddRange<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)] TDerived>(TDerived[] items) where TDerived : T
		{
			Requires.NotNull(items, "items");
			int count = Count;
			Count += items.Length;
			Array.Copy(items, 0, _elements, count, items.Length);
		}

		public void AddRange(T[] items, int length)
		{
			Requires.NotNull(items, "items");
			Requires.Range(length >= 0 && length <= items.Length, "length");
			int count = Count;
			Count += length;
			Array.Copy(items, 0, _elements, count, length);
		}

		public void AddRange([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T> items)
		{
			AddRange(items, items.Length);
		}

		public void AddRange([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T> items, int length)
		{
			Requires.Range(length >= 0, "length");
			if (items.array != null)
			{
				AddRange(items.array, length);
			}
		}

		[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
		public void AddRange<TDerived>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<TDerived> items) where TDerived : T
		{
			if (items.array != null)
			{
				AddRange(items.array);
			}
		}

		public void AddRange([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0 })] Builder items)
		{
			Requires.NotNull(items, "items");
			AddRange(items._elements, items.Count);
		}

		public void AddRange<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)] TDerived>(System.Collections.Immutable.ImmutableArray<TDerived>.Builder items) where TDerived : T
		{
			Requires.NotNull(items, "items");
			AddRange(items._elements, items.Count);
		}

		public bool Remove(T element)
		{
			int num = IndexOf(element);
			if (num >= 0)
			{
				RemoveAt(num);
				return true;
			}
			return false;
		}

		public void RemoveAt(int index)
		{
			Requires.Range(index >= 0 && index < Count, "index");
			if (index < Count - 1)
			{
				Array.Copy(_elements, index + 1, _elements, index, Count - index - 1);
			}
			Count--;
		}

		public bool Contains(T item)
		{
			return IndexOf(item) >= 0;
		}

		public T[] ToArray()
		{
			if (Count == 0)
			{
				return System.Collections.Immutable.ImmutableArray<T>.Empty.array;
			}
			T[] array = new T[Count];
			Array.Copy(_elements, array, Count);
			return array;
		}

		public void CopyTo(T[] array, int index)
		{
			Requires.NotNull(array, "array");
			Requires.Range(index >= 0 && index + Count <= array.Length, "index");
			Array.Copy(_elements, 0, array, index, Count);
		}

		private void EnsureCapacity(int capacity)
		{
			if (_elements.Length < capacity)
			{
				int newSize = Math.Max(_elements.Length * 2, capacity);
				Array.Resize(ref _elements, newSize);
			}
		}

		public int IndexOf(T item)
		{
			return IndexOf(item, 0, _count, EqualityComparer<T>.Default);
		}

		public int IndexOf(T item, int startIndex)
		{
			return IndexOf(item, startIndex, Count - startIndex, EqualityComparer<T>.Default);
		}

		public int IndexOf(T item, int startIndex, int count)
		{
			return IndexOf(item, startIndex, count, EqualityComparer<T>.Default);
		}

		public int IndexOf(T item, int startIndex, int count, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer)
		{
			if (count == 0 && startIndex == 0)
			{
				return -1;
			}
			Requires.Range(startIndex >= 0 && startIndex < Count, "startIndex");
			Requires.Range(count >= 0 && startIndex + count <= Count, "count");
			equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
			if (equalityComparer == EqualityComparer<T>.Default)
			{
				return Array.IndexOf(_elements, item, startIndex, count);
			}
			for (int i = startIndex; i < startIndex + count; i++)
			{
				if (equalityComparer.Equals(_elements[i], item))
				{
					return i;
				}
			}
			return -1;
		}

		public int LastIndexOf(T item)
		{
			if (Count == 0)
			{
				return -1;
			}
			return LastIndexOf(item, Count - 1, Count, EqualityComparer<T>.Default);
		}

		public int LastIndexOf(T item, int startIndex)
		{
			if (Count == 0 && startIndex == 0)
			{
				return -1;
			}
			Requires.Range(startIndex >= 0 && startIndex < Count, "startIndex");
			return LastIndexOf(item, startIndex, startIndex + 1, EqualityComparer<T>.Default);
		}

		public int LastIndexOf(T item, int startIndex, int count)
		{
			return LastIndexOf(item, startIndex, count, EqualityComparer<T>.Default);
		}

		public int LastIndexOf(T item, int startIndex, int count, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer)
		{
			if (count == 0 && startIndex == 0)
			{
				return -1;
			}
			Requires.Range(startIndex >= 0 && startIndex < Count, "startIndex");
			Requires.Range(count >= 0 && startIndex - count + 1 >= 0, "count");
			equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
			if (equalityComparer == EqualityComparer<T>.Default)
			{
				return Array.LastIndexOf(_elements, item, startIndex, count);
			}
			for (int num = startIndex; num >= startIndex - count + 1; num--)
			{
				if (equalityComparer.Equals(item, _elements[num]))
				{
					return num;
				}
			}
			return -1;
		}

		public void Reverse()
		{
			int num = 0;
			int num2 = _count - 1;
			T[] elements = _elements;
			while (num < num2)
			{
				T val = elements[num];
				elements[num] = elements[num2];
				elements[num2] = val;
				num++;
				num2--;
			}
		}

		public void Sort()
		{
			if (Count > 1)
			{
				Array.Sort(_elements, 0, Count, Comparer<T>.Default);
			}
		}

		public void Sort(Comparison<T> comparison)
		{
			Requires.NotNull(comparison, "comparison");
			if (Count > 1)
			{
				Array.Sort(_elements, 0, _count, Comparer<T>.Create(comparison));
			}
		}

		public void Sort([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IComparer<T> comparer)
		{
			if (Count > 1)
			{
				Array.Sort(_elements, 0, _count, comparer);
			}
		}

		public void Sort(int index, int count, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IComparer<T> comparer)
		{
			Requires.Range(index >= 0, "index");
			Requires.Range(count >= 0 && index + count <= Count, "count");
			if (count > 1)
			{
				Array.Sort(_elements, index, count, comparer);
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < Count; i++)
			{
				yield return this[i];
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

		private void AddRange<TDerived>(TDerived[] items, int length) where TDerived : T
		{
			EnsureCapacity(Count + length);
			int count = Count;
			Count += length;
			T[] elements = _elements;
			for (int i = 0; i < length; i++)
			{
				elements[count + i] = (T)(object)items[i];
			}
		}
	}

	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
	public struct Enumerator(T[] array)
	{
		private readonly T[] _array = array;

		private int _index = -1;

		public T Current => _array[_index];

		public bool MoveNext()
		{
			return ++_index < _array.Length;
		}
	}

	private class EnumeratorObject : IEnumerator<T>, IDisposable, IEnumerator
	{
		private static readonly IEnumerator<T> s_EmptyEnumerator = new EnumeratorObject(System.Collections.Immutable.ImmutableArray<T>.Empty.array);

		private readonly T[] _array;

		private int _index;

		public T Current
		{
			get
			{
				if ((uint)_index < (uint)_array.Length)
				{
					return _array[_index];
				}
				throw new InvalidOperationException();
			}
		}

		object IEnumerator.Current => Current;

		private EnumeratorObject(T[] array)
		{
			_index = -1;
			_array = array;
		}

		public bool MoveNext()
		{
			int num = _index + 1;
			int num2 = _array.Length;
			if ((uint)num <= (uint)num2)
			{
				_index = num;
				return (uint)num < (uint)num2;
			}
			return false;
		}

		void IEnumerator.Reset()
		{
			_index = -1;
		}

		public void Dispose()
		{
		}

		internal static IEnumerator<T> Create(T[] array)
		{
			if (array.Length != 0)
			{
				return new EnumeratorObject(array);
			}
			return s_EmptyEnumerator;
		}
	}

	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public static readonly System.Collections.Immutable.ImmutableArray<T> Empty = new System.Collections.Immutable.ImmutableArray<T>(new T[0]);

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })]
	internal T[] array;

	T IList<T>.this[int index]
	{
		get
		{
			System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
			immutableArray.ThrowInvalidOperationIfNotInitialized();
			return immutableArray[index];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	bool ICollection<T>.IsReadOnly => true;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	int ICollection<T>.Count
	{
		get
		{
			System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
			immutableArray.ThrowInvalidOperationIfNotInitialized();
			return immutableArray.Length;
		}
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	int IReadOnlyCollection<T>.Count
	{
		get
		{
			System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
			immutableArray.ThrowInvalidOperationIfNotInitialized();
			return immutableArray.Length;
		}
	}

	T IReadOnlyList<T>.this[int index]
	{
		get
		{
			System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
			immutableArray.ThrowInvalidOperationIfNotInitialized();
			return immutableArray[index];
		}
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	bool IList.IsFixedSize => true;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	bool IList.IsReadOnly => true;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	int ICollection.Count
	{
		get
		{
			System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
			immutableArray.ThrowInvalidOperationIfNotInitialized();
			return immutableArray.Length;
		}
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	bool ICollection.IsSynchronized => true;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	object ICollection.SyncRoot
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)]
	object IList.this[int index]
	{
		get
		{
			System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
			immutableArray.ThrowInvalidOperationIfNotInitialized();
			return immutableArray[index];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public T this[int index]
	{
		[_003C516ecac9_002Db1f3_002D482d_002Da3ef_002Db04c2a7ccc00_003ENonVersionable]
		get
		{
			return array[index];
		}
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public bool IsEmpty
	{
		[_003C516ecac9_002Db1f3_002D482d_002Da3ef_002Db04c2a7ccc00_003ENonVersionable]
		get
		{
			return array.Length == 0;
		}
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public int Length
	{
		[_003C516ecac9_002Db1f3_002D482d_002Da3ef_002Db04c2a7ccc00_003ENonVersionable]
		get
		{
			return array.Length;
		}
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public bool IsDefault => array == null;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public bool IsDefaultOrEmpty
	{
		get
		{
			System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
			if (immutableArray.array != null)
			{
				return immutableArray.array.Length == 0;
			}
			return true;
		}
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)]
	Array IImmutableArray.Array => array;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string DebuggerDisplay
	{
		get
		{
			System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
			if (!immutableArray.IsDefault)
			{
				return string.Format(CultureInfo.CurrentCulture, "Length = {0}", immutableArray.Length);
			}
			return "Uninitialized";
		}
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public ReadOnlySpan<T> AsSpan()
	{
		return new ReadOnlySpan<T>(array);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public ReadOnlyMemory<T> AsMemory()
	{
		return new ReadOnlyMemory<T>(array);
	}

	public int IndexOf(T item)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		return immutableArray.IndexOf(item, 0, immutableArray.Length, EqualityComparer<T>.Default);
	}

	public int IndexOf(T item, int startIndex, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		return immutableArray.IndexOf(item, startIndex, immutableArray.Length - startIndex, equalityComparer);
	}

	public int IndexOf(T item, int startIndex)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		return immutableArray.IndexOf(item, startIndex, immutableArray.Length - startIndex, EqualityComparer<T>.Default);
	}

	public int IndexOf(T item, int startIndex, int count)
	{
		return IndexOf(item, startIndex, count, EqualityComparer<T>.Default);
	}

	public int IndexOf(T item, int startIndex, int count, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowNullRefIfNotInitialized();
		if (count == 0 && startIndex == 0)
		{
			return -1;
		}
		Requires.Range(startIndex >= 0 && startIndex < immutableArray.Length, "startIndex");
		Requires.Range(count >= 0 && startIndex + count <= immutableArray.Length, "count");
		equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
		if (equalityComparer == EqualityComparer<T>.Default)
		{
			return Array.IndexOf(immutableArray.array, item, startIndex, count);
		}
		for (int i = startIndex; i < startIndex + count; i++)
		{
			if (equalityComparer.Equals(immutableArray.array[i], item))
			{
				return i;
			}
		}
		return -1;
	}

	public int LastIndexOf(T item)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		if (immutableArray.Length == 0)
		{
			return -1;
		}
		return immutableArray.LastIndexOf(item, immutableArray.Length - 1, immutableArray.Length, EqualityComparer<T>.Default);
	}

	public int LastIndexOf(T item, int startIndex)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		if (immutableArray.Length == 0 && startIndex == 0)
		{
			return -1;
		}
		return immutableArray.LastIndexOf(item, startIndex, startIndex + 1, EqualityComparer<T>.Default);
	}

	public int LastIndexOf(T item, int startIndex, int count)
	{
		return LastIndexOf(item, startIndex, count, EqualityComparer<T>.Default);
	}

	public int LastIndexOf(T item, int startIndex, int count, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowNullRefIfNotInitialized();
		if (startIndex == 0 && count == 0)
		{
			return -1;
		}
		Requires.Range(startIndex >= 0 && startIndex < immutableArray.Length, "startIndex");
		Requires.Range(count >= 0 && startIndex - count + 1 >= 0, "count");
		equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
		if (equalityComparer == EqualityComparer<T>.Default)
		{
			return Array.LastIndexOf(immutableArray.array, item, startIndex, count);
		}
		for (int num = startIndex; num >= startIndex - count + 1; num--)
		{
			if (equalityComparer.Equals(item, immutableArray.array[num]))
			{
				return num;
			}
		}
		return -1;
	}

	public bool Contains(T item)
	{
		return IndexOf(item) >= 0;
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> Insert(int index, T item)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowNullRefIfNotInitialized();
		Requires.Range(index >= 0 && index <= immutableArray.Length, "index");
		if (immutableArray.Length == 0)
		{
			return System.Collections.Immutable.ImmutableArray.Create(item);
		}
		T[] array = new T[immutableArray.Length + 1];
		array[index] = item;
		if (index != 0)
		{
			Array.Copy(immutableArray.array, array, index);
		}
		if (index != immutableArray.Length)
		{
			Array.Copy(immutableArray.array, index, array, index + 1, immutableArray.Length - index);
		}
		return new System.Collections.Immutable.ImmutableArray<T>(array);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> InsertRange(int index, IEnumerable<T> items)
	{
		System.Collections.Immutable.ImmutableArray<T> result = this;
		result.ThrowNullRefIfNotInitialized();
		Requires.Range(index >= 0 && index <= result.Length, "index");
		Requires.NotNull(items, "items");
		if (result.Length == 0)
		{
			return System.Collections.Immutable.ImmutableArray.CreateRange(items);
		}
		int count = ImmutableExtensions.GetCount(ref items);
		if (count == 0)
		{
			return result;
		}
		T[] array = new T[result.Length + count];
		if (index != 0)
		{
			Array.Copy(result.array, array, index);
		}
		if (index != result.Length)
		{
			Array.Copy(result.array, index, array, index + count, result.Length - index);
		}
		if (!items.TryCopyTo(array, index))
		{
			int num = index;
			foreach (T item in items)
			{
				array[num++] = item;
			}
		}
		return new System.Collections.Immutable.ImmutableArray<T>(array);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> InsertRange(int index, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T> items)
	{
		System.Collections.Immutable.ImmutableArray<T> result = this;
		result.ThrowNullRefIfNotInitialized();
		items.ThrowNullRefIfNotInitialized();
		Requires.Range(index >= 0 && index <= result.Length, "index");
		if (result.IsEmpty)
		{
			return items;
		}
		if (items.IsEmpty)
		{
			return result;
		}
		T[] array = new T[result.Length + items.Length];
		if (index != 0)
		{
			Array.Copy(result.array, array, index);
		}
		if (index != result.Length)
		{
			Array.Copy(result.array, index, array, index + items.Length, result.Length - index);
		}
		Array.Copy(items.array, 0, array, index, items.Length);
		return new System.Collections.Immutable.ImmutableArray<T>(array);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> Add(T item)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		if (immutableArray.Length == 0)
		{
			return System.Collections.Immutable.ImmutableArray.Create(item);
		}
		return immutableArray.Insert(immutableArray.Length, item);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> AddRange(IEnumerable<T> items)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		return immutableArray.InsertRange(immutableArray.Length, items);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> AddRange([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T> items)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		return immutableArray.InsertRange(immutableArray.Length, items);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> SetItem(int index, T item)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowNullRefIfNotInitialized();
		Requires.Range(index >= 0 && index < immutableArray.Length, "index");
		T[] array = new T[immutableArray.Length];
		Array.Copy(immutableArray.array, array, immutableArray.Length);
		array[index] = item;
		return new System.Collections.Immutable.ImmutableArray<T>(array);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> Replace(T oldValue, T newValue)
	{
		return Replace(oldValue, newValue, EqualityComparer<T>.Default);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> Replace(T oldValue, T newValue, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		int num = immutableArray.IndexOf(oldValue, 0, immutableArray.Length, equalityComparer);
		if (num < 0)
		{
			throw new ArgumentException(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.CannotFindOldValue, "oldValue");
		}
		return immutableArray.SetItem(num, newValue);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> Remove(T item)
	{
		return Remove(item, EqualityComparer<T>.Default);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> Remove(T item, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer)
	{
		System.Collections.Immutable.ImmutableArray<T> result = this;
		result.ThrowNullRefIfNotInitialized();
		int num = result.IndexOf(item, 0, result.Length, equalityComparer);
		if (num >= 0)
		{
			return result.RemoveAt(num);
		}
		return result;
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> RemoveAt(int index)
	{
		return RemoveRange(index, 1);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> RemoveRange(int index, int length)
	{
		System.Collections.Immutable.ImmutableArray<T> result = this;
		result.ThrowNullRefIfNotInitialized();
		Requires.Range(index >= 0 && index <= result.Length, "index");
		Requires.Range(length >= 0 && index + length <= result.Length, "length");
		if (length == 0)
		{
			return result;
		}
		T[] array = new T[result.Length - length];
		Array.Copy(result.array, array, index);
		Array.Copy(result.array, index + length, array, index, result.Length - index - length);
		return new System.Collections.Immutable.ImmutableArray<T>(array);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> RemoveRange(IEnumerable<T> items)
	{
		return RemoveRange(items, EqualityComparer<T>.Default);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> RemoveRange(IEnumerable<T> items, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowNullRefIfNotInitialized();
		Requires.NotNull(items, "items");
		SortedSet<int> sortedSet = new SortedSet<int>();
		foreach (T item in items)
		{
			int num = immutableArray.IndexOf(item, 0, immutableArray.Length, equalityComparer);
			while (num >= 0 && !sortedSet.Add(num) && num + 1 < immutableArray.Length)
			{
				num = immutableArray.IndexOf(item, num + 1, equalityComparer);
			}
		}
		return immutableArray.RemoveAtRange(sortedSet);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> RemoveRange([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T> items)
	{
		return RemoveRange(items, EqualityComparer<T>.Default);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> RemoveRange([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T> items, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer)
	{
		System.Collections.Immutable.ImmutableArray<T> result = this;
		Requires.NotNull(items.array, "items");
		if (items.IsEmpty)
		{
			result.ThrowNullRefIfNotInitialized();
			return result;
		}
		if (items.Length == 1)
		{
			return result.Remove(items[0], equalityComparer);
		}
		return result.RemoveRange(items.array, equalityComparer);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> RemoveAll(Predicate<T> match)
	{
		System.Collections.Immutable.ImmutableArray<T> result = this;
		result.ThrowNullRefIfNotInitialized();
		Requires.NotNull(match, "match");
		if (result.IsEmpty)
		{
			return result;
		}
		List<int> list = null;
		for (int i = 0; i < result.array.Length; i++)
		{
			if (match(result.array[i]))
			{
				if (list == null)
				{
					list = new List<int>();
				}
				list.Add(i);
			}
		}
		if (list == null)
		{
			return result;
		}
		return result.RemoveAtRange(list);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> Clear()
	{
		return Empty;
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> Sort()
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		return immutableArray.Sort(0, immutableArray.Length, Comparer<T>.Default);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> Sort(Comparison<T> comparison)
	{
		Requires.NotNull(comparison, "comparison");
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		return immutableArray.Sort(Comparer<T>.Create(comparison));
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> Sort([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IComparer<T> comparer)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		return immutableArray.Sort(0, immutableArray.Length, comparer);
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public System.Collections.Immutable.ImmutableArray<T> Sort(int index, int count, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IComparer<T> comparer)
	{
		System.Collections.Immutable.ImmutableArray<T> result = this;
		result.ThrowNullRefIfNotInitialized();
		Requires.Range(index >= 0, "index");
		Requires.Range(count >= 0 && index + count <= result.Length, "count");
		if (count > 1)
		{
			if (comparer == null)
			{
				comparer = Comparer<T>.Default;
			}
			bool flag = false;
			for (int i = index + 1; i < index + count; i++)
			{
				if (comparer.Compare(result.array[i - 1], result.array[i]) > 0)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				T[] array = new T[result.Length];
				Array.Copy(result.array, array, result.Length);
				Array.Sort(array, index, count, comparer);
				return new System.Collections.Immutable.ImmutableArray<T>(array);
			}
		}
		return result;
	}

	public IEnumerable<TResult> OfType<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TResult>()
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		if (immutableArray.array == null || immutableArray.array.Length == 0)
		{
			return Enumerable.Empty<TResult>();
		}
		return immutableArray.array.OfType<TResult>();
	}

	void IList<T>.Insert(int index, T item)
	{
		throw new NotSupportedException();
	}

	void IList<T>.RemoveAt(int index)
	{
		throw new NotSupportedException();
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

	IImmutableList<T> IImmutableList<T>.Clear()
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowInvalidOperationIfNotInitialized();
		return immutableArray.Clear();
	}

	IImmutableList<T> IImmutableList<T>.Add(T value)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowInvalidOperationIfNotInitialized();
		return immutableArray.Add(value);
	}

	IImmutableList<T> IImmutableList<T>.AddRange(IEnumerable<T> items)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowInvalidOperationIfNotInitialized();
		return immutableArray.AddRange(items);
	}

	IImmutableList<T> IImmutableList<T>.Insert(int index, T element)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowInvalidOperationIfNotInitialized();
		return immutableArray.Insert(index, element);
	}

	IImmutableList<T> IImmutableList<T>.InsertRange(int index, IEnumerable<T> items)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowInvalidOperationIfNotInitialized();
		return immutableArray.InsertRange(index, items);
	}

	IImmutableList<T> IImmutableList<T>.Remove(T value, IEqualityComparer<T> equalityComparer)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowInvalidOperationIfNotInitialized();
		return immutableArray.Remove(value, equalityComparer);
	}

	IImmutableList<T> IImmutableList<T>.RemoveAll(Predicate<T> match)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowInvalidOperationIfNotInitialized();
		return immutableArray.RemoveAll(match);
	}

	IImmutableList<T> IImmutableList<T>.RemoveRange(IEnumerable<T> items, IEqualityComparer<T> equalityComparer)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowInvalidOperationIfNotInitialized();
		return immutableArray.RemoveRange(items, equalityComparer);
	}

	IImmutableList<T> IImmutableList<T>.RemoveRange(int index, int count)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowInvalidOperationIfNotInitialized();
		return immutableArray.RemoveRange(index, count);
	}

	IImmutableList<T> IImmutableList<T>.RemoveAt(int index)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowInvalidOperationIfNotInitialized();
		return immutableArray.RemoveAt(index);
	}

	IImmutableList<T> IImmutableList<T>.SetItem(int index, T value)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowInvalidOperationIfNotInitialized();
		return immutableArray.SetItem(index, value);
	}

	IImmutableList<T> IImmutableList<T>.Replace(T oldValue, T newValue, IEqualityComparer<T> equalityComparer)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowInvalidOperationIfNotInitialized();
		return immutableArray.Replace(oldValue, newValue, equalityComparer);
	}

	int IList.Add(object value)
	{
		throw new NotSupportedException();
	}

	void IList.Clear()
	{
		throw new NotSupportedException();
	}

	bool IList.Contains(object value)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowInvalidOperationIfNotInitialized();
		return immutableArray.Contains((T)value);
	}

	int IList.IndexOf(object value)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowInvalidOperationIfNotInitialized();
		return immutableArray.IndexOf((T)value);
	}

	void IList.Insert(int index, object value)
	{
		throw new NotSupportedException();
	}

	void IList.Remove(object value)
	{
		throw new NotSupportedException();
	}

	void IList.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	void ICollection.CopyTo(Array array, int index)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowInvalidOperationIfNotInitialized();
		Array.Copy(immutableArray.array, 0, array, index, immutableArray.Length);
	}

	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		Array array = other as Array;
		if (array == null && other is IImmutableArray immutableArray2)
		{
			array = immutableArray2.Array;
			if (immutableArray.array == null && array == null)
			{
				return true;
			}
			if (immutableArray.array == null)
			{
				return false;
			}
		}
		IStructuralEquatable structuralEquatable = immutableArray.array;
		return structuralEquatable.Equals(array, comparer);
	}

	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		return ((IStructuralEquatable)immutableArray.array)?.GetHashCode(comparer) ?? immutableArray.GetHashCode();
	}

	int IStructuralComparable.CompareTo(object other, IComparer comparer)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		Array array = other as Array;
		if (array == null && other is IImmutableArray immutableArray2)
		{
			array = immutableArray2.Array;
			if (immutableArray.array == null && array == null)
			{
				return 0;
			}
			if ((immutableArray.array == null) ^ (array == null))
			{
				throw new ArgumentException(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.ArrayInitializedStateNotEqual, "other");
			}
		}
		if (array != null)
		{
			IStructuralComparable structuralComparable = immutableArray.array;
			if (structuralComparable == null)
			{
				throw new ArgumentException(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.ArrayInitializedStateNotEqual, "other");
			}
			return structuralComparable.CompareTo(array, comparer);
		}
		throw new ArgumentException(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.ArrayLengthsNotEqual, "other");
	}

	private System.Collections.Immutable.ImmutableArray<T> RemoveAtRange(ICollection<int> indicesToRemove)
	{
		System.Collections.Immutable.ImmutableArray<T> result = this;
		result.ThrowNullRefIfNotInitialized();
		Requires.NotNull(indicesToRemove, "indicesToRemove");
		if (indicesToRemove.Count == 0)
		{
			return result;
		}
		T[] array = new T[result.Length - indicesToRemove.Count];
		int num = 0;
		int num2 = 0;
		int num3 = -1;
		foreach (int item in indicesToRemove)
		{
			int num4 = ((num3 == -1) ? item : (item - num3 - 1));
			Array.Copy(result.array, num + num2, array, num, num4);
			num2++;
			num += num4;
			num3 = item;
		}
		Array.Copy(result.array, num + num2, array, num, result.Length - (num + num2));
		return new System.Collections.Immutable.ImmutableArray<T>(array);
	}

	internal ImmutableArray([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] T[] items)
	{
		array = items;
	}

	[_003C516ecac9_002Db1f3_002D482d_002Da3ef_002Db04c2a7ccc00_003ENonVersionable]
	public static bool operator ==([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T> left, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T> right)
	{
		return left.Equals(right);
	}

	[_003C516ecac9_002Db1f3_002D482d_002Da3ef_002Db04c2a7ccc00_003ENonVersionable]
	public static bool operator !=([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T> left, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T> right)
	{
		return !left.Equals(right);
	}

	public static bool operator ==([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T>? left, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T>? right)
	{
		return left.GetValueOrDefault().Equals(right.GetValueOrDefault());
	}

	public static bool operator !=([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T>? left, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T>? right)
	{
		return !left.GetValueOrDefault().Equals(right.GetValueOrDefault());
	}

	[return: _003C213e6825_002D2666_002D4c32_002Dad9b_002D2809d47857fd_003EIsReadOnly]
	public ref T ItemRef(int index)
	{
		return ref array[index];
	}

	public void CopyTo(T[] destination)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowNullRefIfNotInitialized();
		Array.Copy(immutableArray.array, destination, immutableArray.Length);
	}

	public void CopyTo(T[] destination, int destinationIndex)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowNullRefIfNotInitialized();
		Array.Copy(immutableArray.array, 0, destination, destinationIndex, immutableArray.Length);
	}

	public void CopyTo(int sourceIndex, T[] destination, int destinationIndex, int length)
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowNullRefIfNotInitialized();
		Array.Copy(immutableArray.array, sourceIndex, destination, destinationIndex, length);
	}

	public Builder ToBuilder()
	{
		System.Collections.Immutable.ImmutableArray<T> items = this;
		if (items.Length == 0)
		{
			return new Builder();
		}
		Builder builder = new Builder(items.Length);
		builder.AddRange(items);
		return builder;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
	public Enumerator GetEnumerator()
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowNullRefIfNotInitialized();
		return new Enumerator(immutableArray.array);
	}

	public override int GetHashCode()
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		if (immutableArray.array != null)
		{
			return immutableArray.array.GetHashCode();
		}
		return 0;
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	public override bool Equals(object obj)
	{
		if (obj is IImmutableArray immutableArray)
		{
			return array == immutableArray.Array;
		}
		return false;
	}

	[_003C516ecac9_002Db1f3_002D482d_002Da3ef_002Db04c2a7ccc00_003ENonVersionable]
	public bool Equals([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T> other)
	{
		return array == other.array;
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
	public static System.Collections.Immutable.ImmutableArray<T> CastUp<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TDerived>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<TDerived> items) where TDerived : class, T
	{
		T[] items2 = items.array;
		return new System.Collections.Immutable.ImmutableArray<T>(items2);
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
	public System.Collections.Immutable.ImmutableArray<TOther> CastArray<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TOther>() where TOther : class
	{
		return new System.Collections.Immutable.ImmutableArray<TOther>((TOther[])(object)array);
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
	public System.Collections.Immutable.ImmutableArray<TOther> As<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TOther>() where TOther : class
	{
		return new System.Collections.Immutable.ImmutableArray<TOther>(array as TOther[]);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowInvalidOperationIfNotInitialized();
		return EnumeratorObject.Create(immutableArray.array);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		System.Collections.Immutable.ImmutableArray<T> immutableArray = this;
		immutableArray.ThrowInvalidOperationIfNotInitialized();
		return EnumeratorObject.Create(immutableArray.array);
	}

	internal void ThrowNullRefIfNotInitialized()
	{
		_ = array.Length;
	}

	private void ThrowInvalidOperationIfNotInitialized()
	{
		if (IsDefault)
		{
			throw new InvalidOperationException(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.InvalidOperationOnDefaultArray);
		}
	}
}
