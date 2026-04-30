using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Runtime.CompilerServices;

[StructLayout(LayoutKind.Auto)]
internal readonly struct ConfiguredCancelableAsyncEnumerable<T>(IAsyncEnumerable<T> enumerable, bool continueOnCapturedContext, CancellationToken cancellationToken)
{
	[StructLayout(LayoutKind.Auto)]
	public readonly struct Enumerator(IAsyncEnumerator<T> enumerator, bool continueOnCapturedContext)
	{
		private readonly IAsyncEnumerator<T> _enumerator = enumerator;

		private readonly bool _continueOnCapturedContext = continueOnCapturedContext;

		public T Current => _enumerator.Current;

		public ConfiguredValueTaskAwaitable<bool> MoveNextAsync()
		{
			return _enumerator.MoveNextAsync().ConfigureAwait(_continueOnCapturedContext);
		}

		public ConfiguredValueTaskAwaitable DisposeAsync()
		{
			return _enumerator.DisposeAsync().ConfigureAwait(_continueOnCapturedContext);
		}
	}

	private readonly IAsyncEnumerable<T> _enumerable = enumerable;

	private readonly CancellationToken _cancellationToken = cancellationToken;

	private readonly bool _continueOnCapturedContext = continueOnCapturedContext;

	public ConfiguredCancelableAsyncEnumerable<T> ConfigureAwait(bool continueOnCapturedContext)
	{
		return new ConfiguredCancelableAsyncEnumerable<T>(_enumerable, continueOnCapturedContext, _cancellationToken);
	}

	public ConfiguredCancelableAsyncEnumerable<T> WithCancellation(CancellationToken cancellationToken)
	{
		return new ConfiguredCancelableAsyncEnumerable<T>(_enumerable, _continueOnCapturedContext, cancellationToken);
	}

	public Enumerator GetAsyncEnumerator()
	{
		return new Enumerator(_enumerable.GetAsyncEnumerator(_cancellationToken), _continueOnCapturedContext);
	}
}
