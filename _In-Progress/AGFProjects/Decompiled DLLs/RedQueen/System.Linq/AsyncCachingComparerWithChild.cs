using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1, 1 })]
[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
internal sealed class AsyncCachingComparerWithChild<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TElement, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TKey> : AsyncCachingComparer<TElement, TKey>
{
	private readonly AsyncCachingComparer<TElement> _child;

	public AsyncCachingComparerWithChild(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending, AsyncCachingComparer<TElement> child)
		: base(keySelector, comparer, descending)
	{
		_child = child;
	}

	[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(0)]
	internal override async ValueTask<int> Compare([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(1)] TElement element, bool cacheLower, CancellationToken cancellationToken)
	{
		TKey val = _keySelector(element);
		int cmp = (_descending ? _comparer.Compare(_lastKey, val) : _comparer.Compare(val, _lastKey));
		if (cmp == 0)
		{
			return await _child.Compare(element, cacheLower, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (cacheLower == cmp < 0)
		{
			_lastKey = val;
			await _child.SetElement(element, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return cmp;
	}

	internal override async ValueTask SetElement(TElement element, CancellationToken cancellationToken)
	{
		await base.SetElement(element, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await _child.SetElement(element, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}
}
