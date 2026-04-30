using System.Collections.Generic;
using System.Text;
using Unity.Profiling;

public class ProfilingMetricCapture
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<IMetric> metrics = new List<IMetric>();

	[PublicizedFrom(EAccessModifier.Private)]
	public StringBuilder outputBuilder = new StringBuilder();

	public void AddDummy(string header)
	{
		metrics.Add(new ConstantValueMetric
		{
			Header = header,
			value = 0
		});
	}

	public void Add(string header, ProfilerRecorder recorder)
	{
		if (!recorder.IsRunning)
		{
			recorder.Start();
		}
		metrics.Add(new ProfilerRecorderMetric
		{
			recorder = recorder,
			Header = header
		});
	}

	public void Add(string header, CallbackMetric.GetLastValue callback)
	{
		metrics.Add(new CallbackMetric
		{
			callback = callback,
			Header = header
		});
	}

	public void Add(IMetric metric)
	{
		metrics.Add(metric);
	}

	public void Cleanup()
	{
		foreach (IMetric metric in metrics)
		{
			metric.Cleanup();
		}
		metrics.Clear();
	}

	public string GetCsvHeader()
	{
		for (int i = 0; i < metrics.Count; i++)
		{
			outputBuilder.Append(metrics[i].Header);
			if (i < metrics.Count - 1)
			{
				outputBuilder.Append(",");
			}
		}
		string result = outputBuilder.ToString();
		outputBuilder.Clear();
		return result;
	}

	public string GetLastValueCsv()
	{
		for (int i = 0; i < metrics.Count; i++)
		{
			metrics[i].AppendLastValue(outputBuilder);
			if (i < metrics.Count - 1)
			{
				outputBuilder.Append(",");
			}
		}
		string result = outputBuilder.ToString();
		outputBuilder.Clear();
		return result;
	}

	public string PrettyPrint()
	{
		for (int i = 0; i < metrics.Count; i++)
		{
			outputBuilder.AppendFormat("{0}: ", metrics[i].Header);
			metrics[i].AppendLastValue(outputBuilder);
			outputBuilder.AppendLine();
		}
		string result = outputBuilder.ToString();
		outputBuilder.Clear();
		return result;
	}
}
