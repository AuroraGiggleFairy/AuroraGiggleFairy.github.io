using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq.Internal;

[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(0)]
internal class Lookup<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TKey, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TElement> : ILookup<TKey, TElement>, IEnumerable<IGrouping<TKey, TElement>>, IEnumerable, IAsyncIListProvider<IAsyncGrouping<TKey, TElement>>, IAsyncEnumerable<IAsyncGrouping<TKey, TElement>>
{
	private readonly IEqualityComparer<TKey> _comparer;

	private Grouping<TKey, TElement>[] _groupings;

	[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1, 1 })]
	private Grouping<TKey, TElement> _lastGrouping;

	public int Count { get; private set; }

	public IEnumerable<TElement> this[TKey key]
	{
		get
		{
			Grouping<TKey, TElement> grouping = GetGrouping(key);
			if (grouping != null)
			{
				return grouping;
			}
			return Array.Empty<TElement>();
		}
	}

	private Lookup([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer)
	{
		_comparer = comparer ?? EqualityComparer<TKey>.Default;
		_groupings = new Grouping<TKey, TElement>[7];
	}

	public bool Contains(TKey key)
	{
		return GetGrouping(key) != null;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
	{
		Grouping<TKey, TElement> g = _lastGrouping;
		if (g != null)
		{
			do
			{
				g = g._next;
				yield return g;
			}
			while (g != _lastGrouping);
		}
	}

	public IEnumerable<TResult> ApplyResultSelector<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TResult>(Func<TKey, IAsyncEnumerable<TElement>, TResult> resultSelector)
	{
		Grouping<TKey, TElement> g = _lastGrouping;
		if (g != null)
		{
			do
			{
				g = g._next;
				g.Trim();
				yield return resultSelector(g._key, g._elements.ToAsyncEnumerable());
			}
			while (g != _lastGrouping);
		}
	}

	internal static async Task<Lookup<TKey, TElement>> CreateAsync<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TSource>(IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		Lookup<TKey, TElement> lookup = new Lookup<TKey, TElement>(comparer);
		await foreach (TSource item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			TKey key = keySelector(item);
			Grouping<TKey, TElement> orCreateGrouping = lookup.GetOrCreateGrouping(key);
			TElement element = elementSelector(item);
			orCreateGrouping.Add(element);
		}
		return lookup;
	}

	internal static async Task<Lookup<TKey, TElement>> CreateAsync(IAsyncEnumerable<TElement> source, Func<TElement, TKey> keySelector, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		Lookup<TKey, TElement> lookup = new Lookup<TKey, TElement>(comparer);
		await foreach (TElement item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			TKey key = keySelector(item);
			lookup.GetOrCreateGrouping(key).Add(item);
		}
		return lookup;
	}

	internal static async Task<Lookup<TKey, TElement>> CreateForJoinAsync(IAsyncEnumerable<TElement> source, Func<TElement, TKey> keySelector, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		Lookup<TKey, TElement> lookup = new Lookup<TKey, TElement>(comparer);
		await foreach (TElement item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			TKey val = keySelector(item);
			if (val != null)
			{
				lookup.GetOrCreateGrouping(val).Add(item);
			}
		}
		return lookup;
	}

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1, 1 })]
	internal Grouping<TKey, TElement> GetGrouping(TKey key)
	{
		int hashCode = InternalGetHashCode(key);
		return GetGrouping(key, hashCode);
	}

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1, 1 })]
	internal Grouping<TKey, TElement> GetGrouping(TKey key, int hashCode)
	{
		for (Grouping<TKey, TElement> grouping = _groupings[hashCode % _groupings.Length]; grouping != null; grouping = grouping._hashNext)
		{
			if (grouping._hashCode == hashCode && _comparer.Equals(grouping._key, key))
			{
				return grouping;
			}
		}
		return null;
	}

	internal Grouping<TKey, TElement> GetOrCreateGrouping(TKey key)
	{
		int num = InternalGetHashCode(key);
		Grouping<TKey, TElement> grouping = GetGrouping(key, num);
		if (grouping != null)
		{
			return grouping;
		}
		if (Count == _groupings.Length)
		{
			Resize();
		}
		int num2 = num % _groupings.Length;
		Grouping<TKey, TElement> grouping2 = new Grouping<TKey, TElement>(key, num, new TElement[1], _groupings[num2]);
		_groupings[num2] = grouping2;
		if (_lastGrouping == null)
		{
			grouping2._next = grouping2;
		}
		else
		{
			grouping2._next = _lastGrouping._next;
			_lastGrouping._next = grouping2;
		}
		_lastGrouping = grouping2;
		Count++;
		return grouping2;
	}

	internal int InternalGetHashCode(TKey key)
	{
		if (key != null)
		{
			return _comparer.GetHashCode(key) & 0x7FFFFFFF;
		}
		return 0;
	}

	internal TResult[] ToArray<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TResult>(Func<TKey, IAsyncEnumerable<TElement>, TResult> resultSelector)
	{
		TResult[] array = new TResult[Count];
		int num = 0;
		Grouping<TKey, TElement> grouping = _lastGrouping;
		if (grouping != null)
		{
			do
			{
				grouping = grouping._next;
				grouping.Trim();
				array[num] = resultSelector(grouping._key, grouping._elements.ToAsyncEnumerable());
				num++;
			}
			while (grouping != _lastGrouping);
		}
		return array;
	}

	internal List<TResult> ToList<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TResult>(Func<TKey, IAsyncEnumerable<TElement>, TResult> resultSelector)
	{
		List<TResult> list = new List<TResult>(Count);
		Grouping<TKey, TElement> grouping = _lastGrouping;
		if (grouping != null)
		{
			do
			{
				grouping = grouping._next;
				grouping.Trim();
				TResult item = resultSelector(grouping._key, grouping._elements.ToAsyncEnumerable());
				list.Add(item);
			}
			while (grouping != _lastGrouping);
		}
		return list;
	}

	private void Resize()
	{
		int num = checked(Count * 2 + 1);
		Grouping<TKey, TElement>[] array = new Grouping<TKey, TElement>[num];
		Grouping<TKey, TElement> grouping = _lastGrouping;
		do
		{
			grouping = grouping._next;
			int num2 = grouping._hashCode % num;
			grouping._hashNext = array[num2];
			array[num2] = grouping;
		}
		while (grouping != _lastGrouping);
		_groupings = array;
	}

	[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(0)]
	public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
	{
		return new ValueTask<int>(Count);
	}

	IAsyncEnumerator<IAsyncGrouping<TKey, TElement>> IAsyncEnumerable<IAsyncGrouping<TKey, TElement>>.GetAsyncEnumerator(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return Enumerable.Cast<IAsyncGrouping<TKey, TElement>>(this).ToAsyncEnumerable().GetAsyncEnumerator(cancellationToken);
	}

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1, 1, 1, 1 })]
	ValueTask<List<IAsyncGrouping<TKey, TElement>>> IAsyncIListProvider<IAsyncGrouping<TKey, TElement>>.ToListAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		List<IAsyncGrouping<TKey, TElement>> list = new List<IAsyncGrouping<TKey, TElement>>(Count);
		Grouping<TKey, TElement> grouping = _lastGrouping;
		if (grouping != null)
		{
			do
			{
				grouping = grouping._next;
				list.Add(grouping);
			}
			while (grouping != _lastGrouping);
		}
		return new ValueTask<List<IAsyncGrouping<TKey, TElement>>>(list);
	}

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1, 1, 1, 1 })]
	ValueTask<IAsyncGrouping<TKey, TElement>[]> IAsyncIListProvider<IAsyncGrouping<TKey, TElement>>.ToArrayAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		IAsyncGrouping<TKey, TElement>[] array = new IAsyncGrouping<TKey, TElement>[Count];
		int num = 0;
		Grouping<TKey, TElement> grouping = _lastGrouping;
		if (grouping != null)
		{
			do
			{
				grouping = (Grouping<TKey, TElement>)(array[num] = grouping._next);
				num++;
			}
			while (grouping != _lastGrouping);
		}
		return new ValueTask<IAsyncGrouping<TKey, TElement>[]>(array);
	}
}
