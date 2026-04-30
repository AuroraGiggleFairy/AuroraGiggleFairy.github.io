namespace System.Threading.Tasks.Sources;

internal static class ManualResetValueTaskSourceCoreShared
{
	internal static readonly Action<object> s_sentinel = CompletionSentinel;

	private static void CompletionSentinel(object _)
	{
		throw new InvalidOperationException();
	}
}
