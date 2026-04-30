using System.Threading.Tasks;

namespace System;

internal interface IAsyncDisposable
{
	ValueTask DisposeAsync();
}
