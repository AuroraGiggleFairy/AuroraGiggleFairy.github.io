using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

internal interface IAsyncIListProvider<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TElement> : IAsyncEnumerable<TElement>
{
	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1, 1 })]
	ValueTask<TElement[]> ToArrayAsync(CancellationToken cancellationToken);

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1, 1 })]
	ValueTask<List<TElement>> ToListAsync(CancellationToken cancellationToken);

	ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken);
}
