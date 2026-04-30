using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Discord;

internal class Page<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
{
	private readonly IReadOnlyCollection<T> _items;

	public int Index { get; }

	int IReadOnlyCollection<T>.Count => _items.Count;

	public Page(PageInfo info, IEnumerable<T> source)
	{
		Index = info.Page;
		_items = source.ToImmutableArray();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _items.GetEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return _items.GetEnumerator();
	}
}
