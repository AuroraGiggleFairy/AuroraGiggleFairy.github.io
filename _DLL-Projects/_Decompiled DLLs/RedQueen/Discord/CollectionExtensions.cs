using System;
using System.Collections.Generic;

namespace Discord;

internal static class CollectionExtensions
{
	public static IReadOnlyCollection<TValue> ToReadOnlyCollection<TValue>(this ICollection<TValue> source)
	{
		return new CollectionWrapper<TValue>(source, () => source.Count);
	}

	public static IReadOnlyCollection<TValue> ToReadOnlyCollection<TKey, TValue>(this IDictionary<TKey, TValue> source)
	{
		return new CollectionWrapper<TValue>(source.Values, () => source.Count);
	}

	public static IReadOnlyCollection<TValue> ToReadOnlyCollection<TValue, TSource>(this IEnumerable<TValue> query, IReadOnlyCollection<TSource> source)
	{
		return new CollectionWrapper<TValue>(query, () => source.Count);
	}

	public static IReadOnlyCollection<TValue> ToReadOnlyCollection<TValue>(this IEnumerable<TValue> query, Func<int> countFunc)
	{
		return new CollectionWrapper<TValue>(query, countFunc);
	}
}
