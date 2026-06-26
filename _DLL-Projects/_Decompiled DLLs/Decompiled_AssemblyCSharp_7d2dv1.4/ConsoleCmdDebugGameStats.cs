using System.Collections.Generic;
using System.IO;
using System.Text;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdDebugGameStats : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string logFile;

	[PublicizedFrom(EAccessModifier.Private)]
	public StringBuilder stringBuilder;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "debuggamestats" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "GameStats commands";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count >= 1)
		{
			if (_params[0].ToUpper() == "LOG")
			{
				if (_params.Count >= 2 && bool.TryParse(_params[1], out var result))
				{
					if (result)
					{
						logFile = Path.Join(PlatformApplicationManager.Application.temporaryCachePath, "GameStats.tsv");
						this.stringBuilder = new StringBuilder();
						this.stringBuilder.AppendLine(DebugGameStats.GetHeader('\t'));
						File.AppendAllText(logFile, this.stringBuilder.ToString());
						DebugGameStats.StartStatisticsUpdate(logDebugGameStats);
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Started logging debug stats to " + logFile + ".");
					}
					else
					{
						DebugGameStats.StopStatisticsUpdate();
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Stopped logging debug stats to " + logFile + ".");
					}
					return;
				}
			}
			else if (_params[0].ToUpper() == "PRINT")
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(DebugGameStats.GetHeader(','));
				stringBuilder.AppendLine(DebugGameStats.GetCurrentStatsString());
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(stringBuilder.ToString());
				return;
			}
		}
		Log.Out("Incorrect params, expected 'log [true|false]' or 'print'");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void logDebugGameStats(Dictionary<string, string> statisticsDictionary)
	{
		stringBuilder.Clear();
		foreach (KeyValuePair<string, string> item in statisticsDictionary)
		{
			stringBuilder.Append(item.Value);
			stringBuilder.Append('\t');
		}
		stringBuilder.AppendLine();
		File.AppendAllText(logFile, stringBuilder.ToString());
	}
}
