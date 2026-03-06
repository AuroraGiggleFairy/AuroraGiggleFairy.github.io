using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Linq;

internal sealed class BinaryDisposable : IDisposable
{
	[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)]
	private IDisposable _d1;

	[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)]
	private IDisposable _d2;

	[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
	public BinaryDisposable(IDisposable d1, IDisposable d2)
	{
		_d1 = d1;
		_d2 = d2;
	}

	public void Dispose()
	{
		IDisposable disposable = Interlocked.Exchange(ref _d1, null);
		if (disposable != null)
		{
			disposable.Dispose();
			Interlocked.Exchange(ref _d2, null)?.Dispose();
		}
	}
}
