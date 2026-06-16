using System;
using System.Collections;
using System.Collections.Generic;

namespace UniLinq;

public static class Enumerable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum Fallback
	{
		Default,
		Throw
	}

	public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
	{
		Check.SourceAndFunc(source, func);
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw EmptySequence();
		}
		TSource val = enumerator.Current;
		while (enumerator.MoveNext())
		{
			val = func(val, enumerator.Current);
		}
		return val;
	}

	public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
	{
		Check.SourceAndFunc(source, func);
		TAccumulate val = seed;
		foreach (TSource item in source)
		{
			val = func(val, item);
		}
		return val;
	}

	public static TResult Aggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
	{
		Check.SourceAndFunc(source, func);
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		TAccumulate val = seed;
		foreach (TSource item in source)
		{
			val = func(val, item);
		}
		return resultSelector(val);
	}

	public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		Check.SourceAndPredicate(source, predicate);
		foreach (TSource item in source)
		{
			if (!predicate(item))
			{
				return false;
			}
		}
		return true;
	}

	public static bool Any<TSource>(this IEnumerable<TSource> source)
	{
		Check.Source(source);
		if (source is ICollection<TSource> collection)
		{
			return collection.Count > 0;
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		return enumerator.MoveNext();
	}

	public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		Check.SourceAndPredicate(source, predicate);
		foreach (TSource item in source)
		{
			if (predicate(item))
			{
				return true;
			}
		}
		return false;
	}

	public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source)
	{
		return source;
	}

	public static double Average(this IEnumerable<int> source)
	{
		Check.Source(source);
		long num = 0L;
		int num2 = 0;
		foreach (int item in source)
		{
			num = checked(num + item);
			num2++;
		}
		if (num2 == 0)
		{
			throw EmptySequence();
		}
		return (double)num / (double)num2;
	}

	public static double Average(this IEnumerable<long> source)
	{
		Check.Source(source);
		long num = 0L;
		long num2 = 0L;
		foreach (long item in source)
		{
			num += item;
			num2++;
		}
		if (num2 == 0L)
		{
			throw EmptySequence();
		}
		return (double)num / (double)num2;
	}

	public static double Average(this IEnumerable<double> source)
	{
		Check.Source(source);
		double num = 0.0;
		long num2 = 0L;
		foreach (double item in source)
		{
			num += item;
			num2++;
		}
		if (num2 == 0L)
		{
			throw EmptySequence();
		}
		return num / (double)num2;
	}

	public static float Average(this IEnumerable<float> source)
	{
		Check.Source(source);
		float num = 0f;
		long num2 = 0L;
		foreach (float item in source)
		{
			num += item;
			num2++;
		}
		if (num2 == 0L)
		{
			throw EmptySequence();
		}
		return num / (float)num2;
	}

	public static decimal Average(this IEnumerable<decimal> source)
	{
		Check.Source(source);
		decimal num = default(decimal);
		long num2 = 0L;
		foreach (decimal item in source)
		{
			num += item;
			num2++;
		}
		if (num2 == 0L)
		{
			throw EmptySequence();
		}
		return num / (decimal)num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TResult? AverageNullable<TElement, TAggregate, TResult>(this IEnumerable<TElement?> source, Func<TAggregate, TElement, TAggregate> func, Func<TAggregate, long, TResult> result) where TElement : struct where TAggregate : struct where TResult : struct
	{
		Check.Source(source);
		TAggregate arg = default(TAggregate);
		long num = 0L;
		foreach (TElement? item in source)
		{
			if (item.HasValue)
			{
				arg = func(arg, item.Value);
				num++;
			}
		}
		if (num == 0L)
		{
			return null;
		}
		return result(arg, num);
	}

	public static double? Average(this IEnumerable<int?> source)
	{
		Check.Source(source);
		long num = 0L;
		long num2 = 0L;
		foreach (int? item in source)
		{
			if (item.HasValue)
			{
				num += item.Value;
				num2++;
			}
		}
		if (num2 == 0L)
		{
			return null;
		}
		return (double)num / (double)num2;
	}

	public static double? Average(this IEnumerable<long?> source)
	{
		Check.Source(source);
		long num = 0L;
		long num2 = 0L;
		foreach (long? item in source)
		{
			if (item.HasValue)
			{
				num = checked(num + item.Value);
				num2++;
			}
		}
		if (num2 == 0L)
		{
			return null;
		}
		return (double)num / (double)num2;
	}

	public static double? Average(this IEnumerable<double?> source)
	{
		Check.Source(source);
		double num = 0.0;
		long num2 = 0L;
		foreach (double? item in source)
		{
			if (item.HasValue)
			{
				num += item.Value;
				num2++;
			}
		}
		if (num2 == 0L)
		{
			return null;
		}
		return num / (double)num2;
	}

	public static decimal? Average(this IEnumerable<decimal?> source)
	{
		Check.Source(source);
		decimal num = default(decimal);
		long num2 = 0L;
		foreach (decimal? item in source)
		{
			if (item.HasValue)
			{
				num += item.Value;
				num2++;
			}
		}
		if (num2 == 0L)
		{
			return null;
		}
		return num / (decimal)num2;
	}

	public static float? Average(this IEnumerable<float?> source)
	{
		Check.Source(source);
		float num = 0f;
		long num2 = 0L;
		foreach (float? item in source)
		{
			if (item.HasValue)
			{
				num += item.Value;
				num2++;
			}
		}
		if (num2 == 0L)
		{
			return null;
		}
		return num / (float)num2;
	}

	public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
	{
		Check.SourceAndSelector(source, selector);
		long num = 0L;
		long num2 = 0L;
		foreach (TSource item in source)
		{
			num += selector(item);
			num2++;
		}
		if (num2 == 0L)
		{
			throw EmptySequence();
		}
		return (double)num / (double)num2;
	}

	public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
	{
		Check.SourceAndSelector(source, selector);
		long num = 0L;
		long num2 = 0L;
		foreach (TSource item in source)
		{
			int? num3 = selector(item);
			if (num3.HasValue)
			{
				num += num3.Value;
				num2++;
			}
		}
		if (num2 == 0L)
		{
			return null;
		}
		return (double)num / (double)num2;
	}

	public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
	{
		Check.SourceAndSelector(source, selector);
		long num = 0L;
		long num2 = 0L;
		foreach (TSource item in source)
		{
			num = checked(num + selector(item));
			num2++;
		}
		if (num2 == 0L)
		{
			throw EmptySequence();
		}
		return (double)num / (double)num2;
	}

	public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
	{
		Check.SourceAndSelector(source, selector);
		long num = 0L;
		long num2 = 0L;
		foreach (TSource item in source)
		{
			long? num3 = selector(item);
			if (num3.HasValue)
			{
				num = checked(num + num3.Value);
				num2++;
			}
		}
		if (num2 == 0L)
		{
			return null;
		}
		return (double)num / (double)num2;
	}

	public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
	{
		Check.SourceAndSelector(source, selector);
		double num = 0.0;
		long num2 = 0L;
		foreach (TSource item in source)
		{
			num += selector(item);
			num2++;
		}
		if (num2 == 0L)
		{
			throw EmptySequence();
		}
		return num / (double)num2;
	}

	public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
	{
		Check.SourceAndSelector(source, selector);
		double num = 0.0;
		long num2 = 0L;
		foreach (TSource item in source)
		{
			double? num3 = selector(item);
			if (num3.HasValue)
			{
				num += num3.Value;
				num2++;
			}
		}
		if (num2 == 0L)
		{
			return null;
		}
		return num / (double)num2;
	}

	public static float Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
	{
		Check.SourceAndSelector(source, selector);
		float num = 0f;
		long num2 = 0L;
		foreach (TSource item in source)
		{
			num += selector(item);
			num2++;
		}
		if (num2 == 0L)
		{
			throw EmptySequence();
		}
		return num / (float)num2;
	}

	public static float? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
	{
		Check.SourceAndSelector(source, selector);
		float num = 0f;
		long num2 = 0L;
		foreach (TSource item in source)
		{
			float? num3 = selector(item);
			if (num3.HasValue)
			{
				num += num3.Value;
				num2++;
			}
		}
		if (num2 == 0L)
		{
			return null;
		}
		return num / (float)num2;
	}

	public static decimal Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
	{
		Check.SourceAndSelector(source, selector);
		decimal num = default(decimal);
		long num2 = 0L;
		foreach (TSource item in source)
		{
			num += selector(item);
			num2++;
		}
		if (num2 == 0L)
		{
			throw EmptySequence();
		}
		return num / (decimal)num2;
	}

	public static decimal? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
	{
		Check.SourceAndSelector(source, selector);
		decimal num = default(decimal);
		long num2 = 0L;
		foreach (TSource item in source)
		{
			decimal? num3 = selector(item);
			if (num3.HasValue)
			{
				num += num3.Value;
				num2++;
			}
		}
		if (num2 == 0L)
		{
			return null;
		}
		return num / (decimal)num2;
	}

	public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source)
	{
		Check.Source(source);
		if (source is IEnumerable<TResult> result)
		{
			return result;
		}
		return CreateCastIterator<TResult>(source);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TResult> CreateCastIterator<TResult>(IEnumerable source)
	{
		foreach (TResult item in source)
		{
			yield return item;
		}
	}

	public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
	{
		Check.FirstAndSecond(first, second);
		return CreateConcatIterator(first, second);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TSource> CreateConcatIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second)
	{
		foreach (TSource item in first)
		{
			yield return item;
		}
		foreach (TSource item2 in second)
		{
			yield return item2;
		}
	}

	public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value)
	{
		if (source is ICollection<TSource> collection)
		{
			return collection.Contains(value);
		}
		return source.Contains(value, null);
	}

	public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
	{
		Check.Source(source);
		if (comparer == null)
		{
			comparer = EqualityComparer<TSource>.Default;
		}
		foreach (TSource item in source)
		{
			if (comparer.Equals(item, value))
			{
				return true;
			}
		}
		return false;
	}

	public static int Count<TSource>(this IEnumerable<TSource> source)
	{
		Check.Source(source);
		if (source is ICollection<TSource> collection)
		{
			return collection.Count;
		}
		int num = 0;
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			num = checked(num + 1);
		}
		return num;
	}

	public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		Check.SourceAndSelector(source, predicate);
		int num = 0;
		foreach (TSource item in source)
		{
			if (predicate(item))
			{
				num = checked(num + 1);
			}
		}
		return num;
	}

	public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source)
	{
		return source.DefaultIfEmpty(default(TSource));
	}

	public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source, TSource defaultValue)
	{
		Check.Source(source);
		return CreateDefaultIfEmptyIterator(source, defaultValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TSource> CreateDefaultIfEmptyIterator<TSource>(IEnumerable<TSource> source, TSource defaultValue)
	{
		bool empty = true;
		foreach (TSource item in source)
		{
			empty = false;
			yield return item;
		}
		if (empty)
		{
			yield return defaultValue;
		}
	}

	public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source)
	{
		return source.Distinct(null);
	}

	public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
	{
		Check.Source(source);
		if (comparer == null)
		{
			comparer = EqualityComparer<TSource>.Default;
		}
		return CreateDistinctIterator(source, comparer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TSource> CreateDistinctIterator<TSource>(IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
	{
		HashSet<TSource> items = new HashSet<TSource>(comparer);
		foreach (TSource item in source)
		{
			if (!items.Contains(item))
			{
				items.Add(item);
				yield return item;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index, Fallback fallback)
	{
		long num = 0L;
		foreach (TSource item in source)
		{
			if (index == num++)
			{
				return item;
			}
		}
		if (fallback == Fallback.Throw)
		{
			throw new ArgumentOutOfRangeException();
		}
		return default(TSource);
	}

	public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index)
	{
		Check.Source(source);
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (source is IList<TSource> list)
		{
			return list[index];
		}
		return source.ElementAt(index, Fallback.Throw);
	}

	public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index)
	{
		Check.Source(source);
		if (index < 0)
		{
			return default(TSource);
		}
		if (source is IList<TSource> list)
		{
			if (index >= list.Count)
			{
				return default(TSource);
			}
			return list[index];
		}
		return source.ElementAt(index, Fallback.Default);
	}

	public static IEnumerable<TResult> Empty<TResult>()
	{
		return new TResult[0];
	}

	public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
	{
		return first.Except(second, null);
	}

	public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
	{
		Check.FirstAndSecond(first, second);
		if (comparer == null)
		{
			comparer = EqualityComparer<TSource>.Default;
		}
		return CreateExceptIterator(first, second, comparer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TSource> CreateExceptIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
	{
		HashSet<TSource> items = new HashSet<TSource>(second, comparer);
		foreach (TSource item in first)
		{
			if (items.Add(item))
			{
				yield return item;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Fallback fallback)
	{
		foreach (TSource item in source)
		{
			if (predicate(item))
			{
				return item;
			}
		}
		if (fallback == Fallback.Throw)
		{
			throw NoMatchingElement();
		}
		return default(TSource);
	}

	public static TSource First<TSource>(this IEnumerable<TSource> source)
	{
		Check.Source(source);
		if (source is IList<TSource> list)
		{
			if (list.Count != 0)
			{
				return list[0];
			}
		}
		else
		{
			using IEnumerator<TSource> enumerator = source.GetEnumerator();
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		throw EmptySequence();
	}

	public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		Check.SourceAndPredicate(source, predicate);
		return source.First(predicate, Fallback.Throw);
	}

	public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
	{
		Check.Source(source);
		using (IEnumerator<TSource> enumerator = source.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		return default(TSource);
	}

	public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		Check.SourceAndPredicate(source, predicate);
		return source.First(predicate, Fallback.Default);
	}

	public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return source.GroupBy(keySelector, null);
	}

	public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		Check.SourceAndKeySelector(source, keySelector);
		return source.CreateGroupByIterator(keySelector, comparer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<IGrouping<TKey, TSource>> CreateGroupByIterator<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		Dictionary<TKey, List<TSource>> dictionary = new Dictionary<TKey, List<TSource>>(comparer);
		List<TSource> nullList = new List<TSource>();
		int counter = 0;
		int nullCounter = -1;
		foreach (TSource item in source)
		{
			TKey val = keySelector(item);
			if (val == null)
			{
				nullList.Add(item);
				if (nullCounter == -1)
				{
					nullCounter = counter;
					counter++;
				}
				continue;
			}
			if (!dictionary.TryGetValue(val, out var value))
			{
				value = new List<TSource>();
				dictionary.Add(val, value);
				counter++;
			}
			value.Add(item);
		}
		counter = 0;
		foreach (KeyValuePair<TKey, List<TSource>> group in dictionary)
		{
			if (counter == nullCounter)
			{
				yield return new Grouping<TKey, TSource>(default(TKey), nullList);
				counter++;
			}
			yield return new Grouping<TKey, TSource>(group.Key, group.Value);
			counter++;
		}
		if (counter == nullCounter)
		{
			yield return new Grouping<TKey, TSource>(default(TKey), nullList);
		}
	}

	public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
	{
		return source.GroupBy(keySelector, elementSelector, null);
	}

	public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
	{
		Check.SourceAndKeyElementSelectors(source, keySelector, elementSelector);
		return source.CreateGroupByIterator(keySelector, elementSelector, comparer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<IGrouping<TKey, TElement>> CreateGroupByIterator<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
	{
		Dictionary<TKey, List<TElement>> dictionary = new Dictionary<TKey, List<TElement>>(comparer);
		List<TElement> nullList = new List<TElement>();
		int counter = 0;
		int nullCounter = -1;
		foreach (TSource item2 in source)
		{
			TKey val = keySelector(item2);
			TElement item = elementSelector(item2);
			if (val == null)
			{
				nullList.Add(item);
				if (nullCounter == -1)
				{
					nullCounter = counter;
					counter++;
				}
				continue;
			}
			if (!dictionary.TryGetValue(val, out var value))
			{
				value = new List<TElement>();
				dictionary.Add(val, value);
				counter++;
			}
			value.Add(item);
		}
		counter = 0;
		foreach (KeyValuePair<TKey, List<TElement>> group in dictionary)
		{
			if (counter == nullCounter)
			{
				yield return new Grouping<TKey, TElement>(default(TKey), nullList);
				counter++;
			}
			yield return new Grouping<TKey, TElement>(group.Key, group.Value);
			counter++;
		}
		if (counter == nullCounter)
		{
			yield return new Grouping<TKey, TElement>(default(TKey), nullList);
		}
	}

	public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
	{
		return source.GroupBy(keySelector, elementSelector, resultSelector, null);
	}

	public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
	{
		Check.GroupBySelectors(source, keySelector, elementSelector, resultSelector);
		return source.CreateGroupByIterator(keySelector, elementSelector, resultSelector, comparer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TResult> CreateGroupByIterator<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
	{
		IEnumerable<IGrouping<TKey, TElement>> enumerable = source.GroupBy(keySelector, elementSelector, comparer);
		foreach (IGrouping<TKey, TElement> item in enumerable)
		{
			yield return resultSelector(item.Key, item);
		}
	}

	public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
	{
		return source.GroupBy(keySelector, resultSelector, null);
	}

	public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
	{
		Check.SourceAndKeyResultSelectors(source, keySelector, resultSelector);
		return source.CreateGroupByIterator(keySelector, resultSelector, comparer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TResult> CreateGroupByIterator<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
	{
		IEnumerable<IGrouping<TKey, TSource>> enumerable = source.GroupBy(keySelector, comparer);
		foreach (IGrouping<TKey, TSource> item in enumerable)
		{
			yield return resultSelector(item.Key, item);
		}
	}

	public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
	{
		return outer.GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector, null);
	}

	public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
	{
		Check.JoinSelectors(outer, inner, outerKeySelector, innerKeySelector, resultSelector);
		if (comparer == null)
		{
			comparer = EqualityComparer<TKey>.Default;
		}
		return outer.CreateGroupJoinIterator(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TResult> CreateGroupJoinIterator<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
	{
		ILookup<TKey, TInner> innerKeys = inner.ToLookup(innerKeySelector, comparer);
		foreach (TOuter item in outer)
		{
			TKey val = outerKeySelector(item);
			if (val != null && innerKeys.Contains(val))
			{
				yield return resultSelector(item, innerKeys[val]);
			}
			else
			{
				yield return resultSelector(item, Empty<TInner>());
			}
		}
	}

	public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
	{
		return first.Intersect(second, null);
	}

	public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
	{
		Check.FirstAndSecond(first, second);
		if (comparer == null)
		{
			comparer = EqualityComparer<TSource>.Default;
		}
		return CreateIntersectIterator(first, second, comparer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TSource> CreateIntersectIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
	{
		HashSet<TSource> items = new HashSet<TSource>(second, comparer);
		foreach (TSource item in first)
		{
			if (items.Remove(item))
			{
				yield return item;
			}
		}
	}

	public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
	{
		Check.JoinSelectors(outer, inner, outerKeySelector, innerKeySelector, resultSelector);
		if (comparer == null)
		{
			comparer = EqualityComparer<TKey>.Default;
		}
		return outer.CreateJoinIterator(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TResult> CreateJoinIterator<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
	{
		ILookup<TKey, TInner> innerKeys = inner.ToLookup(innerKeySelector, comparer);
		foreach (TOuter element in outer)
		{
			TKey val = outerKeySelector(element);
			if (val == null || !innerKeys.Contains(val))
			{
				continue;
			}
			foreach (TInner item in innerKeys[val])
			{
				yield return resultSelector(element, item);
			}
		}
	}

	public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector)
	{
		return outer.Join(inner, outerKeySelector, innerKeySelector, resultSelector, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Fallback fallback)
	{
		bool flag = true;
		TSource result = default(TSource);
		foreach (TSource item in source)
		{
			if (predicate(item))
			{
				result = item;
				flag = false;
			}
		}
		if (!flag)
		{
			return result;
		}
		if (fallback == Fallback.Throw)
		{
			throw NoMatchingElement();
		}
		return result;
	}

	public static TSource Last<TSource>(this IEnumerable<TSource> source)
	{
		Check.Source(source);
		if (source is ICollection<TSource> { Count: 0 })
		{
			throw EmptySequence();
		}
		if (source is IList<TSource> list)
		{
			return list[list.Count - 1];
		}
		bool flag = true;
		TSource result = default(TSource);
		foreach (TSource item in source)
		{
			result = item;
			flag = false;
		}
		if (!flag)
		{
			return result;
		}
		throw EmptySequence();
	}

	public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		Check.SourceAndPredicate(source, predicate);
		return source.Last(predicate, Fallback.Throw);
	}

	public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source)
	{
		Check.Source(source);
		if (source is IList<TSource> list)
		{
			if (list.Count <= 0)
			{
				return default(TSource);
			}
			return list[list.Count - 1];
		}
		bool flag = true;
		TSource result = default(TSource);
		foreach (TSource item in source)
		{
			result = item;
			flag = false;
		}
		return result;
	}

	public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		Check.SourceAndPredicate(source, predicate);
		return source.Last(predicate, Fallback.Default);
	}

	public static long LongCount<TSource>(this IEnumerable<TSource> source)
	{
		Check.Source(source);
		if (source is TSource[] array)
		{
			return array.LongLength;
		}
		long num = 0L;
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			num++;
		}
		return num;
	}

	public static long LongCount<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		Check.SourceAndSelector(source, predicate);
		long num = 0L;
		foreach (TSource item in source)
		{
			if (predicate(item))
			{
				num++;
			}
		}
		return num;
	}

	public static int Max(this IEnumerable<int> source)
	{
		Check.Source(source);
		bool flag = true;
		int num = int.MinValue;
		foreach (int item in source)
		{
			num = Math.Max(item, num);
			flag = false;
		}
		if (flag)
		{
			throw EmptySequence();
		}
		return num;
	}

	public static long Max(this IEnumerable<long> source)
	{
		Check.Source(source);
		bool flag = true;
		long num = long.MinValue;
		foreach (long item in source)
		{
			num = Math.Max(item, num);
			flag = false;
		}
		if (flag)
		{
			throw EmptySequence();
		}
		return num;
	}

	public static double Max(this IEnumerable<double> source)
	{
		Check.Source(source);
		bool flag = true;
		double num = double.MinValue;
		foreach (double item in source)
		{
			num = Math.Max(item, num);
			flag = false;
		}
		if (flag)
		{
			throw EmptySequence();
		}
		return num;
	}

	public static float Max(this IEnumerable<float> source)
	{
		Check.Source(source);
		bool flag = true;
		float num = float.MinValue;
		foreach (float item in source)
		{
			num = Math.Max(item, num);
			flag = false;
		}
		if (flag)
		{
			throw EmptySequence();
		}
		return num;
	}

	public static decimal Max(this IEnumerable<decimal> source)
	{
		Check.Source(source);
		bool flag = true;
		decimal num = decimal.MinValue;
		foreach (decimal item in source)
		{
			num = Math.Max(item, num);
			flag = false;
		}
		if (flag)
		{
			throw EmptySequence();
		}
		return num;
	}

	public static int? Max(this IEnumerable<int?> source)
	{
		Check.Source(source);
		bool flag = true;
		int num = int.MinValue;
		foreach (int? item in source)
		{
			if (item.HasValue)
			{
				num = Math.Max(item.Value, num);
				flag = false;
			}
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static long? Max(this IEnumerable<long?> source)
	{
		Check.Source(source);
		bool flag = true;
		long num = long.MinValue;
		foreach (long? item in source)
		{
			if (item.HasValue)
			{
				num = Math.Max(item.Value, num);
				flag = false;
			}
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static double? Max(this IEnumerable<double?> source)
	{
		Check.Source(source);
		bool flag = true;
		double num = double.MinValue;
		foreach (double? item in source)
		{
			if (item.HasValue)
			{
				num = Math.Max(item.Value, num);
				flag = false;
			}
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static float? Max(this IEnumerable<float?> source)
	{
		Check.Source(source);
		bool flag = true;
		float num = float.MinValue;
		foreach (float? item in source)
		{
			if (item.HasValue)
			{
				num = Math.Max(item.Value, num);
				flag = false;
			}
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static decimal? Max(this IEnumerable<decimal?> source)
	{
		Check.Source(source);
		bool flag = true;
		decimal num = decimal.MinValue;
		foreach (decimal? item in source)
		{
			if (item.HasValue)
			{
				num = Math.Max(item.Value, num);
				flag = false;
			}
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static TSource Max<TSource>(this IEnumerable<TSource> source)
	{
		Check.Source(source);
		Comparer<TSource> comparer = Comparer<TSource>.Default;
		TSource val = default(TSource);
		if (default(TSource) == null)
		{
			foreach (TSource item in source)
			{
				if (item != null && (val == null || comparer.Compare(item, val) > 0))
				{
					val = item;
				}
			}
		}
		else
		{
			bool flag = true;
			foreach (TSource item2 in source)
			{
				if (flag)
				{
					val = item2;
					flag = false;
				}
				else if (comparer.Compare(item2, val) > 0)
				{
					val = item2;
				}
			}
			if (flag)
			{
				throw EmptySequence();
			}
		}
		return val;
	}

	public static int Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		int num = int.MinValue;
		foreach (TSource item in source)
		{
			num = Math.Max(selector(item), num);
			flag = false;
		}
		if (flag)
		{
			throw NoMatchingElement();
		}
		return num;
	}

	public static long Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		long num = long.MinValue;
		foreach (TSource item in source)
		{
			num = Math.Max(selector(item), num);
			flag = false;
		}
		if (flag)
		{
			throw NoMatchingElement();
		}
		return num;
	}

	public static double Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		double num = double.MinValue;
		foreach (TSource item in source)
		{
			num = Math.Max(selector(item), num);
			flag = false;
		}
		if (flag)
		{
			throw NoMatchingElement();
		}
		return num;
	}

	public static float Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		float num = float.MinValue;
		foreach (TSource item in source)
		{
			num = Math.Max(selector(item), num);
			flag = false;
		}
		if (flag)
		{
			throw NoMatchingElement();
		}
		return num;
	}

	public static decimal Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		decimal num = decimal.MinValue;
		foreach (TSource item in source)
		{
			num = Math.Max(selector(item), num);
			flag = false;
		}
		if (flag)
		{
			throw NoMatchingElement();
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static U Iterate<T, U>(IEnumerable<T> source, U initValue, Func<T, U, U> selector)
	{
		bool flag = true;
		foreach (T item in source)
		{
			initValue = selector(item, initValue);
			flag = false;
		}
		if (flag)
		{
			throw NoMatchingElement();
		}
		return initValue;
	}

	public static int? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		int? num = null;
		foreach (TSource item in source)
		{
			int? num2 = selector(item);
			if (!num.HasValue)
			{
				num = num2;
			}
			else if (num2 > num)
			{
				num = num2;
			}
			flag = false;
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static long? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		long? num = null;
		foreach (TSource item in source)
		{
			long? num2 = selector(item);
			if (!num.HasValue)
			{
				num = num2;
			}
			else if (num2 > num)
			{
				num = num2;
			}
			flag = false;
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static double? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		double? num = null;
		foreach (TSource item in source)
		{
			double? num2 = selector(item);
			if (!num.HasValue)
			{
				num = num2;
			}
			else if (num2 > num)
			{
				num = num2;
			}
			flag = false;
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static float? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		float? num = null;
		foreach (TSource item in source)
		{
			float? num2 = selector(item);
			if (!num.HasValue)
			{
				num = num2;
			}
			else if (num2 > num)
			{
				num = num2;
			}
			flag = false;
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static decimal? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		decimal? num = null;
		foreach (TSource item in source)
		{
			decimal? num2 = selector(item);
			if (!num.HasValue)
			{
				num = num2;
			}
			else if (num2 > num)
			{
				num = num2;
			}
			flag = false;
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static TResult Max<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
	{
		Check.SourceAndSelector(source, selector);
		return source.Select(selector).Max();
	}

	public static int Min(this IEnumerable<int> source)
	{
		Check.Source(source);
		bool flag = true;
		int num = int.MaxValue;
		foreach (int item in source)
		{
			num = Math.Min(item, num);
			flag = false;
		}
		if (flag)
		{
			throw EmptySequence();
		}
		return num;
	}

	public static long Min(this IEnumerable<long> source)
	{
		Check.Source(source);
		bool flag = true;
		long num = long.MaxValue;
		foreach (long item in source)
		{
			num = Math.Min(item, num);
			flag = false;
		}
		if (flag)
		{
			throw EmptySequence();
		}
		return num;
	}

	public static double Min(this IEnumerable<double> source)
	{
		Check.Source(source);
		bool flag = true;
		double num = double.MaxValue;
		foreach (double item in source)
		{
			num = Math.Min(item, num);
			flag = false;
		}
		if (flag)
		{
			throw EmptySequence();
		}
		return num;
	}

	public static float Min(this IEnumerable<float> source)
	{
		Check.Source(source);
		bool flag = true;
		float num = float.MaxValue;
		foreach (float item in source)
		{
			num = Math.Min(item, num);
			flag = false;
		}
		if (flag)
		{
			throw EmptySequence();
		}
		return num;
	}

	public static decimal Min(this IEnumerable<decimal> source)
	{
		Check.Source(source);
		bool flag = true;
		decimal num = decimal.MaxValue;
		foreach (decimal item in source)
		{
			num = Math.Min(item, num);
			flag = false;
		}
		if (flag)
		{
			throw EmptySequence();
		}
		return num;
	}

	public static int? Min(this IEnumerable<int?> source)
	{
		Check.Source(source);
		bool flag = true;
		int num = int.MaxValue;
		foreach (int? item in source)
		{
			if (item.HasValue)
			{
				num = Math.Min(item.Value, num);
				flag = false;
			}
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static long? Min(this IEnumerable<long?> source)
	{
		Check.Source(source);
		bool flag = true;
		long num = long.MaxValue;
		foreach (long? item in source)
		{
			if (item.HasValue)
			{
				num = Math.Min(item.Value, num);
				flag = false;
			}
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static double? Min(this IEnumerable<double?> source)
	{
		Check.Source(source);
		bool flag = true;
		double num = double.MaxValue;
		foreach (double? item in source)
		{
			if (item.HasValue)
			{
				num = Math.Min(item.Value, num);
				flag = false;
			}
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static float? Min(this IEnumerable<float?> source)
	{
		Check.Source(source);
		bool flag = true;
		float num = float.MaxValue;
		foreach (float? item in source)
		{
			if (item.HasValue)
			{
				num = Math.Min(item.Value, num);
				flag = false;
			}
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static decimal? Min(this IEnumerable<decimal?> source)
	{
		Check.Source(source);
		bool flag = true;
		decimal num = decimal.MaxValue;
		foreach (decimal? item in source)
		{
			if (item.HasValue)
			{
				num = Math.Min(item.Value, num);
				flag = false;
			}
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static TSource Min<TSource>(this IEnumerable<TSource> source)
	{
		Check.Source(source);
		Comparer<TSource> comparer = Comparer<TSource>.Default;
		TSource val = default(TSource);
		if (default(TSource) == null)
		{
			foreach (TSource item in source)
			{
				if (item != null && (val == null || comparer.Compare(item, val) < 0))
				{
					val = item;
				}
			}
		}
		else
		{
			bool flag = true;
			foreach (TSource item2 in source)
			{
				if (flag)
				{
					val = item2;
					flag = false;
				}
				else if (comparer.Compare(item2, val) < 0)
				{
					val = item2;
				}
			}
			if (flag)
			{
				throw EmptySequence();
			}
		}
		return val;
	}

	public static int Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		int num = int.MaxValue;
		foreach (TSource item in source)
		{
			num = Math.Min(selector(item), num);
			flag = false;
		}
		if (flag)
		{
			throw NoMatchingElement();
		}
		return num;
	}

	public static long Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		long num = long.MaxValue;
		foreach (TSource item in source)
		{
			num = Math.Min(selector(item), num);
			flag = false;
		}
		if (flag)
		{
			throw NoMatchingElement();
		}
		return num;
	}

	public static double Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		double num = double.MaxValue;
		foreach (TSource item in source)
		{
			num = Math.Min(selector(item), num);
			flag = false;
		}
		if (flag)
		{
			throw NoMatchingElement();
		}
		return num;
	}

	public static float Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		float num = float.MaxValue;
		foreach (TSource item in source)
		{
			num = Math.Min(selector(item), num);
			flag = false;
		}
		if (flag)
		{
			throw NoMatchingElement();
		}
		return num;
	}

	public static decimal Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		decimal num = decimal.MaxValue;
		foreach (TSource item in source)
		{
			num = Math.Min(selector(item), num);
			flag = false;
		}
		if (flag)
		{
			throw NoMatchingElement();
		}
		return num;
	}

	public static int? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		int? num = null;
		foreach (TSource item in source)
		{
			int? num2 = selector(item);
			if (!num.HasValue)
			{
				num = num2;
			}
			else if (num2 < num)
			{
				num = num2;
			}
			flag = false;
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static long? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		long? num = null;
		foreach (TSource item in source)
		{
			long? num2 = selector(item);
			if (!num.HasValue)
			{
				num = num2;
			}
			else if (num2 < num)
			{
				num = num2;
			}
			flag = false;
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static float? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		float? num = null;
		foreach (TSource item in source)
		{
			float? num2 = selector(item);
			if (!num.HasValue)
			{
				num = num2;
			}
			else if (num2 < num)
			{
				num = num2;
			}
			flag = false;
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static double? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		double? num = null;
		foreach (TSource item in source)
		{
			double? num2 = selector(item);
			if (!num.HasValue)
			{
				num = num2;
			}
			else if (num2 < num)
			{
				num = num2;
			}
			flag = false;
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static decimal? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
	{
		Check.SourceAndSelector(source, selector);
		bool flag = true;
		decimal? num = null;
		foreach (TSource item in source)
		{
			decimal? num2 = selector(item);
			if (!num.HasValue)
			{
				num = num2;
			}
			else if (num2 < num)
			{
				num = num2;
			}
			flag = false;
		}
		if (flag)
		{
			return null;
		}
		return num;
	}

	public static TResult Min<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
	{
		Check.SourceAndSelector(source, selector);
		return source.Select(selector).Min();
	}

	public static IEnumerable<TResult> OfType<TResult>(this IEnumerable source)
	{
		Check.Source(source);
		return CreateOfTypeIterator<TResult>(source);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TResult> CreateOfTypeIterator<TResult>(IEnumerable source)
	{
		foreach (object item in source)
		{
			if (item is TResult)
			{
				yield return (TResult)item;
			}
		}
	}

	public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return source.OrderBy(keySelector, null);
	}

	public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
	{
		Check.SourceAndKeySelector(source, keySelector);
		return new OrderedSequence<TSource, TKey>(source, keySelector, comparer, SortDirection.Ascending);
	}

	public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return source.OrderByDescending(keySelector, null);
	}

	public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
	{
		Check.SourceAndKeySelector(source, keySelector);
		return new OrderedSequence<TSource, TKey>(source, keySelector, comparer, SortDirection.Descending);
	}

	public static IEnumerable<int> Range(int start, int count)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if ((long)start + (long)count - 1 > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException();
		}
		return CreateRangeIterator(start, count);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<int> CreateRangeIterator(int start, int count)
	{
		for (int i = 0; i < count; i++)
		{
			yield return start + i;
		}
	}

	public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		return CreateRepeatIterator(element, count);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TResult> CreateRepeatIterator<TResult>(TResult element, int count)
	{
		for (int i = 0; i < count; i++)
		{
			yield return element;
		}
	}

	public static IEnumerable<TSource> Reverse<TSource>(this IEnumerable<TSource> source)
	{
		Check.Source(source);
		return CreateReverseIterator(source);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TSource> CreateReverseIterator<TSource>(IEnumerable<TSource> source)
	{
		TSource[] array = source.ToArray();
		for (int i = array.Length - 1; i >= 0; i--)
		{
			yield return array[i];
		}
	}

	public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
	{
		Check.SourceAndSelector(source, selector);
		return CreateSelectIterator(source, selector);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TResult> CreateSelectIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
	{
		foreach (TSource item in source)
		{
			yield return selector(item);
		}
	}

	public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
	{
		Check.SourceAndSelector(source, selector);
		return CreateSelectIterator(source, selector);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TResult> CreateSelectIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
	{
		int counter = 0;
		foreach (TSource item in source)
		{
			yield return selector(item, counter);
			counter++;
		}
	}

	public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
	{
		Check.SourceAndSelector(source, selector);
		return CreateSelectManyIterator(source, selector);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TResult> CreateSelectManyIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
	{
		foreach (TSource item in source)
		{
			foreach (TResult item2 in selector(item))
			{
				yield return item2;
			}
		}
	}

	public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
	{
		Check.SourceAndSelector(source, selector);
		return CreateSelectManyIterator(source, selector);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TResult> CreateSelectManyIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
	{
		int counter = 0;
		foreach (TSource item in source)
		{
			foreach (TResult item2 in selector(item, counter))
			{
				yield return item2;
			}
			counter++;
		}
	}

	public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
	{
		Check.SourceAndCollectionSelectors(source, collectionSelector, resultSelector);
		return CreateSelectManyIterator(source, collectionSelector, resultSelector);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TResult> CreateSelectManyIterator<TSource, TCollection, TResult>(IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> selector)
	{
		foreach (TSource element in source)
		{
			foreach (TCollection item in collectionSelector(element))
			{
				yield return selector(element, item);
			}
		}
	}

	public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
	{
		Check.SourceAndCollectionSelectors(source, collectionSelector, resultSelector);
		return CreateSelectManyIterator(source, collectionSelector, resultSelector);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TResult> CreateSelectManyIterator<TSource, TCollection, TResult>(IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> selector)
	{
		int counter = 0;
		foreach (TSource element in source)
		{
			foreach (TCollection item in collectionSelector(element, counter++))
			{
				yield return selector(element, item);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Fallback fallback)
	{
		bool flag = false;
		TSource result = default(TSource);
		foreach (TSource item in source)
		{
			if (predicate(item))
			{
				if (flag)
				{
					throw MoreThanOneMatchingElement();
				}
				flag = true;
				result = item;
			}
		}
		if (!flag && fallback == Fallback.Throw)
		{
			throw NoMatchingElement();
		}
		return result;
	}

	public static TSource Single<TSource>(this IEnumerable<TSource> source)
	{
		Check.Source(source);
		bool flag = false;
		TSource result = default(TSource);
		foreach (TSource item in source)
		{
			if (flag)
			{
				throw MoreThanOneElement();
			}
			flag = true;
			result = item;
		}
		if (!flag)
		{
			throw NoMatchingElement();
		}
		return result;
	}

	public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		Check.SourceAndPredicate(source, predicate);
		return source.Single(predicate, Fallback.Throw);
	}

	public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source)
	{
		Check.Source(source);
		bool flag = false;
		TSource result = default(TSource);
		foreach (TSource item in source)
		{
			if (flag)
			{
				throw MoreThanOneMatchingElement();
			}
			flag = true;
			result = item;
		}
		return result;
	}

	public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		Check.SourceAndPredicate(source, predicate);
		return source.Single(predicate, Fallback.Default);
	}

	public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count)
	{
		Check.Source(source);
		return CreateSkipIterator(source, count);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TSource> CreateSkipIterator<TSource>(IEnumerable<TSource> source, int count)
	{
		IEnumerator<TSource> enumerator = source.GetEnumerator();
		try
		{
			do
			{
				if (count-- <= 0)
				{
					while (enumerator.MoveNext())
					{
						yield return enumerator.Current;
					}
					break;
				}
			}
			while (enumerator.MoveNext());
		}
		finally
		{
			enumerator.Dispose();
		}
	}

	public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		Check.SourceAndPredicate(source, predicate);
		return CreateSkipWhileIterator(source, predicate);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TSource> CreateSkipWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		bool yield = false;
		foreach (TSource item in source)
		{
			if (yield)
			{
				yield return item;
			}
			else if (!predicate(item))
			{
				yield return item;
				yield = true;
			}
		}
	}

	public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
	{
		Check.SourceAndPredicate(source, predicate);
		return CreateSkipWhileIterator(source, predicate);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TSource> CreateSkipWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
	{
		int counter = 0;
		bool yield = false;
		foreach (TSource item in source)
		{
			if (yield)
			{
				yield return item;
			}
			else if (!predicate(item, counter))
			{
				yield return item;
				yield = true;
			}
			counter++;
		}
	}

	public static int Sum(this IEnumerable<int> source)
	{
		Check.Source(source);
		int num = 0;
		foreach (int item in source)
		{
			num = checked(num + item);
		}
		return num;
	}

	public static int? Sum(this IEnumerable<int?> source)
	{
		Check.Source(source);
		int num = 0;
		foreach (int? item in source)
		{
			if (item.HasValue)
			{
				num = checked(num + item.Value);
			}
		}
		return num;
	}

	public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
	{
		Check.SourceAndSelector(source, selector);
		int num = 0;
		foreach (TSource item in source)
		{
			num = checked(num + selector(item));
		}
		return num;
	}

	public static int? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
	{
		Check.SourceAndSelector(source, selector);
		int num = 0;
		foreach (TSource item in source)
		{
			int? num2 = selector(item);
			if (num2.HasValue)
			{
				num = checked(num + num2.Value);
			}
		}
		return num;
	}

	public static long Sum(this IEnumerable<long> source)
	{
		Check.Source(source);
		long num = 0L;
		foreach (long item in source)
		{
			num = checked(num + item);
		}
		return num;
	}

	public static long? Sum(this IEnumerable<long?> source)
	{
		Check.Source(source);
		long num = 0L;
		foreach (long? item in source)
		{
			if (item.HasValue)
			{
				num = checked(num + item.Value);
			}
		}
		return num;
	}

	public static long Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
	{
		Check.SourceAndSelector(source, selector);
		long num = 0L;
		foreach (TSource item in source)
		{
			num = checked(num + selector(item));
		}
		return num;
	}

	public static long? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
	{
		Check.SourceAndSelector(source, selector);
		long num = 0L;
		foreach (TSource item in source)
		{
			long? num2 = selector(item);
			if (num2.HasValue)
			{
				num = checked(num + num2.Value);
			}
		}
		return num;
	}

	public static double Sum(this IEnumerable<double> source)
	{
		Check.Source(source);
		double num = 0.0;
		foreach (double item in source)
		{
			num += item;
		}
		return num;
	}

	public static double? Sum(this IEnumerable<double?> source)
	{
		Check.Source(source);
		double num = 0.0;
		foreach (double? item in source)
		{
			if (item.HasValue)
			{
				num += item.Value;
			}
		}
		return num;
	}

	public static double Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
	{
		Check.SourceAndSelector(source, selector);
		double num = 0.0;
		foreach (TSource item in source)
		{
			num += selector(item);
		}
		return num;
	}

	public static double? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
	{
		Check.SourceAndSelector(source, selector);
		double num = 0.0;
		foreach (TSource item in source)
		{
			double? num2 = selector(item);
			if (num2.HasValue)
			{
				num += num2.Value;
			}
		}
		return num;
	}

	public static float Sum(this IEnumerable<float> source)
	{
		Check.Source(source);
		float num = 0f;
		foreach (float item in source)
		{
			num += item;
		}
		return num;
	}

	public static float? Sum(this IEnumerable<float?> source)
	{
		Check.Source(source);
		float num = 0f;
		foreach (float? item in source)
		{
			if (item.HasValue)
			{
				num += item.Value;
			}
		}
		return num;
	}

	public static float Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
	{
		Check.SourceAndSelector(source, selector);
		float num = 0f;
		foreach (TSource item in source)
		{
			num += selector(item);
		}
		return num;
	}

	public static float? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
	{
		Check.SourceAndSelector(source, selector);
		float num = 0f;
		foreach (TSource item in source)
		{
			float? num2 = selector(item);
			if (num2.HasValue)
			{
				num += num2.Value;
			}
		}
		return num;
	}

	public static decimal Sum(this IEnumerable<decimal> source)
	{
		Check.Source(source);
		decimal result = default(decimal);
		foreach (decimal item in source)
		{
			result += item;
		}
		return result;
	}

	public static decimal? Sum(this IEnumerable<decimal?> source)
	{
		Check.Source(source);
		decimal value = default(decimal);
		foreach (decimal? item in source)
		{
			if (item.HasValue)
			{
				value += item.Value;
			}
		}
		return value;
	}

	public static decimal Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
	{
		Check.SourceAndSelector(source, selector);
		decimal result = default(decimal);
		foreach (TSource item in source)
		{
			result += selector(item);
		}
		return result;
	}

	public static decimal? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
	{
		Check.SourceAndSelector(source, selector);
		decimal value = default(decimal);
		foreach (TSource item in source)
		{
			decimal? num = selector(item);
			if (num.HasValue)
			{
				value += num.Value;
			}
		}
		return value;
	}

	public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count)
	{
		Check.Source(source);
		return CreateTakeIterator(source, count);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TSource> CreateTakeIterator<TSource>(IEnumerable<TSource> source, int count)
	{
		if (count <= 0)
		{
			yield break;
		}
		int counter = 0;
		foreach (TSource item in source)
		{
			yield return item;
			int num = counter + 1;
			counter = num;
			if (num == count)
			{
				yield break;
			}
		}
	}

	public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		Check.SourceAndPredicate(source, predicate);
		return CreateTakeWhileIterator(source, predicate);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TSource> CreateTakeWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		foreach (TSource item in source)
		{
			if (predicate(item))
			{
				yield return item;
				continue;
			}
			yield break;
		}
	}

	public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
	{
		Check.SourceAndPredicate(source, predicate);
		return CreateTakeWhileIterator(source, predicate);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TSource> CreateTakeWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
	{
		int counter = 0;
		foreach (TSource item in source)
		{
			if (predicate(item, counter))
			{
				yield return item;
				counter++;
				continue;
			}
			yield break;
		}
	}

	public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return source.ThenBy(keySelector, null);
	}

	public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
	{
		Check.SourceAndKeySelector(source, keySelector);
		return (source as OrderedEnumerable<TSource>).CreateOrderedEnumerable(keySelector, comparer, descending: false);
	}

	public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return source.ThenByDescending(keySelector, null);
	}

	public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
	{
		Check.SourceAndKeySelector(source, keySelector);
		return (source as OrderedEnumerable<TSource>).CreateOrderedEnumerable(keySelector, comparer, descending: true);
	}

	public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
	{
		Check.Source(source);
		TSource[] array;
		if (source is ICollection<TSource> collection)
		{
			if (collection.Count == 0)
			{
				return new TSource[0];
			}
			array = new TSource[collection.Count];
			collection.CopyTo(array, 0);
			return array;
		}
		int num = 0;
		array = new TSource[0];
		foreach (TSource item in source)
		{
			if (num == array.Length)
			{
				if (num == 0)
				{
					array = new TSource[4];
				}
				else
				{
					Array.Resize(ref array, num * 2);
				}
			}
			array[num++] = item;
		}
		if (num != array.Length)
		{
			Array.Resize(ref array, num);
		}
		return array;
	}

	public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
	{
		return source.ToDictionary(keySelector, elementSelector, null);
	}

	public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
	{
		Check.SourceAndKeyElementSelectors(source, keySelector, elementSelector);
		if (comparer == null)
		{
			comparer = EqualityComparer<TKey>.Default;
		}
		Dictionary<TKey, TElement> dictionary = new Dictionary<TKey, TElement>(comparer);
		foreach (TSource item in source)
		{
			dictionary.Add(keySelector(item), elementSelector(item));
		}
		return dictionary;
	}

	public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return source.ToDictionary(keySelector, null);
	}

	public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		Check.SourceAndKeySelector(source, keySelector);
		if (comparer == null)
		{
			comparer = EqualityComparer<TKey>.Default;
		}
		Dictionary<TKey, TSource> dictionary = new Dictionary<TKey, TSource>(comparer);
		foreach (TSource item in source)
		{
			dictionary.Add(keySelector(item), item);
		}
		return dictionary;
	}

	public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
	{
		Check.Source(source);
		return new List<TSource>(source);
	}

	public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return source.ToLookup(keySelector, null);
	}

	public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		Check.SourceAndKeySelector(source, keySelector);
		List<TSource> list = null;
		Dictionary<TKey, List<TSource>> dictionary = new Dictionary<TKey, List<TSource>>(comparer ?? EqualityComparer<TKey>.Default);
		foreach (TSource item in source)
		{
			TKey val = keySelector(item);
			List<TSource> value;
			if (val == null)
			{
				if (list == null)
				{
					list = new List<TSource>();
				}
				value = list;
			}
			else if (!dictionary.TryGetValue(val, out value))
			{
				value = new List<TSource>();
				dictionary.Add(val, value);
			}
			value.Add(item);
		}
		return new Lookup<TKey, TSource>(dictionary, list);
	}

	public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
	{
		return source.ToLookup(keySelector, elementSelector, null);
	}

	public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
	{
		Check.SourceAndKeyElementSelectors(source, keySelector, elementSelector);
		List<TElement> list = null;
		Dictionary<TKey, List<TElement>> dictionary = new Dictionary<TKey, List<TElement>>(comparer ?? EqualityComparer<TKey>.Default);
		foreach (TSource item in source)
		{
			TKey val = keySelector(item);
			List<TElement> value;
			if (val == null)
			{
				if (list == null)
				{
					list = new List<TElement>();
				}
				value = list;
			}
			else if (!dictionary.TryGetValue(val, out value))
			{
				value = new List<TElement>();
				dictionary.Add(val, value);
			}
			value.Add(elementSelector(item));
		}
		return new Lookup<TKey, TElement>(dictionary, list);
	}

	public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
	{
		return first.SequenceEqual(second, null);
	}

	public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
	{
		Check.FirstAndSecond(first, second);
		if (comparer == null)
		{
			comparer = EqualityComparer<TSource>.Default;
		}
		using IEnumerator<TSource> enumerator = first.GetEnumerator();
		using IEnumerator<TSource> enumerator2 = second.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (!enumerator2.MoveNext())
			{
				return false;
			}
			if (!comparer.Equals(enumerator.Current, enumerator2.Current))
			{
				return false;
			}
		}
		return !enumerator2.MoveNext();
	}

	public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
	{
		Check.FirstAndSecond(first, second);
		return first.Union(second, null);
	}

	public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
	{
		Check.FirstAndSecond(first, second);
		if (comparer == null)
		{
			comparer = EqualityComparer<TSource>.Default;
		}
		return CreateUnionIterator(first, second, comparer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TSource> CreateUnionIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
	{
		HashSet<TSource> items = new HashSet<TSource>(comparer);
		foreach (TSource item in first)
		{
			if (!items.Contains(item))
			{
				items.Add(item);
				yield return item;
			}
		}
		foreach (TSource item2 in second)
		{
			if (!items.Contains(item2))
			{
				items.Add(item2);
				yield return item2;
			}
		}
	}

	public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		Check.SourceAndPredicate(source, predicate);
		if (source is TSource[] source2)
		{
			return CreateWhereIterator(source2, predicate);
		}
		return CreateWhereIterator(source, predicate);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TSource> CreateWhereIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		foreach (TSource item in source)
		{
			if (predicate(item))
			{
				yield return item;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TSource> CreateWhereIterator<TSource>(TSource[] source, Func<TSource, bool> predicate)
	{
		int i = 0;
		while (i < source.Length)
		{
			TSource val = source[i];
			if (predicate(val))
			{
				yield return val;
			}
			int num = i + 1;
			i = num;
		}
	}

	public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
	{
		Check.SourceAndPredicate(source, predicate);
		if (source is TSource[] source2)
		{
			return CreateWhereIterator(source2, predicate);
		}
		return CreateWhereIterator(source, predicate);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TSource> CreateWhereIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
	{
		int counter = 0;
		foreach (TSource item in source)
		{
			if (predicate(item, counter))
			{
				yield return item;
			}
			counter++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<TSource> CreateWhereIterator<TSource>(TSource[] source, Func<TSource, int, bool> predicate)
	{
		int i = 0;
		while (i < source.Length)
		{
			TSource val = source[i];
			if (predicate(val, i))
			{
				yield return val;
			}
			int num = i + 1;
			i = num;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Exception EmptySequence()
	{
		return new InvalidOperationException("Sequence contains no elements");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Exception NoMatchingElement()
	{
		return new InvalidOperationException("Sequence contains no matching element");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Exception MoreThanOneElement()
	{
		return new InvalidOperationException("Sequence contains more than one element");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Exception MoreThanOneMatchingElement()
	{
		return new InvalidOperationException("Sequence contains more than one matching element");
	}
}
