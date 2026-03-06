using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal struct CollectionWrapper<TValue> : IReadOnlyCollection<TValue>, IEnumerable<TValue>, IEnumerable
{
	private readonly IEnumerable<TValue> _query;

	private readonly Func<int> _countFunc;

	public int Count => _countFunc();

	private string DebuggerDisplay => $"Count = {Count}";

	public CollectionWrapper(IEnumerable<TValue> query, Func<int> countFunc)
	{
		_query = query;
		_countFunc = countFunc;
	}

	public IEnumerator<TValue> GetEnumerator()
	{
		return _query.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _query.GetEnumerator();
	}
}
