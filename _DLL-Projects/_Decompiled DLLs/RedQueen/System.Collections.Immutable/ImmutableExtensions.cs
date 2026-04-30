using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
internal static class ImmutableExtensions
{
	private class ListOfTWrapper<T> : IOrderedCollection<T>, IEnumerable<T>, IEnumerable
	{
		private readonly IList<T> _collection;

		public int Count => _collection.Count;

		public T this[int index] => _collection[index];

		internal ListOfTWrapper(IList<T> collection)
		{
			Requires.NotNull(collection, "collection");
			_collection = collection;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _collection.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private class FallbackWrapper<T> : IOrderedCollection<T>, IEnumerable<T>, IEnumerable
	{
		private readonly IEnumerable<T> _sequence;

		private IList<T> _collection;

		public int Count
		{
			get
			{
				if (_collection == null)
				{
					if (_sequence.TryGetCount(out var count))
					{
						return count;
					}
					_collection = _sequence.ToArray();
				}
				return _collection.Count;
			}
		}

		public T this[int index]
		{
			get
			{
				if (_collection == null)
				{
					_collection = _sequence.ToArray();
				}
				return _collection[index];
			}
		}

		internal FallbackWrapper(IEnumerable<T> sequence)
		{
			Requires.NotNull(sequence, "sequence");
			_sequence = sequence;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _sequence.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	internal static bool IsValueType<T>()
	{
		if (default(T) != null)
		{
			return true;
		}
		Type typeFromHandle = typeof(T);
		if (typeFromHandle.IsConstructedGenericType && typeFromHandle.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			return true;
		}
		return false;
	}

	internal static IOrderedCollection<T> AsOrderedCollection<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(this IEnumerable<T> sequence)
	{
		Requires.NotNull(sequence, "sequence");
		if (sequence is IOrderedCollection<T> result)
		{
			return result;
		}
		if (sequence is IList<T> collection)
		{
			return new ListOfTWrapper<T>(collection);
		}
		return new FallbackWrapper<T>(sequence);
	}

	internal static void ClearFastWhenEmpty<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(this Stack<T> stack)
	{
		if (stack.Count > 0)
		{
			stack.Clear();
		}
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1, 0 })]
	internal static DisposableEnumeratorAdapter<T, TEnumerator> GetEnumerableDisposable<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)] TEnumerator>(this IEnumerable<T> enumerable) where TEnumerator : struct, IStrongEnumerator<T>, IEnumerator<T>
	{
		Requires.NotNull(enumerable, "enumerable");
		if (enumerable is IStrongEnumerable<T, TEnumerator> strongEnumerable)
		{
			return new DisposableEnumeratorAdapter<T, TEnumerator>(strongEnumerable.GetEnumerator());
		}
		return new DisposableEnumeratorAdapter<T, TEnumerator>(enumerable.GetEnumerator());
	}

	internal static bool TryGetCount<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(this IEnumerable<T> sequence, out int count)
	{
		return ((IEnumerable)sequence).TryGetCount<T>(out count);
	}

	internal static bool TryGetCount<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(this IEnumerable sequence, out int count)
	{
		if (sequence is ICollection collection)
		{
			count = collection.Count;
			return true;
		}
		if (sequence is ICollection<T> collection2)
		{
			count = collection2.Count;
			return true;
		}
		if (sequence is IReadOnlyCollection<T> readOnlyCollection)
		{
			count = readOnlyCollection.Count;
			return true;
		}
		count = 0;
		return false;
	}

	internal static int GetCount<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(ref IEnumerable<T> sequence)
	{
		if (!sequence.TryGetCount(out var count))
		{
			List<T> list = sequence.ToList();
			count = list.Count;
			sequence = list;
		}
		return count;
	}

	internal static bool TryCopyTo<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(this IEnumerable<T> sequence, T[] array, int arrayIndex)
	{
		if (sequence is IList<T>)
		{
			if (sequence is List<T> list2)
			{
				list2.CopyTo(array, arrayIndex);
				return true;
			}
			if (sequence.GetType() == typeof(T[]))
			{
				T[] array2 = (T[])sequence;
				Array.Copy(array2, 0, array, arrayIndex, array2.Length);
				return true;
			}
			if (sequence is System.Collections.Immutable.ImmutableArray<T> immutableArray)
			{
				Array.Copy(immutableArray.array, 0, array, arrayIndex, immutableArray.Length);
				return true;
			}
		}
		return false;
	}

	internal static T[] ToArray<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(this IEnumerable<T> sequence, int count)
	{
		Requires.NotNull(sequence, "sequence");
		Requires.Range(count >= 0, "count");
		if (count == 0)
		{
			return System.Collections.Immutable.ImmutableArray<T>.Empty.array;
		}
		T[] array = new T[count];
		if (!sequence.TryCopyTo(array, 0))
		{
			int num = 0;
			foreach (T item in sequence)
			{
				Requires.Argument(num < count);
				array[num++] = item;
			}
			Requires.Argument(num == count);
		}
		return array;
	}
}
