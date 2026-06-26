using System;
using System.Collections.Generic;

namespace UniLinq;

[PublicizedFrom(EAccessModifier.Internal)]
public class SortSequenceContext<TElement, TKey> : SortContext<TElement>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Func<TElement, TKey> selector;

	[PublicizedFrom(EAccessModifier.Private)]
	public IComparer<TKey> comparer;

	[PublicizedFrom(EAccessModifier.Private)]
	public TKey[] keys;

	public SortSequenceContext(Func<TElement, TKey> selector, IComparer<TKey> comparer, SortDirection direction, SortContext<TElement> child_context)
		: base(direction, child_context)
	{
		this.selector = selector;
		this.comparer = comparer;
	}

	public override void Initialize(TElement[] elements)
	{
		if (child_context != null)
		{
			child_context.Initialize(elements);
		}
		keys = new TKey[elements.Length];
		for (int i = 0; i < keys.Length; i++)
		{
			keys[i] = selector(elements[i]);
		}
	}

	public override int Compare(int first_index, int second_index)
	{
		int num = comparer.Compare(keys[first_index], keys[second_index]);
		if (num == 0)
		{
			if (child_context != null)
			{
				return child_context.Compare(first_index, second_index);
			}
			num = ((direction == SortDirection.Descending) ? (second_index - first_index) : (first_index - second_index));
		}
		if (direction != SortDirection.Descending)
		{
			return num;
		}
		return -num;
	}
}
