using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices;

[StructLayout(LayoutKind.Auto)]
internal readonly struct ConfiguredAsyncDisposable(IAsyncDisposable source, bool continueOnCapturedContext)
{
	private readonly IAsyncDisposable _source = source;

	private readonly bool _continueOnCapturedContext = continueOnCapturedContext;

	public ConfiguredValueTaskAwaitable DisposeAsync()
	{
		return _source.DisposeAsync().ConfigureAwait(_continueOnCapturedContext);
	}
}
