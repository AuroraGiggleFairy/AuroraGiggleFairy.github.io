using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

internal abstract class AsyncCachingComparer<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TElement>
{
	internal abstract ValueTask<int> Compare([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(1)] TElement element, bool cacheLower, CancellationToken cancellationToken);

	[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
	internal abstract ValueTask SetElement(TElement element, CancellationToken cancellationToken);
}
[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1 })]
internal class AsyncCachingComparer<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TElement, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TKey> : AsyncCachingComparer<TElement>
{
	protected readonly Func<TElement, TKey> _keySelector;

	protected readonly IComparer<TKey> _comparer;

	protected readonly bool _descending;

	protected TKey _lastKey;

	public AsyncCachingComparer(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
	{
		_keySelector = keySelector;
		_comparer = comparer;
		_descending = descending;
		_lastKey = default(TKey);
	}

	[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(0)]
	internal override ValueTask<int> Compare([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(1)] TElement element, bool cacheLower, CancellationToken cancellationToken)
	{
		TKey val = _keySelector(element);
		int num = (_descending ? _comparer.Compare(_lastKey, val) : _comparer.Compare(val, _lastKey));
		if (cacheLower == num < 0)
		{
			_lastKey = val;
		}
		return new ValueTask<int>(num);
	}

	internal override ValueTask SetElement(TElement element, CancellationToken cancellationToken)
	{
		_lastKey = _keySelector(element);
		return default(ValueTask);
	}
}
