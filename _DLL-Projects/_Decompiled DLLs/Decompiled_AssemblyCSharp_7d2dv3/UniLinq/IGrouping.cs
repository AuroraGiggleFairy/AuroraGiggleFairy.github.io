using System.Collections;
using System.Collections.Generic;

namespace UniLinq;

public interface IGrouping<TKey, TElement> : IEnumerable<TElement>, IEnumerable
{
	TKey Key { get; }
}
