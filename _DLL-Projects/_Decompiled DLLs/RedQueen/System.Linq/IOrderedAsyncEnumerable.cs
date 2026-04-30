using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
internal interface IOrderedAsyncEnumerable<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] out TElement> : IAsyncEnumerable<TElement>
{
	IOrderedAsyncEnumerable<TElement> CreateOrderedEnumerable<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TKey>(Func<TElement, TKey> keySelector, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })] IComparer<TKey> comparer, bool descending);

	IOrderedAsyncEnumerable<TElement> CreateOrderedEnumerable<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TKey>([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TElement, ValueTask<TKey>> keySelector, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })] IComparer<TKey> comparer, bool descending);

	IOrderedAsyncEnumerable<TElement> CreateOrderedEnumerable<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TKey>([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TElement, CancellationToken, ValueTask<TKey>> keySelector, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })] IComparer<TKey> comparer, bool descending);
}
