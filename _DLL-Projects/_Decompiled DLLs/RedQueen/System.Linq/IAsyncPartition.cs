using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
internal interface IAsyncPartition<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TElement> : IAsyncIListProvider<TElement>, IAsyncEnumerable<TElement>
{
	IAsyncPartition<TElement> Skip(int count);

	IAsyncPartition<TElement> Take(int count);

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 0, 1 })]
	ValueTask<Maybe<TElement>> TryGetElementAtAsync(int index, CancellationToken cancellationToken);

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 0, 1 })]
	ValueTask<Maybe<TElement>> TryGetFirstAsync(CancellationToken cancellationToken);

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 0, 1 })]
	ValueTask<Maybe<TElement>> TryGetLastAsync(CancellationToken cancellationToken);
}
