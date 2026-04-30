using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Linq;

internal sealed class AnonymousDisposable : IDisposable
{
	[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)]
	private Action _action;

	[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
	public AnonymousDisposable(Action action)
	{
		_action = action;
	}

	public void Dispose()
	{
		Interlocked.Exchange(ref _action, null)?.Invoke();
	}
}
