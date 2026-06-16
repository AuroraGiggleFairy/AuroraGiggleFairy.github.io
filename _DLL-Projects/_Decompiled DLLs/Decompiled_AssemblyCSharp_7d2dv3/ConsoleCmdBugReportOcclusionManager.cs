using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdBugReportOcclusionManager : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => false;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "testoccreport", "toccr" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Test the occlusion manager self reporting to backtrace, requires Backtrace to be enabled at build creation";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) && OcclusionManager.Instance.WriteListToDisk(out var fileList))
		{
			BacktraceUtils.SendErrorReport("OcclusionManagerUsedUpAllEntries", "Occlusion Manager used all entries", fileList);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("testoccreport: Wrote Files to disk: " + string.Join(",", fileList));
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("testoccreport: Posted report to backtrace");
		}
	}
}
