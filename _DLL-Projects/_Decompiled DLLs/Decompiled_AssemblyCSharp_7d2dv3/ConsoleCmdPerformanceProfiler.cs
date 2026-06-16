using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPerformanceProfiler : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override int DefaultPermissionLevel => 1000;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "performanceprofiler", "pp" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Performance Profiling Utility";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Type 'pp' to toggle capture, or 'pp start [fps]' / 'pp stop'.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			Toggle();
			return;
		}
		string text = _params[0].ToLower();
		if (!(text == "start"))
		{
			if (text == "stop")
			{
				CmdStop();
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown sub-command '" + _params[0] + "'. See 'help pp'.");
			}
		}
		else
		{
			CmdStart(_params);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Toggle()
	{
		if (PerformanceProfiler.IsCapturing())
		{
			Log.Out($"[PerformanceProfiler] Stopping after {PerformanceProfiler.GetCurrentFrameCount()} frames.");
			PerformanceProfiler.StopCapture();
		}
		else
		{
			Log.Out("[PerformanceProfiler] Starting capture (30 FPS target).");
			PerformanceProfiler.StartCapture();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdStart(List<string> p)
	{
		if (PerformanceProfiler.IsCapturing())
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Capture already running.");
			return;
		}
		int result;
		int num = ((p.Count > 1 && int.TryParse(p[1], out result)) ? result : 30);
		PerformanceProfiler.StartCapture(null, null, num);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Capture started at {num} FPS target.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdStop()
	{
		if (!PerformanceProfiler.IsCapturing())
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No capture in progress.");
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Stopping capture after {PerformanceProfiler.GetCurrentFrameCount()} frames.");
		PerformanceProfiler.StopCapture();
	}
}
