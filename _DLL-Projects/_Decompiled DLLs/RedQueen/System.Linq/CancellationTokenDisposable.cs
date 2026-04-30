using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Linq;

internal sealed class CancellationTokenDisposable : IDisposable
{
	[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(1)]
	private readonly CancellationTokenSource _cts = new CancellationTokenSource();

	public CancellationToken Token => _cts.Token;

	public void Dispose()
	{
		if (!_cts.IsCancellationRequested)
		{
			_cts.Cancel();
		}
	}
}
