using System.Text;
using Unity.Profiling;

public class ProfilerRecorderMetric : IMetric
{
	public ProfilerRecorder recorder;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Header { get; set; }

	public void AppendLastValue(StringBuilder builder)
	{
		ProfilerUtils.AppendLastValue(recorder, builder);
	}

	public void Cleanup()
	{
		recorder.Dispose();
	}
}
