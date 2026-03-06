using System.Collections;
using System.Collections.Generic;

public class ReadOnlyListWrapper<TIn, TOut> : IReadOnlyList<TOut>, IEnumerable<TOut>, IEnumerable, IReadOnlyCollection<TOut> where TIn : TOut
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IList<TIn> m_list;

	public int Count => m_list.Count;

	public TOut this[int index] => (TOut)(object)m_list[index];

	public ReadOnlyListWrapper(IList<TIn> list)
	{
		m_list = list;
	}

	public IEnumerator<TOut> GetEnumerator()
	{
		return (IEnumerator<TOut>)m_list.GetEnumerator();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_list.GetEnumerator();
	}
}
