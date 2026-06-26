using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

public class BenchmarkObject
{
	public class BenchmarkContainer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public string pName;

		public long startTick;

		public long endTick;

		public string name
		{
			get
			{
				return pName;
			}
			set
			{
				pName = value;
			}
		}

		public long ticks => endTick - startTick;

		public BenchmarkContainer(string _name)
		{
			pName = _name;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<object, BenchmarkContainer> benchmarks = new Dictionary<object, BenchmarkContainer>();

	[Conditional("PROFILEx")]
	public static void StartTimer(string _benchmarkName, object _watchObject)
	{
		BenchmarkContainer benchmarkContainer = new BenchmarkContainer(_benchmarkName);
		lock (benchmarks)
		{
			benchmarks[_watchObject] = benchmarkContainer;
		}
		benchmarkContainer.startTick = DateTime.Now.Ticks;
	}

	[Conditional("PROFILEx")]
	public static void SwitchObject(object _old, object _new)
	{
		lock (benchmarks)
		{
			if (benchmarks.ContainsKey(_old))
			{
				BenchmarkContainer value = benchmarks[_old];
				benchmarks.Remove(_old);
				benchmarks[_new] = value;
			}
			else
			{
				Log.Out("SWITCHOBJECT: Object not found");
			}
		}
	}

	[Conditional("PROFILEx")]
	public static void UpdateName(object _watchObject, string _nameAppend)
	{
		BenchmarkContainer benchmarkContainer = null;
		lock (benchmarks)
		{
			if (benchmarks.ContainsKey(_watchObject))
			{
				benchmarkContainer = benchmarks[_watchObject];
			}
		}
		if (benchmarkContainer != null)
		{
			benchmarkContainer.name += _nameAppend;
		}
		else
		{
			Log.Out("UPDATENAME: Object not found: " + _nameAppend);
		}
	}

	[Conditional("PROFILEx")]
	public static void StopTimer(object _watchObject)
	{
		long ticks = DateTime.Now.Ticks;
		lock (benchmarks)
		{
			if (benchmarks.ContainsKey(_watchObject))
			{
				benchmarks[_watchObject].endTick = ticks;
			}
			else
			{
				Log.Out("STOPTIMER: Object not found");
			}
		}
	}

	[Conditional("PROFILEx")]
	public static void PrintAll()
	{
		if (benchmarks.Count <= 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		foreach (KeyValuePair<object, BenchmarkContainer> benchmark in benchmarks)
		{
			if (benchmark.Value.name.Length > num)
			{
				num = benchmark.Value.name.Length;
			}
		}
		string format = "{0} {1} {2} {3}" + Environment.NewLine;
		stringBuilder.Append(string.Format(format, "Name", "Start", "End", "Duration"));
		foreach (BenchmarkContainer item in from b in benchmarks.Values.ToList()
			orderby b.startTick
			select b)
		{
			stringBuilder.Append(string.Format(format, item.name, item.startTick / 10, item.endTick / 10, item.ticks / 10));
		}
		SdFile.WriteAllText(GameIO.GetGameDir("") + "durations.txt", stringBuilder.ToString());
	}
}
