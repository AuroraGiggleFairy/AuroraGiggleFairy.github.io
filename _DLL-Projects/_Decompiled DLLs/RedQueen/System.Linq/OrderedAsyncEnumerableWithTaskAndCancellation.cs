using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1 })]
internal sealed class OrderedAsyncEnumerableWithTaskAndCancellation<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TElement, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TKey> : OrderedAsyncEnumerable<TElement>
{
	private readonly IComparer<TKey> _comparer;

	private readonly bool _descending;

	[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 1, 1, 0, 1 })]
	private readonly Func<TElement, CancellationToken, ValueTask<TKey>> _keySelector;

	[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })]
	private readonly OrderedAsyncEnumerable<TElement> _parent;

	public OrderedAsyncEnumerableWithTaskAndCancellation(IAsyncEnumerable<TElement> source, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TElement, CancellationToken, ValueTask<TKey>> keySelector, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })] IComparer<TKey> comparer, bool descending, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })] OrderedAsyncEnumerable<TElement> parent)
		: base(source)
	{
		_keySelector = keySelector ?? throw Error.ArgumentNull("keySelector");
		_comparer = comparer ?? Comparer<TKey>.Default;
		_descending = descending;
		_parent = parent;
	}

	public override AsyncIteratorBase<TElement> Clone()
	{
		return new OrderedAsyncEnumerableWithTaskAndCancellation<TElement, TKey>(_source, _keySelector, _comparer, _descending, _parent);
	}

	internal override AsyncEnumerableSorter<TElement> GetAsyncEnumerableSorter([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })] AsyncEnumerableSorter<TElement> next, CancellationToken cancellationToken)
	{
		AsyncKeySelectorAsyncEnumerableSorterWithCancellation<TElement, TKey> asyncKeySelectorAsyncEnumerableSorterWithCancellation = new AsyncKeySelectorAsyncEnumerableSorterWithCancellation<TElement, TKey>(_keySelector, _comparer, _descending, next, cancellationToken);
		if (_parent != null)
		{
			return _parent.GetAsyncEnumerableSorter(asyncKeySelectorAsyncEnumerableSorterWithCancellation, cancellationToken);
		}
		return asyncKeySelectorAsyncEnumerableSorterWithCancellation;
	}

	internal override AsyncCachingComparer<TElement> GetComparer([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })] AsyncCachingComparer<TElement> childComparer)
	{
		AsyncCachingComparer<TElement> asyncCachingComparer = ((childComparer == null) ? new AsyncCachingComparerWithTaskAndCancellation<TElement, TKey>(_keySelector, _comparer, _descending) : new AsyncCachingComparerWithTaskAndCancellationAndChild<TElement, TKey>(_keySelector, _comparer, _descending, childComparer));
		if (_parent == null)
		{
			return asyncCachingComparer;
		}
		return _parent.GetComparer(asyncCachingComparer);
	}
}
