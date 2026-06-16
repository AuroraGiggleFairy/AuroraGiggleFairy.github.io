using System.Collections.Generic;

namespace UniLinq;

[PublicizedFrom(EAccessModifier.Internal)]
public abstract class SortContext<TElement> : IComparer<int>
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public SortDirection direction;

	[PublicizedFrom(EAccessModifier.Protected)]
	public SortContext<TElement> child_context;

	[PublicizedFrom(EAccessModifier.Protected)]
	public SortContext(SortDirection direction, SortContext<TElement> child_context)
	{
		this.direction = direction;
		this.child_context = child_context;
	}

	public abstract void Initialize(TElement[] elements);

	public abstract int Compare(int first_index, int second_index);
}
