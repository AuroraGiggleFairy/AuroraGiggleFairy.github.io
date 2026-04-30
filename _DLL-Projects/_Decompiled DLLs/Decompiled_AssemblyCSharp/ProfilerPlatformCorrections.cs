using System.Text;
using Unity.Profiling;

public static class ProfilerPlatformCorrections
{
	public class NativeDefault : IMetric
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public ProfilerRecorder graphics;

		[PublicizedFrom(EAccessModifier.Private)]
		public ProfilerRecorder total;

		[PublicizedFrom(EAccessModifier.Private)]
		public ProfilerRecorder managed;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public string Header { get; set; }

		public NativeDefault(string header, string usedOrReserved)
		{
			Header = header;
			graphics = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx " + usedOrReserved + " Memory");
			total = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total " + usedOrReserved + " Memory");
			managed = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC " + usedOrReserved + " Memory");
		}

		public void AppendLastValue(StringBuilder builder)
		{
			double num = total.LastValueAsDouble - graphics.LastValueAsDouble - managed.LastValueAsDouble;
			builder.AppendFormat("{0:F2}", num * 9.5367431640625E-07);
		}

		public void Cleanup()
		{
			graphics.Dispose();
			total.Dispose();
			managed.Dispose();
		}
	}

	public class TotalTrackedPS5 : IMetric
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public ProfilerRecorder graphics;

		[PublicizedFrom(EAccessModifier.Private)]
		public ProfilerRecorder total;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public string Header { get; set; }

		public TotalTrackedPS5(string header, string usedOrReserved)
		{
			Header = header;
			graphics = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx " + usedOrReserved + " Memory");
			total = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total " + usedOrReserved + " Memory");
		}

		public void AppendLastValue(StringBuilder builder)
		{
			double num = total.LastValueAsDouble - graphics.LastValueAsDouble;
			builder.AppendFormat("{0:F2}", num * 9.5367431640625E-07);
		}

		public void Cleanup()
		{
			graphics.Dispose();
			total.Dispose();
		}
	}

	public static IMetric Graphics(string header, string usedOrReserved)
	{
		return new ProfilerRecorderMetric
		{
			Header = header,
			recorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx " + usedOrReserved + " Memory")
		};
	}

	public static IMetric Native(string header, string usedOrReserved)
	{
		return new NativeDefault(header, usedOrReserved);
	}

	public static IMetric TotalTracked(string header, string usedOrReserved)
	{
		return new ProfilerRecorderMetric
		{
			Header = header,
			recorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total " + usedOrReserved + " Memory")
		};
	}
}
