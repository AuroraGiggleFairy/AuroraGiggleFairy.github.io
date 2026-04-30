using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdListThreads : ConsoleCmdAbstract
{
	public override int DefaultPermissionLevel => 1000;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "listthreads", "lt" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Threads:");
		int num = 0;
		foreach (KeyValuePair<string, ThreadManager.ThreadInfo> activeThread in ThreadManager.ActiveThreads)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(++num + ". " + activeThread.Key);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "lists all threads";
	}
}
