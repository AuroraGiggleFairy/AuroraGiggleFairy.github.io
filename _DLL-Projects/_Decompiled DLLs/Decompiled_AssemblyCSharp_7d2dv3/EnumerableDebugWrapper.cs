using System.Collections;
using System.Collections.Generic;

public class EnumerableDebugWrapper<T> : EnumerableDebugWrapper, IEnumerable<T>, IEnumerable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public new readonly IEnumerable<T> m_enumerable;

	public EnumerableDebugWrapper(IEnumerable<T> enumerable)
		: this((DebugWrapper)null, enumerable)
	{
	}

	public EnumerableDebugWrapper(DebugWrapper parent, IEnumerable<T> enumerable)
		: base(parent, enumerable)
	{
		m_enumerable = enumerable;
	}

	public new IEnumerator<T> GetEnumerator()
	{
		return DebugEnumerator(m_enumerable.GetEnumerator());
	}
}
public class EnumerableDebugWrapper : DebugWrapper, IEnumerable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IEnumerable m_enumerable;

	public EnumerableDebugWrapper(IEnumerable enumerable)
		: this(null, enumerable)
	{
	}

	public EnumerableDebugWrapper(DebugWrapper parent, IEnumerable enumerable)
		: base(parent)
	{
		m_enumerable = enumerable;
	}

	public IEnumerator GetEnumerator()
	{
		return DebugEnumerator(m_enumerable.GetEnumerator());
	}
}
