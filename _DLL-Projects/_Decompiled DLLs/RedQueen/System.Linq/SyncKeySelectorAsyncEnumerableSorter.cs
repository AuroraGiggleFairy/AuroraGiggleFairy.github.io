using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.Linq;

[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1, 1 })]
internal sealed class SyncKeySelectorAsyncEnumerableSorter<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TElement, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TKey> : AsyncEnumerableSorterBase<TElement, TKey>
{
	private readonly Func<TElement, TKey> _keySelector;

	public SyncKeySelectorAsyncEnumerableSorter(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })] AsyncEnumerableSorter<TElement> next)
		: base(comparer, descending, next)
	{
		_keySelector = keySelector;
	}

	internal override async ValueTask ComputeKeys(TElement[] elements, int count)
	{
		_keys = new TKey[count];
		for (int i = 0; i < count; i++)
		{
			_keys[i] = _keySelector(elements[i]);
		}
		if (_next != null)
		{
			await _next.ComputeKeys(elements, count).ConfigureAwait(continueOnCapturedContext: false);
		}
	}
}
