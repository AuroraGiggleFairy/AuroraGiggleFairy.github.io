using System.Collections;
using System.Collections.Generic;

namespace UniLinq;

public interface ILookup<TKey, TElement> : IEnumerable<IGrouping<TKey, TElement>>, IEnumerable
{
	int Count { get; }

	IEnumerable<TElement> this[TKey key] { get; }

	bool Contains(TKey key);
}
