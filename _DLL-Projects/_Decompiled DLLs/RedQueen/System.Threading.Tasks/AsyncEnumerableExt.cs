using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks;

internal static class AsyncEnumerableExt
{
	[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1 })]
	public static ConfiguredCancelableAsyncEnumerable<T>.Enumerator GetConfiguredAsyncEnumerator<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] T>(this IAsyncEnumerable<T> enumerable, CancellationToken cancellationToken, bool continueOnCapturedContext)
	{
		return enumerable.ConfigureAwait(continueOnCapturedContext).WithCancellation(cancellationToken).GetAsyncEnumerator();
	}
}
