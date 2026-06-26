using System;
using System.Collections;
using System.Collections.Generic;

namespace UniLinq;

public class Lookup<TKey, TElement> : IEnumerable<IGrouping<TKey, TElement>>, IEnumerable, ILookup<TKey, TElement>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IGrouping<TKey, TElement> nullGrouping;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<TKey, IGrouping<TKey, TElement>> groups;

	public int Count
	{
		get
		{
			if (nullGrouping != null)
			{
				return groups.Count + 1;
			}
			return groups.Count;
		}
	}

	public IEnumerable<TElement> this[TKey key]
	{
		get
		{
			if (key == null && nullGrouping != null)
			{
				return nullGrouping;
			}
			if (key != null && groups.TryGetValue(key, out var value))
			{
				return value;
			}
			return new TElement[0];
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public Lookup(Dictionary<TKey, List<TElement>> lookup, IEnumerable<TElement> nullKeyElements)
	{
		groups = new Dictionary<TKey, IGrouping<TKey, TElement>>(lookup.Comparer);
		foreach (KeyValuePair<TKey, List<TElement>> item in lookup)
		{
			groups.Add(item.Key, new Grouping<TKey, TElement>(item.Key, item.Value));
		}
		if (nullKeyElements != null)
		{
			nullGrouping = new Grouping<TKey, TElement>(default(TKey), nullKeyElements);
		}
	}

	public IEnumerable<TResult> ApplyResultSelector<TResult>(Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
	{
		if (nullGrouping != null)
		{
			yield return resultSelector(nullGrouping.Key, nullGrouping);
		}
		foreach (KeyValuePair<TKey, IGrouping<TKey, TElement>> group in groups)
		{
			yield return resultSelector(group.Value.Key, group.Value);
		}
	}

	public bool Contains(TKey key)
	{
		if (key == null)
		{
			return nullGrouping != null;
		}
		return groups.ContainsKey(key);
	}

	public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
	{
		if (nullGrouping != null)
		{
			yield return nullGrouping;
		}
		foreach (KeyValuePair<TKey, IGrouping<TKey, TElement>> group in groups)
		{
			yield return group.Value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
