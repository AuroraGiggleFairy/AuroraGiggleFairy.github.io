using System;
using System.Collections.Generic;

namespace UniLinq;

[PublicizedFrom(EAccessModifier.Internal)]
public class QuickSort<TElement>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TElement[] elements;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] indexes;

	[PublicizedFrom(EAccessModifier.Private)]
	public SortContext<TElement> context;

	[PublicizedFrom(EAccessModifier.Private)]
	public QuickSort(IEnumerable<TElement> source, SortContext<TElement> context)
	{
		List<TElement> list = new List<TElement>();
		foreach (TElement item in source)
		{
			list.Add(item);
		}
		elements = list.ToArray();
		indexes = CreateIndexes(elements.Length);
		this.context = context;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] CreateIndexes(int length)
	{
		int[] array = new int[length];
		for (int i = 0; i < length; i++)
		{
			array[i] = i;
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PerformSort()
	{
		if (elements.Length > 1)
		{
			context.Initialize(elements);
			Array.Sort(indexes, context);
		}
	}

	public static IEnumerable<TElement> Sort(IEnumerable<TElement> source, SortContext<TElement> context)
	{
		QuickSort<TElement> sorter = new QuickSort<TElement>(source, context);
		sorter.PerformSort();
		for (int i = 0; i < sorter.elements.Length; i++)
		{
			yield return sorter.elements[sorter.indexes[i]];
		}
	}
}
