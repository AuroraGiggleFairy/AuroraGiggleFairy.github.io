using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdProfiler : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilingMetricCapture memMetrics;

	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "profiler" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			return;
		}
		string text = _params[0];
		if (!(text == "listrawmetrics"))
		{
			if (text == "mem")
			{
				if (memMetrics == null)
				{
					InitMemoryMetrics();
				}
				if (_params.Count == 1)
				{
					LogPretty(memMetrics);
				}
				else
				{
					if (_params.Count != 2)
					{
						return;
					}
					string text2 = _params[1];
					if (!(text2 == "csv"))
					{
						if (text2 == "pretty")
						{
							LogPretty(memMetrics);
						}
					}
					else
					{
						LogCsv(memMetrics);
					}
				}
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			}
		}
		else
		{
			Log.Out(ProfilerUtils.GetAvailableMetricsCsv());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitMemoryMetrics()
	{
		if (memMetrics != null)
		{
			memMetrics.Cleanup();
		}
		memMetrics = ProfilerCaptureUtils.CreateMemoryProfiler();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogCsv(ProfilingMetricCapture _metrics)
	{
		ThreadManager.StartCoroutine(LogCsvNextFrame(_metrics));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator LogCsvNextFrame(ProfilingMetricCapture _metrics)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		Log.Out(_metrics.GetCsvHeader());
		Log.Out(_metrics.GetLastValueCsv());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogPretty(ProfilingMetricCapture _metrics)
	{
		ThreadManager.StartCoroutine(LogPrettyNextFrame(_metrics));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator LogPrettyNextFrame(ProfilingMetricCapture _metrics)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		Log.Out(_metrics.PrettyPrint());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Utilities for collection profiling data from a variety of sources";
	}
}
