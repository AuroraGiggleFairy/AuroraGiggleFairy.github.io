using System.Runtime.CompilerServices;

namespace System.Linq;

[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(0)]
[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
internal static class Disposable
{
	public static IDisposable Create(IDisposable d1, IDisposable d2)
	{
		return new BinaryDisposable(d1, d2);
	}

	public static IDisposable Create(Action action)
	{
		return new AnonymousDisposable(action);
	}
}
