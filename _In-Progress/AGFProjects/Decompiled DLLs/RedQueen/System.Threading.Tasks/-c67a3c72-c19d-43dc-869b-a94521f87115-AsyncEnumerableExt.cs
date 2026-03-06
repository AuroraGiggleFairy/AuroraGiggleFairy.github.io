using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks;

internal static class _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt
{
	[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(1)]
	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1 })]
	public static ConfiguredCancelableAsyncEnumerable<T>.Enumerator GetConfiguredAsyncEnumerator<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] T>(this IAsyncEnumerable<T> enumerable, CancellationToken cancellationToken, bool continueOnCapturedContext)
	{
		return enumerable.ConfigureAwait(continueOnCapturedContext).WithCancellation(cancellationToken).GetAsyncEnumerator();
	}
}
