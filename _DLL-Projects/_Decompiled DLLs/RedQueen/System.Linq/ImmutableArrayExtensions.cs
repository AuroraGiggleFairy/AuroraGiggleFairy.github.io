using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace System.Linq;

[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
internal static class ImmutableArrayExtensions
{
	public static IEnumerable<TResult> Select<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TResult>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray, Func<T, TResult> selector)
	{
		immutableArray.ThrowNullRefIfNotInitialized();
		return immutableArray.array.Select(selector);
	}

	public static IEnumerable<TResult> SelectMany<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TSource, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TCollection, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TResult>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<TSource> immutableArray, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
	{
		immutableArray.ThrowNullRefIfNotInitialized();
		if (collectionSelector == null || resultSelector == null)
		{
			return Enumerable.SelectMany(immutableArray, collectionSelector, resultSelector);
		}
		if (immutableArray.Length != 0)
		{
			return immutableArray.SelectManyIterator(collectionSelector, resultSelector);
		}
		return Enumerable.Empty<TResult>();
	}

	public static IEnumerable<T> Where<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray, Func<T, bool> predicate)
	{
		immutableArray.ThrowNullRefIfNotInitialized();
		return immutableArray.array.Where(predicate);
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	public static bool Any<T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray)
	{
		return immutableArray.Length > 0;
	}

	public static bool Any<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray, Func<T, bool> predicate)
	{
		immutableArray.ThrowNullRefIfNotInitialized();
		Requires.NotNull(predicate, "predicate");
		T[] array = immutableArray.array;
		foreach (T arg in array)
		{
			if (predicate(arg))
			{
				return true;
			}
		}
		return false;
	}

	public static bool All<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray, Func<T, bool> predicate)
	{
		immutableArray.ThrowNullRefIfNotInitialized();
		Requires.NotNull(predicate, "predicate");
		T[] array = immutableArray.array;
		foreach (T arg in array)
		{
			if (!predicate(arg))
			{
				return false;
			}
		}
		return true;
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
	public static bool SequenceEqual<TDerived, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TBase>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<TBase> immutableArray, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<TDerived> items, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TBase> comparer = null) where TDerived : TBase
	{
		immutableArray.ThrowNullRefIfNotInitialized();
		items.ThrowNullRefIfNotInitialized();
		if ((object)immutableArray.array == items.array)
		{
			return true;
		}
		if (immutableArray.Length != items.Length)
		{
			return false;
		}
		if (comparer == null)
		{
			comparer = EqualityComparer<TBase>.Default;
		}
		for (int i = 0; i < immutableArray.Length; i++)
		{
			if (!comparer.Equals(immutableArray.array[i], (TBase)(object)items.array[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static bool SequenceEqual<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)] TDerived, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TBase>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<TBase> immutableArray, IEnumerable<TDerived> items, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TBase> comparer = null) where TDerived : TBase
	{
		Requires.NotNull(items, "items");
		if (comparer == null)
		{
			comparer = EqualityComparer<TBase>.Default;
		}
		int num = 0;
		int length = immutableArray.Length;
		foreach (TDerived item in items)
		{
			if (num == length)
			{
				return false;
			}
			if (!comparer.Equals(immutableArray[num], (TBase)(object)item))
			{
				return false;
			}
			num++;
		}
		return num == length;
	}

	public static bool SequenceEqual<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)] TDerived, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TBase>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<TBase> immutableArray, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<TDerived> items, Func<TBase, TBase, bool> predicate) where TDerived : TBase
	{
		Requires.NotNull(predicate, "predicate");
		immutableArray.ThrowNullRefIfNotInitialized();
		items.ThrowNullRefIfNotInitialized();
		if ((object)immutableArray.array == items.array)
		{
			return true;
		}
		if (immutableArray.Length != items.Length)
		{
			return false;
		}
		int i = 0;
		for (int length = immutableArray.Length; i < length; i++)
		{
			if (!predicate(immutableArray[i], (TBase)(object)items[i]))
			{
				return false;
			}
		}
		return true;
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	public static T Aggregate<T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] Func<T, T, T> func)
	{
		Requires.NotNull(func, "func");
		if (immutableArray.Length == 0)
		{
			return default(T);
		}
		T val = immutableArray[0];
		int i = 1;
		for (int length = immutableArray.Length; i < length; i++)
		{
			val = func(val, immutableArray[i]);
		}
		return val;
	}

	public static TAccumulate Aggregate<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TAccumulate, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func)
	{
		Requires.NotNull(func, "func");
		TAccumulate val = seed;
		T[] array = immutableArray.array;
		foreach (T arg in array)
		{
			val = func(val, arg);
		}
		return val;
	}

	public static TResult Aggregate<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TAccumulate, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TResult, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
	{
		Requires.NotNull(resultSelector, "resultSelector");
		return resultSelector(immutableArray.Aggregate(seed, func));
	}

	public static T ElementAt<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray, int index)
	{
		return immutableArray[index];
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	public static T ElementAtOrDefault<T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray, int index)
	{
		if (index < 0 || index >= immutableArray.Length)
		{
			return default(T);
		}
		return immutableArray[index];
	}

	public static T First<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray, Func<T, bool> predicate)
	{
		Requires.NotNull(predicate, "predicate");
		T[] array = immutableArray.array;
		foreach (T val in array)
		{
			if (predicate(val))
			{
				return val;
			}
		}
		return Enumerable.Empty<T>().First();
	}

	public static T First<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray)
	{
		if (immutableArray.Length <= 0)
		{
			return immutableArray.array.First();
		}
		return immutableArray[0];
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	public static T FirstOrDefault<T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray)
	{
		if (immutableArray.array.Length == 0)
		{
			return default(T);
		}
		return immutableArray.array[0];
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	public static T FirstOrDefault<T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] Func<T, bool> predicate)
	{
		Requires.NotNull(predicate, "predicate");
		T[] array = immutableArray.array;
		foreach (T val in array)
		{
			if (predicate(val))
			{
				return val;
			}
		}
		return default(T);
	}

	public static T Last<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray)
	{
		if (immutableArray.Length <= 0)
		{
			return immutableArray.array.Last();
		}
		return immutableArray[immutableArray.Length - 1];
	}

	public static T Last<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray, Func<T, bool> predicate)
	{
		Requires.NotNull(predicate, "predicate");
		for (int num = immutableArray.Length - 1; num >= 0; num--)
		{
			if (predicate(immutableArray[num]))
			{
				return immutableArray[num];
			}
		}
		return Enumerable.Empty<T>().Last();
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	public static T LastOrDefault<T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray)
	{
		immutableArray.ThrowNullRefIfNotInitialized();
		return immutableArray.array.LastOrDefault();
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	public static T LastOrDefault<T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] Func<T, bool> predicate)
	{
		Requires.NotNull(predicate, "predicate");
		for (int num = immutableArray.Length - 1; num >= 0; num--)
		{
			if (predicate(immutableArray[num]))
			{
				return immutableArray[num];
			}
		}
		return default(T);
	}

	public static T Single<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray)
	{
		immutableArray.ThrowNullRefIfNotInitialized();
		return immutableArray.array.Single();
	}

	public static T Single<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray, Func<T, bool> predicate)
	{
		Requires.NotNull(predicate, "predicate");
		bool flag = true;
		T result = default(T);
		T[] array = immutableArray.array;
		foreach (T val in array)
		{
			if (predicate(val))
			{
				if (!flag)
				{
					System.Collections.Immutable.ImmutableArray.TwoElementArray.Single();
				}
				flag = false;
				result = val;
			}
		}
		if (flag)
		{
			Enumerable.Empty<T>().Single();
		}
		return result;
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	public static T SingleOrDefault<T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray)
	{
		immutableArray.ThrowNullRefIfNotInitialized();
		return immutableArray.array.SingleOrDefault();
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	public static T SingleOrDefault<T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] Func<T, bool> predicate)
	{
		Requires.NotNull(predicate, "predicate");
		bool flag = true;
		T result = default(T);
		T[] array = immutableArray.array;
		foreach (T val in array)
		{
			if (predicate(val))
			{
				if (!flag)
				{
					System.Collections.Immutable.ImmutableArray.TwoElementArray.Single();
				}
				flag = false;
				result = val;
			}
		}
		return result;
	}

	public static Dictionary<TKey, T> ToDictionary<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray, Func<T, TKey> keySelector)
	{
		return immutableArray.ToDictionary(keySelector, EqualityComparer<TKey>.Default);
	}

	public static Dictionary<TKey, TElement> ToDictionary<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TElement, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray, Func<T, TKey> keySelector, Func<T, TElement> elementSelector)
	{
		return immutableArray.ToDictionary(keySelector, elementSelector, EqualityComparer<TKey>.Default);
	}

	public static Dictionary<TKey, T> ToDictionary<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray, Func<T, TKey> keySelector, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer)
	{
		Requires.NotNull(keySelector, "keySelector");
		Dictionary<TKey, T> dictionary = new Dictionary<TKey, T>(immutableArray.Length, comparer);
		System.Collections.Immutable.ImmutableArray<T>.Enumerator enumerator = immutableArray.GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			dictionary.Add(keySelector(current), current);
		}
		return dictionary;
	}

	public static Dictionary<TKey, TElement> ToDictionary<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TElement, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray, Func<T, TKey> keySelector, Func<T, TElement> elementSelector, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer)
	{
		Requires.NotNull(keySelector, "keySelector");
		Requires.NotNull(elementSelector, "elementSelector");
		Dictionary<TKey, TElement> dictionary = new Dictionary<TKey, TElement>(immutableArray.Length, comparer);
		T[] array = immutableArray.array;
		foreach (T arg in array)
		{
			dictionary.Add(keySelector(arg), elementSelector(arg));
		}
		return dictionary;
	}

	public static T[] ToArray<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] this System.Collections.Immutable.ImmutableArray<T> immutableArray)
	{
		immutableArray.ThrowNullRefIfNotInitialized();
		if (immutableArray.array.Length == 0)
		{
			return System.Collections.Immutable.ImmutableArray<T>.Empty.array;
		}
		return (T[])immutableArray.array.Clone();
	}

	public static T First<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(this System.Collections.Immutable.ImmutableArray<T>.Builder builder)
	{
		Requires.NotNull(builder, "builder");
		if (!builder.Any())
		{
			throw new InvalidOperationException();
		}
		return builder[0];
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	public static T FirstOrDefault<T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] this System.Collections.Immutable.ImmutableArray<T>.Builder builder)
	{
		Requires.NotNull(builder, "builder");
		if (!builder.Any())
		{
			return default(T);
		}
		return builder[0];
	}

	public static T Last<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(this System.Collections.Immutable.ImmutableArray<T>.Builder builder)
	{
		Requires.NotNull(builder, "builder");
		if (!builder.Any())
		{
			throw new InvalidOperationException();
		}
		return builder[builder.Count - 1];
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	public static T LastOrDefault<T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] this System.Collections.Immutable.ImmutableArray<T>.Builder builder)
	{
		Requires.NotNull(builder, "builder");
		if (!builder.Any())
		{
			return default(T);
		}
		return builder[builder.Count - 1];
	}

	public static bool Any<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(this System.Collections.Immutable.ImmutableArray<T>.Builder builder)
	{
		Requires.NotNull(builder, "builder");
		return builder.Count > 0;
	}

	private static IEnumerable<TResult> SelectManyIterator<TSource, TCollection, TResult>(this System.Collections.Immutable.ImmutableArray<TSource> immutableArray, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
	{
		TSource[] array = immutableArray.array;
		foreach (TSource item in array)
		{
			foreach (TCollection item2 in collectionSelector(item))
			{
				yield return resultSelector(item, item2);
			}
		}
	}
}
