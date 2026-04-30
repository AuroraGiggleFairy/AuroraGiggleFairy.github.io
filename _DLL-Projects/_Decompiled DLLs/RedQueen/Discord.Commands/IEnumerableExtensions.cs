using System;
using System.Collections.Generic;

namespace Discord.Commands;

internal static class IEnumerableExtensions
{
	public static IEnumerable<TResult> Permutate<TFirst, TSecond, TResult>(this IEnumerable<TFirst> set, IEnumerable<TSecond> others, Func<TFirst, TSecond, TResult> func)
	{
		foreach (TFirst elem in set)
		{
			foreach (TSecond other in others)
			{
				yield return func(elem, other);
			}
		}
	}
}
