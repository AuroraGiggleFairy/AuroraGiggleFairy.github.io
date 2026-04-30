using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks;

internal static class TaskAsyncEnumerableExtensions
{
	public static ConfiguredAsyncDisposable ConfigureAwait(this IAsyncDisposable source, bool continueOnCapturedContext)
	{
		return new ConfiguredAsyncDisposable(source, continueOnCapturedContext);
	}

	public static ConfiguredCancelableAsyncEnumerable<T> ConfigureAwait<T>(this IAsyncEnumerable<T> source, bool continueOnCapturedContext)
	{
		return new ConfiguredCancelableAsyncEnumerable<T>(source, continueOnCapturedContext, default(CancellationToken));
	}

	public static ConfiguredCancelableAsyncEnumerable<T> WithCancellation<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken)
	{
		return new ConfiguredCancelableAsyncEnumerable<T>(source, continueOnCapturedContext: true, cancellationToken);
	}
}
