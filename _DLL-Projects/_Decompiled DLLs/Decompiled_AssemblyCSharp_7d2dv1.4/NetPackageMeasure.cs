using System;
using System.Collections.Generic;
using System.Diagnostics;

public class NetPackageMeasure
{
	public struct Sample(long _timestamp, long _totalBytesSent)
	{
		public long totalBytesSent = _totalBytesSent;

		public long timestamp = _timestamp;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public long timeWindowTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public LinkedList<Sample> samples = new LinkedList<Sample>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Stopwatch timer = new Stopwatch();

	public long totalSent;

	public NetPackageMeasure(double _windowSizeSeconds)
	{
		timeWindowTicks = (long)(_windowSizeSeconds * (double)Stopwatch.Frequency);
		timer.Start();
	}

	public void SamplePackages(List<NetPackage> _packages)
	{
		long num = 0L;
		foreach (NetPackage _package in _packages)
		{
			num += _package.GetLength();
		}
		AddSample(num);
	}

	public void AddSample(long _totalBytes)
	{
		samples.AddLast(new Sample(timer.ElapsedTicks, _totalBytes));
		totalSent += _totalBytes;
	}

	public void RecalculateTotals()
	{
		timer.Stop();
		while (samples.First != null && Math.Abs(timer.ElapsedTicks - samples.First.Value.timestamp) > timeWindowTicks)
		{
			totalSent -= samples.First.Value.totalBytesSent;
			samples.RemoveFirst();
		}
		timer.Start();
	}
}
