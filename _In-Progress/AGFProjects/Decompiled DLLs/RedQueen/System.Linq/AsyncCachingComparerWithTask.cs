using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1 })]
internal class AsyncCachingComparerWithTask<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TElement, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TKey> : AsyncCachingComparer<TElement>
{
	[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 1, 1, 0, 1 })]
	protected readonly Func<TElement, ValueTask<TKey>> _keySelector;

	protected readonly IComparer<TKey> _comparer;

	protected readonly bool _descending;

	protected TKey _lastKey;

	public AsyncCachingComparerWithTask([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TElement, ValueTask<TKey>> keySelector, IComparer<TKey> comparer, bool descending)
	{
		_keySelector = keySelector;
		_comparer = comparer;
		_descending = descending;
		_lastKey = default(TKey);
	}

	[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(0)]
	internal override async ValueTask<int> Compare([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(1)] TElement element, bool cacheLower, CancellationToken cancellationToken)
	{
		TKey val = await _keySelector(element).ConfigureAwait(continueOnCapturedContext: false);
		int num = (_descending ? _comparer.Compare(_lastKey, val) : _comparer.Compare(val, _lastKey));
		if (cacheLower == num < 0)
		{
			_lastKey = val;
		}
		return num;
	}

	internal override async ValueTask SetElement(TElement element, CancellationToken cancellationToken)
	{
		_lastKey = await _keySelector(element).ConfigureAwait(continueOnCapturedContext: false);
	}
}
