using System.Diagnostics;

public static class ProfilerExt
{
	[Conditional("ENABLE_PROFILER")]
	public static void BeginSampleThreadSafe(string _caption)
	{
		ThreadManager.IsMainThread();
	}

	[Conditional("ENABLE_PROFILER")]
	public static void EndSampleThreadSafe()
	{
		ThreadManager.IsMainThread();
	}
}
