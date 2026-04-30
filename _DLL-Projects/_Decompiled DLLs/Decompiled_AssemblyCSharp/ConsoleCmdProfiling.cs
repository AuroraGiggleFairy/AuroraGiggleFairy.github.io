using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdProfiling : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ConsoleCmdProfileNetwork cmdNetwork;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool profileNetwork;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int FramesDefault = 300;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int FramesMin = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int FramesMax = 3000;

	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "profiling" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (cmdNetwork == null)
		{
			cmdNetwork = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommand("profilenetwork", _alreadyTokenized: true) as ConsoleCmdProfileNetwork;
		}
		if (_params.Count == 1 && _params[0].EqualsCaseInsensitive("stop"))
		{
			if (!Profiler.enabled)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Profiling not running.");
				return;
			}
			stopProfiling();
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Profiling stopped.");
		}
		else
		{
			if (Profiler.enabled)
			{
				return;
			}
			profileNetwork = true;
			int result = 300;
			if (_params.Count > 0 && !int.TryParse(_params[0], out result))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Not a valid integer for number of frames (\"{result}\")");
				return;
			}
			if (result < 10 || result > 3000)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Number of frames needs to be within {10} and {3000}");
				return;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Enabled profiling for {result} frames (typically 5 - 10 seconds)");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Profiler mem: {Profiler.maxUsedMemory}");
			Profiler.logFile = $"{GameIO.GetApplicationPath()}/profiling_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_unity.log";
			Profiler.enableBinaryLog = true;
			Profiler.enabled = true;
			ThreadManager.StartCoroutine(stopProfilingLater(result));
			if (profileNetwork)
			{
				cmdNetwork?.resetData();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator stopProfilingLater(int _frames)
	{
		for (int i = 0; i < _frames; i++)
		{
			if (!Profiler.enabled)
			{
				break;
			}
			yield return null;
		}
		if (Profiler.enabled)
		{
			stopProfiling();
			Log.Out("Profiling done");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stopProfiling()
	{
		Profiler.enabled = false;
		Profiler.logFile = null;
		if (profileNetwork)
		{
			cmdNetwork?.doProfileNetwork();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Enable Unity profiling for 300 frames";
	}
}
