using System.Threading;

namespace System.Collections.Generic;

internal interface IAsyncEnumerable<out T>
{
	IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken));
}
