using System.Diagnostics;
using Unity.Profiling;

public static class WaterStatsProfiler
{
	public struct Timer(string name)
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly double tickToNanos = 1000000000.0 / (double)Stopwatch.Frequency;

		[PublicizedFrom(EAccessModifier.Private)]
		public Stopwatch stopwatch = new Stopwatch();

		public ProfilerCounter<double> counterValue = new ProfilerCounter<double>(ProfilerCategory.Scripts, name, ProfilerMarkerDataUnit.TimeNanoseconds);

		public void Start()
		{
			stopwatch.Start();
		}

		public void Stop()
		{
			stopwatch.Stop();
		}

		public void Sample()
		{
			stopwatch.Reset();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounter<int> NumChunksProcessed = new ProfilerCounter<int>(ProfilerCategory.Scripts, "Num Chunks Processed", ProfilerMarkerDataUnit.Count);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounter<int> NumChunksActive = new ProfilerCounter<int>(ProfilerCategory.Scripts, "Num Chunks Active", ProfilerMarkerDataUnit.Count);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounter<int> NumFlowEvents = new ProfilerCounter<int>(ProfilerCategory.Scripts, "Num Flow Events", ProfilerMarkerDataUnit.Count);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounter<int> NumVoxelProcessed = new ProfilerCounter<int>(ProfilerCategory.Scripts, "Num Voxels Processed", ProfilerMarkerDataUnit.Count);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounter<int> NumVoxelsPutToSleep = new ProfilerCounter<int>(ProfilerCategory.Scripts, "Num Voxels Put To Sleep", ProfilerMarkerDataUnit.Count);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounter<int> NumVoxelsWokeUp = new ProfilerCounter<int>(ProfilerCategory.Scripts, "Num Voxels Woke Up", ProfilerMarkerDataUnit.Count);

	public static void SampleTick(WaterStats stats)
	{
	}
}
