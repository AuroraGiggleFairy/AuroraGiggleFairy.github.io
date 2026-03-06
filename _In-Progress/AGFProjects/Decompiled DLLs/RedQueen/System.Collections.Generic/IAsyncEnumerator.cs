using System.Threading.Tasks;

namespace System.Collections.Generic;

internal interface IAsyncEnumerator<out T> : IAsyncDisposable
{
	T Current { get; }

	ValueTask<bool> MoveNextAsync();
}
